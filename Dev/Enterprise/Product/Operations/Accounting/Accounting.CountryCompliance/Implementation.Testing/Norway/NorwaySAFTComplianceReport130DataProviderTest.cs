using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Norway.Testing
{
	public class NorwaySAFTComplianceReport130DataProviderTest : TestCaseWithFactory
	{
		public void TestImplements_ISAFTComplianceReport130DataProvider()
		{
			AssertNotNull(GetFeatureInterface());
		}

		public void TestExpectedValues()
		{
			AssertEquals("A", GetFeatureInterface().TaxAccountingBasis);
			AssertEquals("Skatteetaten", GetFeatureInterface().TaxAuthority);
			AssertEquals("Merverdiavgift", GetFeatureInterface().TaxTableDescription);
			AssertEquals("A", GetFeatureInterface().JournalType);
		}

		ISAFTComplianceReport130DataProvider GetFeatureInterface() =>
			((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory())
			.GetFeatureInterface<ISAFTComplianceReport130DataProvider>(Core.Constants.CountryCodes.Norway);
	}
}
