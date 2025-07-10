using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonPostcodeAddressWrapperTest : WrapperHelperTest<CommonPostcodeAddressWrapper>
{
	public void TestHouseNumber()
	{
		AssertEquals("Expected filled HouseNumber", "houseNum", wrapper.HouseNumber);
	}

	public void TestPostCode()
	{
		AssertEquals("Expected filled PostCode", "12345", wrapper.PostCode);
	}

	public void TestCountry()
	{
		AssertEquals("Expected filled Country", "ES", wrapper.Country);
	}

	protected override void SetUp()
	{
		base.SetUp();

		wrapper = new CommonPostcodeAddressWrapper("houseNum", "12345", "ES");
	}

	CommonPostcodeAddressWrapper wrapper;

	protected override CommonPostcodeAddressWrapper GetProvider() => wrapper;
}
