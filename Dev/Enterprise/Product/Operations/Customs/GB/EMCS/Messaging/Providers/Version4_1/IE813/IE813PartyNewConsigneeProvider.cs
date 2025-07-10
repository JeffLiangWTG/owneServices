using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE813PartyNewConsigneeProvider : IEMCSPartyConsignee
	{
		public static IE813PartyNewConsigneeProvider NewOrNull(NewConsigneeTraderType consigneeTrader) => consigneeTrader != null ? new IE813PartyNewConsigneeProvider(consigneeTrader) : null;

		IE813PartyNewConsigneeProvider(NewConsigneeTraderType consigneeTrader)
		{
			this.consigneeTrader = consigneeTrader;
		}
		readonly NewConsigneeTraderType consigneeTrader;

		public string EoriNumber => string.Empty;

		public string TraderId => consigneeTrader.Traderid;

		public string Name => consigneeTrader.TraderName;

		public string Address => Extensions.GetAddress(consigneeTrader.StreetName, consigneeTrader.StreetNumber);

		public string City => consigneeTrader.City;

		public string Postcode => consigneeTrader.Postcode;

		public string Country => consigneeTrader.Traderid.GetCountryPrefix();

		public string Language => consigneeTrader.Language.ToUpperInvariant();
	}
}
