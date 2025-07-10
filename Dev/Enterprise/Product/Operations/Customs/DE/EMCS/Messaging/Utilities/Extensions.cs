using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public static class Extensions
	{
		public static ZString GetCountryPrefix(this string countryRegNo)
		{
			var countryPrefix = new ZString(countryRegNo).Left(2).ToUpperInvariant();
			if (countryPrefix == "EL")
			{
				countryPrefix = Core.Constants.CountryCodes.Greece;
			}
			return countryPrefix;
		}

		public static ZString RemoveCountryPrefix(this string countryRegNo) => new ZString(countryRegNo).SubstringSafe(2);

		public static ZString GetAddress(string streetName, string streetNumber) => streetName + " " + streetNumber;
	}
}
