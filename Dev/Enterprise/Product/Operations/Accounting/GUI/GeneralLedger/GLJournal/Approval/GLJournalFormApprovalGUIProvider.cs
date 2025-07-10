using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Security;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	class GLJournalFormApprovalGUIProvider : TransactionFormApprovalGUIProvider<GLJournal, GLJournalApprovalBulk, GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		protected override GLJournalApprovalBulk GetNewApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, GLJournalApprovalRequest approvalRequest)
		{
			return new GLJournalApprovalBulk(factory, interactiveSecurityOverrideProvider, approvalRequest);
		}

		protected override TransactionApprovalBulkForm<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails> GetApprovalFormToSetDescription(GLJournalApprovalBulk approvalBulk, TransactionApprovalFormModes actionMode)
		{
			return new GLJournalApprovalBulkForm(approvalBulk, actionMode);
		}

		protected override ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderCore(bool showApprovalRequestButton, bool alwaysCreateApprovalRequest, bool supportMultipleApprover = false)
		{
			return new GLJournalSecurityOverrideProvider(showApprovalRequestButton, alwaysCreateApprovalRequest);
		}

		protected override ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderCoreForARCreditNote(bool showApprovalRequestButton, bool alwaysCreateApprovalRequest, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] approvalRequests = null)
		{
			throw new System.NotImplementedException();
		}
	}
}
