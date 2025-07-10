using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.China.Testing
{
	public class ChinaComplianceNumberProviderTest : TestCaseWithFactory
	{
		public void TestCountryFactoryCanReturnComplianceNumberProvider()
		{
			AssertNotNull(GetFeatureInterface());
		}

		public void TestCanAllocateComplianceNumberAlwaysReturnsFalse()
		{
			var featureInterface = GetFeatureInterface();
			AssertEquals(false, featureInterface.CanAllocateComplianceNumber(true));
			AssertEquals(false, featureInterface.CanAllocateComplianceNumber(false));
		}

		IComplianceNumberProvider GetFeatureInterface() => ((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory()).GetFeatureInterface<IComplianceNumberProvider>(Constants.CountryCodes.China);
	}
}
