
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Business.Declaration
{
	[CodeAlive("Template")]
	public class JobComInvoiceHeaderLookups : EU.Business.Declaration.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;
	}
}
