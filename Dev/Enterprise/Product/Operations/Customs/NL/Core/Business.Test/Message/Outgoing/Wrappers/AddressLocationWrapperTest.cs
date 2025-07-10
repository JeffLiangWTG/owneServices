using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class AddressLocationWrapperTest : DataProviderTestCase<AddressLocationWrapper>
{
	public void TestStreetNumberID()
	{
		AssertEquals("Kerkstraat 1", wrapper.StreetNumberID);
	}

	public void TestCityName()
	{
		AssertNull(wrapper.CityName);
	}

	public void TestCountryCode()
	{
		AssertEquals(Core.Constants.CountryCodes.Netherlands, wrapper.CountryCode);
	}

	public void TestLine()
	{
		AssertNull(wrapper.Line);
	}

	public void TestPostcodeId()
	{
		AssertEquals("1234AB", wrapper.PostcodeId);
	}

	public void TestNullCases()
	{
		var wrapperWithNullAddress = AddressLocationWrapper.New(null);
		AssertNull(wrapperWithNullAddress);
	}

	protected override AddressLocationWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		var goodsLocation = Factory.New<CusGoodsLocation>();
		goodsLocation.CGL_AdditionalIdentifier = "Kerkstraat 1";
		goodsLocation.Address.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		goodsLocation.Address.E2_Postcode = "1234AB";
		wrapper = AddressLocationWrapper.New(goodsLocation);
	}
	AddressLocationWrapper wrapper;
}
