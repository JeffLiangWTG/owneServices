namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class RemoveAndDeleteActionTest : NodeNetworkActionsTestCase
	{
		public void TestApplicability()
		{
			AssertActionIsNotAllowedForRootDiagramButAllowedForChildShapes((networkViewModel) => new RemoveAndDeleteAction(networkViewModel), AccessibilityLevel.Applicability);
		}

		public void TestEnabledness()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new RemoveAndDeleteAction(networkViewModel);

			var entityWithoutSupport = CreateChildEntity(networkViewModel);
			entityWithoutSupport.CanDeleteUnderlyingEntity = false;
			Assert("Precondition", !entityWithoutSupport.IsRoot());

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The entity should represent an object which can be deleted from the database.", action.IsEnabledForEntity(entityWithoutSupport));

			var entityWithSupport = CreateChildEntity(networkViewModel);
			entityWithSupport.CanDeleteUnderlyingEntity = true;
			Assert("Precondition", !entityWithSupport.IsRoot());

			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(entityWithSupport));
		}

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new RemoveAndDeleteAction(networkViewModel), "Cross");
		}
	}
}
