using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	sealed class JobComInvoiceLineCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
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

		protected override JobComInvoiceLineViewCollection GetCollectionToTest()
		{
			return Invoice.JobComInvoiceLines;
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

		#region Invoice
		JobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					fInvoice = declaration.Invoices.AddNew();
				}
				return fInvoice;
			}
		}
		JobComInvoiceHeader fInvoice;
		#endregion
	}
}
