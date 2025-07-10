using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
sealed class CusEntryHeaderDocumentSupporterTest : DocumentSupporterTest
{
	public void TestGetDocumentWrappersInternal_Passar()
	{
		EntryHeader.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;

		var menuItemForTesting = Factory.New<IStmMenuItem>();
		var wrappers = DocumentSupporter.GetDocumentWrappers(DataContext.CusEntryHeader, menuItemForTesting);

		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Wrapped Objects", new[] { EntryHeader }, wrappers.Select(x => x.WrappedObject));
			AssertType<DocPassarCusEntryHeader>(wrappers[0]);
		});
	}

	public void TestGetDocumentWrappersInternal_Edec()
	{
		EntryHeader.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		var menuItemForTesting = Factory.New<IStmMenuItem>();
		var wrappers = DocumentSupporter.GetDocumentWrappers(DataContext.CusEntryHeader, menuItemForTesting);

		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Wrapped Objects", new[] { EntryHeader }, wrappers.Select(x => x.WrappedObject));
			AssertType<DocEdecCusEntryHeader>(wrappers[0]);
		});
	}

	public void TestGetFilterValue()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var supporter = (CusEntryHeaderDocumentSupporter)entryHeader.DocumentSupporter;

		AssertEquals("No exception for DocumentFilters.CTY", CountryCodes.Switzerland, supporter.GetFilterValue(DocumentFilters.CTY));
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var instructionPreviousDocument = entryInstruction.PreviousDocuments.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		declaration.DoMergeForTesting();

		return declaration.CustomsEntryHeaders.First();
	}

	public override void TestRunningDocumentsShouldNotCauseException() => Assert(true);

	CusEntryHeader EntryHeader => entryHeader ??= CreateEntryHeader();
	CusEntryHeader entryHeader;

	CusEntryHeader CreateEntryHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return entryHeader;
	}

	CusEntryHeaderDocumentSupporter DocumentSupporter => documentSupporter ??= new CusEntryHeaderDocumentSupporter(EntryHeader);
	CusEntryHeaderDocumentSupporter documentSupporter;
}
