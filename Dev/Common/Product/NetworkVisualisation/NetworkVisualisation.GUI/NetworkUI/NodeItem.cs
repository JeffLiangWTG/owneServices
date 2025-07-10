using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// This is a UI element that represents a network/flow-chart node.
	/// </summary>
	public class NodeItem : ListBoxItem, IDragResizable
	{
		#region Dependency Property/Event Definitions

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly DependencyProperty IsContinuouslyChangingProperty =
					DependencyProperty.Register("IsContinuouslyChanging", typeof(bool), typeof(NodeItem),
						new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly DependencyProperty XProperty =
					DependencyProperty.Register("X", typeof(double), typeof(NodeItem),
						new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly DependencyProperty YProperty =
					DependencyProperty.Register("Y", typeof(double), typeof(NodeItem),
						new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly DependencyProperty ZIndexProperty =
					DependencyProperty.Register("ZIndex", typeof(int), typeof(NodeItem),
						new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static readonly DependencyProperty ParentNetworkViewProperty =
					DependencyProperty.Register("ParentNetworkView", typeof(NetworkView), typeof(NodeItem),
						new FrameworkPropertyMetadata(ParentNetworkView_PropertyChanged));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static readonly RoutedEvent NodeDragStartedEvent =
					EventManager.RegisterRoutedEvent("NodeDragStarted", RoutingStrategy.Bubble, typeof(NodeDragStartedEventHandler), typeof(NodeItem));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static readonly RoutedEvent NodeDraggingEvent =
					EventManager.RegisterRoutedEvent("NodeDragging", RoutingStrategy.Bubble, typeof(NodeDraggingEventHandler), typeof(NodeItem));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly RoutedEvent NodeDragCompletedEvent =
					EventManager.RegisterRoutedEvent("NodeDragCompleted", RoutingStrategy.Bubble, typeof(NodeDragCompletedEventHandler), typeof(NodeItem));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static readonly RoutedEvent NodeResizeStartedEvent =
					EventManager.RegisterRoutedEvent("NodeResizeStarted", RoutingStrategy.Bubble, typeof(EventHandler<NodeResizeStartedEventArgs>), typeof(NodeItem));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static readonly RoutedEvent NodeResizingEvent =
					EventManager.RegisterRoutedEvent("NodeResizing", RoutingStrategy.Bubble, typeof(EventHandler<NodeResizingEventArgs>), typeof(NodeItem));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static readonly RoutedEvent NodeResizeCompletedEvent =
					EventManager.RegisterRoutedEvent("NodeResizeCompleted", RoutingStrategy.Bubble, typeof(EventHandler<NodeResizeCompletedEventArgs>), typeof(NodeItem));

		#endregion Dependency Property/Event Definitions

		public NodeItem()
		{
			//
			// By default, we don't want this UI element to be focusable.
			//
			Focusable = false;
		}

		/// <summary>
		/// A semaphore to determine if the node is in the process of being dragged or resized
		/// </summary>
		public bool IsContinuouslyChanging
		{
			get
			{
				return (bool)GetValue(IsContinuouslyChangingProperty);
			}
			set
			{
				SetValue(IsContinuouslyChangingProperty, value);
			}
		}

		/// <summary>
		/// The X coordinate of the node.
		/// </summary>
		public double X
		{
			get
			{
				return (double)GetValue(XProperty);
			}
			set
			{
				SetValue(XProperty, value);
			}
		}

		/// <summary>
		/// The Y coordinate of the node.
		/// </summary>
		public double Y
		{
			get
			{
				return (double)GetValue(YProperty);
			}
			set
			{
				SetValue(YProperty, value);
			}
		}

		/// <summary>
		/// The Z index of the node.
		/// </summary>
		public int ZIndex
		{
			get
			{
				return (int)GetValue(ZIndexProperty);
			}
			set
			{
				SetValue(ZIndexProperty, value);
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

		NetworkViewMouseController MouseController => ParentNetworkView.MouseController;

		#endregion

		/// <summary>
		/// The threshold distance the mouse-cursor must move before dragging begins.
		/// </summary>
		const double DragThreshold = 5;

		#region Private Methods

		/// <summary>
		/// Static constructor.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static NodeItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeItem), new FrameworkPropertyMetadata(typeof(NodeItem)));
		}

		#region Mouse Event Handlers

		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			base.OnLostMouseCapture(e);

			var wasDragging = MouseController.IsDragging;
			MouseController.ProcessLostMouseCapture();

			if (wasDragging)
			{
				NotifyStoppedDragging();
			}
		}

		/// <summary>
		/// Called when a mouse button is held down.
		/// </summary>
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);

			ParentNetworkView?.Focus();

			if (e.ChangedButton == MouseButton.Left && this.ParentNetworkView != null)
			{
				MouseController.ProcessLeftMouseDownOnElement(e, this);
				LeftMouseDownSelectionLogic();
				e.Handled = true;
			}
			else if (e.ChangedButton == MouseButton.Right)
			{
				if (ParentNetworkView != null)
				{
					this.ParentNetworkView.IsClearSelectionOnEmptySpaceClickEnabled = false;
					RightMouseDownSelectionLogic();
				}
				MouseController.StopDraggingLeftClickedElement();
			}
		}

		/// <summary>
		/// Called when a mouse double clicks a control.
		/// The control is selected upon double click.
		/// </summary>
		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);
			if (this.ParentNetworkView != null)
			{
				MouseController.ProcessDoubleClick(e);
				LeftMouseDownSelectionLogic();
				e.Handled = true;
			}
		}

		/// <summary>
		/// This method contains selection logic that is invoked when the left mouse button is pressed down.
		/// The reason this exists in its own method rather than being included in OnMouseDown is 
		/// so that ConnectorItem can reuse this logic from its OnMouseDown.
		/// </summary>
		internal void LeftMouseDownSelectionLogic()
		{
			if (!MouseController.IsCtrlPressed)
			{
				if (this.ParentNetworkView.SelectedNodes.Count == 0)
				{
					//
					// Nothing already selected, select the item.
					//
					this.IsSelected = true;
				}
				else if (this.ParentNetworkView.SelectedNodes.Contains(this) ||
						 this.ParentNetworkView.SelectedNodes.Contains(this.DataContext))
				{
					// 
					// Item is already selected, do nothing.
					// We will act on this in the MouseUp if there was no drag operation.
					//
				}
				else
				{
					//
					// Item is not selected.
					// Deselect all, and select the item.
					//
					this.ParentNetworkView.SelectedNodes.Clear();
					this.IsSelected = true;
				}
			}
			else
			{
				//
				// Control key was held down.
				// Toggle the selection.
				//
				this.IsSelected = !this.IsSelected;
			}
		}

		/// <summary>
		/// This method contains selection logic that is invoked when the right mouse button is pressed down.
		/// The reason this exists in its own method rather than being included in OnMouseDown is 
		/// so that ConnectorItem can reuse this logic from its OnMouseDown.
		/// </summary>
		internal void RightMouseDownSelectionLogic()
		{
			if (this.ParentNetworkView.SelectedNodes.Count == 0)
			{
				//
				// Nothing already selected, select the item.
				//
				this.IsSelected = true;
			}
			else if (this.ParentNetworkView.SelectedNodes.Contains(this) ||
					 this.ParentNetworkView.SelectedNodes.Contains(this.DataContext))
			{
				// 
				// Item is already selected, do nothing.
				//
			}
			else
			{
				//
				// Item is not selected.
				// Deselect all, and select the item.
				//
				this.ParentNetworkView.SelectedNodes.Clear();
				this.IsSelected = true;
			}
		}

		/// <summary>
		/// Called when the mouse cursor is moved.
		/// </summary>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (IsSelected)
			{
				if (MouseController.IsDragging)
				{
					//
					// Raise the event to notify that dragging is in progress.
					//

					object item = this;
					if (DataContext != null)
					{
						item = DataContext;
					}

					MouseController.ProcessMouseMoveWhileDragging(e,
						(offset) => RaiseEvent(new NodeDraggingEventArgs(NodeDraggingEvent, this, new object[] { item }, offset.X, offset.Y)));
					e.Handled = true;
				}
				else if (MouseController.IsLeftMouseDown && this.ParentNetworkView.EnableNodeDragging)
				{
					//
					// The user is left-dragging the node,
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
						NodeDragStartedEventArgs eventArgs = new NodeDragStartedEventArgs(NodeDragStartedEvent, this, new NodeItem[] { this });
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
		/// Called when a mouse button is released.
		/// </summary>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.ChangedButton == MouseButton.Left)
			{
				var wasDragging = MouseController.IsDragging;
				MouseController.ProcessLeftMouseUp();

				if (wasDragging && IsSelected)
				{
					NotifyStoppedDragging();
				}
				e.Handled = true;
			}
		}

		void NotifyStoppedDragging()
		{
			RaiseEvent(new NodeDragCompletedEventArgs(NodeDragCompletedEvent, this, new NodeItem[] { this }));
		}

		#endregion

		/// <summary>
		/// Event raised when the ParentNetworkView property has changed.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		static void ParentNetworkView_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			//
			// Bring new nodes to the front of the z-order.
			//
			var nodeItem = (NodeItem)o;
			//TODO: just call this for new entities created through the designer, not existing entities added to the surface.
			//nodeItem.BringToFront();
		}

		#endregion Private Methods

		#region IDragResizable

		void IDragResizable.NotifyResizeStarted(DragStartedEventArgs e)
		{
			RaiseEvent(new NodeResizeStartedEventArgs(NodeResizeStartedEvent, this, new NodeItem[] { this }));
		}

		void IDragResizable.NotifyResizing(DragDeltaEventArgs e)
		{
			ParentNetworkView.Focus();
			RaiseEvent(new NodeResizingEventArgs(NodeResizingEvent, this, new NodeItem[] { this }, e.HorizontalChange, e.VerticalChange));
		}

		void IDragResizable.NotifyResizeCompleted(DragCompletedEventArgs e)
		{
			RaiseEvent(new NodeResizeCompletedEventArgs(NodeResizeCompletedEvent, this, new NodeItem[] { this }));
		}

		#endregion
	}
}
