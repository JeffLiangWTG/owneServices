using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableJobworkDataProviderTest : ExportSbTableJobworkDataProviderAbstractClassBase
{
	public override void TestAmendmentDate()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentDate);
	}

	public override void TestAmendmentNo()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentNo);
	}

	public override void TestAmendmentType()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentType);
	}

	public override void TestBeDate()
	{
		AssertNull(CreateDataProvider().BeDate);

		JobWork.CSI_DateOfIssue = new ZDateTime(2024, 6, 13);
		AssertEquals(new ZDateTime(2024, 6, 13), CreateDataProvider().BeDate);
	}

	public override void TestBeInvoiceNumber()
	{
		AssertEquals("TBA", CreateDataProvider().BeInvoiceNumber);
	}

	public override void TestBeInvoiceSerialNo()
	{
		JobWork.CSI_ReferenceNumber2 = "23";
		AssertEquals("23", CreateDataProvider().BeInvoiceSerialNo);
	}

	public override void TestBeItemNumber()
	{
		JobWork.CSI_ItemNumber = 1234;
		AssertEquals(1234, CreateDataProvider().BeItemNumber);
	}

	public override void TestBeNumber()
	{
		JobWork.CSI_ReferenceNumber = "1234567";
		AssertEquals("1234567", CreateDataProvider().BeNumber);
	}

	public override void TestBePortCode()
	{
		JobWork.CSI_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().BePortCode);
	}

	public override void TestBeQtyUsed()
	{
		JobWork.CSI_Quantity = 1234.002m;
		AssertEquals(1234.002m, CreateDataProvider().BeQtyUsed);
	}

	public override void TestCustomHouseCode()
	{
		AssertEquals("INBLR", CreateDataProvider().CustomHouseCode);
	}

	public override void TestInvoiceSrNumber()
	{
		Invoice.JZ_InvoiceDisplaySequence = 1;
		AssertEquals(1, CreateDataProvider().InvoiceSrNumber);
	}

	public override void TestItemSrNumberInInvoice()
	{
		EntryLine.CL_LineNumber = 1;
		AssertEquals(1, CreateDataProvider().ItemSrNumberInInvoice);
	}

	public override void TestJobDate()
	{
		AssertNull(CreateDataProvider().BeDate);

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

	public override void TestQtyUnits()
	{
		JobWork.CSI_UnitOfQuantity = "Kgs";
		AssertEquals("Kgs", CreateDataProvider().QtyUnits);
	}

	public override void TestSbDate()
	{
		AssertNull(CreateDataProvider().BeDate);

		Instruction.ShippingBillDate = new ZDateTime(2024, 6, 13);
		AssertEquals(new ZDateTime(2024, 6, 13).ToDateTime(), CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		Instruction.ShippingBillNumber = "1234";
		AssertEquals("1234", CreateDataProvider().SbNo);
	}

	public override void TestSrno()
	{
		JobWork.CSI_LineNo = 1;
		AssertEquals(1, CreateDataProvider().Srno);
	}

	protected override TableJobworkDataProviderAbstractClass CreateDataProvider()
	{
		Declaration.JE_CustomsOffice = "INBLR";
		return ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProvider).Sb.TableJobwork.First();
	}

	ExportSbCACHE01AdditionalDataProvider AdditionalDataProvider => new ExportSbCACHE01AdditionalDataProvider(messageSendingObject);

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobComInvoiceHeader Invoice => invoice ??= Declaration.Invoices[0];
	JobComInvoiceHeader invoice;

	CusEntryLine EntryLine => entryLine ??= Declaration.ActiveEntryHeaders[0].MergedLines[0];
	CusEntryLine entryLine;

	JobWork JobWork => jobWork ??= Declaration.ActiveEntryHeaders[0].JobWorks[0];
	JobWork jobWork;

	CusEntryInstruction Instruction => instruction ??= Declaration.ActiveEntryHeaders[0].EntryInstruction;
	CusEntryInstruction instruction;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		jobDeclaration.ActiveEntryHeaders.Add(header);
		var entryLine = header.MergedLines.AddNew();
		var invoice = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JobWorks.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var entryInstruction = Factory.New<CusEntryInstruction>();
		invoiceLine.JI_CEI = entryInstruction.PK;
		header.CH_CEI_Instruction = entryInstruction.PK;
		return jobDeclaration;
	}
}
