using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.ServiceManager;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class StmServiceTaskAdapter : AutoStmServiceTaskAdapter, IServiceTaskSchedule
	{
		public StmServiceTaskAdapter(StmServiceTask adaptedServiceTask)
			: this(new BusinessObjectFactory(), adaptedServiceTask)
		{
		}

		public StmServiceTaskAdapter(BusinessObjectFactory factory, StmServiceTask adaptedServiceTask)
			: base(factory)
		{
			serviceTask = adaptedServiceTask;
		}

		public ZBool SecondsRange
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorSeconds;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorSeconds { Period = 1 };
				}
			}
		}

		public ZBool MinutesRange
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorMinutes;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMinutes { Period = 1 };
				}
			}
		}

		public ZBool HoursRange
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorHours;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorHours { Period = 1 };
				}
			}
		}

		public ZBool DaysRange
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorDays or NextRunTimeCalculatorWorkingDays;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorDays { Period = 1 };
				}
			}
		}

		public ZBool WeeksRange
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorWeeks;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorWeeks { Period = 1, DaysOfOccurrence = new[] { DayOfWeek.Sunday } };
				}
			}
		}

		public ZBool MonthsRange
		{
			get
			{
				return serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorMonthsByDate
					or NextRunTimeCalculatorMonthsByDayOfWeek
					or NextRunTimeCalculatorMonthsByLastDay;
			}
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMonthsByDate { Period = 1, DayOfOccurrence = 1 };
				}
			}
		}

		public ZBool MonthsLastDay
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorMonthsByLastDay;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMonthsByLastDay { Period = 1 };
				}
			}
		}

		public ZBool MonthlyWeekDay
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorMonthsByDayOfWeek;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMonthsByDayOfWeek { DayOfTheWeek = DayOfWeek.Sunday, WeekOfTheMonth = 1 };
				}
			}
		}

		public ZBool YearsRange
		{
			get
			{
				return serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorYearsByDate
					or NextRunTimeCalculatorYearsByDayOfMonth;
			}
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorYearsByDate { Month = 1, Day = 1 };
				}
			}
		}

		public override ZInt Period
		{
			get =>
				serviceTask.NextRunTimeCalculator switch
				{
					NextRunTimeCalculatorSeconds calculatorSeconds => calculatorSeconds.Period,
					NextRunTimeCalculatorMinutes calculatorMinutes => calculatorMinutes.Period,
					NextRunTimeCalculatorHours calculatorHours => calculatorHours.Period,
					NextRunTimeCalculatorDays calculatorDays => calculatorDays.Period,
					NextRunTimeCalculatorWeeks calculatorWeeks => calculatorWeeks.Period,
					NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate => calculatorMonthsByDate.Period,
					NextRunTimeCalculatorMonthsByLastDay calculatorMonthsByLastDay => calculatorMonthsByLastDay.Period,
					_ => 1
				};

			set
			{
				switch (serviceTask.NextRunTimeCalculator)
				{
					case NextRunTimeCalculatorSeconds calculatorSeconds:
						calculatorSeconds.Period = value;
						serviceTask.NextRunTimeCalculator = calculatorSeconds;
						break;
					case NextRunTimeCalculatorMinutes calculatorMinutes:
						calculatorMinutes.Period = value;
						serviceTask.NextRunTimeCalculator = calculatorMinutes;
						break;
					case NextRunTimeCalculatorHours calculatorHours:
						calculatorHours.Period = value;
						serviceTask.NextRunTimeCalculator = calculatorHours;
						break;
					case NextRunTimeCalculatorDays calculatorDays:
						calculatorDays.Period = value;
						serviceTask.NextRunTimeCalculator = calculatorDays;
						break;
					case NextRunTimeCalculatorWeeks calculatorWeeks:
						calculatorWeeks.Period = value;
						serviceTask.NextRunTimeCalculator = calculatorWeeks;
						break;
					case NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate:
						calculatorMonthsByDate.Period = value;
						serviceTask.NextRunTimeCalculator = calculatorMonthsByDate;
						break;
					case NextRunTimeCalculatorMonthsByLastDay calculatorMonthsByLastDay:
						calculatorMonthsByLastDay.Period = value;
						serviceTask.NextRunTimeCalculator = calculatorMonthsByLastDay;
						break;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidatePeriod();
				}

				PeriodInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NextRunTimeCalculatorInfo => serviceTask.SST_ConfigurationInfo;

		[BusinessObjectTestExclude]
		public override ZDateTime CalcDailyEndTimeUtc
		{
			get
			{
				return serviceTask.NextRunTimeCalculator switch
				{
					NextRunTimeCalculatorSeconds calculatorSeconds => GetUtcDateTimeSafe(calculatorSeconds.EndTime),
					NextRunTimeCalculatorMinutes calculatorMinutes => GetUtcDateTimeSafe(calculatorMinutes.EndTime),
					NextRunTimeCalculatorHours calculatorHours => GetUtcDateTimeSafe(calculatorHours.EndTime),
					_ => ZDateTime.Empty
				};
			}
			set
			{
				switch (serviceTask.NextRunTimeCalculator)
				{
					case NextRunTimeCalculatorSeconds calculatorSeconds:
						calculatorSeconds.EndTime = value.IsValid ? value.TimeOfDay : null;
						serviceTask.NextRunTimeCalculator = calculatorSeconds;
						break;
					case NextRunTimeCalculatorMinutes calculatorMinutes:
						calculatorMinutes.EndTime = value.IsValid ? value.TimeOfDay : null;
						serviceTask.NextRunTimeCalculator = calculatorMinutes;
						break;
					case NextRunTimeCalculatorHours calculatorHours:
						calculatorHours.EndTime = value.IsValid ? value.TimeOfDay : null;
						serviceTask.NextRunTimeCalculator = calculatorHours;
						break;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateCalcDailyEndTimeUtc();
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZDateTime CalcDailyStartTimeUtc
		{
			get
			{
				return serviceTask.NextRunTimeCalculator switch
				{
					NextRunTimeCalculatorSeconds calculatorSeconds => GetUtcDateTimeSafe(calculatorSeconds.StartTime),
					NextRunTimeCalculatorMinutes calculatorMinutes => GetUtcDateTimeSafe(calculatorMinutes.StartTime),
					NextRunTimeCalculatorHours calculatorHours => GetUtcDateTimeSafe(calculatorHours.StartTime),
					_ => ZDateTime.Empty
				};
			}
			set
			{
				switch (serviceTask.NextRunTimeCalculator)
				{
					case NextRunTimeCalculatorSeconds calculatorSeconds:
						calculatorSeconds.StartTime = value.IsValid ? value.TimeOfDay : null;
						serviceTask.NextRunTimeCalculator = calculatorSeconds;
						break;
					case NextRunTimeCalculatorMinutes calculatorMinutes:
						calculatorMinutes.StartTime = value.IsValid ? value.TimeOfDay : null;
						serviceTask.NextRunTimeCalculator = calculatorMinutes;
						break;
					case NextRunTimeCalculatorHours calculatorHours:
						calculatorHours.StartTime = value.IsValid ? value.TimeOfDay : null;
						serviceTask.NextRunTimeCalculator = calculatorHours;
						break;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateCalcDailyStartTimeUtc();
				}
			}
		}

		public ZDateTime RecurringStartTimeUtc
		{
			get
			{
				return serviceTask.NextRunTimeCalculator switch
				{
					NextRunTimeCalculatorDays calculatorDays => GetUtcDateTimeSafe(calculatorDays.ScheduledRunTime),
					NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate => GetUtcDateTimeSafe(calculatorMonthsByDate.ScheduledRunTime),
					NextRunTimeCalculatorMonthsByDayOfWeek calculatorMonthsByDayOfWeek => GetUtcDateTimeSafe(calculatorMonthsByDayOfWeek.ScheduledRunTime),
					NextRunTimeCalculatorMonthsByLastDay calculatorMonthsByLastDay => GetUtcDateTimeSafe(calculatorMonthsByLastDay.ScheduledRunTime),
					NextRunTimeCalculatorWeeks calculatorWeeks => GetUtcDateTimeSafe(calculatorWeeks.ScheduledRunTime),
					NextRunTimeCalculatorWorkingDays calculatorWorkingDays => GetUtcDateTimeSafe(calculatorWorkingDays.ScheduledRunTime),
					NextRunTimeCalculatorYearsByDate calculatorYearsByDate => GetUtcDateTimeSafe(calculatorYearsByDate.ScheduledRunTime),
					NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth => GetUtcDateTimeSafe(calculatorYearsByDayOfMonth.ScheduledRunTime),
					_ => ZDateTime.Empty
				};
			}
			set
			{
				switch (serviceTask.NextRunTimeCalculator)
				{
					case NextRunTimeCalculatorDays calculatorDays:
						calculatorDays.ScheduledRunTime = value.TimeOfDay;
						serviceTask.NextRunTimeCalculator = calculatorDays;
						break;
					case NextRunTimeCalculatorWeeks calculatorWeeks:
						calculatorWeeks.ScheduledRunTime = value.TimeOfDay;
						serviceTask.NextRunTimeCalculator = calculatorWeeks;
						break;
					case NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate:
						calculatorMonthsByDate.ScheduledRunTime = value.TimeOfDay;
						serviceTask.NextRunTimeCalculator = calculatorMonthsByDate;
						break;
					case NextRunTimeCalculatorMonthsByDayOfWeek calculatorMonthsByDayOfWeek:
						calculatorMonthsByDayOfWeek.ScheduledRunTime = value.TimeOfDay;
						serviceTask.NextRunTimeCalculator = calculatorMonthsByDayOfWeek;
						break;
					case NextRunTimeCalculatorMonthsByLastDay calculatorMonthsByLastDay:
						calculatorMonthsByLastDay.ScheduledRunTime = value.TimeOfDay;
						serviceTask.NextRunTimeCalculator = calculatorMonthsByLastDay;
						break;
					case NextRunTimeCalculatorWorkingDays calculatorWorkingDays:
						calculatorWorkingDays.ScheduledRunTime = value.TimeOfDay;
						serviceTask.NextRunTimeCalculator = calculatorWorkingDays;
						break;
					case NextRunTimeCalculatorYearsByDate calculatorYearsByDate:
						calculatorYearsByDate.ScheduledRunTime = value.TimeOfDay;
						serviceTask.NextRunTimeCalculator = calculatorYearsByDate;
						break;
					case NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth:
						calculatorYearsByDayOfMonth.ScheduledRunTime = value.TimeOfDay;
						serviceTask.NextRunTimeCalculator = calculatorYearsByDayOfMonth;
						break;
				}
			}
		}

		public ZString CalcDailyEndTimeLocalText
		{
			get
			{
				var endTime = serviceTask.NextRunTimeCalculator switch
				{
					NextRunTimeCalculatorSeconds calculatorSeconds => calculatorSeconds.EndTime,
					NextRunTimeCalculatorMinutes calculatorMinutes => calculatorMinutes.EndTime,
					NextRunTimeCalculatorHours calculatorHours => calculatorHours.EndTime,
					_ => null
				};

				return ConvertToLocalTimeDisplayString(endTime);
			}
		}

		public ZString CalcDailyStartTimeLocalText
		{
			get
			{
				var startTime = serviceTask.NextRunTimeCalculator switch
				{
					NextRunTimeCalculatorSeconds calculatorSeconds => calculatorSeconds.StartTime,
					NextRunTimeCalculatorMinutes calculatorMinutes => calculatorMinutes.StartTime,
					NextRunTimeCalculatorHours calculatorHours => calculatorHours.StartTime,
					_ => null
				};

				return ConvertToLocalTimeDisplayString(startTime);
			}
		}

		public ZString RecurringStartTimeLocalText
		{
			get
			{
				TimeSpan? scheduledRunTime = serviceTask.NextRunTimeCalculator switch
				{
					NextRunTimeCalculatorDays calculatorDays => calculatorDays.ScheduledRunTime,
					NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate => calculatorMonthsByDate.ScheduledRunTime,
					NextRunTimeCalculatorMonthsByDayOfWeek calculatorMonthsByDayOfWeek => calculatorMonthsByDayOfWeek.ScheduledRunTime,
					NextRunTimeCalculatorMonthsByLastDay calculatorMonthsByLastDay => calculatorMonthsByLastDay.ScheduledRunTime,
					NextRunTimeCalculatorWeeks calculatorWeeks => calculatorWeeks.ScheduledRunTime,
					NextRunTimeCalculatorWorkingDays calculatorWorkingDays => calculatorWorkingDays.ScheduledRunTime,
					NextRunTimeCalculatorYearsByDate calculatorYearsByDate => calculatorYearsByDate.ScheduledRunTime,
					NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth => calculatorYearsByDayOfMonth.ScheduledRunTime,
					_ => null
				};

				return ConvertToLocalTimeDisplayString(scheduledRunTime);
			}
		}

		static ZDateTime GetUtcDateTimeSafe(TimeSpan? fromTime)
		{
			return fromTime is null
				? ZDateTime.Empty
				: ZDateTime.MinSmallDateTimeValue.Date.Add(fromTime.Value);
		}

		static string ConvertToLocalTimeDisplayString(TimeSpan? time)
		{
			if (!time.HasValue)
			{
				return string.Empty;
			}

			return new ZDateTime(Env.Time.GetLocalTimeFromUtc(ZDateTime.UtcNow.ToDateTime().Date.Add(time.Value)))
				.ToShortTimeString();
		}

		public ZString WeekCountAsString
		{
			get
			{
				return serviceTask.NextRunTimeCalculator switch
				{
					NextRunTimeCalculatorMonthsByDayOfWeek calculatorMonthsByDayOfWeek => calculatorMonthsByDayOfWeek.WeekOfTheMonth.ToString(),
					NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth => calculatorYearsByDayOfMonth.WeekOfTheMonth.ToString(),
					_ => "1"
				};
			}
			set
			{
				int result;
				switch (serviceTask.NextRunTimeCalculator)
				{
					case NextRunTimeCalculatorMonthsByDayOfWeek calculatorMonthsByDayOfWeek:
						calculatorMonthsByDayOfWeek.WeekOfTheMonth = int.TryParse(value, out result) ? result : 0;
						serviceTask.NextRunTimeCalculator = calculatorMonthsByDayOfWeek;
						break;
					case NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth:
						calculatorYearsByDayOfMonth.WeekOfTheMonth = int.TryParse(value, out result) ? result : 0;
						serviceTask.NextRunTimeCalculator = calculatorYearsByDayOfMonth;
						break;
				}
			}
		}

		public override ZInt DayOfMonth
		{
			get
			{
				return serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate
					? calculatorMonthsByDate.DayOfOccurrence
					: 1;
			}
			set
			{
				if (serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate)
				{
					calculatorMonthsByDate.DayOfOccurrence = value;
					serviceTask.NextRunTimeCalculator = calculatorMonthsByDate;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDayOfMonth();
				}

				DayOfMonthInfo.RefreshBinding();
			}
		}

		public ZBool MonthlyDay
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorMonthsByDate;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMonthsByDate { DayOfOccurrence = 1, Period = 1 };
				}
			}
		}

		[MaxLength(3)]
		[BusinessObjectTestExclude]
		public ZString DayName
		{
			get
			{
				return serviceTask.NextRunTimeCalculator switch
				{
					NextRunTimeCalculatorMonthsByDayOfWeek calculatorMonthsByDayOfWeek => Lookups.WeekDays.GetCodeFromDescription(((int)calculatorMonthsByDayOfWeek.DayOfTheWeek + 1).ToString()),
					NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth => Lookups.WeekDays.GetCodeFromDescription(((int)calculatorYearsByDayOfMonth.DayOfTheWeek + 1).ToString()),
					_ => ""
				};
			}
			set
			{
				CheckMaximumLength(DayNameInfo, value);

				DayOfWeek dayOfWeek;
				if (int.TryParse(Lookups.WeekDays.GetDescriptionFromCode(value), out var dayName))
				{
					dayOfWeek = (DayOfWeek)(dayName - 1);
				}
				else
				{
					dayOfWeek = DayOfWeek.Sunday;
				}

				switch (serviceTask.NextRunTimeCalculator)
				{
					case NextRunTimeCalculatorMonthsByDayOfWeek calculatorMonthsByDayOfWeek:
						calculatorMonthsByDayOfWeek.DayOfTheWeek = dayOfWeek;
						serviceTask.NextRunTimeCalculator = calculatorMonthsByDayOfWeek;
						break;
					case NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth:
						calculatorYearsByDayOfMonth.DayOfTheWeek = dayOfWeek;
						serviceTask.NextRunTimeCalculator = calculatorYearsByDayOfMonth;
						break;
				}

				DayNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DayNameInfo => GetZPropertyInfo(nameof(DayName));

		[BusinessObjectTestExclude]
		public override ZBool Sunday
		{
			get => WeeksCalculatorContainsDayOfWeek(DayOfWeek.Sunday);
			set
			{
				if (value)
				{
					AddDayOfWeekToWeeksCalculator(DayOfWeek.Sunday);
				}
				else
				{
					RemoveDayOfWeekFromWeeksCalculator(DayOfWeek.Sunday);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDaysOfWeek();
				}

				SundayInfo.RefreshBinding();
				serviceTask.SST_ConfigurationInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude]
		public override ZBool Monday
		{
			get => WeeksCalculatorContainsDayOfWeek(DayOfWeek.Monday);
			set
			{
				if (value)
				{
					AddDayOfWeekToWeeksCalculator(DayOfWeek.Monday);
				}
				else
				{
					RemoveDayOfWeekFromWeeksCalculator(DayOfWeek.Monday);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDaysOfWeek();
				}

				MondayInfo.RefreshBinding();
				serviceTask.SST_ConfigurationInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude]
		public override ZBool Tuesday
		{
			get => WeeksCalculatorContainsDayOfWeek(DayOfWeek.Tuesday);
			set
			{
				if (value)
				{
					AddDayOfWeekToWeeksCalculator(DayOfWeek.Tuesday);
				}
				else
				{
					RemoveDayOfWeekFromWeeksCalculator(DayOfWeek.Tuesday);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDaysOfWeek();
				}

				TuesdayInfo.RefreshBinding();
				serviceTask.SST_ConfigurationInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude]
		public override ZBool Wednesday
		{
			get => WeeksCalculatorContainsDayOfWeek(DayOfWeek.Wednesday);
			set
			{
				if (value)
				{
					AddDayOfWeekToWeeksCalculator(DayOfWeek.Wednesday);
				}
				else
				{
					RemoveDayOfWeekFromWeeksCalculator(DayOfWeek.Wednesday);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDaysOfWeek();
				}

				WednesdayInfo.RefreshBinding();
				serviceTask.SST_ConfigurationInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude]
		public override ZBool Thursday
		{
			get => WeeksCalculatorContainsDayOfWeek(DayOfWeek.Thursday);
			set
			{
				if (value)
				{
					AddDayOfWeekToWeeksCalculator(DayOfWeek.Thursday);
				}
				else
				{
					RemoveDayOfWeekFromWeeksCalculator(DayOfWeek.Thursday);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDaysOfWeek();
				}

				ThursdayInfo.RefreshBinding();
				serviceTask.SST_ConfigurationInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude]
		public override ZBool Friday
		{
			get => WeeksCalculatorContainsDayOfWeek(DayOfWeek.Friday);
			set
			{
				if (value)
				{
					AddDayOfWeekToWeeksCalculator(DayOfWeek.Friday);
				}
				else
				{
					RemoveDayOfWeekFromWeeksCalculator(DayOfWeek.Friday);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDaysOfWeek();
				}

				FridayInfo.RefreshBinding();
				serviceTask.SST_ConfigurationInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude]
		public override ZBool Saturday
		{
			get => WeeksCalculatorContainsDayOfWeek(DayOfWeek.Saturday);
			set
			{
				if (value)
				{
					AddDayOfWeekToWeeksCalculator(DayOfWeek.Saturday);
				}
				else
				{
					RemoveDayOfWeekFromWeeksCalculator(DayOfWeek.Saturday);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDaysOfWeek();
				}

				SaturdayInfo.RefreshBinding();
				serviceTask.SST_ConfigurationInfo.RefreshBinding();
			}
		}

		bool WeeksCalculatorContainsDayOfWeek(DayOfWeek dayOfWeek)
		{
			return (serviceTask.NextRunTimeCalculator as NextRunTimeCalculatorWeeks)
				?.DaysOfOccurrence
				.Contains(dayOfWeek) ?? false;
		}

		void AddDayOfWeekToWeeksCalculator(DayOfWeek dayOfWeek)
		{
			if (serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorWeeks calculatorWeeks)
			{
				if (!calculatorWeeks.DaysOfOccurrence.Contains(dayOfWeek))
				{
					calculatorWeeks.DaysOfOccurrence = new List<DayOfWeek>(calculatorWeeks.DaysOfOccurrence) { dayOfWeek }.ToArray();
				}
				serviceTask.NextRunTimeCalculator = calculatorWeeks;
			}
		}

		void RemoveDayOfWeekFromWeeksCalculator(DayOfWeek dayOfWeek)
		{
			if (serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorWeeks calculatorWeeks)
			{
				if (calculatorWeeks.DaysOfOccurrence.Contains(dayOfWeek))
				{
					calculatorWeeks.DaysOfOccurrence = calculatorWeeks.DaysOfOccurrence.Where(d => d != dayOfWeek).ToArray();
					serviceTask.NextRunTimeCalculator = calculatorWeeks;
				}
			}
		}

		public ZBool WeekDaysOnly
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorWorkingDays;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorWorkingDays();
				}
			}
		}

		public ZBool DailyDay
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorDays;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorDays { Period = 1 };
				}
			}
		}

		public ZBool YearlyEvery
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorYearsByDate;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorYearsByDate { Day = 1, Month = 1 };
				}
			}
		}

		public ZBool YearlyWeekDay
		{
			get => serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorYearsByDayOfMonth;
			set
			{
				if (value)
				{
					serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorYearsByDayOfMonth { DayOfTheWeek = DayOfWeek.Sunday, MonthOfTheYear = 1, WeekOfTheMonth = 1 };
				}
			}
		}

		public override ZInt YearlyDay
		{
			get
			{
				return serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorYearsByDate calculatorYearsByDate
					? calculatorYearsByDate.Day
					: 1;
			}
			set
			{
				if (serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorYearsByDate calculatorYearsByDate)
				{
					calculatorYearsByDate.Day = value;
					serviceTask.NextRunTimeCalculator = calculatorYearsByDate;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateYearlyDay();
				}

				YearlyDayInfo.RefreshBinding();
			}
		}

		public ZString EveryMonthNumberDayAsString
		{
			get
			{
				return serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorYearsByDate calculatorYearsByDate
					? calculatorYearsByDate.Month.ToString()
					: "1";
			}
			set
			{
				if (serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorYearsByDate calculatorYearsByDate && ZByte.TryParse(value, out var monthNumber))
				{
					calculatorYearsByDate.Month = monthNumber;
					serviceTask.NextRunTimeCalculator = calculatorYearsByDate;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateYearlyDay();
				}
			}
		}

		public ZString MonthNumberAsString
		{
			get
			{
				return serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth
					? calculatorYearsByDayOfMonth.MonthOfTheYear.ToString()
					: "1";
			}
			set
			{
				if (serviceTask.NextRunTimeCalculator is NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth && ZByte.TryParse(value, out var monthOfTheYear))
				{
					calculatorYearsByDayOfMonth.MonthOfTheYear = monthOfTheYear;
					serviceTask.NextRunTimeCalculator = calculatorYearsByDayOfMonth;
				}
			}
		}

		public ZString RandomStartOffset
		{
			get
			{
				var defaultSchedule = serviceTask.StaticServiceAttributes.DefaultSchedule;
				var randomDuration = ServiceTaskScheduleValidation.GetPeriodDuration(defaultSchedule.RandomStartOffset, TimeSpan.Zero, isRandomPeriod: false);
				return ZDateTime.MinSmallDateTimeValue.Date.Add(randomDuration).ToShortTimeString();
			}
		}

		public StmServiceTaskLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new StmServiceTaskLookups(serviceTask);
				}
				return lookups;
			}
		}

		StmServiceTaskLookups lookups;

		internal INextRunTimeCalculator TaskNextRunTimeCalculator => serviceTask.NextRunTimeCalculator;
		internal bool TaskIsNudgeable => serviceTask.IsNudgeable;
		internal IHostedServiceAttribute TaskAttributes => serviceTask.StaticServiceAttributes;
		internal TimeSpan TaskScheduleDuration => serviceTask.SchedulePeriodDuration;

		string IServiceTaskSchedule.GetBranchCountryCode()
		{
			throw new NotImplementedException();
		}

		string IServiceTaskSchedule.ConfigString
		{
			get => serviceTask.ConfigString;
			set => serviceTask.ConfigString = value;
		}

		bool IServiceTaskSchedule.IsNudgeable => serviceTask.IsNudgeable;

		ZGuid IStmScheduleTask.S5_GB
		{
			get => serviceTask.SST_GB_Branch;
			set => serviceTask.SST_GB_Branch = value;
		}

		ZString IStmScheduleTask.S5_ParentTableCode
		{
			get => Constants.ServiceTask.ParentTableCode;
			set { }
		}

		ZString IStmScheduleTask.S5_ScheduleDescription
		{
			get => serviceTask.Description;
			set { }
		}

		ZString IStmScheduleTask.S5_ScheduleType
		{
			get => serviceTask.SST_ServiceTaskCode;
			set { }
		}

		ZBool IStmScheduleTask.S5_IsActive
		{
			get => serviceTask.SST_Active;
			set => serviceTask.SST_Active = value;
		}

		ZDateTime IStmScheduleTask.S5_NextScheduledPrintRunTimeUtc
		{
			get => serviceTask.NextRunTime;
			set { }
		}

		readonly StmServiceTask serviceTask;
	}
}
