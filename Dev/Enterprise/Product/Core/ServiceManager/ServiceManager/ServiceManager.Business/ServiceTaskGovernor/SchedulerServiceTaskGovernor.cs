using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using ServiceManager.Shared.Abstractions;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Business
{
	public class SchedulerServiceTaskGovernor : IServiceTaskGovernor
	{
		public SchedulerServiceTaskGovernor(BusinessObjectFactory factory, ServiceTaskSchedule serviceTask)
		{
			this.factory = factory;
			cachedTask = serviceTask ?? throw new ArgumentNullException(nameof(serviceTask));
		}

		public void SetActive(bool active)
		{
			cachedTask.S5_IsActive = active;
		}

		public void SetBranchPk(Guid pk)
		{
			cachedTask.S5_GB = pk;
		}

		public void SetBranchFromCode(string branchCode)
		{
			var branch = factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
			if (branch != null)
			{
				cachedTask.S5_GB = branch.PK;
			}
		}

		public void SetNextRunTime(DateTimeOffset runTime)
		{
			cachedTask.S5_NextScheduledPrintRunTimeUtc = runTime.UtcDateTime;
		}

		public void SetLastRunTime(DateTimeOffset lastRunTime)
		{
			cachedTask.LastRunTime = lastRunTime.UtcDateTime;
		}

		public void SetStartDate(DateTimeOffset startDate)
		{
			cachedTask.S5_StartDate = startDate.UtcDateTime;
		}

		public void SetSchedule(int frequency, string recurrence)
		{
			cachedTask.S5_TaskPeriodCount = frequency;
			cachedTask.S5_TaskPeriod = recurrence;
		}

		public void SetWeeklySchedule(int frequency, DayOfWeek[] daysOfWeek)
		{
			cachedTask.S5_TaskPeriodCount = frequency;
			cachedTask.S5_TaskPeriod = ScheduleRecurrenceType.Weekly;

			char[] days = { 'N', 'N', 'N', 'N', 'N', 'N', 'N' };
			foreach (var dayOfWeek in daysOfWeek)
			{
				days[(int)dayOfWeek] = 'Y';
			}
			cachedTask.S5_DayList = new string(days);
		}

		public void SetMonthlySchedule(int frequency, int dayOfMonth)
		{
			cachedTask.S5_TaskPeriodCount = frequency;
			cachedTask.S5_TaskPeriod = ScheduleRecurrenceType.Monthly;
			cachedTask.Recurrence.DayOfMonth = dayOfMonth;
		}

		public void SetDailyStartTimeLocal(TimeSpan startTime, TimeSpan offSet)
		{
			cachedTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.Add(startTime + offSet);
		}

		public void SetDailyStartTimeUtc(TimeSpan startTime, TimeSpan offSet)
		{
			cachedTask.CalcDailyStartTimeUtc = ZDateTime.MinSmallDateTimeValue.Date.Add(startTime + offSet);
		}

		public void SetDailyEndTimeLocal(TimeSpan endTime)
		{
			cachedTask.S5_DailyEndTime = ZDateTime.MinSmallDateTimeValue.Date.Add(endTime);
		}

		public void SetDailyEndTimeUtc(TimeSpan endTime)
		{
			cachedTask.CalcDailyEndTimeUtc = ZDateTime.MinSmallDateTimeValue.Date.Add(endTime);
		}

		public void SetOverdueDuration(TimeSpan durationOverdue)
		{
			cachedTask.S5_OverdueDurationInSeconds = (ZInt)durationOverdue.TotalSeconds;
		}

		public TaskInstanceStatus UpdateStatus()
		{
			return cachedTask.UpdateStatus();
		}

		public void Reload()
		{
			cachedTask.Reload();
		}

		public void OnErrorReported()
		{
			cachedTask.OnErrorReported();
		}

		public bool ResetScheduleToDefault(bool reEnableMandatory, ILogger logger)
		{
			var scheduleHasBeenUpdated = FixTaskIsActiveForMandatoryTasks();
			if (!cachedTask.S5_IsActive || !CheckAndFixSchedulePeriod())
			{
				return scheduleHasBeenUpdated;
			}

			ReCalculateNextRunTime();
			return true;

			void ReCalculateNextRunTime()
			{
				var originalNextRuntime = cachedTask.S5_NextScheduledPrintRunTimeUtc;
				cachedTask.S5_NextScheduledPrintRunTimeUtc = DateTime.MinValue;
				cachedTask.S5_NextScheduledPrintRunTimeUtc = cachedTask.CalculateNextRunTime(logger);
				if (cachedTask.S5_NextScheduledPrintRunTimeUtc != originalNextRuntime)
				{
					logger.Log(
						LogLevel.Warning,
						Invariant($"Service task {GovernedTask.Description} - \"next runtime\" has been corrected from: {originalNextRuntime:o} to: {cachedTask.S5_NextScheduledPrintRunTimeUtc:o} because the original scheduled value was outside the allowed interval."));
				}
			}

			bool FixTaskIsActiveForMandatoryTasks()
			{
				if (reEnableMandatory && !cachedTask.S5_IsActive && cachedTask.StaticServiceAttributes.IsMandatory)
				{
					logger.Log(LogLevel.Warning, Invariant($"Task {GovernedTask.Description} is mandatory but inactive - reactivating"));
					cachedTask.S5_IsActive = true;
					return true;
				}

				return false;
			}

			bool CheckAndFixSchedulePeriod()
			{
				var scheduledPeriod = cachedTask.SchedulePeriodDuration;
				var scheduleUpdatedToMinimum = FixPeriodIfOutOfLimit(
					cachedTask.StaticServiceAttributes.MinimumPeriod,
					scheduledPeriod,
					TimeSpan.MinValue,
					periodLimitedByTaskAttribute => scheduledPeriod < periodLimitedByTaskAttribute);
				var scheduleUpdatedToMaximum = FixPeriodIfOutOfLimit(
					cachedTask.StaticServiceAttributes.MaximumPeriod,
					scheduledPeriod,
					TimeSpan.MaxValue,
					periodLimitedByTaskAttribute => scheduledPeriod > periodLimitedByTaskAttribute);

				return scheduleUpdatedToMinimum || scheduleUpdatedToMaximum;
			}

			bool FixPeriodIfOutOfLimit(string taskAttributePeriodLimit, TimeSpan scheduledPeriod, TimeSpan defaultToDuration, Func<TimeSpan, bool> isOutOfLimit)
			{
				taskAttributePeriodLimit = taskAttributePeriodLimit ?? string.Empty;
				var periodLimitByTaskAttribute = ServiceTaskScheduleValidation.GetPeriodDuration(taskAttributePeriodLimit, defaultToDuration, isRandomPeriod: false);
				if (isOutOfLimit(periodLimitByTaskAttribute)
					&& ServiceTaskScheduleValidation.ParseFrequency(taskAttributePeriodLimit, out int limitedPeriodCount, out string limitedPeriodType))
				{
					cachedTask.S5_TaskPeriodCount = limitedPeriodCount;
					cachedTask.S5_TaskPeriod = limitedPeriodType;
					logger.Log(LogLevel.Warning, Invariant($"Service task period [{scheduledPeriod}] was updated to [{periodLimitByTaskAttribute}] to be in allowed interval for service task {GovernedTask.Description}."));

					switch (cachedTask.S5_TaskPeriod)
					{
						case ScheduleRecurrenceType.Weekly:
							if (string.Equals(cachedTask.S5_DayList, "NNNNNNN", StringComparison.InvariantCultureIgnoreCase))
							{
								cachedTask.S5_DayList = "YNNNNNN";
								logger.Log(LogLevel.Warning, Invariant($"Service task {GovernedTask.Description} has been set to run on Sunday(s) because there was no day selected on the weekly schedule."));
							}
							break;

						case ScheduleRecurrenceType.Yearly:
							if (cachedTask.S5_MonthNumber == 0)
							{
								cachedTask.S5_MonthNumber = 1;
								cachedTask.S5_DayNumber = 1;
								logger.Log(LogLevel.Warning, Invariant($"Service task {GovernedTask.Description} has been set to run on January because there was no month of the year selected on the yearly schedule."));
							}

							break;
					}

					return true;
				}

				return false;
			}
		}

		public IServiceTask GovernedTask => governedTask ??= new SchedulerServiceTask(cachedTask);

		public INextRunTimeCalculator Calculator => null;

		readonly ServiceTaskSchedule cachedTask;
		readonly BusinessObjectFactory factory;
		IServiceTask governedTask;
	}
}
