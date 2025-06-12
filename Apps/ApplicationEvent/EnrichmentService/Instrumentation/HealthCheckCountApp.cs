using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace eServices.ApplicationEvent.EnrichmentService.Instrumentation;

public class HealthCheckCountApp(string name, HealthCheckMetrics healthCheckMetrics) : IHealthCheck
{
	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		var count = healthCheckMetrics.CounterPerApplication.TryGetValue(name, out var c) ? c : 0;
		return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy, $"Count={count:N0}"));
	}
}