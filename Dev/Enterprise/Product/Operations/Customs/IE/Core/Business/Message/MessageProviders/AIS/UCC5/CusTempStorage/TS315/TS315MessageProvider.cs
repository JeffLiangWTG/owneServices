using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class TS315MessageProvider : MessageProvider, ITS315Header
	{
		public TS315MessageProvider(TemporaryStorageMessageSendingObject messageSendingObject) : base()
		{
			messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
			header = messageSendingObject.Header;
		}
		protected readonly TemporaryStorageMessageSendingObject sendingObject;
		protected readonly TemporaryStorageHeader header;

		public ITS313AndTS315DeclarationType Declaration => CachedValueHelper.GetValue(ref declarationCached, () => new TS313AndTS315DeclarationTypeProvider(header));
		CachedValue<ITS313AndTS315DeclarationType> declarationCached;

		public ITS313AndTS315GoodsShipmentType GoodsShipment => CachedValueHelper.GetValue(ref goodsShipmentCached, () => new TS313AndTS315GoodsShipmentTypeProvider(header));
		CachedValue<ITS313AndTS315GoodsShipmentType> goodsShipmentCached;
	}
}
