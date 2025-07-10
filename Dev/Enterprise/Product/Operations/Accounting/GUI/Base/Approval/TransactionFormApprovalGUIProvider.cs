using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public abstract class TransactionFormApprovalGUIProvider<TransactionType, ApprovalBulkType, RequestType, DetailsType> : IPostingTransactionApprovalGUIProvider
		where TransactionType : TransactionHeader
		where ApprovalBulkType : TransactionApprovalBulk<TransactionType, RequestType, DetailsType>
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public BusinessObjectFactory FactoryForApprovalRequests
		{
			get { return factoryForApprovalRequests ?? (factoryForApprovalRequests = GetNewFactory()); }
		}
		BusinessObjectFactory factoryForApprovalRequests;

		protected virtual BusinessObjectFactory GetNewFactory()
		{
			return new BusinessObjectFactory();
		}

		public void ResetFactoryForApprovalRequests()
		{
			factoryForApprovalRequests = null;
		}

		public bool AlwaysCreateApprovalRequest { get; set; }

		public bool IsPostingCanceled { get; private set; }

		public void InitializeNewPosting()
		{
			ResetFactoryForApprovalRequests();
			IsPostingCanceled = false;
		}

		protected abstract ApprovalBulkType GetNewApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, RequestType approvalRequest);
		protected abstract TransactionApprovalBulkForm<TransactionType, RequestType, DetailsType> GetApprovalFormToSetDescription(ApprovalBulkType approvalBulk, TransactionApprovalFormModes actionMode);
		protected abstract ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderCore(bool showApprovalRequestButton, bool alwaysCreateApprovalRequest, bool supportMultipleApprover = false);
		protected abstract ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderCoreForARCreditNote(bool showApprovalRequestButton, bool alwaysCreateApprovalRequest, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] approvalRequests = null);

		#region IPostingTransactionApprovalGUIProvider

		bool IPostingTransactionApprovalGUIProvider.IsBulkPosting
		{
			get { return false; }
		}

		JobInvoicingPostingOption IPostingTransactionApprovalGUIProvider.PostingOption
		{
			get { return JobInvoicingPostingOption.All; }
		}

		ZDialogResult IPostingTransactionApprovalGUIProvider.ShowMessage(string messageText, string messageCaption, ZMessageBoxButtons messageBoxButtons, ZMessageBoxIcon messageBoxIcon, ZDialogResult dialogResult)
		{
			return Globals.Message.Show(messageText, messageCaption, messageBoxButtons, messageBoxIcon, dialogResult);
		}

		ZDialogResult IPostingTransactionApprovalGUIProvider.ShowApprovalFormToSetDescription(GenApprovalRequest approvingRequest)
		{
			var approvingRequestCasted = approvingRequest as RequestType;
			var approvalBulk = GetNewApprovalBulk(approvingRequestCasted.Factory, new InteractiveSecurityOverrideProvider(), approvingRequestCasted);
			var formToShow = GetApprovalFormToSetDescription(approvalBulk, TransactionApprovalFormModes.SetDescription);

			return (ZDialogResult)ZFormModaliser.ShowDialogAndDispose(formToShow);
		}

		void IPostingTransactionApprovalGUIProvider.NotifyBulkPostingIsNotAuthorized(string message)
		{
			Globals.Message.ShowError(message);
		}

		BusinessObjectFactory IPostingTransactionApprovalGUIProvider.FactoryForApprovalRequests
		{
			get { return FactoryForApprovalRequests; }
		}

		void IPostingTransactionApprovalGUIProvider.ResetFactoryForApprovalRequests()
		{
			ResetFactoryForApprovalRequests();
		}

		Tuple<ZGuid, ZString> IPostingTransactionApprovalGUIProvider.GetParentIdAndTableCodeForJobPostingAction()
		{
			throw new NotSupportedException("This method for Job posting only and is not applicable for invoice posting."); // Developer message
		}

#if DEBUG
		bool IPostingTransactionApprovalGUIProvider.ShowLoginFormForTest
		{
			get { return this.ShowLoginFormForTest; }
		}

		public bool ShowLoginFormForTest = true;

		string IPostingTransactionApprovalGUIProvider.SecurityItemForTest
		{
			get { return this.SecurityItemForTest; }
		}

		public string SecurityItemForTest;
#endif

		void IPostingTransactionApprovalGUIProvider.RollbackPosting()
		{
			IsPostingCanceled = true;
		}

		ISecurityOverrideProviderWithApprovalRequest IPostingTransactionApprovalGUIProvider.GetNewSecurityOverrideProvider(bool showApprovalRequestButton, bool supportMultipleApprover)
		{
			return GetNewSecurityOverrideProviderCore(showApprovalRequestButton, AlwaysCreateApprovalRequest, supportMultipleApprover);
		}

		ISecurityOverrideProviderWithApprovalRequest IPostingTransactionApprovalGUIProvider.GetNewSecurityOverrideProviderForARCreditNote(bool showApprovalRequestButton, bool supportMultipleApprover, ARCreditNoteApprovalRequest[] approvalRequests)
		{
			return GetNewSecurityOverrideProviderCoreForARCreditNote(showApprovalRequestButton, AlwaysCreateApprovalRequest, supportMultipleApprover, approvalRequests);
		}

		Tuple<ZString, ZString> IPostingTransactionApprovalGUIProvider.ShowCreditNoteReversalReasonForm(string existingReasonCode)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
