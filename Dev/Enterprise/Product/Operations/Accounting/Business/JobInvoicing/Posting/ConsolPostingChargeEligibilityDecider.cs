using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class ConsolPostingChargeEligibilityDecider : PostingChargeEligibilityDecider
	{
		public ConsolPostingChargeEligibilityDecider(IJobCostingPlugIn consol, Charge[] charges)
			: base(charges)
		{
			Consol = consol;
		}

		readonly IJobCostingPlugIn Consol;

		protected override bool IsAgentCharge(IReceivablesPostingCharge chargeBeingDecided)
		{
			return Consol.IsAgentCharge(chargeBeingDecided as Charge);
		}

		protected override bool IsGatewayCharge(IReceivablesPostingCharge chargeBeingDecided)
		{
			return base.IsGatewayCharge(chargeBeingDecided)
				|| Consol.IsGatewayCharge(chargeBeingDecided as Charge);
		}
	}
}
