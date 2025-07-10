using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	class JobComInvoiceLineCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
	{
		public void TestTypedIndexer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var lineCollection = new InvoiceLineCompleteCollection(declaration);
			var collection = new JobComInvoiceLineViewCollection(header, lineCollection);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		protected override JobComInvoiceLineViewCollection GetCollectionToTest() => Invoice.JobComInvoiceLines;

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

		protected JobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = Factory.New<JobDeclaration>();
				}

				return fTestDec;
			}
		}

		JobDeclaration fTestDec;

		protected JobComInvoiceHeader Invoice
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

		JobComInvoiceHeader fInvoice;
	}
}
