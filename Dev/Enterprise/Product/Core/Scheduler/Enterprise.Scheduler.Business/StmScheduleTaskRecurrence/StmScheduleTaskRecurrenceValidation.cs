using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskRecurrenceValidation : ZValidation
	{
		public StmScheduleTaskRecurrenceValidation(StmScheduleTaskRecurrence parent)
			: base(parent)
		{
		}

		StmScheduleTaskRecurrence Parent => (StmScheduleTaskRecurrence)base.ParentFilter;

		public override Type AutoValidationType => null;

		public override void ValidateAll()
		{
			ValidateDayName();
			ValidateEveryMonthNumberDayAsString();
			ValidateMonthNumberAsString();
			ValidateStartDateLocalForUser();
			ValidateWeekCountAsString();
			ValidateWeeklyDayInfos();

			Parent.RefreshBinding();
		}

		bool IsRangeAndWeekDay =>
			Parent.MonthlyRange && Parent.MonthlyWeekDay ||
			Parent.AccountingRange && Parent.AccountingWeekDay ||
			Parent.YearlyRange && Parent.YearlyWeekDay;

		bool IsWeeklyAndDayListIsEmpty
		{
			get
			{
				return Parent.WeeklyRange && Parent.WeekDays.All(selected => !selected);
			}
		}

		#region Day Name

		public void ValidateDayName()
		{
			ValidateCalculatedProperty(Parent.DayNameInfo);
		}

		protected void CheckDayName()
		{
			if (IsRangeAndWeekDay)
			{
				MandatoryValidation.CheckEntered(Parent.DayNameInfo);
				ListValidation.ErrorIfInvalidCode(Parent.DayNameInfo, Parent.Lookups.WeekDays);
			}
		}

		#endregion

		#region Every Month Number Day as String

		public void ValidateEveryMonthNumberDayAsString()
		{
			ValidateCalculatedProperty(Parent.EveryMonthNumberDayAsStringInfo);
		}

		protected void CheckEveryMonthNumberDayAsString()
		{
			if (Parent.YearlyRange)
			{
				MandatoryValidation.CheckEntered(Parent.EveryMonthNumberDayAsStringInfo);
				ListValidation.ErrorIfInvalidCode(Parent.EveryMonthNumberDayAsStringInfo, Parent.Lookups.Months);
			}
		}

		#endregion

		#region Month Number as String

		public void ValidateMonthNumberAsString()
		{
			ValidateCalculatedProperty(Parent.MonthNumberAsStringInfo);
		}

		protected void CheckMonthNumberAsString()
		{
			if (Parent.YearlyRange && Parent.YearlyWeekDay)
			{
				MandatoryValidation.CheckEntered(Parent.MonthNumberAsStringInfo);
				ListValidation.ErrorIfInvalidCode(Parent.MonthNumberAsStringInfo, Parent.Lookups.Months);
			}
		}

		#endregion

		#region Start Date

		public void ValidateStartDateLocalForUser()
		{
			ValidateCalculatedProperty(Parent.StartDateLocalForUserInfo);
		}

		protected void CheckStartDateLocalForUser()
		{
			MandatoryValidation.CheckEntered(Parent.StartDateLocalForUserInfo);
			if (!Parent.StartDateLocalForUserInfo.HasErrors())
			{
				if (Parent.AccountingRange)
				{
					AccountingPeriodCalculator calculator = new AccountingPeriodCalculator(Parent.Factory);
					if (calculator.GetPeriodFromDate(Parent.StartDateLocal) == 0) //intentional - the actual local date that will be used in the schedule task is local to the scheduled task, but the local date presented to the user is StartDateLocalForUser, in their current branch time zone
					{
						Parent.StartDateLocalForUserInfo.AddWarning(Res.GetString("993f356c-d6f4-44d3-a99a-88635867b75e", "An Accounting Period does not exist for the entered Start Date."));
					}
				}
			}
		}

		#endregion

		#region Week Count as String

		public void ValidateWeekCountAsString()
		{
			ValidateCalculatedProperty(Parent.WeekCountAsStringInfo);
		}

		protected void CheckWeekCountAsString()
		{
			if (IsRangeAndWeekDay)
			{
				MandatoryValidation.CheckEntered(Parent.WeekCountAsStringInfo);
				ListValidation.ErrorIfInvalidCode(Parent.WeekCountAsStringInfo, Parent.Lookups.WeekCounts);
			}
		}

		#endregion

		#region Weekly Day Infos 

		public void ValidateWeeklyDayInfos()
		{
			ValidateCalculatedProperty(Parent.SundayInfo);
			ValidateCalculatedProperty(Parent.MondayInfo);
			ValidateCalculatedProperty(Parent.TuesdayInfo);
			ValidateCalculatedProperty(Parent.WednesdayInfo);
			ValidateCalculatedProperty(Parent.ThursdayInfo);
			ValidateCalculatedProperty(Parent.FridayInfo);
			ValidateCalculatedProperty(Parent.SaturdayInfo);
		}

		protected void CheckMonday()
		{
			if (IsWeeklyAndDayListIsEmpty)
			{
				Parent.MondayInfo.AddError(Res.GetString("09c6f3c8-79c6-4dd4-86f5-a89a52ca3fe3", "Please select at least one day."));
			}
		}

		protected void CheckTuesday()
		{
			if (IsWeeklyAndDayListIsEmpty)
			{
				Parent.TuesdayInfo.AddError(Res.GetString("09c6f3c8-79c6-4dd4-86f5-a89a52ca3fe3", "Please select at least one day."));
			}
		}

		protected void CheckWednesday()
		{
			if (IsWeeklyAndDayListIsEmpty)
			{
				Parent.WednesdayInfo.AddError(Res.GetString("09c6f3c8-79c6-4dd4-86f5-a89a52ca3fe3", "Please select at least one day."));
			}
		}

		protected void CheckThursday()
		{
			if (IsWeeklyAndDayListIsEmpty)
			{
				Parent.ThursdayInfo.AddError(Res.GetString("09c6f3c8-79c6-4dd4-86f5-a89a52ca3fe3", "Please select at least one day."));
			}
		}

		protected void CheckFriday()
		{
			if (IsWeeklyAndDayListIsEmpty)
			{
				Parent.FridayInfo.AddError(Res.GetString("09c6f3c8-79c6-4dd4-86f5-a89a52ca3fe3", "Please select at least one day."));
			}
		}

		protected void CheckSaturday()
		{
			if (IsWeeklyAndDayListIsEmpty)
			{
				Parent.SaturdayInfo.AddError(Res.GetString("09c6f3c8-79c6-4dd4-86f5-a89a52ca3fe3", "Please select at least one day."));
			}
		}

		protected void CheckSunday()
		{
			if (IsWeeklyAndDayListIsEmpty)
			{
				Parent.SundayInfo.AddError(Res.GetString("09c6f3c8-79c6-4dd4-86f5-a89a52ca3fe3", "Please select at least one day."));
			}
		}

		#endregion
	}
}
