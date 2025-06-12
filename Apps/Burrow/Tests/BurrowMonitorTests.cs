using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProcRoll;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace eServices.BurrowMonitor.Tests;

public class BurrowMonitorTests
{
	readonly ProcessActions writeToStdOut = new() { StdOut = TestContext.WriteLine };

	[Test]
	public void BurrowHealthCheck_Test_WtgStatus()
	{
		using var server = WireMockServer.Start();
		server.Given(Request.Create().WithPath("/v3/kafka/eservices-a/consumer/eServices-billing-transactions-consumer/status").UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody("{\"status\":{\"maxlag\":{\"end\":{\"timestamp\":" + DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() + "}},\"totallag\":0}}")
			);
		server.Given(Request.Create().WithPath("/v3/kafka/eservices-a/consumer/message-events/status").UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody("{\"status\":{\"maxlag\":{\"end\":{\"timestamp\":" + DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(20)).ToUnixTimeMilliseconds().ToString() + "}},\"totallag\":50}}")
			);

		const string monitorUri = "http://localhost:4000";
		ProcessStartInfo monitorStartInfo = new()
		{
			FileName = "eServices.BurrowMonitor.exe",
			Arguments = $"Urls={monitorUri} Burrow:Address={server.Url}/v3/kafka/ Burrow:AlertThresholdMinutes=10 " +
			"Burrow:Monitors:eServices-Gateway-To-Billing-Web-Service=eservices-a/consumer/eServices-billing-transactions-consumer/status " +
			"Burrow:Monitors:eServices-Message-Events=eservices-a/consumer/message-events/status"
		};
		using var monitorProcess = Process.Run(monitorStartInfo, writeToStdOut);
		using HttpClient httpClient = new();
		var result = httpClient.GetStringAsync($"{monitorUri}/wtg/status").Result;

		Assert.That(result, Is.EqualTo(
			$"INFO(eServices-Gateway-To-Billing-Web-Service): Count=0, Age=0, Report={server.Url}/v3/kafka/eservices-a/consumer/eServices-billing-transactions-consumer/status\r\n" +
			$"ERROR(eServices-Message-Events): Count=50, Age=20, Report={server.Url}/v3/kafka/eservices-a/consumer/message-events/status\r\n"));
	}

	[Test]
	public void BurrowHealthCheck_Test_Healthy()
	{
		using var server = WireMockServer.Start();
		server.Given(Request.Create().WithPath("/v3/kafka/eservices-a/consumer/eServices-billing-transactions-consumer/status").UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody("{\"status\":{\"maxlag\":{\"end\":{\"timestamp\":" + DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() + "}},\"totallag\":0}}")
			);

		using HttpClient httpClient = new() { BaseAddress = new Uri($"{server.Url}/v3/kafka/") };
		var httpClientFactory = new Mock<IHttpClientFactory>();
		httpClientFactory.Setup(x => x.CreateClient("Burrow")).Returns(httpClient);
		IConfiguration config = new ConfigurationBuilder()
			.AddCommandLine(["Burrow:AlertThresholdMinutes=10"])
			.Build();
		var healthCheckContext = new HealthCheckContext();
		var burrowHealthCheck = new BurrowHealthCheck("eservices-a/consumer/eServices-billing-transactions-consumer/status", httpClientFactory.Object, config);

		var result = burrowHealthCheck.CheckHealthAsync(healthCheckContext).Result;

		Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
	}

	[Test]
	public void BurrowHealthCheck_Test_Unhealthy()
	{
		using var server = WireMockServer.Start();
		server.Given(Request.Create().WithPath("/v3/kafka/eservices-a/consumer/eServices-billing-transactions-consumer/status").UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody("{\"status\":{\"maxlag\":{\"end\":{\"timestamp\":" + DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(20)).ToUnixTimeMilliseconds().ToString() + "}},\"totallag\":20}}")
			);

		using HttpClient httpClient = new() { BaseAddress = new Uri($"{server.Url}/v3/kafka/") };
		var httpClientFactory = new Mock<IHttpClientFactory>();
		httpClientFactory.Setup(x => x.CreateClient("Burrow")).Returns(httpClient);
		IConfiguration config = new ConfigurationBuilder()
			.AddCommandLine(["Burrow:AlertThresholdMinutes=10"])
			.Build();
		var healthCheckContext = new HealthCheckContext();
		var burrowHealthCheck = new BurrowHealthCheck("eservices-a/consumer/eServices-billing-transactions-consumer/status", httpClientFactory.Object, config);

		var result = burrowHealthCheck.CheckHealthAsync(healthCheckContext).Result;

		Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
	}
}