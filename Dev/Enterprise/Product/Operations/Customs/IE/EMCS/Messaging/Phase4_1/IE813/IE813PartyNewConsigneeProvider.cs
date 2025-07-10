using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE813;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE813PartyNewConsigneeProvider : IEMCSPartyConsignee
	{
		public static IE813PartyNewConsigneeProvider NewOrNull(NewConsigneeTraderType consigneeTrader)
			=> consigneeTrader != null ? new IE813PartyNewConsigneeProvider(consigneeTrader) : null;

		IE813PartyNewConsigneeProvider(NewConsigneeTraderType consigneeTrader)
		{
			this.consigneeTrader = Argument.NotNull(consigneeTrader, nameof(consigneeTrader));
		}
		readonly NewConsigneeTraderType consigneeTrader;

		public string EoriNumber => string.Empty;
		public string TraderId => consigneeTrader.Traderid;
		public string Name => consigneeTrader.TraderName;
		public string StreetAndNumber => Extensions.GetAddress(consigneeTrader.StreetName, consigneeTrader.StreetNumber);
		public string City => consigneeTrader.City;
		public string Postcode => consigneeTrader.Postcode;
		public string Country => consigneeTrader.Traderid.GetCountryPrefix();
		public string Language => consigneeTrader.Language.ToUpperInvariant();
	}
}
