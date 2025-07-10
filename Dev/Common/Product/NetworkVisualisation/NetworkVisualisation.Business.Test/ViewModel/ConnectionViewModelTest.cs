using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	class ConnectionViewModelTest : TestCase
	{
		public void TestCanHide()
		{
			var network = new DummyNetwork();
			var entity = new Entity { SupportedActions = NetworkActions.Hide | NetworkActions.Show };
			var networkViewModel = new NetworkViewModel(network);
			var nodeViewModel = new NodeViewModel(entity, networkViewModel);
			var connectorViewModel = new ConnectorViewModel("") { ParentNode = nodeViewModel };
			var connectionViewModel = new ConnectionViewModel { DestConnector = connectorViewModel };

			AssertEquals(true, connectionViewModel.CanHide);

			entity.SupportedActions = NetworkActions.None;
			AssertEquals(false, connectionViewModel.CanHide);
		}

		public void TestIsVisible_ShouldBeVisibleBeforeRelationshipSet()
		{
			var relationship = new Relationship { IsVisible = false };
			var connectionViewModel = new ConnectionViewModel();
			AssertEquals(true, connectionViewModel.IsVisible);

			connectionViewModel.Relationship = relationship;
			AssertEquals(false, connectionViewModel.IsVisible);

			relationship.IsVisible = true;
			AssertEquals(true, connectionViewModel.IsVisible);
		}
	}
}
