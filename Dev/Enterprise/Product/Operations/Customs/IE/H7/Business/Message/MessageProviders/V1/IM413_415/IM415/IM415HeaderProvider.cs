using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM415HeaderProvider : IIM415Header
	{
		readonly MessageSendingObject messageSendingObject;

		public IM415HeaderProvider(MessageSendingObject messageSendingObject)
		{
			this.messageSendingObject = messageSendingObject;
		}

		public IIM413AndIM415Declaration Declaration => CachedValueHelper.GetValue(ref declarationCached, () => new IM413AndIM415DeclarationProvider(messageSendingObject, generateNewLRN: true));
		CachedValue<IIM413AndIM415Declaration> declarationCached;

		public IIM413AndIM415GoodsShipment GoodsShipment => CachedValueHelper.GetValue(ref goodsShipmentCached, () => new IM413AndIM415GoodsShipmentProvider((AsycudaBill)messageSendingObject.Bill));
		CachedValue<IIM413AndIM415GoodsShipment> goodsShipmentCached;
	}
}
