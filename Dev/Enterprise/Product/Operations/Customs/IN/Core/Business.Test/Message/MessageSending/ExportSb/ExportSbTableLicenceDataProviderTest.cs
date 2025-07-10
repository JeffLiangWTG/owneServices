using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableLicenceDataProviderTest : ExportSbTableLicenceDataProviderAbstractClassBase
{
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

	public override void TestSrno()
	{
		AssertEquals("TBA", CreateDataProvider().Srno);
	}

	public override void TestRegistrationNumber()
	{
		AssertEquals("TBA", CreateDataProvider().RegistrationNumber);
	}

	public override void TestRegistrationDate()
	{
		AssertEquals("TBA", CreateDataProvider().RegistrationDate);
	}

	public override void TestItemSerialNumberInPartE()
	{
		AssertEquals("TBA", CreateDataProvider().ItemSerialNumberInPartE);
	}

	public override void TestItemSerialNumberInPartC()
	{
		AssertEquals("TBA", CreateDataProvider().ItemSerialNumberInPartC);
	}

	public override void TestQuantity()
	{
		AssertEquals("TBA", CreateDataProvider().Quantity);
	}

	public override void TestExportQuantity()
	{
		AssertEquals("TBA", CreateDataProvider().ExportQuantity);
	}

	public override void TestWhetherIndigenousImportedNM()
	{
		AssertEquals("TBA", CreateDataProvider().WhetherIndigenousImportedNM);
	}

	public override void TestAmendmentType()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentType);
	}

	public override void TestAmendmentNo()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentNo);
	}

	public override void TestAmendmentDate()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentDate);
	}

	protected override TableLicenceDataProviderAbstractClass CreateDataProvider()
	{
		Declaration.JE_CustomsOffice = "INBLR";
		return ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProvider).Sb.TableLicence.First();
	}

	ExportSbCACHE01AdditionalDataProvider AdditionalDataProvider => new ExportSbCACHE01AdditionalDataProvider(messageSendingObject);

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobComInvoiceHeader Invoice => invoice ??= Declaration.Invoices[0];
	JobComInvoiceHeader invoice;

	CusEntryLine EntryLine => entryLine ??= Declaration.ActiveEntryHeaders[0].MergedLines[0];
	CusEntryLine entryLine;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		jobDeclaration.ActiveEntryHeaders.Add(header);
		var entryLine = header.MergedLines.AddNew();
		var invoice = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		return jobDeclaration;
	}
}
