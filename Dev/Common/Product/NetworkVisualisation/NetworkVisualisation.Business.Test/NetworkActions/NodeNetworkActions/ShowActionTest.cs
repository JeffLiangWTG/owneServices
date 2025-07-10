using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class ShowActionTest : NodeNetworkActionsTestCase
	{
		public void TestApplicability()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new ShowAction(networkViewModel);

			var entityWithoutSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.None);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The entity should support showing hidden objects on the diagram.", action.IsApplicableToEntity(entityWithoutSupport));

			var entityWithSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.Show);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableToEntity(entityWithSupport));
		}

		public void TestEnabledness()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new ShowAction(networkViewModel);

			var entityWithSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.Show);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(entityWithSupport));
		}

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new ShowAction(networkViewModel), string.Empty);
		}
	}
}
