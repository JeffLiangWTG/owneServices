using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Security;

namespace Enterprise.Accounting.GUI.InvoicingApproval
{
	class ARCreditNoteForAmendingApprovalGUIProvider : TransactionFormApprovalGUIProvider<InvoicingBase, ARCreditNoteApprovalBulk, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		protected override ARCreditNoteApprovalBulk GetNewApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, ARCreditNoteApprovalRequest approvalRequest)
		{
			return new ARCreditNoteApprovalBulk(factory, interactiveSecurityOverrideProvider, approvalRequest);
		}

		protected override TransactionApprovalBulkForm<InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails> GetApprovalFormToSetDescription(ARCreditNoteApprovalBulk approvalBulk, TransactionApprovalFormModes actionMode)
		{
			return new ARCreditNoteApprovalBulkForm(approvalBulk, actionMode);
		}

		protected override ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderCore(bool showApprovalRequestButton, bool alwaysCreateApprovalRequest, bool supportMultipleApprover = false)
		{
			return new InvoicingSecurityOverrideProvider(showApprovalRequestButton, alwaysCreateApprovalRequest, supportMultipleApprover: supportMultipleApprover);
		}

		protected override ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderCoreForARCreditNote(bool showApprovalRequestButton, bool alwaysCreateApprovalRequest, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] approvalRequests = null)
		{
			return new InvoicingSecurityOverrideProvider(showApprovalRequestButton, alwaysCreateApprovalRequest, supportMultipleApprover: supportMultipleApprover, aRCreditNoteApprovalRequests: approvalRequests);
		}
	}
}
