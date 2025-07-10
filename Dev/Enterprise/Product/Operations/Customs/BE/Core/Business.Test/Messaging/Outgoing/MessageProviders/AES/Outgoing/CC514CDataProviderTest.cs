using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC514CDataProvider))]
class CC514CDataProviderTest : AESMessageHeaderProviderAbstractTest<CC514CDataProvider>
{
	protected override string MessageType => Constants.BECMessageTypes.Outgoing.CC514C;

	public void TestCustomsOfficeOfExportReferenceNumber()
	{
		jobDeclaration.JE_CustomsOffice = "R1234567";
		AssertEquals("R1234567", provider.CustomsOfficeOfExportReferenceNumber);
	}

	public void TestExporter()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = organisation.MainAddress;
		var jobDocAddress = jobDeclaration.ExporterDocAddress;
		MessageProviderDataHelper.SetupOrgheaderAndAddress(organisation, orgAddress, string.Empty, "BE Exporter");
		jobDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		jobDocAddress.DocAddressType = Enterprise.MasterFiles.Integration.DocAddressType.Exporter;
		jobDocAddress.E2_OA_Address = orgAddress.PK;
		CombineAssertions(() =>
		{
			AssertEquals("Name", "BE Exporter", provider.Exporter.Name);
			AssertEquals("StreetAndNumber", "1 test avenue", provider.Exporter.Address.StreetAndNumber);
			AssertEquals("City", "Brussle", provider.Exporter.Address.City);
			AssertEquals("Country", Core.Constants.CountryCodes.Belgium, provider.Exporter.Address.Country);
			AssertEquals("Postcode", "1200", provider.Exporter.Address.Postcode);
		});
	}

	public void TestDeclarant()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = organisation.MainAddress;
		jobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;
		MessageProviderDataHelper.SetupEORI(orgAddress, "R1234");
		CombineAssertions(() =>
		{
			AssertEquals("Id", "BER1234", provider.Declarant.IdentificationNumber);
			AssertEquals("Contact Name", GlbStaff.CurrentUser.GS_FullName, provider.Declarant.ContactPerson.Name);
			AssertEquals("Phone", GlbStaff.CurrentUser.GS_WorkPhone, provider.Declarant.ContactPerson.PhoneNumber);
			AssertEquals("Email", GlbStaff.CurrentUser.GS_EmailAddress, provider.Declarant.ContactPerson.EMailAddress);
		});
	}

	public void TestDeclarantWithAddress()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = organisation.MainAddress;
		jobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;
		MessageProviderDataHelper.SetupOrgheaderAndAddress(organisation, orgAddress);
		CombineAssertions(() =>
		{
			AssertEquals("Name", "BE Declarant", provider.Declarant.Name);
			AssertEquals("Street Number", "1 test avenue", provider.Declarant.Address.StreetAndNumber);
			AssertEquals("City", "Brussle", provider.Declarant.Address.City);
			AssertEquals("Country", Core.Constants.CountryCodes.Belgium, provider.Declarant.Address.Country);
			AssertEquals("Postcode", "1200", provider.Declarant.Address.Postcode);
			AssertEquals("Contact Name", GlbStaff.CurrentUser.GS_FullName, provider.Declarant.ContactPerson.Name);
			AssertEquals("Phone", GlbStaff.CurrentUser.GS_WorkPhone, provider.Declarant.ContactPerson.PhoneNumber);
			AssertEquals("Email", GlbStaff.CurrentUser.GS_EmailAddress, provider.Declarant.ContactPerson.EMailAddress);
		});
	}

	public void TestDeclarantContactWithoutIdentificationAndAddress()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = organisation.MainAddress;
		organisation.OH_FullName = string.Empty;
		jobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;
		AssertNull(provider.Declarant.ContactPerson);
	}

	public void TestRepresentative()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = organisation.MainAddress;
		jobDeclaration.JE_OA_Representative = orgAddress.PK;
		MessageProviderDataHelper.SetupEORI(orgAddress, "R1234");
		CombineAssertions(() =>
		{
			AssertEquals("Id", "BER1234", provider.Representative.IdentificationNumber);
			AssertEquals("Status", "2", provider.Representative.Status);
			AssertEquals("Contact Person Name", GlbStaff.CurrentUser.GS_FullName, provider.Representative.ContactPerson.Name);
			AssertEquals("Contact Person Phone", GlbStaff.CurrentUser.GS_WorkPhone, provider.Representative.ContactPerson.PhoneNumber);
			AssertEquals("Contact Person Email", GlbStaff.CurrentUser.GS_EmailAddress, provider.Representative.ContactPerson.EMailAddress);
		});
	}
}
