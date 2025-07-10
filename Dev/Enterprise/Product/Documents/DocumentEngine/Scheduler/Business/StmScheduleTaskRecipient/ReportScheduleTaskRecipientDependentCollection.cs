using Enterprise.Scheduler.Business;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ReportScheduleTaskRecipientDependentCollection : StmScheduleTaskRecipientDependentCollection
	{
		public ReportScheduleTaskRecipientDependentCollection(ReportScheduleTask scheduleTask)
			: base(scheduleTask)
		{
			RptScheduleTask = scheduleTask;
		}

		public new ReportScheduleTaskRecipient this[int i]
		{
			get { return (ReportScheduleTaskRecipient)base[i]; }
		}

		public new ReportScheduleTaskRecipient AddNew()
		{
			return (ReportScheduleTaskRecipient)base.AddNew();
		}

		public ReportScheduleTask RptScheduleTask { get; }
	}
}
