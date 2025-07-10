using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CommodityDataProvider))]
sealed class CommodityDataProviderTest : BaseDepartureDataProviderTest<CommodityDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNewConstructor()
	{
		AssertNull(CommodityDataProvider.New(null));
	}

	public void TestDescriptionOfGoods()
	{
		DepartureGoodsItem.BY_Description = "Kaugummi";
		AssertEquals("Kaugummi", DataProvider.DescriptionOfGoods);
	}

	public void TestCUSCode()
	{
		DepartureGoodsItem.BY_CusC4Number = "123456789";
		AssertEquals("123456789", DataProvider.CUSCode);
	}

	public void TestDangerousGoods() => CombineAssertions(() =>
	{
		AddDangerousGood("0004");
		AddDangerousGood("0005");

		AssertEquals("count", 2, DataProvider.DangerousGoods.Count);
		AssertSame("cached", DataProvider.DangerousGoods, DataProvider.DangerousGoods);

		void AddDangerousGood(string unNo, string variant = "") => DepartureGoodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, unNo, variant, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO).First().PK;
	});

	public void TestCommodityCode() => CombineAssertions(() =>
	{
		DepartureGoodsItem.BY_HarmonisedTariff = ZString.Empty;
		AssertNull(DataProvider.CommodityCode);
	});

	public void TestGoodsMeasure()
	{
		AssertNotNull(DataProvider.GoodsMeasure);
	}

	public void TestCommoditySpecification() => CombineAssertions(() =>
	{
		NctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		AssertType<NationalTransitCommoditySpecificationDataProvider>(DataProvider.CommoditySpecification);
		AssertSame("cached", DataProvider.CommoditySpecification, DataProvider.CommoditySpecification);

		ResetDataProvider();
		NctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		AssertNull(DataProvider.CommoditySpecification);
	});

	public void TestGrossMass()
	{
		DepartureGoodsItem.BY_GrossWeight = 12345;
		DepartureGoodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
		AssertEquals(12.345m, DataProvider.GrossMass);
	}

	public void TestNetMass()
	{
		CombineAssertions(() =>
		{
			DepartureGoodsItem.BY_NetWeight = ZDecimal.Zero;
			DepartureGoodsItem.BY_NetWeightUnit = Core.Constants.Weight.Grams;
			AssertNull("When empty", DataProvider.NetMass);

			ResetDataProvider();
			DepartureGoodsItem.BY_NetWeight = 1;
			DepartureGoodsItem.BY_NetWeightUnit = ZString.Empty;
			AssertNull("When no unit", DataProvider.NetMass);

			ResetDataProvider();
			DepartureGoodsItem.BY_NetWeight = 12345;
			DepartureGoodsItem.BY_NetWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals("When not empty", 12.345m, DataProvider.NetMass);
		});
	}

	public void TestSupplementaryUnits()
	{
		CombineAssertions(() =>
		{
			DepartureGoodsItem.BY_CustomsSecondQuantity = 0;
			AssertNull("Zero", DataProvider.SupplementaryUnits);
			DepartureGoodsItem.BY_CustomsSecondQuantity = 16;
			AssertEquals("Not zero", 16m, DataProvider.SupplementaryUnits);
		});
	}

	protected override CommodityDataProvider CreateDataProvider() => CommodityDataProvider.New(DepartureGoodsItem);
}
