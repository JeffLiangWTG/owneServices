namespace eServices.ApplicationEvent.EnrichmentService.Instrumentation;

public class MetricsOptions
{
	public bool ExportToConsole { get; set; }
	public bool HealthCheckCountersEnabled { get; set; }
	public long? LagErrorThreshold { get; set; }
}
