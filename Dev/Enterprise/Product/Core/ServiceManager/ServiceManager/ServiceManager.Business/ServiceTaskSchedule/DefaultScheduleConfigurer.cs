using System;
using Enterprise.Scheduler.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace Enterprise.ServiceManager.Business
{
	public class DefaultScheduleConfigurer : IDefaultScheduleConfigurer
	{
		public DefaultScheduleConfigurer(IDateTimeProvider dateTimeProvider)
		{
			this.dateTimeProvider = dateTimeProvider;
		}

		public void SetDefaultScheduleForTaskForDisplay(IServiceTaskGovernor taskGovernor, IHostedServiceAttribute hostedServiceAttribute)
		{
			disableRandomOffset = true;
			SetDefaultScheduleForTask(taskGovernor, hostedServiceAttribute);
		}

		public void SetDefaultScheduleForTask(IServiceTaskGovernor taskGovernor, IHostedServiceAttribute hostedServiceAttribute)
		{
			var defaultSchedule = hostedServiceAttribute.DefaultSchedule;

			var now = new DateTimeOffset(dateTimeProvider.CurrentDateTimeUtc, TimeSpan.Zero);
			taskGovernor.SetStartDate(now);
			taskGovernor.SetNextRunTime(now);
			taskGovernor.SetActive(false);

			if (ServiceTaskScheduleValidation.ParseFrequency(defaultSchedule.RunEvery, out var scheduleFrequency, out var scheduleRecurrence))
			{
				if (scheduleRecurrence == ScheduleRecurrenceType.Weekly)
				{
					ValidateAndSetWeeklySchedule(taskGovernor, scheduleFrequency, defaultSchedule.DaysOfWeek);
				}
				else if (scheduleRecurrence == ScheduleRecurrenceType.Monthly)
				{
					ValidateAndSetDayOfMonth(taskGovernor, scheduleFrequency, defaultSchedule.DayOfMonth);
				}
				else
				{
					taskGovernor.SetSchedule(scheduleFrequency, scheduleRecurrence);
				}
			}

			if (!string.IsNullOrEmpty(defaultSchedule.StartAtLocal))
			{
				var defaultLocalStart = ServiceTaskScheduleValidation.GetPeriodDuration(defaultSchedule.StartAtLocal, TimeSpan.Zero, isRandomPeriod: false);

				taskGovernor.SetDailyStartTimeLocal(defaultLocalStart, GetRandomStartOffsetDuration(defaultSchedule.RandomStartOffset));
				SetNextRunTime(taskGovernor, now);
			}

			if (!string.IsNullOrEmpty(defaultSchedule.StartAtUtc))
			{
				var defaultUtcStart = ServiceTaskScheduleValidation.GetPeriodDuration(defaultSchedule.StartAtUtc, TimeSpan.Zero, isRandomPeriod: false);

				taskGovernor.SetDailyStartTimeUtc(defaultUtcStart, GetRandomStartOffsetDuration(defaultSchedule.RandomStartOffset));
				SetNextRunTime(taskGovernor, now);
			}

			if (!string.IsNullOrEmpty(defaultSchedule.EndAtLocal))
			{
				taskGovernor.SetDailyEndTimeLocal(ServiceTaskScheduleValidation.GetPeriodDuration(defaultSchedule.EndAtLocal, TimeSpan.Zero, isRandomPeriod: false));
				SetNextRunTime(taskGovernor, now);
			}

			if (!string.IsNullOrEmpty(defaultSchedule.EndAtUtc))
			{
				taskGovernor.SetDailyEndTimeUtc(ServiceTaskScheduleValidation.GetPeriodDuration(defaultSchedule.EndAtUtc, TimeSpan.Zero, isRandomPeriod: false));
				SetNextRunTime(taskGovernor, now);
			}

			if (!string.IsNullOrEmpty(defaultSchedule.DoNotRunTillNextDueTimeIfOverdue))
			{
				taskGovernor.SetOverdueDuration(ServiceTaskScheduleValidation.GetPeriodDuration(defaultSchedule.DoNotRunTillNextDueTimeIfOverdue, TimeSpan.Zero, isRandomPeriod: false));
			}

			taskGovernor.SetActive(hostedServiceAttribute.ActiveByDefault);
		}

		public void SetNextRunTimeForTask(IServiceTaskGovernor taskGovernor, DateTime nextRunTime)
		{
			var now = new DateTimeOffset(dateTimeProvider.CurrentDateTimeUtc, TimeSpan.Zero);
			taskGovernor.SetNextRunTime(nextRunTime);
			startDateAlreadySetUp = true;
			SetNextRunTime(taskGovernor, now);
		}

		void SetNextRunTime(IServiceTaskGovernor taskGovernor, DateTimeOffset now)
		{
			if (!startDateAlreadySetUp || taskGovernor.GovernedTask.NextRunTime < now)
			{
				if (taskGovernor.GovernedTask.DailyStartTime == TimeSpan.Zero)
				{
					taskGovernor.SetNextRunTime(now);
				}
				else
				{
					var startTime = taskGovernor.GovernedTask.DailyStartTime;
					if (taskGovernor.GovernedTask.DailyEndTime == TimeSpan.Zero)
					{
						taskGovernor.SetNextRunTime(new DateTimeOffset(now.Date.Add(startTime), TimeSpan.Zero));
					}
					else
					{
						var endTime = taskGovernor.GovernedTask.DailyEndTime;

						if (startTime > endTime && now.TimeOfDay < endTime)
						{
							taskGovernor.SetNextRunTime(now);
						}
						else
						{
							taskGovernor.SetNextRunTime(new DateTimeOffset(now.Date.Add(startTime), TimeSpan.Zero));
						}
					}
				}

				startDateAlreadySetUp = false;
			}
		}

		static void ValidateAndSetWeeklySchedule(IServiceTaskGovernor taskGovernor, int frequency, params DayOfWeek[] daysOfWeek)
		{
			if (daysOfWeek == null || daysOfWeek.Length <= 0)
			{
				throw new InvalidOperationException("Cannot calculate weekly schedule when no days are selected");
			}

			taskGovernor.SetWeeklySchedule(frequency, daysOfWeek);
		}

		static void ValidateAndSetDayOfMonth(IServiceTaskGovernor taskGovernor, int frequency, int dayOfMonth)
		{
			if (dayOfMonth <= 0)
			{
				throw new InvalidOperationException("Cannot set monthly schedule when provided an invalid day");
			}
			if (dayOfMonth > 28)
			{
				throw new InvalidOperationException("Cannot set monthly schedule for day that exceeds the shortest month");
			}

			taskGovernor.SetMonthlySchedule(frequency, dayOfMonth);
		}

		TimeSpan GetRandomStartOffsetDuration(string randomStartOffset)
		{
			return string.IsNullOrEmpty(randomStartOffset) || disableRandomOffset
				? TimeSpan.Zero
				: ServiceTaskScheduleValidation.GetPeriodDuration(randomStartOffset, TimeSpan.Zero, isRandomPeriod: true);
		}

		bool startDateAlreadySetUp;
		bool disableRandomOffset;
		readonly IDateTimeProvider dateTimeProvider;
	}
}

