using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.TaxFramework
{
	class ConsolPostedProfitShareAndMasterFreightCollectChargesAPInvoiceTaxableLine_GetTaxCalculationParametersTest : ConsolPostedTaxableLine_GetTaxCalculationParametersTest
	{
		protected override InvoicingLineBase CreateInvoiceLine(int attemptNumber, OrgHeader invoiceOrg)
		{
			return GetInvoiceLine(attemptNumber, JobInvoicingPostingOption.Agent, invoiceOrg, null);
		}

		protected override InvoicingLineBase GetLineFromInvoice(InvoicingBase invoiceBase)
		{
			AssertEquals("Precondition: Invoice Line Count", 2, invoiceBase.Lines.Count);
			if (invoiceBase.Lines[0].AL_AC == Creator.FRT.PK)
			{
				return invoiceBase.Lines[0];
			}
			else
			{
				return invoiceBase.Lines[1];
			}
		}

		protected override JobInvoicingPostingOption GetJobInvoicingPostingOption()
		{
			return JobInvoicingPostingOption.Agent;
		}

		protected override ForwardingConsol CreateConsol(string origin, string destination, string consolNum, OrgHeader invoiceOrg = null)
		{
			return Creator.CreateConsol(origin, destination, consolNum, sendingForwarderAddress: invoiceOrg);
		}
	}
}
