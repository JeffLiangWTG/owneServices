using Enterprise.Integration.Freight;

namespace Enterprise.Customs.CA.Business
{
	public class ShipmentLinkingMessagesSupporterProvider : Integration.Customs.CA.IShipmentLinkingMessagesSupporterProvider
	{
		public Integration.Customs.CA.IShipmentLinkingMessagesSupporter Create(ICommonShipment shipment)
		{
			return new ShipmentLinkingMessagesSupporter(shipment);
		}
	}
}
