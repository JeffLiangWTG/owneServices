using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class SupportingDocumentToEntriesLinkProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new SupportingDocumentToEntriesLinkProvider(null));
		AssertNoExceptionThrown(() => new SupportingDocumentToEntriesLinkProvider(Factory.New<SupportingDocument>()));
	}

	public void TestEntryHeaders()
	{
		var declaration = Factory.New<JobDeclaration>();
		var supportingDocument = declaration.SupportingDocuments.AddNew();

		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine2 = entryHeader2.MergedLines.AddNew();
		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		AssertContainsExactElementsInAnyOrder("No entries", Array.Empty<CusEntryHeader>(), supportingDocument.EntriesLinkProvider.EntryHeaders);

		invoiceLine1.JI_CL = entryLine1.PK;
		AssertContainsExactElementsInAnyOrder("1 entry", new CusEntryHeader[] { entryHeader1 }, supportingDocument.EntriesLinkProvider.EntryHeaders);

		invoiceLine2.JI_CL = entryLine2.PK;
		AssertContainsExactElementsInAnyOrder("2 entries", new CusEntryHeader[] { entryHeader1, entryHeader2 }, supportingDocument.EntriesLinkProvider.EntryHeaders);

		var invoiceLine3 = invoice1.InvoiceLines.AddNew();
		invoiceLine3.JI_CL = entryLine1.PK;
		AssertContainsExactElementsInAnyOrder("2 entries distinct", new CusEntryHeader[] { entryHeader1, entryHeader2 }, supportingDocument.EntriesLinkProvider.EntryHeaders);

		entryLine2.CL_CH = ZGuid.Empty;
		entryLine2.IsNull = true;
		AssertContainsExactElementsInAnyOrder($"Only for test purposes: {nameof(entryHeader2)} has been detached", new CusEntryHeader[] { entryHeader1 }, supportingDocument.EntriesLinkProvider.EntryHeaders);
	}

	public void TestEntryLines_JobLevelSupportingDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var supportingDocument = declaration.SupportingDocuments.AddNew();

		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine2 = entryHeader2.MergedLines.AddNew();
		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		AssertContainsExactElementsInAnyOrder("No entry lines", Array.Empty<CusEntryLine>(), supportingDocument.EntriesLinkProvider.EntryLines);

		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine2.PK;
		AssertContainsExactElementsInAnyOrder("2 entry lines", new CusEntryLine[] { entryLine1, entryLine2 }, supportingDocument.EntriesLinkProvider.EntryLines);

		var invoiceLine3 = invoice1.InvoiceLines.AddNew();
		invoiceLine3.JI_CL = entryLine1.PK;
		AssertContainsExactElementsInAnyOrder("2 entry lines distinct", new CusEntryLine[] { entryLine1, entryLine2 }, supportingDocument.EntriesLinkProvider.EntryLines);

		var invoice3 = declaration.Invoices.AddNew();
		var invoiceLine4 = invoice3.InvoiceLines.AddNew();
		invoiceLine4.JI_CL = entryLine1.PK;
		AssertContainsExactElementsInAnyOrder("2 entry lines distinct with multiple-invoice entry line", new CusEntryLine[] { entryLine1, entryLine2 }, supportingDocument.EntriesLinkProvider.EntryLines);
	}

	public void TestEntryLines_InvoiceLevelSupportingDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var supportingDocument = invoice.SupportingDocuments.AddNew();

		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();

		var entryLine2 = entryHeader1.MergedLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		AssertContainsExactElementsInAnyOrder("No entry lines", Array.Empty<CusEntryLine>(), supportingDocument.EntriesLinkProvider.EntryLines);

		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine2.PK;
		AssertContainsExactElementsInAnyOrder("2 entry lines", new CusEntryLine[] { entryLine1, entryLine2 }, supportingDocument.EntriesLinkProvider.EntryLines);

		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_CL = entryLine2.PK;
		AssertContainsExactElementsInAnyOrder("2 entry lines distinct", new CusEntryLine[] { entryLine1, entryLine2 }, supportingDocument.EntriesLinkProvider.EntryLines);
	}

	public void TestEntryLines_InvoiceLineLevelSupportingDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		AssertContainsExactElementsInAnyOrder("No entry line", Array.Empty<CusEntryLine>(), supportingDocument.EntriesLinkProvider.EntryLines);

		invoiceLine.JI_CL = entryLine.PK;
		AssertContainsExactElementsInAnyOrder("Linked entry line", new CusEntryLine[] { entryLine }, supportingDocument.EntriesLinkProvider.EntryLines);
	}

	public void TestEntryLines_InvalidParent()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		AssertContainsExactElementsInAnyOrder("Null parent", Array.Empty<CusEntryLine>(), supportingDocument.EntriesLinkProvider.EntryLines);

		var invalidSupportingDocumentParent = Factory.New<CusEntryInstruction>();
		supportingDocument.CSI_ParentID = invalidSupportingDocumentParent.PK;
		supportingDocument.CSI_ParentTableCode = invalidSupportingDocumentParent.TablePrefix;
		AssertContainsExactElementsInAnyOrder("Invalid parent", Array.Empty<CusEntryLine>(), supportingDocument.EntriesLinkProvider.EntryLines);
	}
}
