using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(InventoryConsignmentItemDataProvider))]
sealed class InventoryConsignmentItemDataProviderTest : BaseArrivalDataProviderTest<InventoryConsignmentItemDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	public void TestNewCollection()
	{
		AssertNull(ConsignmentItemDataProvider.NewCollection(null));
	}

	public void TestReferenceNumberUCR() => CombineAssertions(() =>
	{
		ArrivalGoodsItem.BY_CommercialReferenceNumber = "U123";
		AssertEquals("has a value when both BM_UniqueConsignmentReference and B0_ReferenceID are empty", "U123", DataProvider.ReferenceNumberUCR);

		NctsBill.B0_ReferenceID = "U321";
		ArrivalMovementHeader.BM_UniqueConsignmentReference = "U456";
		AssertEquals("has a value when both BM_UniqueConsignmentReference and B0_ReferenceID are not empty", "U123", DataProvider.ReferenceNumberUCR);

		ArrivalGoodsItem.BY_CommercialReferenceNumber = ZString.Empty;
		AssertNull("null when empty", DataProvider.ReferenceNumberUCR);
	});

	public void TestUnloadingProperties() => CombineAssertions(() =>
	{
		((NctsBill)ArrivalGoodsItem.Bill).UnloadingRemarkCode = "C2";
		((NctsBill)ArrivalGoodsItem.Bill).UnloadingRemarkText = "Bill remarks";
		ArrivalGoodsItem.UnloadingRemarkCode = "C1";
		ArrivalGoodsItem.UnloadingRemarkText = "GoodsItem remarks";

		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;

		AssertEquals("DIF - GoodsItem UnloadingRemarkCode", "C1", DataProvider.UnloadingRemarkCode);
		AssertEquals("DIF - GoodsItem UnloadingRemarkText", "GoodsItem remarks", DataProvider.UnloadingRemarkText);

		((NctsBill)ArrivalGoodsItem.Bill).UnloadingRemarkCode = "C2";
		((NctsBill)ArrivalGoodsItem.Bill).UnloadingRemarkText = "Bill remarks";
		ArrivalGoodsItem.UnloadingRemarkCode = "C1";
		ArrivalGoodsItem.UnloadingRemarkText = "GoodsItem remarks";

		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;

		ResetDataProvider();
		AssertEquals("MIS - Bill UnloadingRemarkCode", "C2", DataProvider.UnloadingRemarkCode);
		AssertEquals("MIS - Bill UnloadingRemarkText", "Bill remarks", DataProvider.UnloadingRemarkText);
	});

	public void TestUnloadingRemarks()
	{
		ArrivalGoodsItem.UnloadingRemarkCode = "G";
		ArrivalGoodsItem.UnloadingRemarkText = "GoodsItem";
		ArrivalGoodsItem.UnloadedStatus = NctsUnloadedStateList.Codes.DIF;
		AssertEquals("GoodsItem UnloadingCode", "G", DataProvider.UnloadingRemarkCode);
		AssertEquals("GoodsItem UnloadingText", "GoodsItem", DataProvider.UnloadingRemarkText);

		NctsBill.UnloadingRemarkCode = "H";
		NctsBill.UnloadingRemarkText = "House";
		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		ResetDataProvider();
		AssertEquals("HouseConsignment UnloadingCode", "H", DataProvider.UnloadingRemarkCode);
		AssertEquals("HouseConsignment UnloadingText", "House", DataProvider.UnloadingRemarkText);
	}

	public void TestPackagings() => CombineAssertions(() =>
	{
		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;

		var package1 = ArrivalGoodsItem.Packages.AddNew();
		var package2 = ArrivalGoodsItem.Packages.AddNew();
		var package3 = ArrivalGoodsItem.Packages.AddNew();
		var package4 = ArrivalGoodsItem.Packages.AddNew();
		package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		package3.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
		package4.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;

		AssertEquals("NEW", true, DataProvider.Packagings.Any(x => x.SequenceNumber == package1.B5_SequenceNumber));
		AssertEquals("MIS", true, DataProvider.Packagings.Any(x => x.SequenceNumber == package2.B5_SequenceNumber));
		AssertEquals("DIF", true, DataProvider.Packagings.Any(x => x.SequenceNumber == package3.B5_SequenceNumber));
		AssertEquals("DEC", false, DataProvider.Packagings.Any(x => x.SequenceNumber == package4.B5_SequenceNumber));

		AssertSame("cached", DataProvider.Packagings, DataProvider.Packagings);
	});

	public void TestCommodity() => CombineAssertions(() =>
	{
		ArrivalGoodsItem.BY_HarmonisedTariff = "12345678901";
		var package = ArrivalGoodsItem.Packages.AddNew();
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;

		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertNotNull(DataProvider.Commodity);
		AssertEquals("1234.5678", DataProvider.Commodity.CommodityCode.NationalCustomsTariffNumber);

		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		ResetDataProvider();
		AssertNotNull(DataProvider.Commodity);
		AssertEquals("1234.5678", DataProvider.Commodity.CommodityCode.NationalCustomsTariffNumber);

		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		ResetDataProvider();
		AssertNull("Commodity when GoodsItem UnloadedState = MIS", DataProvider.Commodity);

		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		ResetDataProvider();
		AssertNull("Commodity when Bill UnloadedState = MIS", DataProvider.Commodity);

		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		ResetDataProvider();
		AssertNull("Commodity if Bill UnloadedState = DIF, Goods Item BY_UnloadedState = DIF and Package B5_TypeOfDifference = MIS and no changes", DataProvider.Commodity);

		ArrivalGoodsItem.UnloadedGoodsItem.BY_HarmonisedTariff = "10987654321";
		ResetDataProvider();
		AssertNotNull(DataProvider.Commodity);
		AssertEquals("Commodity if Bill UnloadedState = DIF, Goods Item BY_UnloadedState = DIF and Package B5_TypeOfDifference = MIS and changes", "1098.7654", DataProvider.Commodity.CommodityCode.NationalCustomsTariffNumber);

		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		ResetDataProvider();
		AssertNotNull("Commodity if Bill UnloadedState = DEC, Goods Item BY_UnloadedState = DIF and Package B5_TypeOfDifference = MIS", DataProvider.Commodity);

		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		ResetDataProvider();
		AssertNotNull("Commodity if Bill UnloadedState = DIF, Goods Item BY_UnloadedState = DIF and Package B5_TypeOfDifference = DEC", DataProvider.Commodity);
	});

	protected override InventoryConsignmentItemDataProvider CreateDataProvider() => InventoryConsignmentItemDataProvider.NewCollection(NctsBill.ArrivalGoodsItems).First();
}
