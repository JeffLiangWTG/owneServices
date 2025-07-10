using System;
using CargoWise.Types;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Business
{
	public class StmServiceTaskAdapterValidation : AutoStmServiceTaskAdapterValidation
	{
		public StmServiceTaskAdapterValidation(AutoStmServiceTaskAdapter parent) : base(parent)
		{ }

		protected new StmServiceTaskAdapter Parent => (StmServiceTaskAdapter)base.Parent;

		protected override void CheckPeriod()
		{
			if (PeriodIsNonPositive(Parent.TaskNextRunTimeCalculator))
			{
				Parent.PeriodInfo.AddError(Res.GetString("567C1692-C313-4291-8F76-EF6CD2324EFE", "Period cannot be zero or negative."));
				return;
			}

			try
			{
				var maxValue = GetMaxDuration(Parent.TaskNextRunTimeCalculator);

				if (PeriodExceedsMaxDuration(Parent.TaskNextRunTimeCalculator))
				{
					Parent.PeriodInfo.AddError(Res.GetString("12A97B93-5B3C-4A97-8F95-4787AA1D7875", "Period Count is out of bounds for associated TimeSpan. Bounds are (0, {0}).", maxValue / NextRunTimeRecordCollection.NextRunTimeEstimatesLimit));
				}
				else
				{
					ValidateFrequency();
				}
			}
			catch (Exception ex) when (ex is NotSupportedException && ex.Message.Equals(Parent.TaskNextRunTimeCalculator.GetType().Name))
			{
				Parent.PeriodInfo.AddError(Res.GetString("8AA69CD6-45BA-41AF-B4F0-E07D921E6931", "Schedule calculator {0} is not recognised.", Parent.TaskNextRunTimeCalculator.GetType().Name));
			}
		}

		bool PeriodExceedsMaxDuration(INextRunTimeCalculator calculator) => calculator switch
		{
			NextRunTimeCalculatorSeconds calculatorSeconds => calculatorSeconds.Period > TimeSpan.MaxValue.TotalSeconds / NextRunTimeRecordCollection.NextRunTimeEstimatesLimit,
			NextRunTimeCalculatorMinutes calculatorMinutes => calculatorMinutes.Period > TimeSpan.MaxValue.TotalMinutes / NextRunTimeRecordCollection.NextRunTimeEstimatesLimit,
			NextRunTimeCalculatorHours calculatorHours => calculatorHours.Period > TimeSpan.MaxValue.TotalHours / NextRunTimeRecordCollection.NextRunTimeEstimatesLimit,
			NextRunTimeCalculatorDays calculatorDays => calculatorDays.Period > TimeSpan.MaxValue.TotalDays / NextRunTimeRecordCollection.NextRunTimeEstimatesLimit,
			NextRunTimeCalculatorWeeks calculatorWeeks => calculatorWeeks.Period > TimeSpan.MaxValue.TotalDays / (7 * NextRunTimeRecordCollection.NextRunTimeEstimatesLimit),
			NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate => calculatorMonthsByDate.Period > TimeSpan.MaxValue.TotalDays / (28 * NextRunTimeRecordCollection.NextRunTimeEstimatesLimit),
			NextRunTimeCalculatorMonthsByLastDay calculatorMonthsByLastDay => calculatorMonthsByLastDay.Period > TimeSpan.MaxValue.TotalDays / (28 * NextRunTimeRecordCollection.NextRunTimeEstimatesLimit),
			_ => false
		};

		double GetMaxDuration(INextRunTimeCalculator calculator) => calculator switch
		{
			NextRunTimeCalculatorSeconds => TimeSpan.MaxValue.TotalSeconds,
			NextRunTimeCalculatorMinutes => TimeSpan.MaxValue.TotalMinutes,
			NextRunTimeCalculatorHours => TimeSpan.MaxValue.TotalHours,
			NextRunTimeCalculatorDays or NextRunTimeCalculatorWorkingDays => TimeSpan.MaxValue.TotalDays,
			NextRunTimeCalculatorWeeks => TimeSpan.MaxValue.TotalDays / 7,
			NextRunTimeCalculatorMonthsByDate or NextRunTimeCalculatorMonthsByDayOfWeek or NextRunTimeCalculatorMonthsByLastDay => TimeSpan.MaxValue.TotalDays / 28,
			NextRunTimeCalculatorYearsByDate or NextRunTimeCalculatorYearsByDayOfMonth => TimeSpan.MaxValue.TotalDays / 365,
			_ => throw new NotSupportedException(calculator.GetType().Name)
		};

		public void ValidateDaysOfWeek()
		{
			ValidateCalculatedProperty(Parent.MondayInfo);
			ValidateCalculatedProperty(Parent.TuesdayInfo);
			ValidateCalculatedProperty(Parent.WednesdayInfo);
			ValidateCalculatedProperty(Parent.ThursdayInfo);
			ValidateCalculatedProperty(Parent.FridayInfo);
			ValidateCalculatedProperty(Parent.SaturdayInfo);
			ValidateCalculatedProperty(Parent.SundayInfo);
		}

		protected override void CheckMonday()
		{
			if (IsWeeksCalculatorAndDaysListIsEmpty())
			{
				Parent.MondayInfo.AddError(Res.GetString("7739097B-04A1-493F-A729-A6D975790C8F", "Please select at least one day."));
			}
		}

		protected override void CheckTuesday()
		{
			if (IsWeeksCalculatorAndDaysListIsEmpty())
			{
				Parent.TuesdayInfo.AddError(Res.GetString("7739097B-04A1-493F-A729-A6D975790C8F", "Please select at least one day."));
			}
		}

		protected override void CheckWednesday()
		{
			if (IsWeeksCalculatorAndDaysListIsEmpty())
			{
				Parent.WednesdayInfo.AddError(Res.GetString("7739097B-04A1-493F-A729-A6D975790C8F", "Please select at least one day."));
			}
		}

		protected override void CheckThursday()
		{
			if (IsWeeksCalculatorAndDaysListIsEmpty())
			{
				Parent.ThursdayInfo.AddError(Res.GetString("7739097B-04A1-493F-A729-A6D975790C8F", "Please select at least one day."));
			}
		}

		protected override void CheckFriday()
		{
			if (IsWeeksCalculatorAndDaysListIsEmpty())
			{
				Parent.FridayInfo.AddError(Res.GetString("7739097B-04A1-493F-A729-A6D975790C8F", "Please select at least one day."));
			}
		}

		protected override void CheckSaturday()
		{
			if (IsWeeksCalculatorAndDaysListIsEmpty())
			{
				Parent.SaturdayInfo.AddError(Res.GetString("7739097B-04A1-493F-A729-A6D975790C8F", "Please select at least one day."));
			}
		}

		protected override void CheckSunday()
		{
			if (IsWeeksCalculatorAndDaysListIsEmpty())
			{
				Parent.SundayInfo.AddError(Res.GetString("7739097B-04A1-493F-A729-A6D975790C8F", "Please select at least one day."));
			}
		}

		bool IsWeeksCalculatorAndDaysListIsEmpty()
		{
			if (Parent.TaskNextRunTimeCalculator is NextRunTimeCalculatorWeeks calculatorWeeks)
			{
				return calculatorWeeks.DaysOfOccurrence.Length == 0;
			}

			return false;
		}

		protected override void CheckDayOfMonth()
		{
			if (Parent.TaskNextRunTimeCalculator is NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate)
			{
				ValidateDayOfMonth(calculatorMonthsByDate.DayOfOccurrence);
			}
		}

		protected override void CheckYearlyDay()
		{
			if (Parent.TaskNextRunTimeCalculator is NextRunTimeCalculatorYearsByDate calculatorYearsByDate)
			{
				ValidateYearsDayOfMonth(calculatorYearsByDate.Day, calculatorYearsByDate.Month);
			}
		}

		void ValidateYearsDayOfMonth(int dayOfMonth, int month)
		{
			var daysInMonth = 28;

			if (month > 0 && month <= 12)
			{
				daysInMonth = DateTime.DaysInMonth(BaseNonLeapYear, month);
			}

			if (dayOfMonth > daysInMonth)
			{
				Parent.YearlyDayInfo.AddError(Res.GetString("9C8E936B-7BA0-4528-92F5-9D3FF2EA966C", "Please select a valid day for the chosen month."));
			}
		}

		void ValidateDayOfMonth(int dayOfMonth)
		{
			if (dayOfMonth > 28)
			{
				Parent.DayOfMonthInfo.AddError(Res.GetString("E3125A44-3358-4DD6-B022-0C38A5FD28D0", "Please enter a number from 1 to 28 for Day of the Month."));
			}
		}

		bool PeriodIsNonPositive(INextRunTimeCalculator calculator)
		{
			var period = calculator switch
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

			return period <= 0;
		}

		const int BaseNonLeapYear = 2023;

		public void ValidateFrequency()
		{
			if (!Parent.TaskIsNudgeable)
			{
				var attributes = Parent.TaskAttributes;
				var minimumPeriod = ServiceTaskScheduleHelper.GetPeriodDuration(attributes.MinimumPeriod, TimeSpan.MinValue, isRandomPeriod: false);
				var maximumPeriod = ServiceTaskScheduleHelper.GetPeriodDuration(attributes.MaximumPeriod, TimeSpan.MaxValue, isRandomPeriod: false);

				if (minimumPeriod == maximumPeriod)
				{
					if (Parent.TaskScheduleDuration > maximumPeriod)
					{
						Parent.PeriodInfo.AddError(GetExactFrequencyMessage(attributes.MaximumPeriod));
					}
					else if (Parent.TaskScheduleDuration < minimumPeriod)
					{
						Parent.PeriodInfo.AddError(GetExactFrequencyMessage(attributes.MinimumPeriod));
					}
				}
				else if (Parent.TaskScheduleDuration < minimumPeriod)
				{
					Parent.PeriodInfo.AddError(GetMaximumFrequencyMessage(attributes.MinimumPeriod));
				}
				else if (Parent.TaskScheduleDuration > maximumPeriod)
				{
					Parent.PeriodInfo.AddError(GetMinimumFrequencyMessage(attributes.MaximumPeriod));
				}
			}
		}

		static string GetExactFrequencyMessage(string period)
		{
			return Res.GetString("529D049F-55C7-49D5-BB94-F74B3759B038", "The task should be run {0}", ToFrequencyString(period));
		}

		static string GetMaximumFrequencyMessage(string period)
		{
			return Res.GetString("E78E478F-A93F-47B8-91A1-CA1F3D6C6CFD", "The task schedule shouldn't be more frequent than {0}", ToFrequencyString(period));
		}

		static string GetMinimumFrequencyMessage(string period)
		{
			return Res.GetString("8DE71664-EE63-402B-80A7-1180130C8284", "The task should be run at least {0}", ToFrequencyString(period));
		}

		static string ToFrequencyString(string s)
		{
			int value;
			string recurrence;
			if (ServiceTaskScheduleHelper.ParseFrequency(s, out value, out recurrence))
			{
				switch (recurrence)
				{
					case ScheduleRecurrenceType.Second:
						return Res.GetString("0788A3BF-1F4C-4364-BD80-B5AD4F075BC2", "every {0} second{1}", value, value > 1 ? "s" : "");

					case ScheduleRecurrenceType.Minute:
						return Res.GetString("CC99051A-391B-4D50-BE48-0F7A990FB24A", "every {0} minute{1}", value, value > 1 ? "s" : "");

					case ScheduleRecurrenceType.Hourly:
						return Res.GetString("022111C2-2B73-47EF-98B6-1C99912A73B2", "every {0} hour{1}", value, value > 1 ? "s" : "");

					case ScheduleRecurrenceType.Daily:
						return Res.GetString("C0935C6C-0A4E-4C93-A9F3-5C0665B89489", "every {0} day{1}", value, value > 1 ? "s" : "");

					case ScheduleRecurrenceType.Weekly:
						return Res.GetString("FB46C965-71E9-46E4-83C2-003877BC750D", "every {0} week{1}", value, value > 1 ? "s" : "");

					case ScheduleRecurrenceType.Monthly:
						return Res.GetString("A4A573A3-D05B-4958-8287-463F0BA07F28", "every {0} month{1}", value, value > 1 ? "s" : "");
				}
			}

			return "";
		}

		protected override void CheckCalcDailyStartTimeUtc()
		{
			if (!ValidateStartAndEndTime())
			{
				Parent.CalcDailyStartTimeUtcInfo.AddError(Res.GetString("EBC2B8A5-1587-4A3F-95FE-474FA15B09FE", "You must specify either both a start time and an end time, or neither."));
			}
		}

		protected override void CheckCalcDailyStartTimeUtcIsValidZDateTimeRange()
		{
		}

		protected override void CheckCalcDailyEndTimeUtcIsValidZDateTime()
		{
		}

		protected override void CheckCalcDailyEndTimeUtc()
		{
			if (!ValidateStartAndEndTime())
			{
				Parent.CalcDailyEndTimeUtcInfo.AddError(Res.GetString("EBC2B8A5-1587-4A3F-95FE-474FA15B09FE", "You must specify either both a start time and an end time, or neither."));
			}
		}

		protected override void CheckCalcDailyEndTimeUtcIsValidZDateTimeRange()
		{
		}

		protected override void CheckCalcDailyStartTimeUtcIsValidZDateTime()
		{
		}

		bool ValidateStartAndEndTime()
		{
			if (Parent.CalcDailyStartTimeUtc != ZDateTime.Empty && Parent.CalcDailyEndTimeUtc != ZDateTime.Empty)
			{
				return true;
			}

			if (Parent.CalcDailyStartTimeUtc == ZDateTime.Empty && Parent.CalcDailyEndTimeUtc == ZDateTime.Empty)
			{
				return true;
			}

			return false;
		}
	}
}
