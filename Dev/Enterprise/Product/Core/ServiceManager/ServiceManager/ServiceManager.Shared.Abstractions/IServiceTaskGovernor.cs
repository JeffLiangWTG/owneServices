using System;
using Microsoft.Extensions.Logging;

namespace ServiceManager.Shared.Abstractions
{
	public interface IServiceTaskGovernor
	{
		void SetActive(bool value);
		void SetBranchPk(Guid pk);
		void SetStartDate(DateTimeOffset startDate);
		void SetNextRunTime(DateTimeOffset runTime);
		void SetLastRunTime(DateTimeOffset lastRunTime);
		bool ResetScheduleToDefault(bool reEnableMandatory, ILogger logger);
		void SetBranchFromCode(string branchCode);
		void SetSchedule(int frequency, string recurrence);
		void SetWeeklySchedule(int frequency, DayOfWeek[] daysOfWeek);
		void SetMonthlySchedule(int frequency, int dayOfMonth);
		void SetDailyStartTimeLocal(TimeSpan startTime, TimeSpan offset);
		void SetDailyStartTimeUtc(TimeSpan startTime, TimeSpan offset);
		void SetDailyEndTimeLocal(TimeSpan endTime);
		void SetDailyEndTimeUtc(TimeSpan endTime);
		void SetOverdueDuration(TimeSpan secondsOverdue);
		TaskInstanceStatus UpdateStatus();
		void Reload();
		void OnErrorReported();

		IServiceTask GovernedTask { get; }
		INextRunTimeCalculator Calculator { get; }
	}
}
