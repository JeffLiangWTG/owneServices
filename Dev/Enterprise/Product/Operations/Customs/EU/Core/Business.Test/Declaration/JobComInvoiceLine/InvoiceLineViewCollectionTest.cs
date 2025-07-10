using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineViewCollection<JobComInvoiceLine>))]
	class InvoiceLineCollectionBOTest : Customs.Business.Testing.InvoiceLineCollectionBOTest<InvoiceLineViewCollection<JobComInvoiceLine>>
	{
		public void TestTypedIndexer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collection = new InvoiceLineViewCollection<JobComInvoiceLine>(declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		public void TestFetchStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collection = new InvoiceLineViewCollection<JobComInvoiceLine>(declaration);
			AssertType<FetchStrategies.InvoiceLineViewCollectionFetchStrategy>(collection.FetchStrategy);
		}

		protected override InvoiceLineViewCollection<JobComInvoiceLine> GetCollectionToTest()
		{
			return new InvoiceLineViewCollection<JobComInvoiceLine>(JobDeclaration);
		}

		protected new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected new JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.Invoice; }
		}

		public void TestCopyLastLineDetailsToNewLinesIfEnabledUsingClone()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var collection = declaration.FilteredInvoiceLines;
			var invoiceLine1 = collection.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 5;
			invoiceLine1.JI_LinePrice = 123.45;
			var supDoc1 = invoiceLine1.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = "ABC";

			collection.CopyLastLineDetailsToNewLines = true;
			var invoiceLine2 = collection.AddNew();

			AssertEquals((ZShort)2, invoiceLine2.JI_LineNo);
			AssertEquals(invoiceLine1.JI_InvoiceQuantity, invoiceLine2.JI_InvoiceQuantity);
			AssertEquals(invoiceLine1.JI_LinePrice, invoiceLine2.JI_LinePrice);
			AssertEquals(1, invoiceLine2.SupportingDocuments.Count);
			AssertEquals(supDoc1.CSI_Code, invoiceLine2.SupportingDocuments[0].CSI_Code);
		}
	}
}
