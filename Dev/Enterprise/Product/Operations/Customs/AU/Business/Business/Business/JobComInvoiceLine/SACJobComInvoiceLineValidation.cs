namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SACJobComInvoiceLineValidation : CMRImportJobComInvoiceLineValidation
	{
		public SACJobComInvoiceLineValidation(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override bool IsTariffMandatory
		{
			get { return true; }
		}

		protected override void CheckJI_IsPackToBondForLine()
		{
			if (Parent.JI_IsPackToBondForLine)
			{
				Parent.JI_IsPackToBondForLineInfo.AddMessageError("SAC Nature 20 declaration is not allowed");
			}
		}
	}
}
