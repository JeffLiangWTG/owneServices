using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class InvoiceAndDueDateCalculatorForTest : InvoiceAndDueDateCalculator
	{
		public InvoiceAndDueDateCalculatorForTest(IJobInvoicingPlugIn plugIn, ZDateTime initialInvoiceDate, OrgHeader organisation, JobInvoicingConsumerType jobType, ZString direction, ZString transportMode, ZGuid branchPK, ZGuid departmentPK, string ledger, string invoiceType)
				: base(plugIn, initialInvoiceDate, organisation, jobType, direction, transportMode, branchPK, departmentPK, ledger, invoiceType)
		{
		}

		public InvoiceAndDueDateCalculatorForTest(IJobInvoicingPlugIn plugIn, ZDateTime initialInvoiceDate, InvoiceTerm invoiceTerm)
				: base(plugIn, initialInvoiceDate, invoiceTerm)
		{
		}

		public new ZDateTime GetDateByInvoiceTerm()
		{
			return base.GetDateByInvoiceTerm();
		}
	}
}