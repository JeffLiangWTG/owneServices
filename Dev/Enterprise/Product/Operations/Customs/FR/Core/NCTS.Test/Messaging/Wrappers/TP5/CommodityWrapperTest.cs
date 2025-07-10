using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CommodityWrapperTest : Customs.Business.Testing.DataProviderTestCase<CommodityWrapper>
	{
		public void TestDescriptionOfGoods()
		{
			AssertEquals("DescriptionOfGoods should equal BY_Description", "Description", Provider.DescriptionOfGoods);
		}

		public void TestCusCode()
		{
			AssertEquals("CusCode should equal BY_CusC4Number", "0145792-7", Provider.CusCode);
		}

		public void TestCommodityCode()
		{
			AssertEquals("HarmonizedSystemSubHeadingCode should be mapped to tariff first 6 digits.", "112233", Provider.CommodityCode.HarmonizedSystemSubHeadingCode);
			AssertEquals("CombinedNomenclatureCode should be mapped to tariff 7th and 8th digits.", "44", Provider.CommodityCode.CombinedNomenclatureCode);
		}

		public void TestDangerousGoods()
		{
			AssertContainsExactElementsInAnyOrder("UNNumber shoul equal the dangerous items substance code.", new string[] { "0004b", "0005a" }, Provider.DangerousGoods.Select(x => x.UNNumber));

			var item = Factory.New<EU.NCTS.Business.NctsArrivalCargoDesc>();
			var arrivalProvider = CommodityWrapper.New(item);
			AssertNull(arrivalProvider.DangerousGoods);
		}

		public void TestGoodsMeasure()
		{
			var goodsMeasure = Provider.GoodsMeasure;
			AssertEquals("GrossMass should equal BY_GrossWeight", 2.99m, goodsMeasure.GrossMass);
			AssertEquals("NetMass should equal BY_NetWeight", 1.99m, goodsMeasure.NetMass);
			AssertEquals("SupplementaryUnits should equal BY_CustomsSecondQuantity", 3.99m, goodsMeasure.SupplementaryUnits);
		}

		protected override CommodityWrapper GetProvider()
		{
			var item = Factory.New<EU.NCTS.Business.NctsDepartureCargoDesc>();

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Code = "0004b";
			var undgItem1 = Factory.New<UNDGDataItem>();
			undgItem1.DI_DG = substance1.PK;
			item.UNDGs.Add(undgItem1);

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Code = "0005a";
			var undgItem2 = Factory.New<UNDGDataItem>();
			undgItem2.DI_DG = substance2.PK;
			item.UNDGs.Add(undgItem2);

			item.BY_GrossWeight = 2.99m;
			item.BY_NetWeight = 1.99m;
			item.BY_CustomsSecondQuantity = 3.99m;
			item.BY_HarmonisedTariff = "1122334455";
			item.BY_CusC4Number = "0145792-7";
			item.BY_Description = "Description";

			return CommodityWrapper.New(item);
		}
	}
}
