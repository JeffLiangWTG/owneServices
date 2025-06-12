namespace eServices.ApplicationEvent.EnrichmentService.Instrumentation;

public sealed class RollingFileLoggerConfiguration
{
	public string File { get; set; } = null!;
	public string MaxFileSize { get; set; } = null!;
	public int MaxFileCount { get; set; } = 0;
}
