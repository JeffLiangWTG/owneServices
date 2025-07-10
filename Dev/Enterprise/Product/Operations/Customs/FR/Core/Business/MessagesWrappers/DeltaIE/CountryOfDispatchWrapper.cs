using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CountryOfDispatchWrapper : ICountryOfDispatch
	{
		CountryOfDispatchWrapper(string country)
		{
			this.country = Argument.NotNull(country, nameof(country));
		}

		readonly string country;

		public static CountryOfDispatchWrapper New(string country) => country.IsNullOrEmpty() ? null : new CountryOfDispatchWrapper(country);

		public string CountryOfDispatch => countryOfDispatch ?? (countryOfDispatch = country);
		string countryOfDispatch;
	}
}
