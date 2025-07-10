using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RFPNumberCollection))]
	sealed class RFPNumberCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestClone()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var number = invoiceLine.RFPNumbers.AddNew();
			number.ZA_RFPNumber = "TEST1";
			number = invoiceLine.RFPNumbers.AddNew();
			number.ZA_RFPNumber = "TEST2";
			Factory.Save();

			var clonedInvoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.RFPNumbers.Clone(clonedInvoiceLine);

			AssertEquals("Cloned RFPNumbers count", 2, clonedInvoiceLine.RFPNumbers.Count);
			AssertEquals("Cloned RFPNumber 1", "TEST1", clonedInvoiceLine.RFPNumbers[0].ZA_RFPNumber);
			AssertEquals("Cloned RFPNumber 2", "TEST2", clonedInvoiceLine.RFPNumbers[1].ZA_RFPNumber);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new RFPNumberCollection(Factory.New<JobComInvoiceLine>());
	}
}
