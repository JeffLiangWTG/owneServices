using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class AddressProviderTest : Customs.Business.Testing.DataProviderTestCase<AddressProvider>
{
	public void TestStreetAndNumber()
	{
		orgAddress.OA_Address1 = "4";
		orgAddress.OA_Address2 = "Albert St";
		AssertEquals("4 Albert St", Provider.StreetAndNumber);
	}

	public void TestCountry()
	{
		orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		AssertEquals("BE", Provider.Country);
	}

	public void TestPostcode()
	{
		orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		orgAddress.OA_PostCode = "1140";
		AssertEquals("1140", Provider.Postcode);
	}

	public void TestCity()
	{
		orgAddress.OA_City = "Brussels";
		AssertEquals("Brussels", Provider.City);
	}

	public void TestStreetAndNumberMaxLength()
	{
		AssertEquals("When not in TransitionPeriod, StreetAndNumberMaxLength should be 70", 70, provider.StreetAndNumberMaxLength);

		provider = new AddressProvider(orgAddress, true);
		AssertEquals("When in TransitionPeriod, StreetAndNumberMaxLength should be 35", 35, provider.StreetAndNumberMaxLength);
	}

	protected override AddressProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.New<OrgHeader>();
		orgAddress = orgHeader.MainAddress;
		provider = new AddressProvider(orgAddress);
	}

	OrgHeader orgHeader;
	OrgAddress orgAddress;

	AddressProvider provider;
}
