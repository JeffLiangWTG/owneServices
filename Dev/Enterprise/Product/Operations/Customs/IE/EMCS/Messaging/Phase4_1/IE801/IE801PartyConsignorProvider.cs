using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE801PartyConsignorProvider : IEMCSPartyConsignor
	{
		public static IE801PartyConsignorProvider NeworNull(ConsignorTraderType consignorTrader)
			=> consignorTrader != null ? new IE801PartyConsignorProvider(consignorTrader) : null;

		IE801PartyConsignorProvider(ConsignorTraderType consignorTrader)
		{
			this.consignorTrader = Argument.NotNull(consignorTrader, nameof(consignorTrader));
		}
		readonly ConsignorTraderType consignorTrader;

		public string TraderExciseNumber => consignorTrader.TraderExciseNumber;

		public string Language => consignorTrader.Language?.ToUpperInvariant();

		public string Name => consignorTrader.TraderName;

		public string StreetAndNumber => Extensions.GetAddress(consignorTrader.StreetName, consignorTrader.StreetNumber);

		public string City => consignorTrader.City;

		public string Postcode => consignorTrader.Postcode;

		public string Country => consignorTrader.TraderExciseNumber.GetCountryPrefix();
	}
}
