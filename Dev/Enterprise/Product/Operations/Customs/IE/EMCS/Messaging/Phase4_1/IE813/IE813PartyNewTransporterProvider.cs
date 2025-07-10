using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE813;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE813PartyNewTransporterProvider : IEMCSPartyTransporter
	{
		public static IE813PartyNewTransporterProvider NewOrNull(NewTransporterTraderType newTransporterTrader)
			=> newTransporterTrader != null ? new IE813PartyNewTransporterProvider(newTransporterTrader) : null;

		IE813PartyNewTransporterProvider(NewTransporterTraderType newTransporterTrader)
		{
			this.newTransporterTrader = Argument.NotNull(newTransporterTrader, nameof(newTransporterTrader));
		}
		readonly NewTransporterTraderType newTransporterTrader;

		public string VatNumber => newTransporterTrader.VatNumber.RemoveCountryPrefix();
		public string Name => newTransporterTrader.TraderName;
		public string StreetAndNumber => Extensions.GetAddress(newTransporterTrader.StreetName, newTransporterTrader.StreetNumber);
		public string City => newTransporterTrader.City;
		public string Postcode => newTransporterTrader.Postcode;
		public string Country => newTransporterTrader.VatNumber.GetCountryPrefix();
		public string Language => newTransporterTrader.Language.ToUpperInvariant();
	}
}
