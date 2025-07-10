using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED801PartyGuarantorProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyGuarantor
	{
		public static ED801PartyGuarantorProvider NewOrNull(ED801DBodyEadContainerMovementGuaranteeGuarantorTrader guarantorTrader)
			=> guarantorTrader != null ? new ED801PartyGuarantorProvider(guarantorTrader) : null;

		ED801PartyGuarantorProvider(ED801DBodyEadContainerMovementGuaranteeGuarantorTrader guarantorTrader)
		{
			this.guarantorTrader = Argument.NotNull(guarantorTrader, nameof(guarantorTrader));
		}
		readonly ED801DBodyEadContainerMovementGuaranteeGuarantorTrader guarantorTrader;

		public string TraderExciseNumber => guarantorTrader.TraderExciseNumber;
		public string VatNumber => guarantorTrader.VatNumber.RemoveCountryPrefix();
		public string Name => guarantorTrader.TraderName;
		public string Address => Extensions.GetAddress(guarantorTrader.StreetName, guarantorTrader.StreetNumber);
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
		public string Language => guarantorTrader.NadLng?.ToUpperInvariant() ?? string.Empty;
	}
}
