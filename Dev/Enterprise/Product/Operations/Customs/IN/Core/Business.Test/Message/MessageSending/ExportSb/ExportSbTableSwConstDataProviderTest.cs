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
sealed class ExportSbTableSwConstDataProviderTest : ExportSbTableSwConstDataProviderAbstractClassBase
{
	public override void TestActiveIngredient()
	{
		swConstituent.CSI_Status = "Y";
		AssertEquals("Y", CreateDataProvider().ActiveIngredient);
	}

	public override void TestConstituentCode()
	{
		swConstituent.CSI_Code = "ADH";
		AssertEquals("ADH", CreateDataProvider().ConstituentCode);
	}

	public override void TestConstituentElementName()
	{
		swConstituent.CSI_Description = "INNSA1";
		AssertEquals("INNSA1", CreateDataProvider().ConstituentElementName);
	}

	public override void TestConstituentPercentage()
	{
		swConstituent.CSI_Quantity = 19.000m;
		AssertEquals(19.000m, CreateDataProvider().ConstituentPercentage);
	}

	public override void TestConstituentYieldPercentage()
	{
		swConstituent.CSI_Quantity2 = 30.000m;
		AssertEquals(30.000m, CreateDataProvider().ConstituentYieldPercentage);
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
		AssertNull(CreateDataProvider().SbDate);
		entryInstruction.ShippingBillDate = new ZDateTime(2024, 6, 13);
		AssertEquals(new ZDateTime(2024, 6, 13), CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		entryInstruction.ShippingBillNumber = "10";
		AssertEquals("10", CreateDataProvider().SbNo);
	}

	public override void TestSerialNo()
	{
		swConstituent.CSI_LineNo = 1;
		AssertEquals(1, CreateDataProvider().SerialNo);
	}

	public override void TestSiteId()
	{
		declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().SiteId);
	}

	protected override TableSwConstDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, new ExportSbCACHE01AdditionalDataProvider(messageSendingObject)).Sb.TableSwConst.First();
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

		swConstituent = invoiceLine.SWConstituents.AddNew();
		entryInstruction = Factory.New<CusEntryInstruction>();
		header.CH_CEI_Instruction = entryInstruction.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	SWConstituent swConstituent;
	CusEntryInstruction entryInstruction;
}
