using KafkaFlow.Consumers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace eServices.ApplicationEvent.EnrichmentService.Instrumentation;

public class HealthCheckLag(IConsumerAccessor consumerAccessor, IOptionsSnapshot<MetricsOptions> metricsOptions) : IHealthCheck
{
	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		var consumer = consumerAccessor.GetConsumer(nameof(KafkaOptions.MessageEvents));
		var lags = consumer.GetTopicPartitionsLag().Select(l => l.Lag).ToList();
		var maxLag = lags.DefaultIfEmpty().Max();
		var message = $"Lags=[{string.Join(", ", lags)}] MaxLag={maxLag:N0} ErrorThreshold={metricsOptions.Value.LagErrorThreshold:N0}";
		return Task.FromResult(
			metricsOptions.Value.LagErrorThreshold is long threshold && maxLag > threshold
			? HealthCheckResult.Unhealthy(message)
			: HealthCheckResult.Healthy(message));
	}
}
