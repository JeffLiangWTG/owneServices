namespace Enterprise.Customs.FI.Business;

public class JobComInvoiceLineValidation : EU.Business.Declaration.JobComInvoiceLineValidation
{
	public JobComInvoiceLineValidation(JobComInvoiceLine parent)
		: base(parent)
	{
	}

	protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;
}
