using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class DominicanRepublicAccountingCountryFactory :
		IAccountingCountryFactory,
		IInstanceProvider<IEInvoicingEligibilityDecider>
	{
		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new DominicanRepublicEInvoicingEligibilityDecider();
	}
}
