using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class UnitedKingdomAccountingCountryFactory  : IAccountingCountryFactory,
		IInstanceProvider<IEInvoicingEligibilityDecider>
	{
		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new D365EligibilityDecider();
	}
}
