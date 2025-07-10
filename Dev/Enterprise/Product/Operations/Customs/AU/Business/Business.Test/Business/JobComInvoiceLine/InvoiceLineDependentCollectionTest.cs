using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InvoiceLineDependentCollection))]
	public class InvoiceLineDependentCollectionTest : Customs.Business.Testing.InvoiceLineDependentCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			return new InvoiceLineDependentCollection(invoice);
		}
	}
}
