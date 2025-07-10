using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CargoWise.Common;
using CargoWise.Main.Navigation.WPF;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Control = System.Windows.Controls.Control;
using Cursors = System.Windows.Input.Cursors;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// This is a Window that uses NetworkView to display a flow-chart.
	/// </summary>
	[Obsolete("Deprecated in favour of Winzor controls.")]
	public partial class DiagramAreaUserControl
	{
		//the parameterless constructor is used in XAML
		public DiagramAreaUserControl()
		{
			Initialize();
		}

#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public DiagramAreaUserControl(NodeViewModelProvider viewModelProvider, NetworkUserControl parentNetworkControl, bool isNonScheduled)
		{
			IsNonScheduled = isNonScheduled;
			this.viewModelProvider = viewModelProvider;
			this.parentNetworkControl = parentNetworkControl;
			Initialize();
		}
#pragma warning restore CS0618 // Disable the warning for obsolete usage

		readonly NodeViewModelProvider viewModelProvider;
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		readonly NetworkUserControl parentNetworkControl;
#pragma warning restore CS0618 // Disable the warning for obsolete usage

		public bool IsNonScheduled { get; }

		void Initialize()
		{
			InitializeComponent();
			Loaded += XamlTranslator.GetControlLoadedEvent<DiagramAreaUserControl>();
			ScaleGrid.GetHeaderHeight = () => GetCanvasOffsetForSize().Y;
			NetworkControl.SelectionChanged += NetworkViewOnSelectionChanged;
		}

		internal void SetDataContext(DiagramAreaUserControlViewModel viewModel)
		{
			Argument.NotNull(viewModel, nameof(viewModel));
			Argument.NotNull(viewModel.NetworkControlViewModel, nameof(viewModel.NetworkControlViewModel));

			if (DataContext != null && DataContext is DiagramAreaUserControlViewModel previousContext)
			{
				viewModel.ContentOffsetX = previousContext.ContentOffsetX;
			}

			DataContext = viewModel;
			RefreshScale();
			AdjustViewWidthAndHeight();
			CheckBranchAndDepartment(viewModel);
		}

		void CheckBranchAndDepartment(DiagramAreaUserControlViewModel viewModel)
		{
			var network = viewModel.NetworkViewModel.DiagramNodeViewModel.Network;

			if (!network.DiagramEntity.IsDiagramScaled ||
				network.ScaleDescriptor == null ||
				!network.ScaleDescriptor.IsBranchOrDepartmentInvalidated)
			{
				return;
			}

			var msgTitle = Res.GetString("ABCACD43-033E-43B9-883C-7F358647A2F9", "Diagram needs Branch and Department");
			var msg = Res.GetString("BEC88168-7E06-47E4-940A-A97B048DC070", "In order to calculate values, this diagram requires a Branch and Department. Please select the missing Branch and Department values.");

			Globals.Message.ShowError(msg, msgTitle);

			var action = new EditPropertiesAction(viewModel.NetworkViewModel);
			ExecuteContextMenuAction(action);
		}

		#region Selection Changed Event

		void NetworkViewOnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsSelectionChangedBehaviourSuspended)
			{
				parentNetworkControl.ClearSelectedNodesOnDiagramControlsExcept(this);
			}
		}

		internal IDisposable SuspendClearingNodesOnOtherControlsWhenSelectionChanges()
		{
			return new SelectionChangedBehaviourSuspensionDisposable(this);
		}

		class SelectionChangedBehaviourSuspensionDisposable : IDisposable
		{
			readonly DiagramAreaUserControl control;

			public SelectionChangedBehaviourSuspensionDisposable(DiagramAreaUserControl control)
			{
				this.control = control;
				control.selectionChangedBehaviourSuspensionCounter++;
			}

			public void Dispose()
			{
				control.selectionChangedBehaviourSuspensionCounter--;
			}
		}

		int selectionChangedBehaviourSuspensionCounter;

		bool IsSelectionChangedBehaviourSuspended => selectionChangedBehaviourSuspensionCounter > 0;

		#endregion

		internal void RefreshScale()
		{
			var grid = (DynamicGrid)FindName("ScaleGrid");

			if (grid != null && grid.Visibility == Visibility.Visible)
			{
				grid.RefreshScale();
			}
		}

		internal void PopulateBackgrounds()
		{
			var grid = (DynamicGrid)FindName("ScaleGrid");

			if (grid != null && grid.Visibility == Visibility.Visible)
			{
				grid.PopulateBackgroundsIfNeeded();
			}
		}

		void ScrollToFirstOpenShape(ScrollViewer scroll)
		{
			var earliestNodes = NetworkViewModel.Nodes.Where(x => !x.Status.IsClosed() && !(x is AnnotationViewModel)).Select(y => y.X).ToArray();

			if (earliestNodes.Any())
			{
				var earliestNode = earliestNodes.Min();
				scroll.ScrollToHorizontalOffset(earliestNode - ScaleBarOffset);
			}
		}

		void ScrollToCurrentColumn(ScrollViewer scroll, DynamicGrid grid)
		{
			scroll.ScrollToHorizontalOffset(((grid.PresentColumnIndex) * ViewModel.NetworkControlViewModel.DiagramEntity.ScaleUnitPixelSize) - ScaleBarOffset);
		}

		double ScaleBarOffset => 3 * ViewModel.NetworkControlViewModel.DiagramEntity.ScaleUnitPixelSize;

		public void ScrollToShape()
		{
			var grid = (DynamicGrid)FindName("ScaleGrid");

			if (grid != null)
			{
				var scroll = ZoomAndPanControl.FindParent<ScrollViewer>();
				if (scroll != null && ViewModel.NetworkControlViewModel != null)
				{
					if (ViewModel.NetworkControlViewModel.DiagramEntity.ScrollPositions != ScrollPositionStates.Start)
					{
						if (ViewModel.NetworkControlViewModel.DiagramEntity.ScrollPositions == ScrollPositionStates.FirstOpenShape)
						{
							ScrollToFirstOpenShape(scroll);
						}
						else if (ViewModel.NetworkControlViewModel.DiagramEntity.ScrollPositions == ScrollPositionStates.Current)
						{
							ScrollToCurrentColumn(scroll, grid);
						}
					}
				}
			}
		}

		public DiagramAreaUserControlViewModel ViewModel => (DiagramAreaUserControlViewModel)DataContext;

		public NetworkViewModel NetworkViewModel => ViewModel?.NetworkControlViewModel?.NetworkViewModel;

		/// <summary>
		/// Event raised when the user has started to drag out a connection.
		/// </summary>
		void NetworkControl_ConnectionDragStarted(object sender, ConnectionDragStartedEventArgs e)
		{
			var draggedOutConnector = (ConnectorViewModel)e.ConnectorDraggedOut;
			var curDragPoint = MousePosition;

			//
			// Delegate the real work to the view model.
			//
			var connection = ViewModel.ConnectionDragStarted(draggedOutConnector, curDragPoint);

			//
			// Must return the view-model object that represents the connection via the event args.
			// This is so that NetworkView can keep track of the object while it is being dragged.
			//
			e.Connection = connection;
		}

		/// <summary>
		/// Event raised, to query for feedback, while the user is dragging a connection.
		/// </summary>
		void NetworkControl_QueryConnectionFeedback(object sender, QueryConnectionFeedbackEventArgs e)
		{
			var draggedOutConnector = (ConnectorViewModel)e.ConnectorDraggedOut;
			var draggedOverConnector = (ConnectorViewModel)e.DraggedOverConnector;

			ViewModel.QueryConnnectionFeedback(draggedOutConnector, draggedOverConnector, out object feedbackIndicator, out bool connectionOk);

			//
			// Return the feedback object to NetworkView.
			// The object combined with the data-template for it will be used to create a 'feedback icon' to
			// display (in an adorner) to the user.
			//
			e.FeedbackIndicator = feedbackIndicator;

			//
			// Let NetworkView know if the connection is ok or not ok.
			//
			e.ConnectionOk = connectionOk;
		}

		/// <summary>
		/// Event raised while the user is dragging a connection.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Exception Message")]
		void NetworkControl_ConnectionDragging(object sender, ConnectionDraggingEventArgs e)
		{
			NetworkVisualisationErrorReporter.ReportIfAssertionFailed(e != null, "Null connection dragging event args");
			var connection = (ConnectionViewModel)e.Connection;
			NetworkVisualisationErrorReporter.ReportIfAssertionFailed(ViewModel != null, "ViewModel is null. DataContext was not set.");
			ViewModel.ConnectionDragging(MousePosition, connection);
		}

		/// <summary>
		/// Event raised when the user has finished dragging out a connection.
		/// </summary>
		void NetworkControl_ConnectionDragCompleted(object sender, ConnectionDragCompletedEventArgs e)
		{
			var connectorDraggedOut = (ConnectorViewModel)e.ConnectorDraggedOut;
			var connectorDraggedOver = (ConnectorViewModel)e.ConnectorDraggedOver;
			var newConnection = (ConnectionViewModel)e.Connection;
			ViewModel.ConnectionDragCompleted(newConnection, connectorDraggedOut, connectorDraggedOver);
		}

		/// <summary>
		/// Event raised to delete the selected node.
		/// </summary>
		void DeleteSelectedNodes_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ViewModel.DeleteSelectedNodes();
		}

		/// <summary>
		/// Event raised to delete a node.
		/// </summary>
		void DefaultDeleteOperation_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var node = (NodeViewModel)e.Parameter;
			if (node != null)
			{
				if (node.CanHide)
				{
					ViewModel.HideNode(node);
				}
				else if (node.CanDelete)
				{
					ViewModel.DeleteNode(node);
				}
			}
		}

		/// <summary>
		/// Event raised to delete a connection.
		/// </summary>
		void DeleteConnection_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var connection = (ConnectionViewModel)e.Parameter;
			ViewModel.DeleteConnection(connection);
		}

		void HideConnection_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var connection = (ConnectionViewModel)e.Parameter;
			ViewModel.HideConnection(connection);
		}

		void OpenRootMenu_Executed(object sender, ContextMenuEventArgs e)
		{
			OnDiagramContextMenuOpening();
		}

		void OpenMenu_Executed(object sender, ContextMenuEventArgs e)
		{
			var contentControl = e?.Source as ContentControl;
			var source = contentControl?.DataContext as NodeViewModel;
			OnNodeContextMenuOpening(source);
		}

		void CloseMenu_Executed(object sender, ContextMenuEventArgs e)
		{
			CloseContextMenu();
		}

#if DEBUG
		public
#endif
		void OnDiagramContextMenuOpening()
		{
			if (NetworkViewModel.RightClickedNode == null) // to suppress bubbling the event from a shape to the root diagram
			{
				mousePositionOnMenuOpened = MousePosition;
				NetworkViewModel.RightClickedNode = NetworkViewModel.DiagramNodeViewModel;

				ViewModel?.NetworkControlViewModel.ReloadMenuItems();
			}
		}

#if DEBUG
		public
#endif
		void OnNodeContextMenuOpening(NodeViewModel node)
		{
			mousePositionOnMenuOpened = MousePosition;
			NetworkViewModel.RightClickedNode = node;

			node?.ReloadMenuItems();
		}

#if DEBUG
		public
#endif
		void CloseContextMenu()
		{
			NetworkViewModel.RightClickedNode = null;
		}

		public IEnumerable<INetworkEntity> SelectedEntities
		{
			get { return NetworkViewModel.SelectedNodes.Cast<NodeViewModel>().Select(n => n.Entity).ToArray(); }
		}

		public void SelectEntity(INetworkEntity entity, bool animated = false)
		{
			var provider = viewModelProvider ?? new NodeViewModelProvider();
			var selectedNode = provider.Create(entity, NetworkViewModel);
			NetworkControl.SelectedNode = selectedNode;

			ScrollToEntity(entity, animated);
		}

		public void ScrollToEntity(INetworkEntity entity, bool animated = false)
		{
			if (animated)
			{
				ZoomAndPanControl.AnimatedSnapTo(new Point(entity.X, entity.Y));
			}
			else
			{
				ZoomAndPanControl.SnapTo(new Point(entity.X, entity.Y));
			}
		}

		Point mousePositionOnMenuOpened;

		void PerformGenericAction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteContextMenuAction((NetworkActionMenuItem)e.Parameter);
		}

#if DEBUG
		public
#endif
		void ExecuteContextMenuAction(NetworkActionMenuItem menuItem)
		{
			ExecuteContextMenuAction(menuItem?.Action);
		}

		public void ExecuteContextMenuAction(INetworkAction action)
		{
			if (action is PositionalNetworkAction positionAction)
			{
				ExecuteGenericPositionalAction(positionAction, new Location(mousePositionOnMenuOpened.X, mousePositionOnMenuOpened.Y));
			}
			else if (action != null)
			{
				ExecuteGenericNonPositionalAction(action, () => new Location(mousePositionOnMenuOpened.X, mousePositionOnMenuOpened.Y), executeForRibbon: false);
			}
		}

		public void ExecuteRibbonAction(INetworkAction action)
		{
			ExecuteGenericNonPositionalAction(action,
				locationGetter: () => NetworkViewModel.Network.EntityPositionStrategy.GetPositionForNewEntities(new Location(ViewModel.LastCreatedNodeLocationRelativeToViewport.X, ViewModel.LastCreatedNodeLocationRelativeToViewport.Y),
					viewport: new ViewportRect(ViewModel.ContentOffsetX, ViewModel.ContentOffsetY, ViewModel.ContentViewportWidth, ViewModel.ContentViewportHeight),
					NetworkViewModel),
				executeForRibbon: true);
		}

		void ExecuteGenericPositionalAction(PositionalNetworkAction action, Location location)
		{
			Dispatcher.BeginInvoke(new Action(() => action.Execute(location)));
		}

		void ExecuteGenericNonPositionalAction(INetworkAction action, Func<Location> locationGetter, bool executeForRibbon)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				INetworkActionResult result;

				using (NetworkViewModel.Network.SuspendRefreshingOnEntityCountChanged())
				{
					result = action.Execute();
				}

				if (result != null && result.IsHandledByVisualiser)
				{
					var entity = result as INetworkEntity;
					var collection = result as INetworkEntityCollection;
					INetworkEntity[] results = null;

					if (entity != null)
					{
						if (IsNonScheduled)
						{
							entity.IsNonScheduled = true;
						}

						ViewModel.CreateNewNode(locationGetter(), entity, disableCentering: executeForRibbon);
						results = new[] { entity };
					}
					else if (collection != null)
					{
						ViewModel.CreateNewNodes(collection.Entities, locationGetter());
						results = collection.Entities.ToArray();
					}

					NetworkViewModel.Network.Refresh(RefreshType.EntitiesReloaded, results);
					AdjustViewWidthAndHeight();
				}
			}));
		}

		public void OnEntityMovedToDiagramSection(INetworkEntity entity)
		{
			var middleOfViewport = GetXCoordinateForMiddleOfViewport();
			entity.X = Math.Max(middleOfViewport - (entity.Width / 2), 0);

			var node = NetworkViewModel.GetNodeForEntity(entity);
			node.OnDragCompleted();
			node.OnResizeCompleted();
		}

		double GetXCoordinateForMiddleOfViewport()
		{
			return (ViewModel.ContentViewportWidth / 2) + ScrollViewer.ContentHorizontalOffset;
		}

		Point MousePosition =>
#if DEBUG
			mousePositionOverride ??
#endif
			Mouse.GetPosition(NetworkControl);

		void NetworkControl_NodeDragStarted(object sender, NodeDragStartedEventArgs e)
		{
			ViewModel.NetworkControlViewModel.SelectRelevantNodes();
		}

		void NetworkControl_NodeResizeCompleted(object sender, NodeResizeCompletedEventArgs e)
		{
			foreach (Control node in e.Nodes)
			{
				var viewModel = node.DataContext as NodeViewModel;
				viewModel?.OnResizeCompleted();
			}

			RefreshScale();
			AdjustViewWidthAndHeight();
		}

		void NetworkControl_NodeDragCompleted(object sender, NodeDragCompletedEventArgs e)
		{
			foreach (NodeViewModel node in e.Nodes)
			{
				node.OnDragCompleted();
			}

			RefreshScale();
			AdjustViewWidthAndHeight();
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			ZoomAndPanControl.SizeChanged += (sender, e) => AdjustViewWidthAndHeight();
			ZoomAndPanControl.ContentScaleChanged += (sender, e) => AdjustViewWidthAndHeight();
		}

		void AdjustViewWidthAndHeight()
		{
			var viewModel = ViewModel;

			if (viewModel.NetworkControlViewModel != null)
			{
				var cornerRadius = viewModel.NetworkControlViewModel.DiagramCornerRadius;
				var currentViewPortWidth = ZoomAndPanControl.ContentViewportWidth - cornerRadius;

				var offset = GetCanvasOffsetForSize();
				var diagramEntity = viewModel.NetworkControlViewModel.DiagramEntity;

				var isDiagramSurfaceFixed = diagramEntity.IsDiagramSurfaceFixed;

				if (AdjustEntitiesLocationToChannelBounds(diagramEntity) || (isDiagramSurfaceFixed && AdjustEntitiesLocationToDiagramBounds(diagramEntity)))
				{
					parentNetworkControl.ReloadNetwork();
				}

				var nodes = NetworkViewModel.GetAppropriateNodeCollection(IsNonScheduled);
				var widthForContent = nodes.Select(a => a.X > 0 ? a.X + a.Width + offset.X : 0).Concat(new double[] { currentViewPortWidth }).Max();
				var widthForChannelHeaders = diagramEntity.DiagramChannels.Any() ? diagramEntity.ScaleUnitPixelSize : 0;

				viewModel.ContentWidth = widthForContent + widthForChannelHeaders;

				if (isDiagramSurfaceFixed && !diagramEntity.IsDeleted)
				{
					var diagramWidth = diagramEntity.Width + offset.X + widthForChannelHeaders;

					if (viewModel.ContentWidth != diagramWidth)
					{
						viewModel.ContentWidth = diagramWidth;
					}
				}

				parentNetworkControl.AdjustDiagramControlHeights();

				ContentStackPanel.Margin = new Thickness(viewModel.ContentLeftMargin, 0, 0, 0);
			}
		}

		internal double GetHeightForContent()
		{
			var offset = GetCanvasOffsetForSize();
			var diagramEntity = ViewModel.NetworkControlViewModel.DiagramEntity;
			var cornerRadius = ViewModel.NetworkControlViewModel.DiagramCornerRadius;
			var currentViewPortHeight = ZoomAndPanControl.ContentViewportHeight - cornerRadius;
			var heightForChannels = offset.Y + NetworkViewModel.GetChannelYAxisRanges().LastOrDefault()?.Item2.Maximum ?? 0;

			double heightForContent;

			if (diagramEntity.IsDiagramSurfaceFixed && diagramEntity.Height > 0d)
			{
				heightForContent = diagramEntity.Height + offset.Y;
			}
			else
			{
				var nodes = NetworkViewModel.GetAppropriateNodeCollection(IsNonScheduled);
				heightForContent = nodes.Select(a => a.Y > 0 ? a.Y + a.Height + offset.Y : 0).Concat(new[] { currentViewPortHeight }).Max();
			}

			return Math.Max(heightForContent, heightForChannels);
		}

		internal void AdjustContentHeight(double height)
		{
			ViewModel.ContentHeight = height;
			ZoomAndPanControl.UpdateContentViewportSize();
		}

		static bool AdjustEntitiesLocationToDiagramBounds(IDiagramEntity diagramEntity, bool useOffset = false)
		{
			var lastShapes = diagramEntity.Children.OrderByDescending(node => node.X).Where(shape => shape.X + shape.Width > diagramEntity.Width);
			var result = false;

			foreach (var shape in lastShapes)
			{
				result |= shape.AdjustEntityLocationToDiagramBounds(diagramEntity, useOffset);

				if (shape.Children.Any())
				{
					AdjustEntitiesLocationToDiagramBounds(shape as IDiagramEntity, true);
				}
			}
			return result;
		}

		bool AdjustEntitiesLocationToChannelBounds(IDiagramEntity diagramEntity)
		{
			if (!diagramEntity.DiagramChannels.Any())
			{
				return false;
			}

			var shapes = diagramEntity.Children.OrderByDescending(node => node.X);
			var result = false;

			foreach (var shape in shapes)
			{
				if (shape.AdjustLocationToChannels(diagramEntity, NetworkViewModel))
				{
					result = true;
				}
			}

			return result;
		}

		Vector GetCanvasOffsetForSize()
		{
			var offset = GetOffsetFromAncestor(ZoomAndPanControl, NetworkControl);
			const int bottomMarginOffset = Constants.DynamicGridBottomMargin * 2;

			return new Vector(offset.X, offset.Y + bottomMarginOffset);
		}

		static Vector GetOffsetFromAncestor(UIElement ancestor, UIElement descendant)
		{
			var xOffset = 0d;
			var yOffset = 0d;

			var current = descendant;

			while (current != ancestor)
			{
				if (current == null)
				{
					return new Vector(0, 0);
				}

				var offset = VisualTreeHelper.GetOffset(current);
				current = VisualTreeHelper.GetParent(current) as UIElement;
				xOffset += offset.X;
				yOffset += offset.Y;
			}

			return new Vector(xOffset, yOffset);
		}

		void ResizeThumb_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ToggleContinuousChangeOnSender(sender, true);
		}

		void ResizeThumb_MouseUp(object sender, MouseButtonEventArgs e)
		{
			ToggleContinuousChangeOnSender(sender, false);
		}

		static void ToggleContinuousChangeOnSender(object sender, bool isContinuouslyChanging)
		{
			var resizeThumb = sender as ResizeThumb;

			if (resizeThumb?.DataContext is NodeViewModel node)
			{
				node.IsContinuouslyChanging = isContinuouslyChanging;

				if (isContinuouslyChanging)
				{
					if (resizeThumb.HorizontalAlignment == HorizontalAlignment.Left)
					{
						node.ReferencePoint = (resizeThumb.VerticalAlignment == VerticalAlignment.Top ? NodeResizeReferencePoint.LeftTop : NodeResizeReferencePoint.LeftBottom);
					}
					else
					{
						node.ReferencePoint = (resizeThumb.VerticalAlignment == VerticalAlignment.Top ? NodeResizeReferencePoint.RightTop : NodeResizeReferencePoint.RightBottom);
					}
				}
			}
		}

		void PasteExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = NetworkViewModel.TryHandlePaste();
		}

		public void CtrlCIsPressed()
		{
			var selectedNodes = NetworkViewModel.SelectedNodes.Select(x => x.Entity);
			if (selectedNodes.Any())
			{
				SafeClipboard.Clear();
				NetworkViewModel.Network.TryCopyShapeStateToClipBoard(selectedNodes);
			}
		}

		public void CtrlVIsPressed()
		{
			if (!NetworkViewModel.SelectedEntities.Any())
			{
				NetworkViewModel.Network.PasteShapeFromClipBoard(NetworkViewModel);
			}
		}

		void ToolTip_RefreshBinding(object sender, ToolTipEventArgs e)
		{
			var depObj = sender as DependencyObject;

			if (depObj != null)
			{
				var bindingExpression = BindingOperations.GetBindingExpression(depObj, FrameworkElement.ToolTipProperty);
				bindingExpression?.UpdateTarget();
			}
		}

		void EntityDetails_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.HeightChanged)
			{
				AdjustViewWidthAndHeight();
				RefreshScale();
			}
		}

		#region Zoom and Pan

		/// <summary>
		/// Specifies the current state of the mouse handling logic.
		/// </summary>
		MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;

		/// <summary>
		/// The point that was clicked relative to the ZoomAndPanControl.
		/// </summary>
		Point origZoomAndPanControlMouseDownPoint;

		/// <summary>
		/// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
		/// </summary>
		Point origContentMouseDownPoint;

		/// <summary>
		/// Records which mouse button clicked during mouse dragging.
		/// </summary>
		MouseButton mouseButtonDown;

		/// <summary>
		/// Saves the previous zoom rectangle, pressing the backspace key jumps back to this zoom rectangle.
		/// </summary>
		Rect prevZoomRect;

		/// <summary>
		/// Save the previous content scale, pressing the backspace key jumps back to this scale.
		/// </summary>
		double prevZoomScale;

		#region Mouse Events

		/// <summary>
		/// Event raised on mouse down in the NetworkView.
		/// </summary> 
		void NetworkControl_MouseDown(object sender, MouseButtonEventArgs e)
		{
			NetworkControl.Focus();
			Keyboard.Focus(NetworkControl);

			mouseButtonDown = e.ChangedButton;
			origZoomAndPanControlMouseDownPoint = e.GetPosition(ZoomAndPanControl);
			origContentMouseDownPoint = e.GetPosition(NetworkControl);

			if (mouseHandlingMode == MouseHandlingMode.None)
			{
				if (mouseButtonDown == MouseButton.Middle)
				{
					mouseHandlingMode = MouseHandlingMode.Panning;
				}
				else
				{
					switch (Keyboard.Modifiers)
					{
						case ModifierKeys.None:
						case ModifierKeys.Control:
							if (mouseButtonDown == MouseButton.Left)
							{
								mouseHandlingMode = MouseHandlingMode.Selecting;
								NetworkControl.StartSelection(this.DragSelection,
									retainExisting: Keyboard.Modifiers == ModifierKeys.Control);
							}

							break;
						case ModifierKeys.Alt:
							if (mouseButtonDown == MouseButton.Left)
							{
								mouseHandlingMode = MouseHandlingMode.Panning;
							}

							break;
						case ModifierKeys.Shift:
							if (mouseButtonDown == MouseButton.Left || mouseButtonDown == MouseButton.Right)
							{
								mouseHandlingMode = MouseHandlingMode.Zooming;
							}

							break;
					}
				}
			}

			// Capture the mouse so that we eventually receive the mouse up event.
			NetworkControl.CaptureMouse();
			e.Handled = true;
		}

		/// <summary>
		/// Event raised on mouse move in the NetworkView.
		/// </summary>
		void NetworkControl_MouseMove(object sender, MouseEventArgs e)
		{
			if (mouseHandlingMode == MouseHandlingMode.Panning)
			{
				var curZoomAndPanControlMousePoint = e.GetPosition(ZoomAndPanControl);
				var dragOffset = curZoomAndPanControlMousePoint - origZoomAndPanControlMouseDownPoint;
				const double dragThreshold = 10d;

				if (Math.Abs(dragOffset.X) > dragThreshold || Math.Abs(dragOffset.Y) > dragThreshold)
				{
					//
					// The user has dragged the cursor further than the threshold distance, initiate
					// drag panning.
					//
					mouseHandlingMode = MouseHandlingMode.DragPanning;
					NetworkControl.IsClearSelectionOnEmptySpaceClickEnabled = false;
					Mouse.OverrideCursor = Cursors.ScrollAll;
				}

				e.Handled = true;
			}
			else if (mouseHandlingMode == MouseHandlingMode.DragPanning)
			{
				//
				// The user is left-dragging the mouse.
				// Pan the viewport by the appropriate amount.
				//
				var curContentMousePoint = e.GetPosition(NetworkControl);
				var dragOffset = curContentMousePoint - origContentMouseDownPoint;

				ZoomAndPanControl.ContentOffsetX -= dragOffset.X;
				ZoomAndPanControl.ContentOffsetY -= dragOffset.Y;

				e.Handled = true;
			}
			else if (mouseHandlingMode == MouseHandlingMode.Zooming)
			{
				var curZoomAndPanControlMousePoint = e.GetPosition(ZoomAndPanControl);
				var dragOffset = curZoomAndPanControlMousePoint - origZoomAndPanControlMouseDownPoint;
				const double dragThreshold = 10;

				if (mouseButtonDown == MouseButton.Left && (Math.Abs(dragOffset.X) > dragThreshold || Math.Abs(dragOffset.Y) > dragThreshold))
				{
					//
					// When Shift + left-down zooming mode and the user drags beyond the drag threshold,
					// initiate drag zooming mode where the user can drag out a rectangle to select the area
					// to zoom in on.
					//
					mouseHandlingMode = MouseHandlingMode.DragZooming;
					var curContentMousePoint = e.GetPosition(NetworkControl);
					InitDragZoomRect(origContentMouseDownPoint, curContentMousePoint);
				}

				e.Handled = true;
			}
			else if (mouseHandlingMode == MouseHandlingMode.DragZooming)
			{
				//
				// When in drag zooming mode continously update the position of the rectangle
				// that the user is dragging out.
				//
				var curContentMousePoint = e.GetPosition(NetworkControl);
				SetDragZoomRect(origContentMouseDownPoint, curContentMousePoint);

				e.Handled = true;
			}
			else if (mouseHandlingMode == MouseHandlingMode.Selecting)
			{
				UpdateSelection();
				e.Handled = true;
			}
		}

		void UpdateSelection()
		{
			NetworkControl.UpdateSelection();
			parentNetworkControl.ClearSelectedNodesOnDiagramControlsExcept(this);
		}

		/// <summary>
		/// Event raised on mouse up in the NetworkView.
		/// </summary>
		void NetworkControl_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (mouseHandlingMode != MouseHandlingMode.None)
			{
				if (mouseHandlingMode == MouseHandlingMode.Panning)
				{
					//
					// Panning was initiated but dragging was abandoned before the mouse
					// cursor was dragged further than the threshold distance.
					// This means that this basically just a regular left mouse click.
					// Because it was a mouse click in empty space we need to clear the current selection.
					//
				}
				else if (mouseHandlingMode == MouseHandlingMode.Zooming)
				{
					if (mouseButtonDown == MouseButton.Left)
					{
						// Shift + left-click zooms in on the content.
						ZoomIn(origContentMouseDownPoint);
					}
					else if (mouseButtonDown == MouseButton.Right)
					{
						// Shift + left-click zooms out from the content.
						ZoomOut(origContentMouseDownPoint);
					}
				}
				else if (mouseHandlingMode == MouseHandlingMode.DragZooming)
				{
					// When drag-zooming has finished we zoom in on the rectangle that was highlighted by the user.
					ApplyDragZoomRect();
				}
				else if (mouseHandlingMode == MouseHandlingMode.Selecting)
				{
					NetworkControl.EndSelection();
					NetworkControl.IsClearSelectionOnEmptySpaceClickEnabled = false;
				}
				//
				// Reset the override cursor.
				// This is set to a special cursor while drag panning is in progress.
				//
				Mouse.OverrideCursor = null;
				NetworkControl.ReleaseMouseCapture();
				mouseHandlingMode = MouseHandlingMode.None;
				e.Handled = true;
			}

			if (NetworkControl.IsClearSelectionOnEmptySpaceClickEnabled)
			{
				NetworkControl.SelectedNodes.Clear();
				parentNetworkControl.ClearSelectedNodesOnDiagramControlsExcept(this);
			}
			else
			{
				// Reenable clearing of selection when empty space is clicked.
				// This is disabled when drag panning or selection is in progress.
				//
				NetworkControl.IsClearSelectionOnEmptySpaceClickEnabled = true;
			}
		}

		/// <summary>
		/// Event raised by rotating the mouse wheel.
		/// </summary>
		void NetworkControl_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			var viewer = ((DependencyObject)sender).FindParent<ScrollViewer>();

			if (viewer != null)
			{
				if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
#if DEBUG
					|| IsCtrlDown_ForTest
#endif
					)
				{
					var curContentMousePoint = e.GetPosition(ZoomAndPanControl);
					if (e.Delta > 1)
					{
						ZoomIn(curContentMousePoint);
					}
					else if (e.Delta < -1)
					{
						ZoomOut(curContentMousePoint);
					}
				}
				else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				{
					viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset - e.Delta);
				}
				else
				{
					viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
				}

				e.Handled = true;
			}
		}

		/// <summary>
		/// Event raised when the user has double clicked in the zoom and pan control.
		/// </summary>
		void NetworkControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var network = NetworkViewModel.Network;
			var selectedEntity = NetworkViewModel.FirstSelectedEntity;

			if (selectedEntity != null && !ControlsToExclude.Contains(e.OriginalSource.GetType().FullName))
			{
				network.ViewEntity(selectedEntity);
			}

			e.Handled = true;
		}

		#endregion

		System.Collections.ArrayList ControlsToExclude => controlsToExclude ?? (controlsToExclude = new System.Collections.ArrayList(new[] { "System.Windows.Controls.TextBoxView", "CargoWise.NetworkVisualisation.GUI.TextBoxWithPlaceholder" }));

		public bool IsPreviousZoomRectangleSet { get; set; }

		System.Collections.ArrayList controlsToExclude;

		public void ZoomIn()
		{
			ZoomIn(new Point(ZoomAndPanControl.ContentZoomFocusX, ZoomAndPanControl.ContentZoomFocusY));
		}

		public void ZoomOut()
		{
			ZoomOut(new Point(ZoomAndPanControl.ContentZoomFocusX, ZoomAndPanControl.ContentZoomFocusY));
		}

		internal void FitNodes()
		{
			if (NetworkControl.SelectedNodes.Count > 0)
			{
				FitSelectedNodes();
			}
			else
			{
				FitAllNodes();
			}
		}

		void FitSelectedNodes()
		{
			ZoomToNodes(NetworkControl.SelectedNodes);
		}

		void FitAllNodes()
		{
			ZoomToNodes(NetworkViewModel.GetAppropriateNodeCollection(IsNonScheduled));
		}

		void ZoomToNodes(System.Collections.IList nodes)
		{
			if (nodes.Count == 0)
			{
				return;
			}

			SavePrevZoomRect();
			var actualContentRect = DetermineAreaOfNodes(nodes);

			//
			// Inflate the content rect by a fraction of the actual size of the total content area.
			// This puts a nice border around the content we are fitting to the viewport.
			//
			actualContentRect.Inflate(NetworkControl.ActualWidth / 40, NetworkControl.ActualHeight / 40);
			ZoomAndPanControl.AnimatedZoomTo(actualContentRect);
		}

		/// <summary>
		/// Determine the area covered by the specified list of nodes.
		/// </summary>
		static Rect DetermineAreaOfNodes(System.Collections.IList nodes)
		{
			var firstNode = (NodeViewModel)nodes[0];
			var actualContentRect = new Rect(firstNode.X, firstNode.Y, firstNode.Width, firstNode.Height);

			for (var i = 1; i < nodes.Count; ++i)
			{
				var node = (NodeViewModel)nodes[i];
				var nodeRect = new Rect(node.X, node.Y, node.Width, node.Height);
				actualContentRect = Rect.Union(actualContentRect, nodeRect);
			}

			return actualContentRect;
		}

		internal void Fill()
		{
			SavePrevZoomRect();
			ZoomAndPanControl.AnimatedScaleToFit();
		}

		internal void OneHundredPercent()
		{
			SavePrevZoomRect();
			ZoomAndPanControl.AnimatedZoomTo(1.0);
		}

		/// <summary>
		/// Jump back to the previous zoom level.
		/// </summary>
		internal void JumpBackToPrevZoom()
		{
			ZoomAndPanControl.AnimatedZoomTo(prevZoomScale, prevZoomRect);
			ClearPrevZoomRect();
		}

		/// <summary>
		/// Zoom the viewport out, centering on the specified point (in content coordinates).
		/// </summary>
		void ZoomOut(Point contentZoomCenter)
		{
			Argument.NotNull(ViewModel, nameof(ViewModel));

			if (ViewModel.NetworkControlViewModel == null) // that's the case before setting data context
			{
				return;
			}
			Argument.NotNull(ZoomAndPanControl, nameof(ZoomAndPanControl));

			if (ViewModel.NetworkControlViewModel.CanZoomOut(ZoomAndPanControl.ContentScale))
			{
				ZoomAndPanControl.ZoomAboutPoint(ZoomAndPanControl.ContentScale - 0.1, contentZoomCenter);
			}
		}

		/// <summary>
		/// Zoom the viewport in, centering on the specified point (in content coordinates).
		/// </summary>
		void ZoomIn(Point contentZoomCenter)
		{
			ZoomAndPanControl.ZoomAboutPoint(ZoomAndPanControl.ContentScale + 0.1, contentZoomCenter);
			RefreshScrollViewer();
		}

		void RefreshScrollViewer()
		{
			ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset);
			ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset);
			ScrollViewerRefreshed?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler ScrollViewerRefreshed;

		/// <summary>
		/// Initialize the rectangle that the use is dragging out.
		/// </summary>
		void InitDragZoomRect(Point pt1, Point pt2)
		{
			SetDragZoomRect(pt1, pt2);

			DragZoomCanvas.Visibility = Visibility.Visible;
			DragZoomBorder.Opacity = 0.5;
		}

		/// <summary>
		/// Update the position and size of the rectangle that user is dragging out.
		/// </summary>
		void SetDragZoomRect(Point pt1, Point pt2)
		{
			double x, y, width, height;

			//
			// Deterine x,y,width and height of the rect inverting the points if necessary.
			// 

			if (pt2.X < pt1.X)
			{
				x = pt2.X;
				width = pt1.X - pt2.X;
			}
			else
			{
				x = pt1.X;
				width = pt2.X - pt1.X;
			}

			if (pt2.Y < pt1.Y)
			{
				y = pt2.Y;
				height = pt1.Y - pt2.Y;
			}
			else
			{
				y = pt1.Y;
				height = pt2.Y - pt1.Y;
			}

			//
			// Update the coordinates of the rectangle that is being dragged out by the user.
			// The we offset and rescale to convert from content coordinates.
			//
			Canvas.SetLeft(DragZoomBorder, x);
			Canvas.SetTop(DragZoomBorder, y);
			DragZoomBorder.Width = width;
			DragZoomBorder.Height = height;
		}

		/// <summary>
		/// When the user has finished dragging out the rectangle the zoom operation is applied.
		/// </summary>
		void ApplyDragZoomRect()
		{
			//
			// Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
			//
			SavePrevZoomRect();

			//
			// Retreive the rectangle that the user draggged out and zoom in on it.
			//
			var contentX = Canvas.GetLeft(DragZoomBorder);
			var contentY = Canvas.GetTop(DragZoomBorder);
			var contentWidth = DragZoomBorder.Width;
			var contentHeight = DragZoomBorder.Height;
			ZoomAndPanControl.AnimatedZoomTo(new Rect(contentX, contentY, contentWidth, contentHeight));

			FadeOutDragZoomRect();
		}

		//
		// Fade out the drag zoom rectangle.
		//
		void FadeOutDragZoomRect()
		{
			AnimationHelper.StartAnimation(DragZoomBorder, UIElement.OpacityProperty, 0.0, 0.1, (sender, e) =>
			{
				DragZoomCanvas.Visibility = Visibility.Collapsed;
			});
		}

		//
		// Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
		//
		void SavePrevZoomRect()
		{
			prevZoomRect = new Rect(ZoomAndPanControl.ContentOffsetX, ZoomAndPanControl.ContentOffsetY, ZoomAndPanControl.ContentViewportWidth, ZoomAndPanControl.ContentViewportHeight);
			prevZoomScale = ZoomAndPanControl.ContentScale;
			IsPreviousZoomRectangleSet = true;
		}

		/// <summary>
		/// Clear the memory of the previous zoom level.
		/// </summary>
		void ClearPrevZoomRect()
		{
			IsPreviousZoomRectangleSet = false;
		}

		#endregion

		#region For Test
#if DEBUG
		public bool IsCtrlDown_ForTest { get; set; }

		public double ContentScale => ZoomAndPanControl.ContentScale;

		public void UpdateSelection_ForTest()
		{
			UpdateSelection();
		}

		Point? mousePositionOverride;

		public void SetMousePosition_ForTest(Point position)
		{
			mousePositionOverride = position;
		}
#endif
		#endregion

		void DockPanel_KeyDown(object sender, KeyEventArgs e)
		{
			var ctrlIsPressedOnKeyBoard = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
			if (ctrlIsPressedOnKeyBoard)
			{
				switch (e.Key)
				{
					case Key.C:
						CtrlCIsPressed();
						break;
					case Key.V:
						CtrlVIsPressed();
						break;
					default:
						break;
				}
			}
		}
	}
}
