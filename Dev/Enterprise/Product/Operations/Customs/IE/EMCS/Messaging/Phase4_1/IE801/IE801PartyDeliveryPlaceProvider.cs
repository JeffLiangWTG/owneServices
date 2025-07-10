using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE801PartyDeliveryPlaceProvider : IEMCSPartyDeliveryPlace
	{
		public static IE801PartyDeliveryPlaceProvider NewOrNull(DeliveryPlaceTraderType deliveryPlaceTrader)
			=> deliveryPlaceTrader != null ? new IE801PartyDeliveryPlaceProvider(deliveryPlaceTrader) : null;

		IE801PartyDeliveryPlaceProvider(DeliveryPlaceTraderType deliveryPlaceTrader)
		{
			this.deliveryPlaceTrader = Argument.NotNull(deliveryPlaceTrader, nameof(deliveryPlaceTrader));
		}
		readonly DeliveryPlaceTraderType deliveryPlaceTrader;

		public string TraderId => deliveryPlaceTrader.Traderid;

		public string Language => deliveryPlaceTrader.Language?.ToUpperInvariant();

		public string Name => deliveryPlaceTrader.TraderName;

		public string StreetAndNumber => Extensions.GetAddress(deliveryPlaceTrader.StreetName, deliveryPlaceTrader.StreetNumber);

		public string City => deliveryPlaceTrader.City;

		public string Postcode => deliveryPlaceTrader.Postcode;

		public string Country => deliveryPlaceTrader.Traderid.GetCountryPrefix();
	}
}
