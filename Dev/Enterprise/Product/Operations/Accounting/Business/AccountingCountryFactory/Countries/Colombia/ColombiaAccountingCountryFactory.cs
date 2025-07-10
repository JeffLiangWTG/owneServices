using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Colombia;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class ColombiaAccountingCountryFactory :
		IAccountingCountryFactory,
		IInstanceProvider<IEInvoicingEligibilityDecider>,
		IInstanceProvider<ITaxFrameworkConfigurationDefaults>
	{
		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new ColombiaEInvoicingEligibilityDecider();

		ITaxFrameworkConfigurationDefaults IInstanceProvider<ITaxFrameworkConfigurationDefaults>.Get() => new TaxFrameworkConfigurationDefaults();
	}
}
