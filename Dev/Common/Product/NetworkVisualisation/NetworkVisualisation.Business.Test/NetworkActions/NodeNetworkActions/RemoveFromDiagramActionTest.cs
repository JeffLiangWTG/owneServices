namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class RemoveFromDiagramActionTest : NodeNetworkActionsTestCase
	{
		public void TestApplicability()
		{
			AssertActionIsNotAllowedForRootDiagramButAllowedForChildShapes((networkViewModel) => new RemoveFromDiagramAction(networkViewModel), AccessibilityLevel.Applicability);
		}

		public void TestEnabledness()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new RemoveFromDiagramAction(networkViewModel);

			var entityWithoutSupport = CreateEntityToSupportActions(networkViewModel, Integration.NetworkActions.None);
			entityWithoutSupport.Parent = network.DiagramEntity;
			Assert("Precondition", !entityWithoutSupport.IsRoot());

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The entity should support removing from diagram.", action.IsEnabledForEntity(entityWithoutSupport));

			var entityWithSupport = CreateEntityToSupportActions(networkViewModel, Integration.NetworkActions.Hide);
			entityWithSupport.Parent = network.DiagramEntity;
			Assert("Precondition", !entityWithSupport.IsRoot());

			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(entityWithSupport));
		}

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new RemoveFromDiagramAction(networkViewModel), string.Empty);
		}
	}
}
