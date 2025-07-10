using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE801PartyTransportArrangerProvider : IEMCSPartyTransporter
	{
		public static IE801PartyTransportArrangerProvider NewOrNull(TransportArrangerTraderType transportArrangerTrader)
			=> transportArrangerTrader != null ? new IE801PartyTransportArrangerProvider(transportArrangerTrader) : null;

		IE801PartyTransportArrangerProvider(TransportArrangerTraderType transportArrangerTrader)
		{
			this.transportArrangerTrader = Argument.NotNull(transportArrangerTrader, nameof(transportArrangerTrader));
		}
		readonly TransportArrangerTraderType transportArrangerTrader;

		public string VatNumber => transportArrangerTrader.VatNumber.RemoveCountryPrefix();

		public string Language => transportArrangerTrader.Language?.ToUpperInvariant();

		public string Name => transportArrangerTrader.TraderName;

		public string StreetAndNumber => Extensions.GetAddress(transportArrangerTrader.StreetName, transportArrangerTrader.StreetNumber);

		public string City => transportArrangerTrader.City;

		public string Postcode => transportArrangerTrader.Postcode;

		public string Country => transportArrangerTrader.VatNumber.GetCountryPrefix();
	}
}
