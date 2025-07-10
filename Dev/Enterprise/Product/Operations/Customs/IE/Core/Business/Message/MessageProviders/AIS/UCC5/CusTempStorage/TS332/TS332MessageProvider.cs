using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class TS332MessageProvider : MessageProvider, ITS332Header
	{
		public TS332MessageProvider(TemporaryStorageMessageSendingObject sender)
		{
			sendingObject = Argument.NotNull(sender, nameof(sender));
			header = Argument.NotNull(sender.Header, nameof(sender.Header));
		}
		protected readonly TemporaryStorageHeader header;
		protected readonly TemporaryStorageMessageSendingObject sendingObject;

		public ITS332DeclarationType Declaration => CachedValueHelper.GetValue(ref declarationCached, () => TS332DeclarationTypeProvider.New(header));
		CachedValue<ITS332DeclarationType> declarationCached;

		public ITS332GoodsShipmentType GoodsShipment => CachedValueHelper.GetValue(ref goodsShipmentCached, () => TS332GoodsShipmentTypeProvider.New(header));
		CachedValue<ITS332GoodsShipmentType> goodsShipmentCached;
	}
}
