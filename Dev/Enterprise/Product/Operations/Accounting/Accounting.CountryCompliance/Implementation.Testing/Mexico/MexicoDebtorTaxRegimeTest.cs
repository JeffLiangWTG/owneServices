using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Mexico.Testing
{
	public class MexicoDebtorTaxRegimeTest : TestCaseWithFactory
	{
		public void TestImplements_IDebtorTaxRegime()
		{
			AssertNotNull(GetFeatureInterface());
		}

		public void TestGetOrgCusCodeReturnExpectedCode()
		{
			AssertEquals("REG", GetFeatureInterface().GetOrgCusCode());
		}

		public void TestGetTaxRegimeIdTypesReturnExpectedSetOfIdTypes()
		{
			var expectedIdTypes = new string[] { "601", "603", "605", "606", "607", "608", "610", "611", "612", "614", "615", "616", "620", "621", "622", "623", "624", "625", "626" };
			var actualCodes = GetFeatureInterface().GetTaxRegimeIdTypes().GetAllCodes();
			AssertArrayEqualsByElements(expectedIdTypes, actualCodes);
		}

		IDebtorTaxRegime GetFeatureInterface() => ((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory()).GetFeatureInterface<IDebtorTaxRegime>(Core.Constants.CountryCodes.Mexico);
	}
}
