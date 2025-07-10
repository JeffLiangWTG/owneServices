using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class OriginProvider : IOrigin
	{
		OriginProvider(string countryOfOrigin, string countryOfPreferentialOrigin)
		{
			CountryOfOrigin = countryOfOrigin;
			CountryOfPreferentialOrigin = countryOfPreferentialOrigin;
		}

		public static OriginProvider New(string countryOfOrigin, string countryOfPreferentialOrigin)
		{
			if (string.IsNullOrEmpty(countryOfOrigin) && string.IsNullOrEmpty(countryOfPreferentialOrigin))
			{
				return null;
			}
			return new OriginProvider(countryOfOrigin, countryOfPreferentialOrigin);
		}

		public string CountryOfOrigin { get; }

		public string CountryOfPreferentialOrigin { get; }
	}
}
