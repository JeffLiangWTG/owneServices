using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceCommodityRiskLog))]
	public class ComplianceCommodityRiskLogTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var commodity = new Commodity
			{
				Code = "000123",
				Conditions = "Dummy",
				HsCodeDescription = "S123: Description 1",
				RiskStatus = Codes.Clear,
				Source = "S123",
				CommoditySource = "Packing",
				Notes = "NOTES",
				OriginOfGoods = "AU",
				GoodsDescription = "Test",
				IsAssessmentInitiated = true,
				DateAddedUtc = new DateTime(2024, 9, 6, 8, 45, 12)
			};

			var commodityRiskLog = new ComplianceCommodityRiskLog(commodity);

			AssertEquals("000123", commodityRiskLog.Code);
			AssertEquals("Dummy", commodityRiskLog.Conditions);
			AssertEquals("S123: Description 1", commodityRiskLog.HsCodeDescription);
			AssertEquals("CLR", commodityRiskLog.RiskStatus);
			AssertEquals("Clear", commodityRiskLog.RiskStatusDescription);
			AssertEquals("S123", commodityRiskLog.Source);
			AssertEquals("Packing", commodityRiskLog.CommoditySource);
			AssertEquals("NOTES", commodityRiskLog.Notes);
			AssertEquals("AU", commodityRiskLog.OriginOfGoods);
			AssertEquals("Test", commodityRiskLog.GoodsDescription);
			AssertEquals(new DateTime(2024, 9, 6, 8, 45, 12), commodityRiskLog.DateAddedUtc);

			commodity.RiskStatus = Codes.Released;
			AssertEquals("Released", commodityRiskLog.RiskStatusDescription);

			commodity.RiskStatus = Codes.PotentialRisk;
			AssertEquals("Potential Risk", commodityRiskLog.RiskStatusDescription);

			commodity.RiskStatus = Codes.Blocked;
			AssertEquals("Blocked", commodityRiskLog.RiskStatusDescription);

			commodity.RiskStatus = Codes.PossibleRisk;
			AssertEquals("Possible Risk", commodityRiskLog.RiskStatusDescription);

			commodity.RiskStatus = Codes.HighRisk;
			AssertEquals("High Risk", commodityRiskLog.RiskStatusDescription);

			commodity.RiskStatus = Codes.Unknown;
			AssertEquals("Unknown", commodityRiskLog.RiskStatusDescription);

			commodity.RiskStatus = Codes.NotChecked;
			AssertEquals("Not Checked", commodityRiskLog.RiskStatusDescription);

			commodity.RiskStatus = string.Empty;
			AssertEquals("** INITIATE ASSESSMENT TO VIEW **", commodityRiskLog.RiskStatusDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComplianceCommodityRiskLog(new Commodity());
		}
	}
}
