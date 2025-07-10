using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideInvoiceAddressContactHelper))]
	public class OverrideInvoiceAddressContactHelperTest : OverrideInvoiceDetailsHelperTest
	{
		protected override void AssertBusinessContext(InvoicingBase invoice)
		{
			Assert("Invoice is in correct context", invoice.HasContext(BusinessContext.OverrideInvoiceAddressContact));
		}

		protected override void AssertWritableColumns(InvoicingBase invoice)
		{
			AssertEquals("invoice.DisplayInvoiceAddressOverride.ReadOnly", false, invoice.DisplayInvoiceAddressOverrideInfo.ReadOnly);
			AssertEquals("invoice.DisplayInvoiceContactOverride.ReadOnly", false, invoice.DisplayInvoiceContactOverrideInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoicePKs != null ? new OverrideInvoiceAddressContactHelper(Factory, InvoicePKs) : new OverrideInvoiceAddressContactHelper(Factory, Factory.NewWithValidTestData<ARInvoice>().PK);
		}

		public void TestPaymentCollection()
		{
			var arPayment = Factory.NewWithValidTestData<ARPayment>();
			var apPayment = Factory.NewWithValidTestData<APPayment>();

			AssertEquals(false, arPayment.AH_OSExTaxAmountInfo.ReadOnly);
			AssertEquals(false, apPayment.AH_OSExTaxAmountInfo.ReadOnly);

			InvoicePKs = new ZGuid[] { arPayment.PK, apPayment.PK };

			OverrideInvoiceDetailsHelper testObject = (OverrideInvoiceDetailsHelper)GetNewBusinessObject();
			AssertEquals(2, testObject.WrappedObjects.Count);
			AssertEquals(false, testObject.WrappedObjects.AllowNew);
			AssertCollectionContains(arPayment, testObject.WrappedObjects);
			AssertCollectionContains(apPayment, testObject.WrappedObjects);

			arPayment.HasContext(BusinessContext.OverrideInvoiceAddressContact);
			apPayment.HasContext(BusinessContext.OverrideInvoiceAddressContact);
			AssertEquals(true, arPayment.AH_OSExTaxAmountInfo.ReadOnly);
			AssertEquals(true, apPayment.AH_OSExTaxAmountInfo.ReadOnly);

			AssertEquals(false, arPayment.DisplayInvoiceAddressOverrideInfo.ReadOnly);
			AssertEquals(false, apPayment.DisplayInvoiceAddressOverrideInfo.ReadOnly);

			AssertEquals(false, apPayment.DisplayInvoiceContactOverrideInfo.ReadOnly);
			AssertEquals(false, arPayment.DisplayInvoiceContactOverrideInfo.ReadOnly);
		}
	}
}
