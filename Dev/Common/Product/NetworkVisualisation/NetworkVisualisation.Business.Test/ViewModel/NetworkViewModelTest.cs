using System;
using System.Drawing;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class NetworkViewModelTest : TestCase
	{
		#region Selection

		public void TestSelection()
		{
			AssertEquals("Precondition", false, node1.IsSelected);
			AssertEquals("Precondition", false, node2.IsSelected);

			AssertContainsExactElementsInAnyOrder(Array.Empty<NodeViewModel>(), networkViewModel.SelectedNodes);

			RefreshArgs eventArgs = null;
			networkViewModel.SelectionChanged += (s, e) =>
			{
				AssertEquals(networkViewModel, s);
				eventArgs = e;
			};

			CombineAssertions("Adding node1 to selection", () =>
			{
				node1.IsSelected = true;
				AssertSelectionAndNotification(networkViewModel, new[] { node1.Entity }, eventArgs);
			});

			eventArgs = null;
			CombineAssertions("Adding node2 to selection", () =>
			{
				node2.IsSelected = true;
				AssertSelectionAndNotification(networkViewModel, new[] { node1.Entity, node2.Entity }, eventArgs);
			});

			eventArgs = null;
			CombineAssertions("Removing node1 from selection", () =>
			{
				node1.IsSelected = false;
				AssertSelectionAndNotification(networkViewModel, new[] { node2.Entity }, eventArgs);
			});

			eventArgs = null;
			CombineAssertions("Removing node2 from selection", () =>
			{
				node2.IsSelected = false;
				AssertSelectionAndNotification(networkViewModel, Array.Empty<INetworkEntity>(), eventArgs);
			});
		}

		public void TestSelectSingleEntity()
		{
			AssertEquals("Precondition", false, node1.IsSelected);
			AssertEquals("Precondition", false, node2.IsSelected);

			RefreshArgs eventArgs = null;
			networkViewModel.SelectionChanged += (s, e) =>
			{
				AssertEquals(networkViewModel, s);
				eventArgs = e;
			};

			CombineAssertions("Selecting entity1", () =>
			{
				networkViewModel.SelectSingleEntity(node1.Entity);
				AssertEquals("node1", true, node1.IsSelected);
				AssertEquals("node2", false, node2.IsSelected);
				AssertSelectionAndNotification(networkViewModel, new[] { node1.Entity }, eventArgs);
			});

			eventArgs = null;
			CombineAssertions("Selecting entity2", () =>
			{
				networkViewModel.SelectSingleEntity(node2.Entity);
				AssertEquals("node1", false, node1.IsSelected);
				AssertEquals("node2", true, node2.IsSelected);
				AssertSelectionAndNotification(networkViewModel, new[] { node2.Entity }, eventArgs);
			});

			eventArgs = null;
			CombineAssertions("Selecting nothing", () =>
			{
				networkViewModel.SelectSingleEntity(null);
				AssertEquals("node1", false, node1.IsSelected);
				AssertEquals("node2", false, node2.IsSelected);
				AssertSelectionAndNotification(networkViewModel, Array.Empty<INetworkEntity>(), eventArgs);
			});
		}

		public void TestSelectSingleEntity_ShouldNotifySelectionChangedForEachNode()
		{
			networkViewModel.SelectSingleEntity(node1.Entity);

			AssertEquals("Precondition", true, node1.IsSelected);
			AssertEquals("Precondition", false, node2.IsSelected);

			int numberOfNotifications = 0;
			networkViewModel.SelectionChanged += (s, e) =>
			{
				numberOfNotifications++;
			};

			networkViewModel.SelectSingleEntity(node2.Entity);
			AssertEquals("Should notify once for each node, so that the ribbon can be updated considering all newly-selected entities.", 2, numberOfNotifications);
		}

		public void TestShouldNotNotify_IfSelectionHasNotChanged()
		{
			AssertEquals("Precondition", false, node1.IsSelected);

			RefreshArgs eventArgs = null;
			networkViewModel.SelectionChanged += (s, e) =>
			{
				AssertEquals(networkViewModel, s);
				eventArgs = e;
			};

			CombineAssertions("Selecting entity1", () =>
			{
				networkViewModel.SelectSingleEntity(node1.Entity);
				AssertEquals("node1", true, node1.IsSelected);
				AssertSelectionAndNotification(networkViewModel, new[] { node1.Entity }, eventArgs);
			});

			eventArgs = null;
			CombineAssertions("Selecting entity1 again", () =>
			{
				networkViewModel.SelectSingleEntity(node1.Entity);
				AssertEquals("node1", true, node1.IsSelected);
				AssertContainsExactElementsInAnyOrder(new[] { node1 }, networkViewModel.SelectedNodes);
				AssertNull(eventArgs);
			});
		}

		public void TestSelectSingleEntity_ShouldNotifyFocusedEntityChanged()
		{
			AssertEquals("Precondition", false, node1.IsSelected);
			AssertEquals("Precondition", false, node2.IsSelected);

			FocusedEntityChangedEventArgs eventArgs = null;
			networkViewModel.FocusedEntityChanged += (s, e) =>
			{
				AssertEquals(networkViewModel, s);
				eventArgs = e;
			};

			CombineAssertions("Selecting entity1 with focusing", () =>
			{
				networkViewModel.SelectSingleEntity(node1.Entity, shouldFocusOnSelection: true);
				AssertNotNull(eventArgs);
				AssertEquals(node1.Entity, eventArgs.FocusedEntity);
			});

			eventArgs = null;
			CombineAssertions("Selecting entity1 with focusing again", () =>
			{
				networkViewModel.SelectSingleEntity(node1.Entity, shouldFocusOnSelection: true);
				AssertNotNull(eventArgs);
				AssertEquals(node1.Entity, eventArgs.FocusedEntity);
			});

			eventArgs = null;
			CombineAssertions("Selecting entity2 without focusing", () =>
			{
				networkViewModel.SelectSingleEntity(node2.Entity, shouldFocusOnSelection: false);
				AssertNull(eventArgs);
			});

			eventArgs = null;
			CombineAssertions("Removing selection", () =>
			{
				networkViewModel.SelectSingleEntity(null, shouldFocusOnSelection: true);
				AssertNotNull(eventArgs);
				AssertEquals(null, eventArgs.FocusedEntity);
			});
		}

		public void TestShouldChangeSelectionAndNotify_WhenDeletingSelectedNodes()
		{
			networkViewModel.SelectSingleEntity(node1.Entity);
			AssertEquals("Precondition: node1", true, node1.IsSelected);
			AssertEquals("Precondition: node2", false, node2.IsSelected);
			AssertContainsExactElementsInAnyOrder("Precondition: selected nodes", new NodeViewModel[] { node1 }, networkViewModel.SelectedNodes);

			RefreshArgs eventArgs = null;
			networkViewModel.SelectionChanged += (s, e) =>
			{
				AssertEquals(networkViewModel, s);
				eventArgs = e;
			};

			CombineAssertions("Removing selected entity1 - should change selection and notify", () =>
			{
				networkViewModel.ScheduledNodes.Remove(node1);
				AssertSelectionAndNotification(networkViewModel, Array.Empty<INetworkEntity>(), eventArgs);
			});

			eventArgs = null;
			CombineAssertions("Removing non selected entity2 - should not change selection and should not notify", () =>
			{
				networkViewModel.ScheduledNodes.Remove(node2);
				AssertContainsExactElementsInAnyOrder(Array.Empty<NodeViewModel>(), networkViewModel.SelectedNodes);
				AssertNull(eventArgs);
			});
		}

		#endregion

		#region Right Clicked Node and Active Node

		public void TestRightClickedNodeAndActiveNode()
		{
			CombineAssertions("Selecting one node", () =>
			{
				node2.IsSelected = true;
				AssertEquals("RightClickedNode", null, networkViewModel.RightClickedNode);
				AssertEquals("ActiveNode", node2, networkViewModel.ActiveNode);
			});

			CombineAssertions("Multiple selection", () =>
			{
				node1.IsSelected = true;
				AssertContainsExactElementsInAnyOrder(new NodeViewModel[] { node1, node2 }, networkViewModel.SelectedNodes);
				AssertEquals("RightClickedNode", null, networkViewModel.RightClickedNode);
				AssertEquals("ActiveNode should equal to the first node in the selection", node1, networkViewModel.ActiveNode);
			});

			CombineAssertions("Nothing is selected", () =>
			{
				networkViewModel.SelectSingleEntity(null);
				AssertEquals("RightClickedNode", null, networkViewModel.RightClickedNode);
				AssertEquals("ActiveNode", networkViewModel.DiagramNodeViewModel, networkViewModel.ActiveNode);
			});

			CombineAssertions("Right click", () =>
			{
				networkViewModel.RightClickedNode = node1;
				AssertEquals("RightClickedNode", node1, networkViewModel.RightClickedNode);
				AssertEquals("ActiveNode", node1, networkViewModel.ActiveNode);
			});

			CombineAssertions("Right click on the diagram", () =>
			{
				networkViewModel.RightClickedNode = networkViewModel.DiagramNodeViewModel;
				AssertEquals("RightClickedNode", networkViewModel.DiagramNodeViewModel, networkViewModel.RightClickedNode);
				AssertEquals("ActiveNode", networkViewModel.DiagramNodeViewModel, networkViewModel.ActiveNode);
			});

			CombineAssertions("Right click finalisation (closing context menu)", () =>
			{
				networkViewModel.RightClickedNode = null;
				AssertEquals("RightClickedNode", null, networkViewModel.RightClickedNode);
				AssertEquals("ActiveNode", networkViewModel.DiagramNodeViewModel, networkViewModel.ActiveNode);
			});
		}

		public void TestRightClickNodeAndActiveNode_ShouldNotPointToDeletedNode()
		{
			networkViewModel.RightClickedNode = node1;
			AssertEquals("Precondition: RightClickedNode", node1, networkViewModel.RightClickedNode);
			AssertEquals("Precondition: ActiveNode", node1, networkViewModel.ActiveNode);

			networkViewModel.ScheduledNodes.Remove(node1);

			AssertEquals("RightClickedNode", null, networkViewModel.RightClickedNode);
			AssertEquals("ActiveNode", networkViewModel.DiagramNodeViewModel, networkViewModel.ActiveNode);
		}

		#endregion

		#region Action Implementation

		#region Create and Remove Affinity Links

		public void TestApplyAffinities()
		{
			var affinity = new DummyAffinity(Guid.NewGuid(), "white", Color.MintCream);
			((INetworkEntity)diagramEntity).AppliedAffinities.Add(affinity);

			AssertNoExceptionThrown(() => networkViewModel.CreateAffinityLink(affinity, diagramEntity));

			AssertNoExceptionThrown(() => networkViewModel.RemoveAffinityLink(affinity, diagramEntity));
		}

		#endregion

		#region Remove Node

		public void TestRemoveFromDiagram_ShouldNotExecuteForDiagramNode_FoolProof()
		{
			var mocks = new MockRepository(MockBehavior.Loose);
			var network = mocks.Create<INetwork>();

			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.HideEntity(It.IsAny<IProposedNetworkEntity>()));
			network.Setup(n => n.EntityPositionStrategy).Returns(new EntityPositionStrategy());

			networkViewModel = new NetworkViewModel(network.Object, null);

			node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1");
			node2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity2");
			AssertEquals("Precondition", 2, networkViewModel.Nodes.Count());

			networkViewModel.RemoveFromDiagram(networkViewModel.DiagramNodeViewModel);
			AssertEquals(2, networkViewModel.Nodes.Count());

			network.Verify(m => m.HideEntity(It.IsAny<IProposedNetworkEntity>()), Times.Never);
		}

		public void TestDeleteNode_ShouldNotExecuteForDiagramNode_FoolProof()
		{
			var mocks = new MockRepository(MockBehavior.Loose);
			var network = mocks.Create<INetwork>();

			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.DeleteEntity(It.IsAny<IProposedNetworkEntity>()));
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());

			networkViewModel = new NetworkViewModel(network.Object, null);

			node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1");
			node2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity2");
			AssertEquals("Precondition", 2, networkViewModel.Nodes.Count());

			networkViewModel.DeleteNode(networkViewModel.DiagramNodeViewModel);
			AssertEquals(2, networkViewModel.Nodes.Count());

			network.Verify(m => m.DeleteEntity(It.IsAny<IProposedNetworkEntity>()), Times.Never);
		}

		#endregion

		#endregion

		#region Channels

		public void TestGetChannelYAxisRange()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var channel1 = new DummyChannel("Choc", 169);
			var channel2 = new DummyChannel("Orange", 170);
			var channel3 = new DummyChannel("Bar", 171);

			diagramEntity.DiagramChannels = new[] { channel1, channel2, channel3 };

			var viewModel = new NetworkViewModel(network);

			AssertEquals(new Range<int>(0, 169), viewModel.GetChannelYAxisRange(channel1));
			AssertEquals(new Range<int>(169, 339), viewModel.GetChannelYAxisRange(channel2));
			AssertEquals(new Range<int>(339, 510), viewModel.GetChannelYAxisRange(channel3));
		}

		public void TestGetChannelYAxisRange_ShouldAdjustToMinAndMaxHeights()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var channel1 = new DummyChannel("Choc", -69);
			var channel2 = new DummyChannel("Orange", 69);
			var channel3 = new DummyChannel("Bar", 6969);

			diagramEntity.DiagramChannels = new[] { channel1, channel2, channel3 };

			var viewModel = new NetworkViewModel(network);

			AssertEquals(new Range<int>(0, 100), viewModel.GetChannelYAxisRange(channel1));
			AssertEquals(new Range<int>(100, 200), viewModel.GetChannelYAxisRange(channel2));
			AssertEquals(new Range<int>(200, 2200), viewModel.GetChannelYAxisRange(channel3));
		}

		public void TestGetChannelYAxisRanges()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var channel1 = new DummyChannel("Choc", 100);
			var channel2 = new DummyChannel("Orange", 200);
			var channel3 = new DummyChannel("Bar", 300);

			diagramEntity.DiagramChannels = new[] { channel1, channel2, channel3 };

			var viewModel = new NetworkViewModel(network);

			AssertSequencesEqual(new[]
			{
				Tuple.Create((IDiagramChannel)channel1, new Range<int>(0, 100)),
				Tuple.Create((IDiagramChannel)channel2, new Range<int>(100, 300)),
				Tuple.Create((IDiagramChannel)channel3, new Range<int>(300, 600))
			}, viewModel.GetChannelYAxisRanges());
		}

		#endregion

		#region Non-Scheduled Section

		public void TestScheduledNodes()
		{
			((Entity)node2.Entity).IsNonScheduled = true;
			var viewModel = new NetworkViewModel(network);
			viewModel.BuildNetwork(isReloading: false);

			AssertContainsExactElementsInAnyOrder("Only scheduled entities should be included. SAD!", new[] { "Entity1" }, viewModel.ScheduledNodes.Select(x => x.Name));
		}

		public void TestNonScheduledNodes()
		{
			((Entity)node2.Entity).IsNonScheduled = true;
			var viewModel = new NetworkViewModel(network);
			viewModel.BuildNetwork(isReloading: false);

			AssertContainsExactElementsInAnyOrder("Only non-scheduled entities should be included. SAD!", new[] { "Entity2" }, viewModel.NonScheduledNodes.Select(x => x.Name));
		}

		public void TestNodes_ShouldCombineScheduledAndNonScheduledNodes()
		{
			((Entity)node2.Entity).IsNonScheduled = true;
			var viewModel = new NetworkViewModel(network);
			viewModel.BuildNetwork(isReloading: false);

			AssertContainsExactElementsInAnyOrder("Both scheduled and non-scheduled nodes should be included in the Nodes property. SAD!", new[] { "Entity1", "Entity2" }, viewModel.Nodes.Select(x => x.Name));
		}

		public void TestConnections_WhenNotNonScheduled_ShouldExcludeNonScheduledConnections()
		{
			AssertConnectionsForRegularOrNonScheduledViewModel("Only relationships between regular entities should be shown. SAD!", false, "Entity1");
		}

		public void TestConnections_WhenNonScheduled_ShouldOnlyIncludeNonScheduledConnections()
		{
			AssertConnectionsForRegularOrNonScheduledViewModel("Only relationships between nonscheduled entities should be shown. SAD!", true, "Entity3");
		}

		void AssertConnectionsForRegularOrNonScheduledViewModel(string message, bool isNonScheduled, params string[] expectedFromNodes)
		{
			var entity1 = new Entity { IsNonScheduled = false, Name = "Entity1" };
			var entity2 = new Entity { IsNonScheduled = false, Name = "Entity2" };
			var entity3 = new Entity { IsNonScheduled = true, Name = "Entity3" };
			var entity4 = new Entity { IsNonScheduled = true, Name = "Entity4" };
			network.Entities.Add(entity1);
			network.Entities.Add(entity2);
			network.Entities.Add(entity3);
			network.Entities.Add(entity4);
			network.CreateRelationship(entity1, entity2);
			network.CreateRelationship(entity3, entity4);

			var viewModel = new NetworkViewModel(network);

			viewModel.BuildNetwork(isReloading: false);
			var connections = isNonScheduled ? viewModel.NonScheduledConnections : viewModel.ScheduledConnections;

			AssertContainsExactElementsInAnyOrder(message, expectedFromNodes, connections.Select(x => x.SourceConnector.ParentNode.Name));
		}

		public void TestConnections_ShouldCombineScheduledAndNonScheduledConnections()
		{
			var entity1 = new Entity { IsNonScheduled = false, Name = "Entity1" };
			var entity2 = new Entity { IsNonScheduled = false, Name = "Entity2" };
			var entity3 = new Entity { IsNonScheduled = true, Name = "Entity3" };
			var entity4 = new Entity { IsNonScheduled = true, Name = "Entity4" };
			network.Entities.Add(entity1);
			network.Entities.Add(entity2);
			network.Entities.Add(entity3);
			network.Entities.Add(entity4);
			network.CreateRelationship(entity1, entity2);
			network.CreateRelationship(entity3, entity4);

			var viewModel = new NetworkViewModel(network);

			viewModel.BuildNetwork(isReloading: false);

			AssertContainsExactElementsInAnyOrder("Both scheduled and non-scheduled connections should be included in the Connections property. SAD!", new[] { "Entity1", "Entity3" }, viewModel.Connections.Select(x => x.SourceConnector.ParentNode.Name));
		}

		#endregion

		#region Implementation

		DummyNetwork network;
		Entity diagramEntity;
		NetworkViewModel networkViewModel;
		NodeViewModel node1;
		NodeViewModel node2;

		protected override void SetUp()
		{
			base.SetUp();

			var refresher = new NetworkRefresher();
			network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			diagramEntity = new Entity();
			network.DiagramEntity = diagramEntity;
			networkViewModel = new NetworkViewModel(network, null);

			node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1");
			node2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity2");
		}

		static void AssertSelectionAndNotification(NetworkViewModel networkViewModel, INetworkEntity[] expectedSelection, RefreshArgs actualEventArgs)
		{
			AssertContainsExactElementsInAnyOrder(expectedSelection, networkViewModel.SelectedEntities);
			AssertNotNull(actualEventArgs);
			AssertContainsExactElementsInAnyOrder(expectedSelection, actualEventArgs.Entities);
		}

		#endregion
	}
}
