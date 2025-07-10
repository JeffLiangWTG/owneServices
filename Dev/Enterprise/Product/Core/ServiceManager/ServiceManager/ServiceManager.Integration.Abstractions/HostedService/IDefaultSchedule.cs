using System;

namespace ServiceManager.Integration.Abstractions
{
	public interface IDefaultSchedule
	{
		string RunEvery { get; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Connected to the NextRunTimeCalculatorWeeks class which is serialized and requires a simple type for the XML Array.")]
		DayOfWeek[] DaysOfWeek { get; }
		int DayOfMonth { get; }
		string StartAtLocal { get; }
		string StartAtUtc { get; }
		string RandomStartOffset { get; }
		string EndAtLocal { get; }
		string EndAtUtc { get; }
		string DoNotRunTillNextDueTimeIfOverdue { get; }
	}
}
