using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CopyCommercialInvoiceModuleCollection))]
	public class CopyCommercialInvoiceModuleCollectionTest : ActiveBusinessObjectCollectionTestCase<CopyCommercialInvoiceModuleCollection>
	{
		public void TestRelationshipFilter()
		{
			var invoice1 = Factory.New<JobDeclaration>().Invoices.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();
			var invoice2 = Factory.New<JobComInvoiceHeader>();
			invoice2.InvoiceLines.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();

			var collection = new CopyCommercialInvoiceModuleCollection(declaration);
			AssertContainsExactElementsInAnyOrder(new[] { invoice1, invoice2 }, collection);
		}

		protected override CopyCommercialInvoiceModuleCollection GetCollectionToTest() => new CopyCommercialInvoiceModuleCollection(Factory.New<JobDeclaration>());
	}
}
