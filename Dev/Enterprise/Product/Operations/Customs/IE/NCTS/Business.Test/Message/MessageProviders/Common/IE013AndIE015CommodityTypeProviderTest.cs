using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE013AndIE015CommodityTypeProviderTest : Customs.Business.Testing.DataProviderTestCase<IE013AndIE015CommodityTypeProvider>
	{
		protected override IE013AndIE015CommodityTypeProvider GetProvider() => new IE013AndIE015CommodityTypeProvider(cargoDesc);

		public void TestGoodsDescription()
		{
			cargoDesc.BY_Description = "Desc";
			AssertEquals("GoodsDescription", "Desc", Provider.GoodsDescription);
		}

		public void TestCUSCode()
		{
			cargoDesc.BY_CusC4Number = "1";
			AssertEquals("CUSCode", "1", Provider.CUSCode);
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			cargoDesc.BY_FormattedHarmonisedTariff = "1010";
			AssertEquals("HarmonizedSystemSubHeadingCode", "1010", Provider.HarmonizedSystemSubHeadingCode);
			cargoDesc.BY_FormattedHarmonisedTariff = "690.3 201564";
			AssertEquals("HarmonizedSystemSubHeadingCode", "690320", Provider.HarmonizedSystemSubHeadingCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			cargoDesc.BY_FormattedHarmonisedTariff = "1";
			AssertEquals("CombinedNomenclatureCode", string.Empty, Provider.CombinedNomenclatureCode);
			cargoDesc.BY_FormattedHarmonisedTariff = "690320";
			AssertEquals("CombinedNomenclatureCode", string.Empty, Provider.CombinedNomenclatureCode);
			cargoDesc.BY_FormattedHarmonisedTariff = "690.3 201564";
			AssertEquals("CombinedNomenclatureCode", "15", Provider.CombinedNomenclatureCode);
		}

		public void TestDangerousGoods()
		{
			SetupDangerousGood("0001");
			SetupDangerousGood("  ");
			SetupDangerousGood("0003");
			cargoDesc.UNDGs.AddNew();
			var dangerousGoods = Provider.DangerousGoods.ToArray();
			AssertEquals("DangerousGoods", 2, dangerousGoods.Length);
			AssertEquals("DangerousGoods", "0001", dangerousGoods[0]);
			AssertEquals("DangerousGoods", "0003", dangerousGoods[1]);
		}

		void SetupDangerousGood(string code)
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = code;
			var undg = cargoDesc.UNDGs.AddNew();
			undg.DI_DG = substance.PK;
			var pivot = undg.UNDGSubstancePivotCollection.AddNew();
			pivot.DP_UNNO = code;
			pivot.DP_IsDefault = true;
		}

		public void TestGrossMass()
		{
			cargoDesc.BY_GrossWeight = 20.1222m;
			AssertEquals("GrossMass", 20.1222m, Provider.GrossMass);
		}

		public void TestNetMass()
		{
			cargoDesc.BY_NetWeight = 22.52m;
			AssertEquals("NetMass", 22.52m, Provider.NetMass);
		}

		public void TestSupplementaryUnits()
		{
			cargoDesc.BY_CustomsSecondQuantity = 14.328m;
			AssertEquals("SupplementaryUnits", 14.328m, Provider.SupplementaryUnits);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			bill = nctsHeader.Bills.AddNew();
			cargoDesc = bill.GoodsItems.AddNew();
		}
		NctsDepartureCargoDesc cargoDesc;
		NctsBill bill;
		NctsHeader nctsHeader;
	}
}
