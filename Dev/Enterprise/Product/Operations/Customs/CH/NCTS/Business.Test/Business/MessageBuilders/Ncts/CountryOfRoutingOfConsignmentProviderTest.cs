using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class CountryOfRoutingOfConsignmentProviderTest : TestCaseWithFactory
{
	public void TestNewCollection()
	{
		AssertNull("null", CountryOfRoutingOfConsignmentProvider.NewCollection(null));
	}

	public void TestSequenceNumber()
	{
		NctsHeader.CountriesOfRouting.AddNew().CY_Code = "COR1";
		NctsHeader.CountriesOfRouting.AddNew().CY_Code = "COR2";
		NctsHeader.CountriesOfRouting.AddNew().CY_Code = "COR3";
		var dataProviders = CountryOfRoutingOfConsignmentProvider.NewCollection(NctsHeader.CountriesOfRouting);
		CombineAssertions(() =>
		{
			AssertEquals("COR1", 1, dataProviders.ElementAt(0).SequenceNumber);
			AssertEquals("COR2", 2, dataProviders.ElementAt(1).SequenceNumber);
			AssertEquals("COR3", 3, dataProviders.ElementAt(2).SequenceNumber);
		});
	}

	public void TestCountry()
	{
		var countryOfRouting = NctsHeader.CountriesOfRouting.AddNew();
		countryOfRouting.CY_Data = Core.Constants.CountryCodes.Switzerland;
		var dataProvider = CreateDataProviders().First();
		AssertEquals("Country", Core.Constants.CountryCodes.Switzerland, dataProvider.Country);
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		return nctsHeader;
	}

	IEnumerable<CountryOfRoutingOfConsignmentProvider> CreateDataProviders() => CountryOfRoutingOfConsignmentProvider.NewCollection(NctsHeader.CountriesOfRouting);
}
