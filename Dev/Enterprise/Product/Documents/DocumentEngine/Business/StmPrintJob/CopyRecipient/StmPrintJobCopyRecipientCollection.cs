using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmPrintJobCopyRecipientCollection : CopyRecipientCollection<StmPrintJobCopyRecipient, StmPrintJob>
	{
		public StmPrintJobCopyRecipientCollection(StmPrintJob printJob, string type) : base(printJob, type, StmPrintJobCopyRecipientSchema.SPR_EmailAddress, StmPrintJobCopyRecipientSchema.SPR_RecipientType)
		{
		}

		protected override void SetDefaultsForNewElementCore(StmPrintJobCopyRecipient newElement)
		{
			newElement.SPR_SP = Owner.PK;
		}
	}
}
