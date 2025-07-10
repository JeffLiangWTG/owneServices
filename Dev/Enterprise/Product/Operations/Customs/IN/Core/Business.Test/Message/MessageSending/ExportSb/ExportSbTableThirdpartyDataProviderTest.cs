using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableThirdpartyDataProviderTest : ExportSbTableThirdpartyDataProviderAbstractClassBase
{
	public override void TestMessageType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestCustomHouseCode()
	{
		declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().CustomHouseCode);
	}

	public override void TestJobNumber()
	{
		header.CH_BGMReference = "1234";
		AssertEquals("1234", CreateDataProvider().JobNumber);
	}

	public override void TestJobDate()
	{
		header.CH_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
		AssertEquals(ZDateTime.BrettsBirthday.ToLocalBranchTime().ToDateTime(), CreateDataProvider().JobDate);
	}

	public override void TestSbNo()
	{
		Instruction.ShippingBillNumber = "1234";
		AssertEquals("1234", CreateDataProvider().SbNo);
	}

	public override void TestSbDate()
	{
		Instruction.ShippingBillDate = ZDateTime.BrettsBirthday;
		AssertEquals(ZDateTime.BrettsBirthday.ToDateTime(), CreateDataProvider().SbDate);
	}

	public override void TestInvoiceSrNumber()
	{
		invoiceHeader.JZ_InvoiceDisplaySequence = 1;
		AssertEquals(1, CreateDataProvider().InvoiceSrNumber);
	}

	public override void TestItemSrNumberInInvoice()
	{
		entryLine.CL_LineNumber = 1;
		AssertEquals(1, CreateDataProvider().ItemSrNumberInInvoice);
	}

	public override void TestIec()
	{
		Assert("to do in future WI", true);
	}

	public override void TestBranchSerialNumber()
	{
		Assert("to do in future WI", true);
	}

	public override void TestExporterName()
	{
		Assert("to do in future WI", true);
	}

	public override void TestExporterAddr1()
	{
		Assert("to do in future WI", true);
	}

	public override void TestExporterAddr2()
	{
		Assert("to do in future WI", true);
	}

	public override void TestCity()
	{
		Assert("to do in future WI", true);
	}

	public override void TestPin()
	{
		Assert("to do in future WI", true);
	}

	public override void TestAmendmentType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestAmendmentNo()
	{
		Assert("to do in future WI", true);
	}

	public override void TestAmendmentDate()
	{
		Assert("to do in future WI", true);
	}

	public override void TestGstnType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestGstnId()
	{
		Assert("to do in future WI", true);
	}

	protected override TableThirdpartyDataProviderAbstractClass CreateDataProvider() => ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProvider).Sb.TableThirdparty.First();

	ExportSbCACHE01AdditionalDataProvider AdditionalDataProvider => new(messageSendingObject);

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;

	CusEntryInstruction Instruction => instruction ??= GetEntryInstruction();
	CusEntryInstruction instruction;

	protected override void SetUp()
	{
		base.SetUp();
		entryLine = header.MergedLines.AddNew();
		declaration = GetJobDeclaration();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = GetInvoiceLine(invoiceHeader);
	}

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		header.CH_JE = jobDeclaration.PK;
		return jobDeclaration;
	}

	JobComInvoiceLine GetInvoiceLine(JobComInvoiceHeader invoiceHeader)
	{
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		return invoiceLine;
	}

	CusEntryInstruction GetEntryInstruction()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		invoiceLine.JI_CEI = entryInstruction.PK;
		header.CH_CEI_Instruction = entryInstruction.PK;
		return invoiceLine.EntryInstruction;
	}
}
