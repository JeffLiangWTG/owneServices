using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED813PartyGuarantorProvider : CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSPartyGuarantor
	{
		public static ED813PartyGuarantorProvider NewOrNull(ED813FBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTrader guarantorTrader)
			=> guarantorTrader != null ? new ED813PartyGuarantorProvider(guarantorTrader) : null;

		ED813PartyGuarantorProvider(ED813FBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTrader guarantorTrader)
		{
			this.guarantorTrader = Argument.NotNull(guarantorTrader, nameof(guarantorTrader));
		}
		readonly ED813FBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTrader guarantorTrader;

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
		public string Language => guarantorTrader.NadLng?.ToUpperInvariant();
	}
}
