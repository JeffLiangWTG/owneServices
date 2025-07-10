using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.TaxFramework
{
	class ARInvoiceTaxableLineGetTaxCalculationParametersTest : NonJobRelatedInvoicingLineBaseTaxableGetTest_TaxCalculationParametersTest
	{
		protected override InvoicingLineBase CreateInvoiceLine(int attemptNumber, OrgHeader invoiceOrg)
		{
			var invoice = Creator.CreateInvoice(typeof(ARInvoice), organisation: invoiceOrg);
			var line = Creator.CreateInvoiceLine(invoice, 100M);
			return line;
		}

		protected override CostSell GetExpectedCostOrSell()
		{
			return CostSell.Revenue;
		}
	}
}
