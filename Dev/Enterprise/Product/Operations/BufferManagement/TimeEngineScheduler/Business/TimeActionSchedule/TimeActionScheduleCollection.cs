using CargoWise.EntityFramework;

namespace Enterprise.TimeEngineScheduler.Business
{
	public class TimeActionScheduleCollection : ActiveBusinessObjectCollection<TimeActionSchedule>
	{
		public TimeActionScheduleCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public TimeActionScheduleCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}
	}
}
