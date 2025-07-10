using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Vietnam.Testing
{
	class VietnamSupportNegativeAmountProviderTest : TestCaseWithFactory
	{
		public void TestIsNegativeChargesAllowed()
		{
			var featureInterface = GetFeatureInterface();
			Assert(featureInterface.IsNegativeChargesAllowed);
		}

		ISupportNegativeAmountOnARTransactions GetFeatureInterface() => ((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory()).GetFeatureInterface<ISupportNegativeAmountOnARTransactions>(Constants.CountryCodes.VietNam);
	}
}
