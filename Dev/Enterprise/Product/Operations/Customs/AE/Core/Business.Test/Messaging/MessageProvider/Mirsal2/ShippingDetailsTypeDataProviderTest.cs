using System.Linq;
using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class ShippingDetailsTypeDataProviderTest : Mirsal2ShippingDetailsTypeDataProviderAbstractClassBase
{
	public override void TestDestinationCountry()
	{
		Declaration.JE_RL_NKFinalDestination = "AB123";
		AssertEquals(Declaration.JE_RL_NKFinalDestination.Left(2), CreateDataProvider().DestinationCountry);
	}

	public override void TestExitPort()
	{
		Declaration.JE_ExitPoint = "ABC";
		AssertEquals(Declaration.JE_ExitPoint, CreateDataProvider().ExitPort);
	}

	public override void TestExportEntityFreezoneCode()
	{
		Assert("to do in future WI", true);
	}

	public override void TestExportEntityWarehouseCode()
	{
		Assert("to do in future WI", true);
	}

	public override void TestImportEntityFreezoneCode()
	{
		Assert("to do in future WI", true);
	}

	public override void TestImportEntityWarehouseCode()
	{
		Assert("to do in future WI", true);
	}

	public override void TestInvoices()
	{
		var invoiceHeader = Declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var cusEntryLine = header.MergedLines.AddNew();
		cusEntryLine.InvoiceLines.Add(invoiceLine);
		AssertEquals($"{nameof(InvoiceTypeDataProviderAbstractClass)} Type", "InvoiceTypeDataProvider", CreateDataProvider().Invoices.Single().GetType().Name);
	}

	public override void TestOriginalLoadPort()
	{
		Declaration.JE_RL_NKOrigin = "TEST";
		AssertEquals(Declaration.JE_RL_NKOrigin, CreateDataProvider().OriginalLoadPort);
	}

	public override void TestPortOfDischarge()
	{
		Declaration.JE_RL_NKPortOfLoading = "TEST1";
		AssertEquals(Declaration.JE_RL_NKPortOfLoading, CreateDataProvider().PortOfDischarge);
	}

	public override void TestPortOfLoading()
	{
		Declaration.JE_RL_NKPortOfArrival = "TEST2";
		AssertEquals(Declaration.JE_RL_NKPortOfArrival, CreateDataProvider().PortOfLoading);
	}

	protected override ShippingDetailsTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.ShippingDetails;
	}

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		jobDeclaration.ActiveEntryHeaders.Add(header);
		var importer = Factory.New<OrgHeader>();
		jobDeclaration.JE_OH_Importer = importer.PK;
		return jobDeclaration;
	}
}
