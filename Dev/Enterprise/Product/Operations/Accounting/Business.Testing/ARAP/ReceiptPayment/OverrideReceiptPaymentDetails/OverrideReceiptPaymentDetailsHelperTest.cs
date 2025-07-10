using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public abstract class OverrideReceiptPaymentDetailsHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReceiptPaymentCollection()
		{
			ARReceipt receipt = Factory.NewWithValidTestData<ARReceipt>();
			ARPayment payment = Factory.NewWithValidTestData<ARPayment>();

			AssertEquals("Precondition: receipt.AH_OSExTaxAmountInfo.ReadOnly", false, receipt.AH_OSExTaxAmountInfo.ReadOnly);
			AssertEquals("Precondition: payment.AH_OSExTaxAmountInfo.ReadOnly", false, payment.AH_OSExTaxAmountInfo.ReadOnly);

			ReceiptPaymentPKs = new ZGuid[] { receipt.PK, payment.PK };

			OverrideReceiptPaymentDetailsHelper testObject = (OverrideReceiptPaymentDetailsHelper)GetNewBusinessObject();
			AssertEquals("InvoiceCollection.Count", 2, testObject.WrappedObjects.Count);
			AssertCollectionContains(receipt, testObject.WrappedObjects);
			AssertCollectionContains(payment, testObject.WrappedObjects);

			AssertBusinessContext(receipt);
			AssertEquals("receipt.AH_OSExTaxAmountInfo.ReadOnly", true, receipt.AH_OSExTaxAmountInfo.ReadOnly);
			AssertWritableColumns(receipt);

			AssertBusinessContext(payment);
			AssertEquals("payment.AH_OSExTaxAmountInfo.ReadOnly", true, payment.AH_OSExTaxAmountInfo.ReadOnly);
			AssertWritableColumns(payment);
		}

		protected abstract void AssertBusinessContext(ReceiptPaymentBase receiptPayment);
		protected abstract void AssertWritableColumns(ReceiptPaymentBase receiptPayment);

		protected virtual void AssertDefaultValues(ReceiptPaymentBase receiptPayment)
		{
		}

		public ZGuid[] ReceiptPaymentPKs { get; set; }
	}
}