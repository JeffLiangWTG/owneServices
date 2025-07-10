using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(EoriTraderWrapper))]
public sealed class EoriTraderWrapperTest : DataProviderTestCase<EoriTraderWrapper>
{
	public void TestEoriNumber()
	{
		AssertNull("EoriNumber", Provider.EoriNumber);
	}

	public void TestIdentificationNumber()
	{
		AssertEquals("IdentificationNumber.", "123456789", Provider.IdentificationNumber);
	}

	public void TestAddress()
	{
		CombineAssertions("Provider address details", () =>
		{
			AssertEquals("Address Name.", "WiseTech", Provider.Address.Name);
			AssertEquals("Address StreetAndNumber.", "25 Bourke Rd Alexandria", Provider.Address.StreetAndNumber);
			AssertEquals("Address Country.", "Australia", Provider.Address.Country);
			AssertEquals("Address ZipCode.", "2015", Provider.Address.ZipCode);
			AssertEquals("Address City.", "Sydney", Provider.Address.City);
		});
	}

	protected override EoriTraderWrapper GetProvider()
	{
		var identificationNumber = "123456789";
		var address = new AddressWrapper("WiseTech", "25 Bourke Rd Alexandria", "Australia", "2015", "Sydney");

		return new EoriTraderWrapper(identificationNumber, address);
	}
}
