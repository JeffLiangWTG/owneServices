using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	public class TraderWrapper : ITrader
	{
		readonly ZString companyName;
		readonly ZString street;
		readonly ZString postcode;
		readonly ZString city;
		readonly ZString countryCode;
		readonly ZString languageCode;
		readonly ZString configCode;

		public TraderWrapper(ZString companyName, ZString street, ZString postcode, ZString city, ZString countryCode, ZString languageCode, ZString configCode)
		{
			this.companyName = companyName;
			this.street = street;
			this.postcode = postcode;
			this.city = city;
			this.countryCode = countryCode;
			this.languageCode = languageCode;
			this.configCode = configCode;
		}

		ZString ITrader.Name => companyName;

		ZString ITrader.StreetAndNumber => street;

		ZString ITrader.PostalCode => postcode;

		ZString ITrader.City => city;

		ZString ITrader.CountryCode => countryCode;

		ZString ITrader.LanguageCode => languageCode;

		ZString ITrader.ConfigCode => configCode;
	}
}
