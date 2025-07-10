using CargoWise.Types;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList.Codes;

namespace Enterprise.ComplianceRisk.Integration
{
	public static class ComplianceStatusUtils
	{
		public static bool HasCommodityRiskFactor(this ZString riskStatus)
		{
			return HasCommodityRiskFactorCore(riskStatus);
		}

		public static bool HasCommodityRiskFactor(this string riskStatus)
		{
			return riskStatus.HasCommodityRiskFactorCore();
		}

		public static bool HasBlockedOrReleased(this ZString riskStatus)
		{
			return riskStatus.ToString() is Released or Blocked;
		}

		public static bool HasComplianceRiskFactorInPartyLocationCommodity(this ZString riskStatus)
		{
			return riskStatus.ToString() is HighRisk or Blocked or Incomplete or PotentialRisk or Unknown;
		}

		public static bool HasOverallRisk(this ZString riskStatus)
		{
			return riskStatus.ToString() is Blocked or PotentialRisk or Held;
		}

		static bool HasCommodityRiskFactorCore(this string riskStatus)
		{
			return riskStatus is PotentialRisk or Blocked or NotChecked or HighRisk;
		}
	}
}
