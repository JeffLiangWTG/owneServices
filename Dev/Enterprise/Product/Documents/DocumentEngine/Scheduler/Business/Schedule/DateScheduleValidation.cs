using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class DateScheduleValidation : ScheduleValidation
	{
		public DateScheduleValidation(DateSchedule parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDayName();
		}

		protected new DateSchedule Parent
		{
			get { return (DateSchedule)base.Parent; }
		}

		#region Day Name

		public void ValidateDayName()
		{
			ValidateCalculatedProperty(Parent.DayNameInfo);
		}

		protected void CheckDayName()
		{
			if (Parent.ByWeek)
			{
				MandatoryValidation.CheckEntered(Parent.DayNameInfo);
				ListValidation.ErrorIfInvalidCode(Parent.DayNameInfo, Parent.Lookups.WeekDays);
			}
		}

		protected override void CheckPeriodScope()
		{
			MandatoryValidation.CheckEntered(Parent.PeriodScopeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PeriodScopeInfo, Parent.ByHourAndMinute ? Parent.Lookups.HourMinutePeriodScopes : Parent.Lookups.PeriodScopes);
		}

		#endregion
	}
}
