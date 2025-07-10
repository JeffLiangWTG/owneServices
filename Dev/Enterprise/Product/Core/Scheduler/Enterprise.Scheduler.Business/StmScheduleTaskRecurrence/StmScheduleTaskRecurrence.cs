using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskRecurrence : NonPersistentBusinessObject
	{
		public StmScheduleTaskRecurrence(StmScheduleTask scheduleTask)
			: base(scheduleTask.Factory)
		{
			this.scheduleTask = scheduleTask;
			scheduleTask.S5_StartDateInfo.ValueChanged += delegate
			{ RefreshStartDate(); };
		}

		internal bool IsLastDay => (DayNumber == 99);

		bool IsRangeAndNotDay => (DailyRange && !DailyDay) || (MonthlyRange && !MonthlyDay) || (AccountingRange && !AccountingDay);

		bool IsRangeAndNotWeekDay => (YearlyRange && !YearlyWeekDay) || (MonthlyRange && !MonthlyWeekDay) || (AccountingRange && !AccountingWeekDay);

		ScheduleCalculator Calculator
		{
			get
			{
				if (calculator == null)
				{
					calculator = new ScheduleCalculator(Factory);
				}
				return calculator;
			}
		}
		ScheduleCalculator calculator;

		public DayOfWeek GetDayOfWeek()
		{
			return Calculator.ConvertToDayOfWeek(DayNumber);
		}

		void RefreshStartDate()
		{
			StartDateLocalForUserInfo.RefreshBinding();
		}

		internal readonly StmScheduleTask scheduleTask;

		#region Proxied Properties

		#region Day List

		[BusinessObjectTestExclude]
		public ZString DayList
		{
			get => scheduleTask.S5_DayList;
			set
			{
				scheduleTask.S5_DayList = value;
				Validation.ValidateWeeklyDayInfos();
				scheduleTask.S5_DayListInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DayListInfo
		{
			get { return scheduleTask == null ? null : GetWrappedZPropertyInfo(nameof(DayList), x => scheduleTask.S5_DayListInfo); }
		}

		#endregion

		#region Day Number

		public ZByte DayNumber
		{
			get => scheduleTask.S5_DayNumber;
			set
			{
				scheduleTask.S5_DayNumber = value;
				DayNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DayNumberInfo => GetZPropertyInfo(nameof(DayNumber));

		#endregion

		#region End After Count

		public ZInt EndAfterCount
		{
			get => scheduleTask.S5_EndAfterCount;
			set => scheduleTask.S5_EndAfterCount = value;
		}

		public ZPropertyInfo EndAfterCountInfo
		{
			get { return scheduleTask == null ? null : GetWrappedZPropertyInfo(nameof(EndAfterCount), x => scheduleTask.S5_EndAfterCountInfo); }
		}

		protected bool EndAfterCount_ReadOnly => !EndAfter;

		#endregion

		#region End Date

		public ZDateTime EndDateLocal
		{
			get => GetLocalDate(scheduleTask.S5_EndDate);
			set => scheduleTask.S5_EndDate = GetUtcDate(value);
		}

		public ZDateTime EndDateLocalForUser
		{
			get => GetLocalDateForUser(scheduleTask.S5_EndDate);
			set => scheduleTask.S5_EndDate = GetUtcDateFromLocalForUser(value);
		}

		public ZPropertyInfo EndDateLocalForUserInfo
		{
			get { return scheduleTask == null ? null : GetWrappedZPropertyInfo(nameof(EndDateLocalForUser), x => scheduleTask.S5_EndDateInfo); }
		}

		protected bool EndDateLocalForUser_ReadOnly => !EndBy;

		#endregion

		#region Month Number

		public ZByte MonthNumber
		{
			get => scheduleTask.S5_MonthNumber;
			set
			{
				scheduleTask.S5_MonthNumber = value;

				if (YearlyEvery && value > 0 && DayOfMonth > 0)
				{
					StartDateLocal = CalculateNextDateLocal(value, DayOfMonth);
				}
			}
		}

		public ZPropertyInfo MonthNumberInfo
		{
			get { return scheduleTask == null ? null : GetWrappedZPropertyInfo(nameof(MonthNumber), x => scheduleTask.S5_MonthNumberInfo); }
		}

		#endregion

		#region Next Scheduled Print Run Time

		public ZDateTime NextScheduledPrintRunTimeLocal
		{
			get => GetLocalDate(scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			set => scheduleTask.S5_NextScheduledPrintRunTimeUtc = GetUtcDate(value);
		}

		public ZPropertyInfo NextScheduledPrintRunTimeLocalInfo
		{
			get { return scheduleTask == null ? null : GetWrappedZPropertyInfo(nameof(NextScheduledPrintRunTimeLocal), x => scheduleTask.S5_NextScheduledPrintRunTimeUtcInfo); }
		}

		#endregion

		#region Start Date

		public ZDateTime StartDateLocal
		{
			get => GetLocalDate(scheduleTask.S5_StartDate);
			set
			{
				scheduleTask.S5_StartDate = GetUtcDate(value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateStartDateLocalForUser();
				}
			}
		}

		public ZDateTime StartDateLocalForUser
		{
			get => GetLocalDateForUser(scheduleTask.S5_StartDate);
			set
			{
				scheduleTask.S5_StartDate = GetUtcDateFromLocalForUser(value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateStartDateLocalForUser();
				}
			}
		}

		public ZPropertyInfo StartDateLocalForUserInfo => GetZPropertyInfo(nameof(StartDateLocalForUser));

		#endregion

		#region Recurring Start Time

		[BusinessObjectTestExclude]
		public ZDateTime RecurringStartTimeUtc
		{
			get => scheduleTask.CalcDailyStartTimeUtc;
			set
			{
				scheduleTask.CalcDailyStartTimeUtc = value;
				RecurringStartTimeUtcInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RecurringStartTimeUtcInfo => GetZPropertyInfo(nameof(RecurringStartTimeUtc));

		[BusinessObjectTestExclude]
		public ZDateTime RecurringStartTimeLocal
		{
			get => scheduleTask.CalcDailyStartTimeLocal;
			set
			{
				scheduleTask.CalcDailyStartTimeLocal = value;
				RecurringStartTimeLocalInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RecurringStartTimeLocalInfo => GetZPropertyInfo(nameof(RecurringStartTimeLocal));

		#endregion

		#region Task Period

		[MaxLength(StmScheduleTask.Schema.S5_TaskPeriodMaxLength)]
		public ZString TaskPeriod
		{
			get => scheduleTask.S5_TaskPeriod;
			set
			{
				CheckMaximumLength(TaskPeriodInfo, value);
				scheduleTask.S5_TaskPeriod = value;

				if (YearlyEvery && MonthNumber > 0 && DayOfMonth > 0)
				{
					StartDateLocal = CalculateNextDateLocal(MonthNumber, DayOfMonth);
				}

				TaskPeriodInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo TaskPeriodInfo => GetZPropertyInfo(nameof(TaskPeriod));

		#endregion

		#region Task Period Count

		[ReadOnlyMember(nameof(IsRangeAndNotDay))]
		public ZInt TaskPeriodCount
		{
			get => scheduleTask.S5_TaskPeriodCount;
			set
			{
				scheduleTask.S5_TaskPeriodCount = value;
				TaskPeriodCountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TaskPeriodCountInfo
		{
			get { return scheduleTask == null ? null : GetWrappedZPropertyInfo(nameof(TaskPeriodCount), x => scheduleTask.S5_TaskPeriodCountInfo); }
		}

		#endregion

		#region Week Days Only

		public ZBool WeekDaysOnly
		{
			get => scheduleTask.S5_WeekDaysOnly;
			set
			{
				scheduleTask.S5_WeekDaysOnly = value;
				WeekDaysOnlyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WeekDaysOnlyInfo => GetZPropertyInfo(nameof(WeekDaysOnly));

		#endregion

		#region Week Day Occurrence Number

		public ZByte WeekDayOccurrenceNumber
		{
			get => scheduleTask.S5_WeekDayOccurrenceNumber;
			set
			{
				scheduleTask.S5_WeekDayOccurrenceNumber = value;
				WeekDayOccurrenceNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WeekDayOccurrenceNumberInfo => GetZPropertyInfo(nameof(WeekDayOccurrenceNumber));

		#endregion

		#endregion

		#region Bound Properties

		#region End After

		#region End After

		public ZBool EndAfter
		{
			get => (EndAfterCount != 0);
			set
			{
				if (value)
				{
					if (EndAfterCount == 0)
					{
						EndAfterCount = 1;
					}
				}
				else
				{
					EndAfterCount = 0;
				}
				EndAfterCountInfo.RefreshBinding();
				EndAfterInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EndAfterInfo => GetZPropertyInfo(nameof(EndAfter));

		#endregion

		#region End By

		[BusinessObjectTestExclude]
		public ZBool EndBy
		{
			get => !EndDateLocal.IsEmpty && !EndAfter;
			set
			{
				if (value)
				{
					if (EndDateLocal.IsEmpty)
					{
						EndDateLocal = StartDateLocal;
					}
				}
				else
				{
					EndDateLocal = ZDateTime.Empty;
				}
				EndDateLocalForUserInfo.RefreshBinding();
				EndByInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EndByInfo => GetZPropertyInfo(nameof(EndBy));

		#endregion

		#region No End Date

		[BusinessObjectTestExclude]
		public ZBool NoEndDate
		{
			get => (EndAfterCount == 0) && EndDateLocal.IsEmpty;
			set
			{
				if (value)
				{
					EndAfterCount = 0;
					EndDateLocal = ZDateTime.Empty;
				}
				NoEndDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NoEndDateInfo => GetZPropertyInfo(nameof(NoEndDate));

		#endregion

		#endregion

		#region Recurrence Range

		#region Second Range

		public ZBool SecondRange
		{
			get => TaskPeriod == ScheduleRecurrenceType.Second;
			set
			{
				TaskPeriod = value ? ScheduleRecurrenceType.Second : "";
				SecondRangeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SecondRangeInfo => GetZPropertyInfo(nameof(SecondRange));

		#endregion

		#region Minute Range

		public ZBool MinuteRange
		{
			get => TaskPeriod == ScheduleRecurrenceType.Minute;
			set
			{
				TaskPeriod = value ? ScheduleRecurrenceType.Minute : "";
				MinuteRangeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MinuteRangeInfo => GetZPropertyInfo(nameof(MinuteRange));

		#endregion

		#region Hourly Range

		public ZBool HourlyRange
		{
			get => TaskPeriod == ScheduleRecurrenceType.Hourly;
			set
			{
				TaskPeriod = value ? ScheduleRecurrenceType.Hourly : "";
				HourlyRangeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo HourlyRangeInfo => GetZPropertyInfo(nameof(HourlyRange));

		#endregion

		#region Accounting Range

		public ZBool AccountingRange
		{
			get => TaskPeriod == ScheduleRecurrenceType.AccountingPeriod;
			set
			{
				TaskPeriod = value ? ScheduleRecurrenceType.AccountingPeriod : "";
				AccountingRangeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AccountingRangeInfo => GetZPropertyInfo(nameof(AccountingRange));

		#endregion

		#region Daily Range

		public ZBool DailyRange
		{
			get => TaskPeriod == ScheduleRecurrenceType.Daily;
			set
			{
				TaskPeriod = value ? ScheduleRecurrenceType.Daily : "";
				DailyRangeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DailyRangeInfo => GetZPropertyInfo(nameof(DailyRange));

		#endregion

		#region Monthly Range

		public ZBool MonthlyRange
		{
			get => TaskPeriod == ScheduleRecurrenceType.Monthly;
			set
			{
				TaskPeriod = value ? ScheduleRecurrenceType.Monthly : "";
				MonthlyRangeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MonthlyRangeInfo => GetZPropertyInfo(nameof(MonthlyRange));

		#endregion

		#region Weekly Range

		[BusinessObjectTestExclude]
		public ZBool WeeklyRange
		{
			get => TaskPeriod == ScheduleRecurrenceType.Weekly;
			set
			{
				TaskPeriod = value ? ScheduleRecurrenceType.Weekly : "";
				Validation.ValidateWeeklyDayInfos();
				WeeklyRangeInfo.RefreshBinding();
				scheduleTask.S5_DayListInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WeeklyRangeInfo => GetZPropertyInfo(nameof(WeeklyRange));

		#endregion

		#region Yearly Range

		public ZBool YearlyRange
		{
			get => TaskPeriod == ScheduleRecurrenceType.Yearly;
			set
			{
				TaskPeriod = value ? ScheduleRecurrenceType.Yearly : "";
				YearlyRangeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo YearlyRangeInfo => GetZPropertyInfo(nameof(YearlyRange));

		#endregion

		#endregion

		#region Week Days

		public IEnumerable<ZBool> WeekDays => new[] { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday };

		#region Sunday

		public ZBool Sunday
		{
			get => DayList.SubstringSafe(0, 1) == "Y";
			set
			{
				DayList = value.ToString() + DayList.SubstringSafe(1, 6);
				SundayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SundayInfo => GetZPropertyInfo(nameof(Sunday));

		#endregion

		#region Monday

		public ZBool Monday
		{
			get => DayList.SubstringSafe(1, 1) == "Y";
			set
			{
				DayList = DayList.SubstringSafe(0, 1) + value.ToString() + DayList.SubstringSafe(2, 5);
				MondayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MondayInfo => GetZPropertyInfo(nameof(Monday));

		#endregion

		#region Tuesday

		public ZBool Tuesday
		{
			get => DayList.SubstringSafe(2, 1) == "Y";
			set
			{
				DayList = DayList.SubstringSafe(0, 2) + value.ToString() + DayList.SubstringSafe(3, 4);
				TuesdayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TuesdayInfo => GetZPropertyInfo(nameof(Tuesday));

		#endregion

		#region Wednesday

		public ZBool Wednesday
		{
			get => DayList.SubstringSafe(3, 1) == "Y";
			set
			{
				DayList = DayList.SubstringSafe(0, 3) + value.ToString() + DayList.SubstringSafe(4, 3);
				WednesdayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WednesdayInfo => GetZPropertyInfo(nameof(Wednesday));

		#endregion

		#region Thursday

		public ZBool Thursday
		{
			get => DayList.SubstringSafe(4, 1) == "Y";
			set
			{
				DayList = DayList.SubstringSafe(0, 4) + value.ToString() + DayList.SubstringSafe(5, 2);
				ThursdayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ThursdayInfo => GetZPropertyInfo(nameof(Thursday));

		#endregion

		#region Friday

		public ZBool Friday
		{
			get => DayList.SubstringSafe(5, 1) == "Y";
			set
			{
				DayList = DayList.SubstringSafe(0, 5) + value.ToString() + DayList.SubstringSafe(6, 1);
				FridayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FridayInfo => GetZPropertyInfo(nameof(Friday));

		#endregion

		#region Saturday

		public ZBool Saturday
		{
			get => DayList.SubstringSafe(6, 1) == "Y";
			set
			{
				DayList = DayList.SubstringSafe(0, 6) + value.ToString();
				SaturdayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SaturdayInfo => GetZPropertyInfo(nameof(Saturday));

		#endregion

		#endregion

		#region Months

		#region Every Month Number Day as String

		[MaxLength(2)]
		public ZString EveryMonthNumberDayAsString
		{
			get
			{
				ZString result;
				if (everyMonthNumberDayAsString.HasValue)
				{
					result = everyMonthNumberDayAsString.Value;
				}
				else
				{
					result = MonthNumber.IsEmpty ? "" : MonthNumber.ToString();
				}
				return result;
			}
			set
			{
				CheckMaximumLength(EveryMonthNumberDayAsStringInfo, value);
				everyMonthNumberDayAsString = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEveryMonthNumberDayAsString();
				}
				ZByte monthNumber;
				if (!EveryMonthNumberDayAsStringInfo.HasErrors() && ZByte.TryParse(value, out monthNumber))
				{
					MonthNumber = monthNumber;
				}
				else
				{
					MonthNumber = 0;
				}
				EveryMonthNumberDayAsStringInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EveryMonthNumberDayAsStringInfo => GetZPropertyInfo(nameof(EveryMonthNumberDayAsString));

		protected bool EveryMonthNumberDayAsString_ReadOnly => !IsRangeAndNotWeekDay;

		ZString? everyMonthNumberDayAsString;

		#endregion

		#region Month Number as String

		[ReadOnlyMember(nameof(IsRangeAndNotWeekDay))]
		[MaxLength(2)]
		public ZString MonthNumberAsString
		{
			get
			{
				ZString result;
				if (monthNumberAsString.HasValue)
				{
					result = monthNumberAsString.Value;
				}
				else
				{
					result = (MonthNumber.IsEmpty) ? "" : MonthNumber.ToString();
				}
				return result;
			}
			set
			{
				CheckMaximumLength(MonthNumberAsStringInfo, value);
				monthNumberAsString = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateMonthNumberAsString();
				}

				ZByte monthNumber;
				if (!MonthNumberAsStringInfo.HasErrors() && ZByte.TryParse(value, out monthNumber))
				{
					MonthNumber = monthNumber;
				}
				else
				{
					MonthNumber = 0;
				}
				MonthNumberAsStringInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MonthNumberAsStringInfo => GetZPropertyInfo(nameof(MonthNumberAsString));

		ZString? monthNumberAsString;

		#endregion

		#endregion

		#region Options in Ranges

		#region Accounting Day

		[BusinessObjectTestExclude]
		public ZBool AccountingDay
		{
			get => (TaskPeriod == ScheduleRecurrenceType.AccountingPeriod) && (WeekDayOccurrenceNumber == 0);
			set
			{
				if (value)
				{
					WeekDayOccurrenceNumber = 0;
				}
				else
				{
					ResetWeekCountAndDayName();
				}
				AccountingDayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AccountingDayInfo => GetZPropertyInfo(nameof(AccountingDay));

		#endregion

		#region Accounting Week Day

		[BusinessObjectTestExclude]
		public ZBool AccountingWeekDay
		{
			get => !AccountingDay;
			set
			{
				AccountingDay = !value;
				AccountingWeekDayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AccountingWeekDayInfo => GetZPropertyInfo(nameof(AccountingWeekDay));

		#endregion

		#region Monthly Day

		[BusinessObjectTestExclude]
		public ZBool MonthlyDay
		{
			get => (TaskPeriod == ScheduleRecurrenceType.Monthly) && (WeekDayOccurrenceNumber == 0);
			set
			{
				if (value)
				{
					WeekDayOccurrenceNumber = 0;
				}
				else
				{
					ResetWeekCountAndDayName();
				}
				MonthlyDayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MonthlyDayInfo => GetZPropertyInfo(nameof(MonthlyDay));

		#endregion

		#region Monthly Week Day

		[BusinessObjectTestExclude]
		public ZBool MonthlyWeekDay
		{
			get => !MonthlyDay;
			set
			{
				MonthlyDay = !value;
				MonthlyWeekDayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MonthlyWeekDayInfo => GetZPropertyInfo(nameof(MonthlyWeekDay));

		#endregion

		#region Yearly Every

		[BusinessObjectTestExclude]
		public ZBool YearlyEvery
		{
			get => (TaskPeriod == ScheduleRecurrenceType.Yearly) && (WeekDayOccurrenceNumber == 0);
			set
			{
				if (value)
				{
					WeekDayOccurrenceNumber = 0;
				}
				else
				{
					ResetWeekCountAndDayName();
				}
				YearlyEveryInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo YearlyEveryInfo => GetZPropertyInfo(nameof(YearlyEvery));

		#endregion

		void ResetWeekCountAndDayName()
		{
			if (WeekDayOccurrenceNumber == 0)
			{
				WeekCountAsString = "1";
			}
			if (IsLastDay)
			{
				DayName = ZString.Empty;
			}
		}

		#region Yearly Week Day

		[BusinessObjectTestExclude]
		public ZBool YearlyWeekDay
		{
			get => !YearlyEvery;
			set
			{
				YearlyEvery = !value;
				YearlyWeekDayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo YearlyWeekDayInfo => GetZPropertyInfo(nameof(YearlyWeekDay));

		#endregion

		#endregion

		#region Week Count as String

		[ReadOnlyMember(nameof(IsRangeAndNotWeekDay))]
		[MaxLength(1)]
		public ZString WeekCountAsString
		{
			get => weekCountAsString ?? WeekDayOccurrenceNumber.ToString();
			set
			{
				CheckMaximumLength(WeekCountAsStringInfo, value);
				weekCountAsString = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWeekCountAsString();
				}
				ZByte weekDayOccurrenceNumber;
				if (!WeekCountAsStringInfo.HasErrors() && ZByte.TryParse(value, out weekDayOccurrenceNumber))
				{
					WeekDayOccurrenceNumber = weekDayOccurrenceNumber;
				}
				else
				{
					WeekDayOccurrenceNumber = 0;
				}
				WeekCountAsStringInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WeekCountAsStringInfo => GetZPropertyInfo(nameof(WeekCountAsString));

		ZString? weekCountAsString;

		#endregion

		#region Day Name

		[ReadOnlyMember(nameof(IsRangeAndNotWeekDay))]
		[MaxLength(3)]
		public ZString DayName
		{
			get
			{
				ZString result;
				if (dayName.HasValue)
				{
					result = dayName.Value;
				}
				else
				{
					if (DayNumber.IsEmpty || IsLastDay)
					{
						result = ZString.Empty;
					}
					else
					{
						result = Lookups.WeekDays.GetCodeFromDescription(DayNumber.ToString());
					}
				}
				return result;
			}
			set
			{
				CheckMaximumLength(DayNameInfo, value);
				dayName = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDayName();
				}
				ZByte dayNumber;
				if (!DayNameInfo.HasErrors() && ZByte.TryParse(Lookups.WeekDays.GetDescriptionFromCode(value), out dayNumber))
				{
					DayNumber = dayNumber;
				}
				else
				{
					DayNumber = 0;
				}
				DayNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DayNameInfo => GetZPropertyInfo(nameof(DayName));

		ZString? dayName;

		#endregion

		#region Last Day Options

		#region Accounting Last Day

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsRangeAndNotDay))]
		public ZBool AccountingLastDay
		{
			get => (TaskPeriod == ScheduleRecurrenceType.AccountingPeriod) && IsLastDay;
			set
			{
				if (value)
				{
					if (TaskPeriod == ScheduleRecurrenceType.AccountingPeriod)
					{
						DayNumber = 99;
					}
				}
				else
				{
					DayNumber = 0;
				}
				AccountingLastDayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AccountingLastDayInfo => GetZPropertyInfo(nameof(AccountingLastDay));

		#endregion

		#region Month Last Day

		[BusinessObjectTestExclude]
		public ZBool MonthLastDay
		{
			get => (TaskPeriod == ScheduleRecurrenceType.Monthly) && IsLastDay;
			set
			{
				if (TaskPeriod == ScheduleRecurrenceType.Monthly)
				{
					if (value)
					{
						DayNumber = 99;
					}
					else
					{
						DayNumber = 0;
					}
				}
				MonthLastDayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MonthLastDayInfo => GetZPropertyInfo(nameof(MonthLastDay));

		protected bool MonthLastDay_ReadOnly => !IsRangeAndNotWeekDay;

		#endregion

		#endregion

		#region Day of Period

		#region Day of Accounting Period

		[BusinessObjectTestExclude]
		public ZInt DayOfAccountingPeriod
		{
			get
			{
				ZInt accPeriod = Calculator.AccPeriodCalculator.GetPeriodFromDate(StartDateLocal);
				ZDateTime date = Calculator.AccPeriodCalculator.GetFirstDayForPeriod(accPeriod);
				return date.IsValid ? (ZInt)(StartDateLocal - date).TotalDays + 1 : 0;
			}
			set
			{
				ZInt accPeriod = Calculator.AccPeriodCalculator.GetPeriodFromDate(StartDateLocal);
				ZDateTime date = Calculator.AccPeriodCalculator.GetFirstDayForPeriod(accPeriod);
				if (date.IsValid)
				{
					date = date.AddDays(value - 1);
					if (date < StartDateLocal)
					{
						ZInt nextPeriod = Calculator.AccPeriodCalculator.GetNextPeriod(accPeriod);
						if (!nextPeriod.IsEmpty)
						{
							date = Calculator.AccPeriodCalculator.GetFirstDayForPeriod(nextPeriod);
						}
						if (!date.IsValid || nextPeriod.IsEmpty)
						{
							date = StartDateLocal;
						}
						else
						{
							date = date.AddDays(value - 1);
						}
					}
					StartDateLocal = date;
				}
				DayOfAccountingPeriodInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DayOfAccountingPeriodInfo => GetZPropertyInfo(nameof(DayOfAccountingPeriod));

		protected bool DayOfAccountingPeriod_ReadOnly => IsLastDay || AccountingWeekDay;

		#endregion

		#region Day of Month

		[BusinessObjectTestExclude]
		public ZInt DayOfMonth
		{
			get => StartDateLocal.IsValid ? StartDateLocal.Day : 0;
			set
			{
				ZDateTime newStartDate;

				if (TaskPeriod == ScheduleRecurrenceType.Yearly)
				{
					newStartDate = CalculateNextDateLocal(MonthNumber, value);
				}
				else
				{
					newStartDate = Calculator.GetCorrectedDate(StartDateLocal.Year, StartDateLocal.Month, value);
					if (newStartDate < StartDateLocal && newStartDate < ZDateTime.Today)
					{
						newStartDate = newStartDate.AddMonths(1);
					}
				}

				StartDateLocal = newStartDate;
				DayOfMonthInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DayOfMonthInfo => GetZPropertyInfo(nameof(DayOfMonth));

		protected bool DayOfMonth_ReadOnly =>
			(MonthlyRange && (IsLastDay || !MonthlyDay)) ||
			(YearlyRange && (!YearlyEvery || (YearlyEvery && MonthNumber == 0)));

		#endregion

		#endregion

		#region Daily Day

		public ZBool DailyDay
		{
			get => !WeekDaysOnly;
			set
			{
				WeekDaysOnly = !value;
				DailyDayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DailyDayInfo => GetZPropertyInfo(nameof(DailyDay));

		#endregion

		#region Local Run Time UTC Offset

		public static double CurrentBranchUtcOffset =>  (ZDateTime.Now - ZDateTime.UtcNow).TotalHours;

		public ZString LocalRunTimeUtcOffset => string.Format("UTC{0:+#.#;-#.#;#}", CurrentBranchUtcOffset);

		public ZPropertyInfo LocalRunTimeUtcOffsetInfo => GetZPropertyInfo(nameof(LocalRunTimeUtcOffset));

		#endregion

		#region Recurrence Binding Text

		[BusinessObjectTestExclude]
		public ZString RecurringStartTimeLocalText
		{
			get
			{
				return RecurringStartTimeLocal.ToShortTimeString();
			}
		}

		[BusinessObjectTestExclude]
		public ZString CalcDailyStartTimeLocalText
		{
			get
			{
				return scheduleTask.CalcDailyStartTimeLocal.ToShortTimeString();
			}
		}

		[BusinessObjectTestExclude] 
		public ZString CalcDailyEndTimeLocalText
		{
			get
			{
				return scheduleTask.CalcDailyEndTimeLocal.ToShortTimeString();
			}
		}

		#endregion

		#endregion

		#region Calculation Methods

		public ZInt CalculateAccountingPeriodBasedOnDate(ZDateTime localTime)
		{
			return Calculator.AccPeriodCalculator.GetPeriodFromDate(localTime);
		}

		public ZDateTime CalculateLastDayForAccountingPeriod(ZInt accPeriod)
		{
			return Calculator.AccPeriodCalculator.GetLastDayForPeriod(accPeriod);
		}

		public ZDateTime CalculateFirstDayForAccountingPeriod(ZInt accPeriod)
		{
			return Calculator.AccPeriodCalculator.GetFirstDayForPeriod(accPeriod);
		}

		internal ZDateTime CalculateUtcDateFromLocal(int weekNumber, DayOfWeek dayOfWeek, int month, int year)
		{
			var resultLocal = new ZDateTime(year, month, 1);
			var result = GetUtcDate(resultLocal);

			while (resultLocal.DayOfWeek != dayOfWeek)
			{
				result = result.AddDays(1);
				resultLocal = GetLocalDate(result);
			}
			result = result.AddDays(7 * (weekNumber - 1));

			return result;
		}

		ZDateTime CalculateLocalDateFromLocal(int weekNumber, DayOfWeek dayOfWeek, int month, int year)
		{
			var result = new ZDateTime(year, month, 1);

			while (result.DayOfWeek != dayOfWeek)
			{
				result = result.AddDays(1);
			}
			result = result.AddDays(7 * (weekNumber - 1));

			return result;
		}

		internal ZDateTime CalculateUtcDateForAccountingPeriodFromLocal(int weekNumber, DayOfWeek dayOfWeek, int accountingPeriod, ZDateTime defaultUtcDateTime)
		{
			ZDateTime result;
			if (accountingPeriod != 0)
			{
				var localResult = CalculateFirstDayForAccountingPeriod(accountingPeriod);
				result = GetUtcDate(localResult);
				if (result.IsValid)
				{
					while (localResult.DayOfWeek != dayOfWeek)
					{
						result = result.AddDays(1);
						localResult = GetLocalDate(result);
					}
					result = result.AddDays(7 * (weekNumber - 1));
				}
				else
				{
					result = defaultUtcDateTime;
				}
			}
			else
			{
				result = defaultUtcDateTime;
			}
			return result;
		}

		public ZDateTime CalculateLocalDateForAccountingPeriod(int weekNumber, DayOfWeek dayOfWeek, int accountingPeriod, ZDateTime defaultLocalDateTime)
		{
			ZDateTime result;
			if (accountingPeriod != 0)
			{
				result = CalculateFirstDayForAccountingPeriod(accountingPeriod);
				if (result.IsValid)
				{
					while (result.DayOfWeek != dayOfWeek)
					{
						result = result.AddDays(1);
					}
					result = result.AddDays(7 * (weekNumber - 1));
				}
				else
				{
					result = defaultLocalDateTime;
				}
			}
			else
			{
				result = defaultLocalDateTime;
			}
			return result;
		}

		ZDateTime CalculateNewMonthlyUtcDate(ZDateTime previousUtcDate)
		{
			var previousLocalDate = GetLocalDate(previousUtcDate);
			int month = (previousLocalDate.Month == 12) ? 1 : previousLocalDate.Month + 1;
			int year = (previousLocalDate.Month == 12) ? previousLocalDate.Year + 1 : previousLocalDate.Year;
			return CalculateUtcDateFromLocal(WeekDayOccurrenceNumber, GetDayOfWeek(), month, year);
		}

		ZDateTime CalculateNewMonthlyLocalDate(ZDateTime previousLocalDate)
		{
			int month = (previousLocalDate.Month == 12) ? 1 : previousLocalDate.Month + 1;
			int year = (previousLocalDate.Month == 12) ? previousLocalDate.Year + 1 : previousLocalDate.Year;
			return CalculateLocalDateFromLocal(WeekDayOccurrenceNumber, GetDayOfWeek(), month, year);
		}

		ZDateTime CalculateNewMonthlyLocalDate(ZDateTime previousLocalDate, int dayOfMonth, bool isRecurrencePatternChanged)
		{
			var now = ZDateTime.UtcNow;
			var calculationDay = Math.Min(dayOfMonth, DateTime.DaysInMonth(now.Year, now.Month));
			var isInFuture = calculationDay > now.Day;

			var newLocalDate = previousLocalDate.AddMonths(isRecurrencePatternChanged ? isInFuture ? 0 : 1 : TaskPeriodCount);
			var day = Math.Min(dayOfMonth, DateTime.DaysInMonth(newLocalDate.Year, newLocalDate.Month));
			var localResult = new ZDateTime(newLocalDate.Year, newLocalDate.Month, day);
			return localResult;
		}

		internal ZDateTime GetLocalDate(ZDateTime utcDate)
		{
			if (utcDate.IsEmpty || !utcDate.IsValid)
			{
				return utcDate;
			}
			else if (scheduleTask.UtcOffsetOverride.HasValue)
			{
				return utcDate.Add(scheduleTask.UtcOffsetOverride.Value);
			}
			else
			{
				return string.IsNullOrEmpty(Unloco)
					? Env.Time.GetLocalTimeFromUtc(utcDate.ToDateTime())
					: Env.Time.GetUnlocoTimeFromUtc(Unloco, utcDate.ToDateTime());
			}
		}

		internal ZDateTime GetUtcDate(ZDateTime localDate)
		{
			if (localDate.IsEmpty || !localDate.IsValid)
			{
				return localDate;
			}
			else if (scheduleTask.UtcOffsetOverride.HasValue)
			{
				return localDate.Add(-scheduleTask.UtcOffsetOverride.Value);
			}
			else
			{
				return string.IsNullOrEmpty(Unloco)
					? Env.Time.GetUtcFromLocalTime(localDate.ToDateTime())
					: Env.Time.GetUtcFromUnlocoTime(Unloco, localDate.ToDateTime());
			}
		}

		ZDateTime GetLocalDateForUser(ZDateTime utcDate)
		{
			return
				utcDate.IsEmpty || !utcDate.IsValid
					? utcDate
					: Env.Time.GetLocalTimeFromUtc(utcDate.ToDateTime());
		}

		ZDateTime GetUtcDateFromLocalForUser(ZDateTime localDate)
		{
			return
				localDate.IsEmpty || !localDate.IsValid
					? localDate
					: Env.Time.GetUtcFromUnlocoTime(Unloco, localDate.ToDateTime());
		}

		string Unloco => scheduleTask.Branch == null ? null : scheduleTask.Branch.GB_RL_NKHomePort;

		#region Calculate New Start Date

		public ZDateTime CalculateNewStartDate()
		{
			ZDateTime result;

			switch (TaskPeriod)
			{
				case ScheduleRecurrenceType.AccountingPeriod:
					result = CalculateNewAccountingPeriodStartUtcDate();
					break;

				case ScheduleRecurrenceType.Weekly:
					result = CalculateNewWeeklyStartUtcDate();
					break;

				case ScheduleRecurrenceType.Daily:
					result = CalculateNewDailyStartUtcDate();
					break;

				case ScheduleRecurrenceType.Monthly:
					result = CalculateNewMonthlyStartUtcDate();
					break;

				case ScheduleRecurrenceType.Yearly:
					result = CalculateNewYearlyStartUtcDate();
					break;

				case ScheduleRecurrenceType.Hourly:
					result = CalculateNewDailyStartUtcDate();
					break;

				case ScheduleRecurrenceType.Minute:
					result = CalculateNewDailyStartUtcDate();
					break;

				case ScheduleRecurrenceType.Second:
					result = CalculateNewDailyStartUtcDate();
					break;

				default:
					throw new InvalidOperationException("CalculateNewStartDate cannot be called when TaskPeriod is invalid.");
			}

			return result;
		}

		ZDateTime CalculateNewAccountingPeriodStartUtcDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (WeekDayOccurrenceNumber == 0)
			{
				if (IsLastDay)
				{
					ZInt accPeriod = Calculator.AccPeriodCalculator.GetPeriodFromDate(StartDateLocal);
					result = GetUtcDate(Calculator.AccPeriodCalculator.GetLastDayForPeriod(accPeriod));
				}
			}
			else
			{
				DayOfWeek dayOfWeek = GetDayOfWeek();
				ZInt accPeriod = Calculator.AccPeriodCalculator.GetPeriodFromDate(StartDateLocal);
				result = CalculateUtcDateForAccountingPeriodFromLocal(WeekDayOccurrenceNumber, dayOfWeek, accPeriod, scheduleTask.S5_StartDate);
				if (result <= scheduleTask.S5_StartDate)
				{
					accPeriod = Calculator.AccPeriodCalculator.GetNextPeriod(accPeriod);
					result = CalculateUtcDateForAccountingPeriodFromLocal(WeekDayOccurrenceNumber, dayOfWeek, accPeriod, scheduleTask.S5_StartDate);
				}
			}

			if (!result.IsValid)
			{
				result = scheduleTask.S5_StartDate;
			}

			return result;
		}

		ZDateTime CalculateNewWeeklyStartUtcDate()
		{
			var result = scheduleTask.S5_StartDate;
			var resultLocal = GetLocalDate(result);
			while (DayList.SubstringSafe((int)resultLocal.DayOfWeek, 1) != "Y")
			{
				result = result.AddDays(1);
				resultLocal = GetLocalDate(result);
			}
			return result;
		}

		ZDateTime CalculateNewDailyStartUtcDate()
		{
			var result = scheduleTask.S5_StartDate;
			var resultLocal = GetLocalDate(result);
			if (WeekDaysOnly)
			{
				while (resultLocal.DayOfWeek == DayOfWeek.Sunday || resultLocal.DayOfWeek == DayOfWeek.Saturday)
				{
					result = result.AddDays(1);
					resultLocal = GetLocalDate(result);
				}
			}
			return result;
		}

		ZDateTime CalculateNewMonthlyStartUtcDate()
		{
			ZDateTime result;

			if (WeekDayOccurrenceNumber == 0)
			{
				if (IsLastDay)
				{
					var resultLocal = Calculator.GetCorrectedDate(StartDateLocal.Year, StartDateLocal.Month, DateTime.DaysInMonth(StartDateLocal.Year, StartDateLocal.Month));
					result = GetUtcDate(resultLocal.Date.Add(StartDateLocal.TimeOfDay));
				}
				else
				{
					result = scheduleTask.S5_StartDate;
				}
			}
			else
			{
				DayOfWeek dayOfWeek = GetDayOfWeek();
				result = CalculateUtcDateFromLocal(WeekDayOccurrenceNumber, dayOfWeek, StartDateLocal.Month, StartDateLocal.Year);
				result = GetUtcDate(GetLocalDate(result).Date.Add(StartDateLocal.TimeOfDay));
				if (result < scheduleTask.S5_StartDate)
				{
					result = CalculateNewMonthlyUtcDate(scheduleTask.S5_StartDate);
					result = GetUtcDate(GetLocalDate(result).Date.Add(StartDateLocal.TimeOfDay));
				}
			}

			return result;
		}

		ZDateTime CalculateNewYearlyStartUtcDate()
		{
			ZDateTime result;

			var monthNumber = this.GetValidMonthNumber(StartDateLocal.Month);

			if (WeekDayOccurrenceNumber == 0)
			{
				result = GetUtcDate(CalculateNextDateLocal(monthNumber, DayOfMonth));
			}
			else
			{
				DayOfWeek dayOfWeek = GetDayOfWeek();
				result = CalculateUtcDateFromLocal(WeekDayOccurrenceNumber, dayOfWeek, monthNumber, StartDateLocal.Year);
				if (result < scheduleTask.S5_StartDate)
				{
					result = CalculateUtcDateFromLocal(WeekDayOccurrenceNumber, dayOfWeek, monthNumber, StartDateLocal.Year + 1);
				}
			}

			return result;
		}

		ZDateTime CalculateNextDateLocal(int monthNumber, int dayOfMonth)
		{
			var newStartDate = Calculator.GetCorrectedDate(StartDateLocal.Year, monthNumber, dayOfMonth);
			if (newStartDate < StartDateLocal && newStartDate < ZDateTime.Today)
			{
				newStartDate = Calculator.GetCorrectedDate(StartDateLocal.Year + 1, monthNumber, dayOfMonth);
			}

			return newStartDate;
		}

		#endregion

		#region Calculate Next Schedule Date

		public ZDateTime CalculateNextScheduleDate()
		{
			ZDateTime initialTime = scheduleTask.S5_NextScheduledPrintRunTimeUtc;
			if (initialTime.IsEmpty)
			{
				initialTime = ZDateTime.UtcNow;
			}
			return CalculateNextScheduleDate(initialTime);
		}

		public ZDateTime CalculateNextScheduleDate(ZDateTime initialTime, bool isRecurrencePatternChanged = false)
		{
			var localTime = GetLocalDate(initialTime);

			var result = ZDateTime.Empty;

			switch (TaskPeriod)
			{
				case ScheduleRecurrenceType.AccountingPeriod:
					result = CalculateNextAccountingPeriodScheduleUtcDate(localTime, isRecurrencePatternChanged);
					break;

				case ScheduleRecurrenceType.Yearly:
					result = CalculateNextYearlyScheduleUtcDate(localTime, isRecurrencePatternChanged);
					break;

				case ScheduleRecurrenceType.Monthly:
					result = CalculateNextMonthlyScheduleUtcDate(localTime, isRecurrencePatternChanged);
					break;

				case ScheduleRecurrenceType.Weekly:
					result = CalculateNextWeeklyScheduleUtcDate(localTime, isRecurrencePatternChanged);
					break;

				case ScheduleRecurrenceType.Daily:
					result = CalculateNextDailyScheduleUtcDate(localTime, isRecurrencePatternChanged);
					break;

				case ScheduleRecurrenceType.Hourly:
					return CalculateNextScheduleUtcDate(initialTime, new TimeSpan(0, TaskPeriodCount, 0, 0));

				case ScheduleRecurrenceType.Minute:
					return CalculateNextScheduleUtcDate(initialTime, new TimeSpan(0, 0, TaskPeriodCount, 0));

				case ScheduleRecurrenceType.Second:
					return CalculateNextScheduleUtcDate(initialTime, new TimeSpan(0, 0, 0, TaskPeriodCount));

				default:
					throw new InvalidOperationException("CalculateNextScheduleDate cannot be called when TaskPeriod is invalid.");
			}

			if (scheduleTask.S5_DailyStartTime.IsValid) //valid: specific local time of day
			{
				result = GetUtcDate(result.Date.Add(scheduleTask.S5_DailyStartTime.TimeOfDay));
			}
			else //invalid: always local midnight
			{
				result = GetUtcDate(result.Date);
			}

			return result;
		}

		ZDateTime CalculateNextAccountingPeriodScheduleUtcDate(ZDateTime localTime, bool isRecurrencePatternChanged = false)
		{
			var result = ZDateTime.Empty;

			if (WeekDayOccurrenceNumber == 0 || IsLastDay)
			{
				var previousAccPeriod = CalculateAccountingPeriodBasedOnDate(localTime.Date);
				var accPeriod = Calculator.CalculateAccountingPeriod(previousAccPeriod, isRecurrencePatternChanged ? 1 : (int)TaskPeriodCount);
				if (IsLastDay)
				{
					result = CalculateLastDayForAccountingPeriod(accPeriod);
				}
				else
				{
					result = Calculator.AccPeriodCalculator.GetFirstDayForPeriod(previousAccPeriod);
					if (result.IsValid)
					{
						var daySpan = (localTime - result);
						result = Calculator.AccPeriodCalculator.GetFirstDayForPeriod(accPeriod);
						if (result.IsValid)
						{
							result = result.AddDays(isRecurrencePatternChanged ? (DayOfAccountingPeriod - 1) : (int)daySpan.TotalDays);
						}
					}
				}
			}
			else
			{
				var accPeriod = CalculateAccountingPeriodBasedOnDate(localTime.Date);
				if (!accPeriod.IsEmpty)
				{
					accPeriod = Calculator.AccPeriodCalculator.GetNextPeriod(accPeriod);
					if (!accPeriod.IsEmpty)
					{
						result = CalculateLocalDateForAccountingPeriod(WeekDayOccurrenceNumber, GetDayOfWeek(), accPeriod, localTime);
					}
				}
			}

			if (!result.IsValid)
			{
				result = localTime;
			}

			return result;
		}

		ZDateTime CalculateNextWeeklyScheduleUtcDate(ZDateTime localTime, bool isRecurrencePatternChanged = false)
		{
			if (NextScheduledPrintRunTimeLocal.IsValid && NextScheduledPrintRunTimeLocal.DayOfWeek != DayOfWeek.Sunday)
			{
				localTime = localTime.AddDays(1);
			}
			else
			{
				localTime = localTime.AddDays(isRecurrencePatternChanged ? 1 : 7 * (TaskPeriodCount - 1) + 1);
			}

			if (DayList == "NNNNNNN")
			{
				throw new InvalidOperationException("Cannot calculate weekly schedule when no days are selected");
			}
			while (DayList.SubstringSafe((int)localTime.DayOfWeek, 1) != "Y")
			{
				if (localTime.DayOfWeek != DayOfWeek.Sunday)
				{
					localTime = localTime.AddDays(1);
				}
				else
				{
					localTime = localTime.AddDays(isRecurrencePatternChanged ? 1 : 7 * (TaskPeriodCount - 1) + 1);
				}
			}

			return localTime;
		}

		ZDateTime CalculateNextDailyScheduleUtcDate(ZDateTime localTime, bool isRecurrencePatternChanged = false)
		{
			if (WeekDaysOnly)
			{
				localTime = localTime.AddDays(1);

				while (localTime.DayOfWeek == DayOfWeek.Sunday || localTime.DayOfWeek == DayOfWeek.Saturday)
				{
					localTime = localTime.AddDays(1);
				}
			}
			else
			{
				localTime = localTime.AddDays(isRecurrencePatternChanged ? 1 : (int)TaskPeriodCount);
			}

			return localTime;
		}

		ZDateTime CalculateNextMonthlyScheduleUtcDate(ZDateTime localTime, bool isRecurrencePatternChanged = false)
		{
			ZDateTime result;
			if (WeekDayOccurrenceNumber == 0)
			{
				if (IsLastDay)
				{
					result = localTime.AddMonths(isRecurrencePatternChanged ? 1 : (int)TaskPeriodCount);
					result = Calculator.GetCorrectedDate(result.Year, result.Month, DateTime.DaysInMonth(result.Year, result.Month));
				}
				else
				{
					result = CalculateNewMonthlyLocalDate(localTime, DayOfMonth, isRecurrencePatternChanged);
				}
			}
			else
			{
				result = CalculateNewMonthlyLocalDate(localTime);
			}

			return result;
		}

		ZDateTime CalculateNextYearlyScheduleUtcDate(ZDateTime localTime, bool isRecurrencePatternChanged = false)
		{
			ZDateTime result;

			var monthNumber = this.GetValidMonthNumber(localTime.Month);

			if (WeekDayOccurrenceNumber == 0)
			{
				result = Calculator.GetCorrectedDate(localTime.Year, isRecurrencePatternChanged ? MonthNumber : monthNumber, DayOfMonth);
				if (result <= localTime)
				{
					result = Calculator.GetCorrectedDate(localTime.Year + 1, isRecurrencePatternChanged ? MonthNumber : monthNumber, DayOfMonth);
				}
			}
			else
			{
				result = CalculateLocalDateFromLocal(WeekDayOccurrenceNumber, GetDayOfWeek(), isRecurrencePatternChanged ? MonthNumber : monthNumber, localTime.Year + 1);
			}

			return result;
		}

		int GetValidMonthNumber(int currentMonth)
		{
			if (MonthNumber <= 0 || MonthNumber > 12)
			{
				var message = string.Format(
					CultureInfo.InvariantCulture,
					(NoResString)"Month number '{0}' is invalid, falling back to current month '{1}'; Task Period = '{2}', Start Date = '{3}'",
					MonthNumber,
					currentMonth,
					TaskPeriod,
					StartDateLocal);

				ErrorReporter.ReportOnce("StmScheduleTaskRecurrence.GetValidMonthNumber", message);

				return currentMonth;
			}

			return MonthNumber;
		}

		ZDateTime CalculateNextScheduleUtcDate(ZDateTime fromUtcTime, TimeSpan span)
		{
			if (fromUtcTime.IsEmpty || !fromUtcTime.IsValid)
			{
				fromUtcTime = ZDateTime.UtcNow;
			}
			ZDateTime result = AdjustOutOfRangeNextRunTime(fromUtcTime.Add(span));

			/*if (WeekDaysOnly)
			{
				var resultLocal = GetLocalDate(result);
				if (resultLocal.DayOfWeek == DayOfWeek.Saturday || resultLocal.DayOfWeek == DayOfWeek.Sunday)
				{
					//set result to correct utc time of day OR to local midnight
					if (scheduleTask.S5_DailyStartTime.IsValid)
					{
						result = result.Date.Add(scheduleTask.S5_DailyStartTime.TimeOfDay);
					}
					else
					{
						result = GetUtcDate(GetLocalDate(result).Date);
					}
					if (result < fromUtcTime)
					{
						result.AddDays(1);
					}
					resultLocal = GetLocalDate(result);

					while (resultLocal.DayOfWeek == DayOfWeek.Saturday || resultLocal.DayOfWeek == DayOfWeek.Sunday)
					{
						result = result.AddDays(1);
						resultLocal = GetLocalDate(result);
					}
				}
			}*/

			return result;
		}

		ZDateTime AdjustOutOfRangeNextRunTime(ZDateTime dateTimeToAdjust)
		{
			ZDateTime result = dateTimeToAdjust;
			if (scheduleTask.S5_DailyStartTime.IsValid && scheduleTask.S5_DailyEndTime.IsValid)
			{
				TimeSpan localStartTime = scheduleTask.CalcDailyStartTimeLocal.TimeOfDay;
				TimeSpan localEndTime = scheduleTask.CalcDailyEndTimeLocal.TimeOfDay;

				ZDateTime resultLocal = GetLocalDate(result);
				if (localStartTime < localEndTime)
				{
					if (resultLocal.TimeOfDay < localStartTime)
					{
						resultLocal = resultLocal.Date.Add(localStartTime);
					}
					else if (resultLocal.TimeOfDay > localEndTime)
					{
						resultLocal = resultLocal.Date.AddDays(1).Add(localStartTime);
					}
				}
				else if (localStartTime > localEndTime)
				{
					if (resultLocal.TimeOfDay > localEndTime && resultLocal.TimeOfDay < localStartTime)
					{
						resultLocal = resultLocal.Date.Add(localStartTime);
					}
				}
				result = GetUtcDate(resultLocal);
			}

			return result;
		}

		#endregion

		#endregion

		#region Lookups

		public StmScheduleTaskRecurrenceLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new StmScheduleTaskRecurrenceLookups(this);
				}
				return lookups;
			}
		}

		StmScheduleTaskRecurrenceLookups lookups;

		#endregion

		#region Validation

		public StmScheduleTaskRecurrenceValidation Validation => new StmScheduleTaskRecurrenceValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#endregion
	}
}
