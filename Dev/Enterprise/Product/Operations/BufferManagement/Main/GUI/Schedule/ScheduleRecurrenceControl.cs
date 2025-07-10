using Enterprise.Scheduler.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class ScheduleRecurrenceControl : RecurrenceControl
	{
		protected override StmScheduleTaskRecurrence Recurrence => ((StmScheduleTask)CurrentDataItem)?.Recurrence;
	}
}
