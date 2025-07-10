using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskCopyRecipientCollection : CopyRecipientCollection<StmScheduleTaskCopyRecipient, StmScheduleTaskRecipient>
	{
		public StmScheduleTaskCopyRecipientCollection(StmScheduleTaskRecipient scheduleTaskRecipient, string type) : base(scheduleTaskRecipient, type, StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress, StmScheduleTaskCopyRecipientSchema.SCR_RecipientType)
		{
		}

		protected override void SetDefaultsForNewElementCore(StmScheduleTaskCopyRecipient newElement)
		{
			newElement.SCR_S6 = Owner.PK;
		}
	}
}
