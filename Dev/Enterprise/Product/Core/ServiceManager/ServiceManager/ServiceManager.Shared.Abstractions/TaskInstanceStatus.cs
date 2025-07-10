using System;

namespace ServiceManager.Shared.Abstractions
{
	public class TaskInstanceStatus
	{
		public string StatusString { get; set; } = string.Empty;
		public string PlaceInQueueString { get; set; } = string.Empty;
		public string SecondsInQueueString { get; set; } = string.Empty;
		public int RunningCount { get; set; }
		public string ProcessIDsString { get; set; } = string.Empty;
		public string SecondsRunningString { get; set; } = string.Empty;
		public string RegisteredOnHosts { get; set; } = string.Empty;
		public string BindingTypes { get; set; } = string.Empty;
		public int BindingCount { get; set; }
		public DateTime? NextRunTime { get; set; }
		public DateTime? LastRunTime { get; set; }
		public DateTime? LastErrorTime { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Not a timespan")]
		public int ErrorCountLast24Hours { get; set; }
	}
}
