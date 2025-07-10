using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;

namespace CargoWise.NetworkVisualisation.Business
{
	public class Entity : IProposedNetworkEntity, IScheduledNetworkEntity, IDiagramEntity
	{
		public Entity()
		{
			relationships = new List<IEntityRelationship>();
			ShapeType = ShapeTypes.Shape;

			IsDiagramScaled = false;

			Scale = 60;
			ResolutionIncrement = 60;
			ScaleUnitPixelSize = 100;
			DiagramChannels = Array.Empty<IDiagramChannel>();
		}

		readonly List<IEntityRelationship> relationships;

		public string Name { get; set; }
		public string JobNumber { get; set; }

		public ScrollPositionStates ScrollPositions { get; }

		public string JobName
		{
			get { return jobName; }
			set
			{
				jobName = value;
				OnPropertyChanged();
			}
		}
		string jobName;

		public bool JobName_ReadOnly { get; set; }
		public string Description { get; set; }
		public bool IsNonScheduled { get; set; }
		public string AdditionalDetail { get; set; }
		public string AdditionalDetailTooltip { get; set; }

		public string Notes { get; set; }
		public string CompletionCriteria { get; set; }

		public string EstimateSummary { get; set; }
		public WorkStatus Status { get; set; }

		public EntityState EntityState
		{
			get { return entityState; }
			set
			{
				if (value.HasFlag(EntityState.Fixed))
				{
					fixedX = X;
					fixedY = Y;
				}
				else
				{
					fixedX = null;
					fixedY = null;
				}
				entityState = value;
			}
		}
		EntityState entityState;

		public bool IsOnCriticalPath { get; set; }
		public bool CanUnlinkEntity { get; set; }
		public bool CanDeleteUnderlyingEntity { get; set; }
		public bool IsCalculationSuspended { get; set; }

		public bool IsHandledByVisualiser
		{
			get { return true; }
		}

		public bool CanCreateRelationship(INetworkEntity other)
		{
			return true;
		}

		Guid INetworkEntity.EntityPK { get; } = Guid.NewGuid();

		#region Coordinates

		public virtual double X
		{
			get { return fixedX ?? (x + GetParentXOffset()); }
			set
			{
				x = value - GetParentXOffset();
				IsPositioned = true;
			}
		}
		double x;
		double? fixedX;

		double GetParentXOffset()
		{
			var parentEntity = Parent as INetworkEntity;
			return parentEntity != null ? parentEntity.X : 0d;
		}

		public virtual double Y
		{
			get { return fixedY ?? (y + GetParentYOffset()); }
			set
			{
				y = value - GetParentYOffset();
				IsPositioned = true;
			}
		}
		double y;
		double? fixedY;

		double GetParentYOffset()
		{
			var parentEntity = Parent as INetworkEntity;
			return parentEntity != null ? parentEntity.Y : 0d;
		}

		public double Width { get; set; }
		public double Height { get; set; }

		#endregion

		public int ZIndex { get; set; }

		public int CornerRadius { get; set; }
		public Color BackColor { get; set; }
		public Color ForeColor { get; set; }

		public IObservableReloadableCollection<IAffinity> AvailableAffinities
		{
			get { return availableAffinities ?? (availableAffinities = new ImpObservableCollection<IAffinity>()); }
		}
		IObservableReloadableCollection<IAffinity> availableAffinities;

		public IObservableReloadableCollection<IAffinity> AppliedAffinities
		{
			get { return appliedAffinities ?? (appliedAffinities = new ImpObservableCollection<IAffinity>()); }
		}
		IObservableReloadableCollection<IAffinity> appliedAffinities;

		public NetworkActions SupportedActions
		{
			get => supportedActions;
			set
			{
				supportedActions = value;
				OnPropertyChanged(nameof(SupportedActions));
			}
		}
		NetworkActions supportedActions;

		public string ShapeType { get; set; }

		public IProposedNetworkEntity Parent
		{
			get { return parent; }
			set
			{
				RemoveChild(parent, this);
				parent = value;
				AddChild(parent, this);
			}
		}
		IProposedNetworkEntity parent;

		public IEnumerable<INetworkEntity> Children
		{
			get { return children; }
		}
		readonly List<INetworkEntity> children = new List<INetworkEntity>();

		public void AddChildEntity(INetworkEntity childEntity)
		{
			children.Add(childEntity);
		}

		public void AddChildEntities(IEnumerable<INetworkEntity> childEntities)
		{
			children.AddRange(childEntities);
		}

		public IDisposable SuspendSettingHasChanges()
		{
			return null;
		}

		public INetworkPin Pin
		{
			get
			{
				if (EntityState.HasFlag(EntityState.Fixed))
				{
					return new NetworkPin { Owner = GetRoot() };
				}

				return null;
			}
		}

		INetworkEntity GetRoot()
		{
			var root = (INetworkEntity)this;
			while (root.Parent != null)
			{
				root = (INetworkEntity)root.Parent;
			}
			return root;
		}

		#region NetworkPin

		class NetworkPin : INetworkPin
		{
			public INetworkEntity Owner { get; set; }
		}

		#endregion

		static void RemoveChild(IProposedNetworkEntity parent, IProposedNetworkEntity child)
		{
			var childEntity = child as Entity;
			var parentEntity = parent as Entity;
			if (childEntity != null && parentEntity != null)
			{
				parentEntity.children.Remove(childEntity);
			}
		}

		static void AddChild(IProposedNetworkEntity parent, IProposedNetworkEntity child)
		{
			var childEntity = child as Entity;
			var parentEntity = parent as Entity;
			if (childEntity != null && parentEntity != null)
			{
				parentEntity.children.Add(childEntity);
			}
		}

		public bool IsLeafEntity
		{
			get { return children.Count == 0; }
		}

		public bool HasLinkedEntity
		{
			get;
		}

		public bool IsLinkedToWorkflow
		{
			get;
		}

		public bool IsDiagramWithRibbon
		{
			get;
#if DEBUG
			set;
#endif
		}

		public bool IsPositioned
		{
			get; set;
		}

		public IList<IEntityRelationship> Links
		{
			get { return relationships; }
		}

		IEnumerable<IEntityRelationship> IProposedNetworkEntity.Links
		{
			get { return relationships; }
		}

		public IEnumerable<IEntityRelationship> PreRequisiteLinks
		{
			get { return Links.Where(l => l.To == this); }
		}

		public IEnumerable<IEntityRelationship> PostRequisiteLinks
		{
			get { return Links.Where(l => l.From == this); }
		}

		public bool HasNotifications { get; set; }

		public IEnumerable<IEntityNotification> EntityNotifications
		{
			get { return Notifications; }
		}

		public List<EntityNotification> Notifications
		{
			get { return entityNotifications ?? (entityNotifications = new List<EntityNotification>()); }
		}
		List<EntityNotification> entityNotifications;

		public void CreateAffinityLink(INetworkEntity networkEntity, IAffinity affinity)
		{
			networkEntity.AvailableAffinities.Remove(affinity);
			networkEntity.AppliedAffinities.Add(affinity);
		}
		public void RemoveAffinityLink(INetworkEntity networkEntity, IAffinity affinity)
		{
			networkEntity.AppliedAffinities.Remove(affinity);
			networkEntity.AvailableAffinities.Add(affinity);
		}

		public bool IsDiagramScaled { get; set; }
		public int Scale { get; set; }
		public int ResolutionIncrement { get; set; }
		public int ScaleUnitPixelSize { get; set; }
		public bool IsDiagramSurfaceFixed { get; set; }
		public bool IsDeleted { get; set; }
		public bool ShouldShowNonScheduledSection { get; set; }
		public double NonScheduledSectionWidth { get; set; }
		public bool ShapeInspectorVisible { get; set; }

		public IEnumerable<IDiagramChannel> DiagramChannels { get; set; }

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsStartable
		{
			get { return Status == WorkStatus.Startable; }
		}

		public string StatusDescription
		{
			get { return Status.ToString(); }
		}

		public string StatusName
		{
			get { return StatusDescription; }
		}

		public bool IsSameEntity(IProposedNetworkEntity other)
		{
			return Equals(other);
		}

		public bool Equals(INetworkEntity other)
		{
			return base.Equals(other);
		}

		#region INetworkEntityWithChildren Members

		public IObservableReloadableCollection<IProposedNetworkEntity> HiddenEntities
		{
			get { return hiddenEntities ?? (hiddenEntities = new ImpObservableCollection<IProposedNetworkEntity>()); }
		}

		IObservableReloadableCollection<IProposedNetworkEntity> hiddenEntities;

		public IObservableReloadableCollection<IEntityRelationship> HiddenRelationships
		{
			get { return hiddenRelationships ?? (hiddenRelationships = new ImpObservableCollection<IEntityRelationship>()); }
		}

		IObservableReloadableCollection<IEntityRelationship> hiddenRelationships;

		public bool CanHaveChildren { get; set; }

		#endregion

		public bool IsCriticalPath { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int ExplicitDurationMinutes { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int RemainingDurationMinutes { get; set; }

		public DateTime ScheduledStartTimeLocal { get; set; }

		public DateTime ScheduledEndTimeLocal { get; set; }

		public void SwapNonScheduledState()
		{
			IsNonScheduled = !IsNonScheduled;
		}
	}
}
