using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM413HeaderProvider : IIM413Header
	{
		readonly MessageSendingObject messageSendingObject;

		public IM413HeaderProvider(MessageSendingObject messageSendingObject)
		{
			this.messageSendingObject = messageSendingObject;
		}

		public IIM413Declaration Declaration => CachedValueHelper.GetValue(ref declarationCached, () => new IM413DeclarationProvider(messageSendingObject));
		CachedValue<IIM413Declaration> declarationCached;

		public IIM413AndIM415GoodsShipment GoodsShipment => CachedValueHelper.GetValue(ref goodsShipmentCached, () => new IM413AndIM415GoodsShipmentProvider((AsycudaBill)messageSendingObject.Bill));
		CachedValue<IIM413AndIM415GoodsShipment> goodsShipmentCached;
	}
}
