using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class RepresentativeWrapperTest : DataProviderTestCase<RepresentativeWrapper>
	{
		public void TestContactPerson()
		{
			AssertEquals("Should return EMailAddress from CusAgent when EMailAddress and PhoneNumber both are available.", "emailAddress", Provider.ContactPerson.EMailAddress);
			AssertEquals("Should return name from CusAgent.", "Name", Provider.ContactPerson.Name);
			AssertEquals("Should return PhoneNumber from CusAgent when EMailAddress and PhoneNumber both are available.", "staffPhone", Provider.ContactPerson.PhoneNumber);

			var declaration = Factory.New<JobDeclaration>();
			var provider = DeclarantWrapper.New(declaration);
			AssertNull("Should return null in case JE_GS_NKCusAgent is empty.", provider.ContactPerson);

			declaration.Branch.GB_Email = "branchEmail";
			declaration.Branch.GB_Phone_Formatted = "branchPhone";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "CUT";
			staff.GS_FullName = "name";
			staff.GS_LoginName = "loginName";
			Factory.Save();
			declaration.JE_GS_NKCusAgent = "CUT";

			AssertEquals("Should return PhoneNumber from branch in case both PhoneNumber and EMailAddress are not present in CusAgent.", "branchPhone", provider.ContactPerson.PhoneNumber);
			AssertEquals("Should return EMailAddress address from branch in case both PhoneNumber and EMailAddress are not present in CusAgent.", "branchEmail", provider.ContactPerson.EMailAddress);
		}

		public void TestIdentificationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var noRepresentativeWrapper = RepresentativeWrapper.New(declaration);

			AssertEquals(string.Empty, noRepresentativeWrapper.IdentificationNumber);

			var representative = Factory.New<OrgHeader>();
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			representative.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			var representativeWrapper = RepresentativeWrapper.New(declaration);

			AssertEquals("FR12345678900001", representativeWrapper.IdentificationNumber);
		}

		public void TestStatus()
		{
			CombineAssertions("Status should be equivalent to RepresentationTypeNo.", () =>
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
				var alternativeProvider1 = RepresentativeWrapper.New(declaration1);
				AssertEquals("Status should be 1 when JE_DeclarantType is SEL.", "1", alternativeProvider1.Status);

				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_DeclarantType = RepresentationTypeList.Codes.DIR;
				var alternativeProvider2 = RepresentativeWrapper.New(declaration2);
				AssertEquals("Status should be 2 when JE_DeclarantType is DIR.", "2", alternativeProvider2.Status);

				var declaration3 = Factory.New<JobDeclaration>();
				declaration3.JE_DeclarantType = RepresentationTypeList.Codes.IND;
				var alternativeProvider3 = RepresentativeWrapper.New(declaration3);
				AssertEquals("Status should be 3 when JE_DeclarantType is IND.", "3", alternativeProvider3.Status);
			});
		}

		protected override RepresentativeWrapper GetProvider()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.RemoveAndDeleteAll();
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			declarant.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "CUS";
			staff.GS_FullName = "Name";
			staff.GS_WorkPhone = "staffPhone";
			var email = staff.EmailAddresses.AddNew();
			email.GSE_EmailAddress = "emailAddress";
			email.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;
			Factory.Save();
			declaration.JE_GS_NKCusAgent = "CUS";

			return RepresentativeWrapper.New(declaration);
		}
	}
}
