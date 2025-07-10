using Enterprise.Accounting.Business.AccountingCountryFactory.Vietnam;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class VietnamAccountingCountryFactory : IAccountingCountryFactory,
		IInstanceProvider<IEInvoicingRequeueProvider>,
		IInstanceProvider<IEInvoicingReversingProvider>,
		IInstanceProvider<IEInvoicingPreEligibilityProvider>
	{
		IEInvoicingRequeueProvider IInstanceProvider<IEInvoicingRequeueProvider>.Get() => new VietnamEInvoicingPivotsToRequeueInfo();
		IEInvoicingReversingProvider IInstanceProvider<IEInvoicingReversingProvider>.Get() => new VietnamEInvoicingReversingProvider();
		IEInvoicingPreEligibilityProvider IInstanceProvider<IEInvoicingPreEligibilityProvider>.Get() => new VietnamEInvoicingPreEligibilityProvider();
	}
}
