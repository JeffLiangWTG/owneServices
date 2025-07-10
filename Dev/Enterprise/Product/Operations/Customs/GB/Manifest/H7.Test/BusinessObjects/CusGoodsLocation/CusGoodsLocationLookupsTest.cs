using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	public class CusGoodsLocationLookupsTest : TestCaseWithFactory
	{
		public void TestQualifierList()
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			var list = cusGoodsLocation.Lookups.QualifierList;

			CombineAssertions(() =>
			{
				AssertEquals(1, list.Count);

				Assert(list.ContainsCode("U"));
				AssertEquals("UN/LOCODE", list.GetDescriptionFromCode("U"));
			});
		}

		public void TestUnlocodeList()
		{
			var bill = Factory.New<AsycudaBill>();
			var lookup = bill.CusGoodsLocation.Lookups;

			bill.CusGoodsLocation.CGL_Type = "A";
			bill.CusGoodsLocation.CGL_Qualifier = "U";
			var list = bill.CusGoodsLocation.Lookups.UnlocodeList;
			AssertType<ZZRefCusCodeListCombinedCollection>("Type", list);
			var collection = list as ZZRefCusCodeListCombinedCollection;

			CombineAssertions(() =>
			{
				AssertEquals("Attribute Value:Property", "AU", collection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);
				AssertEquals("List Type:Property", "PORT", collection.FilterBusinessObjectDefaults["List Type:Property"].Value);
				AssertEquals("Effective Date:Property1", ZDateTime.Today, collection.FilterBusinessObjectDefaults["Effective Date:Property1"].Value);
				AssertContainsExactElementsInExactOrder("Country/Region or Grouping", new[] { "CDS" }, collection.DataGroupingCodes);
			});

			bill.CusGoodsLocation.CGL_Type = "B";
			collection = bill.CusGoodsLocation.Lookups.UnlocodeList as ZZRefCusCodeListCombinedCollection;
			AssertEquals("Attribute Value is set to BU when CGL_Type is B", "BU", collection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);

			bill.CusGoodsLocation.CGL_Type = "C";
			collection = bill.CusGoodsLocation.Lookups.UnlocodeList as ZZRefCusCodeListCombinedCollection;
			AssertEquals("Attribute Value is set to CU when CGL_Type is C", "CU", collection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);

			bill.CusGoodsLocation.CGL_Type = "D";
			collection = bill.CusGoodsLocation.Lookups.UnlocodeList as ZZRefCusCodeListCombinedCollection;
			AssertEquals("Attribute Value is set to DU when CGL_Type is D", "DU", collection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);
		}
	}
}
