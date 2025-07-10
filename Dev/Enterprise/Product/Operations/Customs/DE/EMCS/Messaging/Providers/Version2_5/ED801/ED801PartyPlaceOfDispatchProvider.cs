using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED801PartyPlaceOfDispatchProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyPlaceOfDispatch
	{
		public static ED801PartyPlaceOfDispatchProvider NewOrNull(ED801EBodyEadContainerPlaceOfDispatchTrader placeOfDispatchTrader)
			=> placeOfDispatchTrader != null ? new ED801PartyPlaceOfDispatchProvider(placeOfDispatchTrader) : null;

		ED801PartyPlaceOfDispatchProvider(ED801EBodyEadContainerPlaceOfDispatchTrader placeOfDispatchTrader)
		{
			this.placeOfDispatchTrader = Argument.NotNull(placeOfDispatchTrader, nameof(placeOfDispatchTrader));
		}
		readonly ED801EBodyEadContainerPlaceOfDispatchTrader placeOfDispatchTrader;

		public string ReferenceOfTaxWarehouse => placeOfDispatchTrader.ReferenceOfTaxWarehouse;
		public string Name => placeOfDispatchTrader.TraderName;
		public string Address => Extensions.GetAddress(placeOfDispatchTrader.StreetName, placeOfDispatchTrader.StreetNumber);
		public string City => placeOfDispatchTrader.City;
		public string Postcode => placeOfDispatchTrader.Postcode;
		public string Country => placeOfDispatchTrader.ReferenceOfTaxWarehouse.GetCountryPrefix();
		public string Language => placeOfDispatchTrader.NadLng.ToUpperInvariant();
	}
}
