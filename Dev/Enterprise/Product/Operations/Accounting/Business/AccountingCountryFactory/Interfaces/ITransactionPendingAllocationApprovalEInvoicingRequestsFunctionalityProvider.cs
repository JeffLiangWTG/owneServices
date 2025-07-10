using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider
	{
		// NOTE: This interface is for enabling eInvoicing Requests functionality for AP Invoices that its country awaits information about Rejecting and Approving of AP Invoices in Transactions Pending Allocation Approval module

		bool IsTransactionEligibleToCreateApprovalRequest(AccTransactionHeader transaction);

		bool IsTransactionEligibleToCreateRejectionRequest(AccTransactionHeader transaction);

		bool IsTransactionEditable(AccTransactionHeader transaction);
	}
}
