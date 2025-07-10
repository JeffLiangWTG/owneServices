using CargoWise.Types;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSAddressExtendedProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSAddressExtendedProvider>
{
	public void TestCountry()
	{
		AssertEquals("BE", provider.Country);
	}

	public void TestPostCode()
	{
		AssertEquals("2940", provider.PostCode);
	}

	public void TestCity()
	{
		AssertEquals("Stabroek", provider.City);
	}

	public void TestStreet()
	{
		AssertEquals("Waterstraat", provider.Street);
	}

	public void TestStreetAdditionalLine()
	{
		AssertEquals("59", provider.StreetAdditionalLine);
	}

	public void TestNumber()
	{
		AssertEquals(ZString.Empty, provider.Number);
	}

	public void TestPoBox()
	{
		AssertEquals(ZString.Empty, provider.PoBox);
	}

	public void TestSubDivision()
	{
		AssertEquals(ZString.Empty, provider.SubDivision);
	}

	protected override PNTSAddressExtendedProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		provider = new PNTSAddressExtendedProvider("Waterstraat", "59", "BE", "2940", "Stabroek");
	}

	PNTSAddressExtendedProvider provider;
}
