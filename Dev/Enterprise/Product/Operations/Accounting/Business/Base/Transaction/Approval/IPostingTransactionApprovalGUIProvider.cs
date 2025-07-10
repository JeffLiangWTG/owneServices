using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public interface IPostingTransactionApprovalGUIProvider
	{
		bool IsBulkPosting { get; }

		ZDialogResult ShowMessage(string messageText, string messageCaption, ZMessageBoxButtons messageBoxButtons, ZMessageBoxIcon messageBoxIcon, ZDialogResult dialogResult);

		ZDialogResult ShowApprovalFormToSetDescription(GenApprovalRequest approvingRequest);

		void NotifyBulkPostingIsNotAuthorized(string message);

		BusinessObjectFactory FactoryForApprovalRequests { get; }
		void ResetFactoryForApprovalRequests();

		void RollbackPosting();

		ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProvider(bool showApprovalRequestButton, bool supportMultipleApprover = false);

		ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderForARCreditNote(bool showApprovalRequestButton, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] approvalRequests = null);
		Tuple<ZGuid, ZString> GetParentIdAndTableCodeForJobPostingAction();

		JobInvoicingPostingOption PostingOption { get; }

		Tuple<ZString, ZString> ShowCreditNoteReversalReasonForm(string existingReasonCode);

#if DEBUG
		bool ShowLoginFormForTest { get; }
		string SecurityItemForTest { get; }
#endif
	}

	public interface IPostingJobAndTransactionApprovalGUIProvider : IPostingTransactionApprovalGUIProvider
	{
		ISecurityOverrideProvider GetNewSecurityOverrideProviderForPosting(InvoicingBase transaction);
		string PostRequest(APInvoiceChargesApprovalRequest request);
		IApprovalRequestPostingResult PostRequestInBulk(APInvoiceChargesApprovalRequest request, IBulkPostingCache bulkPostingCache);
		void BulkPrintAPInvoicesAndCreditNotes(ZGuid[] postedTransactionPKs);
		void PrintAPInvoiceAndCreditNote(ZGuid postedTransactionPK);
		ContinueWithSave IsPostingWithInvalidPostingGroupsAllowed(InvoicingBase invoice);
	}
}
