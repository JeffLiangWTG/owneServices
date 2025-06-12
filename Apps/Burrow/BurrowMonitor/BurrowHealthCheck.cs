using System.Text.Json.Nodes;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace eServices.BurrowMonitor;

public class BurrowHealthCheck : IHealthCheck
{
	private readonly string query;
	private readonly IHttpClientFactory httpClientFactory;
	private readonly int alertThresholdMinutes;

	public BurrowHealthCheck(string query, IHttpClientFactory httpClientFactory, IConfiguration configuration)
	{
		this.query = query;
		this.httpClientFactory = httpClientFactory;
		alertThresholdMinutes = configuration.GetValue<int>("Burrow:AlertThresholdMinutes");
	}

	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		using var httpClient = httpClientFactory.CreateClient("Burrow");
		var response = await httpClient.GetStringAsync(query, cancellationToken);
		var report = JsonNode.Parse(response)!["status"]!;
		var lag = (int)report["totallag"]!;
		var timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)report["maxlag"]!["end"]!["timestamp"]!);
		var data = new Dictionary<string, object>
		{
			["lag"] = lag,
			["timestamp"] = timestamp,
			["url"] = string.Concat(httpClient.BaseAddress, query)
		};

		var status = (lag > 0 && timestamp < DateTimeOffset.UtcNow.AddMinutes(-alertThresholdMinutes)) ? HealthStatus.Unhealthy : HealthStatus.Healthy;

		return new HealthCheckResult(status, data: data);
	}
}
