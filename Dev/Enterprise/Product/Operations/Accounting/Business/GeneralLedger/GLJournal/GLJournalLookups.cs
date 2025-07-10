using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class GLJournalLookups : TransactionHeaderLookups
	{
		public GLJournalLookups(GLJournal parent)
			: base(parent)
		{
		}

		protected new GLJournal Parent
		{
			get { return (GLJournal)base.Parent; }
		}

		public CodeDescriptionPairList ApprovalStatusList
		{
			get
			{
				var request = Parent.GetLatestLinkedApprovalRequestInDB();
				return request != null ? request.Lookups.ApprovalStatusList : new CodeDescriptionPairList();
			}
		}
	}
}
