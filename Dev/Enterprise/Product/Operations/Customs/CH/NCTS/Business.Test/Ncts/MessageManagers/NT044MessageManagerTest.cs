using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT044MessageManagerTest : BasePassarMessageManagerTest<NT044MessageManager, NctsHeaderArrivalMessageSendingObject>
{
	protected override ZString MovementType => NctsMovementType.Codes.Arrival;

	protected override string MessageSendingObjectMessageType => PassarMessageTypeList.Codes.NT044;

	protected override string ExpectedMovementHeaderPhase => "044";
	protected override string ExpectedMessageSubType => "044";
	protected override Event ExpectedEventType => Events.DeclarationSentToCustoms;
	protected override string ExpectedEventReference => "NT044";
	protected override ZString InitialCustomsStatus => NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;

	protected override NT044MessageManager CreateMessageManager(NctsHeader nctsHeader) => new NT044MessageManager(new NctsHeaderArrivalMessageSendingObject(nctsHeader) { MessageType = MessageSendingObjectMessageType });

	protected override void AssertAfterGenerateMessage() => AssertEquals("BM_CustomsStatus", NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, NctsHeader.CommonMovementHeader.BM_CustomsStatus);

	protected override void AssertRollbackOnSaveFailed() => AssertEquals("BM_CustomsStatus", InitialCustomsStatus, NctsHeader.CommonMovementHeader.BM_CustomsStatus);

	public void TestAssignUnassignedDeclarationGoodsItemNumbers() => CombineAssertions(() =>
	{
		NctsHeader.CommonMovementHeader.BM_Phase = ExpectedMovementHeaderPhase;
		var bill1 = NctsHeader.Bills.AddNew();
		bill1.SequenceNumber = 1;
		var goodsItem11 = bill1.ArrivalGoodsItems.AddNew();
		var goodsItem12 = bill1.ArrivalGoodsItems.AddNew();
		var goodsItem13 = bill1.ArrivalGoodsItems.AddNew();
		var goodsItem14 = bill1.ArrivalGoodsItems.AddNew();
		var goodsItem15 = bill1.ArrivalGoodsItems.AddNew();
		var goodsItem16 = bill1.ArrivalGoodsItems.AddNew();
		var bill2 = NctsHeader.Bills.AddNew();
		bill2.SequenceNumber = 2;
		var goodsItem21 = bill2.ArrivalGoodsItems.AddNew();
		var goodsItem22 = bill2.ArrivalGoodsItems.AddNew();
		var goodsItem23 = bill2.ArrivalGoodsItems.AddNew();

		goodsItem11.BY_DeclarationGoodsItemNumber = 1;
		goodsItem11.BY_UnloadedState = ZString.Empty;

		goodsItem12.BY_DeclarationGoodsItemNumber = 0;
		goodsItem12.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;

		goodsItem13.BY_DeclarationGoodsItemNumber = 2;
		goodsItem13.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;

		goodsItem14.BY_DeclarationGoodsItemNumber = 0;
		goodsItem14.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;

		goodsItem15.BY_DeclarationGoodsItemNumber = 0;
		goodsItem15.BY_UnloadedState = ZString.Empty;

		goodsItem16.BY_DeclarationGoodsItemNumber = 3;
		goodsItem16.BY_UnloadedState = ZString.Empty;

		goodsItem21.BY_DeclarationGoodsItemNumber = 6;
		goodsItem21.BY_UnloadedState = ZString.Empty;

		goodsItem22.BY_DeclarationGoodsItemNumber = 4;
		goodsItem22.BY_UnloadedState = ZString.Empty;

		goodsItem23.BY_DeclarationGoodsItemNumber = 0;
		goodsItem23.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;

		var manager = CreateMessageManager(NctsHeader);
		var message = manager.GenerateMessages().First();

		AssertEquals("BY_DeclarationGoodsItemNumber 11", 1, goodsItem11.BY_DeclarationGoodsItemNumber);
		AssertEquals("BY_DeclarationGoodsItemNumber 12", 7, goodsItem12.BY_DeclarationGoodsItemNumber);
		AssertEquals("BY_DeclarationGoodsItemNumber 13", 2, goodsItem13.BY_DeclarationGoodsItemNumber);
		AssertEquals("BY_DeclarationGoodsItemNumber 14", 8, goodsItem14.BY_DeclarationGoodsItemNumber);
		AssertEquals("BY_DeclarationGoodsItemNumber 15", 0, goodsItem15.BY_DeclarationGoodsItemNumber);
		AssertEquals("BY_DeclarationGoodsItemNumber 16", 3, goodsItem16.BY_DeclarationGoodsItemNumber);
		AssertEquals("BY_DeclarationGoodsItemNumber 21", 6, goodsItem21.BY_DeclarationGoodsItemNumber);
		AssertEquals("BY_DeclarationGoodsItemNumber 22", 4, goodsItem22.BY_DeclarationGoodsItemNumber);
		AssertEquals("BY_DeclarationGoodsItemNumber 23", 9, goodsItem23.BY_DeclarationGoodsItemNumber);
	});
}
