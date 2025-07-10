using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	class DiagramAreaUserControlViewModelTest : TestCase
	{
		#region Connection Dragging

		public void TestConnectionDragCompleted_WhenNetworkReturnsNull()
		{
			var network = new Mock<INetwork>();

			var diagramEntity = new Entity();
			var entity1 = new Entity();
			var entity2 = new Entity();

			network.Setup(m => m.Entities).Returns(new ImpObservableCollection<INetworkEntity> { entity1, entity2 });
			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());

			network.Setup(m => m.CreateRelationship(entity1, entity2)).Returns((IEntityRelationship)null);

			var networkControlViewModel = new NetworkUserControlViewModel(network.Object);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			var sourceConnector = new ConnectorViewModel("") { ParentNode = new NodeViewModel(entity1, viewModel.NetworkViewModel), Type = ConnectorType.Output };
			var destConnector = new ConnectorViewModel("") { ParentNode = new NodeViewModel(entity2, viewModel.NetworkViewModel), Type = ConnectorType.Input };

			viewModel.ConnectionDragStarted(sourceConnector, new Point());
			AssertEquals(1, viewModel.NetworkViewModel.Connections.Count());

			var connection = viewModel.NetworkViewModel.Connections.First();
			viewModel.ConnectionDragCompleted(connection, connection.SourceConnector, destConnector);
			AssertEquals(0, viewModel.NetworkViewModel.Connections.Count());
		}

		public void TestConnectionDragCompleted_WhenNetworkReturnsValidRelationship()
		{
			var network = new Mock<INetwork>();

			var diagramEntity = new Entity();
			var entity1 = new Entity();
			var entity2 = new Entity();
			var relationship = new Relationship { From = entity1, To = entity2 };

			network.Setup(m => m.Entities).Returns(new ImpObservableCollection<INetworkEntity> { entity1, entity2 });
			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());

			network.Setup(m => m.CreateRelationship(entity1, entity2)).Returns(relationship);

			var networkControlViewModel = new NetworkUserControlViewModel(network.Object);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			var sourceConnector = new ConnectorViewModel("") { ParentNode = new NodeViewModel(entity1, viewModel.NetworkViewModel), Type = ConnectorType.Output };
			var destConnector = new ConnectorViewModel("") { ParentNode = new NodeViewModel(entity2, viewModel.NetworkViewModel), Type = ConnectorType.Input };

			viewModel.ConnectionDragStarted(sourceConnector, new Point());
			AssertEquals(1, viewModel.NetworkViewModel.Connections.Count());

			var connection = viewModel.NetworkViewModel.Connections.First();
			viewModel.ConnectionDragCompleted(connection, connection.SourceConnector, destConnector);
			AssertEquals(1, viewModel.NetworkViewModel.Connections.Count());
		}

		public void TestConnection_CreatingAndDeletingConnectionBeforeSaving()
		{
			var network = new Mock<INetwork>();

			var diagramEntity = new Entity();
			var entity1 = new Entity();
			var entity2 = new Entity();
			var relationship = new Relationship { From = entity1, To = entity2 };

			network.Setup(m => m.Entities).Returns(new ImpObservableCollection<INetworkEntity> { entity1, entity2 });
			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());

			network.Setup(m => m.CreateRelationship(entity1, entity2)).Returns(relationship);
			network.Setup(m => m.DeleteRelationship(relationship)).Returns(true);

			var networkControlViewModel = new NetworkUserControlViewModel(network.Object);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			var sourceConnector = new ConnectorViewModel("") { ParentNode = new NodeViewModel(entity1, viewModel.NetworkViewModel), Type = ConnectorType.Output };
			var destConnector = new ConnectorViewModel("") { ParentNode = new NodeViewModel(entity2, viewModel.NetworkViewModel), Type = ConnectorType.Input };

			viewModel.ConnectionDragStarted(sourceConnector, new Point());
			AssertEquals(1, viewModel.NetworkViewModel.Connections.Count());

			var connection = viewModel.NetworkViewModel.Connections.First();
			viewModel.ConnectionDragCompleted(connection, connection.SourceConnector, destConnector);
			AssertEquals(1, viewModel.NetworkViewModel.Connections.Count());

			viewModel.DeleteConnection(connection);

			AssertEquals("Creating relationship then deleting it then saving it should not show it.", 0, viewModel.NetworkViewModel.Connections.Count());
			}

		public void TestHideConnection_WhenNetworkModelReturnsFalse_ShouldNotHide()
		{
			var network = new Mock<INetwork>();

			var diagramEntity = new Entity();
			var entity1 = new Entity();
			var entity2 = new Entity();

			var relationship = new Relationship { From = entity1, To = entity2 };
			entity1.Links.Add(relationship);
			entity2.Links.Add(relationship);

			network.Setup(m => m.Entities).Returns(new ImpObservableCollection<INetworkEntity> { entity1, entity2 });
			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());

			network.Setup(m => m.HideRelationship(relationship)).Returns(false);

			var networkControlViewModel = new NetworkUserControlViewModel(network.Object);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			AssertEquals(1, viewModel.NetworkViewModel.Connections.Count());

			viewModel.HideConnection(viewModel.NetworkViewModel.Connections.First());
			AssertEquals(1, viewModel.NetworkViewModel.Connections.Count());
		}

		#endregion

		#region Create Nodes

		public void TestFixedEntity_FromExistingEntity_ShouldSetLayoutValues()
		{
			var entity = new Entity { X = 10.0, Y = 100.0, Width = 20.0, Height = 200.0 };
			entity.EntityState = EntityState.Fixed; // Locking coordinates in place. 
			var network = new DummyNetwork { DiagramEntity = new Entity() };
			network.Entities.Add(entity);

			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			var newViewModel = viewModel.CreateNode(entity, new Location(entity.X, entity.Y), false);

			AssertEquals(10.0, newViewModel.X);
			AssertEquals(100.0, newViewModel.Y);
			AssertEquals(20.0, newViewModel.Width);
			AssertEquals(200.0, newViewModel.Height);
		}

		public void TestCreateNewNode_WhenNetworkReturnsNull()
		{
			var diagramEntity = new Entity();
			var network = new Mock<INetwork>();

			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.Entities).Returns(new ImpObservableCollection<INetworkEntity>());
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());

			var networkControlViewModel = new NetworkUserControlViewModel(network.Object);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			viewModel.CreateNewNode(new Location(), null);
			AssertEquals(0, viewModel.NetworkViewModel.Nodes.Count());
		}

		public void TestAdjustLocation_CalledForNewEntities()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);

			var entity = new Entity { Parent = diagramEntity, Width = 101, Height = 101 };
			network.Entities.Add(entity);

			var entityViewModel = viewModel.CreateNewNode(new Location(101d, 80d), entity);

			AssertEquals("Should snap to scaled width", 100d, entityViewModel.X);
			AssertEquals("No centering has occured.", 80d, entityViewModel.Y);
			AssertEquals("Should snap to new position.", 100d, entityViewModel.Width);
			AssertEquals("No adjustment occurs.", 101d, entityViewModel.Height);
		}

		public void TestAdjustLocationFixedDiagram_CalledForNewEntities()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				IsDiagramSurfaceFixed = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);

			var entity = new Entity { Parent = diagramEntity, Width = 101, Height = 101 };
			network.Entities.Add(entity);

			var entityViewModel = viewModel.CreateNewNode(new Location(101d, 80d), entity);

			AssertEquals("Should snap to scaled width", 100d, entityViewModel.X);
			AssertEquals("No centering has occured.", 80d, entityViewModel.Y);
			AssertEquals("Should snap to new position.", 100d, entityViewModel.Width);
			AssertEquals("No adjustment occurs.", 101d, entityViewModel.Height);
		}

		#endregion

		#region Show/Hide Entity

		public void TestShowEntity_ShouldImportAllEntitiesShown()
		{
			var diagramEntity = new Entity();
			var newEntity1 = new Entity();
			var newEntity2 = new Entity();
			var network = new Mock<INetwork>();

			network.Setup(m => m.Entities).Returns(new ImpObservableCollection<INetworkEntity>());
			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());
			network.Setup(m => m.Refresher).Returns(new NetworkRefresher());

			network.Setup(m => m.ShowEntity(newEntity1, diagramEntity)).Returns(new[] { newEntity1, newEntity2 });

			var networkControlViewModel = new NetworkUserControlViewModel(network.Object);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			AssertEquals(0, viewModel.NetworkViewModel.Nodes.Count());

			viewModel.ShowEntity(newEntity1, diagramEntity, new Point());

			AssertEquals(2, viewModel.NetworkViewModel.Nodes.Count());
			AssertCollectionContains(viewModel.NetworkViewModel.Nodes, n => n.Entity == newEntity1);
			AssertCollectionContains(viewModel.NetworkViewModel.Nodes, n => n.Entity == newEntity2);
		}

		public void TestShowEntity_SubDiagramAndSubEntities_ShouldPlaceOverSubDiagramInZIndex()
		{
			var diagramEntity = new Entity();
			var subDiagramEntity = new Entity { Parent = diagramEntity };
			var subDiagramSubEntity1 = new Entity { Parent = subDiagramEntity };
			var subDiagramSubEntity2 = new Entity { Parent = subDiagramEntity };

			var network = new Mock<INetwork>();

			network.Setup(m => m.Entities).Returns(new ImpObservableCollection<INetworkEntity>());
			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());
			network.Setup(m => m.Refresher).Returns(new NetworkRefresher());

			network.Setup(m => m.ShowEntity(subDiagramEntity, diagramEntity)).Returns(new[] { subDiagramEntity, subDiagramSubEntity1, subDiagramSubEntity2 });

			var networkControlViewModel = new NetworkUserControlViewModel(network.Object);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			AssertEquals(0, viewModel.NetworkViewModel.Nodes.Count());

			viewModel.ShowEntity(subDiagramEntity, diagramEntity, new Point(10, 10));
			AssertEquals(3, viewModel.NetworkViewModel.Nodes.Count());

			var subDiagramNode = viewModel.NetworkViewModel.Nodes.First(n => n.Entity == subDiagramEntity);
			var subEntity1Node = viewModel.NetworkViewModel.Nodes.First(n => n.Entity == subDiagramSubEntity1);
			var subEntity2Node = viewModel.NetworkViewModel.Nodes.First(n => n.Entity == subDiagramSubEntity2);

			AssertEquals(0, subDiagramNode.ZIndex);
			AssertEquals(1, subEntity1Node.ZIndex);
			AssertEquals(1, subEntity2Node.ZIndex);
		}

		public void TestShowEntity_SubDiagramSubEntityIndividually_ShouldPlaceOverSubDiagramInZIndex()
		{
			var diagramEntity = new Entity();
			var subDiagramEntity = new Entity { Parent = diagramEntity };
			var subDiagramSubEntity1 = new Entity { Parent = subDiagramEntity, ZIndex = 1 };
			var subDiagramSubEntity2 = new Entity { Parent = subDiagramEntity };

			var network = new Mock<INetwork>();

			network.Setup(m => m.Entities).Returns(new ImpObservableCollection<INetworkEntity> { subDiagramEntity, subDiagramSubEntity1 });
			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());
			network.Setup(m => m.Refresher).Returns(new NetworkRefresher());

			network.Setup(m => m.ShowEntity(subDiagramSubEntity2, subDiagramEntity)).Returns(new[] { subDiagramSubEntity2 });

			var networkControlViewModel = new NetworkUserControlViewModel(network.Object);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			AssertEquals(2, viewModel.NetworkViewModel.Nodes.Count());

			viewModel.ShowEntity(subDiagramSubEntity2, subDiagramEntity, new Point(10, 10));
			AssertEquals(3, viewModel.NetworkViewModel.Nodes.Count());

			var subDiagramNode = viewModel.NetworkViewModel.Nodes.First(n => n.Entity == subDiagramEntity);
			var subEntity1Node = viewModel.NetworkViewModel.Nodes.First(n => n.Entity == subDiagramSubEntity1);
			var subEntity2Node = viewModel.NetworkViewModel.Nodes.First(n => n.Entity == subDiagramSubEntity2);

			AssertEquals(0, subDiagramNode.ZIndex);
			AssertEquals(1, subEntity1Node.ZIndex);
			AssertEquals(1, subEntity2Node.ZIndex);
		}

		public void TestHideEntity()
		{
			var network = new Mock<INetwork>();
			var entity1 = new Entity { X = 1 };
			var entity2 = new Entity { X = 1 };
			var entity3 = new Entity { X = 1 };

			network.Setup(m => m.Entities).Returns(new ImpObservableCollection<INetworkEntity>(new[] { entity1, entity2, entity3 }));

			network.Setup(m => m.HideEntity(entity1)).Returns(new[] { entity1, entity2 });
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());

			var networkControlViewModel = new NetworkUserControlViewModel(network.Object);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			AssertEquals(3, viewModel.NetworkViewModel.Nodes.Count());

			var node = viewModel.NetworkViewModel.Nodes.First();
			viewModel.HideNode(node);
			AssertEquals(1, viewModel.NetworkViewModel.Nodes.Count());
			AssertEquals(entity3, viewModel.NetworkViewModel.Nodes.First().Entity);
		}

		public void TestRemoveFromDiagram()
		{
			var diagramEntity = new Entity();
			var entity1 = new Entity();
			var entity2 = new Entity();

			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity1);
			network.Entities.Add(entity2);

			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			var networkViewModel = networkControlViewModel.NetworkViewModel;

			var node1 = networkViewModel.ScheduledNodes[0];
			var node2 = networkViewModel.ScheduledNodes[1];

			AssertEquals("Initially must not have connection i.e. 0 connection", 0, networkViewModel.Connections.Count());

			var sourceConnector = new ConnectorViewModel("") { ParentNode = node1, Type = ConnectorType.Output };
			node1.OutputConnectors.Add(sourceConnector);
			viewModel.ConnectionDragStarted(sourceConnector, new Point());
			var destConnector = new ConnectorViewModel("") { ParentNode = node2, Type = ConnectorType.Input };
			node2.InputConnectors.Add(destConnector);
			var connection = networkViewModel.Connections.First();
			viewModel.ConnectionDragCompleted(connection, connection.SourceConnector, destConnector);
			AssertEquals("1 Connection must be created", 1, networkViewModel.Connections.Count());

			network.Entities.Remove(entity2);

			networkViewModel.Refresh();

			AssertEquals("Connections must be removed when shape is removed", 0, networkViewModel.Connections.Count());
		}

		#endregion

		#region Import Entity

		public void TestImportEntity()
		{
			var diagramEntity = new Entity();
			var newEntity1 = new Entity();
			var newEntity2 = new Entity();
			var network = new Mock<INetwork>();

			network.Setup(m => m.Entities).Returns(new ImpObservableCollection<INetworkEntity>());
			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());
			network.Setup(m => m.Refresher).Returns(new NetworkRefresher());

			network.Setup(m => m.PickAndImportEntities(diagramEntity)).Returns(new[] { newEntity1, newEntity2 });

			var networkControlViewModel = new NetworkUserControlViewModel(network.Object);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			AssertEquals(0, viewModel.NetworkViewModel.Nodes.Count());

			viewModel.ImportEntity(new Point(), diagramEntity);

			AssertEquals(2, viewModel.NetworkViewModel.Nodes.Count());
			AssertCollectionContains(viewModel.NetworkViewModel.Nodes, n => n.Entity == newEntity1);
			AssertCollectionContains(viewModel.NetworkViewModel.Nodes, n => n.Entity == newEntity2);
		}

		#endregion

		#region Project Buffer

		public void TestConnectionDragStarted_ForBuffer_ShouldDisallowNewLinks()
		{
			var entity1 = new Mock<INetworkEntity>();
			var entity2 = new Mock<INetworkEntity>();

			entity1.Setup(m => m.Children).Returns(Enumerable.Empty<INetworkEntity>());
			entity2.Setup(m => m.Children).Returns(Enumerable.Empty<INetworkEntity>());
			entity1.Setup(m => m.PreRequisiteLinks).Returns(Enumerable.Empty<IEntityRelationship>());
			entity2.Setup(m => m.PreRequisiteLinks).Returns(Enumerable.Empty<IEntityRelationship>());
			entity1.Setup(m => m.Links).Returns(new List<IEntityRelationship>());
			entity2.Setup(m => m.Links).Returns(new List<IEntityRelationship>());
			entity1.Setup(m => m.X).Returns(1);
			entity2.Setup(m => m.X).Returns(1);

			entity1.Setup(e => e.CanCreateRelationship(null)).Returns(true);
			entity2.Setup(e => e.CanCreateRelationship(null)).Returns(false);

			var network = new DummyNetwork();
			network.Entities.Add(entity1.Object);
			network.Entities.Add(entity2.Object);

			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			var entity1Connector = new ConnectorViewModel(string.Empty) { ParentNode = new NodeViewModelProvider().Create(entity1.Object, viewModel.NetworkViewModel) };
			var entity2Connector = new ConnectorViewModel(string.Empty) { ParentNode = new NodeViewModelProvider().Create(entity2.Object, viewModel.NetworkViewModel) };

			AssertNotNull(viewModel.ConnectionDragStarted(entity1Connector, new Point()));
			AssertNull(viewModel.ConnectionDragStarted(entity2Connector, new Point()));
		}

		public void TestQueryConnnectionFeedback_ForBuffer_ShouldShowNegativeFeedbackOnBufferRelationship()
		{
			var entity1 = new Mock<INetworkEntity>();
			var entity2 = new Mock<INetworkEntity>();
			var entity3 = new Mock<INetworkEntity>();

			entity1.Setup(m => m.PreRequisiteLinks).Returns(Enumerable.Empty<IEntityRelationship>());
			entity2.Setup(m => m.PreRequisiteLinks).Returns(Enumerable.Empty<IEntityRelationship>());
			entity3.Setup(m => m.PreRequisiteLinks).Returns(Enumerable.Empty<IEntityRelationship>());
			entity1.Setup(m => m.Children).Returns(Enumerable.Empty<INetworkEntity>());
			entity2.Setup(m => m.Children).Returns(Enumerable.Empty<INetworkEntity>());
			entity3.Setup(m => m.Children).Returns(Enumerable.Empty<INetworkEntity>());
			entity1.Setup(m => m.Links).Returns(new List<IEntityRelationship>());
			entity2.Setup(m => m.Links).Returns(new List<IEntityRelationship>());
			entity3.Setup(m => m.Links).Returns(new List<IEntityRelationship>());
			entity1.Setup(m => m.X).Returns(1);
			entity2.Setup(m => m.X).Returns(1);
			entity3.Setup(m => m.X).Returns(1);

			entity1.Setup(e => e.CanCreateRelationship(entity2.Object)).Returns(true);
			entity1.Setup(e => e.CanCreateRelationship(entity3.Object)).Returns(false);

			var network = new DummyNetwork();
			network.Entities.Add(entity3.Object);
			network.Entities.Add(entity1.Object);
			network.Entities.Add(entity2.Object);

			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			var entity1Connector = new ConnectorViewModel(string.Empty) { ParentNode = new NodeViewModelProvider().Create(entity1.Object, viewModel.NetworkViewModel), Type = ConnectorType.Input };
			var entity2Connector = new ConnectorViewModel(string.Empty) { ParentNode = new NodeViewModelProvider().Create(entity2.Object, viewModel.NetworkViewModel), Type = ConnectorType.Output };
			var entity3Connector = new ConnectorViewModel(string.Empty) { ParentNode = new NodeViewModelProvider().Create(entity3.Object, viewModel.NetworkViewModel), Type = ConnectorType.Output };

			object feedbackIndicator;
			bool connectionOk;

			viewModel.QueryConnnectionFeedback(entity1Connector, entity2Connector, out feedbackIndicator, out connectionOk);
			AssertEquals(true, connectionOk);
			AssertType<ConnectionOkIndicator>("Should be able to create a link between non-buffer shapes", feedbackIndicator);

			viewModel.QueryConnnectionFeedback(entity1Connector, entity3Connector, out feedbackIndicator, out connectionOk);
			AssertEquals(false, connectionOk);
			AssertType<ConnectionBadIndicator>("Should warn of not being able to create a link to a buffer shape", feedbackIndicator);
		}

		public void TestConnectionCreation_InvolvingProjectBuffer_ShouldResultInNoConnection()
		{
			var network = new Mock<INetwork>();

			var diagramEntity = new Entity();
			var entity1 = new Mock<INetworkEntity>();
			var entity2 = new Mock<INetworkEntity>();

			// populate mocked network
			network.Setup(m => m.Entities).Returns(new ImpObservableCollection<INetworkEntity> { entity1.Object, entity2.Object });
			network.Setup(m => m.DiagramEntity).Returns(diagramEntity);
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());

			// populate mocked entities
			entity1.Setup(m => m.Children).Returns(Enumerable.Empty<INetworkEntity>());
			entity2.Setup(m => m.Children).Returns(Enumerable.Empty<INetworkEntity>());
			entity1.Setup(m => m.PreRequisiteLinks).Returns(Enumerable.Empty<IEntityRelationship>());
			entity2.Setup(m => m.PreRequisiteLinks).Returns(Enumerable.Empty<IEntityRelationship>());
			entity1.Setup(m => m.Links).Returns(new List<IEntityRelationship>());
			entity2.Setup(m => m.Links).Returns(new List<IEntityRelationship>());
			entity1.Setup(m => m.X).Returns(1);
			entity2.Setup(m => m.X).Returns(1);

			// setup enforced results for two important functions we'll use later!
			entity1.Setup(e => e.CanCreateRelationship(null)).Returns(true);
			entity1.Setup(e => e.CanCreateRelationship(entity2.Object)).Returns(true);

			var networkControlViewModel = new NetworkUserControlViewModel(network.Object);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			var sourceConnector = new ConnectorViewModel("") { ParentNode = new NodeViewModel(entity1.Object, viewModel.NetworkViewModel), Type = ConnectorType.Output };
			var destConnector = new ConnectorViewModel("") { ParentNode = new NodeViewModel(entity2.Object, viewModel.NetworkViewModel), Type = ConnectorType.Input };

			viewModel.ConnectionDragStarted(sourceConnector, new Point());
			AssertEquals(1, viewModel.NetworkViewModel.Connections.Count());

			var connection = viewModel.NetworkViewModel.Connections.First();
			viewModel.ConnectionDragCompleted(connection, connection.SourceConnector, destConnector);
			AssertEquals(0, viewModel.NetworkViewModel.Connections.Count());
		}

		#endregion

		#region Width/Height

		public void TestContentWidthAndHeight_AtBeginingShouldNotHaveValue()
		{
			var viewModel = new DiagramAreaUserControlViewModel();
			AssertEquals(viewModel.ContentWidth, 0D);
			AssertEquals(viewModel.ContentHeight, 0D);
		}

		#endregion
	}
}
