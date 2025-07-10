using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class DeclarantWrapperTest : DataProviderTestCase<DeclarantWrapper>
	{
		#region IdentificationNumber

		public void TestIdentificationNumber()
		{
			TestIdentificationNumberWhenDeclarantHasEORI();
			TestIdentificationNumberWhenDeclarantHasNoEORI();
		}
		public void TestIdentificationNumber_ForOperational()
		{
			TestIdentificationNumberWhenDeclarantHasEORI_ForOperational();
			TestIdentificationNumberWhenDeclarantHasNoEORI_ForOperational();
		}

		void TestIdentificationNumberWhenDeclarantHasEORI()
		{
			AssertEquals("IdentificationNumber should equal declarant EORI.", "FR12345678900001", GetAlternateProvider().IdentificationNumber);
		}

		void TestIdentificationNumberWhenDeclarantHasNoEORI()
		{
			AssertEquals("IdentificationNumber should not be mapped when declarant has no EORI.", string.Empty, Provider.IdentificationNumber);
		}

		void TestIdentificationNumberWhenDeclarantHasEORI_ForOperational()
		{
			SetupGlobalBranch();
			AssertEquals("IdentificationNumber should equal declarant EORI.", "FRABC", DeclarantWrapper.New(true).IdentificationNumber);
		}

		void TestIdentificationNumberWhenDeclarantHasNoEORI_ForOperational()
		{
			SetupGlobalBranch();
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			AssertEquals("IdentificationNumber should not be mapped when declarant has no EORI.", string.Empty, DeclarantWrapper.New(true).IdentificationNumber);
		}

		#endregion

		#region Name

		public void TestName()
		{
			TestNameWhenDeclarantHasEORI();
			TestNameWhenDeclarantHasNoEORI();
		}

		public void TestName_ForOperational()
		{
			TestNameWhenDeclarantHasEORI_ForOperational();
			TestNameWhenDeclarantHasNoEORI_ForOperational();
		}

		void TestNameWhenDeclarantHasEORI()
		{
			AssertEquals("Name not be mapped when declarant has EORI.", null, GetAlternateProvider().Name);
		}

		void TestNameWhenDeclarantHasNoEORI()
		{
			AssertEquals("Name should equal declarant OH_FullName.", "Declarant", Provider.Name);
		}

		void TestNameWhenDeclarantHasEORI_ForOperational()
		{
			SetupGlobalBranch();
			AssertEquals("Name not be mapped when declarant has EORI.", null, DeclarantWrapper.New(true).Name);
		}

		void TestNameWhenDeclarantHasNoEORI_ForOperational()
		{
			SetupGlobalBranch();
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			AssertEquals("Name should equal declarant OH_FullName.", "Declarant Global Branch Name", DeclarantWrapper.New(true).Name);
		}

		#endregion

		#region Address

		public void TestAddress()
		{
			TestAddressWhenDeclarantHasEORI();
			TestAddressWhenDeclarantHasNoEORI();
		}

		public void TestAddress_ForOperational()
		{
			TestAddressWhenDeclarantHasEORI_ForOperational();
			TestAddressWhenDeclarantHasNoEORI_ForOperational();
		}

		void TestAddressWhenDeclarantHasEORI()
		{
			AssertNull("Address should not be mapped when Declarant has EORI", GetAlternateProvider().Address);
		}

		void TestAddressWhenDeclarantHasNoEORI()
		{
			var address = Provider.Address;

			CombineAssertions("Address should use OrganisationAddressWrapper when declarant has no EORI.", () =>
			{
				AssertEquals("City should equal OA_City.", "EYRAUD CREMPSE MAURENS", address.City);
				AssertEquals("Country should equal OA_RN_NKCountryCode.", Core.Constants.CountryCodes.France, address.Country);
				AssertEquals("Postcode should equal OA_PostCode.", "24140", address.Postcode);
				AssertEquals("StreetAndNumber should equal OA_Address1 + OA_Address2, coma separated.", "177 Impasse Jane Poupelet, Lescuretie", address.StreetAndNumber);
			});
		}

		void TestAddressWhenDeclarantHasEORI_ForOperational()
		{
			SetupGlobalBranch();
			AssertNull("Address should not be mapped when Declarant has EORI", DeclarantWrapper.New(true).Address);
		}

		void TestAddressWhenDeclarantHasNoEORI_ForOperational()
		{
			SetupGlobalBranch();
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();

			var provider = DeclarantWrapper.New(true);
			var address = provider.Address;

			CombineAssertions("Address should use OrganisationAddressWrapper when declarant has no EORI.", () =>
			{
				AssertEquals("City should equal OA_City.", "Global City", address.City);
				AssertEquals("Country should equal OA_RN_NKCountryCode.", Core.Constants.CountryCodes.France, address.Country);
				AssertEquals("Postcode should equal OA_PostCode.", "56789", address.Postcode);
				AssertEquals("StreetAndNumber should equal OA_Address1 + OA_Address2, coma separated.", "Global Road, Global Building", address.StreetAndNumber);
			});
		}

		#endregion

		#region ContactPerson

		public void TestContactPerson()
		{
			var provider = Provider;
			AssertEquals("Should return EMailAddress from CusAgent when EMailAddress and PhoneNumber both are available.", "emailAddress", provider.ContactPerson.EMailAddress);
			AssertEquals("Should return name from CusAgent.", "Name", provider.ContactPerson.Name);
			AssertEquals("Should return PhoneNumber from CusAgent when EMailAddress and PhoneNumber both are available.", "staffPhone", provider.ContactPerson.PhoneNumber);

			var declaration = Factory.New<JobDeclaration>();
			provider = DeclarantWrapper.New(declaration);
			AssertNull(provider.ContactPerson);

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

		public void TestContactPerson_ForOperational()
		{
			SetupGlobalBranch();
			AssertNull("ContactPerson should not be served when it is for Operational.", DeclarantWrapper.New(true).ContactPerson);
		}

		#endregion

		#region Setup

		protected override DeclarantWrapper GetProvider()
		{
			var declaration = GetDeclaration();

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "CUS";
			staff.GS_FullName = "Name";
			staff.GS_WorkPhone = "staffPhone";
			var email = staff.EmailAddresses.AddNew();
			email.GSE_EmailAddress = "emailAddress";
			email.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;
			declaration.JE_GS_NKCusAgent = "CUS";

			return DeclarantWrapper.New(declaration);
		}

		DeclarantWrapper GetAlternateProvider()
		{
			var declaration = GetDeclaration();
			declaration.Declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			declaration.Declarant.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			return DeclarantWrapper.New(declaration);
		}

		JobDeclaration GetDeclaration()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "Declarant";
			declarant.MainAddress.OA_Address1 = "177 Impasse Jane Poupelet";
			declarant.MainAddress.OA_Address2 = "Lescuretie";
			declarant.MainAddress.OA_PostCode = "24140";
			declarant.MainAddress.OA_City = "EYRAUD CREMPSE MAURENS";
			declarant.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			return declaration;
		}

		void SetupGlobalBranch()
		{
			//this needs refactor
			GlbBranch.CurrentBranch.OrgProxy.OH_FullName = "Declarant Global Branch Name";
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ABC", Core.Constants.CountryCodes.France);
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address1 = "Global Road";
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address2 = "Global Building";
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_PostCode = "56789";
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_City = "Global City";
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		}

		#endregion
	}
}

