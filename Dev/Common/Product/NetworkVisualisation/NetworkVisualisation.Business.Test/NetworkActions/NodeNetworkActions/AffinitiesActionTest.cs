using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class AffinitiesActionTest : NodeNetworkActionsTestCase
	{
		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new AffinitiesAction(networkViewModel), string.Empty);
		}

		public void TestApplicability()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new AffinitiesAction(networkViewModel);

			var entityWithoutSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.None);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The entity should support affinities.", action.IsApplicableToEntity(entityWithoutSupport));

			var entityWithSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.Affinities);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableToEntity(entityWithSupport));
		}

		public void TestEnabledness()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new AffinitiesAction(networkViewModel);

			var entityWithSupport = CreateEntityToSupportActions(networkViewModel, NetworkActions.Affinities);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(entityWithSupport));
		}
	}
}
