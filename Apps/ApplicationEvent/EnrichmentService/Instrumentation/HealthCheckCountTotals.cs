using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace eServices.ApplicationEvent.EnrichmentService.Instrumentation;

public class HealthCheckCountTotals(HealthCheckMetrics healthCheckMetrics) : IHealthCheck
{
	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy,
			$"Received={healthCheckMetrics.GetCounterMessagesReceived():N0} " +
			$"Sent={healthCheckMetrics.GetCounterMessagesSent():N0} " +
			$"Skipped={healthCheckMetrics.GetCounterMessagesSkipped():N0} " +
			$"Failed={healthCheckMetrics.GetCounterMessagesFailed():N0}"));
	}
}
