using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED801PartyConsignorProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyConsignor
	{
		public static ED801PartyConsignorProvider NeworNull(ED801DBodyEadContainerConsignorTrader consignorTrader)
			=> consignorTrader != null ? new ED801PartyConsignorProvider(consignorTrader) : null;

		ED801PartyConsignorProvider(ED801DBodyEadContainerConsignorTrader consignorTrader)
		{
			this.consignorTrader = Argument.NotNull(consignorTrader, nameof(consignorTrader));
		}
		readonly ED801DBodyEadContainerConsignorTrader consignorTrader;

		public string TraderExciseNumber => consignorTrader.TraderExciseNumber;
		public string Name => consignorTrader.TraderName;
		public string Address => Extensions.GetAddress(consignorTrader.StreetName, consignorTrader.StreetNumber);
		public string City => consignorTrader.City;
		public string Postcode => consignorTrader.Postcode;
		public string Country => consignorTrader.TraderExciseNumber.GetCountryPrefix();
		public string Language => consignorTrader.NadLng.ToUpperInvariant();
	}
}
