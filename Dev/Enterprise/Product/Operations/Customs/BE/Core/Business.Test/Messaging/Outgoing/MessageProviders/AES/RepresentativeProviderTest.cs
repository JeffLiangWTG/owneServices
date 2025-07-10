using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class RepresentativeProviderTest : Customs.Business.Testing.DataProviderTestCase<RepresentativeProvider>
{
	public void TestNew()
	{
		MessageProviderDataHelper.SetupEORI(orgAddress, "R1234");
		CombineAssertions(() =>
		{
			provider = RepresentativeProvider.New(null, RepresentationTypeList.Codes._1Self);
			AssertNull(provider);
			provider = RepresentativeProvider.New(jobDeclaration.Representative, RepresentationTypeList.Codes._1Self);
			AssertNotNull(provider);
			provider = RepresentativeProvider.New(jobDeclaration.Representative, RepresentationTypeList.Codes._3Indirect);
			AssertNotNull(provider);
		});
	}

	public void TestIdentificationNumber()
	{
		MessageProviderDataHelper.SetupEORI(orgAddress, "R1234");
		AssertEquals("BER1234", provider.IdentificationNumber);
	}

	public void TestIdentificationNumber_NonBE()
	{
		var code = orgAddress.CustomsCodes.AddNew();
		code.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
		code.OK_CustomsRegNo = "R1234";
		provider = RepresentativeProvider.New(jobDeclaration.Representative, RepresentationTypeList.Codes._2Direct);
		AssertEquals("DER1234", provider.IdentificationNumber);
	}

	public void TestStatus()
	{
		MessageProviderDataHelper.SetupEORI(orgAddress, "R1234");
		CombineAssertions(() =>
		{
			provider = RepresentativeProvider.New(jobDeclaration.Representative, RepresentationTypeList.Codes._1Self);
			AssertNull(provider.Status);
			provider = RepresentativeProvider.New(jobDeclaration.Representative, RepresentationTypeList.Codes._2Direct);
			AssertEquals("2", provider.Status);
			provider = RepresentativeProvider.New(jobDeclaration.Representative, RepresentationTypeList.Codes._3Indirect);
			AssertNull(provider.Status);
			provider = RepresentativeProvider.New(jobDeclaration.Representative, "Unknown");
			AssertNull(provider.Status);
		});
	}

	public void TestName()
	{
		AssertNull(provider.Name);
	}

	public void TestAddress()
	{
		AssertNull(provider.Address);
	}

	public void TestContactPerson()
	{
		MessageProviderDataHelper.SetupEORI(orgAddress, "R1234");
		CombineAssertions(() =>
		{
			AssertEquals("Name", GlbStaff.CurrentUser.GS_FullName, provider.ContactPerson.Name);
			AssertEquals("Phone", GlbStaff.CurrentUser.GS_WorkPhone, provider.ContactPerson.PhoneNumber);
			AssertEquals("Email", GlbStaff.CurrentUser.GS_EmailAddress, provider.ContactPerson.EMailAddress);
		});
	}

	public void TestContactPersonWithoutIdentificationNumber()
	{
		AssertNull(provider.ContactPerson);
	}

	public void TestNameMaxlength()
	{
		AssertEquals("When not in TransitionPeriod, StreetAndNumberMaxLength should be 70", 70, provider.NameMaxlength);

		provider = RepresentativeProvider.New(jobDeclaration.Representative, jobDeclaration.JE_DeclarantType, true);
		AssertEquals("When in TransitionPeriod, StreetAndNumberMaxLength should be 35", 35, provider.NameMaxlength);
	}

	protected override RepresentativeProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		GlbStaff.CurrentUser.GS_WorkPhone = "1234567890";
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgAddress = orgHeader.Addresses.AddNew();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_OA_Representative = orgAddress.PK;
		provider = RepresentativeProvider.New(jobDeclaration.Representative, jobDeclaration.JE_DeclarantType);
	}
	JobDeclaration jobDeclaration;
	OrgAddress orgAddress;
	RepresentativeProvider provider;
}
