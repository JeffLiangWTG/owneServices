using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideGovernmentAllocatedNumberHelper))]
	public class OverrideGovernmentAllocationNumberTest : OverrideInvoiceDetailsHelperTest
	{
		protected override void AssertBusinessContext(InvoicingBase invoice)
		{
			Assert("Invoice is in correct context", invoice.HasContext(BusinessContext.OverrideGovernmentAllocatedID));
		}

		protected override void AssertWritableColumns(InvoicingBase invoice)
		{
			invoice = GetNewInvoiceBaseOnSecurity();
			AssertEquals("invoice.AH_OSTotalAmountInfo.ReadOnly", true, invoice.AH_OSTotalAmountInfo.ReadOnly);
			AssertEquals("invoice.AH_DescInfo.ReadOnly", true, invoice.AH_DescInfo.ReadOnly);
			AssertEquals("invoice.AH_TransactionTypeInfo.ReadOnly", true, invoice.AH_TransactionTypeInfo.ReadOnly);
			AssertEquals("invoice.AH_TransactionCategoryInfo.ReadOnly", true, invoice.AH_TransactionCategoryInfo.ReadOnly);
			AssertEquals("invoice.AH_JHInfo.ReadOnly", true, invoice.AH_JHInfo.ReadOnly);
			AssertEquals("invoice.AH_RX_NKTransactionCurrencyInfo.ReadOnly", true, invoice.AH_RX_NKTransactionCurrencyInfo.ReadOnly);
			AssertEquals("invoice.AH_OSTaxAmountInfo.ReadOnly", true, invoice.AH_OSTaxAmountInfo.ReadOnly);
			AssertEquals("invoice.AH_OSExTaxAmountInfo.ReadOnly", true, invoice.AH_OSExTaxAmountInfo.ReadOnly);
			AssertEquals("invoice.AH_InvoiceDateInfo.ReadOnly", true, invoice.AH_InvoiceDateInfo.ReadOnly);
			AssertEquals("invoice.AH_DueDateInfo.ReadOnly", true, invoice.AH_DueDateInfo.ReadOnly);
			AssertEquals("invoice.AH_ChequeOrReferenceInfo.ReadOnly", true, invoice.AH_ChequeOrReferenceInfo.ReadOnly);
			AssertEquals("invoice.AH_GovernmentAllocatedIDInfo.ReadOnly", false, invoice.AH_GovernmentAllocatedIDInfo.ReadOnly);
		}

		protected InvoicingBase GetNewInvoiceBaseOnSecurity()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			InvoicePKs = new ZGuid[] { invoice.PK };
			OverrideInvoiceDetailsHelper testObject = (OverrideInvoiceDetailsHelper)GetNewBusinessObject();
			AssertEquals("InvoiceCollection.Count", 1, testObject.WrappedObjects.Count);
			return invoice;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoicePKs != null ? new OverrideGovernmentAllocatedNumberHelper(Factory, InvoicePKs) : new OverrideGovernmentAllocatedNumberHelper(Factory, Factory.NewWithValidTestData<APInvoice>().PK);
		}
	}
}
