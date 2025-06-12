using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;

namespace eServices.ApplicationEvent.EnrichmentService.Instrumentation;

public class HealthCheckMetricsListener(IOptions<MetricsOptions> metricsOptions, HealthCheckMetrics healthCheckMetrics) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (metricsOptions.Value.HealthCheckCountersEnabled)
		{
			var meterListener = new MeterListener();
			meterListener.InstrumentPublished = (instrument, meterListener) =>
			{
				if (instrument.Name
					is Metrics.Names.Counter_MsgsRcvd
					or Metrics.Names.Counter_MsgsSent
					or Metrics.Names.Counter_MsgsSkip
					or Metrics.Names.Counter_MsgsFail
					or Metrics.Names.Counter_Faults)
					meterListener.EnableMeasurementEvents(instrument);
			};
			meterListener.SetMeasurementEventCallback<long>((Instrument instrument, long measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state) =>
			{
				switch (instrument.Name)
				{
					case Metrics.Names.Counter_MsgsRcvd:
						healthCheckMetrics.AddCounterMessagesReceived(measurement);
						break;
					case Metrics.Names.Counter_MsgsSent:
						healthCheckMetrics.AddCounterMessagesSent(measurement, tags);
						break;
					case Metrics.Names.Counter_MsgsSkip:
						healthCheckMetrics.AddCounterMessagesSkipped(measurement);
						break;
					case Metrics.Names.Counter_MsgsFail:
						healthCheckMetrics.AddCounterMessagesFailed(measurement);
						break;
					case Metrics.Names.Counter_Faults:
						healthCheckMetrics.AddCounterFaults(measurement);
						break;
					default:
						break;
				}
			});
			meterListener.Start();
		}

		await Task.Delay(Timeout.Infinite, stoppingToken);
	}
}
