using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE813PartyNewTransportArrangerProvider : IEMCSPartyTransporter
	{
		public static IE813PartyNewTransportArrangerProvider NewOrNull(NewTransportArrangerTraderType newTransportArrangerTrader) => newTransportArrangerTrader != null ? new IE813PartyNewTransportArrangerProvider(newTransportArrangerTrader) : null;

		IE813PartyNewTransportArrangerProvider(NewTransportArrangerTraderType newTransportArrangerTrader)
		{
			this.newTransportArrangerTrader = newTransportArrangerTrader;
		}
		readonly NewTransportArrangerTraderType newTransportArrangerTrader;

		public string VatNumber => newTransportArrangerTrader.VatNumber.RemoveCountryPrefix();

		public string Name => newTransportArrangerTrader.TraderName;

		public string Address => Extensions.GetAddress(newTransportArrangerTrader.StreetName, newTransportArrangerTrader.StreetNumber);

		public string City => newTransportArrangerTrader.City;

		public string Postcode => newTransportArrangerTrader.Postcode;

		public string Country => newTransportArrangerTrader.VatNumber.GetCountryPrefix();

		public string Language => newTransportArrangerTrader.Language.ToUpperInvariant();
	}
}
