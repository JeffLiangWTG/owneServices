using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.India;
using Enterprise.Accounting.Business.AccountingCountryFactory.India;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class IndiaAccountingCountryFactory : IAccountingCountryFactory,
		IEnableDocumentSigningServiceProvider,
		ICloudSigningServiceProviderAPIEndpointProvider,
		IInstanceProvider<IComplianceNumberResetStatus>,
		IInstanceProvider<IEInvoicingEligibilityDecider>,
		IInstanceProvider<ITaxFrameworkConfigurationDefaults>,
		IInstanceProvider<IEInvoicingPreEligibilityProvider>
	{
		IComplianceNumberResetStatus IInstanceProvider<IComplianceNumberResetStatus>.Get() => new ComplianceNumberResetStatus();
		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new IndiaEInvoicingEligibilityDecider();
		IEInvoicingPreEligibilityProvider IInstanceProvider<IEInvoicingPreEligibilityProvider>.Get() => new IndiaEInvoicingPreEligibilityProvider();
		ITaxFrameworkConfigurationDefaults IInstanceProvider<ITaxFrameworkConfigurationDefaults>.Get() => new TaxFrameworkConfigurationDefaults();

		bool IEnableDocumentSigningServiceProvider.IsEnableDocumentSigningService() => true;
		string ICloudSigningServiceProviderAPIEndpointProvider.GetCloudSigningServiceProviderAPIEndpoint() => Env.Instance.IsProductionSystem ? "https://remotesigning-prod.emudhra.com/api/signdoc" : "https://staging-rsds.emudhra.com/api/signdoc";
	}
}
