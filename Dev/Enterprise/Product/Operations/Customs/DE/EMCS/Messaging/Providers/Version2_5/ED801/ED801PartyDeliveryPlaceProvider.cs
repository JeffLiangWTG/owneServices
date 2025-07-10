using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED801PartyDeliveryPlaceProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyDeliveryPlace
	{
		public static ED801PartyDeliveryPlaceProvider NewOrNull(ED801EBodyEadContainerDeliveryPlaceTrader deliveryPlaceTrader)
			=> deliveryPlaceTrader != null ? new ED801PartyDeliveryPlaceProvider(deliveryPlaceTrader) : null;

		ED801PartyDeliveryPlaceProvider(ED801EBodyEadContainerDeliveryPlaceTrader deliveryPlaceTrader)
		{
			this.deliveryPlaceTrader = Argument.NotNull(deliveryPlaceTrader, nameof(deliveryPlaceTrader));
		}
		readonly ED801EBodyEadContainerDeliveryPlaceTrader deliveryPlaceTrader;

		public string TraderId => deliveryPlaceTrader.Traderid;
		public string Name => deliveryPlaceTrader.TraderName;
		public string Address => Extensions.GetAddress(deliveryPlaceTrader.StreetName, deliveryPlaceTrader.StreetNumber);
		public string City => deliveryPlaceTrader.City;
		public string Postcode => deliveryPlaceTrader.Postcode;
		public string Country => deliveryPlaceTrader.Traderid.GetCountryPrefix();
		public string Language => deliveryPlaceTrader.NadLng.ToUpperInvariant();
	}
}
