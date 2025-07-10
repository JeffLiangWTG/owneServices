using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class EMCSVersionSwitchHelperTest : TestCaseWithFactory
	{
		public void TestIsPhase4_1Enabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EMCS_Phase4_1, Core.Constants.CountryCodes.Ireland, ZDateTime.Today, false))
			{
				Assert("Phase4_1 is not enabled cuz FUNC is not available", !EMCSVersionSwitchHelper.IsPhase4_1Enabled(Core.Constants.CountryCodes.Ireland));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EMCS_Phase4_1, Core.Constants.CountryCodes.Ireland, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.EMCS_Phase4_1, Core.Constants.CountryCodes.Ireland, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.System, string.Empty))
			{
				Assert("Phase4_1 is enabled cuz FUNC is available", EMCSVersionSwitchHelper.IsPhase4_1Enabled(Core.Constants.CountryCodes.Ireland));
			}

			var systemCode = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EMCS_Phase4_1, Core.Constants.CountryCodes.Ireland, ZDateTime.Today, false))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.EMCS_Phase4_1, Core.Constants.CountryCodes.Ireland, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.System, systemCode))
			{
				Assert("Phase4_1 is enabled cuz PFUNC matches with current system", EMCSVersionSwitchHelper.IsPhase4_1Enabled(Core.Constants.CountryCodes.Ireland));
			}
		}
	}
}
