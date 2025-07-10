using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineDependentCollection))]
	public class InvoiceLineDependentCollectionTest : EU.Business.Declaration.Testing.InvoiceLineDependentCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			return new InvoiceLineDependentCollection(invoice);
		}

		public void TestElementType()
		{
			var collection = (InvoiceLineDependentCollection)GetCollectionToTest();
			var invoiceLine = collection.AddNew();
			AssertType<JobComInvoiceLine>(invoiceLine);
			AssertType<JobComInvoiceLine>(collection[0]);
		}
	}
}
