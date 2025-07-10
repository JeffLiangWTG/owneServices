using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceCommodityTest : TestCase
	{
		public void TestConstructor()
		{
			var parentJobID = ZGuid.NewZGuid();
			var goodsDescription = "Long Description".PadRight(ComplianceCommodityDetailSchema.CCD_Description.MaxLength + 1, 'L');

			var complianceCommodity = new ComplianceCommodity("WCO", "AU", "Source", parentJobID, "AU", "Compliance", goodsDescription);
			AssertEquals("WCO", complianceCommodity.HarmonizedCode);
			AssertEquals("AU", complianceCommodity.GroupingOrCountry);
			AssertEquals("Source", complianceCommodity.Source);
			AssertEquals(parentJobID, complianceCommodity.ParentJobID);
			AssertEquals("AU", complianceCommodity.Origin);
			AssertEquals("Compliance", complianceCommodity.CommoditySource);
			AssertEquals(goodsDescription.Substring(0, ComplianceCommodityDetailSchema.CCD_Description.MaxLength), complianceCommodity.GoodsDescription);

			complianceCommodity = new ComplianceCommodity("123456", "WCO", "Source", parentJobID, "AU", "Packing", goodsDescription, "REL", "Assessment Notes", new ZDateTime(2024, 9, 6, 1, 2, 3));
			AssertEquals("123456", complianceCommodity.HarmonizedCode);
			AssertEquals("WCO", complianceCommodity.GroupingOrCountry);
			AssertEquals("Source", complianceCommodity.Source);
			AssertEquals(parentJobID, complianceCommodity.ParentJobID);
			AssertEquals("REL", complianceCommodity.RiskStatus);
			AssertEquals("Assessment Notes", complianceCommodity.AssessmentNotes);
			AssertEquals("AU", complianceCommodity.Origin);
			AssertEquals("Packing", complianceCommodity.CommoditySource);
			AssertEquals(goodsDescription.Substring(0, ComplianceCommodityDetailSchema.CCD_Description.MaxLength), complianceCommodity.GoodsDescription);
			AssertEquals(new ZDateTime(2024, 9, 6, 1, 2, 3), complianceCommodity.DateAddedUtc);
		}

		public void TestShouldImplementIComplianceCommodityInterface()
		{
			Assert("ComplianceCommodity class should implement IComplianceCommodity interface", typeof(IComplianceCommodity).IsAssignableFrom(typeof(ComplianceCommodity)));
		}
	}
}
