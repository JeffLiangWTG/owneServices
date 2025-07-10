using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Uruguay;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class UruguayAccountingCountryFactory :
		IAccountingCountryFactory,
		IInstanceProvider<IEInvoicingPivotsToRequeueFilterProvider>,
		IInstanceProvider<IEInvoicingRequeuePivotsStatusProvider>
	{
		IEInvoicingPivotsToRequeueFilterProvider IInstanceProvider<IEInvoicingPivotsToRequeueFilterProvider>.Get() => new UruguayEInvoicingPivotsToRequeueInfo();

		IEInvoicingRequeuePivotsStatusProvider IInstanceProvider<IEInvoicingRequeuePivotsStatusProvider>.Get() => new UruguayEInvoicingRequeuePivotsStatusProvider();
	}
}
