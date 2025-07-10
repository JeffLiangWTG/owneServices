using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;

namespace CargoWise.NetworkVisualisation.Business
{
	[System.Diagnostics.DebuggerDisplay("{Entity.Name}, X:{X}, Y:{Y}")]
	public class NodeViewModel : ViewModelBase, IEquatable<NodeViewModel>
	{
		#region Private Data Members

		/// <summary>
		/// The Z index of the node.
		/// </summary>
		int zIndex;

		/// <summary>
		/// List of input connectors (connections points) attached to the node.
		/// </summary>
		ImpObservableCollection<ConnectorViewModel> inputConnectors;

		/// <summary>
		/// List of output connectors (connections points) attached to the node.
		/// </summary>
		ImpObservableCollection<ConnectorViewModel> outputConnectors;

		/// <summary>
		/// Set to 'true' when the node is selected.
		/// </summary>
		bool isSelected;

		#endregion Private Data Members

		#region Constants

		public const double MinAllowedNodeWidth = 100d;
		public const double MinAllowedNodeHeight = 50d;

		#endregion

		public NodeViewModel()
		{
		}

		public NodeViewModel(INetworkEntity entity, NetworkViewModel networkViewModel)
			: base(entity)
		{
			this.entity = entity;
			this.scheduledEntity = entity as IScheduledNetworkEntity;
			this.networkViewModel = networkViewModel;

			if (entity != null && Network != null && entity != Network.DiagramEntity)
			{
				zIndex = entity.ZIndex;
			}
		}

		public INetworkEntity Entity
		{
			get { return entity; }
		}

		public INetwork Network => NetworkViewModel?.Network;

		public NetworkViewModel NetworkViewModel
		{
			get { return networkViewModel; }
		}

		readonly INetworkEntity entity;
		readonly IScheduledNetworkEntity scheduledEntity;
		readonly NetworkViewModel networkViewModel;

		#region Properties

		public Color ForegroundColor => Entity.ForeColor.IsEmpty ? Color.Black : Entity.ForeColor;

		public virtual int LowerBarHeight => LowerBar != null ? 5 : 0;

		public virtual ProgressBar LowerBar { get; set; }

		public virtual string LowerBarTooltip { get; set; }

		public string DeleteImageSource => "..\\Resources\\trashcan.png";

		public string DeleteTooltip
		{
			get { return entity.CanUnlinkEntity ? Res.GetString("89202b29-7f13-4849-882b-95700ed4de4c", "Remove shape from diagram and delete underlying entity.") : Res.GetString("38903928-25fb-420a-b677-4c80df1cbc87", "Remove shape from diagram."); }
		}

		/// <summary>
		/// The name of the node.
		/// </summary>
		[NetworkDiagramSearchable]
		public string Name
		{
			get { return entity.Name; }
			set
			{
				if (entity.Name == value)
				{
					return;
				}

				entity.Name = value;

				OnPropertyChanged(nameof(Name));
			}
		}

		[NetworkDiagramSearchable]
		public string Description
		{
			get { return entity.Description; }
		}

		#region Show Visual Objects Specific to Shapes with Entities

		public virtual bool IsEntityDecorationVisible
		{
			get { return true; }
		}

		public virtual bool IsEditableNotesVisible
		{
			get { return false; }
		}

		public virtual bool IsCompletionCriteriaVisible
		{
			get { return true; }
		}

		public virtual bool ShowScheduleDetails
		{
			get { return Network.DiagramEntity.IsDiagramScaled && scheduledEntity != null; }
		}

		[NetworkDiagramSearchable]
		public virtual string JobNumberReadableText
		{
			get
			{
				if (string.IsNullOrEmpty(Entity.JobNumber))
				{
					return "N/A";
				}
				else
				{
					return Entity.JobNumber;
				}
			}
		}

		public string JobNumberTooltip
		{
			get { return Res.GetString("64b16274-4885-436f-becd-cbd7ed7bbc6e", "Job number"); }
		}

		[NetworkDiagramSearchable]
		public string AppliedAttributesReadableText
		{
			get
			{
				const int maxAttributeNamesToDisplay = 5;

				var names = GetAppliedAttributeNames().Where(name => !string.IsNullOrEmpty(name)).Take(maxAttributeNamesToDisplay + 1).ToArray();
				var result = string.Join(", ", names.Take(maxAttributeNamesToDisplay));

				if (names.Length > maxAttributeNamesToDisplay)
				{
					result += ", ...";
				}

				return result;
			}
		}

		IEnumerable<string> GetAppliedAttributeNames()
		{
			var channel = Entity.GetMemberChannel(NetworkViewModel);

			if (channel != null)
			{
				yield return channel.Name;
			}

			foreach (var affinity in AppliedAffinities)
			{
				yield return affinity.Name;
			}
		}

		public string AppliedAttributesTooltip
		{
			get
			{
				if (Network.DiagramEntity.DiagramChannels.Any())
				{
					return Res.GetString("524c26f5-cacf-4039-9e57-3d3121127d0f", "Channel and applied affinities");
				}
				else
				{
					return Res.GetString("fa0bce91-9354-4bae-9861-5b9b468e92f2", "Applied affinities");
				}
			}
		}

		public string DurationReadableText
		{
			get
			{
				var durationInHours = (scheduledEntity.ExplicitDurationMinutes / 60m);

				return durationInHours.FormatHoursToTimeString();
			}
		}

		public string DurationTooltip => Res.GetString("2b17a802-4528-4fe1-943d-a21909347d35", "Planned Duration");

		public string StartDateReadableText
		{
			get { return ShowStartDate ? scheduledEntity.ScheduledStartTimeLocal.FormatAsBestReadableDateTime() : string.Empty; }
		}

		public string StartDateToolTip
		{
			get { return Res.GetString("e7128918-2e8c-4f97-88eb-d62a4315d582", "Scheduled Start"); }
		}

		bool ShowStartDate => ShowScheduleDetails && scheduledEntity.ScheduledStartTimeLocal != default(DateTime);

		public string RemainingDurationReadableText
		{
			get
			{
				var remainingDurationMinutes = scheduledEntity.RemainingDurationMinutes;

				return ShowScheduleDetails && remainingDurationMinutes > 0 ? (remainingDurationMinutes / 60m).FormatHoursToTimeString() : string.Empty;
			}
		}

		public string RemainingDurationToolTip
		{
			get { return Res.GetString("e937cf30-b3ed-4662-93be-f70bd08333f2", "Remaining Duration"); }
		}

		public string FinishDateReadableText
		{
			get { return ShowFinishDate ? scheduledEntity.ScheduledEndTimeLocal.FormatAsBestReadableDateTime() : string.Empty; }
		}

		public string FinishDateToolTip
		{
			get { return Res.GetString("25a6c620-6980-42f4-a825-c55dc97f4efd", "Scheduled Finish"); }
		}

		bool ShowFinishDate => ShowScheduleDetails && scheduledEntity.ScheduledEndTimeLocal != default(DateTime);

		[NetworkDiagramSearchable]
		public string Notes
		{
			get { return entity.Notes; }
			set
			{
				if (!IsNotesPlaceholder(value))
				{
					entity.Notes = value;
					OnPropertyChanged(nameof(Notes));
				}
			}
		}

		public bool IsNotesPlaceholder(string notes)
		{
			return string.Equals(NotesPlaceholder, notes, StringComparison.OrdinalIgnoreCase);
		}

		public virtual string NotesPlaceholder
		{
			get { return Res.GetString("c9a2e8e7-3d78-46f8-a573-0b6040b25210", "Notes"); }
		}

		#endregion

		/// <summary>
		/// The Estimate summary of the node.
		/// </summary>
		public string Estimate
		{
			get { return entity.EstimateSummary; }
		}

		public string ToolTip
		{
			get { return entity.Name; }
		}

		#region Job Details

		public string DiagramName
		{
			get { return Entity.Name; }
			set
			{
				Entity.Name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		public string JobNumber
		{
			get { return Entity.JobNumber; }
		}

		[NetworkDiagramSearchable]
		public string JobName
		{
			get { return Entity.JobName; }
			set
			{
				Entity.JobName = value;
				OnPropertyChanged(nameof(JobName));
			}
		}

		public bool ShowJobBar
		{
			get { return Entity.IsLinkedToWorkflow || Network.DiagramEntity.IsDiagramScaled; }
		}

		public bool ShowJobBarAffinities
		{
			get { return Entity.IsLinkedToWorkflow || Network.DiagramEntity.IsDiagramScaled || AppliedAffinities.Any(); }
		}

		public bool JobNameEnabled
		{
			get { return !Entity.JobName_ReadOnly; }
		}

		public bool HasLinkedEntity
		{
			get { return Entity.HasLinkedEntity; }
		}

		public bool IsDiagramWithRibbon
		{
			get { return Entity.IsDiagramWithRibbon; }
		}

		[NetworkDiagramSearchable]
		public string CompletionCriteria
		{
			get { return Entity.CompletionCriteria; }
			set
			{
				if (!IsCompletionCriteriaPlaceholder(value))
				{
					Entity.CompletionCriteria = value;
					OnPropertyChanged(nameof(CompletionCriteria));
				}
			}
		}

		public bool IsCompletionCriteriaPlaceholder(string criteria)
		{
			return string.Equals(CompletionCriteriaPlaceholder, criteria, StringComparison.OrdinalIgnoreCase);
		}

		public virtual string CompletionCriteriaPlaceholder
		{
			get { return Res.GetString("0677e975-d2fa-4e27-9cdd-8ce06e8834cc", "Completion Criteria"); }
		}

		public Color CompletionCriteriaTextColor => IsCompletionCriteriaPlaceholder(CompletionCriteria) ? Color.Gray : ForegroundColor;

		#endregion

		#region Status

		public string StatusTooltip
		{
			get { return Entity.StatusDescription; }
		}

		public NodeColors StatusColors
		{
			get
			{
				var colors = GetStatusColors();
				return new NodeColors(colors, StatusColorsAngle, StatusOpacity);
			}
		}

		protected virtual double StatusColorsAngle => 90;

		protected double StatusOpacity => entity.EntityState.HasFlag(EntityState.Inactive) ? 0.4 : 1.0;

		protected virtual IEnumerable<ColorOffset> GetStatusColors()
		{
			var colors = new List<ColorOffset>();
			if (Status == WorkStatus.Complete || Status == WorkStatus.Cancelled)
			{
				colors.Add(new ColorOffset(Color.White, 0));
				colors.Add(new ColorOffset(Color.LightGray, 0.6));
			}
			else
			{
				var backgroundColors = GetBackgroundGradientColours().ToArray();
				var count = backgroundColors.Length;
				if (count > 0)
				{
					for (var i = 0; i < count; i++)
					{
						colors.Add(new ColorOffset(backgroundColors[i], i / (double)(count - 1)));
					}
				}
				else
				{
					colors.Add(new ColorOffset(Color.White, 0));
					colors.Add(new ColorOffset(Color.Beige, 0.6));
				}
			}

			return colors;
		}

		IEnumerable<Color> GetBackgroundGradientColours()
		{
			var channel = Entity.GetMemberChannel(NetworkViewModel);

			if (channel != null && !channel.Color.IsEmpty)
			{
				yield return channel.Color;
			}

			var affinities = AppliedAffinities;

			if (affinities != null)
			{
				foreach (var affinity in affinities)
				{
					if (!affinity.Colour.IsEmpty)
					{
						yield return affinity.Colour;
					}
				}
			}
		}

		public void UpdateAffinityDetails()
		{
			OnPropertyChanged(nameof(StatusColors));
			OnPropertyChanged(nameof(ShowJobBarAffinities));
			OnPropertyChanged(nameof(AppliedAttributesReadableText));
		}

		public NodeFontWeight StatusTextWeight
		{
			get
			{
				if (CanHaveChildren)
				{
					return HeavyFontWeight;
				}

				switch (Status)
				{
					case WorkStatus.Blocked:
					case WorkStatus.Cancelled:
					case WorkStatus.Complete:
						return NodeFontWeight.Medium;

					default:
						return HeavyFontWeight;
				}
			}
		}

		static NodeFontWeight HeavyFontWeight
		{
			get { return NodeFontWeight.Black; }
		}

		public WorkStatus Status
		{
			get { return entity.Status; }
		}

		#endregion

		#region Supported Actions

		public bool SupportsDiagramVisualStyles
		{
			get { return entity.Supports(NetworkActions.StyleDiagram); }
		}

		public virtual bool SupportsEditEntity
		{
			get { return entity.Supports(NetworkActions.EditEntity); }
		}

		public virtual bool SupportsShowItems
		{
			get { return entity.Supports(NetworkActions.Show); }
		}

		public virtual bool SupportsChildEntities
		{
			get { return entity.Supports(NetworkActions.AddChildEntities); }
		}

		public bool SupportsAffinities
		{
			get { return entity.Supports(NetworkActions.Affinities); }
		}

		#endregion

		#region Menu Items

		public IObservableReloadableCollection<NetworkActionMenuItem> MenuItems
		{
			get { return menuItems ?? (menuItems = new ImpObservableCollection<NetworkActionMenuItem>(GetMenuItemsCore)); }
		}
		ImpObservableCollection<NetworkActionMenuItem> menuItems;

		public void ReloadMenuItems()
		{
			MenuItems.Reload();
		}

		IEnumerable<NetworkActionMenuItem> GetMenuItemsCore()
		{
			return NodeNetworkActionProvider.GetNetworkActions(NetworkViewModel).ToMenuItemsGrouped();
		}

		#endregion

		#region TextAlignment

		public NodeTextAlignment TextAlignment
		{
			get { return CanHaveChildren ? NodeTextAlignment.Left : NodeTextAlignment.Center; }
		}

		#endregion

		public bool CanDelete
		{
			get { return entity.CanDeleteUnderlyingEntity; }
		}

		public bool CanHide
		{
			get { return entity.Supports(NetworkActions.Hide); }
		}

		public bool CanHaveChildren
		{
			get
			{
				if (Entity.IsSameEntity(Network.DiagramEntity))
				{
					return true;
				}
				else
				{
					var entityWithChildren = Entity as INetworkEntityWithChildren;
					if (entityWithChildren != null && entityWithChildren.CanHaveChildren)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool SupportsGenericActions => NetworkViewModel.Network.DiagramEntity.Supports(NetworkActions.GenericActions);

		#region Location

		/// <summary>
		/// The X coordinate for the position of the node.
		/// </summary>
		public double X
		{
			get { return Entity.X; }
			set
			{
				if (!(IsDragged && AncestorsSelected(entity)))
				{
					SetX(value);
				}
			}
		}

		/// <summary>
		/// The Y coordinate for the position of the node.
		/// </summary>
		public double Y
		{
			get { return Entity.Y; }
			set
			{
				if (!(IsDragged && AncestorsSelected(entity)))
				{
					SetY(value);
				}
			}
		}

		internal void SetX(double value, bool forceRefresh = false)
		{
			if (NetworkViewModel.XCoordinateStrategy.SetProperty(value, this, Entity) || forceRefresh)
			{
				OnPropertyChanged(nameof(X));
			}
		}

		internal void SetY(double value, bool forceRefresh = false)
		{
			if (NetworkViewModel.YCoordinateStrategy.SetProperty(value, this, Entity) || forceRefresh)
			{
				OnPropertyChanged(nameof(Y));
			}
		}

		public NodeResizeReferencePoint ReferencePoint { get; set; }

		public void AdjustToScale()
		{
			if (Network.DiagramEntity.IsDiagramScaled)
			{
				Width = Width;
				Height = Height;
				this.AdjustLocationToScale();
				this.AdjustSizeToScale();
			}
		}

		public void AdjustLocationToScale()
		{
			if (entity != null)
			{
				if (entity.AdjustLocationToScale(Network.DiagramEntity))
				{
					OnPropertyChanged(nameof(X));
				}

				if (entity.AdjustLocationToChannels(Network.DiagramEntity, NetworkViewModel))
				{
					OnPropertyChanged(nameof(Y));
				}
			}
		}

		void MaybeRefreshChannelVisualisation()
		{
			if (!Entity.IsCalculationSuspended && Network.DiagramEntity.DiagramChannels.Any())
			{
				OnPropertyChanged(nameof(StatusColors));
				OnPropertyChanged(nameof(AppliedAttributesReadableText));
				OnPropertyChanged(nameof(AppliedAttributesTooltip));
			}
		}

		#endregion

		#region IsDragging

		public bool IsContinuouslyChanging
		{
			set
			{
				if (value)
				{
					entity.SuspendCalculation();
				}
				else
				{
					entity.ResumeCalculation();
				}

				isContinuouslyChanging = value;
			}
			get { return isContinuouslyChanging; }
		}
		bool isContinuouslyChanging;

		public bool IsDragged { get; set; }

		public void OnDragCompleted()
		{
			AdjustLocationToScale();
			MaybeRefreshChannelVisualisation();
		}

		public void OnResizeCompleted()
		{
			if (!IsContinuouslyChanging)
			{
				using (NetworkViewModel.PerformNodeResize())
				{
					AdjustLocationToScale();
					AdjustSizeToScale();

					// update immediate children that may have been affected by parent adjustment
					// (will occur only for scaled diagrams)
					UpdateChildNodes(child =>
					{
						child.AdjustLocationToScale();
						child.AdjustSizeToScale();
					});

					Network.Refresh(RefreshType.EntitySize, Entity);
				}
			}
		}

		#endregion

		#region Update Position

		bool isUpdatingPosition;

		internal void UpdatePositionCore(Action positionUpdater)
		{
			if (!isUpdatingPosition && entity != null)
			{
				isUpdatingPosition = true;

				try
				{
					positionUpdater();
				}
				finally
				{
					isUpdatingPosition = false;
				}
			}
		}

		#endregion

		/// <summary>
		/// The Z index of the node.
		/// </summary>
		public int ZIndex
		{
			get { return zIndex; }
			set
			{
				if (zIndex == value)
				{
					return;
				}

				zIndex = value;

				if (entity != null)
				{
					entity.ZIndex = value;
				}

				OnPropertyChanged(nameof(ZIndex));
			}
		}

		#region Size

		public double GetAppropriateWidth(double proposedWidth)
		{
			var width = entity.Width;
			new WidthCoordinateStrategy((_, w) => width = w, _ => width, NetworkViewModel).SetProperty(proposedWidth, this, Entity);
			return width;
		}

		public double GetAppropriateHeight(double proposedHeight)
		{
			var height = entity.Height;
			new HeightCoordinateStrategy((_, h) => height = h, _ => height, NetworkViewModel).SetProperty(proposedHeight, this, Entity);
			return height;
		}

		public double Width
		{
			get
			{
				var width = entity.Width;
				if (width > 0)
				{
					return width;
				}
				else
				{
					return Network.EntityPositionStrategy.DefaultWidth;
				}
			}
			set
			{
				if (NetworkViewModel.WidthCoordinateStrategy.SetProperty(value, this, Entity))
				{
					OnPropertyChanged(nameof(Width));
				}
			}
		}

		public double Height
		{
			get
			{
				var height = entity.Height;
				if (height > 0)
				{
					return height;
				}
				else
				{
					return Network.EntityPositionStrategy.DefaultHeight;
				}
			}
			set
			{
				if (NetworkViewModel.HeightCoordinateStrategy.SetProperty(value, this, Entity))
				{
					OnPropertyChanged(nameof(Height));
				}
			}
		}

		/// <summary>
		/// Event raised when the size of the node is changed.
		/// The size will change when the UI has determined its size based on the contents
		/// of the nodes data-template.  It then pushes the size through to the view-model
		/// and this 'SizeChanged' event occurs.
		/// </summary>
		public event EventHandler<EventArgs> SizeChanged;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "We're not in ZArchitecture anymore")]
		public void AdjustSizeToScale()
		{
			if (entity != null && !entity.IsNonScheduled && entity.CheckIsScalable(Network.DiagramEntity))
			{
				var scaledWidth = entity.GetScaleWidth(Network.DiagramEntity);
				var scaledHeight = entity.GetHeightAdjustedForChannel(Network.DiagramEntity, NetworkViewModel);

				if (Math.Round(Width, 2) == Math.Round(scaledWidth, 2) && Math.Round(Height, 2) == Math.Round(scaledHeight, 2))
				{
					return;
				}

				Width = scaledWidth;
				Height = scaledHeight;

				SizeChanged?.Invoke(this, EventArgs.Empty);

				OnPropertyChanged(nameof(DurationReadableText));
				OnPropertyChanged(nameof(StartDateReadableText));
				OnPropertyChanged(nameof(FinishDateReadableText));
			}
		}

		#endregion

		/// <summary>
		/// Set to 'true' when the node is selected.
		/// </summary>
		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				if (isSelected == value)
				{
					return;
				}

				isSelected = value;

				NetworkViewModel.OnSelectionChanged();

				OnPropertyChanged(nameof(IsSelected));
				OnPropertyChanged("AdditionalDetail");
			}
		}

		#endregion

		#region Related Entities

		/// <summary>
		/// List of input connectors (connections points) attached to the node.
		/// </summary>
		public ImpObservableCollection<ConnectorViewModel> InputConnectors
		{
			get
			{
				if (inputConnectors == null)
				{
					inputConnectors = new ImpObservableCollection<ConnectorViewModel>();
					inputConnectors.ItemsAdded += new EventHandler<CollectionItemsChangedEventArgs>(InputConnectors_ItemsAdded);
					inputConnectors.ItemsRemoved += new EventHandler<CollectionItemsChangedEventArgs>(InputConnectors_ItemsRemoved);
				}

				return inputConnectors;
			}
		}

		/// <summary>
		/// List of output connectors (connections points) attached to the node.
		/// </summary>
		public ImpObservableCollection<ConnectorViewModel> OutputConnectors
		{
			get
			{
				if (outputConnectors == null)
				{
					outputConnectors = new ImpObservableCollection<ConnectorViewModel>();
					outputConnectors.ItemsAdded += new EventHandler<CollectionItemsChangedEventArgs>(OutputConnectors_ItemsAdded);
					outputConnectors.ItemsRemoved += new EventHandler<CollectionItemsChangedEventArgs>(OutputConnectors_ItemsRemoved);
				}

				return outputConnectors;
			}
		}

		public IObservableReloadableCollection<IAffinity> AvailableAffinities
		{
			get { return entity.AvailableAffinities; }
		}

		public IObservableReloadableCollection<IAffinity> AppliedAffinities
		{
			get { return entity.AppliedAffinities; }
		}

		/// <summary>
		/// A helper property that retrieves a list (a new list each time) of all connections attached to the node. 
		/// </summary>
		public ICollection<ConnectionViewModel> AttachedConnections
		{
			get
			{
				List<ConnectionViewModel> attachedConnections = new List<ConnectionViewModel>();

				foreach (var connector in this.InputConnectors)
				{
					attachedConnections.AddRange(connector.AttachedConnections);
				}

				foreach (var connector in this.OutputConnectors)
				{
					attachedConnections.AddRange(connector.AttachedConnections);
				}

				return attachedConnections;
			}
		}

		public IObservableReloadableCollection<IProposedNetworkEntity> HiddenEntities
		{
			get
			{
				var entityWithChildren = Entity as INetworkEntityWithChildren;
				return entityWithChildren != null ? entityWithChildren.HiddenEntities : null;
			}
		}

		public IObservableReloadableCollection<IEntityRelationship> HiddenDependencies
		{
			get
			{
				var entityWithChildren = Entity as INetworkEntityWithChildren;
				return entityWithChildren != null ? entityWithChildren.HiddenRelationships : null;
			}
		}

		#endregion

		#region Z Order

		public void BringToFront()
		{
			var nodeWithHighestZIndex = NetworkViewModel.Nodes.Where(n => n != this).OrderByDescending(n => n.ZIndex).FirstOrDefault();
			if (nodeWithHighestZIndex != null)
			{
				BringToFrontCore(nodeWithHighestZIndex.ZIndex);
			}
		}

		void BringToFrontCore(int highestZIndex)
		{
			var newZIndex = highestZIndex + 1;
			ZIndex = newZIndex;

			foreach (var node in Entity.Children.Select(NetworkViewModel.GetNodeForEntity).Where(n => n != null))
			{
				node.BringToFrontCore(newZIndex);
			}
		}

		int SendToBackCore(int startZIndex, IEnumerable<INetworkEntity> children)
		{
			int maxZIndex = startZIndex;

			var childrenQuery = children.Select(NetworkViewModel.GetNodeForEntity).Where(n => n != this && n != null);
			var minChildrenZIndex = childrenQuery.OrderByDescending(n => n.ZIndex).Select(n => n.ZIndex).LastOrDefault();

			foreach (var node in childrenQuery)
			{
				node.ZIndex = startZIndex + node.ZIndex - minChildrenZIndex + 1;
				var currZIndex = node.SendToBackCore(node.ZIndex, node.Entity.Children);
				if (currZIndex > maxZIndex)
				{
					maxZIndex = currZIndex;
				}
			}

			return maxZIndex;
		}

		public void SendToBack()
		{
			var parentLayout = Entity.Parent as INetworkEntity;
			if (parentLayout != null)
			{
				ZIndex = parentLayout.ZIndex + 1;
				var startZIndex = SendToBackCore(ZIndex, Entity.Children);

				SendToBackCore(startZIndex, parentLayout.Children);
			}
		}

		public IEnumerable<NodeViewModel> ChildNodes
		{
			get { return entity.Children?.Select(NetworkViewModel.GetNodeForEntity).Where(e => e != null) ?? Enumerable.Empty<NodeViewModel>(); }
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Event raised when connectors are added to the node.
		/// </summary>
		void InputConnectors_ItemsAdded(object sender, CollectionItemsChangedEventArgs e)
		{
			foreach (ConnectorViewModel connector in e.Items)
			{
				connector.ParentNode = this;
				connector.Type = ConnectorType.Input;
			}
		}

		/// <summary>
		/// Event raised when connectors are removed from the node.
		/// </summary>
		void InputConnectors_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
		{
			foreach (ConnectorViewModel connector in e.Items)
			{
				connector.ParentNode = null;
				connector.Type = ConnectorType.Undefined;
			}
		}

		/// <summary>
		/// Event raised when connectors are added to the node.
		/// </summary>
		void OutputConnectors_ItemsAdded(object sender, CollectionItemsChangedEventArgs e)
		{
			foreach (ConnectorViewModel connector in e.Items)
			{
				connector.ParentNode = this;
				connector.Type = ConnectorType.Output;
			}
		}

		/// <summary>
		/// Event raised when connectors are removed from the node.
		/// </summary>
		void OutputConnectors_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
		{
			foreach (ConnectorViewModel connector in e.Items)
			{
				connector.ParentNode = null;
				connector.Type = ConnectorType.Undefined;
			}
		}

		internal void UpdateChildNodes(Action<NodeViewModel> action)
		{
			foreach (var child in ChildNodes)
			{
				action(child);
			}
		}

		bool AncestorsSelected(INetworkEntity entitySelected)
		{
			return entitySelected.GetAncestors().Any(ancestor =>
				{
					var node = NetworkViewModel.GetNodeForEntity(ancestor);
					return node != null && node.IsSelected;
				});
		}

		internal NodeViewModel GetParent()
		{
			var parent = entity.Parent as INetworkEntity;
			return parent != null ? NetworkViewModel.GetNodeForEntity(parent) : null;
		}

		#endregion Private Methods

		#region Property Notifications

		protected override IEnumerable<string> GetWrappingProperties(string propertyName)
		{
			return NetworkViewModel?.GetWrappedPropertiesForNodeViewModel(propertyName) ?? Enumerable.Empty<string>();
		}

		#endregion

		public override int GetHashCode()
		{
			return Entity.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var node = obj as NodeViewModel;
			if (node != null)
			{
				return node.Equals(this);
			}
			else
			{
				return base.Equals(obj);
			}
		}

		public bool Equals(NodeViewModel other)
		{
			return other.Entity.Equals(Entity);
		}

		public class ProgressBar
		{
			public Color background;
			public double percent;
			public double opacity;

			public ProgressBar(Color color, double percent, double opacity = 1)
			{
				background = color;
				this.percent = percent;
				this.opacity = opacity;
			}
		}
	}
}
