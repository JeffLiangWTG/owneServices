using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceLineViewCollection))]
class JobComInvoiceLineViewCollectionTest : BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
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
		invoice.JobComInvoiceLines.Remove(invoiceLine);
		return invoiceLine;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoice = declaration.Invoices.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
}
