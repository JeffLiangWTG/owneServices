using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Threading;
using CargoWise.Common;
using CargoWise.Main.Navigation.WPF;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// This is a Window that uses NetworkView to display a flow-chart.
	/// </summary>
	[Obsolete("Deprecated in favour of Winzor controls.")]
	public partial class NetworkUserControl : IDisposable
	{
		#region Construction, Setup, and Initialization

		//the parameterless constructor is used in XAML
		public NetworkUserControl()
		{
			IsEnabled = false;
			Initialize();
		}

		public NetworkUserControl(IDiagramEntity diagramEntity, INetworkRefresher networkRefresher, DataTemplateSelector selector = null, NodeViewModelProvider viewModelProvider = null, IRibbonDataProvider ribbonDataProvider = null)
		{
			IsEnabled = false;
			this.networkRefresher = networkRefresher;
			networkRefresher.Refreshed += Network_Refreshed;
			this.selector = selector;
			this.viewModelProvider = viewModelProvider;
			this.ribbonDataProvider = ribbonDataProvider;
			resourcesProvider = new NetworkRibbonResourcesProvider();

			MainDiagramControl = new DiagramAreaUserControl(viewModelProvider, this, isNonScheduled: false);

			if (diagramEntity != null && diagramEntity.ShouldShowNonScheduledSection)
			{
				NonScheduledDiagramControl = new DiagramAreaUserControl(viewModelProvider, this, isNonScheduled: true);
			}

			Initialize();
		}

		void Initialize()
		{
			InitializeComponent();
			Loaded += XamlTranslator.GetControlLoadedEvent<NetworkUserControl>();
		}

		public void SetDataContext(INetwork network, bool isReloading)
		{
			UnsubscribeFromNetworkViewModelEvents();

			var previousPortal = DataContext is NetworkUserControlViewModel dataContext ? new { previousScale = dataContext.ContentScale, previousY = dataContext.ContentOffsetY } : null;
			var previouslySelectedRibbonTabIndex = RibbonControl.SelectedTabIndex;

			NetworkViewModel?.Deactivate();
			dataContext = new NetworkUserControlViewModel(network, selector, viewModelProvider, isReloading, ribbonDataProvider, resourcesProvider, this, network.DiagramEntity.ShapeInspectorVisible);
			dataContext.SetupNetworkActions(this);
			RibbonControl.AttachViewModel(dataContext.RibbonViewModel);
			RibbonControl.SelectTab(previouslySelectedRibbonTabIndex);

			if (previousPortal != null)
			{
				dataContext.ContentScale = previousPortal.previousScale;
				dataContext.ContentOffsetY = previousPortal.previousY;
			}

			DataContext = dataContext;

			SetUpDiagramAreaControls();
			SubscribeToNetworkViewModelEvents();
		}

		#endregion

		#region Diagram Area Controls

		void SetUpDiagramAreaControls()
		{
			Argument.NotNull(ViewModel, nameof(ViewModel));

			var diagramEntity = ViewModel.DiagramEntity;

			if (diagramEntity.ShouldShowNonScheduledSection)
			{
				SliderColumn.Width = new GridLength(Constants.SectionSliderWidth);
				NonScheduledColumn.MinWidth = Constants.NonScheduledSectionMinWidth;
				SetMaxWidthForSectionsWhenShowingNonScheduled();
				SetNonScheduledSectionWidthWithoutUpdatingShape(diagramEntity.NonScheduledSectionWidth);

				if (NonScheduledDiagramControl == null)
				{
					NonScheduledDiagramControl = new DiagramAreaUserControl(viewModelProvider, this, isNonScheduled: true);
					DiagramAreaControlsGrid.Children.Add(GridSplitter);
					NonScheduledDiagramContentControl.Content = NonScheduledDiagramControl;
				}

				var nonScheduledViewModel = new DiagramAreaUserControlViewModel(ViewModel, isNonScheduled: true);
				nonScheduledViewModel.PropertyChanged += NonScheduledViewModelOnPropertyChanged;
				NonScheduledDiagramControl.SetDataContext(nonScheduledViewModel);
			}
			else
			{
				SliderColumn.Width = new GridLength(0d);
				NonScheduledColumn.MinWidth = 0d;
				ScheduledColumn.MaxWidth = double.PositiveInfinity;
				SetNonScheduledSectionWidth(0d);
				DiagramAreaControlsGrid.Children.Remove(GridSplitter);
				NonScheduledDiagramControl = null;
			}

			MainDiagramControl.SetDataContext(new DiagramAreaUserControlViewModel(ViewModel, isNonScheduled: diagramEntity.IsNonScheduled));

			IsEnabled = true;
		}

		void NonScheduledViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(DiagramAreaUserControlViewModel.ContentViewportWidth) && ViewModel.DiagramEntity.ShouldShowNonScheduledSection)
			{
				UpdateStoredNonScheduledSectionWidthValue(NonScheduledDiagramControl.ActualWidth);
				SetMaxWidthForSectionsWhenShowingNonScheduled();
			}
		}

		internal void SetMaxWidthForSectionsWhenShowingNonScheduled()
		{
			var baseMaxWidth = MainWindow.ActualWidth - Constants.SectionBaseMaxWidthBuffer; // WPF needs this buffer, else if the user adjusts the slider all the way to the MinWidth, scrollbars stop working. SAD!
			SetMaxWidth(ScheduledColumn, baseMaxWidth - Constants.ScheduledSectionExtraMaxWidthBuffer); // again, needed to avoid strange behaviors when adjusting the slider. Don't change any of these values without testing the slider, and resizing the window!
			SetMaxWidth(NonScheduledColumn, baseMaxWidth);
		}

		static void SetMaxWidth(ColumnDefinition column, double width)
		{
			column.MaxWidth = width >= 0 ? width : double.PositiveInfinity;
		}

		public void SetNonScheduledSectionWidth(double width)
		{
			UpdateStoredNonScheduledSectionWidthValue(width);
			SetNonScheduledSectionWidthWithoutUpdatingShape(width);
		}

		void UpdateStoredNonScheduledSectionWidthValue(double width)
		{
			if (isInitialDrawingComplete)
			{
				ViewModel.DiagramEntity.NonScheduledSectionWidth = width;
			}
		}

		void SetNonScheduledSectionWidthWithoutUpdatingShape(double width)
		{
			NonScheduledColumn.Width = new GridLength(width);
		}

		ColumnDefinition ScheduledColumn => DiagramAreaControlsGrid.ColumnDefinitions[0];
		ColumnDefinition SliderColumn => DiagramAreaControlsGrid.ColumnDefinitions[1];
		ColumnDefinition NonScheduledColumn => DiagramAreaControlsGrid.ColumnDefinitions[2];

		internal void SetInitialDrawingComplete()
		{
			isInitialDrawingComplete = true;
		}

		bool isInitialDrawingComplete;

		public DiagramAreaUserControl MainDiagramControl { get; private set; }
		public DiagramAreaUserControl NonScheduledDiagramControl { get; private set; }

		IEnumerable<DiagramAreaUserControl> DiagramAreaControls => new[] { MainDiagramControl, NonScheduledDiagramControl }.WhereNotNull();

		void DoOnDiagramControls(Action<DiagramAreaUserControl> action)
		{
			foreach (var control in DiagramAreaControls)
			{
				action.Invoke(control);
			}
		}

		#endregion

		readonly INetworkRefresher networkRefresher;
		readonly DataTemplateSelector selector;
		readonly NodeViewModelProvider viewModelProvider;
		readonly IRibbonDataProvider ribbonDataProvider;
		readonly NetworkRibbonResourcesProvider resourcesProvider;

		public INetworkEntityController Controller => NetworkViewModel?.Controller;

		#region IDisposable Support

		bool isDisposed;

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "SearchForm")]
		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
					RibbonControl.Model?.DeactivateButtons();
					CloseFinderForm();
				}

				isDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region Ribbon Search

		public void SwitсhFocusToFinder()
		{
			RibbonControl.SearchBox.Focus();
		}

		#endregion

		#region Finder Form

		SearchFinderForm SearchForm;

		public SearchFinderForm OpenFinderForm()
		{
			if (SearchForm == null || SearchForm.IsDisposed)
			{
				SearchForm = new SearchFinderForm(this);
				SearchForm.Show();
			}
			SearchForm.ReFocus();
			return SearchForm;
		}

		public void CloseFinderForm()
		{
			SearchForm?.Close();
			SearchForm = null;
		}

		#endregion

		#region Testing
#if DEBUG
		public NetworkRibbonControl RibbonControlExposed_ForTesting => RibbonControl;

		public TextBox RibbonSearchBoxExposed_ForTesting => RibbonControl.SearchBox;

		public bool RibbonControlIsVisible_ForTesting => RibbonControl.Visibility == Visibility.Visible;

		public AutoResetEvent NetworkUserControlHosted => new AutoResetEvent(false);
#endif
		#endregion

		public event EventHandler<RefreshArgs> SelectionChanged;

		void SubscribeToNetworkViewModelEvents()
		{
			if (NetworkViewModel != null)
			{
				NetworkViewModel.SelectionChanged += NetworkViewModel_SelectionChanged;
				NetworkViewModel.FocusedEntityChanged += NetworkViewModel_FocusedEntityChanged;
			}
		}

		void UnsubscribeFromNetworkViewModelEvents()
		{
			if (NetworkViewModel != null)
			{
				NetworkViewModel.SelectionChanged -= NetworkViewModel_SelectionChanged;
				NetworkViewModel.FocusedEntityChanged -= NetworkViewModel_FocusedEntityChanged;
			}
		}

		void NetworkViewModel_SelectionChanged(object sender, RefreshArgs e)
		{
			ViewModel?.RibbonViewModel?.RefreshActions(e);
			SelectionChanged?.Invoke(this, e);
		}

		void NetworkViewModel_FocusedEntityChanged(object sender, FocusedEntityChangedEventArgs e)
		{
			if (e.FocusedEntity != null)
			{
				ScrollToEntity(e.FocusedEntity);
			}
		}

		internal void AdjustDiagramControlHeights()
		{
			var diagramControls = DiagramAreaControls.Where(x => ((DiagramAreaUserControlViewModel)x.DataContext)?.NetworkViewModel != null).ToArray();
			var maxHeight = diagramControls.Any() ? diagramControls.Max(x => x.GetHeightForContent()) : 0d;

			DoOnDiagramControls(control => control.AdjustContentHeight(maxHeight));
		}

		void Network_Refreshed(object sender, RefreshArgs e)
		{
			switch (e.RefreshType)
			{
				case RefreshType.AffinitiesRefreshRequired:
				case RefreshType.RedrawDiagram:
				case RefreshType.RefreshButton:
				case RefreshType.EntityEdited:
				case RefreshType.EntityRemoved:
				case RefreshType.Saved:
					ReloadNetwork();
					break;

				case RefreshType.Scale:
				case RefreshType.EntitySize:
				case RefreshType.EntityCoordinates:
				case RefreshType.TextColorUpdated:
					RefreshScale();
					break;

				case RefreshType.EntitiesReloaded:
					ViewModel.RefreshNodes();
					break;

				case RefreshType.EntityAdded:
					ViewModel.NetworkViewModel.AddRelationships(e.Entities);
					break;

				case RefreshType.Close:
					CloseAllChildren();
					break;

				case RefreshType.ResourceDependencyAdded:
					DisplayResourceDependencyLink(e.Entities);
					break;

				case RefreshType.EntitiesMovedToDiagramSection:
					ReloadNetwork();
					ViewModel.RefreshNodes();
					OnEntitiesMovedToDiagramSection(e.Entities.ToArray());
					break;

				case RefreshType.RelationshipAdded:
				case RefreshType.RelationshipRemoved:
				case RefreshType.EntityPinnedOrUnpinned:
				case RefreshType.EntityApprovedOrUnapproved:
				case RefreshType.EntityStatusChanged:
				case RefreshType.ConnectionAdded:
				case RefreshType.Saving:
				case RefreshType.ToogleFreezeChannelHeadersAndTimeLabels:
				case RefreshType.ChannelView:
					break;

				case RefreshType.ShapeInspectorVisibilityChanged:
					ViewModel.ShapeInspectorVisible = !ViewModel.ShapeInspectorVisible;
					ViewModel.ShapeInspectorNodeViewModel = ViewModel.NetworkViewModel.GetNodeForEntity(e.Entities.FirstOrDefault());
					break;

				case RefreshType.Affinities:
					PopulateBackgrounds();
					break;

				default:
					throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Refresh Type not implemented: {0}", e.RefreshType.ToString()));
			}
		}

		void OnEntitiesMovedToDiagramSection(INetworkEntity[] entities)
		{
			var areEntitiesMovingToNonScheduledSection = entities.First().IsNonScheduled;
			var destinationDiagramControl = areEntitiesMovingToNonScheduledSection ? NonScheduledDiagramControl : MainDiagramControl;

			foreach (var entity in entities)
			{
				destinationDiagramControl.OnEntityMovedToDiagramSection(entity);
			}
		}

		internal void ReloadNetwork()
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
			{
				var controller = ViewModel.NetworkModel.Controller;

				// avoid recreating the ribbon after form disposal (after form disposal the controller is deactivated)
				// controller is null when using a dummy network
				if (controller == null || !controller.IsDeactivated)
				{
					SetDataContext(networkRefresher.GetReloadedNetwork(), isReloading: true);
				}
			}));
		}

		void DisplayResourceDependencyLink(IEnumerable<INetworkEntity> entities)
		{
			var fromEntity = entities.First();
			var toEntity = entities.Last();

			var sourceNode = ViewModel.NetworkViewModel.Nodes.FirstOrDefault(n => n.Entity.IsSameEntity(fromEntity));
			var destNode = ViewModel.NetworkViewModel.Nodes.FirstOrDefault(n => n.Entity.IsSameEntity(toEntity));

			var network = sourceNode.Network;

			if (sourceNode != null && destNode != null)
			{
				var sourceEntity = sourceNode.Entity;
				var destEntity = destNode.Entity;
				if (sourceEntity != null && destEntity != null)
				{
					var diagramRelationship = network.GetRelationship(sourceEntity, destEntity);
					if (diagramRelationship != null)
					{
						var connection = new ConnectionViewModel
						{
							Relationship = diagramRelationship,
							SourceConnector = sourceNode.OutputConnectors[0],
							DestConnector = destNode.InputConnectors[0],
						};

						ViewModel.NetworkViewModel.AddConnection(connection);
					}
				}
			}
		}

		void RefreshScale()
		{
			DoOnDiagramControls(control => control.RefreshScale());
		}

		void PopulateBackgrounds()
		{
			DoOnDiagramControls(control => control.PopulateBackgrounds());
		}

		public void ScrollToShape()
		{
			DoOnDiagramControls(control => control.ScrollToShape());
		}

		public NetworkUserControlViewModel ViewModel => (NetworkUserControlViewModel)DataContext;

		public NetworkViewModel NetworkViewModel => ViewModel?.NetworkViewModel;

		public void ScrollToEntity(INetworkEntity entity, bool animated = false)
		{
			DoOnDiagramControls(control => control.ScrollToEntity(entity, animated));
		}

		internal void ClearSelectedNodesOnDiagramControlsExcept(DiagramAreaUserControl controlToNotClearSelection)
		{
			DoOnDiagramControls(control =>
			{
				if (!ReferenceEquals(control, controlToNotClearSelection))
				{
					using (control.SuspendClearingNodesOnOtherControlsWhenSelectionChanges())
					{
						control.NetworkControl.SelectedNodes.Clear();
					}
				}
			});
		}

		void PasteExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = ViewModel.NetworkViewModel.TryHandlePaste();
		}

		#region Network Action Execution

		public void ExecuteRibbonAction(INetworkAction action)
		{
			// TODO: Should be executed for the most relevant section, more specifically:
			// 1) Should execute on the most relevant control, probably on the one which has selected notes or the main control otherwise
			// 2) Network actions should no longer accept NetworkViewModel in their constructors - it should be passed here as a parameter (required a significant refactoring) - it should be either main or non-scheduled network view model
			// Will be done after WI00223676
			// Currently it is executed for the main section only

			MainDiagramControl.ExecuteRibbonAction(action);
		}

		#endregion

		#region Zoom and Pan

		readonly List<Window> popUpChildren = new List<Window>();

		public void CloseAllChildren()
		{
			foreach (var window in popUpChildren)
			{
				window.Close();
			}
		}

		/// <summary>
		/// The 'ZoomIn' command (bound to the plus key) was executed.
		/// </summary>
		void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ZoomIn();
		}

		internal void ZoomIn()
		{
			DoOnDiagramControls(control => control.ZoomIn());
		}

		/// <summary>
		/// The 'ZoomOut' command (bound to the minus key) was executed.
		/// </summary>
		void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ZoomOut();
		}

		internal void ZoomOut()
		{
			DoOnDiagramControls(control => control.ZoomOut());
		}

		/// <summary>
		/// The 'JumpBackToPrevZoom' command was executed.
		/// </summary>
		void JumpBackToPrevZoom_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			JumpBackToPrevZoom();
		}

		internal void JumpBackToPrevZoom()
		{
			DoOnDiagramControls(control => control.JumpBackToPrevZoom());
		}

		/// <summary>
		/// Determines whether the 'JumpBackToPrevZoom' command can be executed.
		/// </summary>
		void JumpBackToPrevZoom_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = MainDiagramControl.IsPreviousZoomRectangleSet;
		}

		/// <summary>
		/// The 'FitContent' command was executed.
		/// </summary>
		void FitContent_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			FitNodes();
		}

		internal void FitNodes()
		{
			DoOnDiagramControls(control => control.FitNodes());
		}

		void PopOut_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			PopOut();
		}

		public Window PopOut()
		{
			var control = new NetworkUserControl(ViewModel.DiagramEntity, networkRefresher, ViewModel.TemplateSelector, ViewModel.NetworkViewModel.NodeViewModelProvider, ribbonDataProvider);
			var window = new Window
			{
				Content = control,
				Title = string.IsNullOrEmpty(ViewModel.NetworkModel.Name) ? Res.GetString("1d7a1e68-87a3-43e8-a357-d3b7705438e6", "Untitled") : ViewModel.NetworkModel.Name
			};
			window.Closed += PopOutWindow_Closed(control);
			popUpChildren.Add(window);

			/*
			 * HACK: This order of actions is unfortunately necessary in order to have both binding and rendering work correctly the first time. 
			 */
			ElementHost.EnableModelessKeyboardInterop(window);
			window.Show();
			control.SetDataContext(networkRefresher.GetReloadedNetwork(), isReloading: false);
			window.Width++;
			window.Width--;

			return window;
		}

		EventHandler PopOutWindow_Closed(NetworkUserControl control)
		{
			return (s, e) =>
			{
				networkRefresher.Refresh(RefreshType.Close);
				networkRefresher.Refresh(RefreshType.RefreshButton);
				control.Dispose();
			};
		}
		void Refresh_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			networkRefresher.Refresh(RefreshType.RefreshButton);
		}

		/// <summary>
		/// The 'Fill' command was executed.
		/// </summary>
		void Fill_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Fill();
		}

		internal void Fill()
		{
			DoOnDiagramControls(control => control.Fill());
		}

		/// <summary>
		/// The 'OneHundredPercent' command was executed.
		/// </summary>
		void OneHundredPercent_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			OneHundredPercent();
		}

		internal void OneHundredPercent()
		{
			DoOnDiagramControls(control => control.OneHundredPercent());
		}

		#endregion

		#region Dependency Properties

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Required for WPF to work properly.")]
		public static readonly DependencyProperty MainDiagramAreaProperty = DependencyProperty.Register(nameof(MainDiagramControl), typeof(UIElement), typeof(DiagramAreaUserControl));

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Required for WPF to work properly.")]
		public static readonly DependencyProperty NonScheduledDiagramAreaProperty = DependencyProperty.Register(nameof(NonScheduledDiagramControl), typeof(UIElement), typeof(DiagramAreaUserControl));

		#endregion

	}
}
