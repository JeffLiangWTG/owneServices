using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class MultiCompaniesGLJournalLevelAuthorizationWithApprovalRequest : GLJournalLevelAuthorizationWithApprovalRequest
	{
		public MultiCompaniesGLJournalLevelAuthorizationWithApprovalRequest(IPostingTransactionApprovalGUIProvider postingGUIProvider, GLJournal journal, bool alwaysCreateApprovalRequest)
			: base(postingGUIProvider, journal, alwaysCreateApprovalRequest)
		{
		}

		protected override GLJournalApprovalRequest CreateNewApprovalRequest(BusinessObjectFactory factory)
		{
			fLastApprovalRequest = base.CreateNewApprovalRequest(factory);
			fLastApprovalRequest.XP_ReasonDescription = journal.AH_Desc;
			return fLastApprovalRequest;
		}

		public GLJournalApprovalRequest LastApprovalRequest
		{
			get { return fLastApprovalRequest; }
		}

		GLJournalApprovalRequest fLastApprovalRequest;

#if DEBUG
		protected override void SetSecurityProvider_ForTestOnly(GLJournal maxTransaction)
		{
		}
#endif
	}
}
