namespace Enterprise.Customs.FI.Business;

public class JobComInvoiceHeaderLookups : EU.Business.Declaration.JobComInvoiceHeaderLookups
{
	public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent) : base(parent)
	{
	}

	protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;
}
