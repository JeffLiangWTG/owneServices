using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE801PartyConsigneeProvider : IEMCSPartyConsignee
	{
		public static IE801PartyConsigneeProvider NewOrNull(ConsigneeTraderType consigneeTrader) => consigneeTrader != null ? new IE801PartyConsigneeProvider(consigneeTrader) : null;

		IE801PartyConsigneeProvider(ConsigneeTraderType consigneeTrader)
		{
			this.consigneeTrader = consigneeTrader;
		}
		readonly ConsigneeTraderType consigneeTrader;

		public string EoriNumber => string.Empty;

		public string TraderId => consigneeTrader.Traderid;

		public string Language => consigneeTrader.Language.ToUpperInvariant();

		public string Name => consigneeTrader.TraderName;

		public string Address => Extensions.GetAddress(consigneeTrader.StreetName, consigneeTrader.StreetNumber);

		public string City => consigneeTrader.City;

		public string Postcode => consigneeTrader.Postcode;

		public string Country => consigneeTrader.Traderid.GetCountryPrefix();
	}
}
