using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class GoodsNotificationAESGoodsShipmentWrapper : IGoodsNotificationAESGoodsShipment
	{
		public GoodsNotificationAESGoodsShipmentWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}
		readonly CusEntryHeader entryHeader;

		public IGoodsNotificationAESConsignment Consignment => consignment ?? (consignment = new GoodsNotificationAESConsignmentWrapper(entryHeader));
		GoodsNotificationAESConsignmentWrapper consignment;
	}
}
