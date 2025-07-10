using System;
using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// Partial definition of the NetworkView class.
	/// This file only contains private members related to dragging nodes.
	/// </summary>
	public partial class NetworkView
	{
		#region Private Methods

		/// <summary>
		/// Cached list of selected NodeItems, used while dragging nodes.
		/// </summary>
		List<NodeItem> cachedSelectedNodeItems;

		/// <summary>
		/// Event raised when the user starts to drag a node.
		/// </summary>
		void NodeItem_DragStarted(object source, NodeDragStartedEventArgs e)
		{
			e.Handled = true;

			IsDragging = true;
			IsNotDragging = false;
			IsDraggingNode = true;
			IsNotDraggingNode = false;

			var eventArgs = new NodeDragStartedEventArgs(NodeDragStartedEvent, this, this.SelectedNodes);
			RaiseEvent(eventArgs);

			e.Cancel = eventArgs.Cancel;
		}

		/// <summary>
		/// Event raised while the user is dragging a node.
		/// </summary>
		void NodeItem_Dragging(object source, NodeDraggingEventArgs e)
		{
			e.Handled = true;

			//
			// Cache the NodeItem for each selected node whilst dragging is in progress.
			//
			if (cachedSelectedNodeItems == null)
			{
				cachedSelectedNodeItems = new List<NodeItem>();

				foreach (var selectedNode in SelectedNodes)
				{
					NodeItem nodeItem = FindAssociatedNodeItem(selectedNode)
						?? throw new ArgumentException("Unexpected code path!");

					nodeItem.IsContinuouslyChanging = true;

					var viewModel = nodeItem.DataContext as NodeViewModel;
					viewModel.IsDragged = true;

					cachedSelectedNodeItems.Add(nodeItem);
				}
			}

			// 
			// Update the position of the node within the Canvas.
			//
			foreach (var nodeItem in cachedSelectedNodeItems)
			{
				nodeItem.X += e.HorizontalChange;
				nodeItem.Y += e.VerticalChange;
			}

			var eventArgs = new NodeDraggingEventArgs(NodeDraggingEvent, this, SelectedNodes, e.HorizontalChange, e.VerticalChange);
			RaiseEvent(eventArgs);
		}

		/// <summary>
		/// Event raised when the user has finished dragging a node.
		/// </summary>
		void NodeItem_DragCompleted(object source, NodeDragCompletedEventArgs e)
		{
			e.Handled = true;

			if (cachedSelectedNodeItems != null)
			{
				foreach (var nodeItem in cachedSelectedNodeItems)
				{
					if (nodeItem == null)
					{
						throw new ArgumentException("Unexpected code path!");
					}

					if (nodeItem.DataContext is NodeViewModel viewModel)
					{
						viewModel.IsDragged = false;
					}

					nodeItem.IsContinuouslyChanging = false;
				}
			}

			var eventArgs = new NodeDragCompletedEventArgs(NodeDragCompletedEvent, this, this.SelectedNodes);
			RaiseEvent(eventArgs);

			cachedSelectedNodeItems = null;

			IsDragging = false;
			IsNotDragging = true;
			IsDraggingNode = false;
			IsNotDraggingNode = true;
		}

		#endregion Private Methods
	}
}
