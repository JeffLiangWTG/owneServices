namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ReExportJobComInvoiceLineValidation : CommonExportJobComInvoiceLineValidation
	{
		public ReExportJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
		{
		}

		protected override bool IsTariffMandatory => false;

		protected override void CheckJI_Tariff_NoPackage()
		{
		}
	}
}
