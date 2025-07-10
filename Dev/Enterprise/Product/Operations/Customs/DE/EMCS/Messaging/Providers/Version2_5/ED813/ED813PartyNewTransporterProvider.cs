using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED813PartyNewTransporterProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyTransporter
	{
		public static ED813PartyNewTransporterProvider NewOrNull(ED813FBodyChangeOfDestinationNewTransporterTrader newTransporterTrader)
			=> newTransporterTrader != null ? new ED813PartyNewTransporterProvider(newTransporterTrader) : null;

		ED813PartyNewTransporterProvider(ED813FBodyChangeOfDestinationNewTransporterTrader newTransporterTrader)
		{
			this.newTransporterTrader = Argument.NotNull(newTransporterTrader, nameof(newTransporterTrader));
		}
		readonly ED813FBodyChangeOfDestinationNewTransporterTrader newTransporterTrader;

		public string VatNumber => newTransporterTrader.VatNumber.RemoveCountryPrefix();
		public string Name => newTransporterTrader.TraderName;
		public string Address => Extensions.GetAddress(newTransporterTrader.StreetName, newTransporterTrader.StreetNumber);
		public string City => newTransporterTrader.City;
		public string Postcode => newTransporterTrader.Postcode;
		public string Country => newTransporterTrader.VatNumber.GetCountryPrefix();
		public string Language => newTransporterTrader.NadLng.ToUpperInvariant();
	}
}
