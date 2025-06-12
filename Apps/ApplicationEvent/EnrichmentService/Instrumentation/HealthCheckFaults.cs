using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace eServices.ApplicationEvent.EnrichmentService.Instrumentation;

public class HealthCheckFaults(HealthCheckMetrics healthCheckMetrics) : IHealthCheck
{
	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		var lastProcessed = new[]
		{
			healthCheckMetrics.LastSent,
			healthCheckMetrics.LastSkipped,
			healthCheckMetrics.LastFailed
		}.Max();
		var start = lastProcessed ?? healthCheckMetrics.LastSent;
		var diff = healthCheckMetrics.LastFaulted - start;
		var details = $"LastReceived={healthCheckMetrics.LastReceived:s} " +
			$"LastSent={healthCheckMetrics.LastSent:s} " +
			$"LastSkipped={healthCheckMetrics.LastSkipped:s} " +
			$"LastFailed={healthCheckMetrics.LastFailed:s} " +
			$"LastFaulted={healthCheckMetrics.LastFaulted:s}";
		return Task.FromResult(
			diff switch
			{
				{ TotalMinutes: >= 15 } => HealthCheckResult.Unhealthy($"Faulting. OutageDuration={diff:c} {details}"),
				{ TotalMinutes: >= 1 } => HealthCheckResult.Degraded($"Faulting. OutageDuration={diff:c} {details}"),
				_ => HealthCheckResult.Healthy($"Healthy. {details}")
			});
	}
}
