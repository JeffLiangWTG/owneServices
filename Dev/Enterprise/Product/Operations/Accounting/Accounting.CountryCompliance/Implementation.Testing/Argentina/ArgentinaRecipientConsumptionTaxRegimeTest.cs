using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Argentina.Testing
{
	public class ArgentinaRecipientConsumptionTaxRegimeTest : TestCaseWithFactory
	{
		public void TestImplements_IRecipientConsumptionTaxRegime()
		{
			AssertNotNull(GetFeatureInterface());
		}

		public void TestGetOrgCusCodesReturnExpectedSetOfCodes()
		{
			var expectedCodes = new ZString[] { "IVE", "IVF", "IVI", "IVM", "IVN", "IVP", "IVR", "IVS", "IVX" };
			var actualCodes = GetFeatureInterface().GetOrgCusCodes();
			AssertArrayEqualsByElements(expectedCodes, actualCodes);
		}

		IRecipientConsumptionTaxRegime GetFeatureInterface() => ((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory()).GetFeatureInterface<IRecipientConsumptionTaxRegime>(Core.Constants.CountryCodes.Argentina);
	}
}
