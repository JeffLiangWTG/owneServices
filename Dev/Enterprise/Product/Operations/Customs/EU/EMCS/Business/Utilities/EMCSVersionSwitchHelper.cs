using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public static class EMCSVersionSwitchHelper
	{
		public static bool IsPhase4_1Enabled(ZString dataGroupingCode)
		{
			var isPhase4_1 = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.EMCS_Phase4_1, dataGroupingCode, ZDateTime.Today, priorityToPilotFunctionality: false);
			if (!isPhase4_1)
			{
				var systemCode = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;
				var liveSystem = ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute(Constants.FunctionalityTypes.EMCS_Phase4_1, dataGroupingCode, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.System);
				isPhase4_1 = systemCode.Equals(liveSystem);
			}
			return isPhase4_1;
		}
	}
}
