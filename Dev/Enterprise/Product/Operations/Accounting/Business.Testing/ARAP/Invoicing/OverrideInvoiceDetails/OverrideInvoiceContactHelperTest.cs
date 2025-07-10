using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideInvoiceContactHelper))]
	public class OverrideInvoiceContactHelperTest : OverrideInvoiceDetailsHelperTest
	{
		protected override void AssertBusinessContext(InvoicingBase invoice)
		{
			Assert("Invoice is in correct context", invoice.HasContext(BusinessContext.OverrideInvoiceAddressContact));
		}

		protected override void AssertWritableColumns(InvoicingBase invoice)
		{
			AssertEquals("invoice.DisplayInvoiceContactOverride.ReadOnly", false, invoice.DisplayInvoiceContactOverrideInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoicePKs != null ? new OverrideInvoiceContactHelper(Factory, null, DefaultContactPK, false, InvoicePKs) : new OverrideInvoiceContactHelper(Factory, null, DefaultContactPK, false, Factory.NewWithValidTestData<ARInvoice>().PK);
		}

		protected override void AssertDefaultValues(InvoicingBase invoice)
		{
			AssertEquals("Default contact should be set", DefaultContactPK, invoice.AH_OC_InvoiceContactOverride);
		}
	}
}
