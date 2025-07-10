using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceLineViewCollection))]
sealed class JobComInvoiceLineViewCollectionTest : EU.Business.Declaration.Testing.JobComInvoiceLineCollectionTest<JobComInvoiceLineViewCollection>
{
	public void TestElementType()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoice = dec.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		AssertType<JobComInvoiceLine>(invoiceLine);
		AssertType<JobComInvoiceLine>(invoice.InvoiceLines[0]);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.JI_JZ = Invoice.PK;
		if (Invoice.JobComInvoiceLines.Contains(invoiceLine))
		{
			Invoice.JobComInvoiceLines.Remove(invoiceLine);
		}
		return invoiceLine;
	}
}
