using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE801PartyFirstTransporterProvider : IEMCSPartyTransporter
	{
		public static IE801PartyFirstTransporterProvider NewOrNull(FirstTransporterTraderType firstTransporterTrader) => firstTransporterTrader != null ? new IE801PartyFirstTransporterProvider(firstTransporterTrader) : null;

		IE801PartyFirstTransporterProvider(FirstTransporterTraderType firstTransporterTrader)
		{
			this.firstTransporterTrader = firstTransporterTrader;
		}
		readonly FirstTransporterTraderType firstTransporterTrader;

		public string VatNumber => firstTransporterTrader.VatNumber.RemoveCountryPrefix();

		public string Language => firstTransporterTrader.Language.ToUpperInvariant();

		public string Name => firstTransporterTrader.TraderName;

		public string Address => Extensions.GetAddress(firstTransporterTrader.StreetName, firstTransporterTrader.StreetNumber);

		public string City => firstTransporterTrader.City;

		public string Postcode => firstTransporterTrader.Postcode;

		public string Country => firstTransporterTrader.VatNumber.GetCountryPrefix();
	}
}
