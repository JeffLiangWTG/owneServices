using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CountryOfRoutingOfConsignmentProvider : ICountryOfRoutingOfConsignment
{
	public static IEnumerable<CountryOfRoutingOfConsignmentProvider> NewCollection(ICusCodeDataCollection<CountryOfRouting> countriesOfRouting)
	{
		return countriesOfRouting?.Cast<CountryOfRouting>().Select((countryOfRouting) => new CountryOfRoutingOfConsignmentProvider(countryOfRouting));
	}

	CountryOfRoutingOfConsignmentProvider(CountryOfRouting countryOfRouting)
	{
		this.countryOfRouting = countryOfRouting;
	}
	readonly CountryOfRouting countryOfRouting;

	public int SequenceNumber => countryOfRouting.CY_Order;

	public string Country => countryOfRouting.CY_Data;
}
