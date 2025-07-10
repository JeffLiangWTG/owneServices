using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
#if !WINZOR
using System.Windows.Controls;
#endif
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class NetworkUserControlViewModel : ViewModelBase
	{
		#region Internal Data Members

		/// <summary>
		/// This is the network that is displayed in the window.
		/// It is the main part of the view-model.
		/// </summary>
		NetworkViewModel networkViewModel;

		///
		/// The current scale at which the content is being viewed.
		/// 
		double contentScale = 1;

		///
		/// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
		/// 
		double contentOffsetY;

		///
		/// The X coordinate of the offset of the viewport onto the content (in content coordinates).
		/// 
#if WINZOR
		double contentOffsetX;
#endif

		#endregion Internal Data Members

		//the parameterless constructor is used in XAML
		public NetworkUserControlViewModel()
			: this(DummyNetwork.GetDummyNetwork(true), null)
		{
		}

#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public NetworkUserControlViewModel(INetwork network,
#if !WINZOR
			DataTemplateSelector selector = null,
#endif
			NodeViewModelProvider viewModelProvider = null, bool isReloading = false, IRibbonDataProvider ribbonDataProvider = null, NetworkRibbonResourcesProvider ribbonResourcesProvider = null, NetworkUserControl control = null, bool shapeInspectorVisible = false)
			: base(network.DiagramEntity)
		{
			NetworkModel = network;

			NetworkViewModel = CreateAndBuildNetworkViewModel(network, viewModelProvider, isReloading);
			NetworkViewModel.SelectionChanged += NetworkViewModel_SelectionChanged;
			ShapeInspectorVisible = shapeInspectorVisible;
#if !WINZOR
			TemplateSelector = selector;
#endif

			RibbonViewModel = ribbonDataProvider?.GetRibbonViewModel(NetworkViewModel, control);
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		static NetworkViewModel CreateAndBuildNetworkViewModel(INetwork network, NodeViewModelProvider viewModelProvider, bool isReloading)
		{
			var viewModel = new NetworkViewModel(network, viewModelProvider);

			viewModel.BuildNetwork(isReloading);

			return viewModel;
		}

		#region For Test
#if DEBUG
		public IEnumerable<INetworkAction> DiagramNetworkActions_ExposedForTest => diagramNetworkActions;
#endif
#endregion

		#region Related Objects

		public INetwork NetworkModel
		{
			get
			{
				return networkModel;
			}
			private set
			{
				networkModel = value;
				OnPropertyChanged(nameof(NetworkModel));
				OnPropertyChanged(nameof(DiagramBackgroundColor));
				OnPropertyChanged(nameof(DiagramForegroundColor));
				OnPropertyChanged(nameof(DiagramCornerRadius));
			}
		}
		INetwork networkModel;

		public INetworkScaleDescriptor ScaleDescriptor => NetworkModel.ScaleDescriptor;

		public IDiagramEntity DiagramEntity => NetworkModel.DiagramEntity;

		public NodeViewModel DiagramEntityViewModel => NetworkViewModel.DiagramNodeViewModel;

		public IObservableReloadableCollection<IAffinity> AvailableAffinities => DiagramEntity.AvailableAffinities;

		public IObservableReloadableCollection<IAffinity> AppliedAffinities => DiagramEntity.AppliedAffinities;

		#region Context Menu

		public IObservableReloadableCollection<NetworkActionMenuItem> MenuItems
		{
			get
			{
				return menuItems ?? (menuItems = new ImpObservableCollection<NetworkActionMenuItem>(GetMenuItemsCore));
			}
		}
		ImpObservableCollection<NetworkActionMenuItem> menuItems;

		public void ReloadMenuItems()
		{
			MenuItems.Reload();
		}

		IEnumerable<NetworkActionMenuItem> GetMenuItemsCore()
		{
			return diagramNetworkActions.ToMenuItemsGrouped();
		}

		IEnumerable<INetworkAction> diagramNetworkActions = Enumerable.Empty<INetworkAction>();

#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public void SetupNetworkActions(NetworkUserControl control)
		{
			diagramNetworkActions = NodeNetworkActionProvider.GetNetworkActions(NetworkViewModel).Concat(DiagramNetworkActionProvider.GetNetworkActions(NetworkViewModel, control));
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		#endregion

		/// <summary>
		/// This is the view model of the network that is displayed in the window.
		/// It is the main part of the view-model.
		/// </summary>
		public NetworkViewModel NetworkViewModel
		{
			get
			{
				return networkViewModel;
			}
			set
			{
				networkViewModel = value;
				OnPropertyChanged(nameof(NetworkViewModel));
			}
		}

		public RibbonViewModel RibbonViewModel { get; private set; }

		#endregion

		#region Properties

		public bool IsNetworkEnabled
		{
			get { return !NetworkModel.IsReadOnly; }
		}

		public string StatusMessage
		{
			get
			{
				if (!IsNetworkEnabled)
				{
					return Res.GetString("36d158a8-8dd9-4d88-bc4a-769517c031aa", "This diagram is read-only.");
				}
				else if (HiddenItemsExist)
				{
					return Res.GetString("e30002db-28af-460c-8e92-b14760ffae61", "There are hidden items within this diagram.");
				}
				else
				{
					return null;
				}
			}
		}

		public string StatusTooltip
		{
			get { return HiddenItemsExist ? Res.GetString("6fe433f8-e214-47f0-8769-586972b123b4", "Right-click the designer surface to show hidden items.") : null; }
		}

		/// </summary>
		/// The current scale at which the content is being viewed.
		/// </summary>
		public double ContentScale
		{
			get { return contentScale; }
			set
			{
				contentScale = value;
				OnPropertyChanged(nameof(ContentScale));
				OnPropertyChanged(nameof(ZoomOutEnabled));
			}
		}

		public bool ZoomOutEnabled
		{
			get { return CanZoomOut(ContentScale); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "We're not in ZArchitecture.")]
		internal bool CanZoomOut(double currentScale)
		{
			const double minZoom = 0.1; // 10% is the lowest zoom we allow. Any smaller and the visualiser isn't usable for scaled diagrams.

			return Math.Round(currentScale, 1) > minZoom;
		}

		/// <summary>
		/// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
		/// </summary>
		public double ContentOffsetY
		{
			get { return contentOffsetY; }
			set
			{
				contentOffsetY = value;
				OnPropertyChanged(nameof(ContentOffsetY));
			}
		}

		/// <summary>
		/// The X coordinate of the offset of the viewport onto the content (in content coordinates).
		/// </summary>
#if WINZOR
		public double ContentOffsetX
		{
			get { return contentOffsetX; }
			set
			{
				contentOffsetX = value;
				OnPropertyChanged(nameof(ContentOffsetX));
			}
		}
#endif

		#region Diagram Entity Layout

		public bool SupportsDiagramVisualStyles
		{
			get { return DiagramEntity.Supports(NetworkActions.StyleDiagram); }
		}

		public bool RequiresProp
		{
			get { return !SupportsDiagramVisualStyles; }
		}

		public bool SupportsEditEntity
		{
			get { return DiagramEntity.Supports(NetworkActions.EditEntity); }
		}

		public bool SupportsChildEntities
		{
			get { return DiagramEntity.Supports(NetworkActions.AddChildEntities); }
		}

		public Color DiagramBackgroundColor
		{
			get { return NetworkModel.DiagramEntity.BackColor; }
			set
			{
				NetworkModel.DiagramEntity.BackColor = value;
				OnPropertyChanged(nameof(DiagramBackgroundColor));
				ColorList.ResetForBinding();
			}
		}

		public Color DiagramForegroundColor
		{
			get { return NetworkModel.DiagramEntity.ForeColor; }
			set
			{
				NetworkModel.DiagramEntity.ForeColor = value;
				OnPropertyChanged(nameof(DiagramForegroundColor));
				ColorList.ResetForBinding();
			}
		}

		public int DiagramCornerRadius
		{
			get { return NetworkModel.DiagramEntity.CornerRadius; }
			set
			{
				NetworkModel.DiagramEntity.CornerRadius = value;
				OnPropertyChanged(nameof(DiagramCornerRadius));
			}
		}

		public ColorList ColorList
		{
			get { return colorList ?? (colorList = new ColorList()); }
		}

		ColorList colorList;

		public string DiagramName
		{
			get { return NetworkModel.DiagramEntity.Name; }
			set { NetworkModel.DiagramEntity.Name = value; }
		}

		public string JobNumber
		{
			get { return NetworkModel.DiagramEntity.JobNumber; }
		}

		public string JobName
		{
			get { return NetworkModel.DiagramEntity.JobName; }
			set { NetworkModel.DiagramEntity.JobName = value; }
		}

		public bool JobNameEnabled
		{
			get { return !NetworkModel.DiagramEntity.JobName_ReadOnly; }
		}

#if !WINZOR
		public DataTemplateSelector TemplateSelector { get; set; }
#endif

#endregion

		#region Shape Inspector

		public bool ShapeInspectorVisible
		{
			get => shapeInspectorShapeVisible;
			set
			{
				shapeInspectorShapeVisible = value;
				OnPropertyChanged();
			}
		}

		bool shapeInspectorShapeVisible;

		public NodeViewModel ShapeInspectorNodeViewModel
		{
			get => shapeInspectorNodeViewModel;
			set
			{
				shapeInspectorNodeViewModel = value;
				OnPropertyChanged();
			}
		}

		NodeViewModel shapeInspectorNodeViewModel;

		void NetworkViewModel_SelectionChanged(object sender, RefreshArgs e)
		{
			if (shapeInspectorShapeVisible)
			{
				if (NetworkViewModel.SelectedNodes.Count() != 1)
				{
					ShapeInspectorNodeViewModel = null;
					return;
				}

				var newModel = NetworkViewModel.SelectedNodes.First();
				ShapeInspectorNodeViewModel = newModel;
			}
		}

		#endregion

#endregion

		#region Nodes

		internal void RefreshNodes()
		{
			NetworkViewModel.Refresh();
		}

		public void SelectRelevantNodes()
		{
			var nodes = NetworkViewModel.Nodes.ToArray();

			foreach (var node in nodes)
			{
				if (node.IsSelected)
				{
					var entity = node.Entity;

					if (entity != null)
					{
						var childNodes = entity.Children.Select(NetworkViewModel.GetNodeForEntity).Where(childNode => childNode != null);

						foreach (var child in childNodes)
						{
							child.IsSelected = !entity.EntityState.HasFlag(EntityState.Fixed);
						}
					}
				}
			}
		}

		#endregion

		#region Hidden Entities / Dependencies

		bool HiddenItemsExist
		{
			get { return HiddenEntities.Count > 0 || HiddenDependencies.Count > 0; }
		}

		public IObservableReloadableCollection<IProposedNetworkEntity> HiddenEntities
		{
			get { return NetworkModel.DiagramEntity.HiddenEntities; }
		}

		public IObservableReloadableCollection<IEntityRelationship> HiddenDependencies
		{
			get { return NetworkModel.DiagramEntity.HiddenRelationships; }
		}

		#endregion
	}
}
