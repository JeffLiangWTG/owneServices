using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	public class JobComInvoiceLineViewCollectionTest : BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
	{
		JobComInvoiceHeader jobComInvoiceHeader;

		public void TestElementType()
		{
			var dec = Factory.New<JobDeclaration>();
			jobComInvoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = jobComInvoiceHeader.InvoiceLines.AddNew();
			AssertType<JobComInvoiceLine>(invoiceLine);
			AssertType<JobComInvoiceLine>(jobComInvoiceHeader.InvoiceLines[0]);
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
