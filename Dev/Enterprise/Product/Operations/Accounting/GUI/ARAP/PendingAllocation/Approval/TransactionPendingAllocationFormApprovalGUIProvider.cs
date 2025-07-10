using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Security;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public class TransactionPendingAllocationFormApprovalGUIProvider : TransactionFormApprovalGUIProvider<TransactionPendingAllocation, TransactionPendingAllocationApprovalBulk, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails>
	{
		protected override BusinessObjectFactory GetNewFactory()
		{
			return new TransactionPendingAllocationApprovalRequestFactory();
		}

		protected override TransactionPendingAllocationApprovalBulk GetNewApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, TransactionPendingAllocationApprovalRequest approvalRequest)
		{
			return new TransactionPendingAllocationApprovalBulk(factory, interactiveSecurityOverrideProvider, approvalRequest);
		}

		protected override TransactionApprovalBulkForm<TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails> GetApprovalFormToSetDescription(TransactionPendingAllocationApprovalBulk approvalBulk, TransactionApprovalFormModes actionMode)
		{
			return new TransactionPendingAllocationApprovalBulkForm(approvalBulk, actionMode);
		}

		protected override ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderCore(bool showApprovalRequestButton, bool alwaysCreateApprovalRequest, bool supportMultipleApprover = false)
		{
			return new InvoicingSecurityOverrideProvider(showApprovalRequestButton, alwaysCreateApprovalRequest);
		}

		protected override ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderCoreForARCreditNote(bool showApprovalRequestButton, bool alwaysCreateApprovalRequest, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] approvalRequests = null)
		{
			throw new NotImplementedException();
		}
	}

	public class TransactionPendingAllocationApprovalRequestFactory : BusinessObjectFactory
	{
		public TransactionPendingAllocationApprovalRequestFactory() : base()
		{
		}

		public TransactionPendingAllocationApprovalRequestFactory(DbConnection connection) : base(connection)
		{
		}
	}
}
