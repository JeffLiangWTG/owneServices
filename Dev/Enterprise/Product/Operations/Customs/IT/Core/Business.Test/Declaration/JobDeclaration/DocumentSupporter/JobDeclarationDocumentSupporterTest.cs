using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(JobDeclarationDocumentSupporter))]
sealed class JobDeclarationDocumentSupporterTestA : EU.Business.Declaration.Testing.JobDeclarationDocumentSupporterTest
{
	public void TestGetSupportedDataContexts()
	{
		var declaration = Factory.New<JobDeclaration>();
		var documentSupporter = declaration.DocumentSupporter;
		var expectedSupportedDataContext = new DataContext[] { DataContext.SADH, DataContext.ITSadAttachment };
		CombineAssertions("Expected Supported DataContexts", () =>
		{
			foreach (var dataContext in expectedSupportedDataContext)
			{
				Assert($"{dataContext} should be supported", documentSupporter.IsDataContextSupported(new DataContextValueForTesting(dataContext)));
			}
		});
	}

	public void TestGetDocumentWrappersInternal_SADHDataContext()
	{
		var menuItemForTesting = Factory.New<IStmMenuItem>();
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		var documentSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);
		var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.SADH, menuItemForTesting);
		AssertNotNull(wrappers);
		AssertEquals(2, wrappers.Length);
		AssertSame(wrappers[0].WrappedObject, entryHeader1);
		AssertSame(wrappers[1].WrappedObject, entryHeader2);
	}

	public void TestGetDocumentWrappersInternal_SADHMenuItem()
	{
		var menuItemForTesting = Factory.New<IStmMenuItem>();
		menuItemForTesting.SU_MenuName = "SADH C88";
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.CH_BGMReference = "X";
		declaration.CustomsEntryHeaders.AddNew();

		var documentSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);

		var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.SADH, menuItemForTesting);
		AssertNotNull(wrappers);
		AssertEquals("When BGMReferenceToPrint is empty, Wrappers", 0, wrappers.Length);

		documentSupporter.SadDocumentSupporter.BGMReferenceToPrint = "X";
		wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.SADH, menuItemForTesting);
		AssertNotNull(wrappers);
		AssertEquals("When BGMReferenceToPrint is X", 1, wrappers.Length);
		AssertSame("Wrapped Object", wrappers[0].WrappedObject, entryHeader1);
	}

	public void TestGetDataStateBeforeRun()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var documentSupporter = declaration.DocumentSupporter;
		var getEntryToPrintHasBeenInvoked = false;
		declaration.OnGetEntryToPrintSadHC88 += GetEntryToPrintSadHC88;

		var documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(null);
		CombineAssertions("Assert GetDataStateBeforeRun result when commandAboutToRun parameter is null", () =>
		{
			AssertEquals("GetEntryToPrint has been invoked", getEntryToPrintHasBeenInvoked, false);
			AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
		});

		var menuItemForTesting = Factory.New<IStmMenuItem>();
		menuItemForTesting.SU_MenuName = "SADH C88";
		documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(menuItemForTesting);
		CombineAssertions("Assert GetDataStateBeforeRun result when Declaration has no Entry Headers", () =>
		{
			AssertEquals("GetEntryToPrint has been invoked", getEntryToPrintHasBeenInvoked, false);
			AssertEquals("IsValid", false, documentSupporterDataState.IsValid);
			AssertEquals("ErrorMessage", "No entries are available", documentSupporterDataState.ErrorMessage);
		});

		declaration.OnGetEntryToPrintSadHC88 -= GetEntryToPrintSadHC88;
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.CH_BGMReference = "X";

		documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(menuItemForTesting);
		CombineAssertions("Assert GetDataStateBeforeRun result when no event has been attached", () =>
		{
			AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
			AssertEquals("BGMReferenceToPrint", "X", documentSupporter.SadDocumentSupporter.BGMReferenceToPrint);
			AssertEquals("LayoutStyle", "C", documentSupporter.SadDocumentSupporter.LayoutStyle);
		});

		declaration.OnGetEntryToPrintSadHC88 += GetEntryToPrintSadHC88;
		documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(Factory.New<IStmMenuItem>());
		CombineAssertions("Assert GetDataStateBeforeRun result for a non SADH C88 menu item", () =>
		{
			AssertEquals("GetEntryToPrint has been invoked", getEntryToPrintHasBeenInvoked, false);
			AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
			AssertEquals("BGMReferenceToPrint", "X", documentSupporter.SadDocumentSupporter.BGMReferenceToPrint);
			AssertEquals("LayoutStyle", "C", documentSupporter.SadDocumentSupporter.LayoutStyle);
		});

		documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(menuItemForTesting);
		CombineAssertions("Assert GetDataStateBeforeRun result when an event has been attached", () =>
		{
			AssertEquals("GetEntryToPrint has been invoked", getEntryToPrintHasBeenInvoked, true);
			AssertEquals("IsValid", false, documentSupporterDataState.IsValid);
			AssertEquals("BGMReferenceToPrint", "X2", documentSupporter.SadDocumentSupporter.BGMReferenceToPrint);
			AssertEquals("LayoutStyle", "8", documentSupporter.SadDocumentSupporter.LayoutStyle);
		});

		declaration.OnGetEntryToPrintSadHC88 -= GetEntryToPrintSadHC88;
		documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(menuItemForTesting);
		CombineAssertions("Assert GetDataStateBeforeRun use default values", () =>
		{
			AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
			AssertEquals("BGMReferenceToPrint", "X", documentSupporter.SadDocumentSupporter.BGMReferenceToPrint);
			AssertEquals("LayoutStyle", "C", documentSupporter.SadDocumentSupporter.LayoutStyle);
		});

		void GetEntryToPrintSadHC88(object s, CancelEventArgs e)
		{
			getEntryToPrintHasBeenInvoked = true;
			e.Cancel = true;
			documentSupporter.SadDocumentSupporter.BGMReferenceToPrint = "X2";
			documentSupporter.SadDocumentSupporter.LayoutStyle = "8";
		}
	}

	public void TestGetDocumentWrappersInternal_ITSadAttachment()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var container1 = declaration.CusContainers.AddNew();
		container1.CO_ContainerNumber = "CNT1";
		var container2 = declaration.CusContainers.AddNew();
		container2.CO_ContainerNumber = "CNT2";
		var container3 = declaration.CusContainers.AddNew();
		container3.CO_ContainerNumber = "CNT3";
		var container4 = declaration.CusContainers.AddNew();
		container4.CO_ContainerNumber = "CNT4";
		var container5 = declaration.CusContainers.AddNew();
		container5.CO_ContainerNumber = "CNT5";
		var container6 = declaration.CusContainers.AddNew();
		container6.CO_ContainerNumber = "CNT6";

		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container1.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container2.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container3.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container4.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container5.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container6.PK;
		var entryLine2 = entryHeader1.MergedLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = container1.PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = container2.PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = container3.PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = container4.PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = container5.PK;

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine3 = entryHeader2.MergedLines.AddNew();
		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_CL = entryLine3.PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = container1.PK;

		var documentSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);
		var menuItemForTesting = Factory.New<IStmMenuItem>();
		var documentWrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.ITSadAttachment, menuItemForTesting);
		AssertNotNull(documentWrappers);
		AssertEquals(1, documentWrappers.Length);
		AssertSame(documentWrappers[0].WrappedObject, entryHeader1);
	}

	public void TestSadDocumentSupporter()
	{
		var documentSupporter = Factory.New<JobDeclaration>().DocumentSupporter;
		CombineAssertions("Check SadDocumentSupporter", () =>
		{
			var sadDocumentSupporter = documentSupporter.SadDocumentSupporter;
			AssertNotNull("SadDocumentSupporter", sadDocumentSupporter);
			AssertSame("SadDocumentSupporter cached", sadDocumentSupporter, documentSupporter.SadDocumentSupporter);
		});
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.PreviousDocuments.AddNew().CSI_Procedure = "A3";
		invoiceLine.PreviousDocuments.AddNew().CSI_Procedure = "A44";
		declaration.ResetApportionedPreviousDocuments();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TEST";
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var landedCostHeader = (BusinessObject)Factory.New<LandedCosting.ILandedCostHeader>();
		landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = declaration.TablePrefix;
		landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = declaration.PK;

		return declaration;
	}
}

class JobDeclarationDocumentSupporterForTesting : JobDeclarationDocumentSupporter
{
	public JobDeclarationDocumentSupporterForTesting(JobDeclaration declaration) : base(declaration)
	{
	}

	public DocumentWrapper[] GetDocumentWrappersInternalExposed(DataContext dataContext, IStmMenuItem commandBeingRun) => GetDocumentWrappersInternal(dataContext, commandBeingRun);
}
