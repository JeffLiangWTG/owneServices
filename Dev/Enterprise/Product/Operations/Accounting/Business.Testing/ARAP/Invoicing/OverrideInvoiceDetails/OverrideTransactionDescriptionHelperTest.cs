using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideTransactionDescriptionHelper))]
	public class OverrideTransactionDescriptionHelperTest : OverrideInvoiceDetailsHelperTest
	{
		protected override void AssertBusinessContext(InvoicingBase invoice)
		{
			Assert("Invoice is in correct context", invoice.HasContext(BusinessContext.OverrideTransactionDescription));
		}

		protected override void AssertWritableColumns(InvoicingBase invoice)
		{
			AssertEquals("invoice.AH_DescInfo.ReadOnly", false, invoice.AH_DescInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoicePKs != null ? new OverrideTransactionDescriptionHelper(Factory, InvoicePKs) : new OverrideTransactionDescriptionHelper(Factory, Factory.NewWithValidTestData<ARInvoice>().PK);
		}
	}
}
