using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionPendingAllocationPostingTriggerGUIProvider : IPostingJobAndTransactionApprovalGUIProvider
	{
		public TransactionPendingAllocationPostingTriggerGUIProvider(InvoicingBase invoice)
		{
			Transaction = invoice;
		}

		readonly InvoicingBase Transaction;

		bool IPostingTransactionApprovalGUIProvider.IsBulkPosting => false;

		BusinessObjectFactory IPostingTransactionApprovalGUIProvider.FactoryForApprovalRequests
		{
			get { return factoryForApprovalRequests ?? (factoryForApprovalRequests = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factoryForApprovalRequests;

		JobInvoicingPostingOption IPostingTransactionApprovalGUIProvider.PostingOption => JobInvoicingPostingOption.All;

		void IPostingJobAndTransactionApprovalGUIProvider.BulkPrintAPInvoicesAndCreditNotes(ZGuid[] postedTransactionPKs)
		{
			throw new NotImplementedException();
		}

		ISecurityOverrideProviderWithApprovalRequest IPostingTransactionApprovalGUIProvider.GetNewSecurityOverrideProvider(bool showApprovalRequestButton, bool supportMultipleApprover)
		{
			return new DefaultAccessSecurityProviderForWorkflowPosting();
		}

		ISecurityOverrideProviderWithApprovalRequest IPostingTransactionApprovalGUIProvider.GetNewSecurityOverrideProviderForARCreditNote(bool showApprovalRequestButton, bool supportMultipleApprover, ARCreditNoteApprovalRequest[] approvalRequests)
		{
			throw new NotImplementedException();
		}

		ISecurityOverrideProvider IPostingJobAndTransactionApprovalGUIProvider.GetNewSecurityOverrideProviderForPosting(InvoicingBase transaction)
		{
			throw new NotImplementedException();
		}

		Tuple<ZGuid, ZString> IPostingTransactionApprovalGUIProvider.GetParentIdAndTableCodeForJobPostingAction()
		{
			throw new NotImplementedException();
		}

		ContinueWithSave IPostingJobAndTransactionApprovalGUIProvider.IsPostingWithInvalidPostingGroupsAllowed(InvoicingBase invoice)
		{
			return ContinueWithSave.No;
		}

		void IPostingTransactionApprovalGUIProvider.NotifyBulkPostingIsNotAuthorized(string message)
		{
			Transaction.AddRowError(message);
		}

		string IPostingJobAndTransactionApprovalGUIProvider.PostRequest(APInvoiceChargesApprovalRequest request)
		{
			throw new NotImplementedException();
		}

		IApprovalRequestPostingResult IPostingJobAndTransactionApprovalGUIProvider.PostRequestInBulk(APInvoiceChargesApprovalRequest request, IBulkPostingCache bulkPostingCache)
		{
			throw new NotImplementedException();
		}

		void IPostingJobAndTransactionApprovalGUIProvider.PrintAPInvoiceAndCreditNote(ZGuid postedTransactionPK)
		{
			throw new NotImplementedException();
		}

		void IPostingTransactionApprovalGUIProvider.ResetFactoryForApprovalRequests()
		{
			factoryForApprovalRequests = null;
		}

		void IPostingTransactionApprovalGUIProvider.RollbackPosting()
		{
			//do nothing
		}

		ZDialogResult IPostingTransactionApprovalGUIProvider.ShowApprovalFormToSetDescription(GenApprovalRequest approvingRequest)
		{
			return ZDialogResult.No;
		}

		ZDialogResult IPostingTransactionApprovalGUIProvider.ShowMessage(string messageText, string messageCaption, ZMessageBoxButtons messageBoxButtons, ZMessageBoxIcon messageBoxIcon, ZDialogResult dialogResult)
		{
			Transaction.AddRowError(messageText);
			return ZDialogResult.No;
		}

		public Tuple<ZString, ZString> ShowCreditNoteReversalReasonForm(string existingReasonCode)
		{
			throw new NotImplementedException();
		}

#if DEBUG
		public bool ShowLoginFormForTest => true;

		public string SecurityItemForTest => null;
#endif

	}
}
