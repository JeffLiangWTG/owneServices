using System;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.BE.Business.Declaration.CusEntryInstruction;
using JobComInvoiceHeader = Enterprise.Customs.BE.Business.Declaration.JobComInvoiceHeader;
using JobDeclaration = Enterprise.Customs.BE.Business.Declaration.JobDeclaration;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC515CDataProvider))]
class CC515CDataProviderTest : AESMessageHeaderProviderAbstractTest<CC515CDataProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CC515CDataProvider(null));
	}

	public void TestAuthorizations()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
		cusAuthorizationUsage.AGC_Code = "ABCD";
		cusAuthorizationUsage.AGC_Number = "ABC123";
		MessageProviderDataHelper.SetupEORI(orgHeader, "ASD123");
		CombineAssertions(() =>
		{
			AssertEquals("Sequence number", "1", provider.Authorizations.FirstOrDefault().SequenceNumber);
			AssertEquals("Type", "ABCD", provider.Authorizations.FirstOrDefault().Type);
			AssertEquals("Reference number", "ABC123", provider.Authorizations.FirstOrDefault().ReferenceNumber);
			AssertEquals("Holder", "ASD123", provider.Authorizations.FirstOrDefault().HolderOfAuthorisation);
		});
	}

	public void TestCustomsOfficeOfExportReferenceNumber()
	{
		jobDeclaration.JE_CustomsOffice = "R1234567";
		AssertEquals("R1234567", provider.CustomsOfficeOfExportReferenceNumber);
	}

	public void TestCustomsOfficeOfPresentationReferenceNumber()
	{
		var office = jobDeclaration.CustomsOffices.AddNew();
		office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		office.CY_Data = "XYZ1";
		AssertEquals("XYZ1", provider.CustomsOfficeOfPresentationReferenceNumber);
	}

	public void TestCustomsOfficeOfExitDeclaredReferenceNumber()
	{
		var office2 = jobDeclaration.CustomsOffices.AddNew();
		office2.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
		office2.CY_Data = "AZE1";
		AssertEquals("AZE1", provider.CustomsOfficeOfExitDeclaredReferenceNumber);
	}

	public void TestExporter()
	{
		MessageProviderDataHelper.SetupOrgheaderAndAddress(organisation, orgAddress);
		organisation.OH_FullName = "LHKHFBE Declarant";
		CombineAssertions(() =>
		{
			AssertEquals("Name", "LHKHFBE Declarant", provider.Exporter.Name);
			AssertEquals("StreetAndNumber", "1 test avenue", provider.Exporter.Address.StreetAndNumber);
			AssertEquals("City", "Brussle", provider.Exporter.Address.City);
			AssertEquals("Country", Core.Constants.CountryCodes.Belgium, provider.Exporter.Address.Country);
			AssertEquals("Postcode", "1200", provider.Exporter.Address.Postcode);
		});
	}

	public void TestDeclarant()
	{
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
		MessageProviderDataHelper.SetupOrgheaderAndAddress(organisation, orgAddress);
		organisation.OH_FullName = "LHKHFBE Declarant";
		jobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;
		CombineAssertions(() =>
		{
			AssertEquals("Name", "LHKHFBE Declarant", provider.Declarant.Name);
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
		organisation.OH_FullName = string.Empty;
		jobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;
		AssertNull(provider.Declarant.ContactPerson);
	}

	public void TestRepresentative()
	{
		MessageProviderDataHelper.SetupEORI(orgAddress, "R1234");
		jobDeclaration.JE_OA_Representative = orgAddress.PK;
		CombineAssertions(() =>
		{
			AssertEquals("Id", "BER1234", provider.Representative.IdentificationNumber);
			AssertEquals("Status", "2", provider.Representative.Status);
			AssertEquals("Contact Person Name", GlbStaff.CurrentUser.GS_FullName, provider.Representative.ContactPerson.Name);
			AssertEquals("Contact Person Phone", GlbStaff.CurrentUser.GS_WorkPhone, provider.Representative.ContactPerson.PhoneNumber);
			AssertEquals("Contact Person Email", GlbStaff.CurrentUser.GS_EmailAddress, provider.Representative.ContactPerson.EMailAddress);
		});
	}

	public void TestCurrencyExchange()
	{
		jobComInvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
		jobComInvoiceHeader.JZ_InvoiceCurrExRate = 1.234567;
		CombineAssertions(() =>
		{
			AssertEquals("Internal currency unit", "EUR", provider.CurrencyExchange.InternalCurrencyUnit);
			AssertEquals("Exchange rate", new Decimal(1.234567), provider.CurrencyExchange.ExchangeRate);
		});
	}

	public void TestDeferredPayment()
	{
		jobDeclaration.JE_DefermentAccountNumber = "ACC123";
		AssertEquals("Deffered payment", "ACC123", provider.DeferredPayment);
	}

	public void TestGoodsShipment()
	{
		AssertNotNull("GoodsItems", Provider.GoodsShipment);
	}

	public void TestSupervisingCustomsOfficeReferenceNumber()
	{
		var office2 = jobDeclaration.CustomsOffices.AddNew();
		office2.CY_Code = EuOfficeCodesTypes.Codes.SupervisingOffice;
		office2.CY_Data = "AZE1";
		AssertEquals("AZE1", provider.SupervisingCustomsOfficeReferenceNumber);
	}

	protected override string MessageType => Constants.BECMessageTypes.Outgoing.CC515C;

	protected override CC515CDataProvider GetProvider() => provider;

	protected override bool IncludeEntryInstructionAndInvoiceHeader => true;

	protected override void SetUp()
	{
		GlbStaff.CurrentUser.GS_WorkPhone = "1234567890";
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDocAddress = jobDeclaration.ExporterDocAddress;
		instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		jobComInvoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = jobComInvoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		jobDocAddress.DocAddressType = Enterprise.MasterFiles.Integration.DocAddressType.Exporter;
		var lineMerger = new EU.Business.Declaration.LineMerger(jobDeclaration);
		lineMerger.DoMerge();
		cusEntryHeader = jobDeclaration.CustomsEntryHeaders.Single();
		cusEntryHeader.CH_CEI_Instruction = instruction.PK;
		organisation = Factory.NewWithValidTestData<OrgHeader>();
		orgAddress = organisation.MainAddress;
		jobDocAddress.E2_OA_Address = orgAddress.PK;
		messageSendingAction = new ExportEntryMessageSendingAction(cusEntryHeader) { TypeOfEntry = BEExportEntryTypeList.Codes.ExportDeclaration };
		provider = new CC515CDataProvider(messageSendingAction);
	}
	JobDocAddress jobDocAddress;
	OrgAddress orgAddress;
	OrgHeader organisation;
	CusEntryInstruction instruction;
	JobComInvoiceHeader jobComInvoiceHeader;
}
