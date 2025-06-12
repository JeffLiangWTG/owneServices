namespace eServices.eHubPortal.Services;

public sealed class GlobalSearchFileLoggerConfiguration
{
	public string File { get; set; } = null!;
	public int MaxDays { get; set; } = 30;
}
