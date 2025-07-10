using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	[TestedType(typeof(TransactionLineForOtherTaxesDisplayCollection))]
	public class TransactionLineForOtherTaxesDisplayCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TransactionLineForOtherTaxesDisplayCollection>
	{
		protected override TransactionLineForOtherTaxesDisplayCollection GetCollectionToTest()
		{
			return new TransactionLineForOtherTaxesDisplayCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = creator.CreateInvoiceLine(invoice, 100M);
			var elementOfCollection = TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(line);
			return elementOfCollection;
		}

		protected override void SetUp()
		{
			creator = new TestObjectCreator(Factory);
			invoice = creator.CreateInvoice(typeof(APInvoice));
		}

		InvoicingBase invoice;
		TestObjectCreator creator;
	}
}
