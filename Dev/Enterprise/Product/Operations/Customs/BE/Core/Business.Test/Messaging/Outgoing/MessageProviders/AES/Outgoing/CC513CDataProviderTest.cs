using System.Linq;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC513CDataProvider))]
sealed class CC513CDataProviderTest : AESMessageHeaderProviderAbstractTest<CC513CDataProvider>
{
	protected override string MessageType => Constants.BECMessageTypes.Outgoing.CC513C;

	public void TestAuthorisation()
	{
		AssertType<AuthorizationProvider>("Authorisation", Provider.Authorisation.FirstOrDefault());
	}

	public void TestCustomsOfficeOfPresentationReferenceNumber()
	{
		var officeOfPresentation = jobDeclaration.CustomsOffices.AddNew();
		officeOfPresentation.CY_Code = "PRE";
		officeOfPresentation.CY_Data = "BEANR00001";
		AssertEquals("CustomsOfficeOfPresentationReferenceNumber", "BEANR00001", Provider.CustomsOfficeOfPresentationReferenceNumber);
	}

	public void TestCustomsOfficeOfExportReferenceNumber()
	{
		jobDeclaration.JE_CustomsOffice = "BEANR00003";
		AssertEquals("CustomsOfficeOfExportReferenceNumber", "BEANR00003", Provider.CustomsOfficeOfExportReferenceNumber);
	}

	public void TestCustomsOfficeOfExitReferenceNumber()
	{
		var officeOfExit = jobDeclaration.CustomsOffices.AddNew();
		officeOfExit.CY_Code = "EXT";
		officeOfExit.CY_Data = "BEANR00002";
		AssertEquals("CustomsOfficeOfExitReferenceNumber", "BEANR00002", Provider.CustomsOfficeOfExitReferenceNumber);
	}

	public void TestExporter()
	{
		var exporter = Factory.New<OrgHeader>();
		exporter.OH_FullName = "Exporter";
		jobDeclaration.JE_OH_Exporter = exporter.PK;

		AssertType<PartyProvider>("Exporter", Provider.Exporter);
	}

	public void TestDeclarant()
	{
		var declarant = jobDeclaration.Declarant;
		declarant.CompanyName = "Declarant";

		AssertType<PartyWithContactProvider>("Declarant", Provider.Declarant);
	}

	public void TestRepresentative()
	{
		var representative = Factory.New<OrgHeader>();
		representative.OH_FullName = "Representative";

		AssertType<RepresentativeProvider>("Representative", Provider.Representative);
	}

	public void TestCurrencyExchange()
	{
		AssertType<CurrencyExchangeProvider>("CurrencyExchange", Provider.CurrencyExchange);
	}

	public void TestDeferredPayment()
	{
		jobDeclaration.JE_DefermentAccountNumber = "BE0446101317";
		AssertEquals("DeferredPayment", "BE0446101317", Provider.DeferredPayment);
	}

	public void TestConsignment()
	{
		AssertNull("Consignment", Provider.Consignment);
	}

	public void TestGoodsShipment()
	{
		AssertNotNull("GoodsShipment", Provider.GoodsShipment);
	}

	protected override void SetUp()
	{
		jobDeclaration = Factory.New<JobDeclaration>();
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		invoiceHeader.JZ_InvoiceNumber = "ABC123";
		invoiceHeader.InvoiceLines.AddNew();

		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "H1";
		var authorisation = entryInstruction.CusAuthorizationUsages.AddNew();
		authorisation.AGC_Number = "1";

		var lineMerger = new EU.Business.Declaration.LineMerger(jobDeclaration);
		lineMerger.DoMerge();

		cusEntryHeader = jobDeclaration.CustomsEntryHeaders.Single();
		cusEntryHeader.CH_BGMReference = "11R1234000000009";
		cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;

		messageSendingAction = new ExportEntryMessageSendingAction(cusEntryHeader) { TypeOfEntry = BEExportEntryTypeList.Codes.ExportDeclaration };
		provider = new CC513CDataProvider(messageSendingAction);
	}
}
