using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.DistanceCalculator
{
	public class DistanceCalculatorGenericBillingSystem : DistanceCalculatorBillingSystem
	{
		public DistanceCalculatorGenericBillingSystem()
			: base(BillingConstants.BillingSystem.DistanceCalculatorGeneric, "GOO")
		{
		}
	}
}

