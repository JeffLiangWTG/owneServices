using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE813;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE813PartyGuarantorProvider : IEMCSPartyGuarantor
	{
		public static IE813PartyGuarantorProvider NewOrNull(GuarantorTraderType guarantorTrader)
			=> guarantorTrader != null ? new IE813PartyGuarantorProvider(guarantorTrader) : null;

		IE813PartyGuarantorProvider(GuarantorTraderType guarantorTrader)
		{
			this.guarantorTrader = Argument.NotNull(guarantorTrader, nameof(guarantorTrader));
		}
		readonly GuarantorTraderType guarantorTrader;

		public string TraderExciseNumber => guarantorTrader.TraderExciseNumber;
		public string VatNumber => guarantorTrader.VatNumber.RemoveCountryPrefix();
		public string Name => guarantorTrader.TraderName;
		public string StreetAndNumber => Extensions.GetAddress(guarantorTrader.StreetName, guarantorTrader.StreetNumber);
		public string City => guarantorTrader.City;
		public string Postcode => guarantorTrader.Postcode;
		public string Country
		{
			get
			{
				var countryCode = guarantorTrader.TraderExciseNumber.GetCountryPrefix();
				if (countryCode.IsEmpty)
				{
					countryCode = guarantorTrader.VatNumber.GetCountryPrefix();
				}
				return countryCode;
			}
		}
		public string Language => guarantorTrader.Language?.ToUpperInvariant();
	}
}
