using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE813PartyGuarantorProvider : IEMCSPartyGuarantor
	{
		public static IE813PartyGuarantorProvider NewOrNull(GuarantorTraderType guarantorTrader) => guarantorTrader != null ? new IE813PartyGuarantorProvider(guarantorTrader) : null;

		IE813PartyGuarantorProvider(GuarantorTraderType guarantorTrader)
		{
			this.guarantorTrader = guarantorTrader;
		}
		readonly GuarantorTraderType guarantorTrader;

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
				if (country == null)
				{
					var countryCode = guarantorTrader.TraderExciseNumber.GetCountryPrefix();
					if (countryCode.IsEmpty)
					{
						countryCode = guarantorTrader.VatNumber.GetCountryPrefix();
					}
					country = new CachedValue<string>(() => countryCode);
				}
				return country.Value;
			}
		}
		CachedValue<string> country;

		public string Language => guarantorTrader.Language?.ToUpperInvariant();
	}
}
