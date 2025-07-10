using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Panama;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class PanamaAccountingCountryFactory :
		IAccountingCountryFactory,
		IInstanceProvider<IEInvoicingEligibilityDecider>,
		IInstanceProvider<IEInvoicingPivotsToRequeueFilterProvider>,
		IInstanceProvider<IEInvoicingRequeuePivotsStatusProvider>
	{
		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new PanamaEInvoicingEligibilityDecider();

		IEInvoicingPivotsToRequeueFilterProvider IInstanceProvider<IEInvoicingPivotsToRequeueFilterProvider>.Get() => new PanamaEInvoicingPivotsToRequeueInfo();

		IEInvoicingRequeuePivotsStatusProvider IInstanceProvider<IEInvoicingRequeuePivotsStatusProvider>.Get() => new PanamaEInvoicingRequeuePivotsStatusProvider();
	}
}
