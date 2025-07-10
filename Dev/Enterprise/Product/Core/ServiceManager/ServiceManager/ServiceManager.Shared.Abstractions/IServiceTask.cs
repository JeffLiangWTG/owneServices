using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ServiceManager.Shared.Abstractions
{
	public interface IServiceTask
	{
		Guid Pk { get; }
		bool IsActive { get; }
		DateTimeOffset NextRunTime { get; }
		TimeSpan DailyStartTime { get; }
		TimeSpan DailyEndTime { get; }
		string Code { get; }
		string Description { get; }
		string MultilingualDescription { get; }
		Guid BranchPk { get; }
		string BranchName { get; }
		string BranchErrorMessage { get; }
		string ConfigString { get; }
		string SettingsXml { get; }
		int SecondaryProcessesMaxCount { get; }
		TimeSpan SchedulePeriodDuration { get; }
		TimeSpan OverdueDuration { get; }
		void PreRunValidation();
		void EnsureThreadSafety();
		DateTimeOffset CalculateNextRunTime(ILogger logger);
		IReadOnlyList<DayOfWeek> ScheduleDaysOfWeek { get; }
		int ScheduleDayOfMonth { get; }
		int ScheduleDayNumber { get; }
		int ScheduleMonth { get; }
		DateTimeOffset ScheduleStartDate { get; }
		string ScheduleOccurrence { get; }
		int ScheduleFrequency { get; }
		bool WeekDaysOnly { get; }
		string Category {  get; }
		IServiceTaskGovernor AssignedGovernor { get; }
	}
}
