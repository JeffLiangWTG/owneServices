using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.DistanceCalculator
{
	public class DistanceCalculatorPcMilerBillingSystem : DistanceCalculatorBillingSystem
	{
		public DistanceCalculatorPcMilerBillingSystem()
			: base(BillingConstants.BillingSystem.DistanceCalculatorPcMiler, "PCM")
		{
		}
	}
}

