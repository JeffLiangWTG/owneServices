using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.DistanceCalculator.Test
{
	public class DistanceCalculatorGenericBillingSystemTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			DistanceCalculatorGenericBillingSystem billing = new DistanceCalculatorGenericBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.DistanceCalculatorGeneric, billing.SystemCode);
			AssertEquals("GOO", billing.eHub_TransactionSubType);
		}
	}
}