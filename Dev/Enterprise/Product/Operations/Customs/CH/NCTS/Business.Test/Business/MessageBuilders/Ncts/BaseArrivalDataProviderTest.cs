using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class BaseArrivalDataProviderTest<TDataProvider, TMessageSendingObject> : BaseTransitDataProviderTest<TDataProvider, TMessageSendingObject>
	where TDataProvider : class
	where TMessageSendingObject : class, INctsMessageSendingObject
{
	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override void AddGoodsItem(NctsBill bill) => bill.ArrivalGoodsItems.AddNew();

	protected MovementReferenceNumberSupportingInfo MovementReferenceNumber
	{
		get
		{
			var movementReferenceNumbers = NctsHeader.ArrivalMovementHeader.MovementReferenceNumbers;
			return movementReferenceNumbers.FirstOrDefault() ?? movementReferenceNumbers.AddNew();
		}
	}

	protected SupernumeraryGoods SupernumeraryGoodsItem
	{
		get
		{
			var supernumeraryGoods = NctsHeader.ArrivalMovementHeader.SupernumeraryGoods;
			return supernumeraryGoods.FirstOrDefault() ?? supernumeraryGoods.AddNew();
		}
	}

	protected AdditionalTransitOperation AdditionalTransitOperation
	{
		get
		{
			var additionalTransitOperations = NctsHeader.ArrivalMovementHeader.AdditionalTransitOperations;
			return additionalTransitOperations.FirstOrDefault() ?? additionalTransitOperations.AddNew();
		}
	}
}
