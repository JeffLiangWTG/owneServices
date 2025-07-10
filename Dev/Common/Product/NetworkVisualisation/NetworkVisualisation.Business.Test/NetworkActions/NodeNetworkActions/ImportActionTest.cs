using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class ImportActionTest : NodeNetworkActionsTestCase
	{
		public void TestApplicability()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new ImportAction(networkViewModel);

			var entityWithoutSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.None);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The entity should support child entities.", action.IsApplicableToEntity(entityWithoutSupport));

			var entityWithSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.AddChildEntities);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableToEntity(entityWithSupport));
		}

		public void TestEnabledness()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new ImportAction(networkViewModel);

			var entityWithSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.AddChildEntities);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(entityWithSupport));
		}

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new ImportAction(networkViewModel), string.Empty);
		}
	}
}
