using System;

namespace ServiceManager.Shared.Abstractions
{
	public interface IServiceTaskScheduleThreadSafeReader
	{
		bool IsActive { get; }
		string ScheduleDescription { get; }
		string ScheduleCategory { get; }
		TimeSpan SchedulePeriodDuration { get; }
		string ConfigString { get; }
		TimeSpan OverdueDuration { get; }
		int SecondaryProcessesMaxCount { get; }
		DateTime LastRunTime { get; }
		DateTime LastErrorTime { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Not a duration")]
		int ErrorCountLast24Hours { get; }
	}
}
