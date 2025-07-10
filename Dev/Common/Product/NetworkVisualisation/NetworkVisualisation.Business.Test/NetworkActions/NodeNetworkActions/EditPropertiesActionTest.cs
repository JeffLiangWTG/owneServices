using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class EditPropertiesActionTest : NodeNetworkActionsTestCase
	{
		public void TestApplicability()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new EditPropertiesAction(networkViewModel);

			var entityWithoutSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.None);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The entity should support properties editing.", action.IsApplicableToEntity(entityWithoutSupport));

			var entityWithSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.EditEntity);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableToEntity(entityWithSupport));
		}

		public void TestEnabledness()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new EditPropertiesAction(networkViewModel);

			var entityWithSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.EditEntity);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(entityWithSupport));
		}

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new EditPropertiesAction(networkViewModel), "Cog");
		}
	}
}
