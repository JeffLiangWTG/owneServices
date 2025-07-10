using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Brazil
{
	class BrazilThresholdMethodProvider : ITaxFrameworkThresholdMethodProvider
	{
		bool ITaxFrameworkThresholdMethodProvider.IsTransactionLevelGroupThresholdMethodSupported => true;

		bool ITaxFrameworkThresholdMethodProvider.IsTransactionLevelTaxBaseThresholdMethodSupported => false;
	}
}
