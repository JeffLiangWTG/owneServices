using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.TaxFramework
{
	class ConsolPostedAPInvoiceTaxableLine_GetTaxCalculationParametersTest : ConsolPostedTaxableLine_GetTaxCalculationParametersTest
	{
		protected override InvoicingLineBase CreateInvoiceLine(int attemptNumber, OrgHeader invoiceOrg)
		{
			return GetInvoiceLine(attemptNumber, JobInvoicingPostingOption.ConsolCosts, invoiceOrg, null);
		}

		protected override InvoicingLineBase GetLineFromInvoice(InvoicingBase invoiceBase)
		{
			AssertEquals("Precondition: Invoice Line Count", 1, invoiceBase.Lines.Count);
			return invoiceBase.Lines[0];
		}

		protected override JobInvoicingPostingOption GetJobInvoicingPostingOption()
		{
			return JobInvoicingPostingOption.ConsolCosts;
		}

		protected override ForwardingConsol CreateConsol(string origin, string destination, string consolNum, OrgHeader invoiceOrg = null)
		{
			return Creator.CreateConsol(origin, destination, consolNum);
		}
	}
}
