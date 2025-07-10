using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	public class JobComInvoiceLineViewCollectionTest : JobComInvoiceLineCollectionTest<JobComInvoiceLineViewCollection>
	{
		public void TestElementType()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertType<JobComInvoiceLine>(invoiceLine);
			AssertType<JobComInvoiceLine>(invoice.InvoiceLines[0]);
		}
	}
}
