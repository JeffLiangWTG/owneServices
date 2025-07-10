using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(JobComInvoiceLineViewCollection))]
public class JobComInvoiceLineCollectionTest : BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
{
	protected override JobComInvoiceLineViewCollection GetCollectionToTest()
	{
		return Invoice.JobComInvoiceLines;
	}

	JobDeclaration fTestDec;
	JobDeclaration TestDec
	{
		get
		{
			if (fTestDec == null)
			{
				fTestDec = JobDeclaration.New(Factory);
			}

			return fTestDec;
		}
	}

	JobComInvoiceHeader fInvoice;
	JobComInvoiceHeader Invoice
	{
		get
		{
			if (fInvoice == null)
			{
				fInvoice = TestDec.Invoices.AddNew();
			}

			return fInvoice;
		}
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.JI_JZ = Invoice.PK;
		if (Invoice.JobComInvoiceLines.Contains(invoiceLine))
		{
			Invoice.JobComInvoiceLines.Remove(invoiceLine);
		}

		return invoiceLine;
	}

	public void TestTypedIndexer()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		JobComInvoiceHeader header = declaration.Invoices.AddNew();
		InvoiceLineCompleteCollection lineCollection = new InvoiceLineCompleteCollection(declaration);
		JobComInvoiceLineViewCollection collection = new JobComInvoiceLineViewCollection(header, lineCollection);
		JobComInvoiceLine invoiceLine = collection.AddNew();
		AssertEquals(invoiceLine, collection[0]);
	}
}
