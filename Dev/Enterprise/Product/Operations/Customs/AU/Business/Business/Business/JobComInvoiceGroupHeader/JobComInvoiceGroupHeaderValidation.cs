namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobComInvoiceGroupHeaderValidation : Customs.Business.JobComInvoiceGroupHeaderValidation
	{
		public JobComInvoiceGroupHeaderValidation(JobComInvoiceGroupHeader groupHeader)
			: base(groupHeader)
		{
		}

		protected override Customs.Business.ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}
	}
}
