using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class ImportH1CommonDeclarantWrapperWithContactPersonTest : WrapperHelperTest<ImportH1CommonDeclarantWrapperWithContactPerson>
{
	public void TestGetNewAESCommonDeclarantWrapper()
	{
		CombineAssertions(() =>
		{
			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = OrgHeaderData.Code;

			JobDeclaration declaration = null;
			AssertNull("Declaration null", ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration));
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_GB = ZGuid.Empty;
			AssertNull("Declaration no DeclarantAddress null", ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration));
			var address = Factory.New<OrgAddress>();
			declaration.JE_OA_DeclarantAddress = address.PK;
			AssertNull("Declaration DeclarantAddress no Header null", ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration));
			address.OA_OH = Factory.New<OrgHeader>().PK;
			AssertNotNull("Declaration not null", ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration));
		});
	}

	public void TestIdForRepresentativeOrDeclarantOrImporter()
	{
		CombineAssertions(() =>
		{
			var orgHeaderImporter = Factory.New<OrgHeader>();
			orgHeaderImporter.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF111111");
			var orgAddressImporter = Factory.New<OrgAddress>();
			orgAddressImporter.OA_OH = orgHeaderImporter.PK;

			var orgHeaderDeclarant = Factory.New<OrgHeader>();
			orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
			var orgAddressDeclarant = Factory.New<OrgAddress>();
			orgAddressDeclarant.OA_OH = orgHeaderDeclarant.PK;

			var orgHeaderRepresent = Factory.New<OrgHeader>();
			orgHeaderRepresent.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF333333");
			var orgAddressRepresent = Factory.New<OrgAddress>();
			orgAddressRepresent.OA_OH = orgHeaderRepresent.PK;

			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeaderImporter.PK;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_GB = ZGuid.Empty;

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			var wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertEquals("If JE_DeclarantType = 1 and declarant is empty result is importer", "ESNIF111111", wrapper.Id);

			declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertEquals("If JE_DeclarantType = 1 and declarant is not empty result is declarant", "ESNIF222222", wrapper.Id);

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertEquals("If JE_DeclarantType = 2 or 5 and representative is empty result is importer", "ESNIF111111", wrapper.Id);

			declaration.JE_OA_Representative = orgAddressRepresent.PK;
			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertEquals("If JE_DeclarantType = 2 or 5 and representative is not empty and declarant is not empty the result is declarant", "ESNIF222222", wrapper.Id);

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertEquals("If JE_DeclarantType = 2 or 5 and representative is not empty and declarant is empty and importer is not empty the result is importer", "ESNIF111111", wrapper.Id);

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
			declaration.JE_OA_Representative = ZGuid.Empty;
			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertEquals("If JE_DeclarantType <> 2 or 5 and representative is empty and declarant is empty and importer is not empty the result is importer", "ESNIF111111", wrapper.Id);

			declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertEquals("If JE_DeclarantType <> 2 or 5 and representative is empty and declarant is not empty and importer is not empty the result is declarant", "ESNIF222222", wrapper.Id);

			declaration.JE_OA_Representative = orgAddressRepresent.PK;
			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertEquals("If JE_DeclarantType <> 2 or 5 and representative is not empty and declarant is not empty and importer is not empty the result is representative", "ESNIF333333", wrapper.Id);
		});
	}

	public void TestNullContactPerson()
	{
		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
		wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
		AssertNull("Expected null ContactPerson", wrapper);
	}

	public void TestContactPerson()
	{
		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
		wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
		var contactPerson = wrapper.ContactPerson;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled ContactPerson where JE_DeclarantType = 1", contactPerson);
			AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertNull("Expected filled ContactPerson where JE_DeclarantType = 2", wrapper);

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertNotNull("Expected filled ContactPerson where JE_DeclarantType = 3", wrapper.ContactPerson);

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._5IndirectATC;
			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertNull("Expected filled ContactPerson where JE_DeclarantType = 5", wrapper);

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._4DirectATC;
			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			AssertNotNull("Expected filled ContactPerson where JE_DeclarantType = 4", wrapper.ContactPerson);
		});
	}

	public void TestContactPerson_ForImporter()
	{
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OA_Representative = ZGuid.Empty;
		declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
		declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress.PK;

		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
		wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
		var contactPerson = wrapper.ContactPerson;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled ContactPerson", contactPerson);
			AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);

			AssertEquals("ContactPerson Email is taken from the correct address, not the main one", "Name", contactPerson.Name);
		});
	}

	public void TestContactPerson_ForDeclarant()
	{
		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
		wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
		var contactPerson = wrapper.ContactPerson;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled ContactPerson", contactPerson);
			AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);

			AssertEquals("ContactPerson Email is taken from the correct address, not the main one", "Name", contactPerson.Name);
		});
	}

	public void TestContactPerson_Representative()
	{
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
		declaration.JE_OA_Representative = orgAddress.PK;

		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
		wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
		var contactPerson = wrapper.ContactPerson;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled ContactPerson", contactPerson);
			AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);

			AssertEquals("ContactPerson Email is taken from the correct address, not the main one", "Name", contactPerson.Name);
		});
	}

	public void TestContactPerson_Phone()
	{
		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Phone = "123456789";
		declaration.JE_OA_DeclarantAddress = orgAddress.PK;

		wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
		var contactPerson = wrapper.ContactPerson;
		AssertEquals("Phone is taken from OrgAddress if not empty", "123456789", contactPerson.PhoneNumber);
	}

	public void TestContactPerson_Email()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient("email2@wisetechglobal.com"))
		{
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Email = "email1@wisetechglobal.com";
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;

			wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
			var contactPerson = wrapper.ContactPerson;

			CombineAssertions(() =>
			{
				AssertEquals("Email is taken from Misc/Declaration Email only if there is not Other Declaration Email", "email2@wisetechglobal.com", contactPerson.Email);

				declaration.ZG_OtherEmailAddr = "email3@wisetechglobal.com";
				wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
				contactPerson = wrapper.ContactPerson;
				AssertEquals("Email is taken from Misc/Declaration Email + Other Declaration Email", "email2@wisetechglobal.com : email3@wisetechglobal.com", contactPerson.Email);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Name";

		orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_DeclarantAddress = orgAddress.PK;
		wrapper = ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
	}

	OrgHeader orgHeader;
	OrgAddress orgAddress;
	JobDeclaration declaration;
	ImportH1CommonDeclarantWrapperWithContactPerson wrapper;

	protected override ImportH1CommonDeclarantWrapperWithContactPerson GetProvider() => wrapper;
}
