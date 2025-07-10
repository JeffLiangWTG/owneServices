using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using NUnit.Framework;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business.Test
{
	class ComplianceStatusUtilsTest : TestCase
	{
		public void TestHasCommodityRisk()
		{
			AssertEquals(true, Codes.NotChecked.HasCommodityRiskFactor());
			AssertEquals(true, Codes.PotentialRisk.HasCommodityRiskFactor());
			AssertEquals(true, Codes.Blocked.HasCommodityRiskFactor());
			AssertEquals(true, Codes.HighRisk.HasCommodityRiskFactor());

			AssertEquals(false, Codes.Clear.HasCommodityRiskFactor());
			AssertEquals(false, Codes.Released.HasCommodityRiskFactor());
			AssertEquals(false, Codes.PossibleRisk.HasCommodityRiskFactor());

			AssertEquals(true, ((ZString)Codes.NotChecked).HasCommodityRiskFactor());
			AssertEquals(true, ((ZString)Codes.PotentialRisk).HasCommodityRiskFactor());
			AssertEquals(true, ((ZString)Codes.Blocked).HasCommodityRiskFactor());
			AssertEquals(true, ((ZString)Codes.HighRisk).HasCommodityRiskFactor());

			AssertEquals(false, ((ZString)Codes.Clear).HasCommodityRiskFactor());
			AssertEquals(false, ((ZString)Codes.Released).HasCommodityRiskFactor());
			AssertEquals(false, ((ZString)Codes.PossibleRisk).HasCommodityRiskFactor());
		}

		public void TestHasBlockedOrReleased()
		{
			AssertEquals(false, ((ZString)Codes.NotChecked).HasBlockedOrReleased());
			AssertEquals(false, ((ZString)Codes.PotentialRisk).HasBlockedOrReleased());
			AssertEquals(false, ((ZString)Codes.Clear).HasBlockedOrReleased());

			AssertEquals(true, ((ZString)Codes.Released).HasBlockedOrReleased());
			AssertEquals(true, ((ZString)Codes.Blocked).HasBlockedOrReleased());
		}

		public void TestHasComplianceRiskFactorInPartyLocationCommodity()
		{
			AssertEquals(true, ((ZString)Codes.HighRisk).HasComplianceRiskFactorInPartyLocationCommodity());
			AssertEquals(true, ((ZString)Codes.Blocked).HasComplianceRiskFactorInPartyLocationCommodity());
			AssertEquals(true, ((ZString)Codes.Incomplete).HasComplianceRiskFactorInPartyLocationCommodity());
			AssertEquals(true, ((ZString)Codes.PotentialRisk).HasComplianceRiskFactorInPartyLocationCommodity());
			AssertEquals(true, ((ZString)Codes.Unknown).HasComplianceRiskFactorInPartyLocationCommodity());

			AssertEquals(false, ((ZString)Codes.Clear).HasComplianceRiskFactorInPartyLocationCommodity());
			AssertEquals(false, ((ZString)Codes.PossibleRisk).HasComplianceRiskFactorInPartyLocationCommodity());
		}

		public void TestHasOverallRisk()
		{
			AssertEquals(true, ((ZString)Codes.Blocked).HasOverallRisk());
			AssertEquals(true, ((ZString)Codes.Held).HasOverallRisk());
			AssertEquals(true, ((ZString)Codes.PotentialRisk).HasOverallRisk());

			AssertEquals(false, ((ZString)Codes.Clear).HasOverallRisk());
			AssertEquals(false, ((ZString)Codes.OverrideClear).HasOverallRisk());
		}
	}
}
