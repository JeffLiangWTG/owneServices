using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ScheduleLookups : ZLookups
	{
		public ScheduleLookups(Schedule parent)
			: base(parent)
		{
		}

		public PeriodScopeList PeriodScopes
		{
			get
			{
				if (periodScopes == null)
				{
					periodScopes = new PeriodScopeList();
				}
				return periodScopes;
			}
		}

		PeriodScopeList periodScopes;
	}
}
