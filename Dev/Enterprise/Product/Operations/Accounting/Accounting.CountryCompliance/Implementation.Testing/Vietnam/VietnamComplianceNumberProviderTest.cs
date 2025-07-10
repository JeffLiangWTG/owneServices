using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Vietnam.Testing
{
	public class VietnamComplianceNumberProviderTest : TestCaseWithFactory
	{
		public void TestCountryFactoryCanReturnComplianceNumberProvider()
		{
			AssertNotNull(GetFeatureInterface());
		}

		public void TestCanAllocateComplianceNumberReternsRevesedValueOfAllLinesWithCMTCharge()
		{
			var featureInterface = GetFeatureInterface();
			AssertEquals(false, featureInterface.CanAllocateComplianceNumber(true));
			AssertEquals(true, featureInterface.CanAllocateComplianceNumber(false));
		}

		IComplianceNumberProvider GetFeatureInterface() => ((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory()).GetFeatureInterface<IComplianceNumberProvider>(Constants.CountryCodes.VietNam);
	}
}
