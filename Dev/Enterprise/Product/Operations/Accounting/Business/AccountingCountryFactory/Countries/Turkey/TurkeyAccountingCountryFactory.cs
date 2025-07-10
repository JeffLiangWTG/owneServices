using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class TurkeyAccountingCountryFactory : IAccountingCountryFactory,
		IInstanceProvider<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider>,
		IInstanceProvider<IEInvoicingPivotActionTypeProvider>,
		IInstanceProvider<IEInvoicingPreEligibilityProvider>
	{
		ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider IInstanceProvider<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider>.Get() => new TurkeyTransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider();

		IEInvoicingPivotActionTypeProvider IInstanceProvider<IEInvoicingPivotActionTypeProvider>.Get() => new TurkeyEInvoicingPivotActionTypeProvider();

		IEInvoicingPreEligibilityProvider IInstanceProvider<IEInvoicingPreEligibilityProvider>.Get() => new TurkeyEInvoicingPreEligibilityProvider();
	}
}
