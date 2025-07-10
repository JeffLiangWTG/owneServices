using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(AddressWrapper))]
public sealed class AddressWrapperTest : DataProviderTestCase<AddressWrapper>
{
	public void TestName()
	{
		AssertEquals("Name should equal Constructor Name.", "WiseTech", Provider.Name);
	}

	public void TestStreetAndNumber()
	{
		AssertEquals("StreetAndNumber should equal Constructor StreetAndNumber.", "25 Bourke Rd Alexandria", Provider.StreetAndNumber);
	}

	public void TestCountry()
	{
		AssertEquals("Country should equal Constructor Country.", "Australia", Provider.Country);
	}

	public void TestZipCode()
	{
		AssertEquals("zipCode should equal Constructor zipCode.", "2015", Provider.ZipCode);
	}

	public void TestCity()
	{
		AssertEquals("City should equal Constructor City.", "Sydney", Provider.City);
	}

	protected override AddressWrapper GetProvider()
	{
		var name = "WiseTech";
		var street1 = "25 Bourke Rd";
		var street2 = "Alexandria";
		var country = "Australia";
		var zipCode = "2015";
		var city = "Sydney";
		return new AddressWrapper(name, street1 + " " + street2, country, zipCode, city);
	}
}
