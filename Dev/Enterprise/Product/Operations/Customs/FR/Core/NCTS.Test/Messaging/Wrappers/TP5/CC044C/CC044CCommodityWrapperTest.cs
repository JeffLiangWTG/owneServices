using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing;

[TestedType(typeof(CC044CCommodityWrapper))]
sealed class CC044CCommodityWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC044CCommodityWrapper>
{
	public void TestDescriptionOfGoods()
	{
		AssertEquals("DescriptionOfGoods should equal BY_Description as BY_UnloadedState is NEW", "Description", Provider.DescriptionOfGoods);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		var unloadedItem = item.UnloadedGoodsItem;
		unloadedItem.BY_Description = "Description DIF";
		var provider = CC044CCommodityWrapper.New(unloadedItem);

		AssertEquals("DescriptionOfGoods should equal BY_Description as UnloadedGoodsItem.BY_UnloadedState is DIF", "Description DIF", provider.DescriptionOfGoods);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		provider = CC044CCommodityWrapper.New(item);

		AssertNullOrEmpty("DescriptionOfGoods should be empty as BY_UnloadedState is MIS", provider.DescriptionOfGoods);
	}

	public void TestCommodityCode()
	{
		AssertEquals("HarmonizedSystemSubHeadingCode should be mapped to BY_FormattedHarmonisedTariff first 6 digits as BY_UnloadedState is NEW.", "112233", Provider.CommodityCode.HarmonizedSystemSubHeadingCode);
		AssertEquals("CombinedNomenclatureCode should be mapped to BY_FormattedHarmonisedTariff 7th and 8th digits as BY_UnloadedState is NEW.", "44", Provider.CommodityCode.CombinedNomenclatureCode);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		var unloadedItem = item.UnloadedGoodsItem;
		unloadedItem.BY_HarmonisedTariff = "12345678";
		var provider = CC044CCommodityWrapper.New(unloadedItem);

		AssertEquals("HarmonizedSystemSubHeadingCode should also be mapped to UnloadedGoodsItem.BY_HarmonisedTariff first 6 digits as BY_UnloadedState is DIF.", "123456", provider.CommodityCode.HarmonizedSystemSubHeadingCode);
		AssertEquals("CombinedNomenclatureCode should also be mapped to UnloadedGoodsItem.BY_HarmonisedTariff 7th and 8th digits as BY_UnloadedState is DIF as BY_UnloadedState is DIF.", "78", provider.CommodityCode.CombinedNomenclatureCode);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		provider = CC044CCommodityWrapper.New(item);

		AssertNull("CommodityCode should be null as BY_UnloadedState is MIS", provider.CommodityCode);
	}

	public void TestGoodsMeasure()
	{
		AssertEquals("NetMass should be BY_NetWeight as BY_UnloadedState is NEW.", 1.99m, Provider.GoodsMeasure.NetMass);
		AssertEquals("GrossMass should be BY_GrossWeight as BY_UnloadedState is NEW.", 2.99m, Provider.GoodsMeasure.GrossMass);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		var unloadedItem = item.UnloadedGoodsItem;
		unloadedItem.BY_NetWeight = 10m;
		unloadedItem.BY_GrossWeight = 20m;
		var provider = CC044CCommodityWrapper.New(unloadedItem);

		AssertEquals("NetMass should be UnloadedGoodsItem.BY_NetWeight as BY_UnloadedState is DIF", 10m, provider.GoodsMeasure.NetMass);
		AssertEquals("GrossMass should be UnloadedGoodsItem.BY_GrossWeight as BY_UnloadedState is DIF", 20m, provider.GoodsMeasure.GrossMass);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		provider = CC044CCommodityWrapper.New(item);

		AssertEquals("NetMass should be 0 as BY_UnloadedState is MIS", 0m, provider.GoodsMeasure.NetMass);
		AssertEquals("GrossMass should be 0 as BY_UnloadedState is MIS", 0m, provider.GoodsMeasure.GrossMass);
	}

	protected override CC044CCommodityWrapper GetProvider()
	{
		item = Factory.New<NctsArrivalCargoDesc>();

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
		item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;

		item.BY_GrossWeight = 2.99m;
		item.BY_NetWeight = 1.99m;
		item.BY_CustomsSecondQuantity = 3.99m;
		item.BY_NetWeightUnit = "KG";
		item.BY_GrossWeightUnit = "KG";
		item.BY_HarmonisedTariff = "1122334455";
		item.BY_CusC4Number = "0145792-7";
		item.BY_Description = "Description";
		return CC044CCommodityWrapper.New(item);
	}
	NctsArrivalCargoDesc item;
}
