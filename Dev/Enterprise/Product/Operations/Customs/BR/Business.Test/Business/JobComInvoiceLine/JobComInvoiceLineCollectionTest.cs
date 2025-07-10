using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	public class JobComInvoiceLineCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
	{
		public void TestTypedIndexer()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			InvoiceLineCompleteCollection lineCollection = new InvoiceLineCompleteCollection(declaration);
			JobComInvoiceLineViewCollection collection = new JobComInvoiceLineViewCollection(header, lineCollection);
			JobComInvoiceLine invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		protected override JobComInvoiceLineViewCollection GetCollectionToTest()
		{
			return Invoice.JobComInvoiceLines;
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

		#region TestDec

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

		#endregion

		#region Invoice

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

		#endregion
	}
}
