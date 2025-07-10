using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED813PartyDeliveryPlaceProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyDeliveryPlace
	{
		public static ED813PartyDeliveryPlaceProvider NewOrNull(ED813FBodyChangeOfDestinationDestinationChangedDeliveryPlaceTrader deliveryPlaceTrader)
			=> deliveryPlaceTrader != null ? new ED813PartyDeliveryPlaceProvider(deliveryPlaceTrader) : null;

		ED813PartyDeliveryPlaceProvider(ED813FBodyChangeOfDestinationDestinationChangedDeliveryPlaceTrader deliveryPlaceTrader)
		{
			this.deliveryPlaceTrader = Argument.NotNull(deliveryPlaceTrader, nameof(deliveryPlaceTrader));
		}
		readonly ED813FBodyChangeOfDestinationDestinationChangedDeliveryPlaceTrader deliveryPlaceTrader;

		public string TraderId => deliveryPlaceTrader.Traderid;
		public string Name => deliveryPlaceTrader.TraderName;
		public string Address => Extensions.GetAddress(deliveryPlaceTrader.StreetName, deliveryPlaceTrader.StreetNumber);
		public string City => deliveryPlaceTrader.City;
		public string Postcode => deliveryPlaceTrader.Postcode;
		public string Country => deliveryPlaceTrader.Traderid.GetCountryPrefix();
		public string Language => deliveryPlaceTrader.NadLng?.ToUpperInvariant();
	}
}
