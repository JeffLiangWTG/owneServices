using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceLineTaxCollection : DependentBusinessObjectCollection<JobComInvoiceLineTax, JobComInvoiceLine>
	{
		public JobComInvoiceLineTaxCollection(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{ }
	}
}
