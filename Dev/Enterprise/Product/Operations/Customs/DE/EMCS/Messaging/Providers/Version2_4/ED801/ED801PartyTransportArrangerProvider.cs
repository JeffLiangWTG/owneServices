using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED801PartyTransportArrangerProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyTransporter
	{
		public static ED801PartyTransportArrangerProvider NewOrNull(ED801DBodyEadContainerTransportArrangerTrader transportArrangerTrader)
			=> transportArrangerTrader != null ? new ED801PartyTransportArrangerProvider(transportArrangerTrader) : null;

		ED801PartyTransportArrangerProvider(ED801DBodyEadContainerTransportArrangerTrader transportArrangerTrader)
		{
			this.transportArrangerTrader = Argument.NotNull(transportArrangerTrader, nameof(transportArrangerTrader));
		}
		readonly ED801DBodyEadContainerTransportArrangerTrader transportArrangerTrader;

		public string VatNumber => transportArrangerTrader.VatNumber.RemoveCountryPrefix();
		public string Name => transportArrangerTrader.TraderName;
		public string Address => Extensions.GetAddress(transportArrangerTrader.StreetName, transportArrangerTrader.StreetNumber);
		public string City => transportArrangerTrader.City;
		public string Postcode => transportArrangerTrader.Postcode;
		public string Country => transportArrangerTrader.VatNumber.GetCountryPrefix();
		public string Language => transportArrangerTrader.NadLng.ToUpperInvariant();
	}
}
