using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE813PartyNewTransporterProvider : IEMCSPartyTransporter
	{
		public static IE813PartyNewTransporterProvider NewOrNull(NewTransporterTraderType newTransporterTrader) => newTransporterTrader != null ? new IE813PartyNewTransporterProvider(newTransporterTrader) : null;

		IE813PartyNewTransporterProvider(NewTransporterTraderType newTransporterTrader)
		{
			this.newTransporterTrader = newTransporterTrader;
		}
		readonly NewTransporterTraderType newTransporterTrader;

		public string VatNumber => newTransporterTrader.VatNumber.RemoveCountryPrefix();

		public string Name => newTransporterTrader.TraderName;

		public string Address => Extensions.GetAddress(newTransporterTrader.StreetName, newTransporterTrader.StreetNumber);

		public string City => newTransporterTrader.City;

		public string Postcode => newTransporterTrader.Postcode;

		public string Country => newTransporterTrader.VatNumber.GetCountryPrefix();

		public string Language => newTransporterTrader.Language.ToUpperInvariant();
	}
}
