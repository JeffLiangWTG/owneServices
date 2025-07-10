using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonRepresentativeWrapperWithContactPersonTest : WrapperHelperTest<CommonRepresentativeWrapperWithContactPerson>
{
	public void TestGetNewCommonRepresentativeWrapper()
	{
		CombineAssertions(() =>
		{
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			AssertNull("Declaration with JE_DeclarantType = 1, result null", CommonRepresentativeWrapperWithContactPerson.New(declaration));

			declaration.JE_OA_Representative = orgAddress.PK;
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			AssertNotNull("Declaration with JE_DeclarantType = 2 with representant, result not null", CommonRepresentativeWrapperWithContactPerson.New(declaration));

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
			AssertNull("Declaration with JE_DeclarantType = 3 with representant, result null", CommonRepresentativeWrapperWithContactPerson.New(declaration));

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertNotNull("JobDeclaration with representant is null and declarant not null and JE_DeclarantType = 2, result not null", CommonRepresentativeWrapperWithContactPerson.New(declaration));

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._5IndirectATC;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNull("JobDeclaration with representant and declarant is null and JE_DeclarantType = 5, result null", CommonRepresentativeWrapperWithContactPerson.New(declaration));

			declaration.JE_OA_Representative = orgAddress.PK;
			AssertNotNull("JobDeclaration with representant is not null and declarant is null and JE_DeclarantType = 5, result not null", CommonRepresentativeWrapperWithContactPerson.New(declaration));

			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			AssertNotNull("JobDeclaration with representant is not null and declarant is not null and JE_DeclarantType = 2, result not null", CommonRepresentativeWrapperWithContactPerson.New(declaration));

			AssertNull("JobDeclaration without DeclarationType", CommonRepresentativeWrapperWithContactPerson.New(Factory.New<JobDeclaration>()));

			AssertNull("JobDeclaration with null", CommonRepresentativeWrapperWithContactPerson.New(null));
		});
	}

	public void TestIdForRepresentativeOrDeclarantWhenRepresentativeTypeEqualTo2Or5()
	{
		CombineAssertions(() =>
		{
			var orgHeaderDeclarant = Factory.New<OrgHeader>();
			orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF111111");
			var orgAddressDeclarant = Factory.New<OrgAddress>();
			orgAddressDeclarant.OA_OH = orgHeaderDeclarant.PK;

			declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
			declaration.JE_GB = ZGuid.Empty;
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			var wrapper = CommonRepresentativeWrapperWithContactPerson.New(declaration);

			AssertEquals("Only with declarant, the values is declarant when JE_DeclarantType = 2", "ESNIF111111", wrapper.Id);

			var orgHeaderRepresent = Factory.New<OrgHeader>();
			orgHeaderRepresent.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
			var orgAddressRepresent = Factory.New<OrgAddress>();
			orgAddressRepresent.OA_OH = orgHeaderRepresent.PK;

			declaration.JE_OA_Representative = orgAddressRepresent.PK;
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			wrapper = CommonRepresentativeWrapperWithContactPerson.New(declaration);
			AssertEquals("With exporter and representative, the values is representative when JE_DeclarantType = 2", "ESNIF222222", wrapper.Id);
		});
	}

	public void TestContactPerson()
	{
		var contactPerson = wrapper.ContactPerson;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled ContactPerson", contactPerson);
			AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);
		});
	}

	public void TestContactPerson_Phone()
	{
		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
		orgHeader.MainAddress.OA_Phone = "987654321";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Phone = "123456789";
		declaration.JE_OA_Representative = orgAddress.PK;

		wrapper = CommonRepresentativeWrapperWithContactPerson.New(declaration);
		var contactPerson = wrapper.ContactPerson;

		CombineAssertions(() =>
		{
			AssertEquals("Phone is taken from OrgAddress if not empty", "123456789", contactPerson.PhoneNumber);

			orgAddress.OA_Phone = ZString.Empty;
			wrapper = CommonRepresentativeWrapperWithContactPerson.New(declaration);
			contactPerson = wrapper.ContactPerson;
			AssertEquals("Phone is taken from MainAddress if OrgAddress Phone is empty", "987654321", contactPerson.PhoneNumber);
		});
	}

	public void TestContactPerson_Email()
	{
		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
		orgHeader.MainAddress.OA_Email = "email2@wisetechglobal.com";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Email = "email1@wisetechglobal.com";
		declaration.JE_OA_Representative = orgAddress.PK;

		wrapper = CommonRepresentativeWrapperWithContactPerson.New(declaration);
		var contactPerson = wrapper.ContactPerson;

		CombineAssertions(() =>
		{
			AssertEquals("Email is taken from OrgAddress if not empty", "email1@wisetechglobal.com", contactPerson.Email);

			orgAddress.OA_Email = ZString.Empty;
			wrapper = CommonRepresentativeWrapperWithContactPerson.New(declaration);
			contactPerson = wrapper.ContactPerson;
			AssertEquals("Email is taken from MainAddress if OrgAddress Email is empty", "email2@wisetechglobal.com", contactPerson.Email);

			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress("email3@wisetechglobal.com"))
			{
				orgHeader.MainAddress.OA_Email = ZString.Empty;
				wrapper = CommonRepresentativeWrapperWithContactPerson.New(declaration);
				contactPerson = wrapper.ContactPerson;
				AssertEquals("Email is taken from Misc/Declaration Email if OrgAddress and MainAddress Email are empty", "email3@wisetechglobal.com", contactPerson.Email);
			}
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		orgAddress = Factory.New<OrgAddress>();
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgAddress.OA_OH = orgHeader.PK;

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_Representative = orgAddress.PK;
		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;

		wrapper = CommonRepresentativeWrapperWithContactPerson.New(declaration);
	}

	OrgAddress orgAddress;
	CommonRepresentativeWrapperWithContactPerson wrapper;
	JobDeclaration declaration;

	protected override CommonRepresentativeWrapperWithContactPerson GetProvider() => wrapper;
}
