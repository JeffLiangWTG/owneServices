using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.DistanceCalculator.Test
{
	public class DistanceCalculatorPcMilerBillingSystemTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			DistanceCalculatorPcMilerBillingSystem billing = new DistanceCalculatorPcMilerBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.DistanceCalculatorPcMiler, billing.SystemCode);
			AssertEquals("PCM", billing.eHub_TransactionSubType);
		}
	}
}