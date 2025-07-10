using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideInvoiceAddressHelper))]
	public class OverrideInvoiceAddressHelperTest : OverrideInvoiceDetailsHelperTest
	{
		protected override void AssertBusinessContext(InvoicingBase invoice)
		{
			Assert("Invoice is in correct context", invoice.HasContext(BusinessContext.OverrideInvoiceAddressContact));
		}

		protected override void AssertWritableColumns(InvoicingBase invoice)
		{
			AssertEquals("invoice.DisplayInvoiceAddressOverride.ReadOnly", false, invoice.DisplayInvoiceAddressOverrideInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoicePKs != null ? new OverrideInvoiceAddressHelper(Factory, null, DefaultAddressPK, false, InvoicePKs) : new OverrideInvoiceAddressHelper(Factory, null, DefaultAddressPK, false, Factory.NewWithValidTestData<ARInvoice>().PK);
		}

		protected override void AssertDefaultValues(InvoicingBase invoice)
		{
			AssertEquals("Default address should be set", DefaultAddressPK, invoice.AH_OA_InvoiceAddressOverride);
		}
	}
}
