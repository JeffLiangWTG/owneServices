using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class TS313MessageProvider : MessageProvider, ITS313Header
	{
		public TS313MessageProvider(TemporaryStorageMessageSendingObject sender)
		{
			sendingObject = Argument.NotNull(sender, nameof(sender));
			header = Argument.NotNull(sender.Header, nameof(sender.Header));
		}
		protected readonly TemporaryStorageHeader header;
		protected readonly TemporaryStorageMessageSendingObject sendingObject;

		public ITS313DeclarationType Declaration
			=> CachedValueHelper.GetValue(ref declarationCached, () => new TS313DeclarationTypeProvider(sendingObject));
		CachedValue<ITS313DeclarationType> declarationCached;

		public ITS313AndTS315GoodsShipmentType GoodsShipment
			=> CachedValueHelper.GetValue(ref goodsShipmentCached, () => new TS313AndTS315GoodsShipmentTypeProvider(header));
		CachedValue<ITS313AndTS315GoodsShipmentType> goodsShipmentCached;
	}
}
