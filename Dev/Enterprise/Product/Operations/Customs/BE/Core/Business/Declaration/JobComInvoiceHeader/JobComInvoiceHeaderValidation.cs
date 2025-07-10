namespace Enterprise.Customs.BE.Business.Declaration;

public class JobComInvoiceHeaderValidation : EU.Business.Declaration.JobComInvoiceHeaderValidation
{
	public JobComInvoiceHeaderValidation(JobComInvoiceHeader parent) : base(parent)
	{
	}

	protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;
}
