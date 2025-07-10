using System;
using Enterprise.Integration;

namespace Enterprise.ServiceManager.Shared;

public record ServiceTaskLogFilters
{
	public string? ServiceTaskCode
	{
		get => serviceTaskCode;
		set => serviceTaskCode = value?.Trim().ToUpperInvariant();
	}

	public string? HostName
	{
		get => hostname;
		set => hostname = value?.Trim().ToLowerInvariant();
	}

	string? hostname;
	string? serviceTaskCode;
	public LogType? Severity { get; set; }
	public string? ProcessId { get; set; }
	public DateTime? FromDateTimeUtc { get; set; }
	public DateTime? ToDateTimeUtc { get; set; }
}
