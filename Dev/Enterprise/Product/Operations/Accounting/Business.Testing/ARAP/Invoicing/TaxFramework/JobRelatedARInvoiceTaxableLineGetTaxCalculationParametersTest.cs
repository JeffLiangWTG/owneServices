using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.TaxFramework
{
	class JobRelatedARInvoiceTaxableLineGetTaxCalculationParametersTest : JobRelatedTaxableLine_GetTaxCalculationParametersTest
	{
		protected override InvoicingBase CreateInvoice(OrgHeader invoiceOrg)
		{
			var invoice = Creator.CreateInvoice(typeof(ARInvoice), organisation: invoiceOrg);
			return invoice;
		}

		protected override CostSell GetExpectedCostOrSell()
		{
			return CostSell.Revenue;
		}
	}
}
