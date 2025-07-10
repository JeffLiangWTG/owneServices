using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(InventoryCommodityDataProvider))]
sealed class InventoryCommodityDataProviderTest : BaseArrivalDataProviderTest<InventoryCommodityDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	public void TestNewConstructor()
	{
		AssertNull(InventoryCommodityDataProvider.New(null));
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		ArrivalGoodsItem.BY_FormattedHarmonisedTariff = "100010";
		ArrivalGoodsItem.BY_CusC4Number = "1000";
		ArrivalGoodsItem.BY_Description = "abc";
		ArrivalGoodsItem.BY_GrossWeight = 101;
		ArrivalGoodsItem.BY_NetWeight = 100;

		var package = ArrivalGoodsItem.Packages.AddNew();
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		package.B5_UnitCount = 1;
		package.B5_UnitType = "CT";
		package.B5_MarksAndNumbers = "M+N";

		CopyAllToUnloaded();
		AssertEquals("NationalCustomsTariffNumber - same", "1000.10", DataProvider.CommodityCode?.NationalCustomsTariffNumber);
		CopyAllToUnloaded(unloadedHarmonizedTariff: ZString.Empty);
		AssertEquals("NationalCustomsTariffNumber - empty", "1000.10", DataProvider.CommodityCode?.NationalCustomsTariffNumber);
		CopyAllToUnloaded(makeAnyDifference: true);
		AssertEquals("NationalCustomsTariffNumber - same, other difference", null, DataProvider.CommodityCode);
		CopyAllToUnloaded(unloadedHarmonizedTariff: ZString.Empty, makeAnyDifference: true);
		AssertEquals("NationalCustomsTariffNumber - empty, other difference", null, DataProvider.CommodityCode);
		CopyAllToUnloaded(unloadedHarmonizedTariff: "200000");
		AssertEquals("NationalCustomsTariffNumber -- change", "2000.00", DataProvider.CommodityCode?.NationalCustomsTariffNumber);
		CopyAllToUnloaded(difPackageHasDifference: false);
		AssertEquals("NationalCustomsTariffNumber - same, package with no difference", "1000.10", DataProvider.CommodityCode?.NationalCustomsTariffNumber);
		CopyAllToUnloaded(difPackageHasDifference: true);
		AssertEquals("NationalCustomsTariffNumber - same, package with difference", null, DataProvider.CommodityCode?.NationalCustomsTariffNumber);

		CopyAllToUnloaded();
		AssertEquals("CUSCode - same", "1000", DataProvider.CUSCode);
		CopyAllToUnloaded(unloadedCus4Number: ZString.Empty);
		AssertEquals("CUSCode - empty", "1000", DataProvider.CUSCode);
		CopyAllToUnloaded(makeAnyDifference: true);
		AssertEquals("CUSCode - same, other difference", null, DataProvider.CUSCode);
		CopyAllToUnloaded(unloadedCus4Number: ZString.Empty, makeAnyDifference: true);
		AssertEquals("CUSCode - empty, other difference", null, DataProvider.CUSCode);
		CopyAllToUnloaded(unloadedCus4Number: "2000");
		AssertEquals("CUSCode -- change", "2000", DataProvider.CUSCode);
		CopyAllToUnloaded(difPackageHasDifference: false);
		AssertEquals("CUSCode - same, package with no difference", "1000", DataProvider.CUSCode);
		CopyAllToUnloaded(difPackageHasDifference: true);
		AssertEquals("CUSCode - same, package with difference", null, DataProvider.CUSCode);

		CopyAllToUnloaded();
		AssertEquals("DescriptionOfGoods - same", "abc", DataProvider.DescriptionOfGoods);
		CopyAllToUnloaded(unloadedDescription: ZString.Empty);
		AssertEquals("DescriptionOfGoods - empty", "abc", DataProvider.DescriptionOfGoods);
		CopyAllToUnloaded(makeAnyDifferenceButNotDescription: true);
		AssertEquals("DescriptionOfGoods - same, other difference", null, DataProvider.DescriptionOfGoods);
		CopyAllToUnloaded(unloadedDescription: ZString.Empty, makeAnyDifferenceButNotDescription: true);
		AssertEquals("DescriptionOfGoods - empty, other difference", null, DataProvider.DescriptionOfGoods);
		CopyAllToUnloaded(unloadedDescription: "xyz");
		AssertEquals("DescriptionOfGoods -- change", "xyz", DataProvider.DescriptionOfGoods);
		CopyAllToUnloaded(difPackageHasDifference: false);
		AssertEquals("DescriptionOfGoods - same, package with no difference", "abc", DataProvider.DescriptionOfGoods);
		CopyAllToUnloaded(difPackageHasDifference: true);
		AssertEquals("DescriptionOfGoods - same, package with difference", null, DataProvider.DescriptionOfGoods);

		CopyAllToUnloaded();
		AssertEquals("GrossMass - same", 101m, DataProvider.GoodsMeasure?.GrossMass);
		CopyAllToUnloaded(unloadedGrossWeight: ZDecimal.Zero);
		AssertEquals("GrossMass - empty", 101m, DataProvider.GoodsMeasure?.GrossMass);
		CopyAllToUnloaded(makeAnyDifference: true, unloadedNetWeight: 99);
		AssertEquals("GrossMass - same, other difference", null, DataProvider.GoodsMeasure.GrossMass);
		CopyAllToUnloaded(unloadedGrossWeight: ZDecimal.Zero, makeAnyDifference: true, unloadedNetWeight: 99);
		AssertEquals("GrossMass - empty, other difference", null, DataProvider.GoodsMeasure.GrossMass);
		CopyAllToUnloaded(unloadedGrossWeight: 201m);
		AssertEquals("GrossMass -- change", 201m, DataProvider.GoodsMeasure?.GrossMass);
		CopyAllToUnloaded(difPackageHasDifference: false);
		AssertEquals("GrossMass - same, package with no difference", 101m, DataProvider.GoodsMeasure?.GrossMass);
		CopyAllToUnloaded(difPackageHasDifference: true);
		AssertEquals("GrossMass - same, package with difference", null, DataProvider.GoodsMeasure?.GrossMass);

		CopyAllToUnloaded();
		AssertEquals("NetMass - same", 100m, DataProvider.GoodsMeasure?.NetMass);
		CopyAllToUnloaded(unloadedNetWeight: ZDecimal.Zero);
		AssertEquals("NetMass - empty", 100m, DataProvider.GoodsMeasure?.NetMass);
		CopyAllToUnloaded(makeAnyDifference: true, unloadedGrossWeight: 99);
		AssertEquals("NetMass - same, other difference", null, DataProvider.GoodsMeasure?.NetMass);
		CopyAllToUnloaded(unloadedNetWeight: ZDecimal.Zero, makeAnyDifference: true, unloadedGrossWeight: 99);
		AssertEquals("NetMass - empty, other difference", null, DataProvider.GoodsMeasure?.NetMass);
		CopyAllToUnloaded(unloadedNetWeight: 200m);
		AssertEquals("NetMass -- change", 200m, DataProvider.GoodsMeasure?.NetMass);
		CopyAllToUnloaded(difPackageHasDifference: false);
		AssertEquals("NetMass - same, package with no difference", 100m, DataProvider.GoodsMeasure?.NetMass);
		CopyAllToUnloaded(difPackageHasDifference: true);
		AssertEquals("NetMass - same, package with difference", null, DataProvider.GoodsMeasure?.NetMass);

		CopyAllToUnloaded(makeAnyDifference: true);
		AssertEquals("No GoodsMeasure (neither gross nor net weight)", null, DataProvider.GoodsMeasure);

		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		ResetDataProvider();
		AssertEquals("NationalCustomsTariffNumber - MIS", "1000.10", DataProvider.CommodityCode?.NationalCustomsTariffNumber);
		AssertEquals("CUSCode - MIS", "1000", DataProvider.CUSCode);
		AssertEquals("DescriptionOfGoods - MIS", "abc", DataProvider.DescriptionOfGoods);
		AssertEquals("GrossMass - MIS", 101m, DataProvider.GoodsMeasure?.GrossMass);
		AssertEquals("NetMass - MIS", 100m, DataProvider.GoodsMeasure?.NetMass);

		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		ResetDataProvider();
		AssertEquals("NationalCustomsTariffNumber - NEW", "1000.10", DataProvider.CommodityCode?.NationalCustomsTariffNumber);
		AssertEquals("CUSCode - NEW", "1000", DataProvider.CUSCode);
		AssertEquals("DescriptionOfGoods - NEW", "abc", DataProvider.DescriptionOfGoods);
		AssertEquals("GrossMass - NEW", 101m, DataProvider.GoodsMeasure?.GrossMass);
		AssertEquals("NetMass - NEW", 100m, DataProvider.GoodsMeasure?.NetMass);

		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		ResetDataProvider();
		AssertNull("CommodityCode if HouseConsignment = DIF, GoodsItem = DIF and Packaging = MIS no changes", DataProvider.CommodityCode);
		AssertNull("GoodsMeasure if HouseConsignment = DIF, GoodsItem = DIF and Packaging = MIS no changes", DataProvider.GoodsMeasure);
		CopyAllToUnloaded(unloadedHarmonizedTariff: "200020", unloadedGrossWeight: 99);
		ResetDataProvider();
		AssertNotNull("CommodityCode if HouseConsignment = DIF, GoodsItem = DIF and Packaging = MIS changes", DataProvider.CommodityCode);
		AssertNotNull("GoodsMeasure if HouseConsignment = DIF, GoodsItem = DIF and Packaging = MIS changes", DataProvider.GoodsMeasure);
		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		ResetDataProvider();
		AssertNotNull("CommodityCode if HouseConsignment = DEC, GoodsItem = DIF and Packaging = MIS", DataProvider.CommodityCode);
		AssertNotNull("GoodsMeasure if HouseConsignment = DEC, GoodsItem = DIF and Packaging = MIS", DataProvider.GoodsMeasure);
		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		ResetDataProvider();
		AssertNotNull("CommodityCode if HouseConsignment = DIF, GoodsItem = DEC and Packaging = MIS", DataProvider.CommodityCode);
		AssertNotNull("GoodsMeasure if HouseConsignment = DIF, GoodsItem = DEC and Packaging = MIS", DataProvider.GoodsMeasure);
		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		ResetDataProvider();
		AssertNotNull("CommodityCode if HouseConsignment = DIF, GoodsItem = DIF and Packaging = DEC", DataProvider.CommodityCode);
		AssertNotNull("GoodsMeasure if HouseConsignment = DIF, GoodsItem = DIF and Packaging = DEC", DataProvider.GoodsMeasure);

		void CopyAllToUnloaded(ZString? unloadedHarmonizedTariff = null, ZString? unloadedCus4Number = null, ZString? unloadedDescription = null, ZDecimal? unloadedGrossWeight = null, ZDecimal? unloadedNetWeight = null, bool makeAnyDifference = false, bool makeAnyDifferenceButNotDescription = false, bool? difPackageHasDifference = null)
		{
			ArrivalGoodsItem.UnloadedGoodsItem.BY_HarmonisedTariff = unloadedHarmonizedTariff ?? ArrivalGoodsItem.BY_HarmonisedTariff;
			ArrivalGoodsItem.UnloadedGoodsItem.BY_CusC4Number = unloadedCus4Number ?? ArrivalGoodsItem.BY_CusC4Number;
			ArrivalGoodsItem.UnloadedGoodsItem.BY_Description = unloadedDescription ?? ArrivalGoodsItem.BY_Description;
			ArrivalGoodsItem.UnloadedGoodsItem.BY_GrossWeight = unloadedGrossWeight ?? ArrivalGoodsItem.BY_GrossWeight;
			ArrivalGoodsItem.UnloadedGoodsItem.BY_NetWeight = unloadedNetWeight ?? ArrivalGoodsItem.BY_NetWeight;
			if (makeAnyDifference || makeAnyDifferenceButNotDescription)
			{
				if (makeAnyDifferenceButNotDescription)
				{
					ArrivalGoodsItem.UnloadedGoodsItem.BY_CusC4Number += "X";
				}
				else
				{
					ArrivalGoodsItem.UnloadedGoodsItem.BY_Description += "X";
				}
				AssertEquals("Pre-condition: IsDIFWithDifferencesIncludingPackages", true, ArrivalGoodsItem.IsDIFWithDifferencesIncludingPackages);
			}
			package.B5_TypeOfDifference = difPackageHasDifference == null ? NctsUnloadedStateList.Codes.DEC : NctsUnloadedStateList.Codes.DIF;
			if (difPackageHasDifference != null)
			{
				package.PackDifference.B5_MarksAndNumbers = difPackageHasDifference.Value ? package.B5_MarksAndNumbers + "X" : package.B5_MarksAndNumbers;
				AssertEquals("Pre-condition: package.IsDIFWithDifferences", difPackageHasDifference.Value, package.IsDIFWithDifferences);
			}
			ResetDataProvider();
		}
	});

	protected override InventoryCommodityDataProvider CreateDataProvider() => InventoryCommodityDataProvider.New(NctsBill.MovementDetail.B9_UnloadedState, ArrivalGoodsItem);
}
