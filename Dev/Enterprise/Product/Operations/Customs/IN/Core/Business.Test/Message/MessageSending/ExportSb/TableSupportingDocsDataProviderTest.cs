using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]

sealed class ExportSbTableSupportingdocsDataProviderTest : ExportSbTableSupportingdocsDataProviderAbstractClassBase
{
	public override void TestSerialNo()
	{
		Assert("to do in future WI", true);
	}

	public override void TestImageReferenceNumber()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentTypeCode()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentIssuingPartyCode()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentIssuingPartyNameAddress1()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentIssuingPartyNameAddress2()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentIssuingPartyNameCity()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentIssuingPartyNamePin()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentReferenceNo()
	{
		Assert("to do in future WI", true);
	}

	public override void TestPlaceOfIssue()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentIssueDate()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentExpiryDate()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentBeneficiaryPartyCode()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentBeneficiaryPartyNameAddress1()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentBeneficiaryPartyNameAddress2()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentBeneficiaryPartyNameCity()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentBeneficiaryPartyNamePin()
	{
		Assert("to do in future WI", true);
	}

	public override void TestFileType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentIssuePartyName()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocumentBeneficiaryPartyName()
	{
		Assert("to do in future WI", true);
	}

	public override void TestIcegateId()
	{
		Assert("to do in future WI", true);
	}

	public override void TestSiteId()
	{
		Declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().SiteId);
	}

	public override void TestInvoiceSrNumber()
	{
		InvoiceHeader.JZ_InvoiceDisplaySequence = 1;
		AssertEquals(1, CreateDataProvider().InvoiceSrNumber);
	}

	public override void TestItemSrNumberInInvoice()
	{
		header.MergedLines[0].CL_LineNumber = 1;
		AssertEquals(1, CreateDataProvider().ItemSrNumberInInvoice);
	}

	public override void TestJobDate()
	{
		header.CH_SystemCreateTimeUtc = new CargoWise.Types.ZDateTime(2024, 6, 13);
		AssertEquals(new CargoWise.Types.ZDateTime(2024, 6, 13).ToLocalBranchTime().ToDateTime(), CreateDataProvider().JobDate);
	}

	public override void TestJobNumber()
	{
		header.CH_BGMReference = "1234";
		AssertEquals("1234", CreateDataProvider().JobNumber);
	}

	public override void TestMessageType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestSbDate()
	{
		Assert("to do in future WI", true);
	}

	public override void TestSbNo()
	{
		Assert("to do in future WI", true);
	}

	protected override TableSupportingdocsDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProviderMock.Object).Sb.TableSupportingDocs.First();
	}

	Mock<IExportSbCACHE01AdditionalDataProvider> AdditionalDataProviderMock => additionalDataProviderMock ??= new Mock<IExportSbCACHE01AdditionalDataProvider>();
	Mock<IExportSbCACHE01AdditionalDataProvider> additionalDataProviderMock;

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		header.CH_JE = jobDeclaration.PK;
		return jobDeclaration;
	}

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoiceHeader;

	JobComInvoiceLine InvoiceLine => invoiceLine ??= GetInvoiceLine();
	JobComInvoiceLine invoiceLine;

	JobComInvoiceLine GetInvoiceLine()
	{
		var invoiceLine = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CL = header.MergedLines.AddNew().PK;
		return invoiceLine;
	}

	CusEntryInstruction SetEntryInstruction(JobComInvoiceLine invoiceLine)
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		invoiceLine.JI_CEI = entryInstruction.PK;
		return invoiceLine.EntryInstruction;
	}

	void SetEntryHeader(CusEntryInstruction entryInstruction)
	{
		entryInstruction.CEI_JE = declaration.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
	}

	protected override void SetUp()
	{
		base.SetUp();
		var invoiceLine = InvoiceLine;
		var entryInstruction = SetEntryInstruction(invoiceLine);
		SetEntryHeader(entryInstruction);
	}
}
