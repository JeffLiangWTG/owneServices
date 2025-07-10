using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class TableSwInfoTypeDataProviderTest : ExportSbTableSwInfoTypeDataProviderAbstractClassBase
{
	public override void TestInfoCode()
	{
		AssertEquals("TBA", CreateDataProvider().InfoCode);
	}

	public override void TestInfoMsr()
	{
		AssertEquals("TBA", CreateDataProvider().InfoMsr);
	}

	public override void TestInfoQualifier()
	{
		AssertEquals("TBA", CreateDataProvider().InfoQualifier);
	}

	public override void TestInfoText()
	{
		AssertEquals("TBA", CreateDataProvider().InfoText);
	}

	public override void TestInfoType()
	{
		AssertEquals("TBA", CreateDataProvider().InfoType);
	}

	public override void TestInfoUqc()
	{
		AssertEquals("TBA", CreateDataProvider().InfoUqc);
	}

	public override void TestInvoiceSrNumber()
	{
		invoice.JZ_InvoiceDisplaySequence = 1;
		AssertEquals(1, CreateDataProvider().InvoiceSrNumber);
	}

	public override void TestItemSrNumberInInvoice()
	{
		entryLine.CL_LineNumber = 1;
		AssertEquals(1, CreateDataProvider().ItemSrNumberInInvoice);
	}

	public override void TestJobDate()
	{
		header.CH_SystemCreateTimeUtc = new ZDateTime(2024, 6, 13);
		AssertEquals(new ZDateTime(2024, 6, 13).ToLocalBranchTime().ToDateTime(), CreateDataProvider().JobDate);
	}

	public override void TestJobNumber()
	{
		header.CH_BGMReference = "1234";
		AssertEquals("1234", CreateDataProvider().JobNumber);
	}

	public override void TestMessageType()
	{
		messageSendingObject.MessageType = DeclarationMessageTypeList.Codes.Fresh;
		AssertEquals(DeclarationMessageTypeList.Codes.Fresh, CreateDataProvider().MessageType);
	}

	public override void TestSbDate()
	{
		AssertEquals("TBA", CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		AssertEquals("TBA", CreateDataProvider().SbNo);
	}

	public override void TestSerialNo()
	{
		AssertEquals("TBA", CreateDataProvider().SerialNo);
	}

	public override void TestSiteId()
	{
		declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().SiteId);
	}

	protected override TableSwInfoTypeDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, new ExportSbCACHE01AdditionalDataProvider(messageSendingObject)).Sb.TableSwInfoType.First();
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.CustomsEntryHeaders.Add(header);
		entryLine = header.MergedLines.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var entryInstruction = Factory.New<CusEntryInstruction>();
		header.CH_CEI_Instruction = entryInstruction.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
}
