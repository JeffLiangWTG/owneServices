using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobComInvoiceLineViewCollection))]
sealed class JobComInvoiceLineCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
{
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
