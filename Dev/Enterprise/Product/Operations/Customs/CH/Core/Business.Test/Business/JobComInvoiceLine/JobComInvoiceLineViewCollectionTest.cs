using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobComInvoiceLineViewCollection))]
class JobComInvoiceLineCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
{
	public void TestAddNewType()
	{
		invoice.InvoiceLines.AddNew();
		AssertType<JobComInvoiceLine>(invoice.InvoiceLines[0]);
	}

	public void TestElementType()
	{
		AssertType<JobComInvoiceLine>(invoice.InvoiceLines.AddNew());
	}

	protected override JobComInvoiceLineViewCollection GetCollectionToTest() => invoice.JobComInvoiceLines;

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.JI_JZ = invoice.PK;
		if (invoice.JobComInvoiceLines.Contains(invoiceLine))
		{
			invoice.JobComInvoiceLines.Remove(invoiceLine);
		}
		return invoiceLine;
	}

	protected override void SetUp()
	{
		base.SetUp();
		invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
	}

	JobComInvoiceHeader invoice;
}
