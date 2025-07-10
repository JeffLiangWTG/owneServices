using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class GLJournalApprovalRequestCollection : TransactionApprovalRequestCollection<GLJournalApprovalRequest>
	{
		public GLJournalApprovalRequestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GLJournalApprovalRequestCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override string[] GetApprovalType()
		{
			return new[] { Constants.GenApprovalRequestApprovalType.GLJournal };
		}
	}
}
