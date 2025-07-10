using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsCommonCargoDescValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_HarmonisedTariff_CheckConditionC015()
		{
			AssertNoMessageErrorContaining(goodsItem.BY_HarmonisedTariffInfo, "C015");
			AddSgiForTest("SGIXX", 12.1m);
			goodsItem.BY_HarmonisedTariff = ZString.Empty;
			AssertHasMessageErrorContaining(goodsItem.BY_HarmonisedTariffInfo, "C015");
		}

		public void TestCheckBY_HarmonisedTariff_BY_HarmonisedTariffCharacterCheck()
		{
			goodsItem.BY_HarmonisedTariff = "123";
			AssertHasMessageError(goodsItem.BY_HarmonisedTariffInfo, "Commodity Code must be at least 4 and up to 10 digits of the full commodity code.(R060)");

			goodsItem.BY_HarmonisedTariff = "12345678901";
			AssertHasMessageError(goodsItem.BY_HarmonisedTariffInfo, "Commodity Code must be at least 4 and up to 10 digits of the full commodity code.(R060)");

			goodsItem.BY_HarmonisedTariff = "123456789";
			AssertNoMessageError(goodsItem.BY_HarmonisedTariffInfo, "Commodity Code must be at least 4 and up to 10 digits of the full commodity code.(R060)");
		}

		public void TestCheckBY_GrossWeightUnit()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(goodsItem.BY_GrossWeightUnitInfo, "!@", "KG");
		}

		public void TestCheckBY_NetWeight()
		{
			const string errorMessage = "If Gross Weight is not 0, then it must be greater or equal to Net Weight.";
			CombineAssertions(() =>
			{
				goodsItem.BY_GrossWeightUnit = "KG";
				goodsItem.BY_GrossWeight = 10;
				goodsItem.BY_NetWeightUnit = "KG";
				goodsItem.BY_NetWeight = 10;
				AssertNoWarningContaining("Equal Gross and Net", goodsItem.BY_NetWeightInfo, errorMessage);
				goodsItem.BY_NetWeight = 20;
				AssertHasWarningContaining("Net larger than gross", goodsItem.BY_NetWeightInfo, errorMessage);
				goodsItem.BY_NetWeight = 9;
				AssertNoWarningContaining("Net less than gross", goodsItem.BY_NetWeightInfo, errorMessage);
				goodsItem.BY_GrossWeight = 0;
				AssertNoWarningContaining("Gross empty, Net filled", goodsItem.BY_NetWeightInfo, errorMessage);
			});
		}

		public void TestCheckBY_NetWeightUnit()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(goodsItem.BY_NetWeightUnitInfo, "!@", "KG");
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			goodsItem = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
		}
		NctsHeader nctsHeader;
		NctsCommonCargoDesc goodsItem;

		void AddSgiForTest(ZString sensitiveGoodsCode, ZDecimal sensitiveGoodsQuantity)
		{
			var sgi = goodsItem.AdditionalInfos.AddNew();
			sgi.CSI_Code = sensitiveGoodsCode;
			sgi.CSI_Description = sensitiveGoodsQuantity.ToString();
		}
	}
}
