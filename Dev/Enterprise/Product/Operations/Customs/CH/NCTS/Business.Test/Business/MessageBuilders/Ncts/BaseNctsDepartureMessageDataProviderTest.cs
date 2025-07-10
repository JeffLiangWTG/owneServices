using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class BaseNctsDepartureMessageDataProviderTest<TDataProvider, TMessageSendingObject> : BaseNctsMessageDataProviderTest<TDataProvider, TMessageSendingObject>
	where TDataProvider : class, IPassarMessage
	where TMessageSendingObject : class, INctsMessageSendingObject
{
	protected sealed override string MovementType => NctsMovementType.Codes.Departure;

	protected override void AddGoodsItem(NctsBill bill) => bill.GoodsItems.AddNew();
}
