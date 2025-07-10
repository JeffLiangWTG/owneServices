using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class InventoryHouseConsignmentDataProviderTest : BaseArrivalDataProviderTest<InventoryHouseConsignmentDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	protected override InventoryHouseConsignmentDataProvider CreateDataProvider() => InventoryHouseConsignmentDataProvider.NewCollection(NctsHeader.Bills).First();

	public void TestNewCollection() => CombineAssertions(() =>
	{
		AssertNull("null", HouseConsignmentDataProvider.NewCollection(null));

		var bill1 = NctsHeader.Bills.AddNew();
		var bill2 = NctsHeader.Bills.AddNew();
		var bill3 = NctsHeader.Bills.AddNew();
		var bill4 = NctsHeader.Bills.AddNew();
		bill1.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		bill2.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		bill3.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		bill4.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		bill1.B0_ReferenceID = "BILL1";
		bill2.B0_ReferenceID = "BILL2";
		bill3.B0_ReferenceID = "BILL3";
		bill4.B0_ReferenceID = "BILL4";

		var providers = InventoryHouseConsignmentDataProvider.NewCollection(NctsHeader.Bills);

		AssertEquals("NEW", false, providers.Any(x => x.ReferenceNumberUCR == "BILL1"));
		AssertEquals("MIS", true, providers.Any(x => x.ReferenceNumberUCR == "BILL2"));
		AssertEquals("DIF", true, providers.Any(x => x.ReferenceNumberUCR == "BILL3"));
		AssertEquals("DEC", false, providers.Any(x => x.ReferenceNumberUCR == "BILL4"));
	});

	public void TestProperties()
	{
		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		NctsBill.MovementDetail.B9_SeqNo = "2";
		AssertEquals("SequenceNumber", 2, DataProvider.SequenceNumber);
	}

	public void TestReferenceUCR()
	{
		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		NctsBill.B0_ReferenceID = "UCR-REF";
		ArrivalMovementHeader.BM_UniqueConsignmentReference = "UCR-REF";
		AssertEquals("has value when BM_UniqueConsignmentReference is not emtpy", "UCR-REF", DataProvider.ReferenceNumberUCR);

		ArrivalMovementHeader.BM_UniqueConsignmentReference = string.Empty;
		AssertEquals("has value when BM_UniqueConsignmentReference is emtpy", "UCR-REF", DataProvider.ReferenceNumberUCR);

		NctsBill.B0_ReferenceID = ZString.Empty;
		AssertNull("null when empty", DataProvider.ReferenceNumberUCR);
	}

	public void TestConsignmentItems() => CombineAssertions(() =>
	{
		var goodsItem1 = NctsBill.ArrivalGoodsItems.AddNew();
		var goodsItem2 = NctsBill.ArrivalGoodsItems.AddNew();
		var goodsItem3 = NctsBill.ArrivalGoodsItems.AddNew();
		var goodsItem4 = NctsBill.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		goodsItem3.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;

		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals("DIF/NEW", true, DataProvider.ConsignmentItems.Any(x => x.GoodsItemNumber == goodsItem1.BY_LineNo));
		AssertEquals("DIF/MIS", true, DataProvider.ConsignmentItems.Any(x => x.GoodsItemNumber == goodsItem2.BY_LineNo));
		AssertEquals("DIF/DIF", true, DataProvider.ConsignmentItems.Any(x => x.GoodsItemNumber == goodsItem3.BY_LineNo));
		AssertEquals("DIF/DEC", false, DataProvider.ConsignmentItems.Any(x => x.GoodsItemNumber == goodsItem4.BY_LineNo));

		NctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		ResetDataProvider();
		AssertEquals("MIS/NEW", false, DataProvider.ConsignmentItems.Any(x => x.GoodsItemNumber == goodsItem1.BY_LineNo));
		AssertEquals("MIS/MIS", true, DataProvider.ConsignmentItems.Any(x => x.GoodsItemNumber == goodsItem2.BY_LineNo));
		AssertEquals("MIS/DIF", true, DataProvider.ConsignmentItems.Any(x => x.GoodsItemNumber == goodsItem3.BY_LineNo));
		AssertEquals("MIS/DEC", true, DataProvider.ConsignmentItems.Any(x => x.GoodsItemNumber == goodsItem4.BY_LineNo));

		AssertSame("cached", DataProvider.ConsignmentItems, DataProvider.ConsignmentItems);
	});
}
