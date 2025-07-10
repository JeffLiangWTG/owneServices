using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ImportDeclarantPartyIdWrapperTest : WrapperHelperTest<ImportDeclarantPartyIdWrapper>
	{
		public void TestGetNewImportCommonDeclarantWrapperIfNotNull()
		{
			CombineAssertions(() =>
			{
				orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = OrgHeaderData.Code;

				JobDeclaration declaration = null;
				AssertNull("Declaration null", ImportDeclarantPartyIdWrapper.New(declaration));
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.JE_GB = ZGuid.Empty;
				AssertNull("Declaration no DeclarantAddress null", ImportDeclarantPartyIdWrapper.New(declaration));
				var address = Factory.New<OrgAddress>();
				declaration.JE_OA_DeclarantAddress = address.PK;
				AssertNull("Declaration DeclarantAddress no Header null", ImportDeclarantPartyIdWrapper.New(declaration));
				address.OA_OH = Factory.New<OrgHeader>().PK;
				AssertNotNull("Declaration not null", ImportDeclarantPartyIdWrapper.New(declaration));
			});
		}

		public void TestType()
		{
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = OrgHeaderData.DeclarantType;
				AssertEquals("Expected filled Type", OrgHeaderData.DeclarantType, wrapper.Type);

				declaration.JE_DeclarantType = OrgHeaderData.DeclarantTypeToMap1;
				AssertEquals("Expected filled mapped Type 1", OrgHeaderData.DeclarantTypeMapped1, wrapper.Type);

				declaration.JE_DeclarantType = OrgHeaderData.DeclarantTypeToMap2;
				AssertEquals("Expected filled mapped Type 2", OrgHeaderData.DeclarantTypeMapped2, wrapper.Type);

				declaration.JE_DeclarantType = OrgHeaderData.DeclarantTypeToMap3;
				AssertEquals("Expected filled mapped Type 3", OrgHeaderData.DeclarantTypeMapped3, wrapper.Type);
			});
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
					AssertEquals("Expected filled EmailAddress with clearance email recipient when filled", clearanceEmail, wrapper.EmailAddress);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
					wrapper = ImportDeclarantPartyIdWrapper.New(declaration);
					AssertEquals("Expected filled EmailAddress with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, wrapper.EmailAddress);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
					wrapper = ImportDeclarantPartyIdWrapper.New(declaration);
					AssertEquals("Expected empty EmailAddress when clearance email recipient and mailbox email address are empty", ZString.Empty, wrapper.EmailAddress);
				}
			});
		}

		public void TestIsAuthorized()
		{
			CombineAssertions(() =>
			{
				declaration.ZG_AuthPerDeclaration = false;
				AssertEquals("Expected false IsAuthorized", false, wrapper.IsAuthorized);

				declaration.ZG_AuthPerDeclaration = true;
				AssertEquals("Expected true IsAuthorized", true, wrapper.IsAuthorized);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = OrgHeaderData.Code;

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			wrapper = ImportDeclarantPartyIdWrapper.New(declaration);
		}

		OrgHeader orgHeader;
		JobDeclaration declaration;
		ImportDeclarantPartyIdWrapper wrapper;

		protected override ImportDeclarantPartyIdWrapper GetProvider() => wrapper;
	}
}
