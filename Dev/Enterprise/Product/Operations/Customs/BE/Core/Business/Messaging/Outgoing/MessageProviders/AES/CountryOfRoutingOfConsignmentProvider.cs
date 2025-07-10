using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

public class CountryOfRoutingOfConsignmentProvider : ICountryOfRoutingOfConsignment
{
	readonly ItineraryCountry country;
	readonly int sequence;

	public CountryOfRoutingOfConsignmentProvider(ItineraryCountry country, int sequence)
	{
		this.country = Argument.NotNull(country, nameof(country));
		this.sequence = Argument.NotNull(sequence, nameof(sequence));
	}

	public int SequenceNumber => sequence;

	public string Country => country.CY_Code;
}
