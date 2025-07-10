using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED801PartyFirstTransporterProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyTransporter
	{
		public static ED801PartyFirstTransporterProvider NewOrNull(ED801DBodyEadContainerFirstTransporterTrader firstTransporterTrader)
			=> firstTransporterTrader != null ? new ED801PartyFirstTransporterProvider(firstTransporterTrader) : null;

		ED801PartyFirstTransporterProvider(ED801DBodyEadContainerFirstTransporterTrader firstTransporterTrader)
		{
			this.firstTransporterTrader = Argument.NotNull(firstTransporterTrader, nameof(firstTransporterTrader));
		}
		readonly ED801DBodyEadContainerFirstTransporterTrader firstTransporterTrader;

		public string VatNumber => firstTransporterTrader.VatNumber.RemoveCountryPrefix();
		public string Name => firstTransporterTrader.TraderName;
		public string Address => Extensions.GetAddress(firstTransporterTrader.StreetName, firstTransporterTrader.StreetNumber);
		public string City => firstTransporterTrader.City;
		public string Postcode => firstTransporterTrader.Postcode;
		public string Country => firstTransporterTrader.VatNumber.GetCountryPrefix();
		public string Language => firstTransporterTrader.NadLng.ToUpperInvariant();
	}
}
