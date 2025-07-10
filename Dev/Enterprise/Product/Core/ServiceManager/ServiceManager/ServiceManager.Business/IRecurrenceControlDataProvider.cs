using Enterprise.Scheduler.Business;

namespace Enterprise.ServiceManager.Business
{
	public interface IRecurrenceControlDataProvider
	{
		StmScheduleTaskRecurrence Recurrence { get; }
		bool IsNudgeable { get; }
		bool IsScheduleReadOnly { get; }
	}
}
