using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbSbDataProviderTest : ExportSbSbDataProviderAbstractClassBase
{
	public override void TestTableSb()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNotNull(nameof(SbDataProviderAbstractClass.TableSb), dataProvider.TableSb);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSb)} Count", 1, dataProvider.TableSb.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSb)} Type", "TableSbDataProvider", dataProvider.TableSb.Single().GetType().Name);
		});
	}

	public override void TestTableInvoice()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableExchange()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableItem()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableItemaccess()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableThirdparty()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableCess()
	{
		var dataProvider = CreateDataProvider();
		var t = dataProvider.TableCess;
		CombineAssertions(() =>
		{
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableCess)} Count", 1, t.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableCess)} Type", "TableCessDataProvider", t.Single().GetType().Name);
		});
	}

	public override void TestTableDbk()
	{
		var dataProvider = CreateDataProvider();
		var t = dataProvider.TableDbk;
		CombineAssertions(() =>
		{
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableDbk)} Count", 1, t.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableDbk)} Type", "TableDbkDataProvider", t.Single().GetType().Name);
		});
	}

	public override void TestTableItemrawmtrl()
	{
		var dataProvider = CreateDataProvider();
		var t = dataProvider.TableItemrawmtrl;
		AssertNull(t);
	}

	public override void TestTableDepb()
	{
		var dataProvider = CreateDataProvider();
		var t = dataProvider.TableDepb;
		AssertNull(t);
	}

	public override void TestTableDepbparent()
	{
		var dataProvider = CreateDataProvider();
		var t = dataProvider.TableDepbparent;
		AssertNull(t);
	}

	public override void TestTableLicence()
	{
		var dataProvider = CreateDataProvider();
		var t = dataProvider.TableLicence;
		CombineAssertions(() =>
		{
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableLicence)} Count", 1, t.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableLicence)} Type", "TableLicenceDataProvider", t.Single().GetType().Name);
		});
	}

	public override void TestTableDfia()
	{
		var dataProvider = CreateDataProvider();
		var t = dataProvider.TableDfia;
		AssertNull(t);
	}

	public override void TestTableJobwork()
	{
		invoiceLine1.JobWorks.AddNew();
		invoiceLine1.JobWorks.AddNew();
		invoiceLine2.JobWorks.AddNew();
		invoiceLine2.JobWorks.AddNew();
		invoiceLine3.JobWorks.AddNew();
		invoiceLine3.JobWorks.AddNew();
		invoiceLine4.JobWorks.AddNew();
		invoiceLine4.JobWorks.AddNew();
		var dataProvider = CreateDataProvider();
		var t = dataProvider.TableJobwork;
		CombineAssertions(() =>
		{
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableJobwork)} Count", 8, t.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableJobwork)} Type", "TableJobworkDataProvider", t.FirstOrDefault().GetType().Name);
		});
	}

	public override void TestTableAr4()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTablePackinglist()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableRotation()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableEou()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableStuff()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableContainer()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		var entryLine = entryHeader.MergedLines.AddNew();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.ActiveEntryHeaders.Add(entryHeader);
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;
		var container1 = declaration.CusContainers.AddNew();
		container1.CO_ContainerNumber = "HXU1";
		var container2 = declaration.CusContainers.AddNew();
		container2.CO_ContainerNumber = "HXU2";

		entryInstruction.ContainersForInstructionForBindingOnly.First(x => x.Container.CO_ContainerNumber == "HXU1").IsForEntry = true;
		var data = ExportSbCACHE01DataProvider.CreateProvider(entryHeader, new ExportSbCACHE01AdditionalDataProvider(messageSendingObject)).Sb.TableContainer.ToList();
		AssertEquals(1, data.Count);

		entryInstruction.ContainersForInstructionForBindingOnly.First(x => x.Container.CO_ContainerNumber == "HXU2").IsForEntry = true;
		data = ExportSbCACHE01DataProvider.CreateProvider(entryHeader, new ExportSbCACHE01AdditionalDataProvider(messageSendingObject)).Sb.TableContainer.ToList();
		AssertEquals(2, data.Count);
	}

	public override void TestTableCargoback()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTablePckgback()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableContainerback()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableStr()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTableSwInfoType()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			var tableSwInfoType = dataProvider.TableSwInfoType;
			AssertNotNull(nameof(SbDataProviderAbstractClass.TableSwInfoType), tableSwInfoType);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSwInfoType)} Count", 1, tableSwInfoType.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSwInfoType)} Type", "TableSwInfoTypeDataProvider", tableSwInfoType.Single().GetType().Name);
		});
	}

	public override void TestTableSwConst()
	{
		CombineAssertions(() =>
		{
			invoiceLine1.SWConstituents.AddNew();
			invoiceLine1.SWConstituents.AddNew();
			invoiceLine2.SWConstituents.AddNew();
			invoiceLine2.SWConstituents.AddNew();

			var dataProvider = CreateDataProvider();
			var t = dataProvider.TableSwConst;
			AssertNotNull(nameof(SbDataProviderAbstractClass.TableSwConst), dataProvider.TableSwConst);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSwConst)} Count", 4, dataProvider.TableSwConst.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSwConst)} Type", "TableSwConstDataProvider", dataProvider.TableSwConst.FirstOrDefault().GetType().Name);
		});
	}

	public override void TestTableSwProd()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			var t = dataProvider.TableSwProd;
			AssertNotNull(nameof(SbDataProviderAbstractClass.TableSwProd), dataProvider.TableSwProd);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSwProd)} Count", 1, dataProvider.TableSwProd.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSwProd)} Type", "TableSwProdDataProvider", dataProvider.TableSwProd.Single().GetType().Name);
		});
	}

	public override void TestTableSwCtrl()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			var t = dataProvider.TableSwCtrl;
			AssertNotNull(nameof(SbDataProviderAbstractClass.TableSwCtrl), dataProvider.TableSwCtrl);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSwCtrl)} Count", 1, dataProvider.TableSwCtrl.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSwCtrl)} Type", "TableSwCtrlDataProvider", dataProvider.TableSwCtrl.Single().GetType().Name);
		});
	}

	public override void TestTableStatement()
	{
		var dataProvider = CreateDataProvider();
		var t = dataProvider.TableStatement;
		CombineAssertions(() =>
		{
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableStatement)} Count", 1, t.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableStatement)} Type", "TableStatementDataProvider", t.Single().GetType().Name);
		});
	}

	public override void TestTableSupportingDocs()
	{
		var dataProvider = CreateDataProvider();
		var t = dataProvider.TableSupportingDocs;
		CombineAssertions(() =>
		{
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSupportingDocs)} Count", 1, t.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableSupportingDocs)} Type", "TableSupportingdocsDataProvider", t.Single().GetType().Name);
		});
	}

	public override void TestTableReexport()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNotNull(nameof(SbDataProviderAbstractClass.TableReexport), dataProvider.TableReexport);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableReexport)} Count", 1, dataProvider.TableReexport.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableReexport)} Type", "TableReexportDataProvider", dataProvider.TableReexport.Single().GetType().Name);
		});
	}

	public override void TestTableAmendhistory()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNotNull(nameof(SbDataProviderAbstractClass.TableAmendhistory), dataProvider.TableAmendhistory);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableAmendhistory)} Count", 1, dataProvider.TableAmendhistory.Count);
			AssertEquals($"{nameof(SbDataProviderAbstractClass.TableAmendhistory)} Type", "TableAmendhistoryDataProvider", dataProvider.TableAmendhistory.Single().GetType().Name);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryLine1 = header.MergedLines.AddNew();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.CustomsEntryHeaders.Add(header);
		invoice1 = declaration.Invoices.AddNew();
		invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
		invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
		invoice2 = declaration.Invoices.AddNew();
		invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
		invoiceLine4 = invoice2.JobComInvoiceLines.AddNew();

		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine1.PK;
		invoiceLine3.JI_CL = entryLine1.PK;
		invoiceLine4.JI_CL = entryLine1.PK;

		entryInstruction = Factory.New<CusEntryInstruction>();
		header.CH_CEI_Instruction = entryInstruction.PK;
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_CEI = entryInstruction.PK;
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice1;
	JobComInvoiceHeader invoice2;
	JobComInvoiceLine invoiceLine1;
	JobComInvoiceLine invoiceLine2;
	JobComInvoiceLine invoiceLine3;
	JobComInvoiceLine invoiceLine4;
	CusEntryInstruction entryInstruction;
	CusEntryLine entryLine1;

	protected override SbDataProviderAbstractClass CreateDataProvider()
		=> ExportSbCACHE01DataProvider.CreateProvider(header, new ExportSbCACHE01AdditionalDataProvider(messageSendingObject)).Sb;
}

