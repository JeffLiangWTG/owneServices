using Enterprise.Scheduler.Business;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class DateScheduleLookups : ScheduleLookups
	{
		public DateScheduleLookups(DateSchedule parent)
			: base(parent)
		{
		}

		#region Week Days

		public WeekDayList WeekDays
		{
			get
			{
				if (weekDays == null)
				{
					weekDays = new WeekDayList();
				}
				return weekDays;
			}
		}

		WeekDayList weekDays;

		#endregion

		#region Hour/Minute Period Scopes

		public PeriodScopeList HourMinutePeriodScopes
		{
			get
			{
				if (hourMinutePeriodScopes == null)
				{
					hourMinutePeriodScopes = new PeriodScopeList(true);
				}

				return hourMinutePeriodScopes;
			}
		}

		PeriodScopeList hourMinutePeriodScopes;

		#endregion
	}
}
