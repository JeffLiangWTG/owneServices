using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	/// <summary>
	/// Defines a network of nodes and connections between the nodes.
	/// </summary>
	public sealed class NetworkViewModel : ViewModelBase, INetworkViewModel
	{
		public NetworkViewModel(INetwork network, NodeViewModelProvider nodeViewModelProvider = null)
		{
			Network = network ?? throw new ArgumentNullException(nameof(network));
			NodeViewModelProvider = nodeViewModelProvider ?? new NodeViewModelProvider();

			if (Network?.DiagramEntity != null)
			{
				DiagramNodeViewModel = NodeViewModelProvider.Create(Network.DiagramEntity, this);
			}

			PopulateChannelYAxisRanges();
			SubscribeSelectionChangedEventToNetwork();
		}

		public void Deactivate()
		{
			UnsubscribeSelectionChangedEventFromNetwork();
		}

		public INetwork Network { get; private set; }

		public INetworkEntityController Controller => Network?.Controller;
		public INetworkUserInteractionImplementor UserInteractionImplementor => Controller?.UserInteractionImplementor;

		readonly ProposedNetworkEntityEqualityComparer entityEqualityComparer = new ProposedNetworkEntityEqualityComparer();

		#region Nodes

		public NodeViewModelProvider NodeViewModelProvider { get; private set; }

		public NodeViewModel DiagramNodeViewModel { get; private set; }

		/// <summary>
		/// All nodes in the network, including scheduled and non-scheduled nodes.
		/// </summary>
		public IEnumerable<NodeViewModel> Nodes => NonScheduledNodes == null ? ScheduledNodes : ScheduledNodes.Concat(NonScheduledNodes);

		public NodeViewModelCollection ScheduledNodes
		{
			get
			{
				if (scheduledNodes == null)
				{
					scheduledNodes = new NodeViewModelCollection();
					scheduledNodes.ItemsRemoved += Nodes_ItemsRemoved;
				}

				return scheduledNodes;
			}
		}

		NodeViewModelCollection scheduledNodes;

		public NodeViewModelCollection NonScheduledNodes
		{
			get
			{
				if (nonScheduledNodes == null)
				{
					nonScheduledNodes = new NodeViewModelCollection();
					nonScheduledNodes.ItemsRemoved += Nodes_ItemsRemoved;
				}

				return nonScheduledNodes;
			}
		}

		NodeViewModelCollection nonScheduledNodes;

		public NodeViewModelCollection GetAppropriateNodeCollection(bool isNonScheduled)
		{
			return isNonScheduled ? NonScheduledNodes : ScheduledNodes;
		}

		public NodeViewModel GetNodeForEntity(INetworkEntity entity)
		{
			if (entity == null)
			{
				return null;
			}

			if (IsDiagramEntity(entity))
			{
				return DiagramNodeViewModel;
			}

			return Nodes.SingleOrDefault(n => entityEqualityComparer.Equals(entity, n.Entity));
		}

		bool IsDiagramEntity(INetworkEntity entity)
		{
			return entityEqualityComparer.Equals(entity, Network.DiagramEntity);
		}

		void Nodes_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
		{
			if (e.Items.Cast<NodeViewModel>().Any(x => x.IsSelected))
			{
				OnSelectionChanged();
			}
		}

		#endregion

		#region Connections

		public IEnumerable<ConnectionViewModel> Connections => NonScheduledConnections == null ? ScheduledConnections : ScheduledConnections.Concat(NonScheduledConnections);

		public ImpObservableCollection<ConnectionViewModel> ScheduledConnections
		{
			get
			{
				if (scheduledConnections == null)
				{
					scheduledConnections = new ImpObservableCollection<ConnectionViewModel>();
					scheduledConnections.ItemsRemoved += new EventHandler<CollectionItemsChangedEventArgs>(Connections_ItemsRemoved);
				}

				return scheduledConnections;
			}
		}

		ImpObservableCollection<ConnectionViewModel> scheduledConnections;

		public ImpObservableCollection<ConnectionViewModel> NonScheduledConnections
		{
			get
			{
				if (nonScheduledConnections == null)
				{
					nonScheduledConnections = new ImpObservableCollection<ConnectionViewModel>();
					nonScheduledConnections.ItemsRemoved += new EventHandler<CollectionItemsChangedEventArgs>(Connections_ItemsRemoved);
				}

				return nonScheduledConnections;
			}
		}

		ImpObservableCollection<ConnectionViewModel> nonScheduledConnections;

		ImpObservableCollection<ConnectionViewModel> GetAppropriateConnectionCollection(bool isNonScheduled)
		{
			return isNonScheduled ? NonScheduledConnections : ScheduledConnections;
		}

		public void AddConnection(ConnectionViewModel connection)
		{
			var isNonScheduled = IsConnectionNonScheduled(connection);
			GetAppropriateConnectionCollection(isNonScheduled).Add(connection);
		}

		public void RemoveConnection(ConnectionViewModel connection)
		{
			var isNonScheduled = IsConnectionNonScheduled(connection);
			GetAppropriateConnectionCollection(isNonScheduled).Remove(connection);
		}

		static bool IsConnectionNonScheduled(ConnectionViewModel connection)
		{
			return connection?.SourceConnector?.ParentNode?.Entity?.IsNonScheduled ?? false;
		}

		#endregion

		#region Selection, Right Clicked Node, Active Node

		#region Selection

		internal void OnSelectionChanged(RefreshType refreshType = RefreshType.None)
		{
			SelectionChanged?.Invoke(this, new RefreshArgs(refreshType, SelectedEntities.ToArray()));
		}

		public IEnumerable<NodeViewModel> SelectedNodes => Nodes.Where(n => n.IsSelected && !n.Entity.IsDeleted).ToArray();

		public NodeViewModel FirstSelectedNode => SelectedNodes.FirstOrDefault();

		public IEnumerable<INetworkEntity> SelectedEntities => SelectedNodes.Select(n => n.Entity).ToArray();

		public INetworkEntity FirstSelectedEntity => FirstSelectedNode?.Entity;

		public void SelectSingleEntity(INetworkEntity entity, bool shouldFocusOnSelection = false)
		{
			SelectEntities(entity != null ? new INetworkEntity[] { entity } : Array.Empty<INetworkEntity>());

			if (shouldFocusOnSelection)
			{
				FocusedEntityChanged?.Invoke(this, new FocusedEntityChangedEventArgs(entity));
			}
		}

		public void SelectEntities(IEnumerable<INetworkEntity> entitiesToSelect)
		{
			var entities = entitiesToSelect.ToHashSet();

			foreach (var node in Nodes)
			{
				node.IsSelected = entities.Contains(node.Entity, entityEqualityComparer);
			}
		}

		#endregion

		#region Right Clicked Node

		//points to the right clicked node when context menu appears
		public NodeViewModel RightClickedNode
		{
			get => rightClickedNode != null
				&& Nodes.Union(new NodeViewModel[] { DiagramNodeViewModel }).Contains(rightClickedNode)
				&& !rightClickedNode.Entity.IsDeleted
					? rightClickedNode
					: null;
			set
			{
				rightClickedNode = value;
				OnSelectionChanged();
			}
		}
		NodeViewModel rightClickedNode;

		#endregion

		#region Active Node

		//points to the main node for which the ribbon is constructed
		public NodeViewModel ActiveNode => RightClickedNode ?? SelectedNodes.FirstOrDefault() ?? DiagramNodeViewModel;

		public INetworkEntity ActiveEntity => ActiveNode?.Entity;

		#endregion

		#endregion

		#region Events

		public event EventHandler<RefreshArgs> SelectionChanged;

		public event EventHandler<FocusedEntityChangedEventArgs> FocusedEntityChanged;

		#endregion

		#region Build Network

		public void BuildNetwork(bool isReloading)
		{
			BuildNetwork(Network.Entities.ToArray(), isReloading);
		}

		void BuildNetwork(INetworkEntity[] entities, bool isReloading)
		{
			using (SuspendBoundsChecking())
			{
				var nodesList = new List<Tuple<INetworkEntity, NodeViewModel>>();

				SetPositionsForNewEntities(entities);

				foreach (var entity in entities.OrderBy(EntityPositionStrategy.CalculateDepth))
				{
					var isSuspended = entity.IsCalculationSuspended;
					try
					{
						entity.IsCalculationSuspended = isReloading;
						var node = SetDefaultsForNewNode(entity, null, false);
						nodesList.Add(Tuple.Create(entity, node));

						if (entity.Parent is INetworkEntity parentEntity && !IsDiagramEntity(parentEntity))
						{
							var parentNode = GetNodeForEntity(parentEntity);

							if (parentNode != null)
							{
								using (entity.SuspendSettingHasChanges())
								{
									node.ZIndex = parentNode.ZIndex + 1;
								}
							}
						}
					}
					finally
					{
						entity.IsCalculationSuspended = isSuspended;
					}
				}

				AddRelationships(nodesList.Select(t => t.Item1));
			}
		}

		void SetPositionsForNewEntities(INetworkEntity[] entities)
		{
			var newEntities = entities.Where(entity => !entity.IsPositioned && entity.Pin == null).ToArray();

			if (!newEntities.Any())
			{
				return;
			}

			var newEntityGroups = newEntities.GroupBy(x => x.IsNonScheduled);
			var positioner = Network.EntityPositionStrategy;

			foreach (var group in newEntityGroups)
			{
				var areNewEntitiesNonScheduled = group.First().IsNonScheduled;
				var existingNodes = GetAppropriateNodeCollection(areNewEntitiesNonScheduled);
				positioner.SetPositionsForNewEntities(group, entities.Concat(existingNodes.Select(n => n.Entity)).Distinct(), new Location(0, 0));
			}
		}

		public void AddRelationships(IEnumerable<INetworkEntity> entities)
		{
			var uniqueRelationships = entities.SelectMany(t => t.Links).Distinct(new UniqueRelationshipComparer());

			foreach (var relationship in uniqueRelationships)
			{
				var fromNode = GetNodeForEntity((INetworkEntity)relationship.From);
				var toNode = GetNodeForEntity((INetworkEntity)relationship.To);

				if (fromNode != null && toNode != null)
				{
					Connect(fromNode, toNode, relationship);
				}
			}
		}

		void Connect(NodeViewModel node1, NodeViewModel node2, IEntityRelationship relationship)
		{
			var connection = new ConnectionViewModel
			{
				SourceConnector = node1.OutputConnectors[0],
				DestConnector = node2.InputConnectors[0],
				Relationship = relationship,
			};
			GetAppropriateConnectionCollection(node1.Entity.IsNonScheduled).Add(connection);
		}

		#endregion

		#region Refresh Network

		public void Refresh()
		{
			var entities = new HashSet<INetworkEntity>(Network.Entities);

			var removedNodes = Nodes.Where(n => !entities.Contains(n.Entity)).ToArray();
			foreach (var removedNode in removedNodes)
			{
				RemoveNodeCore(removedNode);
			}

			var entitiesNeedingNodes = Network.Entities.Where(entity => GetNodeForEntity(entity) == null).ToArray();
			BuildNetwork(entitiesNeedingNodes, isReloading: false);
		}

		#endregion

		#region Create Node

		public IEnumerable<NodeViewModel> SetDefaultsForNewNodes(IEnumerable<INetworkEntity> entities, Location nodeLocation)
		{
			var modifiedEntities = Network.EntityPositionStrategy.SetPositionsForNewEntities(entities, Network.Entities, nodeLocation);

			bool nodeInitialisedForFirstTime;
			foreach (var node in modifiedEntities.OrderBy(EntityPositionStrategy.CalculateDepth).Select(entity => GetOrCreateNode(entity, out nodeInitialisedForFirstTime)))
			{
				var parent = node.GetParent();

				if (parent != null && parent.Entity != Network.DiagramEntity)
				{
					node.TryShrinkToFit(parent);
					node.SetX(node.X);
					node.SetY(node.Y);
				}
			}

			return entities.Select(e => GetOrCreateNode(e, out nodeInitialisedForFirstTime));
		}

		public NodeViewModel SetDefaultsForNewNode(INetworkEntity entity, Location? nodeLocation, bool centerNode)
		{
			bool nodeAdded;
			var node = GetOrCreateNode(entity, out nodeAdded);

			if (nodeLocation.HasValue)
			{
				node.SetX(nodeLocation.Value.X);
				node.SetY(nodeLocation.Value.Y);
			}

			var startingWidth = entity.Width != 0d ? entity.Width : Network.EntityPositionStrategy.DefaultWidth;
			var startingHeight = entity.Height != 0d ? entity.Height : Network.EntityPositionStrategy.DefaultHeight;

			if (centerNode)
			{
				// 
				// We want to center the node.
				//
				// For this to happen we need to wait until the UI has determined the 
				// size based on the node's data-template.
				//
				// So we define an anonymous method to handle the SizeChanged event for a node.
				//
				// Note: If you don't declare sizeChangedEventHandler before initializing it you will get
				//       an error when you try and unsubscribe the event from within the event handler.
				//
				EventHandler<EventArgs> sizeChangedEventHandler = null;
				sizeChangedEventHandler = (sender, e) =>
				{
					//
					// This event handler will be called after the size of the node has been determined.
					// So we can now use the size of the node to modify its position.
					//
					node.SetX(node.X + startingWidth / 2 - node.Width / 2);
					node.SetY(node.Y + startingHeight / 2 - node.Height / 2);

					//
					// Don't forget to unhook the event, after the initial centering of the node
					// we don't need to be notified again of any size changes.
					//
					node.SizeChanged -= sizeChangedEventHandler;
				};

				//
				// Now we hook the SizeChanged event so the anonymous method is called later
				// when the size of the node has actually been determined.
				//
				node.SizeChanged += sizeChangedEventHandler;
				//
				// Just in the network is rebuilt before size change is triggered, we guess the center point using default size.
				//
				node.SetX(node.X - startingWidth / 2);
				node.SetY(node.Y - startingHeight / 2);
			}

			//
			// Add the node to the view-model.
			//

			if (!nodeAdded)
			{
				GetAppropriateNodeCollection(entity.IsNonScheduled).Add(node);
			}

			return node;
		}

		NodeViewModel GetOrCreateNode(INetworkEntity entity, out bool nodeCreated)
		{
			var node = GetNodeForEntity(entity);
			nodeCreated = node != null;
			if (!nodeCreated)
			{
				node = NodeViewModelProvider.Create(entity, this);

				node.InputConnectors.Add(new ConnectorViewModel(""));
				node.OutputConnectors.Add(new ConnectorViewModel(""));
			}

			return node;
		}

		#endregion

		#region Import Node

		public void ImportEntity(Location location, IProposedNetworkEntity selectedEntity)
		{
			var importedEntities = Network.PickAndImportEntities(selectedEntity);
			AddAllEntitiesIntoDiagram(importedEntities, location);
			Network.Refresh(RefreshType.EntityAdded, importedEntities.ToArray());
		}

		#endregion

		#region Remove Node

		public void RemoveFromDiagram(NodeViewModel node)
		{
			if (node == DiagramNodeViewModel)
			{
				return;
			}

			var hiddenEntities = Network.HideEntity(node.Entity);

			foreach (var diagramEntity in hiddenEntities)
			{
				var nodeToRemove = Nodes.FirstOrDefault(n => n.Entity.IsSameEntity(diagramEntity));
				if (nodeToRemove != null)
				{
					RemoveNodeCore(nodeToRemove);
				}
			}
		}

		public void DeleteNode(NodeViewModel node)
		{
			if (node == DiagramNodeViewModel)
			{
				return;
			}

			if (Network.DeleteEntity(node.Entity))
			{
				RemoveNodeCore(node);
			}
		}

		void RemoveNodeCore(NodeViewModel node)
		{
			//
			// Remove all connections attached to the node.
			//
			GetAppropriateConnectionCollection(node.Entity.IsNonScheduled).RemoveRange(node.AttachedConnections);

			//
			// Remove the node from the network.
			//
			GetAppropriateNodeCollection(node.Entity.IsNonScheduled).Remove(node);
		}

		#endregion

		#region Show

		/// <summary>
		/// Shows the entity on this diagram and adds to the view-model.
		/// </summary>
		public void ShowEntity(IProposedNetworkEntity entity, INetworkEntity parentEntity, Location location)
		{
			AddAllEntitiesIntoDiagram(Network.ShowEntity(entity, parentEntity), location);
		}

		public void AddAllEntitiesIntoDiagram(IEnumerable<INetworkEntity> entities, Location location)
		{
			var entitiesShown = new HashSet<INetworkEntity>(entities);
			var newTopLevelEntities = entitiesShown.Where(e => !entitiesShown.Contains(e.Parent)).ToArray();
			var nodesList = new List<NodeViewModel>();

			using (SuspendChildBoundsChecking())
			{
				foreach (var topLevelEntity in newTopLevelEntities)
				{
					AddEntityAndChildren(topLevelEntity, entitiesShown, nodesList, location, centerNode: false);
				}
			}
			Network.Refresh(RefreshType.EntityAdded);
		}

		void AddEntityAndChildren(INetworkEntity entity, HashSet<INetworkEntity> allEntities, List<NodeViewModel> newNodes, Location location, bool centerNode)
		{
			var node = SetDefaultsForNewNode(entity, location, centerNode);
			newNodes.Add(node);

			if (node.Entity.Parent != null)
			{
				var parentNode = Nodes.FirstOrDefault(n => n.Entity == node.Entity.Parent);
				if (parentNode != null)
				{
					using (parentNode.Entity.SuspendSettingHasChanges())
					{
						node.ZIndex = parentNode.ZIndex + 1;
					}
				}

				PersistLayoutParameters(node);

				if (!node.TryMoveToFit())
				{
					if (parentNode != null && parentNode.TryGrowToFit(node) != null)
					{
						node.TryShrinkToFit(parentNode);
					}
				}
			}

			node.AdjustToScale();

			var children = allEntities.Where(e => e.Parent == entity);
			foreach (var child in children)
			{
				AddEntityAndChildren(child, allEntities, newNodes, child.GetLocation(), centerNode: false);
			}
		}

		static void PersistLayoutParameters(params NodeViewModel[] nodes)
		{
			foreach (var node in nodes)
			{
				var layout = node.Entity;
				if (layout != null)
				{
					layout.X = node.X;
					layout.Y = node.Y;
					layout.Width = node.Width;
					layout.Height = node.Height;
				}
			}
		}

		public void ShowConnection(IEntityRelationship relationship)
		{
			var sourceNode = Nodes.FirstOrDefault(n => n.Entity.IsSameEntity(relationship.From));
			var destNode = Nodes.FirstOrDefault(n => n.Entity.IsSameEntity(relationship.To));

			if (sourceNode != null && destNode != null)
			{
				var sourceEntity = sourceNode.Entity;
				var destEntity = destNode.Entity;

				if (sourceEntity != null && destEntity != null)
				{
					var diagramRelationship = Network.ShowRelationship(sourceEntity, destEntity);
					if (diagramRelationship != null)
					{
						var connection = new ConnectionViewModel
						{
							Relationship = diagramRelationship,
							SourceConnector = sourceNode.OutputConnectors[0],
							DestConnector = destNode.InputConnectors[0],
						};
						GetAppropriateConnectionCollection(sourceEntity.IsNonScheduled).Add(connection);
					}
				}
			}
			Network.Refresh(RefreshType.ConnectionAdded);
		}

		#endregion

		#region Create and Remove Affinity Links

		public void CreateAffinityLink(IAffinity affinity, IProposedNetworkEntity proposedEntity)
		{
			var entity = proposedEntity as INetworkEntity;
			if (entity != null)
			{
				Network.DiagramEntity.CreateAffinityLink(entity, affinity);
				var node = GetNodeForEntity(entity);
				if (node != null && node.Entity == entity)
				{
					node.UpdateAffinityDetails();
				}
			}
		}

		public void RemoveAffinityLink(IAffinity affinity, IProposedNetworkEntity proposedEntity)
		{
			var entity = proposedEntity as INetworkEntity;
			if (entity != null)
			{
				Network.DiagramEntity.RemoveAffinityLink(entity, affinity);
				var node = GetNodeForEntity(entity);
				if (node != null && node.Entity == entity)
				{
					node.UpdateAffinityDetails();
				}
			}
		}

		#endregion

		#region Paste

		public bool TryHandlePaste()
		{
			return Network.TryHandlePaste(SelectedEntities);
		}

		#endregion

		#region Cache Property Propagation

		internal IEnumerable<string> GetWrappedPropertiesForNodeViewModel(string wrappedProperty)
		{
			var map = nodeViewModelWrappedPropertyMap ?? (nodeViewModelWrappedPropertyMap = WrappedPropertyPropagationProvider.GetNodeViewModelPropertyMap());
			string[] result;
			if (map.TryGetValue(wrappedProperty, out result))
			{
				return result;
			}
			else
			{
				return Enumerable.Empty<string>();
			}
		}
		Dictionary<string, string[]> nodeViewModelWrappedPropertyMap;

		#endregion

		#region Private Methods

		/// <summary>
		/// Event raised then Connections have been removed.
		/// </summary>
		void Connections_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
		{
			foreach (ConnectionViewModel connection in e.Items)
			{
				connection.SourceConnector = null;
				connection.DestConnector = null;
			}
		}

		#endregion Private Methods

		#region Strategies

		IDisposable SuspendChildBoundsChecking()
		{
			return new SuspendChildBoundsCheckingStrategyProvider(this);
		}

		IDisposable SuspendBoundsChecking()
		{
			return new SuspendBoundsCheckingStrategyProvider(this);
		}

		public IDisposable PerformNodeResize()
		{
			return new ResizeStrategyProvider(this);
		}

		internal CoordinatePropertyStrategyBase HeightCoordinateStrategy
		{
			get { return CoordinateStrategyProvider.Height; }
		}

		internal CoordinatePropertyStrategyBase WidthCoordinateStrategy
		{
			get { return CoordinateStrategyProvider.Width; }
		}

		internal CoordinatePropertyStrategyBase XCoordinateStrategy
		{
			get { return CoordinateStrategyProvider.X; }
		}

		internal CoordinatePropertyStrategyBase YCoordinateStrategy
		{
			get { return CoordinateStrategyProvider.Y; }
		}

		internal CoordinateStrategyProvider CoordinateStrategyProvider
		{
			get { return coordinateStrategy ?? (coordinateStrategy = new DefaultCoordinateStrategyProvider(this)); }
			set { coordinateStrategy = value; }
		}

		CoordinateStrategyProvider coordinateStrategy;

		#endregion

		#region Triggers

		void SubscribeSelectionChangedEventToNetwork()
		{
			if (Network.Refresher != null)
			{
				Network.Refresher.Refreshed += OnNetworkRefreshed;
			}
		}

		void UnsubscribeSelectionChangedEventFromNetwork()
		{
			if (Network.Refresher != null)
			{
				Network.Refresher.Refreshed -= OnNetworkRefreshed;
			}
		}

		void OnNetworkRefreshed(object sender, RefreshArgs args)
		{
			OnSelectionChanged(args.RefreshType);
		}

		#endregion

		#region Channels

		void PopulateChannelYAxisRanges()
		{
			var rangeStart = 0;
			var channels = Network.DiagramEntity?.DiagramChannels;

			if (channels != null)
			{
				foreach (var channel in channels)
				{
					var rangeEnd = rangeStart + GetChannelHeightAdjustedForMinAndMaxBoundaries(channel.Height);

					channelYAxisRanges[channel] = new Range<int>(rangeStart, rangeEnd);

					rangeStart = rangeEnd;
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<Tuple<IDiagramChannel, Range<int>>> GetChannelYAxisRanges()
		{
			return channelYAxisRanges.OrderBy(kvp => kvp.Value.Maximum).Select(kvp => Tuple.Create(kvp.Key, kvp.Value));
		}

		public Range<int> GetChannelYAxisRange(IDiagramChannel channel)
		{
			return channelYAxisRanges[channel];
		}

		readonly Dictionary<IDiagramChannel, Range<int>> channelYAxisRanges = new Dictionary<IDiagramChannel, Range<int>>();

		static int GetChannelHeightAdjustedForMinAndMaxBoundaries(int originalHeight)
		{
			var adjustedHeight = Math.Max(originalHeight, MinimumChannelHeight);
			adjustedHeight = Math.Min(adjustedHeight, MaximumChannelHeight);

			return adjustedHeight;
		}

		const int MinimumChannelHeight = 100;
		const int MaximumChannelHeight = 2000;

		#endregion

		#region Custom Network Actions Accessors

		public IEnumerable<INetworkAction> GetCustomNetworkActions() => Network.GetCustomNetworkActions(this);

		public IEnumerable<INetworkAction> GetCreateEntityActions() => Network.GetCreateEntityActions(this);

		#endregion
	}

	public class FocusedEntityChangedEventArgs : EventArgs
	{
		internal FocusedEntityChangedEventArgs(INetworkEntity focusedEntity)
		{
			FocusedEntity = focusedEntity;
		}

		public INetworkEntity FocusedEntity { get; }
	}
}
