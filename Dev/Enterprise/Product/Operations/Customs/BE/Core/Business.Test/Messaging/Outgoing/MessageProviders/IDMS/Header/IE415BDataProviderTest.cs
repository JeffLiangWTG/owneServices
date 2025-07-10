using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.BE.Business.Declaration.CusEntryInstruction;
using JobComInvoiceHeader = Enterprise.Customs.BE.Business.Declaration.JobComInvoiceHeader;
using JobDeclaration = Enterprise.Customs.BE.Business.Declaration.JobDeclaration;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(IE415BDataProvider))]
sealed class IE415BDataProviderTest : MessageHeaderProviderAbstractTest<IE415BDataProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new IE415BDataProvider(null));
	}

	public void TestGoodsShipment()
	{
		AssertNotNull("GoodsItems", Provider.GoodsShipment);
	}

	public void TestImportOperation()
	{
		AssertNotNull(Provider.ImportOperation);
	}

	public void TestAuthorisations()
	{
		instruction.CusAuthorizationUsages.AddNew();
		AssertEquals(1, Provider.Authorisations.Count);
	}

	public void TestCustomsOfficeOfPresentationReferenceNumber()
	{
		var office = jobDeclaration.CustomsOffices.AddNew();
		office.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		office.CY_Data = "PresRef";
		AssertEquals("PresRef", Provider.CustomsOfficeOfPresentationReferenceNumber);
	}

	public void TestSupervisingCustomsOfficeReferenceNumber()
	{
		var office = jobDeclaration.CustomsOffices.AddNew();
		office.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
		office.CY_Data = "SupRef";
		AssertEquals("SupRef", Provider.SupervisingCustomsOfficeReferenceNumber);
	}

	public void TestImporter()
	{
		var address = Factory.New<OrgAddress>();
		jobDeclaration.JE_OA_ImporterAddress = address.PK;
		AssertNotNull(Provider.Importer);
	}

	public void TestDeclarant()
	{
		var address = Factory.New<OrgAddress>();
		jobDeclaration.JE_OA_DeclarantAddress = address.PK;
		AssertNotNull(Provider.Declarant);
	}

	public void TestPersonProvidingAGuaranteeIdentificationNumber()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI", "BE");
		jobDeclaration.DefermentPartyDocAddress.OrganisationPK = orgHeader.PK;
		AssertEquals("BEEORI", provider.PersonProvidingAGuaranteeIdentificationNumber);
	}

	public void TestPersonPayingCustomsDutyIdentificationNumber()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI", "BE");
		jobDeclaration.JE_OH_DutyPayer = orgHeader.PK;
		AssertEquals("BEEORI", provider.PersonPayingCustomsDutyIdentificationNumber);
	}

	public void TestRepresentative()
	{
		var address = Factory.New<OrgAddress>();
		jobDeclaration.JE_OA_Representative = address.PK;
		AssertNotNull(Provider.Representative);
	}

	public void TestGuarantees()
	{
		var guaranteeTYP = instruction.Guarantees.AddNew();
		guaranteeTYP.PW_BondType = "TYP";
		var guaranteeTYPTwo = instruction.Guarantees.AddNew();
		guaranteeTYPTwo.PW_BondType = "TYP";
		var guaranteeXXX = instruction.Guarantees.AddNew();
		guaranteeXXX.PW_BondType = "XXX";
		AssertEquals(2, Provider.Guarantees.Count);
	}

	public void TestInternalCurrencyUnit()
	{
		var invoiceHeader = jobDeclaration.Invoices.First();
		invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
		AssertEquals("EUR", Provider.InternalCurrencyUnit);
	}

	public void TestDeferredPayments()
	{
		AssertEquals(0, Provider.DeferredPayments.Count);
		jobDeclaration.JE_DefermentAccountNumber = "Test";
		AssertEquals(1, new IE415BDataProvider(messageSendingAction).DeferredPayments.Count);
	}

	public void TestPostalCharges()
	{
		AssertExceptionThrown<NotImplementedException>(() => new ZString(Provider.PostalCharges));
	}

	protected override string MessageType => Constants.BECMessageTypes.Outgoing.IE415B;

	protected override IE415BDataProvider GetProvider() => provider;

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
		messageSendingAction = new BEJobDeclarationMessageSendingObjectForTesting(cusEntryHeader);
		provider = new IE415BDataProvider(messageSendingAction);
	}

	JobDocAddress jobDocAddress;
	OrgAddress orgAddress;
	OrgHeader organisation;
	CusEntryInstruction instruction;
	JobComInvoiceHeader jobComInvoiceHeader;
}
