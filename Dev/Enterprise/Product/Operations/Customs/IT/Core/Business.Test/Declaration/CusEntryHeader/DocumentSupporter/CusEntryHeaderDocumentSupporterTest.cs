using System.Linq;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
sealed class CusEntryHeaderDocumentSupporterTest : DocumentSupporterTest
{
	public void TestBusinessContext()
	{
		var entryHeaderDocumentSupporter = new CusEntryHeaderDocumentSupporter(entryHeader);
		AssertEquals("BusinessContext", BusinessContext.CusEntryHeader, entryHeaderDocumentSupporter.BusinessContext);
	}

	public void TestCustomisationSecurityCheckpoint()
	{
		var entryHeaderDocumentSupporter = new CusEntryHeaderDocumentSupporter(entryHeader);
		AssertEquals("CustomisationSecurityCheckpoint", Env.Security.CustomsDeclarationCustomiseDocument, entryHeaderDocumentSupporter.CustomisationSecurityCheckpoint);
	}

	public void TestGetDocumentWrappersInternal_SADHDataContext()
	{
		var entryHeaderDocumentSupporter = new CusEntryHeaderDocumentSupporterForTesting(entryHeader);
		var wrappers = entryHeaderDocumentSupporter.GetDocumentWrappersInternalExposed(DataContext.SADH, Factory.New<IStmMenuItem>());
		AssertNotNull(wrappers);
		AssertEquals("Wrappers length", 1, wrappers.Length);
		AssertEquals("SADH Wrapper type", "Enterprise.Customs.IT.Business.ITDocSADH", wrappers.Single().GetType().FullName);
	}

	public void TestGetDocumentWrappersInternal_ITSadAttachmentDataContext()
	{
		var entryHeaderDocumentSupporter = new CusEntryHeaderDocumentSupporterForTesting(entryHeader);
		var wrappers = entryHeaderDocumentSupporter.GetDocumentWrappersInternalExposed(DataContext.ITSadAttachment, Factory.New<IStmMenuItem>());
		AssertEquals("When Entry Header has no ITSadAttachment, Wrappers length", 0, wrappers.Length);

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

		var entryLine1 = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container1.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container2.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container3.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container4.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container5.PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = container6.PK;
		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = container1.PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = container2.PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = container3.PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = container4.PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = container5.PK;

		entryHeaderDocumentSupporter = new CusEntryHeaderDocumentSupporterForTesting(entryHeader);
		wrappers = entryHeaderDocumentSupporter.GetDocumentWrappersInternalExposed(DataContext.ITSadAttachment, Factory.New<IStmMenuItem>());
		AssertNotNull(wrappers);
		AssertEquals("Wrappers length", 1, wrappers.Length);
		AssertEquals("ITSadAttachment Wrapper type", "Enterprise.Customs.IT.Business.ITDocSADH", wrappers.Single().GetType().FullName);
	}

	public void TestSupportedDataContexts()
	{
		var entryHeaderDocumentSupporter = new CusEntryHeaderDocumentSupporter(entryHeader);

		CombineAssertions(() =>
		{
			AssertEquals("Declaration DataContext is supported", false, entryHeaderDocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Declaration)));
			AssertEquals("SADH DataContext is supported", true, entryHeaderDocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.SADH)));
			AssertEquals("ITSadAttachment DataContext is supported", true, entryHeaderDocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ITSadAttachment)));
		});
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
	{
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.PreviousDocuments.AddNew().CSI_Procedure = "A3";
		invoiceLine.PreviousDocuments.AddNew().CSI_Procedure = "A44";
		declaration.ResetApportionedPreviousDocuments();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		return entryHeader;
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	class CusEntryHeaderDocumentSupporterForTesting : CusEntryHeaderDocumentSupporter
	{
		public CusEntryHeaderDocumentSupporterForTesting(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public DocumentWrapper[] GetDocumentWrappersInternalExposed(DataContext dataContext, IStmMenuItem commandBeingRun) => GetDocumentWrappersInternal(dataContext, commandBeingRun);
	}
}
