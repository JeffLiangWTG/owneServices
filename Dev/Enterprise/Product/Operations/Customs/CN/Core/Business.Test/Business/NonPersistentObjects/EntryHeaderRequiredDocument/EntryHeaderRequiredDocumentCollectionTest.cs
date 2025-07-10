using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryHeaderRequiredDocumentCollection))]
	class EntryHeaderRequiredDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntryHeaderRequiredDocumentCollection>
	{
		protected override EntryHeaderRequiredDocumentCollection GetCollectionToTest()
		{
			return new EntryHeaderRequiredDocumentCollection(Factory.New<CusEntryHeader>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EntryHeaderRequiredDocument(Factory.New<CIQRequiredDocument>());
		}

		public void TestAllows()
		{
			var collection = GetCollectionToTest();
			Assert("Should not allow new", !collection.AllowNew);
			Assert("Should not allow remove", !collection.AllowRemove);
		}

		public void TestLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			AssertEquals(0, entryHeader.RequiredDocuments.Count);
			var invoiceHeader = declaration.Invoices.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			instruction.CIQRequiredDocuments.AddNew().XC_DocumentType = "13";
			instruction.CIQRequiredDocuments.AddNew().XC_DocumentType = "14";
			entryHeader.ClearCachedMergedData();
			AssertEquals(2, entryHeader.RequiredDocuments.Count);
			instruction.CIQRequiredDocuments.AddNew().XC_DocumentType = "15";
			entryHeader.ClearCachedMergedData();
			AssertEquals(3, entryHeader.RequiredDocuments.Count);
		}
	}
}
