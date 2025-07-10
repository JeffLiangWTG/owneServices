using CargoWise.EntityFramework;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskRecipientDependentCollection : DependentBusinessObjectCollection<StmScheduleTaskRecipient, StmScheduleTask>
	{
		public StmScheduleTaskRecipientDependentCollection(StmScheduleTask scheduleTask)
			: base(scheduleTask)
		{
		}
	}
}
