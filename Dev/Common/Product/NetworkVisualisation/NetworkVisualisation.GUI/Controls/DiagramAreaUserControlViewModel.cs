using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class DiagramAreaUserControlViewModel : ViewModelBase
	{
		public NetworkUserControlViewModel NetworkControlViewModel { get; }
		public bool IsNonScheduled { get; }

		/// <summary>
		///  The parameterless constructor is required for DiagramAreaUserControl.InitializeComponent()
		/// </summary>
		public DiagramAreaUserControlViewModel()
		{
		}

		public DiagramAreaUserControlViewModel(NetworkUserControlViewModel networkControlViewModel, bool isNonScheduled)
		{
			Argument.NotNull(networkControlViewModel, nameof(networkControlViewModel));
			NetworkControlViewModel = networkControlViewModel;
			IsNonScheduled = isNonScheduled;
		}

		/// <summary>
		/// The width of the content (in content coordinates).
		/// </summary>
		public double ContentWidth
		{
			get => contentWidth - ContentLeftMargin;
			set
			{
				contentWidth = value;
				OnPropertyChanged();
			}
		}

		double contentWidth;

		/// <summary>
		/// The heigth of the content (in content coordinates).
		/// </summary>
		public double ContentHeight
		{
			get => contentHeight;
			set
			{
				contentHeight = value;
				OnPropertyChanged();
			}
		}

		double contentHeight;

		/// <summary>
		/// The width of the viewport onto the content (in content coordinates).
		/// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
		/// view-model so that the value can be shared with the overview window.
		/// </summary>
		public double ContentViewportWidth
		{
			get => contentViewportWidth;
			set
			{
				contentViewportWidth = value;
				OnPropertyChanged();
			}
		}

		double contentViewportWidth;

		/// <summary>
		/// The height of the viewport onto the content (in content coordinates).
		/// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
		/// view-model so that the value can be shared with the overview window.
		/// </summary>
		public double ContentViewportHeight
		{
			get => contentViewportHeight;
			set
			{
				contentViewportHeight = value;
				OnPropertyChanged();
			}
		}

		double contentViewportHeight;

		/// <summary>
		/// Represent the left space value between contents on the diagram (in content coordinates).
		/// and used when calculating content width.
		/// </summary>
		public double ContentLeftMargin
		{
			get => contentLeftMargin;
			set
			{
				contentLeftMargin = value;
				OnPropertyChanged();
			}
		}

		double contentLeftMargin;

		/// <summary>
		/// The X coordinate of the offset of the viewport onto the content (in content coordinates).
		/// </summary>
#if WINZOR
		public double ContentOffsetX => NetworkControlViewModel.ContentOffsetX;
#else
		public double ContentOffsetX
		{
			get => contentOffsetX;
			set
			{
				contentOffsetX = value;
				OnPropertyChanged();
			}
		}
		public double contentOffsetX;
#endif

		/// <summary>
		/// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
		/// </summary>
		public double ContentOffsetY => NetworkControlViewModel.ContentOffsetY;
		public double ContentScale => NetworkControlViewModel.ContentScale;

		#region Create

		/// <summary>
		/// Create a new node, add it to the view-model, and add to the underlying model.
		/// </summary>
		public NodeViewModel CreateNewNode(Location location, INetworkEntity entity, Action<NodeViewModel> rememberLastNodeLocation, bool disableCentering = false)
		{
			if (entity == null)
			{
				return null;
			}

			var diagramEntity = NetworkViewModel.Network.DiagramEntity;
			var node = CreateNode(entity, location, centerNode: !diagramEntity.IsDiagramScaled && !disableCentering, rememberLastNodeLocation);
			node.BringToFront();

			if (diagramEntity.IsDiagramScaled)
			{
				node.AdjustToScale();
			}
			node.FitToParent();
			NetworkViewModel.Network.Refresh(RefreshType.EntityAdded, entity);
			return node;
		}

		public NodeViewModel CreateNewNode(Location location, INetworkEntity entity, bool disableCentering = false)
		{
			return CreateNewNode(location, entity, RememberLastCreatedNodeLocation, disableCentering);
		}

		/// <summary>
		/// Create a node and add it to the view-model.
		/// </summary>
		public NodeViewModel CreateNode(INetworkEntity entity, Location nodeLocation, bool centerNode, Action<NodeViewModel> rememberLastNodeLocation)
		{
			var node = NetworkViewModel.SetDefaultsForNewNode(entity, nodeLocation, centerNode);
			rememberLastNodeLocation(node);
			return node;
		}

		public NodeViewModel CreateNode(INetworkEntity entity, Location nodeLocation, bool centerNode)
		{
			return CreateNode(entity, nodeLocation, centerNode, RememberLastCreatedNodeLocation);
		}

		public IEnumerable<NodeViewModel> CreateNewNodes(IEnumerable<INetworkEntity> entities, Location nodeLocation)
		{
			var nodes = NetworkViewModel.SetDefaultsForNewNodes(entities, new Location(nodeLocation.X, nodeLocation.Y));
			RememberLastCreatedNodeLocation(nodes.Last());
			return nodes;
		}

		public Point LastCreatedNodeLocationRelativeToViewport { get; private set; }

		public void RememberLastCreatedNodeLocation(NodeViewModel node)
		{
			LastCreatedNodeLocationRelativeToViewport = new Point(node.X - ContentOffsetX, node.Y - ContentOffsetY);
		}

		public void RememberLastCreatedNodeLocationRelativeToZoom(NodeViewModel node)
		{
			LastCreatedNodeLocationRelativeToViewport = new Point(node.X - (ContentOffsetX / ContentScale), node.Y - (ContentOffsetY / ContentScale));
		}

		public void ResetLastCreatedNodeLocation()
		{
			LastCreatedNodeLocationRelativeToViewport = new Point(0, 0);
		}

		#endregion

		#region Delete

		/// <summary>
		/// Delete the currently selected nodes from the view-model.
		/// </summary>
		public void DeleteSelectedNodes()
		{
			// Take a copy of the selected nodes list so we can delete nodes while iterating.
			var nodesCopy = NetworkViewModel.Nodes.ToArray();
			foreach (var node in nodesCopy)
			{
				if (node.IsSelected)
				{
					DeleteNode(node);
				}
			}
		}

		/// <summary>
		/// Delete the node from the view-model.
		/// Also deletes any connections to or from the node.
		/// </summary>
		public void DeleteNode(NodeViewModel node)
		{
			NetworkViewModel.DeleteNode(node);
		}

		#endregion

		#region Hide / Show

		/// <summary>
		/// Hides this node from the model (leaving the underlying entity in place).
		/// Removes this node from the view-model and any connections (without deleting).
		/// </summary>
		public void HideNode(NodeViewModel node)
		{
			NetworkViewModel.RemoveFromDiagram(node);
		}

		/// <summary>
		/// Shows the entity on this diagram and adds to the view-model.
		/// </summary>
		public void ShowEntity(IProposedNetworkEntity entity, INetworkEntity parentEntity, Point location)
		{
			NetworkViewModel.AddAllEntitiesIntoDiagram(NetworkViewModel.Network.ShowEntity(entity, parentEntity), new Location(location.X, location.Y));
		}

		#endregion

		#region Import

		public void ImportEntity(Point location, IProposedNetworkEntity selectedEntity)
		{
			NetworkViewModel.ImportEntity(new Location(location.X, location.Y), selectedEntity);
		}

		#endregion

		#region CreateAndRemoveAffinityLinks

		public void EditAffinities()
		{
			NetworkViewModel.Network.ModifyAffinities(NetworkViewModel.Network.DiagramEntity);
		}

		public void CreateAffinityLink(IAffinity affinity, INetworkEntity entity)
		{
			NetworkViewModel.Network.DiagramEntity.CreateAffinityLink(entity, affinity);
			foreach (var node in NetworkViewModel.Nodes)
			{
				if (node.Entity == entity)
				{
					node.UpdateAffinityDetails();
				}
			}
		}

		public void RemoveAffinityLink(IAffinity affinity, INetworkEntity entity)
		{
			NetworkViewModel.Network.DiagramEntity.RemoveAffinityLink(entity, affinity);
			foreach (var node in NetworkViewModel.Nodes)
			{
				if (node.Entity == entity)
				{
					node.UpdateAffinityDetails();
				}
			}
		}

		#endregion

		#region Connections

		/// <summary>
		/// Called when the user has started to drag out a connector, thus creating a new connection.
		/// </summary>
		public ConnectionViewModel ConnectionDragStarted(ConnectorViewModel draggedOutConnector, Point curDragPoint)
		{
			if (draggedOutConnector.ParentNode != null)
			{
				var entity = draggedOutConnector.ParentNode.Entity;
				if (entity != null && !entity.CanCreateRelationship(null))
				{
					return null;
				}
			}

			//
			// Create a new connection to add to the view-model.
			//
			var connection = new ConnectionViewModel();

			if (draggedOutConnector.Type == ConnectorType.Output)
			{
				//
				// The user is dragging out a source connector (an output) and will connect it to a destination connector (an input).
				//
				connection.SourceConnector = draggedOutConnector;
				connection.DestConnectorHotspot = new Location(curDragPoint.X, curDragPoint.Y);
			}
			else
			{
				//
				// The user is dragging out a destination connector (an input) and will connect it to a source connector (an output).
				//
				connection.DestConnector = draggedOutConnector;
				connection.SourceConnectorHotspot = new Location(curDragPoint.X, curDragPoint.Y);
			}

			//
			// Add the new connection to the view-model.
			//
			this.NetworkViewModel.AddConnection(connection);

			return connection;
		}

		/// <summary>
		/// Called to query the application for feedback while the user is dragging the connection.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public void QueryConnnectionFeedback(ConnectorViewModel draggedOutConnector, ConnectorViewModel draggedOverConnector, out object feedbackIndicator, out bool connectionOk)
		{
			if (draggedOutConnector == draggedOverConnector)
			{
				//
				// Can't connect to self!
				// Provide feedback to indicate that this connection is not valid!
				//
				feedbackIndicator = new ConnectionBadIndicator();
				connectionOk = false;
			}
			else
			{
				var sourceConnector = draggedOutConnector;
				var destConnector = draggedOverConnector;

				//
				// Only allow connections from output connector to input connector (ie each
				// connector must have a different type).
				// Also only allocation from one node to another, never one node back to the same node.
				//
				connectionOk = IsConnectionOK(sourceConnector, destConnector);

				if (connectionOk)
				{
					// 
					// Yay, this is a valid connection!
					// Provide feedback to indicate that this connection is ok!
					//
					feedbackIndicator = new ConnectionOkIndicator();
				}
				else
				{
					//
					// Connectors with the same connector type (eg input & input, or output & output)
					// can't be connected.
					// Only connectors with separate connector type (eg input & output).
					// Provide feedback to indicate that this connection is not valid!
					//
					feedbackIndicator = new ConnectionBadIndicator();
				}
			}
		}

		/// <summary>
		/// Only allow connections from output connector to input connector (ie each
		/// connector must have a different type).
		/// Also only allocation from one node to another, never one node back to the same node.
		/// </summary>
		static bool IsConnectionOK(ConnectorViewModel sourceConnector, ConnectorViewModel destConnector)
		{
			if (sourceConnector.ParentNode != destConnector.ParentNode && sourceConnector.Type != destConnector.Type)
			{
				var sourceEntityLayout = sourceConnector.ParentNode.Entity;
				var destEntityLayout = destConnector.ParentNode.Entity;

				if (sourceEntityLayout != null && destEntityLayout != null)
				{
					return sourceEntityLayout.CanCreateRelationship(destEntityLayout);
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Called as the user continues to drag the connection.
		/// </summary>
		public void ConnectionDragging(Point curDragPoint, ConnectionViewModel connection)
		{
			if (connection.DestConnector == null)
			{
				connection.DestConnectorHotspot = new Location(curDragPoint.X, curDragPoint.Y);
			}
			else
			{
				connection.SourceConnectorHotspot = new Location(curDragPoint.X, curDragPoint.Y);
			}
		}

		/// <summary>
		/// Called when the user has finished dragging out the new connection.
		/// </summary>
		public void ConnectionDragCompleted(ConnectionViewModel newConnection, ConnectorViewModel connectorDraggedOut, ConnectorViewModel connectorDraggedOver)
		{
			if (connectorDraggedOver == null)
			{
				//
				// The connection was unsuccessful.
				// Maybe the user dragged it out and dropped it in empty space.
				//
				this.NetworkViewModel.RemoveConnection(newConnection);
				return;
			}

			if (!IsConnectionOK(connectorDraggedOut, connectorDraggedOver))
			{
				//
				// Connections between connectors that have the same type,
				// eg input -> input or output -> output, are not allowed,
				// Remove the connection.
				//
				this.NetworkViewModel.RemoveConnection(newConnection);
				return;
			}

			//
			// The user has dragged the connection on top of another valid connector.
			//

			//
			// Remove any existing connection between the same two connectors.
			//
			var existingConnection = FindConnection(connectorDraggedOut, connectorDraggedOver);
			if (existingConnection != null)
			{
				this.NetworkViewModel.RemoveConnection(existingConnection);
			}

			//
			// Finalize the connection by attaching it to the connector
			// that the user dragged the mouse over.
			//
			if (newConnection.DestConnector == null)
			{
				newConnection.DestConnector = connectorDraggedOver;
			}
			else
			{
				newConnection.SourceConnector = connectorDraggedOver;
			}

			var sourceEntity = newConnection.SourceConnector.ParentNode.Entity;
			var destEntity = newConnection.DestConnector.ParentNode.Entity;

			if (sourceEntity != null && destEntity != null)
			{
				var relationship = NetworkViewModel.Network.CreateRelationship(sourceEntity, destEntity);
				if (relationship == null)
				{
					NetworkViewModel.RemoveConnection(newConnection);
				}
				else
				{
					newConnection.Relationship = relationship;
				}
			}
		}

		/// <summary>
		/// Retrieve a connection between the two connectors.
		/// Returns null if there is no connection between the connectors.
		/// </summary>
		public ConnectionViewModel FindConnection(ConnectorViewModel connector1, ConnectorViewModel connector2)
		{
			NetworkVisualisationErrorReporter.ReportIfAssertionFailed(connector1.Type != connector2.Type);

			//
			// Figure out which one is the source connector and which one is the
			// destination connector based on their connector types.
			//
			var sourceConnector = connector1.Type == ConnectorType.Output ? connector1 : connector2;
			var destConnector = connector1.Type == ConnectorType.Output ? connector2 : connector1;

			//
			// Now we can just iterate attached connections of the source
			// and see if it each one is attached to the destination connector.
			//

			foreach (var connection in sourceConnector.AttachedConnections)
			{
				if (connection.DestConnector == destConnector)
				{
					//
					// Found a connection that is outgoing from the source connector
					// and incoming to the destination connector.
					//
					return connection;
				}
			}

			return null;
		}

		/// <summary>
		/// Utility method to delete a connection from the view-model.
		/// </summary>
		public void DeleteConnection(ConnectionViewModel connection)
		{
			var shouldDelete = true;

			if (connection.Relationship != null)
			{
				shouldDelete = NetworkViewModel.Network.DeleteRelationship(connection.Relationship);
			}

			if (shouldDelete)
			{
				NetworkViewModel.RemoveConnection(connection);
			}
		}

		public void HideConnection(ConnectionViewModel connection)
		{
			var shouldRemove = NetworkViewModel.Network.HideRelationship(connection.Relationship);
			if (shouldRemove)
			{
				NetworkViewModel.RemoveConnection(connection);
			}
		}

		/// <summary>
		/// Shows the entity on this diagram and adds to the view-model.
		/// </summary>
		public void ShowConnection(IEntityRelationship relationship)
		{
			NetworkViewModel.ShowConnection(relationship);
		}

		#endregion

		public NetworkViewModel NetworkViewModel => NetworkControlViewModel?.NetworkViewModel;
	}
}
