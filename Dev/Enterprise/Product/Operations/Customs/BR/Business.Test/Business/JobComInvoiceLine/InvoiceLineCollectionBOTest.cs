
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(InvoiceLineViewCollection))]
	public class InvoiceLineCollectionBOTest : Customs.Business.Testing.InvoiceLineCollectionBOTest<InvoiceLineViewCollection>
	{
		public void TestTypedIndexer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collection = new InvoiceLineViewCollection(declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		public void TestCopyLastLineDetailsToNewLinesIfEnabled_Export()
		{
			var invoiceLineToClone = JobComInvoiceLineDeepCloneStrategyTest.CreateExportInvoiceLineForClone(Factory);

			var collection = invoiceLineToClone.Declaration.FilteredInvoiceLines;
			collection.CopyLastLineDetailsToNewLines = true;
			var clonedInvoiceLine = collection.AddNew();
			JobComInvoiceLineDeepCloneStrategyTest.AssertExportInvoiceLineCloned(clonedInvoiceLine, invoiceLineToClone);

			collection.CopyLastLineDetailsToNewLines = false;
			AssertEquals(ZString.Empty, collection.AddNew().FullGoodsDescription);
		}

		public void TestCopyLastLineDetailsToNewLinesIfEnabled_ImportLicense()
		{
			var invoiceLineToClone = JobComInvoiceLineDeepCloneStrategyTest.CreateImportLicenseInvoiceLineForClone(Factory);

			var collection = invoiceLineToClone.Declaration.FilteredInvoiceLines;
			collection.CopyLastLineDetailsToNewLines = true;
			var clonedInvoiceLine = collection.AddNew();
			JobComInvoiceLineDeepCloneStrategyTest.AssertImportLicenseInvoiceLineCloned(clonedInvoiceLine, invoiceLineToClone);

			collection.CopyLastLineDetailsToNewLines = false;
			AssertEquals(ZString.Empty, collection.AddNew().FullGoodsDescription);
		}

		public void TestCopyLastLineDetailsToNewLinesIfEnabled_ImportSiscomex()
		{
			var invoiceLineToClone = JobComInvoiceLineDeepCloneStrategyTest.CreateImportSiscomexInvoiceLineForClone(Factory);
			var collection = invoiceLineToClone.Declaration.FilteredInvoiceLines;
			collection.CopyLastLineDetailsToNewLines = true;
			var clonedInvoiceLine = collection.AddNew();
			JobComInvoiceLineDeepCloneStrategyTest.AssertImportSiscomexInvoiceLineCloned(clonedInvoiceLine, invoiceLineToClone);

			collection.CopyLastLineDetailsToNewLines = false;
			AssertEquals(ZString.Empty, collection.AddNew().FullGoodsDescription);
		}

		public void TestCopyLastLineDetailsToNewLinesIfEnabled_Import()
		{
			var invoiceLineToClone = JobComInvoiceLineDeepCloneStrategyTest.CreateImportInvoiceLineForClone(Factory);
			var collection = invoiceLineToClone.Declaration.FilteredInvoiceLines;
			collection.CopyLastLineDetailsToNewLines = true;
			var clonedInvoiceLine = collection.AddNew();
			JobComInvoiceLineDeepCloneStrategyTest.AssertImportInvoiceLineCloned(clonedInvoiceLine, invoiceLineToClone);

			collection.CopyLastLineDetailsToNewLines = false;
			AssertEquals(ZString.Empty, collection.AddNew().FullGoodsDescription);
		}

		protected override InvoiceLineViewCollection GetCollectionToTest() => new InvoiceLineViewCollection(JobDeclaration);

		protected new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;
	}
}
