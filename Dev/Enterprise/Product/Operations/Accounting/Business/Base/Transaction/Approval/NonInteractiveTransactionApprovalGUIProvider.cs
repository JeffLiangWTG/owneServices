using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class NonInteractiveTransactionApprovalGUIProvider : IPostingTransactionApprovalGUIProvider
	{
		public NonInteractiveTransactionApprovalGUIProvider(BusinessObjectFactory factory, ISecurityOverrideProviderWithApprovalRequest securityOverrideProviderWithApprovalRequest)
		{
			FactoryForApprovalRequests = factory;
			this.securityOverrideProviderWithApprovalRequest = securityOverrideProviderWithApprovalRequest;
		}

		public bool IsBulkPosting => false;

		public BusinessObjectFactory FactoryForApprovalRequests { get; private set; }

		public JobInvoicingPostingOption PostingOption => JobInvoicingPostingOption.All;

		public bool ShowLoginFormForTest => false;

		public string SecurityItemForTest => null;

		public ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProvider(bool showApprovalRequestButton, bool supportMultipleApprover = false)
		{
			return securityOverrideProviderWithApprovalRequest;
		}

		readonly ISecurityOverrideProviderWithApprovalRequest securityOverrideProviderWithApprovalRequest;

		public ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderForARCreditNote(bool showApprovalRequestButton, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] approvalRequests = null)
		{
			throw new NotImplementedException();
		}

		public Tuple<ZGuid, ZString> GetParentIdAndTableCodeForJobPostingAction()
		{
			throw new NotImplementedException();
		}

		public Tuple<ZString, ZString> ShowCreditNoteReversalReasonForm(string existingReasonCode)
		{
			throw new NotImplementedException();
		}

		public void NotifyBulkPostingIsNotAuthorized(string message)
		{
		}

		public void ResetFactoryForApprovalRequests()
		{
		}

		public void RollbackPosting()
		{
		}

		public ZDialogResult ShowApprovalFormToSetDescription(GenApprovalRequest approvingRequest)
		{
			return ZDialogResult.OK;
		}

		public ZDialogResult ShowMessage(string messageText, string messageCaption, ZMessageBoxButtons messageBoxButtons, ZMessageBoxIcon messageBoxIcon, ZDialogResult dialogResult)
		{
			return ZDialogResult.OK;
		}
	}
}
