using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceForBulkPosterCollection))]
	public class APInvoiceForBulkPosterCollectionTest : InvoicingBaseCollectionTest
	{
		public APInvoiceForBulkPosterCollectionTest()
		{
			shouldRaiseNoConcreteTypeExceptionWhenAddingNew = false;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APInvoiceForBulkPosterCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(APInvoiceForBulkPoster));
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Base_TestAddAndCancelOfElementAsThoughBinding();
		}
	}
}
