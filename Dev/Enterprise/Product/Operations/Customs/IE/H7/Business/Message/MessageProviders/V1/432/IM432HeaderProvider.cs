using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM432HeaderProvider : IIM432Header
	{
		public IM432HeaderProvider(MessageSendingObject sendingObject)
		{
			SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		MessageSendingObject SendingObject { get; }

		public IIM432DeclarationType Declaration => CachedValueHelper.GetValue(ref declarationCached, () => new IM432DeclarationTypeProvider((AsycudaBill)SendingObject.Bill));
		CachedValue<IIM432DeclarationType> declarationCached;

		public IIM432GoodsShipmentType GoodsShipment => CachedValueHelper.GetValue(ref goodsShipmentCached, () => new IM432GoodsShipmentTypeProvider((AsycudaBill)SendingObject.Bill));
		CachedValue<IIM432GoodsShipmentType> goodsShipmentCached;
	}
}
