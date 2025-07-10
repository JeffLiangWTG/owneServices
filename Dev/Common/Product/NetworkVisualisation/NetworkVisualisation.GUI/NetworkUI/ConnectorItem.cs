using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// This is the UI element for a connector.
	/// Each nodes has multiple connectors that are used to connect it to other nodes.
	/// </summary>
	public class ConnectorItem : ContentControl
	{
		#region Dependency Property/Event Definitions

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static readonly DependencyProperty HotspotProperty =
			DependencyProperty.Register("Hotspot", typeof(Point), typeof(ConnectorItem));

		internal static readonly DependencyProperty ParentNetworkViewProperty =
			DependencyProperty.Register("ParentNetworkView", typeof(NetworkView), typeof(ConnectorItem),
				new FrameworkPropertyMetadata(ParentNetworkView_PropertyChanged));

		internal static readonly DependencyProperty ParentNodeItemProperty =
			DependencyProperty.Register("ParentNodeItem", typeof(NodeItem), typeof(ConnectorItem));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static readonly RoutedEvent ConnectorDragStartedEvent =
			EventManager.RegisterRoutedEvent("ConnectorDragStarted", RoutingStrategy.Bubble, typeof(ConnectorItemDragStartedEventHandler), typeof(ConnectorItem));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static readonly RoutedEvent ConnectorDraggingEvent =
			EventManager.RegisterRoutedEvent("ConnectorDragging", RoutingStrategy.Bubble, typeof(ConnectorItemDraggingEventHandler), typeof(ConnectorItem));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static readonly RoutedEvent ConnectorDragCompletedEvent =
			EventManager.RegisterRoutedEvent("ConnectorDragCompleted", RoutingStrategy.Bubble, typeof(ConnectorItemDragCompletedEventHandler), typeof(ConnectorItem));

		#endregion Dependency Property/Event Definitions

		/// <summary>
		/// The threshold distance the mouse-cursor must move before dragging begins.
		/// </summary>
		const double DragThreshold = 2;

		public ConnectorItem()
		{
			//
			// By default, we don't want a connector to be focusable.
			//
			Focusable = false;

			//
			// Hook layout update to recompute 'Hotspot' when the layout changes.
			//
			this.LayoutUpdated += new EventHandler(ConnectorItem_LayoutUpdated);
		}

		/// <summary>
		/// Automatically updated dependency property that specifies the hotspot (or center point) of the connector.
		/// Specified in content coordinate.
		/// </summary>
		public Point Hotspot
		{
			get
			{
				return (Point)GetValue(HotspotProperty);
			}
			set
			{
				SetValue(HotspotProperty, value);
			}
		}

		#region Accessors

		/// <summary>
		/// Reference to the data-bound parent NetworkView.
		/// </summary>
		internal NetworkView ParentNetworkView
		{
			get
			{
				return (NetworkView)GetValue(ParentNetworkViewProperty);
			}
			set
			{
				SetValue(ParentNetworkViewProperty, value);
			}
		}

		/// <summary>
		/// Reference to the data-bound parent NodeItem.
		/// </summary>
		public NodeItem ParentNodeItem
		{
			get
			{
				return (NodeItem)GetValue(ParentNodeItemProperty);
			}
			set
			{
				SetValue(ParentNodeItemProperty, value);
			}
		}

		NetworkViewMouseController MouseController => ParentNetworkView?.MouseController;

		#endregion Accessors

		#region Private Methods

		/// <summary>
		/// Static constructor.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static ConnectorItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ConnectorItem), new FrameworkPropertyMetadata(typeof(ConnectorItem)));
		}

		#region Mouse Event Handlers

		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			base.OnLostMouseCapture(e);

			if (MouseController == null)
			{
				return;
			}

			var wasDragging = MouseController.IsDragging;
			MouseController.ProcessLostMouseCapture();

			if (wasDragging)
			{
				NotifyStoppedDragging();
			}
		}

		/// <summary>
		/// A mouse button has been held down.
		/// </summary>
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);

			if (MouseController == null)
			{
				return;
			}

			ParentNetworkView?.Focus();

			if (e.ChangedButton == MouseButton.Left)
			{
				MouseController.ProcessLeftMouseDownOnElement(e, this);

				ParentNodeItem?.LeftMouseDownSelectionLogic();
				e.Handled = true;
			}
			else if (e.ChangedButton == MouseButton.Right)
			{
				ParentNodeItem?.RightMouseDownSelectionLogic();

				// OnMouseUp only called once for multiple OnMouseDown
				if (MouseController.IsDragging)
				{
					NotifyStoppedDragging();
				}

				MouseController.StopDraggingLeftClickedElement();
			}
		}

		/// <summary>
		/// The mouse cursor has been moved.
		/// </summary>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (MouseController == null)
			{
				return;
			}

			if (MouseController.IsDragging)
			{
				//
				// Raise the event to notify that dragging is in progress.
				//
				MouseController.ProcessMouseMoveWhileDragging(e,
					(offset) => RaiseEvent(new ConnectorItemDraggingEventArgs(ConnectorDraggingEvent, this, offset.X, offset.Y)));
				e.Handled = true;
			}
			else if (MouseController.IsLeftMouseDown)
			{
				if (this.ParentNetworkView != null &&
					this.ParentNetworkView.EnableConnectionDragging)
				{
					//
					// The user is left-dragging the connector and connection dragging is enabled,
					// but don't initiate the drag operation until 
					// the mouse cursor has moved more than the threshold distance.
					//
					if (MouseController.IsFarEnoughToStartDragging(e, DragThreshold))
					{
						//
						// When the mouse has been dragged more than the threshold value commence dragging the node.
						//

						//
						// Raise an event to notify that that dragging has commenced.
						//
						var eventArgs = new ConnectorItemDragStartedEventArgs(ConnectorDragStartedEvent, this);
						RaiseEvent(eventArgs);

						if (eventArgs.Cancel)
						{
							//
							// Handler of the event disallowed dragging of the node.
							//
							MouseController.StopDraggingLeftClickedElement();
							return;
						}

						MouseController.StartDraggingLeftClickedElement();
						e.Handled = true;
					}
				}
			}
		}

		/// <summary>
		/// A mouse button has been released.
		/// </summary>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);

			if (MouseController == null)
			{
				return;
			}

			if (e.ChangedButton == MouseButton.Left)
			{
				var wasDragging = MouseController.IsDragging;
				MouseController.ProcessLeftMouseUp();

				if (wasDragging)
				{
					NotifyStoppedDragging();
				}
				e.Handled = true;
			}
		}

		void NotifyStoppedDragging()
		{
			RaiseEvent(new ConnectorItemDragCompletedEventArgs(ConnectorDragCompletedEvent, this));
		}

		/// <summary>
		/// Cancel connection dragging for the connector that was dragged out.
		/// </summary>
		internal void CancelConnectionDragging()
		{
			if (MouseController == null)
			{
				return;
			}

			if (MouseController.IsLeftMouseDown)
			{
				//
				// Raise ConnectorDragCompleted, with a null connector.
				//
				RaiseEvent(new ConnectorItemDragCompletedEventArgs(ConnectorDragCompletedEvent, null));

				MouseController.StopDraggingLeftClickedElement();
			}
		}

		#endregion

		/// <summary>
		/// Event raised when 'ParentNetworkView' property has changed.
		/// </summary>
		static void ParentNetworkView_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ConnectorItem c = (ConnectorItem)d;
			c.UpdateHotspot();
		}

		/// <summary>
		/// Event raised when the layout of the connector has been updated.
		/// </summary>
		void ConnectorItem_LayoutUpdated(object sender, EventArgs e)
		{
			UpdateHotspot();
		}

		/// <summary>
		/// Update the connector hotspot.
		/// </summary>
		void UpdateHotspot()
		{
			if (this.ParentNetworkView == null)
			{
				// No parent NetworkView is set.
				return;
			}

			if (!this.ParentNetworkView.IsAncestorOf(this))
			{
				//
				// The parent NetworkView is no longer an ancestor of the connector.
				// This happens when the connector (and its parent node) has been removed from the network.
				// Reset the property null so we don't attempt to check again.
				//
				this.ParentNetworkView = null;
				return;
			}

			//
			// The parent NetworkView is still valid.
			// Compute the center point of the connector.
			//
			var centerPoint = new Point(this.ActualWidth / 2, this.ActualHeight / 2);

			//
			// Transform the center point so that it is relative to the parent NetworkView.
			// Then assign it to Hotspot.  Usually Hotspot will be data-bound to the application
			// view-model using OneWayToSource so that the value of the hotspot is then pushed through
			// to the view-model.
			//
			this.Hotspot = this.TransformToAncestor(this.ParentNetworkView).Transform(centerPoint);
		}

		#endregion Private Methods
	}
}
