using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Brazil;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class BrazilAccountingCountryFactory : IAccountingCountryFactory,
		IInstanceProvider<ITaxFrameworkConfigurationDefaults>,
		IInstanceProvider<ITaxFrameworkThresholdMethodProvider>,
		IInstanceProvider<IEInvoicingPreEligibilityProvider>
	{
		ITaxFrameworkThresholdMethodProvider IInstanceProvider<ITaxFrameworkThresholdMethodProvider>.Get() => new BrazilThresholdMethodProvider();

		ITaxFrameworkConfigurationDefaults IInstanceProvider<ITaxFrameworkConfigurationDefaults>.Get() => new TaxFrameworkConfigurationDefaults();

		IEInvoicingPreEligibilityProvider IInstanceProvider<IEInvoicingPreEligibilityProvider>.Get() => new BrazilEInvoicingPreEligibilityProvider();
	}
}
