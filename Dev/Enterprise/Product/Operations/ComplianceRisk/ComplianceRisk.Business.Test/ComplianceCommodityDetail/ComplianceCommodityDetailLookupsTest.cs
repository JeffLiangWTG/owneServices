using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceCommodityDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCommodityRiskStatusList()
		{
			var commodityDetail = Factory.New<ComplianceCommodityDetail>();

			CombineAssertions("Commodity Unknown risk status lists", () =>
			{
				commodityDetail.CCD_RiskStatus = Codes.NotChecked;
				Assert(commodityDetail.Lookups.CommodityRiskStatusCodeList.Count == 3);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					Codes.NotChecked,
					Codes.Released,
					Codes.Blocked
				}, commodityDetail.Lookups.CommodityRiskStatusCodeList.Cast<CodeDescriptionPair>().Select(u => u.Code).ToList());
			});

			CombineAssertions("Commodity Clear risk status lists", () =>
			{
				commodityDetail.CCD_RiskStatus = Codes.Clear;
				Assert(commodityDetail.Lookups.CommodityRiskStatusCodeList.Count == 2);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					Codes.Clear,
					Codes.Blocked
				}, commodityDetail.Lookups.CommodityRiskStatusCodeList.Cast<CodeDescriptionPair>().Select(u => u.Code).ToList());
			});

			CombineAssertions("Commodity Potential risk status lists", () =>
			{
				commodityDetail.CCD_RiskStatus = Codes.PotentialRisk;
				Assert(commodityDetail.Lookups.CommodityRiskStatusCodeList.Count == 3);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					Codes.PotentialRisk,
					Codes.Released,
					Codes.Blocked
				}, commodityDetail.Lookups.CommodityRiskStatusCodeList.Cast<CodeDescriptionPair>().Select(u => u.Code).ToList());
			});

			CombineAssertions("Commodity Possible risk status lists", () =>
			{
				commodityDetail.CCD_RiskStatus = Codes.PossibleRisk;
				Assert(commodityDetail.Lookups.CommodityRiskStatusCodeList.Count == 3);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					Codes.PossibleRisk,
					Codes.Released,
					Codes.Blocked
				}, commodityDetail.Lookups.CommodityRiskStatusCodeList.Cast<CodeDescriptionPair>().Select(u => u.Code));
			});

			CombineAssertions("Commodity High risk status lists", () =>
			{
				commodityDetail.CCD_RiskStatus = Codes.HighRisk;
				Assert(commodityDetail.Lookups.CommodityRiskStatusCodeList.Count == 3);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					Codes.HighRisk,
					Codes.Released,
					Codes.Blocked
				}, commodityDetail.Lookups.CommodityRiskStatusCodeList.Cast<CodeDescriptionPair>().Select(u => u.Code));
			});

			CombineAssertions("Commodity Release / Blocked risk status lists", () =>
			{
				commodityDetail.CCD_RiskStatus = Codes.Released;
				Assert(commodityDetail.Lookups.CommodityRiskStatusCodeList.Count == 2);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					Codes.Released,
					Codes.Blocked
				}, commodityDetail.Lookups.CommodityRiskStatusCodeList.Cast<CodeDescriptionPair>().Select(u => u.Code).ToList());
			});
		}

		public void TestCommodityImportAlertForExportCodeList()
		{
			var commodityDetail = Factory.New<ComplianceCommodityDetail>();
			commodityDetail.CCD_RiskStatus = Codes.Released;
			commodityDetail.ImportAlertsForExportJobDescription = "Clear";

			CombineAssertions("Commodity Import Alert For Export Job Code List", () =>
			{
				Assert(commodityDetail.Lookups.CommodityImportAlertForExportCodeList.Count == 3);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					Codes.Clear,
					Codes.PossibleRisk,
					Codes.HighRisk
				}, commodityDetail.Lookups.CommodityImportAlertForExportCodeList.Cast<CodeDescriptionPair>().Select(u => u.Code).ToList());
			});
		}
	}
}
