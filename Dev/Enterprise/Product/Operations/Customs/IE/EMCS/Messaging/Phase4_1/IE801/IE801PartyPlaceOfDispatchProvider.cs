using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE801PartyPlaceOfDispatchProvider : IEMCSPartyPlaceOfDispatch
	{
		public static IE801PartyPlaceOfDispatchProvider NewOrNull(PlaceOfDispatchTraderType placeOfDispatchTrader)
			=> placeOfDispatchTrader != null ? new IE801PartyPlaceOfDispatchProvider(placeOfDispatchTrader) : null;

		IE801PartyPlaceOfDispatchProvider(PlaceOfDispatchTraderType placeOfDispatchTrader)
		{
			this.placeOfDispatchTrader = Argument.NotNull(placeOfDispatchTrader, nameof(placeOfDispatchTrader));
		}
		readonly PlaceOfDispatchTraderType placeOfDispatchTrader;

		public string ReferenceOfTaxWarehouse => placeOfDispatchTrader.ReferenceOfTaxWarehouse;

		public string Language => placeOfDispatchTrader.Language?.ToUpperInvariant();

		public string Name => placeOfDispatchTrader.TraderName;

		public string StreetAndNumber => Extensions.GetAddress(placeOfDispatchTrader.StreetName, placeOfDispatchTrader.StreetNumber);

		public string City => placeOfDispatchTrader.City;

		public string Postcode => placeOfDispatchTrader.Postcode;

		public string Country => placeOfDispatchTrader.ReferenceOfTaxWarehouse.GetCountryPrefix();
	}
}
