using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceRiskStatusLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodesType()
		{
			AssertType<ComplianceRiskStatusCodeList>(new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.Lookups.AllRiskStatusCodes);

			AssertType<CodeDescriptionPairList>(new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.Lookups.OverallRiskStatusCodes);
		}

		public void TestAllRiskStatusCodes()
		{
			var statusCodes = new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.Lookups.AllRiskStatusCodes;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				Codes.Clear,
				Codes.NotApplicable,
				Codes.OverrideClear,
				Codes.PossibleRisk,
				Codes.PotentialRisk,
				Codes.Unknown,
				Codes.Incomplete,
				Codes.Held,
				Codes.HighRisk,
				Codes.Blocked,
				Codes.NotAssessed,
			}, statusCodes.GetAllCodes());
		}

		public void TestOverallRiskStatusCodes()
		{
			var overallStatusCodes = new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.Lookups.OverallRiskStatusCodes;
			AssertContainsExactElementsInAnyOrder(new[]
			{
				Codes.Clear,
				Codes.OverrideClear,
				Codes.PotentialRisk,
				Codes.Held,
				Codes.Blocked,
			}, overallStatusCodes.GetAllCodes());
		}

		public void TestCommodityRiskStatusCodes()
		{
			var commodityStatusCodes = new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.Lookups.CommodityRiskStatusCodes;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				Codes.Clear,
				Codes.NotApplicable,
				Codes.PossibleRisk,
				Codes.PotentialRisk,
				Codes.Incomplete,
				Codes.HighRisk,
				Codes.Unknown,
				Codes.Blocked,
				Codes.NotAssessed,
			}, commodityStatusCodes.GetAllCodes());
		}

		public void TestPartyRiskStatusCodes()
		{
			var partyStatusCodes = new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.Lookups.PartyRiskStatusCodes;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				Codes.Clear,
				Codes.PotentialRisk,
				Codes.HighRisk,
				Codes.Blocked,
			}, partyStatusCodes.GetAllCodes());
		}

		public void TestLocationRiskStatusCodes()
		{
			var locationStatusCodes = new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.Lookups.LocationRiskStatusCodes;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				Codes.Clear,
				Codes.PotentialRisk,
				Codes.Blocked,
			}, locationStatusCodes.GetAllCodes());
		}
	}
}
