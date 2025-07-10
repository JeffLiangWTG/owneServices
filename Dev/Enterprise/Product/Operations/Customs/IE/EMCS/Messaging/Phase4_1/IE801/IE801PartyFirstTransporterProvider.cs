using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE801PartyFirstTransporterProvider : IEMCSPartyTransporter
	{
		public static IE801PartyFirstTransporterProvider NewOrNull(FirstTransporterTraderType firstTransporterTrader)
			=> firstTransporterTrader != null ? new IE801PartyFirstTransporterProvider(firstTransporterTrader) : null;

		IE801PartyFirstTransporterProvider(FirstTransporterTraderType firstTransporterTrader)
		{
			this.firstTransporterTrader = Argument.NotNull(firstTransporterTrader, nameof(firstTransporterTrader));
		}
		readonly FirstTransporterTraderType firstTransporterTrader;

		public string VatNumber => firstTransporterTrader.VatNumber.RemoveCountryPrefix();

		public string Language => firstTransporterTrader.Language?.ToUpperInvariant();

		public string Name => firstTransporterTrader.TraderName;

		public string StreetAndNumber => Extensions.GetAddress(firstTransporterTrader.StreetName, firstTransporterTrader.StreetNumber);

		public string City => firstTransporterTrader.City;

		public string Postcode => firstTransporterTrader.Postcode;

		public string Country => firstTransporterTrader.VatNumber.GetCountryPrefix();
	}
}
