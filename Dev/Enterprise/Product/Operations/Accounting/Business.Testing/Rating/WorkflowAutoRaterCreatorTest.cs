namespace Enterprise.Accounting.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Freight.Forwarding.Business;

	public class WorkflowAutoRaterCreatorTest : TestCaseWithFactory
	{
		public void TestCreateWorkflowAutoRater()
		{
			var creator = new WorkflowAutoRaterCreator();
			var result = creator.CreateWorkflowAutoRater(
				Factory.New<ForwardingShipment>(),
				autoRateRevenue: true,
				autoRateCosts: true,
				excludeConsolLevelCharges: false);

			AssertType(typeof(WorkflowAutoRater), result);
		}
	}
}