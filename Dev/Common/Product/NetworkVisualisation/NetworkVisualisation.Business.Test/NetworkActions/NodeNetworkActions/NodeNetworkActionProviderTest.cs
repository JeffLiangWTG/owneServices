using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class NodeNetworkActionProviderTest : TestCase
	{
		public void TestActions()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var node = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel);
			var actions = NodeNetworkActionProvider.GetNetworkActions(networkViewModel).ToArray();

			var expectedActionTypes = new Type[]
			{
				typeof(CreateNewAction),
				typeof(ShowAction),
				typeof(ImportAction),
				typeof(AffinitiesAction),
				typeof(EditPropertiesAction),
				typeof(RemoveAndDeleteAction),
				typeof(RemoveFromDiagramAction),
				typeof(BringToFrontAction),
				typeof(SendToBackAction)
			};

			CombineAssertions(() =>
			{
				for (int i = 0; i < actions.Length; i++)
				{
					AssertEquals(expectedActionTypes[i], actions[i].GetType());
				}
			});
		}

		public void TestMergesActionsIntoMenuItems()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var node = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity");

			network.AddCustomNetworkAction_ForTest(new StaticNetworkAction(ResString.GetMultilingualString("TestMergesActionsIntoMenuItems: Special Custom Action", "Special Custom Action"))
			{
				Group = 10,
				GroupIndex = 15
			});
			network.AddCreateEntityAction_ForTest(new StaticNetworkAction(ResString.GetMultilingualString("TestMergesActionsIntoMenuItems: Special Create Action", "Special Create Action")));

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, node.Entity))
			{
				var items = NodeNetworkActionProvider.GetNetworkActions(networkViewModel).ToMenuItemsGrouped().ToArray();
				AssertEquals("Create New...", items[0].Name);
				AssertEquals("Special Custom Action", items[1].Name);
				AssertEquals(null, items[2]);
				AssertEquals("Remove and Delete Entity", items[3].Name);
			}
		}
	}
}
