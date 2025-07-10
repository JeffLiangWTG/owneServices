using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

class PartyProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyProvider>
{
	protected override PartyProvider GetProvider() => providerWithJobDocAddress;

	public void TestIdentificationNumber_EORI()
	{
		MessageProviderDataHelper.SetupEORI(orgHeader, "EORI", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
		MessageProviderDataHelper.SetupEORI(orgHeader, "TCU", OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU);
		AssertEquals("BEEORI", providerWithJobDocAddress.IdentificationNumber);
		AssertEquals("BEEORI", providerWithOrgAddress.IdentificationNumber);
	}

	public void TestIdentificationNumber_TCU()
	{
		MessageProviderDataHelper.SetupEORI(orgHeader, "TCU", OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU);
		AssertEquals("BETCU", providerWithJobDocAddress.IdentificationNumber);
		AssertEquals("BETCU", providerWithOrgAddress.IdentificationNumber);
	}

	public void TestName()
	{
		jobDocAddress.Organisation.OH_FullName = "Name";
		AssertEquals("Name", providerWithJobDocAddress.Name);
		AssertEquals("Name", providerWithOrgAddress.Name);
	}

	public void TestNameWhenIdentificationNumber()
	{
		MessageProviderDataHelper.SetupEORI(orgHeader, "EORI", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
		jobDocAddress.Organisation.OH_FullName = "Name";
		AssertNull(providerWithJobDocAddress.Name);
		AssertNull(providerWithOrgAddress.Name);
	}

	public void TestAddress()
	{
		jobDocAddress.Organisation.OH_FullName = "Name";
		AssertNotNull(providerWithJobDocAddress.Address);
		AssertNotNull(providerWithOrgAddress.Address);
	}

	public void TestAddressWhenIdentificationNumber()
	{
		MessageProviderDataHelper.SetupEORI(orgHeader, "EORI", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
		AssertNull(providerWithJobDocAddress.Address);
		AssertNull(providerWithOrgAddress.Address);
	}

	public void TestContactPerson()
	{
		AssertNull(providerWithJobDocAddress.ContactPerson);
		AssertNull(providerWithOrgAddress.ContactPerson);
	}

	public void TestNameMaxlength()
	{
		var provider = new PartyProvider(jobDocAddress);

		AssertEquals("When not in TransitionPeriod, StreetAndNumberMaxLength should be 70", 70, provider.NameMaxlength);

		provider = new PartyProvider(jobDocAddress, true);
		AssertEquals("When in TransitionPeriod, StreetAndNumberMaxLength should be 35", 35, provider.NameMaxlength);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var orgAddress = Factory.New<OrgAddress>();

		jobDocAddress = Factory.New<JobDocAddress>();
		orgHeader = Factory.New<OrgHeader>();

		jobDocAddress.E2_OA_Address = orgAddress.PK;
		orgAddress.OA_OH = orgHeader.PK;
		jobDocAddress.OrganisationPK = orgHeader.PK;

		providerWithJobDocAddress = new PartyProvider(jobDocAddress);
		providerWithOrgAddress = new PartyProvider(jobDocAddress.Address);
	}

	JobDocAddress jobDocAddress;
	OrgHeader orgHeader;
	PartyProvider providerWithJobDocAddress;
	PartyProvider providerWithOrgAddress;
}
