using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE801PartyTransportArrangerProvider : IEMCSPartyTransporter
	{
		public static IE801PartyTransportArrangerProvider NewOrNull(TransportArrangerTraderType transportArrangerTrader) => transportArrangerTrader != null ? new IE801PartyTransportArrangerProvider(transportArrangerTrader) : null;

		IE801PartyTransportArrangerProvider(TransportArrangerTraderType transportArrangerTrader)
		{
			this.transportArrangerTrader = transportArrangerTrader;
		}
		readonly TransportArrangerTraderType transportArrangerTrader;

		public string VatNumber => transportArrangerTrader.VatNumber.RemoveCountryPrefix();

		public string Language => transportArrangerTrader.Language.ToUpperInvariant();

		public string Name => transportArrangerTrader.TraderName;

		public string Address => Extensions.GetAddress(transportArrangerTrader.StreetName, transportArrangerTrader.StreetNumber);

		public string City => transportArrangerTrader.City;

		public string Postcode => transportArrangerTrader.Postcode;

		public string Country => transportArrangerTrader.VatNumber.GetCountryPrefix();
	}
}
