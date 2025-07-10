using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideInvoiceRemittanceTypeHelper))]
	public class OverrideInvoiceRemittanceTypeHelperTest : OverrideInvoiceDetailsHelperTest
	{
		protected override void AssertBusinessContext(InvoicingBase invoice)
		{
			Assert("Invoice is in correct context", invoice.HasContext(BusinessContext.OverrideInvoiceRemittanceType));
		}

		protected override void AssertWritableColumns(InvoicingBase invoice)
		{
			AssertEquals("invoice.AH_AgreedPaymentMethodOverrideInfo.ReadOnly", false, invoice.AH_InvoicePaymentReferenceCodeInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoicePKs != null ? new OverrideInvoiceRemittanceTypeHelper(Factory, InvoicePKs) : new OverrideInvoiceRemittanceTypeHelper(Factory, Factory.NewWithValidTestData<ARInvoice>().PK);
		}
	}
}
