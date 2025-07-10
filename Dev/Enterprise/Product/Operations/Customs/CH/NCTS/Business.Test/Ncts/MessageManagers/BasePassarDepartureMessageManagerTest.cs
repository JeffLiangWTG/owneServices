using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class BasePassarDepartureMessageManagerTest<TMessageManager, TMessageSendingObject> : BasePassarMessageManagerTest<TMessageManager, TMessageSendingObject>
	where TMessageManager : BasePassarMessageManager<TMessageSendingObject>, IMessageManager
	where TMessageSendingObject : BusinessObject, IMessageSendingObjectParent, IMessageSendingObject, INctsMessageSendingObject
{
	protected override ZString MovementType => NctsMovementType.Codes.Departure;

	public void TestAssignUnassignedDeclarationGoodsItemNumbers()
	{
		NctsHeader.CommonMovementHeader.BM_Phase = ExpectedMovementHeaderPhase;
		var bill1 = NctsHeader.Bills.AddNew();
		var goodsItem1 = bill1.GoodsItems.AddNew();
		var goodsItem2 = bill1.GoodsItems.AddNew();
		var bill2 = NctsHeader.Bills.AddNew();
		var goodsItem3 = bill2.GoodsItems.AddNew();
		var goodsItem4 = bill2.GoodsItems.AddNew();

		var manager = CreateMessageManager(NctsHeader);
		var message = manager.GenerateMessages().First();

		CombineAssertions("BeforeGenerateMessage", () =>
		{
			AssertEquals("BY_DeclarationGoodsItemNumber", 1, goodsItem1.BY_DeclarationGoodsItemNumber);
			AssertEquals("BY_DeclarationGoodsItemNumber", 2, goodsItem2.BY_DeclarationGoodsItemNumber);
			AssertEquals("BY_DeclarationGoodsItemNumber", 3, goodsItem3.BY_DeclarationGoodsItemNumber);
			AssertEquals("BY_DeclarationGoodsItemNumber", 4, goodsItem4.BY_DeclarationGoodsItemNumber);
		});
	}
}

abstract class BasePassarDepartureMessageManagerTest<TMessageManager> : BasePassarDepartureMessageManagerTest<TMessageManager, NctsHeaderDepartureMessageSendingObject>
	where TMessageManager : BasePassarMessageManager<NctsHeaderDepartureMessageSendingObject>
{
	protected NctsHeaderDepartureMessageSendingObject CreateMessageSendingObject(NctsHeader nctsHeader) => new NctsHeaderDepartureMessageSendingObject(nctsHeader) { MessageType = MessageSendingObjectMessageType };
}
