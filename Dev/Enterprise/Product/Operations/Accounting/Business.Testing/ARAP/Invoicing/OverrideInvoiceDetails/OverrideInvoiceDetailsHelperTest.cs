using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class OverrideInvoiceDetailsHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInvoiceCollection()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ARCreditNote creditNote = Factory.NewWithValidTestData<ARCreditNote>();

			InvoicePKs = new ZGuid[] { invoice.PK, creditNote.PK };

			OverrideInvoiceDetailsHelper testObject = (OverrideInvoiceDetailsHelper)GetNewBusinessObject();
			AssertEquals("InvoiceCollection.Count", 2, testObject.WrappedObjects.Count);
			AssertEquals(false, testObject.WrappedObjects.AllowNew);
			AssertCollectionContains(invoice, testObject.WrappedObjects);
			AssertCollectionContains(creditNote, testObject.WrappedObjects);

			AssertBusinessContext(invoice);
			AssertEquals("invoice.AH_OSExTaxAmountInfo.ReadOnly", true, invoice.AH_OSExTaxAmountInfo.ReadOnly);
			AssertWritableColumns(invoice);

			AssertBusinessContext(creditNote);
			AssertEquals("creditNote.AH_OSExTaxAmountInfo.ReadOnly", true, creditNote.AH_OSExTaxAmountInfo.ReadOnly);
			AssertWritableColumns(creditNote);
		}

		protected abstract void AssertBusinessContext(InvoicingBase invoice);
		protected abstract void AssertWritableColumns(InvoicingBase invoice);

		protected virtual void AssertDefaultValues(InvoicingBase invoice)
		{
		}

		protected ZGuid[] InvoicePKs;

		protected ZGuid DefaultAddressPK = ZGuid.Empty;
		protected ZGuid DefaultContactPK = ZGuid.Empty;
	}
}
