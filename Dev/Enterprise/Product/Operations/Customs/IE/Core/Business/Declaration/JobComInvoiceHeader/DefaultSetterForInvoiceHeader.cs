using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class DefaultSetterForInvoiceHeader : EU.Business.DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(JobComInvoiceHeader child, JobDeclaration declaration) : base(child, declaration)
		{
		}
		protected new JobDeclaration declaration => (JobDeclaration)base.declaration;

		protected override void SetDefaultIncoTerm(BaseJobComInvoiceHeader previousInvoice)
		{
			if (!declaration.IsExitSummary && !declaration.IsReExport)
			{
				base.SetDefaultIncoTerm(previousInvoice);
			}
		}
	}
}
