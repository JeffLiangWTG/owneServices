using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class CostaRicaAccountingCountryFactory :
		IAccountingCountryFactory,
		IInstanceProvider<IEInvoicingEligibilityDecider>
	{
		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new CostaRicaEInvoicingEligibilityDecider();
	}
}
