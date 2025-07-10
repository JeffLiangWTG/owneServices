using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE801PartyConsignorProvider : IEMCSPartyConsignor
	{
		public static IE801PartyConsignorProvider NeworNull(ConsignorTraderType consignorTrader) => consignorTrader != null ? new IE801PartyConsignorProvider(consignorTrader) : null;

		IE801PartyConsignorProvider(ConsignorTraderType consignorTrader)
		{
			this.consignorTrader = consignorTrader;
		}
		readonly ConsignorTraderType consignorTrader;

		public string TraderExciseNumber => consignorTrader.TraderExciseNumber;

		public string Language => consignorTrader.Language.ToUpperInvariant();

		public string Name => consignorTrader.TraderName;

		public string Address => Extensions.GetAddress(consignorTrader.StreetName, consignorTrader.StreetNumber);

		public string City => consignorTrader.City;

		public string Postcode => consignorTrader.Postcode;

		public string Country => consignorTrader.TraderExciseNumber.GetCountryPrefix();
	}
}
