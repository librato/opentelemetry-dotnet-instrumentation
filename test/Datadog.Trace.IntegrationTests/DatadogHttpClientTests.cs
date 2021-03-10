﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Datadog.Core.Tools;
using Datadog.Trace.Agent;
using Datadog.Trace.Configuration;
using Datadog.Trace.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace Datadog.Trace.IntegrationTests
{
    public class DatadogHttpClientTests
    {
        private readonly ITestOutputHelper _output;

        public DatadogHttpClientTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task DatadogHttpClient_CanSendTracesToAgent()
        {
            var agentPort = TcpPortProvider.GetOpenPort();

            using (var agent = new MockTracerAgent(agentPort))
            {
                var settings = new TracerSettings
                {
                    AgentUri = new Uri($"http://localhost:{agent.Port}"),
                    TracesTransport = TransportStrategy.DatadogTcp,
                };
                var tracer = new Tracer(settings);

                using (var scope = tracer.StartActive("operationName"))
                {
                    scope.Span.ResourceName = "resourceName";
                }

                await tracer.FlushAsync();

                var spans = agent.WaitForSpans(1);
                Assert.Equal(1, spans.Count);
            }
        }
    }
}