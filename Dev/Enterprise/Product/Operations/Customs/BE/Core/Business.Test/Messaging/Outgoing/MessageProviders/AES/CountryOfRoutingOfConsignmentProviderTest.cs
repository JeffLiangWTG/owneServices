using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class CountryOfRoutingOfConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<CountryOfRoutingOfConsignmentProvider>
{
	public void TestSequenceNumber()
	{
		AssertEquals(1, provider.SequenceNumber);
	}

	public void TestCountry()
	{
		country.CY_Code = "BE";
		AssertEquals("BE", provider.Country);
	}

	protected override CountryOfRoutingOfConsignmentProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		country = declaration.ItineraryCountries.AddNew();
		provider = new CountryOfRoutingOfConsignmentProvider(country, 1);
	}

	CountryOfRoutingOfConsignmentProvider provider;
	ItineraryCountry country;
}
