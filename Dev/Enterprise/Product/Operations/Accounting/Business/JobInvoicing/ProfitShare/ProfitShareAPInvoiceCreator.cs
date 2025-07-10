namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public class ProfitShareAPInvoiceCreator : APInvoiceCreator
	{
		public ProfitShareAPInvoiceCreator(Charge chargeToPost)
			: base(chargeToPost.InvoicingJob)
		{
			this.ChargeToPost = chargeToPost;
		}

		readonly Charge ChargeToPost;

		protected override bool ShouldPostAgentRelatedCharge(Charge charge)
		{
			return base.ShouldPostAgentRelatedCharge(charge) && charge.PK == ChargeToPost.PK;
		}
	}
}
