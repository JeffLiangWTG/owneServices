using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCusLinkPackageCollection))]
	class InvoiceLineCusLinkPackageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InvoiceLineCusLinkPackageCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

		protected override InvoiceLineCusLinkPackageCollection GetCollectionToTest()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			return new InvoiceLineCusLinkPackageCollection(invoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			return new InvoiceLineCusLinkPackage(invoiceLine);
		}
	}
}
