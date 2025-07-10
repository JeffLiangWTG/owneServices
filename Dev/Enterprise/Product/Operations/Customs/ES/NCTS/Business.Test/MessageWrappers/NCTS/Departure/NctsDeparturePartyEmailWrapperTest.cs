using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsDeparturePartyEmailWrapperTest : WrapperHelperTest<NctsDeparturePartyEmailWrapper>
	{
		public void TestGetNewNCTSAddressInformationDeclarantCommonWrapperIfNotNull_Null()
		{
			AssertNull("Header null", NctsDeparturePartyEmailWrapper.New(null));
		}

		public void TestGetNewNCTSAddressInformationDeclarantCommonWrapperIfNotNull()
		{
			CombineAssertions(() =>
			{
				nctsHeader.DeclarantAddressPK = ZGuid.Empty;
				AssertNull("Header with declarat null", NctsDeparturePartyEmailWrapper.New(nctsHeader));
				nctsHeader.DeclarantAddressPK = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				AssertNotNull("Header with declarant", NctsDeparturePartyEmailWrapper.New(nctsHeader));
			});
		}

		public void TestCorrectOrganizationFromNctsDeparturePartyIdWrapper()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			newBranch.GB_Code = "XAX";
			var orgProxyHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgProxyHeader.OH_FullName = "Org Proxy, S.A.";
			orgProxyHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "11111111", "ES");
			var orgRepresentative = Factory.NewWithValidTestData<OrgHeader>();
			orgRepresentative.OH_FullName = "Representative, S.A.";
			orgRepresentative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "ES");
			var orgPrincipal = Factory.NewWithValidTestData<OrgHeader>();
			orgPrincipal.OH_FullName = "Principal, S.A.";
			orgPrincipal.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "33333333", "ES");
			newBranch.GB_OH_OrgProxy = orgProxyHeader.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK)))
			{
				var newNctsHeader = CreateNctsHeader();
				Factory.Save();

				var wrapper = NctsDeparturePartyEmailWrapper.New(newNctsHeader);

				CombineAssertions(() =>
				{
					AssertEquals("if DeclarantOrg is not empty Organization Id will be DeclarantOrg Id", "ES11111111", wrapper.Id);
					AssertEquals("if DeclarantOrg is not empty Organization Name will be DeclarantOrg Name", newNctsHeader.DeclarantAddress.Header.OH_FullName, wrapper.Name);

					newBranch.GB_OH_OrgProxy = ZGuid.Empty;
					newNctsHeader = CreateNctsHeader();
					newNctsHeader.MovementHeader.Representative.OrganisationPK = orgRepresentative.PK;
					newNctsHeader.Principal.OrganisationPK = orgPrincipal.PK;
					Factory.Save();
					wrapper = NctsDeparturePartyEmailWrapper.New(newNctsHeader);
					AssertEquals("if DeclarantOrg is empty and Representative is not empty Organization Id will be Representative Id", "ES22222222", wrapper.Id);
					AssertEquals("if DeclarantOrg is empty and Representative is not empty Organization Name will be Representative Name", newNctsHeader.MovementHeader.Representative.Address.Header.OH_FullName, wrapper.Name);

					newNctsHeader.MovementHeader.Representative.OrganisationPK = ZGuid.Empty;
					wrapper = NctsDeparturePartyEmailWrapper.New(newNctsHeader);
					AssertEquals("if DeclarantOrg is empty and Representative is empty also Organization Id will be Principal Id", "ES33333333", wrapper.Id);
					AssertEquals("if DeclarantOrg is empty and Representative is empty also Organization Name will be Principal Name", newNctsHeader.Principal.Address.Header.OH_FullName, wrapper.Name);
				});
			}
		}

		public void TestEmailAddress()
		{
			const string clearanceEmail = "mail1.mail@mail.com";
			const string mailboxEmail = "mail2.mail@mail.com";
			CombineAssertions(() =>
			{
				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					var wrapper = NctsDeparturePartyEmailWrapper.New(nctsHeader);
					AssertEquals("Expected filled EmailAddress with clearance email recipient when filled", clearanceEmail, wrapper.EmailAddress);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					nctsHeader = CreateNctsHeader();
					var wrapper = NctsDeparturePartyEmailWrapper.New(nctsHeader);
					AssertEquals("Expected filled EmailAddress with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, wrapper.EmailAddress);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
				{
					nctsHeader = CreateNctsHeader();
					var wrapper = NctsDeparturePartyEmailWrapper.New(nctsHeader);
					AssertEquals("Expected empty EmailAddress when clearance email recipient and mailbox email address are empty", ZString.Empty, wrapper.EmailAddress);
				}
			});
		}

		NctsHeader CreateNctsHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			return nctsHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = CreateNctsHeader();
		}
		NctsHeader nctsHeader;

		protected override NctsDeparturePartyEmailWrapper GetProvider()
		{
			nctsHeader.DeclarantAddressPK = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			return NctsDeparturePartyEmailWrapper.New(nctsHeader);
		}
	}
}
