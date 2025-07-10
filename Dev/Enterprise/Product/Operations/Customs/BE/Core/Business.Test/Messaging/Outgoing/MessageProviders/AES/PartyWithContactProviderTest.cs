using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PartyWithContactProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyWithContactProvider>
{
	public void TestIdentificationNumber()
	{
		MessageProviderDataHelper.SetupEORI(orgAddress, "R1234");
		AssertEquals("BER1234", provider.IdentificationNumber);
	}

	public void TestName()
	{
		orgHeader.OH_FullName = "BE Declarant";
		AssertEquals("BE Declarant", provider.Name);
	}

	public void TestAddress()
	{
		MessageProviderDataHelper.SetupOrgheaderAndAddress(orgHeader, orgAddress);
		CombineAssertions(() =>
		{
			AssertEquals("StreetAndNumber", "1 test avenue", provider.Address.StreetAndNumber);
			AssertEquals("City", "Brussle", provider.Address.City);
			AssertEquals("Country", Core.Constants.CountryCodes.Belgium, provider.Address.Country);
			AssertEquals("Postcode", "1200", provider.Address.Postcode);
		});
	}

	public void TestContactPerson()
	{
		MessageProviderDataHelper.SetupEORI(orgAddress, "R1234");
		CombineAssertions(() =>
		{
			AssertEquals("Name", GlbStaff.CurrentUser.GS_FullName, provider.ContactPerson.Name);
			AssertEquals("PhoneNumber", GlbStaff.CurrentUser.GS_WorkPhone, provider.ContactPerson.PhoneNumber);
			AssertEquals("Email", GlbStaff.CurrentUser.GS_EmailAddress, provider.ContactPerson.EMailAddress);
		});
	}

	public void TestContactPersonWithoutIdentificationAndAddress()
	{
		AssertNull(provider.ContactPerson);
	}

	public void TestNameMaxlength()
	{
		AssertEquals("When not in TransitionPeriod, StreetAndNumberMaxLength should be 70", 70, provider.NameMaxlength);

		provider = new PartyWithContactProvider(orgAddress, true);
		AssertEquals("When in TransitionPeriod, StreetAndNumberMaxLength should be 35", 35, provider.NameMaxlength);
	}

	protected override PartyWithContactProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		GlbStaff.CurrentUser.GS_WorkPhone = "1234567890";
		orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgAddress = orgHeader.Addresses.AddNew();
		orgAddress.OA_OH = orgHeader.PK;
		provider = new PartyWithContactProvider(orgAddress);
	}
	OrgAddress orgAddress;
	OrgHeader orgHeader;
	PartyWithContactProvider provider;
}
