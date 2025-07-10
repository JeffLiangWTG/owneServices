using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED813PartyNewTransportArrangerProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyTransporter
	{
		public static ED813PartyNewTransportArrangerProvider NewOrNull(ED813EBodyChangeOfDestinationNewTransportArrangerTrader newTransportArrangerTrader)
			=> newTransportArrangerTrader != null ? new ED813PartyNewTransportArrangerProvider(newTransportArrangerTrader) : null;

		ED813PartyNewTransportArrangerProvider(ED813EBodyChangeOfDestinationNewTransportArrangerTrader newTransportArrangerTrader)
		{
			this.newTransportArrangerTrader = Argument.NotNull(newTransportArrangerTrader, nameof(newTransportArrangerTrader));
		}
		readonly ED813EBodyChangeOfDestinationNewTransportArrangerTrader newTransportArrangerTrader;

		public string VatNumber => newTransportArrangerTrader.VatNumber.RemoveCountryPrefix();
		public string Name => newTransportArrangerTrader.TraderName;
		public string Address => Extensions.GetAddress(newTransportArrangerTrader.StreetName, newTransportArrangerTrader.StreetNumber);
		public string City => newTransportArrangerTrader.City;
		public string Postcode => newTransportArrangerTrader.Postcode;
		public string Country => newTransportArrangerTrader.VatNumber.GetCountryPrefix();
		public string Language => newTransportArrangerTrader.NadLng.ToUpperInvariant();
	}
}
