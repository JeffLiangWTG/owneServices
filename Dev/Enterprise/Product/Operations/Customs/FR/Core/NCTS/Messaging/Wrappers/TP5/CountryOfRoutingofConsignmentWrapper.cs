using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CountryOfRoutingofConsignmentWrapper : ICountryOfRoutingofConsignment
	{
		CountryOfRoutingofConsignmentWrapper(string country)
		{
			this.country = Argument.NotNull(country, nameof(country));
		}

		readonly string country;

		public static CountryOfRoutingofConsignmentWrapper New(string country) => string.IsNullOrEmpty(country) ? null : new CountryOfRoutingofConsignmentWrapper(country);

		public string Country => country;
	}
}
