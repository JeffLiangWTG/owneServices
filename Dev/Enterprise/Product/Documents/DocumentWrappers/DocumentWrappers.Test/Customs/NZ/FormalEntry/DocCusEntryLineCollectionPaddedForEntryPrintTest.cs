using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeader;

namespace Enterprise.DocumentWrappers.Customs.NZ.FormalEntry.Testing
{
	[TestedType(typeof(DocCusEntryLineCollectionPaddedForEntryPrint))]
	sealed class DocCusEntryLineCollectionPaddedForEntryPrintTest : DocCusEntryLineCollectionTest<DocCusEntryLineCollectionPaddedForEntryPrint>
	{
		protected override DocCusEntryLineCollectionPaddedForEntryPrint GetCollectionToTest()
		{
			return new DocCusEntryLineCollectionPaddedForEntryPrint(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocCusEntryLine.New(Factory);
		}

		public void TestCollectionKeepsItsCountWithAnExtraLineAtTheEndForImports()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = (CusEntryHeader)declaration.CusEntryHeader;

			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			AssertEquals("CusEntryLineCollection CusEntryLine Count", 2, entryHeader.MergedLines.Count);

			DocCusEntryLineCollectionPaddedForEntryPrint docCusEntryLines = new DocCusEntryLineCollectionPaddedForEntryPrint(entryHeader.MergedLines, Factory);

			AssertEquals("CusEntryLineCollection CusEntryLine Count", 2, entryHeader.MergedLines.Count);

			AssertEquals("DocCusEntryLineCollection DocCusEntryLine Count", 3, docCusEntryLines.Count);
			AssertEquals("Make sure last line can be accessed", "", docCusEntryLines[2].LineNumber);
		}

		public void TestCollectionKeepsItsCountWithAnExtraLineAtTheEndForExports()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = (CusEntryHeader)declaration.CusEntryHeader;

			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			AssertEquals("CusEntryLineCollection CusEntryLine Count", 2, entryHeader.MergedLines.Count);

			DocCusEntryLineCollectionPaddedForEntryPrint docCusEntryLines = new DocCusEntryLineCollectionPaddedForEntryPrint(entryHeader.MergedLines, Factory);

			AssertEquals("CusEntryLineCollection CusEntryLine Count", 2, entryHeader.MergedLines.Count);

			AssertEquals("DocCusEntryLineCollection DocCusEntryLine Count", 4, docCusEntryLines.Count);
			AssertEquals("Make sure second last line can be accessed", "", docCusEntryLines[2].LineNumber);
			AssertEquals("Make sure last line can be accessed", "", docCusEntryLines[3].LineNumber);
		}
	}
}
