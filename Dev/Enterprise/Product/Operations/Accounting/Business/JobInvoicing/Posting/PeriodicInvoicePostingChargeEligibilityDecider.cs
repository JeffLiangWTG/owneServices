namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class PeriodicInvoicePostingChargeEligibilityDecider : PostingChargeEligibilityDecider
	{
		public PeriodicInvoicePostingChargeEligibilityDecider(Charge[] charges, System.Predicate<Charge> isEligibleForPosting)
			: base(charges, isEligibleForPosting)
		{
		}

		protected override bool IsPostingAllowedForDeferredCharge(IReceivablesPostingCharge chargeBeingDecided)
		{
			return chargeBeingDecided.IsDeferredCharge;
		}

		protected override bool IsAllowedToPostThisCharge(IReceivablesPostingCharge chargeBeingDecided)
		{
			return true;
		}
	}
}
