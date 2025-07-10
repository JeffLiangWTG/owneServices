using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED801PartyConsigneeProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyConsignee
	{
		public static ED801PartyConsigneeProvider NewOrNull(ED801DBodyEadContainerConsigneeTrader consigneeTrader)
			=> consigneeTrader != null ? new ED801PartyConsigneeProvider(consigneeTrader) : null;

		ED801PartyConsigneeProvider(ED801DBodyEadContainerConsigneeTrader consigneeTrader)
		{
			this.consigneeTrader = Argument.NotNull(consigneeTrader, nameof(consigneeTrader));
		}
		readonly ED801DBodyEadContainerConsigneeTrader consigneeTrader;

		public string EoriNumber => string.Empty;
		public string TraderId => consigneeTrader.Traderid;
		public string Name => consigneeTrader.TraderName;
		public string Address => Extensions.GetAddress(consigneeTrader.StreetName, consigneeTrader.StreetNumber);
		public string City => consigneeTrader.City;
		public string Postcode => consigneeTrader.Postcode;
		public string Country => consigneeTrader.Traderid.GetCountryPrefix();
		public string Language => consigneeTrader.NadLng.ToUpperInvariant();
	}
}
