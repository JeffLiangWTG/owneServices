using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Microsoft.Extensions.Logging;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	class SchedulerServiceTask : IServiceTask
	{
		public SchedulerServiceTask(ServiceTaskSchedule task)
		{
			cachedTask = task;
		}

		public Guid Pk
		{
			get
			{
				return cachedTask.PK.IsEmpty || !cachedTask.PK.IsValid
					? Guid.Empty
					: cachedTask.PK.ToGuid();
			}
		}

		public bool IsActive => cachedTask.S5_IsActive;

		public string Code => cachedTask.S5_ScheduleType;

		public string Description => cachedTask.S5_ScheduleDescription;

		public string MultilingualDescription => cachedTask.S5_ScheduleDescriptionMultilingual;

		public Guid BranchPk
		{
			get
			{
				return cachedTask.S5_GB.IsEmpty || !cachedTask.S5_GB.IsValid
					? Guid.Empty
					: cachedTask.S5_GB.ToGuid();
			}
		}

		public string BranchName => cachedTask.BranchName;

		public DateTimeOffset NextRunTime
		{
			get
			{
				return cachedTask.S5_NextScheduledPrintRunTimeUtc.IsValid
					? cachedTask.S5_NextScheduledPrintRunTimeUtc.UtcToDateTimeOffset().ToDateTimeOffset()
					: DateTimeOffset.MinValue;
			}
		}

		public TimeSpan DailyStartTime => cachedTask.S5_DailyStartTime.TimeOfDay;

		public TimeSpan DailyEndTime => cachedTask.S5_DailyEndTime.TimeOfDay;

		public string BranchErrorMessage =>
			cachedTask.S5_GBInfo.Notifications
				.GetNotifications(CargoWise.ComponentModel.NotificationType.Error)
				.FirstOrDefault()
				?.Message;

		public string ConfigString => cachedTask.ConfigString;

		public string SettingsXml => cachedTask.S5_ScheduleState.ToAscii();

		public IReadOnlyList<DayOfWeek> ScheduleDaysOfWeek
		{
			get
			{
				var result = new List<DayOfWeek>();
				var daysLength = cachedTask.S5_DayList.Length;
				for (int i = 0; i < daysLength; i++)
				{
					if (char.ToUpper(cachedTask.S5_DayList[i]).Equals('Y'))
					{
						result.Add((DayOfWeek)i);
					}
				}

				return result;
			}
		}

		public int ScheduleDayOfMonth => cachedTask.Recurrence.DayOfMonth;
		public int ScheduleDayNumber => cachedTask.S5_DayNumber;
		public int ScheduleMonth => cachedTask.S5_MonthNumber;
		public DateTimeOffset ScheduleStartDate => cachedTask.S5_StartDate.UtcToDateTimeOffset().ToDateTimeOffset();
		public string ScheduleOccurrence => cachedTask.S5_TaskPeriod;
		public int ScheduleFrequency => cachedTask.S5_TaskPeriodCount;
		public bool WeekDaysOnly => cachedTask.S5_WeekDaysOnly;
		public string Category => cachedTask.S5_TypeOfDocument;

		public void PreRunValidation()
		{
			cachedTask.Validation.ValidateAll();
		}

		readonly ServiceTaskSchedule cachedTask;

		public IServiceTaskScheduleThreadSafeReader ThreadSafeTaskReader
		{
			get
			{
				if (threadSafeTaskReader is not null)
				{
					return threadSafeTaskReader;
				}

				if (cachedTask is not null)
				{
					threadSafeTaskReader = new ServiceTaskScheduleThreadSafeReader(cachedTask);
				}
				else
				{
					threadSafeTaskReader = NullServiceTaskScheduleThreadSafeReader.Instance;
				}

				return threadSafeTaskReader;
			}
		}

		IServiceTaskScheduleThreadSafeReader threadSafeTaskReader;

		public void EnsureThreadSafety()
		{
			cachedTask.Factory.ThreadSentry.EnsureCurrentThreadIsOwner();
		}

		public DateTimeOffset CalculateNextRunTime(ILogger logger)
		{
			return new DateTimeOffset(cachedTask.CalculateNextRunTime(logger).ToDateTime(), TimeSpan.Zero);
		}

		public IServiceTaskGovernor AssignedGovernor => assignedGovernor ??= new SchedulerServiceTaskGovernor(cachedTask.Factory, cachedTask);

		IServiceTaskGovernor assignedGovernor;

		public TimeSpan OverdueDuration => TimeSpan.FromSeconds(cachedTask.S5_OverdueDurationInSeconds);

		public int SecondaryProcessesMaxCount => cachedTask.SecondaryProcessesMaxCount;

		public TimeSpan SchedulePeriodDuration => cachedTask.SchedulePeriodDuration;
	}
}
