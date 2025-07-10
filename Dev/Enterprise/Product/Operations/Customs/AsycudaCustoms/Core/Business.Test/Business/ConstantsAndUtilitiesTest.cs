using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class ConstantsAndUtilitiesTest : TestCaseWithFactory
	{
		public void TestIsRiskManagementEnabled_RiskManagementEnabled()
		{
			AssertIsRiskManagementEnabled(true);
		}

		public void TestIsRiskManagementEnabled_RiskManagementNotEnabled()
		{
			AssertIsRiskManagementEnabled(false);
		}

		void AssertIsRiskManagementEnabled(bool isRiskEnabled)
		{
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, currentCountry, ZDateTime.Today, isRiskEnabled))
			{
				AssertEquals(isRiskEnabled, Extensions.IsRiskManagementEnabled(currentCountry, Factory));
			}
		}
	}
}
