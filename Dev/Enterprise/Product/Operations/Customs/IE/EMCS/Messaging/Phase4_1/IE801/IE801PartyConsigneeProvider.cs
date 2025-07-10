using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE801PartyConsigneeProvider : IEMCSPartyConsignee
	{
		public static IE801PartyConsigneeProvider NewOrNull(ConsigneeTraderType consigneeTrader)
			=> consigneeTrader != null ? new IE801PartyConsigneeProvider(consigneeTrader) : null;

		IE801PartyConsigneeProvider(ConsigneeTraderType consigneeTrader)
		{
			this.consigneeTrader = Argument.NotNull(consigneeTrader, nameof(consigneeTrader));
		}
		readonly ConsigneeTraderType consigneeTrader;

		public string EoriNumber => string.Empty;

		public string TraderId => consigneeTrader.Traderid;

		public string Language => consigneeTrader.Language?.ToUpperInvariant();

		public string Name => consigneeTrader.TraderName;

		public string StreetAndNumber => Extensions.GetAddress(consigneeTrader.StreetName, consigneeTrader.StreetNumber);

		public string City => consigneeTrader.City;

		public string Postcode => consigneeTrader.Postcode;

		public string Country => consigneeTrader.Traderid.GetCountryPrefix();
	}
}
