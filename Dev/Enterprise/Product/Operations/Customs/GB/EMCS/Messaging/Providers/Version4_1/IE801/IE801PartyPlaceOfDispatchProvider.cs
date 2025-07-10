using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE801PartyPlaceOfDispatchProvider : IEMCSPartyPlaceOfDispatch
	{
		public static IE801PartyPlaceOfDispatchProvider NewOrNull(PlaceOfDispatchTraderType placeOfDispatchTrader) => placeOfDispatchTrader != null ? new IE801PartyPlaceOfDispatchProvider(placeOfDispatchTrader) : null;

		IE801PartyPlaceOfDispatchProvider(PlaceOfDispatchTraderType placeOfDispatchTrader)
		{
			this.placeOfDispatchTrader = placeOfDispatchTrader;
		}
		readonly PlaceOfDispatchTraderType placeOfDispatchTrader;

		public string ReferenceOfTaxWarehouse => placeOfDispatchTrader.ReferenceOfTaxWarehouse;

		public string Language => placeOfDispatchTrader.Language.ToUpperInvariant();

		public string Name => placeOfDispatchTrader.TraderName;

		public string Address => Extensions.GetAddress(placeOfDispatchTrader.StreetName, placeOfDispatchTrader.StreetNumber);

		public string City => placeOfDispatchTrader.City;

		public string Postcode => placeOfDispatchTrader.Postcode;

		public string Country => placeOfDispatchTrader.ReferenceOfTaxWarehouse.GetCountryPrefix();
	}
}
