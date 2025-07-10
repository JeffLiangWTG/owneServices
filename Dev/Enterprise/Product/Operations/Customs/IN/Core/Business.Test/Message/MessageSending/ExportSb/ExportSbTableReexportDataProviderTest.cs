using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableReexportDataProviderTest : ExportSbTableReexportDataProviderAbstractClassBase
{
	public override void TestBeAssessableValueClaim()
	{
		AssertEquals("TBA", CreateDataProvider().BeAssessableValueClaim);
	}

	public override void TestBeAssessedValue()
	{
		AssertEquals("TBA", CreateDataProvider().BeAssessedValue);
	}

	public override void TestBeDt()
	{
		AssertEquals("TBA", CreateDataProvider().BeDt);
	}

	public override void TestBeDutyPaid()
	{
		AssertEquals("TBA", CreateDataProvider().BeDutyPaid);
	}

	public override void TestBeDutyPaymentDate()
	{
		AssertEquals("TBA", CreateDataProvider().BeDutyPaymentDate);
	}

	public override void TestBeInvoiceNo()
	{
		AssertEquals("TBA", CreateDataProvider().BeInvoiceNo);
	}

	public override void TestBeItem()
	{
		AssertEquals("TBA", CreateDataProvider().BeItem);
	}

	public override void TestBeItemDescription()
	{
		AssertEquals("TBA", CreateDataProvider().BeItemDescription);
	}

	public override void TestBeItemUsed()
	{
		AssertEquals("TBA", CreateDataProvider().BeItemUsed);
	}

	public override void TestBeNo()
	{
		AssertEquals("TBA", CreateDataProvider().BeNo);
	}

	public override void TestBeOtherIdentifiableParameter()
	{
		AssertEquals("TBA", CreateDataProvider().BeOtherIdentifiableParameter);
	}

	public override void TestBeQuantity()
	{
		AssertEquals("TBA", CreateDataProvider().BeQuantity);
	}

	public override void TestBeQuantityUtilised()
	{
		AssertEquals("TBA", CreateDataProvider().BeQuantityUtilised);
	}

	public override void TestBeSite()
	{
		AssertEquals("TBA", CreateDataProvider().BeSite);
	}

	public override void TestBeUqc()
	{
		AssertEquals("TBA", CreateDataProvider().BeUqc);
	}

	public override void TestCommisionerPermission()
	{
		AssertEquals("TBA", CreateDataProvider().CommisionerPermission);
	}

	public override void TestInputCredit()
	{
		AssertEquals("TBA", CreateDataProvider().InputCredit);
	}

	public override void TestInvoiceSerialNo()
	{
		invoice.JZ_InvoiceDisplaySequence = 1;
		AssertEquals(1, CreateDataProvider().InvoiceSerialNo);
	}

	public override void TestItemSerialNo()
	{
		entryLine.CL_LineNumber = 1;
		AssertEquals(1, CreateDataProvider().ItemSerialNo);
	}

	public override void TestJobDate()
	{
		header.CH_SystemCreateTimeUtc = new ZDateTime(2024, 6, 13);
		AssertEquals(new ZDateTime(2024, 6, 13).ToLocalBranchTime().ToDateTime(), CreateDataProvider().JobDate);
	}

	public override void TestJobNo()
	{
		header.CH_BGMReference = "1234";
		AssertEquals("1234", CreateDataProvider().JobNo);
	}

	public override void TestManualBe()
	{
		AssertEquals("TBA", CreateDataProvider().ManualBe);
	}

	public override void TestMessageType()
	{
		messageSendingObject.MessageType = DeclarationMessageTypeList.Codes.Fresh;
		AssertEquals(DeclarationMessageTypeList.Codes.Fresh, CreateDataProvider().MessageType);
	}

	public override void TestModvatAvailed()
	{
		AssertEquals("TBA", CreateDataProvider().ModvatAvailed);
	}

	public override void TestModvatRepaid()
	{
		AssertEquals("TBA", CreateDataProvider().ModvatRepaid);
	}

	public override void TestPersonalUsed()
	{
		AssertEquals("TBA", CreateDataProvider().PersonalUsed);
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

	public override void TestSiteid()
	{
		declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().Siteid);
	}

	protected override TableReexportDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, new ExportSbCACHE01AdditionalDataProvider(messageSendingObject)).Sb.TableReexport.First();
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
