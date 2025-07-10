using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class AddressWrapperTest : DataProviderTestCase<AddressWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new AddressWrapper(null));
	}

	public void TestCityName()
	{
		AssertEquals("Deventer", wrapper.CityName);
	}

	public void TestCountryCode()
	{
		AssertEquals(Core.Constants.CountryCodes.Netherlands, wrapper.CountryCode);
	}

	public void TestLine()
	{
		CombineAssertions(() =>
		{
			AssertEquals("OA_Address2 is empty", "Rijksweg 102", wrapper.Line);

			orgAddress.OA_Address2 = "Street";
			AssertEquals("OA_Address2 isn't empty", "Rijksweg 102 Street", wrapper.Line);
		});
	}

	public void TestPostcodeId()
	{
		AssertEquals("7201MG", wrapper.PostcodeId);
	}

	protected override AddressWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		orgAddress = Factory.New<OrgAddress>();
		orgAddress.City = "Deventer";
		orgAddress.Address1 = "Rijksweg 102";
		orgAddress.Postcode = "7201MG";
		wrapper = new AddressWrapper(orgAddress);
	}
	OrgAddress orgAddress;
	AddressWrapper wrapper;
}
