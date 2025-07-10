using Moq;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class CreateNewActionTest : NodeNetworkActionsTestCase
	{
		public void TestApplicabilityAndEnabledness()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new CreateNewAction(networkViewModel);
			var childAction = mocks.Create<IDynamicNetworkAction>();
			network.AddCreateEntityAction_ForTest(childAction.Object);

			var entityWithoutSupport = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel).Entity;
			var entityWithSupport = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel).Entity;

			childAction.Setup(m => m.IsApplicableToEntity(entityWithoutSupport)).Returns(NetworkActionAccessibility.Denied_ForTesting);
			childAction.Setup(m => m.IsApplicableToEntity(entityWithSupport)).Returns(NetworkActionAccessibility.Allowed);

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The entity should support create entity actions.", action.IsApplicableToEntity(entityWithoutSupport));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableToEntity(entityWithSupport));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(entityWithSupport));
		}

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new CreateNewAction(networkViewModel), string.Empty);
		}
	}
}
