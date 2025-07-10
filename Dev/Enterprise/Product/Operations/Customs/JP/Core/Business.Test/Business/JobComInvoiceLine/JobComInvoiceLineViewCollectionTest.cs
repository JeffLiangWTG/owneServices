using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	class JobComInvoiceLineViewCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
	{
		public void TestTypedIndexer()
		{
			var lineCollection = new InvoiceLineCompleteCollection(declaration);
			var collection = new JobComInvoiceLineViewCollection(invoiceHeader, lineCollection);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		protected override JobComInvoiceLineViewCollection GetCollectionToTest() => invoiceHeader.JobComInvoiceLines;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			if (invoiceHeader.JobComInvoiceLines.Contains(invoiceLine))
			{
				invoiceHeader.JobComInvoiceLines.Remove(invoiceLine);
			}
			return invoiceLine;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
	}

	class JobComInvoiceLineCollectionCountrySpecificTest : Customs.Business.Testing.BaseJobComInvoiceLineCollectionTest
	{
	}
}
