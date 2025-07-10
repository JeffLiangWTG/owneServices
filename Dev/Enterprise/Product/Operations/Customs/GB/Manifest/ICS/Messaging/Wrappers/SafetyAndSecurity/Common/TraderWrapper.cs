using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Types;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	class TraderWrapper : ITrader
	{
		public TraderWrapper(ZString companyName, ZString street, ZString postcode, ZString city, ZString countryCode, ZString languageCode, ZString configCode, bool sendNameAndAddressOrEori = false)
		{
			if (sendNameAndAddressOrEori)
			{
				if (!configCode.IsEmpty)
				{
					this.configCode = configCode;
				}
				else
				{
					this.companyName = companyName;
					this.street = street;
					this.postcode = postcode;
					this.city = city;
					this.countryCode = countryCode;
					this.languageCode = languageCode;
				}
			}
			else
			{
				this.companyName = companyName;
				this.street = street;
				this.postcode = postcode;
				this.city = city;
				this.countryCode = countryCode;
				this.languageCode = languageCode;
				this.configCode = configCode;
			}
		}

		readonly ZString companyName;
		readonly ZString street;
		readonly ZString postcode;
		readonly ZString city;
		readonly ZString countryCode;
		readonly ZString languageCode;
		readonly ZString configCode;

		public string Name => companyName;

		public string StreetAndNumber => street;

		public string PostalCode => postcode;

		public string City => city;

		public string CountryCode => countryCode;

		public string LanguageCode => languageCode;

		public string ConfigCode => configCode;
	}
}
