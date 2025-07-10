using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC511CDataProvider))]
sealed class CC511CDataProviderTest : AESMessageHeaderProviderAbstractTest<CC511CDataProvider>
{
	protected override string MessageType => Constants.BECMessageTypes.Outgoing.CC511C;

	public void TestLRN()
	{
		cusEntryHeader.CH_BGMReference = "LRN";
		AssertEquals("LRN", provider.ExportOperation.LRN);
	}

	public void TestCustomsOfficeOfPresentationReferenceNumber()
	{
		var presentationOffice = jobDeclaration.CustomsOffices.AddNew();
		presentationOffice.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		presentationOffice.CY_Data = "COPres";
		AssertEquals("COPres", provider.CustomsOfficeOfPresentationReferenceNumber);
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

	public void TestCustomsOfficeOfExportReferenceNumber()
	{
		jobDeclaration.JE_CustomsOffice = "COExport";
		AssertEquals("COExport", provider.CustomsOfficeOfExportReferenceNumber);
	}

	public void TestConsignment()
	{
		AssertNotNull(provider.Consignment);
	}

	public void TestDeclarant()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = organisation.MainAddress;
		MessageProviderDataHelper.SetupEORI(orgAddress, "R1234");
		jobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;

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
		MessageProviderDataHelper.SetupOrgheaderAndAddress(organisation, orgAddress);
		jobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;

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
}
