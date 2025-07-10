using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED813PartyNewConsigneeProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyConsignee
	{
		public static ED813PartyNewConsigneeProvider NewOrNull(ED813EBodyChangeOfDestinationDestinationChangedNewConsigneeTrader consigneeTrader)
			=> consigneeTrader != null ? new ED813PartyNewConsigneeProvider(consigneeTrader) : null;

		ED813PartyNewConsigneeProvider(ED813EBodyChangeOfDestinationDestinationChangedNewConsigneeTrader consigneeTrader)
		{
			this.consigneeTrader = Argument.NotNull(consigneeTrader, nameof(consigneeTrader));
		}
		readonly ED813EBodyChangeOfDestinationDestinationChangedNewConsigneeTrader consigneeTrader;

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
