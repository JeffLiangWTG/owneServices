using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using MissingFrom.Net;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	/// <summary>
	/// This class is a wrapper for behaviour on BMNCNShape.
	/// 
	/// Ideally, we should move as much behaviour from dbo.BMNCNShape to here as possible,
	/// however our extensive unit tests make that tricky without very large changes.
	/// So until someone else decides that finishing the migration is important,
	/// (And removes the interfaces this implements from dbo.BMNCNShape)
	/// this class is in a sortof leaky half-state.
	/// </summary>
	[DebuggerDisplay("ShapeNetworkEntity '{DisplayName}'")]
	public class ShapeNetworkEntity : NonPersistentBusinessObject, IShapeNetworkEntity
	{
		readonly WeakEventManager manager = new WeakEventManager();

		public ShapeNetworkEntity(BMNCNShape shape, JobNetwork network, NetworkEntityCollection entityCollection)
			: base(shape.Factory)
		{
			this.network = network;
			this.shape = shape;
			this.shape.IsTopLevel = false;
			this.entityCollection = entityCollection;
			// Push shape changed events to here without memory leaks.
			manager.AddWeakEventListener(shape, OnPropertyOfChildChanged);
			this.NotificationsChanged += ShapeNetworkEntity_NotificationsChanged;
			ShapePropertiesStrategy = GetAndSetShapePropertiesStrategyProvider.GetShapePropertiesStrategy(shape);
		}

		void ShapeNetworkEntity_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			OnPropertyChanged(nameof(EntityState));
			OnPropertyChanged(nameof(HasNotifications));
		}

		#region State

		readonly JobNetwork network;
		readonly BMNCNShape shape;
		readonly NetworkEntityCollection entityCollection;
		readonly GetAndSetShapePropertiesStrategyBase ShapePropertiesStrategy;

		INetworkEntity InnerEntity => shape;
		ILinkEntity InnerLinkEntity => shape;
		IBranchDepartmentProvider InnerBranchDepartmentProvider => shape;
		public BMNCNShape Shape => shape;

		/// <summary>
		/// Used to safely evaluate a flag on the BMNCNShape for this Entity when it has uncertain existence.
		/// Returns false in case of doubt.
		/// </summary>
		public bool EvaluateFlagOnShapeSafely(Func<BMNCNShape, bool> toEvaluate)
		{
			if (shape.IsRowDeletedOrDetachedOrNull || shape.IsDeleted || shape.IsDeleting)
			{
				return false;
			}

			return toEvaluate(shape);
		}

		/// <summary>
		/// Used to safely access the BMNCNShape for this Entity when it has uncertain existence.
		/// Returns null in case of doubt.
		/// If you're unsure whether to use this or property Shape, use this.
		/// </summary>
		public BMNCNShape ShapeSafe
		{
			get
			{
				if (shape.IsRowDeletedOrDetachedOrNull || shape.IsDeleted || shape.IsDeleting)
				{
					return null;
				}

				return shape;
			}
		}

		public JobNetwork Network => network;

		#endregion

		#region BusinessObject Overrides

		protected override void SetPKAndDefaults()
		{
			// not needed
		}

		protected override void RegisterCustomBusinessObjectAsChild()
		{
			base.RegisterCustomBusinessObjectAsChild();

			RegisterEditableChildObject(Shape);
		}

		protected override IDisposable SuspendSettingHasChangesCore()
		{
			return shape.SuspendSettingHasChanges();
		}

		#endregion

		#region Validation

		/// <summary>
		/// Related child bizos that should be validated on closing a Shape Properties form
		/// Bizos included here should be directly editable through the form such that validation errors can be resolved
		/// </summary>
		List<BusinessObject> ChildObjectsToValidate
		{
			get
			{
				var childObjectsToValidate = new List<BusinessObject>();

				if (DiagramChannels != null)
				{
					childObjectsToValidate.AddRange(DiagramChannels.OfType<BusinessObject>());
				}

				if (LevelingRules != null)
				{
					childObjectsToValidate.AddRange(LevelingRules);
				}

				if (Shape != null)
				{
					childObjectsToValidate.Add(Shape);

					if (Shape.ScheduleBizo != null)
					{
						childObjectsToValidate.Add(Shape.ScheduleBizo);
					}

					if (Shape.ProcessHeader != null)
					{
						childObjectsToValidate.Add(Shape.ProcessHeader);
					}
				}

				return childObjectsToValidate;
			}
		}

		public bool RunValidationAndCheckErrors(ICollection<string> errorsToExclude)
		{
			RunPreSaveValidationExcludingChildren();

			return HasValidationErrors(this, errorsToExclude);
		}

		public bool RunValidationAndCheckErrorsForChildObjects(ICollection<string> errorsToExclude)
		{
			var childObjectsToValidate = ChildObjectsToValidate;

			foreach (var childBizo in childObjectsToValidate)
			{
				childBizo.RunPreSaveValidationExcludingChildren();
			}

			return childObjectsToValidate.Any(childBizo => HasValidationErrors(childBizo, errorsToExclude));
		}

		static bool HasValidationErrors(BusinessObject bizo, ICollection<string> errorsToExclude)
		{
			var errors = bizo.Notifications.GetErrors();

			return errors.Any(error => !errorsToExclude.Any(errorToExclude => error.Message.Contains(errorToExclude)));
		}

		#endregion

		#region INetworkEntity

		#region Additional Detail

		public bool IsNonScheduled
		{
			get => ShapeSafe != null && ShapeSafe.IsNonScheduled;
			set
			{
				if (ShapeSafe != null)
				{
					ShapeSafe.IsNonScheduled = value;
				}
			}
		}

		public void SwapNonScheduledState()
		{
			var width = Width;
			var x = X;

			IsNonScheduled = !IsNonScheduled;

			Width = width;
			X = x;
		}

		public string AdditionalDetail
		{
			get
			{
				var result = new StringBuilder();
				result.Append(GetSchedulingText());

				var shapeNotes = shape.ShapeNotes;
				if (result.Length > 0 && !string.IsNullOrWhiteSpace(shapeNotes))
				{
					result.AppendLine();
				}

				result.Append(shapeNotes);

				return result.ToString();
			}
		}

		public string AdditionalDetailTooltip
		{
			get
			{
				return null;
			}
		}

		string GetSchedulingText()
		{
			var result = new StringBuilder();

			var processHeader = ProcessHeader;
			var plannedDurationHours = shape.ExplicitDurationMinutes / 60.0;

			var schedule = Schedule;

			if (plannedDurationHours > 0)
			{
				result.Append(Res.GetString("077312da-b3c8-4f4c-8038-e80d5e4ba64c", "Planned Dur.: {0}", GetFriendlyApproximateWorkingHoursText(plannedDurationHours)));
				if (!IsBufferShape && schedule != null || shape.IsLinkedToRealEntity)
				{
					result.AppendLine();
				}
			}

			if (shape.IsLinkedToRealEntity)
			{
				var taskHours = processHeader != null ? (double)processHeader.TotalRelevantEstimatedHours : 0.0;
				result.Append(Res.GetString("e7c858e0-5c2e-45ce-8286-fb2a4851ef46", "Std. Est.: {0}", taskHours == 0 ? Res.GetString("1f16185e-9abd-47c8-9942-dedc9390bfff", "0 hours") : GetFriendlyApproximateWorkingHoursText(taskHours)));

				if (schedule != null)
				{
					result.Append("   ");
				}
			}

			if (!IsBufferShape && schedule != null)
			{
				result.Append(Res.GetString("f30bca67-6f1c-4bc7-a16a-e740c2906ac0", "Float: {0}", schedule.FloatHours == 0 ? Res.GetString("83050ad3-b6ee-4516-a094-7cacd2374efe", "none") : TimeSpan.FromHours((double)schedule.FloatHours).ToFriendlyTimeString()));

				var deliveryDateThreat = schedule.DeliveryDateThreat;
				if (deliveryDateThreat != null)
				{
					var threatText = deliveryDateThreat.FloatConsumption > schedule.FloatHours
						? Res.GetString("a6508055-27cb-4af4-b243-0338c57e3b57", "WARNING: delivery date threatened")
						: Res.GetString("fcd80bc0-d7c5-42d9-bb5c-5633bf24781a", "Float consumption: {0}", TimeSpan.FromHours((double)deliveryDateThreat.FloatConsumption).ToFriendlyTimeString());

					result.AppendLine();
					result.Append(threatText);
				}
			}

			return result.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Utilities.Round doesn't support doubles")]
		static string GetFriendlyApproximateWorkingHoursText(double workingHours)
		{
			var totalDays = Math.Round(workingHours / BMConstants.WorkingHoursPerDay, 2); // Utilities.Round doesn't support doubles
			var totalWeeks = Math.Round(totalDays / BMConstants.WorkingDaysPerWeek, 2); // Utilities.Round doesn't support doubles

			if (totalWeeks >= 1)
			{
				return totalWeeks == 1.0
					? Res.GetString("abe57e7e-4e2d-4e57-9552-6dc80ec30c2f", "{0} week", totalWeeks)
					: Res.GetString("907595c9-8c8e-48e9-8dfe-224949979a5a", "{0} weeks", totalWeeks);
			}
			else if (totalDays >= 1)
			{
				return totalDays == 1
					? Res.GetString("9a46e67b-52f7-436d-bd37-8fbc707b3d0c", "{0} day", totalDays)
					: Res.GetString("95598ce1-7c3f-43d1-84e9-977dcc214f69", "{0} days", totalDays);
			}
			else
			{
				return TimeSpan.FromHours(workingHours).ToFriendlyTimeString();
			}
		}

		public JobNetworkNodeEstimateDetails GetEstimateDetails()
		{
			if (!shape.IsDefaultDiagramChild && Root.Shape.DisplayCompletenessIndicator && shape.LinkedEntity != null)
			{
				var remainingEstimate = shape.RemainingEstimateHoursIncludingChildren;
				if (((INetworkEntity)shape).Status != WorkStatus.Complete || remainingEstimate > 0)
				{
					var totalEstimate = shape.TotalNonCancelledEstimatedHoursIncludingChildren;
					if (totalEstimate > 0)
					{
						var completedEstimate = totalEstimate - remainingEstimate;
						return new JobNetworkNodeEstimateDetails(shape, totalEstimate, completedEstimate);
					}
				}
			}

			return null;
		}

		#endregion;

		#region Entity State

		IEnumerable<INotificationProvider> ChildNotificationProviders
		{
			get
			{
				foreach (var attachment in DependencyAttachments)
				{
					yield return attachment;
				}
				foreach (var rule in AppliedLevelingRuleViolations)
				{
					yield return rule;
				}
			}
		}

		protected override bool HasNotificationsCore()
		{
			return base.HasNotificationsCore() || ChildNotificationProviders.Any(s => s.HasNotifications());
		}

		public new bool HasNotifications => HasNotifications();

		public EntityState EntityState
		{
			get
			{
				var state = EntityState.None;

				var notificationProviders = new INotificationProvider[] { shape, this }.Concat(ChildNotificationProviders);

				foreach (var provider in notificationProviders)
				{
					if (provider.HasErrors())
					{
						state |= EntityState.HasErrors;
					}

					if (provider.HasWarnings())
					{
						state |= EntityState.HasWarnings;
					}

					if (provider.HasMessageErrors())
					{
						state |= EntityState.HasMessages;
					}
				}

				if (IsPinned)
				{
					state |= EntityState.Fixed;
				}

				if (!shape.Active)
				{
					state |= EntityState.Inactive;
				}

				if (!shape.IsAnnotation)
				{
					if (IsApproved)
					{
						state |= EntityState.Approved | EntityState.Fixed;
					}
					else
					{
						if (Root.IsApproved)
						{
							state |= EntityState.NotApproved;
						}
					}
				}

				return state;
			}
		}

		public bool IsStoringMinutesOnHorizontalAxis
		{
			get
			{
				if (IsAnnotation || IsNonScheduled)
				{
					return false;
				}

				var rootEntity = Root;
				return rootEntity.IsScaled && rootEntity.Scale.IsValid;
			}
		}

		public bool ShouldStoreWidthOnSchedule
		{
			get { return IsStoringMinutesOnHorizontalAxis && CanHaveSchedule; }
		}

		#endregion

		#region Affinities

		IObservableReloadableCollection<IAffinity> INetworkEntity.AppliedAffinities => AppliedAffinities;
		IObservableReloadableCollection<IAffinity> INetworkEntity.AvailableAffinities => AvailableAffinities;

		AppliedAffinitiesCollection AppliedAffinities
		{
			get
			{
				if (appliedAffinities == null)
				{
					appliedAffinities = new AppliedAffinitiesCollection(this);
				}
				return appliedAffinities;
			}
		}

		AppliedAffinitiesCollection appliedAffinities;

		AvailableAffinitiesCollection AvailableAffinities
		{
			get
			{
				if (availableAffinities == null)
				{
					availableAffinities = new AvailableAffinitiesCollection(this);
				}
				return availableAffinities;
			}
		}

		AvailableAffinitiesCollection availableAffinities;

		internal HashSet<BMNCNLevelingRule> AppliedLevelingRuleViolations => AppliedLevelingRuleViolationsAtIndex.Select(l => l.Item2).ToHashSet();

		internal List<Tuple<int, BMNCNLevelingRule>> AppliedLevelingRuleViolationsAtIndex
		{
			get
			{
				if (appliedLevelingRuleViolationsAtIndex == null)
				{
					appliedLevelingRuleViolationsAtIndex = new List<Tuple<int, BMNCNLevelingRule>>();
				}
				return appliedLevelingRuleViolationsAtIndex;
			}
		}

		List<Tuple<int, BMNCNLevelingRule>> appliedLevelingRuleViolationsAtIndex;

		#endregion

		IEnumerable<INetworkEntity> INetworkEntity.Children => Children;

		IEnumerable<IEntityNotification> INetworkEntity.EntityNotifications
		{
			get
			{
				var set = DependencyAttachments.Cast<INotificationProvider>().
					Concat(AppliedLevelingRuleViolations.Cast<INotificationProvider>()).
					Append(this);

				foreach (var error in set.SelectMany(n => n.GetErrors()))
				{
					yield return new EntityNotification(EntityNotifcationType.Error, error.Message, Name);
				}
				foreach (var warning in set.SelectMany(n => n.GetWarnings()))
				{
					yield return new EntityNotification(EntityNotifcationType.Warning, warning.Message, Name);
				}
				foreach (var message in set.SelectMany(n => n.GetMessageErrors()))
				{
					yield return new EntityNotification(EntityNotifcationType.Message, message.Message, Name);
				}

				foreach (var inner in InnerEntity.EntityNotifications)
				{
					yield return inner;
				}
			}
		}

		Guid INetworkEntity.EntityPK => InnerEntity.EntityPK;
		bool INetworkEntity.IsLeafEntity => InnerEntity.IsLeafEntity;
		bool INetworkEntity.HasLinkedEntity => InnerEntity.HasLinkedEntity;
		bool INetworkEntity.IsLinkedToWorkflow => InnerEntity.IsLinkedToWorkflow;
		bool INetworkEntity.IsDiagramWithRibbon => InnerEntity.IsDiagramWithRibbon;
		bool INetworkEntity.IsPositioned => InnerEntity.IsPositioned;
		INetworkPin INetworkEntity.Pin => PinProvider.Pin;
		string INetworkEntity.ShapeType => InnerEntity.ShapeType;
		NetworkActions INetworkEntity.SupportedActions => ShapeSafe != null ? InnerEntity.SupportedActions : new NetworkActions();

		public int CornerRadius
		{
			get { return InnerEntity.CornerRadius; }
			set { InnerEntity.CornerRadius = value; }
		}

		public Color ForeColor
		{
			get { return InnerEntity.ForeColor; }
			set
			{
				InnerEntity.ForeColor = value;
				OnPropertyChanged(nameof(NodeViewModel.CompletionCriteriaTextColor));
				OnPropertyChanged(nameof(NodeViewModel.ForegroundColor));
				Network.Refresh(RefreshType.TextColorUpdated);
			}
		}

		public int ZIndex
		{
			get { return (int)Shape.ZIndex; }
			set { Shape.ZIndex = value; }
		}

		public Color BackColor
		{
			get { return InnerEntity.BackColor; }
			set
			{
				InnerEntity.BackColor = value;
				Network.Refresh(RefreshType.RedrawDiagram);
			}
		}

		string INetworkEntity.Notes
		{
			get { return InnerEntity.Notes; }
			set { InnerEntity.Notes = value; }
		}

		bool INetworkEntity.CanCreateRelationship(INetworkEntity other)
		{
			return InnerEntity.CanCreateRelationship(other);
		}

		#endregion

		#region IBranchDepartmentProvider

		IGlbBranch IBranchDepartmentProvider.GetBranch(BusinessObjectFactory factory)
		{
			return InnerBranchDepartmentProvider.GetBranch(factory);
		}

		IGlbDepartment IBranchDepartmentProvider.GetDepartment(BusinessObjectFactory factory)
		{
			return InnerBranchDepartmentProvider.GetDepartment(factory);
		}

		#endregion

		#region IScheduledNetworkEntity

		public bool IsCriticalPath
		{
			get { return Schedule != null && Schedule.IsCriticalPath; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		int IScheduledNetworkEntity.ExplicitDurationMinutes
		{
			get { return IsDiagramScaled ? (int)shape.ExplicitDurationMinutes : ProcessHeader == null ? 0 : (int)(ProcessHeader.ImplicitDurationHours * 60); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		int IScheduledNetworkEntity.RemainingDurationMinutes
		{
			get { return ProcessHeader != null ? (int)(ProcessHeader.RemainingEstimateHours * 60) : 0; }
		}

		DateTime IScheduledNetworkEntity.ScheduledStartTimeLocal => shape.ScheduledStartTimeLocal.IsValid ? shape.ScheduledStartTimeLocal.ToDateTime() : default(DateTime);

		DateTime IScheduledNetworkEntity.ScheduledEndTimeLocal => shape.ScheduledFinishTimeLocal.IsValid ? shape.ScheduledFinishTimeLocal.ToDateTime() : default(DateTime);

		#endregion

		#region IDiagramEntity

		public bool IsDiagramScaled => shape.IsScaled;

		public int ScaleUnitPixelSize => shape.ScaleUnitPixelSize;

		int IDiagramEntity.ResolutionIncrement => Convert.ToInt32(shape.ResolutionIncrement.GetMinutesFromDateTimeSpan());
		int IDiagramEntity.Scale => Convert.ToInt32(shape.Scale.GetMinutesFromDateTimeSpan());

		public bool IsDiagramSurfaceFixed => !shape.IsDeleted && (!shape.IsDiagram || (shape.ScheduledStartTimeLocal.IsValid && shape.ScheduledFinishTimeLocal.IsValid));

		[ResourceStringData("BMNCNShape.ShouldShowNonScheduledSection", Caption = "Show Non-Scheduled Section", FullDescription = "When ticked, a section is shown to the right of the main diagram surface which contains shapes that are not associated with a scheduled start or finish time.")]
		public ZBool ShouldShowNonScheduledSection
		{
			get => ShapeAsRootDiagram?.ShouldShowNonScheduledSection ?? ZBool.False;
			set
			{
				if (ShapeAsRootDiagram != null)
				{
					ShapeAsRootDiagram.ShouldShowNonScheduledSection = value;
				}
			}
		}

		public ZPropertyInfo ShouldShowNonScheduledSectionInfo => GetWrappedZPropertyInfo(nameof(ShouldShowNonScheduledSection), sender => ShapeAsRootDiagram?.ShouldShowNonScheduledSectionInfo);

		bool IDiagramEntity.ShouldShowNonScheduledSection
		{
			get => ShouldShowNonScheduledSection;
			set => ShouldShowNonScheduledSection = value;
		}

		double IDiagramEntity.NonScheduledSectionWidth
		{
			get => ShapeAsRootDiagram != null && ShapeAsRootDiagram.ShouldShowNonScheduledSection ? (double)ShapeAsRootDiagram.NonScheduledSectionWidth : 0d;
			set
			{
				if (ShapeAsRootDiagram != null && ShapeAsRootDiagram.ShouldShowNonScheduledSection)
				{
					ShapeAsRootDiagram.NonScheduledSectionWidth = value;
				}
			}
		}

		public IEnumerable<IDiagramChannel> DiagramChannels => GetDiagramChannels();

		IEnumerable<IDiagramChannel> GetDiagramChannels()
		{
			var diagram = ShapeAsRootDiagram;

			if (diagram != null && diagram.SupportsChanneling)
			{
				var channels = diagram.Channels.OrderBy(x => x.BNL_Sequence).ToList<IDiagramChannel>();

				if (channels.Any())
				{
					channels.Add(new NonChanneledChannel());
				}

				return channels;
			}

			return Enumerable.Empty<IDiagramChannel>();
		}

		public IEnumerable<BMNCNLevelingRule> LevelingRules
		{
			get
			{
				var diagram = ShapeAsRootDiagram;

				if (diagram != null)
				{
					return diagram.LevelingRules;
				}

				return null;
			}
		}

		class NonChanneledChannel : IDiagramChannel
		{
			public string Name => Res.GetString("285d0018-03dc-48ef-b8a0-c28f9a87df7a", "Non-channeled");

			public int Height => CCPMConstants.DefaultChannelHeight;

			public Color Color => Color.Empty;
		}

		#region ScrollPositions

		ScrollPositionStates IDiagramEntity.ScrollPositions
		{
			get
			{
				if (IsScaled && IsRoot)
				{
					if (shape.ScrollPosition == ScrollPositionList.Codes.Current)
					{
						return ScrollPositionStates.Current;
					}
					else if (shape.ScrollPosition == ScrollPositionList.Codes.First ||
						(shape.ScrollPosition == ScrollPositionList.Codes.Default && EntityState.HasFlag(EntityState.Approved)))
					{
						return ScrollPositionStates.FirstOpenShape;
					}
					else if (shape.ScrollPosition == ScrollPositionList.Codes.Start ||
						(shape.ScrollPosition == ScrollPositionList.Codes.Default && !EntityState.HasFlag(EntityState.Approved)))
					{
						return ScrollPositionStates.Start;
					}
				}

				return ScrollPositionStates.None;
			}
		}

		#endregion

		#region Affinities

		void IDiagramEntity.CreateAffinityLink(INetworkEntity networkEntity, IAffinity affinity)
		{
			if (networkEntity != null && affinity != null)
			{
				var link = ShapeAffinityLinks.AddNew();
				link.ShapeAffinityPK = affinity.AffinityPK;
				link.ShapePK = networkEntity.EntityPK;
				ReloadWithAffinityChange(networkEntity);
			}
		}

		void IDiagramEntity.RemoveAffinityLink(INetworkEntity networkEntity, IAffinity affinity)
		{
			if (networkEntity != null && affinity != null)
			{
				var existingLinks =
					from ShapeAffinityLink link in ShapeAffinityLinks
					where link.IsApplied
					where link.ShapeAffinityPK == affinity.AffinityPK
					where link.ShapePK == networkEntity.EntityPK
					select link;

				foreach (var link in existingLinks)
				{
					link.IsApplied = ZBool.False;
				}
				ReloadWithAffinityChange(networkEntity);
			}
		}

		static void ReloadWithAffinityChange(INetworkEntity entity)
		{
			entity.AppliedAffinities?.Reload();
			entity.AvailableAffinities?.Reload();
		}

		public ShapeAffinityLinkCollection ShapeAffinityLinks => shape.ShapeAffinityLinks;

		public ShapeAffinityCollection ShapeAffinities => shape.ShapeAffinities;

		#endregion

		#region EntityDurationMinutes

		[ReadOnly(true)] // Duration is set by moving or resizing shapes.
		public ZInt EntityDurationMinutes
		{
			get { return shape.ExplicitDurationMinutes; }
			set { shape.ExplicitDurationMinutes = value; }
		}

		#endregion

		public bool ShapeInspectorVisible
		{
			get
			{
				var diagram = ShapeAsRootDiagram;

				if (diagram != null)
				{
					return diagram.ShapeInspectorVisible;
				}

				return false;
			}

			set
			{
				var diagram = ShapeAsRootDiagram;

				if (diagram != null)
				{
					diagram.ShapeInspectorVisible = value;
				}
			}
		}

		#endregion

		#region INetworkEntityWithChildren

		public bool CanHaveChildren => shape.CanHaveChildren;

		// This property is public so that the JobNetwork can do dirty things to it.
		public IObservableReloadableCollection<IEntityRelationship> HiddenRelationships => hiddenRelationships ?? (hiddenRelationships = new HiddenRelationshipsCollection(this));
		IObservableReloadableCollection<IEntityRelationship> hiddenRelationships;

		// This property is public so that the JobNetwork can do dirty things to it.
		public IObservableReloadableCollection<IProposedNetworkEntity> HiddenEntities => hiddenEntities ?? (hiddenEntities = new HiddenShapesCollection(this));
		IObservableReloadableCollection<IProposedNetworkEntity> hiddenEntities;

		#endregion

		#region IProposedNetworkEntity

		public bool IsOnCriticalPath
		{
			get
			{
				if (Schedule != null)
				{
					return Schedule.IsCriticalPath;
				}
				else
				{
					return shape.IsOnCriticalPath;
				}
			}
		}

		bool IProposedNetworkEntity.CanDeleteUnderlyingEntity => InnerEntity.CanDeleteUnderlyingEntity;
		bool IProposedNetworkEntity.CanUnlinkEntity => InnerEntity.CanUnlinkEntity;
		string IProposedNetworkEntity.Description => InnerEntity.Description;
		string IProposedNetworkEntity.EstimateSummary => InnerEntity.EstimateSummary;
		bool IProposedNetworkEntity.IsStartable => InnerEntity.IsStartable;
		bool IProposedNetworkEntity.JobName_ReadOnly => InnerEntity.JobName_ReadOnly;
		string IProposedNetworkEntity.JobNumber => InnerEntity.JobNumber;
		IEnumerable<IEntityRelationship> IProposedNetworkEntity.Links => Links;
		IProposedNetworkEntity IProposedNetworkEntity.Parent => Owner;
		IEnumerable<IEntityRelationship> IProposedNetworkEntity.PostRequisiteLinks => PostRequisiteLinks;
		IEnumerable<IEntityRelationship> IProposedNetworkEntity.PreRequisiteLinks => PreRequisiteLinks;
		WorkStatus IProposedNetworkEntity.Status => InnerEntity.Status;
		string IProposedNetworkEntity.StatusDescription => InnerEntity.StatusDescription;
		string IProposedNetworkEntity.StatusName => InnerEntity.StatusName;

		string IProposedNetworkEntity.CompletionCriteria
		{
			get { return InnerEntity.CompletionCriteria; }
			set { InnerEntity.CompletionCriteria = value; }
		}

		string IProposedNetworkEntity.JobName
		{
			get { return InnerEntity.JobName; }
			set { InnerEntity.JobName = value; }
		}

		string IProposedNetworkEntity.Name
		{
			get { return Name; }
			set { Name = value; }
		}

		bool IProposedNetworkEntity.IsSameEntity(IProposedNetworkEntity other)
		{
			return InnerEntity.IsSameEntity(other);
		}

		#endregion

		#region INetworkActionResult

		bool INetworkActionResult.IsHandledByVisualiser => InnerEntity.IsHandledByVisualiser;

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyOfChildChanged(object sender, PropertyChangedEventArgs e)
		{
			if (sender != this)
			{
				OnPropertyChanged(e.PropertyName);
			}
		}

		protected internal void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (IsDelayingPropertyChanged)
			{
				propertiesUpdatedDuringDelay.Add(propertyName);
			}
			else if (PropertyChanged != null && !IsDeleted)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#region Delay Property Changed

		bool IsDelayingPropertyChanged
		{
			get { return propertiesUpdatedDuringDelay != null; }
		}

		public IDisposable DelayPropertyChanged()
		{
			var propertiesUpdated = new HashSet<string>();
			propertiesUpdatedDuringDelay = propertiesUpdated;

			return new DisposableAction(() =>
			{
				propertiesUpdatedDuringDelay = null;

				foreach (var property in propertiesUpdated)
				{
					OnPropertyChanged(property);
				}
			});
		}

		HashSet<string> propertiesUpdatedDuringDelay;

		#endregion

		#endregion

		#region ILinkEntity

		DateTime ILinkEntity.AgreedDeliveryDateInUtc => InnerLinkEntity.AgreedDeliveryDateInUtc;
		string ILinkEntity.DisplayName => DisplayName;
		public ZString DisplayName => InnerLinkEntity.DisplayName;
		bool ILinkEntity.IsLeaf => InnerLinkEntity.IsLeaf;
		IReadOnlyCollection<ILink> ILinkEntity.Links => Links.ToList();
		Guid ILinkEntity.PK => InnerLinkEntity.PK;

		ILinkDescendantsStrategy ILinkEntity.GetDefaultDescendantsStrategy()
		{
			return new ShapeNetworkEntityDescendantsStrategy();
		}

		#endregion

		#region IEquatable<BMNCNShape>

		bool IEquatable<BMNCNShape>.Equals(BMNCNShape other)
		{
			return shape == other;
		}

		#endregion

		#region IEquatable<ShapeNetworkEntity>

		public bool Equals(INetworkEntity other)
		{
			return PK == other.EntityPK;
		}

		public override bool Equals(object obj)
		{
			var other = obj as INetworkEntity;
			if (other != null)
			{
				return other.Equals(this);
			}
			else
			{
				return base.Equals(obj);
			}
		}

		public override int GetHashCode()
		{
			return PK.GetHashCode();
		}

		#endregion

		#region IShapeNetworkEntity

		public ZGuid RelatedEntityPK
		{
			get { return shape.BNS_RelatedEntityID; }
			set { shape.BNS_RelatedEntityID = value; }
		}

		public ProcessHeader ProcessHeader
		{
			get { return shape.ProcessHeader; }
		}

		IShapeNetworkEntity IShapeNetworkEntity.Owner => Owner;

		IShapeNetworkEntity IShapeNetworkEntity.Root => Root;

		public bool IsDiagram => shape.IsDiagram;
		public bool IsScaled => shape.IsScaled;
		public bool CanHaveSchedule => shape.CanHaveSchedule;
		public bool IsBuffered => shape.IsBuffered;
		public bool IsBufferShape => shape.IsBufferShape;
		public bool IsApproved => EvaluateFlagOnShapeSafely((shape) => shape.IsApproved);
		public bool IsShapePinned => EvaluateFlagOnShapeSafely((shape) => shape.IsPinned);
		public bool IsAnnotation => EvaluateFlagOnShapeSafely((shape) => shape.IsAnnotation);

		public BMNCNRootDiagramShape ShapeAsRootDiagram => shape as BMNCNRootDiagramShape;

		#region IsPinned

		public ZBool IsPinned => !IsApproved && IsShapePinned;

		public ZPropertyInfo IsPinnedInfo => GetZPropertyInfo(nameof(IsPinned));

		#endregion

		#endregion

		#region NonPersistentBizo Overrides

		protected override ZGuid GetPK()
		{
			return shape?.PK ?? ZGuid.Empty;
		}

		public override bool IsDeleted
		{
			get { return shape.IsDeleted; }
		}

		public override void Delete()
		{
			shape.Delete();
			base.Delete();
		}

		public override bool HasChanges
		{
			get { return shape.HasChanges; }
			set { shape.HasChanges = value; }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public ShapeNetworkEntityValidation Validation
		{
			get { return GetNewValidation(); }
		}

		public ShapeNetworkEntityValidation GetNewValidation()
		{
			return new ShapeNetworkEntityValidation(this);
		}

		#endregion

		#region Validation Properties

		[MaxLength("NameMaxLength")]
		public ZString Name
		{
			get { return ShapePropertiesStrategy.GetShapeName(); }
			set
			{
				ShapePropertiesStrategy.SetShapeName(value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateName();
				}
				NameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		int NameMaxLength => BMNCNShapeSchema.BNS_Name.MaxLength;

		#endregion

		#region Optimised Relationships

		public IEnumerable<NetworkAttachment> DependencyAttachments
		{
			get { return Links.Where(l => l.Attachment.IsArrow); }
		}

		IEnumerable<IShapeNetworkEntity> IShapeNetworkEntity.Children
		{
			get { return Children; }
		}

		public IEnumerable<ShapeNetworkEntity> Children
		{
			get { return Shape.ChildShapes.Select(s => entityCollection.GetInstance(s)); }
		}

		public IEnumerable<NetworkAttachment> Links
		{
			get { return entityCollection.GetInstances((IEnumerable<IEntityRelationship>)Attachments.Where(a => a.FromShape != null && Ancestors.Contains(a.BNA_BNS_Owner))); }
		}

		public IEnumerable<NetworkAttachment> PostRequisiteLinks => Links.Where(l => l.Attachment.IsArrow && l.Attachment.BNA_BNS_FromShape == PK);
		public IEnumerable<NetworkAttachment> PreRequisiteLinks => Links.Where(l => l.Attachment.IsArrow && l.Attachment.BNA_BNS_ToShape == PK);
		public IEnumerable<ShapeNetworkEntity> PostRequisiteEntities => PostRequisiteLinks.Select(s => s.To);
		public IEnumerable<ShapeNetworkEntity> PreRequisiteEntities => PreRequisiteLinks.Select(s => s.From);

		HashSet<ZGuid> Ancestors
		{
			get { return ancestors ?? (ancestors = GetOwnersUpHierarchy().Select(s => s.PK).ToHashSet()); }
		}

		HashSet<ZGuid> ancestors;

		public BMNCNAttachmentCollection Attachments
		{
			get { return attachments ?? InitialiseAttachments(); }
		}

		BMNCNAttachmentCollection attachments;

		BMNCNAttachmentCollection InitialiseAttachments()
		{
			attachments = new BMNCNAttachmentCollection(shape.Factory, shape.GetAllAttachmentQuery());
			shape.RegisterEditableChildObject(attachments);
			return attachments;
		}

		#endregion

		#region X Y Coordinates

		#region X Coordinate

		public double X
		{
			get => GetXConsideringFixation(() => GetPersistedX());
			set
			{
				SetPersistedX(value);
				OnPropertyChanged(nameof(X));

				if (!IsCalculationSuspended)
				{
					UpdateScheduledTimes();

					if (!IsValidationSuspended)
					{
						UpdateLevelingRuleViolations();
						Validation.ValidateRelationships();
					}
				}
			}
		}
		double? fixedX;

		double GetPersistedX()
		{
			var result = GetXOffsetFromParent();

			result += GetParentX();

			return result;
		}

		void SetPersistedX(double value)
		{
			var thisShapeXOffset = GetXOffsetFromParent();
			var currentX = GetPersistedX();
			var offsetFromParent = currentX - thisShapeXOffset;
			var newShapeXOffset = value - offsetFromParent;

			Shape.Left = ConvertCoordinateOffsetToStoredHorizontalOffset(newShapeXOffset);
		}

		double GetParentX()
		{
			if (Owner is INetworkEntity ownerLayout)
			{
				return ownerLayout.X;
			}
			else
			{
				return 0;
			}
		}

		internal double GetXOffsetFromParent()
		{
			return ConvertStoredHorizontalOffsetToCoordinateOffset(Shape.Left);
		}

		internal double ConvertStoredHorizontalOffsetToCoordinateOffset(ZDecimal offset)
		{
			if (IsStoringMinutesOnHorizontalAxis)
			{
				return ShapeOffsetToDateConverter.GetSizeForMinutes(Root.Shape.Scale, (double)offset);
			}
			else
			{
				return (double)offset;
			}
		}

		internal ZDecimal ConvertCoordinateOffsetToStoredHorizontalOffset(double offset)
		{
			if (IsStoringMinutesOnHorizontalAxis)
			{
				return ShapeOffsetToDateConverter.GetMinutesForScaleSize(Root.Shape.Scale, offset);
			}
			else
			{
				return SafeConvertToDecimal(offset);
			}
		}

		#endregion

		#region Y Coordinate

		public double Y
		{
			get => GetYConsideringFixation(() => GetPersistedY());
			set
			{
				SetPersistedY(value);
				OnPropertyChanged();
			}
		}
		double? fixedY;

		double GetPersistedY()
		{
			var result = GetYOffsetFromParent();

			result += GetParentY();

			return result;
		}

		double GetParentY()
		{
			if (Owner is INetworkEntity ownerLayout)
			{
				return ownerLayout.Y;
			}
			else
			{
				return 0;
			}
		}

		void SetPersistedY(double value)
		{
			var currentY = GetPersistedY();
			var thisShapeYOffset = GetYOffsetFromParent();
			var offsetFromParent = currentY - thisShapeYOffset;
			var newShapeYOffset = value - offsetFromParent;

			Shape.Top = SafeConvertToDecimal(newShapeYOffset);
		}

		internal double GetYOffsetFromParent()
		{
			return (double)Shape.Top;
		}

		#endregion

		double GetXConsideringFixation(Func<double> persistedX)
		{
			if (IsPinned)
			{
				if (fixedX == null)
				{
					SetFixedCoordinatesForFixedShape();
				}

				return (double)fixedX;
			}
			else
			{
				return GetOffsetConsideringOwner(persistedX.Invoke());
			}
		}

		double GetYConsideringFixation(Func<double> persistedY)
		{
			if (IsPinned)
			{
				if (fixedY == null)
				{
					SetFixedCoordinatesForFixedShape();
				}

				return (double)fixedY;
			}
			else
			{
				return GetOffsetConsideringOwner(persistedY.Invoke());
			}
		}

		double GetOffsetConsideringOwner(double persistedOffset)
		{
			if (Owner == null)
			{
				return 0d; // Shapes shown as the diagram surface are treated as 0 offset so that nested shapes are positioned relative to the diagram surface, whichever level of the hierarchy is currently shown as the root diagram.
			}
			else
			{
				return persistedOffset;
			}
		}

		#region Pin

		public IShapePinProvider PinProvider
		{
			get { return attachmentPinProvider ?? (attachmentPinProvider = new PinProvider(this)); }
		}

		PinProvider attachmentPinProvider;

		#endregion

		#region Fixed Coordinates

		public void SetFixedCoordinatesForFixedShape()
		{
			if (EntityState.HasFlag(EntityState.Fixed))
			{
				fixedX = GetPersistedX();
				fixedY = GetPersistedY();
			}
		}

		public void UnsetFixedCoordinatesAndUpdateParentRelativePosition()
		{
			if (!EntityState.HasFlag(EntityState.Fixed))
			{
				if (fixedX != null)
				{
					var newX = (double)(fixedX - GetParentX());
					Shape.Left = ConvertCoordinateOffsetToStoredHorizontalOffset(newX);
					fixedX = null;
				}

				if (fixedY != null)
				{
					var newY = (double)(fixedY - GetParentY());
					Shape.Top = SafeConvertToDecimal(newY);
					fixedY = null;
				}
			}
		}

		#endregion

		#endregion

		#region Width Height Coordinates

		public double Height
		{
			get => (double)Shape.Height;
			set
			{
				Shape.Height = SafeConvertToDecimal(value);
				OnPropertyChanged();
			}
		}

		public double Width
		{
			get
			{
				if (ShouldStoreWidthOnSchedule)
				{
					if (WidthProvider != null)
					{
						return WidthProvider.Width;
					}
					else
					{
						return ConvertStoredHorizontalOffsetToCoordinateOffset((ZDecimal)EntityDurationMinutes);
					}
				}
				else
				{
					return ConvertStoredHorizontalOffsetToCoordinateOffset(Shape.Width);
				}
			}
			set
			{
				SetWidth(value);

				if (!IsCalculationSuspended)
				{
					UpdateScheduledTimes();

					if (IsBufferShape)
					{
						OnPropertyChanged(nameof(AdditionalDetail));
					}

					if (!IsValidationSuspended)
					{
						UpdateLevelingRuleViolations();
						Validation.ValidateRelationships();
					}
				}

				OnPropertyChanged();
			}
		}

		void SetWidth(double value)
		{
			var proposedValue = ConvertCoordinateOffsetToStoredHorizontalOffset(value);

			if (ShouldStoreWidthOnSchedule)
			{
				EntityDurationMinutes = proposedValue.Round(0).ToZInt();
			}
			else
			{
				Shape.Width = proposedValue;
			}
		}

		decimal SafeConvertToDecimal(double value)
		{
			if (value >= (double)decimal.MaxValue)
			{
				return decimal.MaxValue;
			}
			if (value <= (double)decimal.MinValue)
			{
				return decimal.MinValue;
			}

			return Convert.ToDecimal(value);
		}

		NetworkDiagramWidthTracker WidthProvider
		{
			get
			{
				if (widthProvider == null && IsRoot)
				{
					widthProvider = new NetworkDiagramWidthTracker(this, entityCollection);
				}
				return widthProvider;
			}
		}

		NetworkDiagramWidthTracker widthProvider;

		#endregion

		#region Leveling Rules

		internal void UpdateLevelingRuleViolations()
		{
			var levelingRules = Network.DiagramEntity.LevelingRules;
			if (levelingRules != null && levelingRules.Any())
			{
				if (AppliedLevelingRuleViolations.Any())
				{
					foreach (var rule in AppliedLevelingRuleViolations)
					{
						rule.ClearRowNotifications();
					}

					var rootShape = Root;
					if (rootShape != this && Shape.ParentShape.PK == rootShape.PK && rootShape.IsScaled && !((IBufferedItem)Shape).IsClosed)
					{
						Validation.ValidateLevelingRuleViolations();
					}
				}

				OnPropertyChanged(nameof(EntityState));
				OnPropertyChanged(nameof(HasNotifications));
			}
		}

		#endregion

		#region Scheduling

		WorkingTimeContext CreateWorkingTimeContext() => WorkingTimeContext.Create(Root.Shape, null, Factory);

		void UpdateScheduledTimes()
		{
			if (!IsRoot && Root.IsScaled)
			{
				var offsetConverter = new ShapeOffsetToDateConverter(Root, CreateWorkingTimeContext(), Factory);

				var position = !root.Shape.ScheduledFinishTimeUtc.IsEmpty && X > root.Width ? root.Width - Width : X;
				shape.ScheduledStartTimeUtc = offsetConverter.GetTimeInUtcForScalePosition(position);
				shape.ScheduledFinishTimeUtc = offsetConverter.GetTimeInUtcForScalePosition(position + Width);

				OnPropertyChanged(nameof(NodeViewModel.StartDateReadableText));
				OnPropertyChanged(nameof(NodeViewModel.FinishDateReadableText));
			}

			Validation.ValidateAgreedDeliveryDateViolation();
		}

		void UpdatePersistedFloatDetails()
		{
			var schedule = Schedule;

			if (schedule != null)
			{
				shape.EarliestStartHours = schedule.EarliestStartHours;
				shape.LatestStartHours = schedule.LatestStartHours;

				shape.EarliestStartTimeUtc = schedule.EarliestStartTimeUtc;
				shape.EarliestFinishTimeUtc = schedule.EarliestFinishTimeUtc;
				shape.LatestStartTimeUtc = schedule.LatestStartTimeUtc;
				shape.LatestFinishTimeUtc = schedule.LatestFinishTimeUtc;

				shape.IsCriticalPath = schedule.IsCriticalPath;
			}
			else
			{
				shape.EarliestStartHours = ZDecimal.Zero;
				shape.LatestStartHours = ZDecimal.Zero;

				shape.EarliestStartTimeUtc = ZDateTime.Empty;
				shape.EarliestFinishTimeUtc = ZDateTime.Empty;
				shape.LatestStartTimeUtc = ZDateTime.Empty;
				shape.LatestFinishTimeUtc = ZDateTime.Empty;

				shape.IsCriticalPath = ZBool.False;
			}

			Validation.ValidateIsCyclic();
			UpdateScheduledTimes();
		}

		public ScheduleNode Schedule
		{
			get { return scheduleNode ?? (scheduleNode = GetScheduleNode()); }
			set
			{
				if (value == null || !value.SchedulingInfoIsEqual(scheduleNode))
				{
					scheduleNode = value;

					if (!IsRoot)
					{
						UpdatePersistedFloatDetails();
					}
				}
			}
		}

		ScheduleNode scheduleNode;

		ScheduleNode GetScheduleNode()
		{
			if (shape.IsDeleted)
			{
				return null;
			}

			var schedule = shape.ScheduleBizo;
			if (schedule != null)
			{
				return CreateScheduleNode(schedule);
			}
			else
			{
				return null;
			}
		}

		internal ScheduleNode GetScheduleWithoutLazyConstruction()
		{
			return scheduleNode; // Prevents stack overflow in UpdateScheduledTimes
		}

		static ScheduleNode CreateScheduleNode(BMNCNSchedule schedule)
		{
			return new NetworklessScheduleNode(schedule.Shape)
			{
				EarliestStartTimeUtc = schedule.BNC_EarliestStartUtc,
				EarliestFinishTimeUtc = schedule.BNC_EarliestFinishUtc,
				LatestStartTimeUtc = schedule.BNC_LatestStartUtc,
				LatestFinishTimeUtc = schedule.BNC_LatestFinishUtc,
				EarliestStartHours = schedule.BNC_EarliestStartOffsetMinutes / 60m,
				LatestStartHours = schedule.BNC_LatestStartOffsetMinutes / 60m,
				IsCriticalPath = schedule.BNC_IsCriticalPath,
				EstimatedDurationHoursIncludingChildren = schedule.BNC_DurationMinutes / 60m,
			};
		}

		internal TimeSpan? CalculateRequiredDurationToAccommodateScheduleAndChildShapes()
		{
			if (!IsRoot)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Should only calculate required duration for child shapes or scheduled start/finish span for the root shape. It was being calculated on [{0}] for diagram [{1}]",
					Shape.Name,
					Root.Shape.Name));
			}

			var startTimeLocal = ScheduledStartTimeLocal;
			var finishTimeLocal = ScheduledFinishTimeLocal;

			if (startTimeLocal.IsValid && finishTimeLocal.IsValid)
			{
				var context = CreateWorkingTimeContext();
				var workTimeArithmetic = context.GetWorkTimeArithmetic(Factory);

				return workTimeArithmetic.TimeDifference(startTimeLocal.ToDateTime(), finishTimeLocal.ToDateTime());
			}
			else
			{
				var lastSchedule = Network.GetRightmostEntity();

				if (lastSchedule != null)
				{
					var pixels = lastSchedule.X + lastSchedule.Width;
					var minutes = ShapeOffsetToDateConverter.GetMinutesForScaleSize(Scale, pixels);

					return TimeSpan.FromMinutes(minutes);
				}
			}

			return null;
		}

		#endregion

		#region Calculation Suspended
		public bool IsCalculationSuspended
		{
			get { return isCalculationSuspended; }
			set
			{
				if (!value && isCalculationSuspended && shape.HasChanges)
				{
					UpdateScheduledTimes();
					Validation.ValidateRelationships();
				}
				isCalculationSuspended = value;
			}
		}

		bool isCalculationSuspended;

		#endregion

		#region Shape Hierarchy

		public ShapeNetworkEntity Owner => ShapeSafe?.ParentShape == null
			? null
			: Network.Entities.GetInstance(Shape.ParentShape, addIfNotPresent: false); // Restricting to entities already in the network forces the diagram to stop walking the hierarchy if we are opening the form on a sub-diagram.

		public ShapeNetworkEntity Root
		{
			get
			{
				if (root == null)
				{
					root = this;
					while (root.Owner != null && root.Owner != this)
					{
						root = root.Owner;
					}
				}

				return root;
			}
		}

		ShapeNetworkEntity root;

		public ZBool IsRoot
		{
			get { return Root == this; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		public int ExplicitDurationMinutes
		{
			get { return WidthProvider?.ExplicitDurationMinutes ?? shape.ExplicitDurationMinutes; }
			set
			{
				shape.ExplicitDurationMinutes = value;
				Network.Refresh(RefreshType.EntitySize);
			}
		}

		public IEnumerable<ShapeNetworkEntity> GetOwnersUpHierarchy()
		{
			if (Owner != null)
			{
				yield return Owner;

				foreach (var parent in Owner.GetOwnersUpHierarchy())
				{
					yield return parent;
				}
			}
		}

		public bool IsChildOfShapeUpHierarchy(BMNCNShape parentShape)
		{
			return GetOwnersUpHierarchy().Any(s => s.Shape == parentShape);
		}

		#endregion

		#region Approve

		public void Approve(string approverCode)
		{
			if (shape.CanApprove)
			{
				if (shape.ScheduleBizo != null)
				{
					var rootShape = Root.Shape;
					shape.ScheduleBizo.BNC_GB_Branch = rootShape.ScheduleBizo?.BNC_GB_Branch ?? ZGuid.Empty;
					shape.ScheduleBizo.BNC_GE_Department = rootShape.ScheduleBizo?.BNC_GE_Department ?? ZGuid.Empty;
				}

				((IApprovable)shape).Approve(approverCode);
				OnPropertyChanged(nameof(EntityState));
			}
		}

		public void UnApprove()
		{
			shape.UnApprove();
			OnPropertyChanged(nameof(EntityState));
		}

		#endregion

		#region Box Factory

		/// <summary>
		///     Returns a rectangle consisting of the width of the shape, and its left offset (ignores vertical offset and size
		///     since shapes 'overlap' on a diagram based only on their width and left offset).
		/// </summary>
		public Rectangle GetScaleRectangle()
		{
			return new Rectangle((int)X, 1, (int)Width, 1);
		}

		#endregion

		#region Clone

		/// <summary>
		/// Pin Attachments should be discarded in cases where a scaled diagram (where pins are used) could be cloned into a non-scaled state.
		/// </summary>
		public BMNCNShape CloneShapeAndAllDescendantsIntoNewParent(BMNCNShape newParent, bool unPinPinnedShapes, BusinessObjectFactory factoryForClone = null)
		{
			Argument.NotNull(newParent, nameof(newParent));

			factoryForClone = factoryForClone ?? Factory;

			var clone = CloneShape(factoryForClone, unPinPinnedShapes, shouldMakeRootDiagramsChildShapes: true);
			clone.CloneParentProperties(newParent);

			CloneAllDescendantsAndRelatedEntities(clone, factoryForClone, unPinPinnedShapes);

			return clone;
		}

		public BMNCNShape CloneDiagramAndAllDescendants(BusinessObjectFactory factoryForClone = null)
		{
			if (!IsDiagram)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot clone a non-diagram without a new parent. Call {0} instead.", nameof(CloneShapeAndAllDescendantsIntoNewParent)));
			}

			factoryForClone = factoryForClone ?? Factory;

			var clone = CloneShape(factoryForClone, shouldUnPinShape: false);
			CloneAllDescendantsAndRelatedEntities(clone, factoryForClone, unPinPinnedShapes: false);

			return clone;
		}

		BMNCNShape CloneShape(BusinessObjectFactory factoryForClone, bool shouldUnPinShape, bool shouldMakeRootDiagramsChildShapes = false)
		{
			var typeOfClone = shouldMakeRootDiagramsChildShapes ? typeof(BMNCNShape) : shape.GetType();
			var args = new BusinessObjectCloneArgs(factoryForClone ?? Factory, new[] { nameof(BMNCNShape.BNS_BNS_ParentShape), nameof(BMNCNShape.BNS_BNS_RootShape) }, typeOfClone, false);

			var clone = (BMNCNShape)shape.Clone(args);

			if (shouldUnPinShape && clone.IsPinned)
			{
				using (clone.AllowChangingPinnedStatus())
				{
					clone.IsPinned = false;
				}
			}

			return clone;
		}

		void CloneAllDescendantsAndRelatedEntities(BMNCNShape clone, BusinessObjectFactory factoryForClone, bool unPinPinnedShapes)
		{
			var shapeToClonedShapePKMap = new Dictionary<ZGuid, ZGuid>();
			var cloneNetwork = JobNetwork.CreateTemporaryNetwork(clone);

			shapeToClonedShapePKMap.Add(PK, clone.PK);
			shapeToClonedShapePKMap.Add(ZGuid.Empty, ZGuid.Empty); // This saves checks later.

			shape.GetShapesWithinSameDiagram(); // Ensure everything is loaded.

			CloneChildren(this, clone, shapeToClonedShapePKMap, cloneNetwork.DiagramEntity, factoryForClone ?? Factory, unPinPinnedShapes);

			var strategy = new ShapeNetworkEntityDescendantsStrategy();
			var attachmentsToClone = this.Descendants(strategy)
				.Append(this).SelectMany(shape => shape.Attachments
					.Where(attachment =>
						attachment.BNA_BNS_Owner == shape.PK
						&& attachment.SupportsClone()
						&& attachment.BNA_Type != AttachmentTypeList.Codes.SwitchToScaled))
				.ToArray();

			foreach (var attachment in attachmentsToClone)
			{
				if (shapeToClonedShapePKMap.TryGetValue(attachment.BNA_BNS_ToShape, out var toShapePK) // some entities like buffers may be not included into recreation and thus may not have their keys present in the map
					&& shapeToClonedShapePKMap.TryGetValue(attachment.BNA_BNS_FromShape, out var fromShapePK))
				{
					var args = new BusinessObjectCloneArgs(factoryForClone ?? Factory, new[] { BMNCNAttachmentSchema.Constants.BNA_GS_NKApprovedBy }, typeof(BMNCNAttachment), false);
					var clonedAttachment = (BMNCNAttachment)attachment.Clone(args);
					clonedAttachment.BNA_BNS_Owner = shapeToClonedShapePKMap[attachment.BNA_BNS_Owner];
					clonedAttachment.BNA_BNS_ToShape = toShapePK;
					clonedAttachment.BNA_BNS_FromShape = fromShapePK;
				}
			}
		}

		static void CloneChildren(ShapeNetworkEntity parentSource, BMNCNShape clonedParent, Dictionary<ZGuid, ZGuid> shapeToClonedShapePKMap, IDiagramEntity diagramEntity, BusinessObjectFactory factoryForClone, bool unPinPinnedShapes)
		{
			foreach (var child in parentSource.Children.Where(s => !s.IsBufferShape))
			{
				var clonedChildShape = child.CloneShape(factoryForClone, unPinPinnedShapes);
				shapeToClonedShapePKMap.Add(child.PK, clonedChildShape.PK);

				clonedChildShape.CloneParentProperties(clonedParent);

				foreach (var affinity in child.AppliedAffinities)
				{
					diagramEntity.CreateAffinityLink(clonedChildShape, affinity);
				}

				CloneChildren(child, clonedChildShape, shapeToClonedShapePKMap, diagramEntity, factoryForClone, unPinPinnedShapes);
			}
		}

		#endregion

		#region Bound Properties

		#region Scroll Position

		[ResourceStringData("BMNCNShape.ScrollPosition", Caption = "Scroll Position")]
		[List("Shape.Lookups.ScrollPositions")]
		public ZString ScrollPosition
		{
			get { return shape.ScrollPosition; }
			set
			{
				shape.ScrollPosition = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateScrollPosition();
				}
				ScrollPositionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ScrollPositionInfo
		{
			get { return GetZPropertyInfo(nameof(ScrollPosition)); }
		}

		protected bool ScrollPosition_ReadOnly
		{
			get { return !IsRoot || !IsScaled; }
		}

		#endregion

		#region DisplayCompleteNessIndicator

		[ReadOnlyMember(nameof(DisplayCompletenessIndicator_ForBinding_ReadOnly))]
		[ResourceStringData("BMNCNShape.DisplayCompletenessIndicator", Caption = "Display Completeness Indicator", FullDescription = "Show a bar on the bottom of all shapes on this diagram which are linked to jobs or workflows, representing the completeness of the entity.")]
		public ZBool DisplayCompletenessIndicator_ForBinding
		{
			get { return Root.Shape.DisplayCompletenessIndicator; }
			set { Shape.DisplayCompletenessIndicator = value; }
		}

		public ZPropertyInfo DisplayCompletenessIndicator_ForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DisplayCompletenessIndicator_ForBinding), _ => Root.Shape.DisplayCompletenessIndicatorInfo); }
		}

		protected bool DisplayCompletenessIndicator_ForBinding_ReadOnly
		{
			get { return !IsRoot; }
		}

		#endregion

		#region Scale

		[BusinessObjectTestExclude] // This property is only used for minutes as a timespan, but the bizo property test tries to assert year as well
		[ReadOnlyMember(nameof(Scale_ReadOnly))]
		[ResourceStringData("BMNCNShape.Scale", Caption = "Scale", FullDescription = "The scale in hours and minutes of the diagram.")]
		public ZDateTime Scale
		{
			get { return shape.Scale; }
			set
			{
				shape.Scale = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateScale();
				}
				ScaleInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ScaleInfo
		{
			get { return GetZPropertyInfo(nameof(Scale)); }
		}

		protected bool Scale_ReadOnly
		{
			get { return !IsRoot || !IsScaled; }
		}

		#endregion

		#region ResolutionIncrement

		[BusinessObjectTestExclude] // This property is only used for minutes as a timespan, but the bizo property test tries to assert year as well
		[ResourceStringData("BMNCNShape.ResolutionIncrement", Caption = "Resolution Increment", FullDescription = "The resolution increment that all shapes must abide to.")]
		public ZDateTime ResolutionIncrement
		{
			get { return shape.ResolutionIncrement; }
			set
			{
				shape.ResolutionIncrement = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateResolutionIncrement();
				}
				ResolutionIncrementInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ResolutionIncrementInfo
		{
			get { return GetZPropertyInfo(nameof(ResolutionIncrement)); }
		}
		protected bool ResolutionIncrement_ReadOnly
		{
			get { return Scale_ReadOnly; }
		}

		#endregion

		#region ScheduledStartTimeLocal

		[ReadOnlyMember(nameof(ScheduleTimes_ReadOnly))]
		[ResourceStringData("ShapeNetworkEntity.ScheduledStartTimeLocal", Caption = "Scheduled Start Time", FullDescription = "The time this diagram or shape is scheduled to commence (in local time).")]
		public ZDateTime ScheduledStartTimeLocal
		{
			get { return Shape.ScheduledStartTimeLocal; }
			set { Shape.ScheduledStartTimeLocal = value; }
		}

		public ZPropertyInfo ScheduledStartTimeLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ScheduledStartTimeLocal), _ => shape.ScheduledStartTimeLocalInfo); }
		}

		#endregion

		#region ScheduledFinishTimeLocal

		[ReadOnlyMember(nameof(ScheduleTimes_ReadOnly))]
		[ResourceStringData("ShapeNetworkEntity.ScheduledFinishTimeLocal", Caption = "Scheduled Finish Time", FullDescription = "The time this diagram or shape is scheduled to complete (in local time).")]
		public ZDateTime ScheduledFinishTimeLocal
		{
			get { return Shape.ScheduledFinishTimeLocal; }
			set { Shape.ScheduledFinishTimeLocal = value; }
		}

		public ZPropertyInfo ScheduledFinishTimeLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ScheduledFinishTimeLocal), _ => shape.ScheduledFinishTimeLocalInfo); }
		}

		#endregion

		#region ReadOnly

		protected bool ScheduleTimes_ReadOnly => !Shape.IsDiagram || !IsScaled;

		#endregion

		#region ScheduleHintLabel

		public ZString ScheduleHintLabel
		{
			get { return network.SchedulesNeedRecalculation() ? Res.GetString("a896e771-5a00-4bbb-a822-427090fe2a8b", "Save the diagram to ensure this information is up-to-date.") : string.Empty; }
		}

		#endregion

		#endregion
	}
}
