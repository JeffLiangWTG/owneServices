using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(AllGroupedPreviousDocumentCollection))]
sealed class AllGroupedPreviousDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AllGroupedPreviousDocumentCollection>
{
	public void TestAllGroupedPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;

		declaration.ResetApportionedPreviousDocuments();
		var allGroupedPreviousDocumentCollection = AllGroupedPreviousDocumentCollection.LoadNew(entryHeader);
		AssertEquals("All Grouped Previous Documents count", 0, allGroupedPreviousDocumentCollection.Count);

		var previousDocument1 = invoiceLine1.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "A3";
		previousDocument1.CSI_ReferenceNumber = "1";
		var previousDocument2 = invoiceLine1.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "A3";
		previousDocument2.CSI_ReferenceNumber = "2";
		var previousDocument3 = invoiceLine2.PreviousDocuments.AddNew();
		previousDocument3.CSI_Procedure = "2";
		previousDocument3.CSI_ReferenceNumber = "1";
		var previousDocument4 = invoiceLine2.PreviousDocuments.AddNew();
		previousDocument4.CSI_Procedure = "2";
		previousDocument4.CSI_ReferenceNumber = "2";

		declaration.ResetApportionedPreviousDocuments();
		allGroupedPreviousDocumentCollection = AllGroupedPreviousDocumentCollection.LoadNew(entryHeader);
		AssertEquals("All Grouped Previous Documents count", 4, allGroupedPreviousDocumentCollection.Count);
		AssertEquals("Grouped Previous Documents for first entry line", 2, allGroupedPreviousDocumentCollection.Cast<GroupedPreviousDocument>().Count(x => x.EntryLineNumber == 1));
		AssertEquals("Grouped Previous Documents for second entry line", 2, allGroupedPreviousDocumentCollection.Cast<GroupedPreviousDocument>().Count(x => x.EntryLineNumber == 2));
	}

	protected override AllGroupedPreviousDocumentCollection GetCollectionToTest()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		return AllGroupedPreviousDocumentCollection.LoadNew(entryHeader);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var entryLine = Factory.New<CusEntryLine>();
		var previousDocument = Factory.New<PreviousDocument>();
		previousDocument.CSI_Procedure = "A3";
		var mergedDocument = MergedPreviousDocument.FromPreviousDocument(previousDocument);
		return new GroupedPreviousDocument(entryLine, mergedDocument, null, Factory);
	}
}
