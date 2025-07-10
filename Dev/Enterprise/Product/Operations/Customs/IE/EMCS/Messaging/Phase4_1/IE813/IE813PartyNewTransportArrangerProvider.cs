using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE813;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE813PartyNewTransportArrangerProvider : IEMCSPartyTransporter
	{
		public static IE813PartyNewTransportArrangerProvider NewOrNull(NewTransportArrangerTraderType newTransportArrangerTrader)
			=> newTransportArrangerTrader != null ? new IE813PartyNewTransportArrangerProvider(newTransportArrangerTrader) : null;

		IE813PartyNewTransportArrangerProvider(NewTransportArrangerTraderType newTransportArrangerTrader)
		{
			this.newTransportArrangerTrader = Argument.NotNull(newTransportArrangerTrader, nameof(newTransportArrangerTrader));
		}
		readonly NewTransportArrangerTraderType newTransportArrangerTrader;

		public string VatNumber => newTransportArrangerTrader.VatNumber.RemoveCountryPrefix();
		public string Name => newTransportArrangerTrader.TraderName;
		public string StreetAndNumber => Extensions.GetAddress(newTransportArrangerTrader.StreetName, newTransportArrangerTrader.StreetNumber);
		public string City => newTransportArrangerTrader.City;
		public string Postcode => newTransportArrangerTrader.Postcode;
		public string Country => newTransportArrangerTrader.VatNumber.GetCountryPrefix();
		public string Language => newTransportArrangerTrader.Language.ToUpperInvariant();
	}
}
