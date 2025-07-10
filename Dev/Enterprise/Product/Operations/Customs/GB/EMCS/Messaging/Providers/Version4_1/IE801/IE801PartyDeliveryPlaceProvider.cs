using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE801PartyDeliveryPlaceProvider : IEMCSPartyDeliveryPlace
	{
		public static IE801PartyDeliveryPlaceProvider NewOrNull(DeliveryPlaceTraderType deliveryPlaceTrader) => deliveryPlaceTrader != null ? new IE801PartyDeliveryPlaceProvider(deliveryPlaceTrader) : null;

		IE801PartyDeliveryPlaceProvider(DeliveryPlaceTraderType deliveryPlaceTrader)
		{
			this.deliveryPlaceTrader = deliveryPlaceTrader;
		}
		readonly DeliveryPlaceTraderType deliveryPlaceTrader;

		public string TraderId => deliveryPlaceTrader.Traderid;

		public string Language => deliveryPlaceTrader.Language.ToUpperInvariant();

		public string Name => deliveryPlaceTrader.TraderName;

		public string Address => Extensions.GetAddress(deliveryPlaceTrader.StreetName, deliveryPlaceTrader.StreetNumber);

		public string City => deliveryPlaceTrader.City;

		public string Postcode => deliveryPlaceTrader.Postcode;

		public string Country => deliveryPlaceTrader.Traderid.GetCountryPrefix();
	}
}
