using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonDeclarantWrapperWithContactPersonTest : WrapperHelperTest<AESCommonDeclarantWrapperWithContactPerson>
	{
		public void TestGetNewAESCommonDeclarantWrapper()
		{
			CombineAssertions(() =>
			{
				orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = OrgHeaderData.Code;

				JobDeclaration declaration = null;
				AssertNull("Declaration null", AESCommonDeclarantWrapperWithContactPerson.New(declaration));
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.JE_GB = ZGuid.Empty;
				AssertNull("Declaration no DeclarantAddress null", AESCommonDeclarantWrapperWithContactPerson.New(declaration));
				var address = Factory.New<OrgAddress>();
				declaration.JE_OA_DeclarantAddress = address.PK;
				AssertNull("Declaration DeclarantAddress no Header null", AESCommonDeclarantWrapperWithContactPerson.New(declaration));
				address.OA_OH = Factory.New<OrgHeader>().PK;
				AssertNotNull("Declaration not null", AESCommonDeclarantWrapperWithContactPerson.New(declaration));
			});
		}

		public void TestIdForRepresentativeOrDeclarantOrExporter()
		{
			CombineAssertions(() =>
			{
				var orgHeaderExporter = Factory.New<OrgHeader>();
				orgHeaderExporter.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF111111");
				var orgAddressExporter = Factory.New<OrgAddress>();
				orgAddressExporter.OA_OH = orgHeaderExporter.PK;

				var orgHeaderDeclarant = Factory.New<OrgHeader>();
				orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
				var orgAddressDeclarant = Factory.New<OrgAddress>();
				orgAddressDeclarant.OA_OH = orgHeaderDeclarant.PK;

				var orgHeaderRepresent = Factory.New<OrgHeader>();
				orgHeaderRepresent.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF333333");
				var orgAddressRepresent = Factory.New<OrgAddress>();
				orgAddressRepresent.OA_OH = orgHeaderRepresent.PK;

				declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeaderExporter.PK;
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.JE_OA_Representative = ZGuid.Empty;
				declaration.JE_GB = ZGuid.Empty;

				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
				var wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				AssertEquals("If JE_DeclarantType = 2 or 5 and representative is empty result is exporter", "ESNIF111111", wrapper.Id);

				declaration.JE_OA_Representative = orgAddressRepresent.PK;
				wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				AssertEquals("If JE_DeclarantType = 2 or 5 and representative is not empty and declarant is empty and exporter is not empty the result is exporter", "ESNIF111111", wrapper.Id);

				declaration.JE_OA_Representative = orgAddressRepresent.PK;
				declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
				declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeaderExporter.PK;
				declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddressExporter.PK;
				wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				AssertEquals("If JE_DeclarantType = 2 or 5 and representative is not empty and declarant is not empty and exporter is not empty the result is declarant", "ESNIF222222", wrapper.Id);

				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
				declaration.JE_OA_Representative = ZGuid.Empty;
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				AssertEquals("If JE_DeclarantType <> 2 or 5 and representative is empty and declarant is empty and exporter is not empty the result is exporter", "ESNIF111111", wrapper.Id);

				declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
				wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				AssertEquals("If JE_DeclarantType <> 2 or 5 and representative is empty and declarant is not empty and exporter is not empty the result is declarant", "ESNIF222222", wrapper.Id);

				declaration.JE_OA_Representative = orgAddressRepresent.PK;
				wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				AssertEquals("If JE_DeclarantType <> 2 or 5 and representative is not empty and declarant is not empty and exporter is not empty the result is representative", "ESNIF333333", wrapper.Id);
			});
		}

		public void TestNullContactPerson()
		{
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertNull("Expected null ContactPerson", wrapper);
		}

		public void TestContactPerson()
		{
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
			var contactPerson = wrapper.ContactPerson;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ContactPerson where JE_DeclarantType = 1", contactPerson);
				AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);

				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
				wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				AssertNull("Expected filled ContactPerson where JE_DeclarantType = 2", wrapper);

				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
				wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				AssertNotNull("Expected filled ContactPerson where JE_DeclarantType = 3", wrapper.ContactPerson);

				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._5IndirectATC;
				wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				AssertNull("Expected filled ContactPerson where JE_DeclarantType = 5", wrapper);

				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._4DirectATC;
				wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				AssertNotNull("Expected filled ContactPerson where JE_DeclarantType = 4", wrapper.ContactPerson);
			});
		}

		public void TestContactPerson_ForSupplier()
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress.PK;

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
			var contactPerson = wrapper.ContactPerson;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ContactPerson", contactPerson);
				AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);

				AssertEquals("ContactPerson Email is taken from the correct address, not the main one", "mail@other.com", contactPerson.Email);
			});
		}

		public void TestContactPerson_ForDeclarant()
		{
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
			var contactPerson = wrapper.ContactPerson;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ContactPerson", contactPerson);
				AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);

				AssertEquals("ContactPerson Email is taken from the correct address, not the main one", "mail@other.com", contactPerson.Email);
			});
		}

		public void TestContactPerson_Representative()
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			declaration.JE_OA_Representative = orgAddress.PK;

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
			var contactPerson = wrapper.ContactPerson;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ContactPerson", contactPerson);
				AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);

				AssertEquals("ContactPerson Email is taken from the correct address, not the main one", "mail@other.com", contactPerson.Email);
			});
		}

		public void TestContactPerson_Phone()
		{
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
			orgHeader.MainAddress.OA_Phone = "987654321";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Phone = "123456789";
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;

			wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
			var contactPerson = wrapper.ContactPerson;

			CombineAssertions(() =>
			{
				AssertEquals("Phone is taken from OrgAddress if not empty", "123456789", contactPerson.PhoneNumber);

				orgAddress.OA_Phone = ZString.Empty;
				wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				contactPerson = wrapper.ContactPerson;
				AssertEquals("Phone is taken from MainAddress if OrgAddress Phone is empty", "987654321", contactPerson.PhoneNumber);
			});
		}

		public void TestContactPerson_Email()
		{
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
			orgHeader.MainAddress.OA_Email = "email2@wisetechglobal.com";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Email = "email1@wisetechglobal.com";
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;

			wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
			var contactPerson = wrapper.ContactPerson;

			CombineAssertions(() =>
			{
				AssertEquals("Email is taken from OrgAddress if not empty", "email1@wisetechglobal.com", contactPerson.Email);

				orgAddress.OA_Email = ZString.Empty;
				wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
				contactPerson = wrapper.ContactPerson;
				AssertEquals("Email is taken from MainAddress if OrgAddress Email is empty", "email2@wisetechglobal.com", contactPerson.Email);

				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress("email3@wisetechglobal.com"))
				{
					orgHeader.MainAddress.OA_Email = ZString.Empty;
					wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
					contactPerson = wrapper.ContactPerson;
					AssertEquals("Email is taken from Misc/Declaration Email if OrgAddress and MainAddress Email are empty", "email3@wisetechglobal.com", contactPerson.Email);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.New<OrgHeader>();
			var orgAddressMain = orgHeader.MainAddress;
			orgAddressMain.OA_Email = "mail@mail.com";

			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Email = "mail@other.com";

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			wrapper = AESCommonDeclarantWrapperWithContactPerson.New(declaration);
		}

		OrgHeader orgHeader;
		OrgAddress orgAddress;
		JobDeclaration declaration;
		AESCommonDeclarantWrapperWithContactPerson wrapper;

		protected override AESCommonDeclarantWrapperWithContactPerson GetProvider() => wrapper;
	}
}
