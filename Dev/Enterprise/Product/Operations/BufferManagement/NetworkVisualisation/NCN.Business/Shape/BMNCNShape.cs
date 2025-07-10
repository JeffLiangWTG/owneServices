using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	[DebuggerDisplay("{DisplayText_ForTest}")]
	[CodeProperty(BMNCNShapeSchema.Constants.BNS_Name)]
	[DescriptionProperty(BMNCNShapeSchema.Constants.BNS_Name)]
	public partial class BMNCNShape : AutoBMNCNShape,
		IProposedNetworkEntity,
		INetworkEntity,
		IBMNCNShape,
		IDocManagerSupport,
		ILinkEntity,
		IApprovable,
		IBufferedItemWithRelatedEntity,
		IBranchDepartmentProvider,
		IAuditParent
	{
		public BMNCNShape(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			NotificationsChanged += Shape_NotificationsChanged;

			if (!BMSRegistry.Instance.WorkflowManagementMode.Value.Equals(WorkflowManagementModes.Codes.PlanningManagement))
			{
				planningManagementDisabledCreationStackTrace = new StackTrace().ToString();
			}
		}

		readonly string planningManagementDisabledCreationStackTrace;

		#region TypeDecider

		class BMNCNShapeTypeDecider : TypeDecider
		{
			public override Type GetTypeForBinding()
			{
				return typeof(BMNCNShape);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				var shapeType = new ZString(row[BMNCNShapeSchema.BNS_ShapeType.Name]);

				if (shapeType == ShapeTypeList.Codes.Diagram && new ZGuid(row[BMNCNShapeSchema.BNS_BNS_RootShape.Name]).IsEmpty)
				{
					return typeof(BMNCNRootDiagramShape);
				}

				switch (shapeType)
				{
					case ShapeTypeList.Codes.DefaultDiagram:
						return typeof(BMNCNShapeDefaultDiagram);

					case ShapeTypeList.Codes.Buffer:
						return typeof(BMNCNBufferShape);

					default:
						return typeof(BMNCNShape);
				}
			}

			public override Type GetTypeForNew()
			{
				return typeof(BMNCNShape);
			}
		}

		public static readonly TypeDecider TypeDecider = new BMNCNShapeTypeDecider();

		#endregion

		#region BusinessObject Overrides

		protected override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield return new XmlColumnSpecification(BMNCNShapeSchema.BNS_LayoutData); }
		}

		public override bool IsSavedByFactory
		{
			get
			{
				var firstInstanceInFactoryCache = Factory.GetBizOsForPK(PK.ToGuid()).FirstOrDefault();

				if (firstInstanceInFactoryCache != null && firstInstanceInFactoryCache != this)
				{
					return firstInstanceInFactoryCache.IsSavedByFactory;
				}

				if (!base.IsSavedByFactory)
				{
					return false;
				}

				if (IsDeleted)
				{
					return true;
				}

				if (BNS_ShapeType == ShapeTypeList.Codes.DefaultDiagram || BNS_ShapeType == ShapeTypeList.Codes.DefaultWorkflow)
				{
					return ShouldSaveDefaultDiagram;
				}

				return true;
			}
		}

		internal bool ShouldSaveDefaultDiagram
		{
			get
			{
				if (!IsInDatabase)
				{
					return HasChanges || (IsDefaultDiagram && this.GetShapesWithinSameDiagram().Any(s => s.HasChanges));
				}
				else
				{
					return true;
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		public override void OnSaving()
		{
			base.OnSaving();

			if (IsDiagram && BNS_BNS_ParentShape.IsValid)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Diagram shapes shouldn't have a parent. Anything within a diagram is a shape. PK: [{0}], Name: [{1}]", PK, Name));
			}

			if (!string.IsNullOrEmpty(planningManagementDisabledCreationStackTrace))
			{
				ReportShapeSavedDespiteNotAllowedInRegistry();
			}
		}

		void ReportShapeSavedDespiteNotAllowedInRegistry()
		{
			var message = new StringBuilder();
			message.AppendLine((NoResString)"A BMNCNShape was initialized, which shouldn't have happened because Planning Management isn't enabled in the registry.");
			message.AppendLine((NoResString)"Stack trace of shape constructor:");
			message.AppendLine(planningManagementDisabledCreationStackTrace);
			ErrorReporter.ReportOnce("BMNCNShapeUsedWhenNotPLN", message.ToString());
		}

		public override void Delete()
		{
			if (!IsDeleted && IsBufferShape && Active)
			{
				UnsetIsBufferedFlag();
			}

			ChildShapes.DeleteAll();

			foreach (var attachment in AllAttachmentsToDelete)
			{
				if (!attachment.IsDeleted)
				{
					attachment.Delete();
				}
			}

			if (!IsDeleted)
			{
				var schedule = ScheduleBizo;

				if (schedule != null)
				{
					schedule.Delete();
				}
			}

			base.Delete();
		}

		void UnsetIsBufferedFlag()
		{
			var root = RootShapeProvider.GetRoot(Factory);
			if (root != null)
			{
				var descendants = root.GetShapesWithinSameDiagram().Append(root);
				var otherActiveBuffers = descendants.Where(s => s.IsBufferShape && s != this && s.Active);
				if (!otherActiveBuffers.Any())
				{
					foreach (var shape in descendants)
					{
						if (shape.IsBuffered)
						{
							shape.IsBuffered = ZBool.False;
						}
					}
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			BNS_ShapeType = ShapeTypeList.Codes.Diagram;
			BNS_Status = ShapeStatusList.Codes.Unknown;
			BNS_Name = DefaultDiagramName;
			JobName = DefaultJobName;
			BackColor = Color.Bisque.Name;
			Active = ZBool.True;
		}

		public static string DefaultDiagramName
		{
			get { return Res.GetString("ac07a264-acf1-4ff3-bcb7-b37cdf4617a8", "New Diagram"); }
		}

		public static string DefaultJobName
		{
			get { return Res.GetString("a7a9fbfc-044d-4ad4-a20a-2f0dfd51a332", "Job Name"); }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return BNS_Name; }
		}

		public override bool HasChanges
		{
			get
			{
				return base.HasChanges;
			}
			set
			{
				var hadChanges = HasChanges;
				base.HasChanges = value;

				if (!IsDeleted && !hadChanges && HasChanges)
				{
					var root = RootShapeProvider.GetRoot(Factory) ?? this;
					if (root != this && !root.HasChanges && root.IsDefaultDiagram)
					{
						root.HasChanges = true;
					}
				}
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new BMNCNShapeFetchStrategy(this);
		}

		#endregion

		#region Fetch Strategy

		class BMNCNShapeFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			internal BMNCNShapeFetchStrategy(BMNCNShape shape)
				: base(shape)
			{
				this.shape = shape;
			}

			readonly BMNCNShape shape;

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();

				Factory.AddFetchHint(BMNCNScheduleSchema.BNC_BNS_Shape, shape.PK);
			}
		}

		#endregion

		#region Properties

		#region BNS_RelatedEntityID

		[List("Lookups.ProcessHeaders")]
		[RelatedBusinessObject("ProcessHeader")]
		public override ZGuid BNS_RelatedEntityID
		{
			get { return base.BNS_RelatedEntityID; }
			set
			{
				var wasLinkedToRealEntity = IsLinkedToRealEntity;

				if (value.IsValid)
				{
					if (Factory.Load<ProcessHeader>(value) != null)
					{
						BNS_RelatedEntityTableCode = ProcessHeaderSchema.Constants.Prefix;
					}
					else if (Factory.Load<BMNCNShape>(value) != null)
					{
						BNS_RelatedEntityTableCode = BMNCNShapeSchema.Constants.Prefix;
					}
					else
					{
						BNS_RelatedEntityTableCode = ZString.Empty;
					}
				}
				else
				{
					BNS_RelatedEntityTableCode = ZString.Empty;
				}

				base.BNS_RelatedEntityID = value;

				if (value.IsValid)
				{
					var processHeader = Factory.Load<ProcessHeader>(value);
					var parent = processHeader?.Parent;

					if (parent != null)
					{
						BNS_JobType = parent.WorkflowType;
					}

					if (!wasLinkedToRealEntity && IsLinkedToRealEntity && !string.IsNullOrWhiteSpace(BNS_CompletionStatements))
					{
						var realEntityCompletionStatements = ((IProposedNetworkEntity)processHeader).CompletionCriteria;
						if (!string.IsNullOrWhiteSpace(realEntityCompletionStatements))
						{
							realEntityCompletionStatements += System.Environment.NewLine;
						}
						realEntityCompletionStatements += BNS_CompletionStatements;

						((IProposedNetworkEntity)processHeader).CompletionCriteria = realEntityCompletionStatements;
						BNS_CompletionStatements = ZString.Empty;
					}
				}

				OnPropertyChanged(nameof(JobNumber));
				OnPropertyChanged(nameof(JobName));

				if (!IsValidationSuspended)
				{
					Validation.ValidateLinkedDiagramSchedulePropagation();
				}
			}
		}

		protected bool BNS_RelatedEntityID_ReadOnly
		{
			get { return BNS_ShapeType == ShapeTypeList.Codes.DefaultDiagram; }
		}

		public override ZString BNS_Status
		{
			get { return base.BNS_Status; }
			set
			{
				base.BNS_Status = value;
				OnPropertyChanged(nameof(INetworkEntity.Status));
			}
		}

		internal bool CanSetStatus()
		{
			return !IsLinkedToRealEntity && IsEntityShape;
		}

		#endregion

		#region BNS_JobType

		[List("Lookups.JobTypes")]
		public override ZString BNS_JobType
		{
			get
			{
				if (base.BNS_JobType.IsEmpty)
				{
					var processHeader = ProcessHeader;
					var parent = processHeader != null ? processHeader.Parent : null;

					if (parent != null)
					{
						return parent.WorkflowType;
					}
				}

				return base.BNS_JobType;
			}
			set { base.BNS_JobType = value; }
		}

		protected bool BNS_JobType_ReadOnly
		{
			get
			{
				var jobHeader = ProcessHeader;
				return jobHeader != null && !jobHeader.FH_ParentId.IsEmpty;
			}
		}

		#endregion

		#region BNS_ShapeType

		[List("Lookups.ShapeTypes")]
		public override ZString BNS_ShapeType
		{
			get { return base.BNS_ShapeType; }
			set { base.BNS_ShapeType = value; }
		}

		#endregion

		#region BNS_Name

		public override ZString BNS_Name
		{
			get { return base.BNS_Name; }
			set
			{
				base.BNS_Name = value;

				OnPropertyChanged(nameof(Name)); // Property name
			}
		}

		#endregion

		#region BNS_GS_NKApprovedBy

		[ReadOnly(true)] // Don't expose this field as writable or you'll break approval process :)
		[BusinessObjectTestExclude] // Setting this property is not supported except through IApprovable.Approve
		public override ZString BNS_GS_NKApprovedBy
		{
			get { return base.BNS_GS_NKApprovedBy; }
			set
			{
				if (!isChangingApprovalAllowed)
				{
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Cannot change approval of this item. Use {0} instead.", nameof(ApproveDiagramAction)));
				}

				base.BNS_GS_NKApprovedBy = value;
			}
		}

		IDisposable AllowChangingApproval()
		{
			isChangingApprovalAllowed = true;
			return new DisposableAction(() => isChangingApprovalAllowed = false);
		}

		bool isChangingApprovalAllowed;

		#endregion

		#endregion

		#region Xml Properties

		#region Positioning

		[XmlColumnProperty]
		public ZBool IsPositioned
		{
			get
			{
				return Left > 0 || Top > 0 || GetXmlColumnPropertyValue<ZBool>(IsPositionedInfo);
			}
			set { SetXmlColumnPropertyValue(IsPositionedInfo, value); }
		}
		public ZPropertyInfo IsPositionedInfo => GetZPropertyInfo(nameof(IsPositioned));

		#region Left 

		[XmlColumnProperty]
		public ZDecimal Left
		{
			get { return GetXmlColumnPropertyValue<ZDecimal>(LeftInfo); }
			set
			{
				SetXmlColumnPropertyValue(LeftInfo, value);
				IsPositioned = true;
			}
		}

		public ZPropertyInfo LeftInfo => GetZPropertyInfo(nameof(Left));

		#endregion

		#region Top
		[XmlColumnProperty]
		public ZDecimal Top
		{
			get { return GetXmlColumnPropertyValue<ZDecimal>(TopInfo); }
			set
			{
				var oldValue = Top;

				SetXmlColumnPropertyValue(TopInfo, value);
				IsPositioned = true;

				if (Top != oldValue)
				{
					ClearShapesInChannels();
				}
			}
		}

		public ZPropertyInfo TopInfo => GetZPropertyInfo(nameof(Top));

		#endregion

		#region Width

		[XmlColumnProperty]
		public ZDecimal Width
		{
			get { return GetXmlColumnPropertyValue<ZDecimal>(WidthInfo); }
			set { SetXmlColumnPropertyValue(WidthInfo, value); }
		}

		public ZPropertyInfo WidthInfo => GetZPropertyInfo(nameof(Width));

		#endregion

		#region Height

		[XmlColumnProperty]
		public ZDecimal Height
		{
			get { return GetXmlColumnPropertyValue<ZDecimal>(HeightInfo); }
			set { SetHeight(value); }
		}

		protected virtual void SetHeight(ZDecimal value)
		{
			var oldValue = Height;
			SetXmlColumnPropertyValue(HeightInfo, value);

			if (Height != oldValue)
			{
				ClearShapesInChannels();
			}
		}

		public ZPropertyInfo HeightInfo => GetZPropertyInfo(nameof(Height));

		void ClearShapesInChannels()
		{
			var diagramShape = FindTopmostDiagramShape() as BMNCNRootDiagramShape;

			if (diagramShape != null)
			{
				diagramShape.Channels.ForEach(c => c.Shapes.Clear());
			}
		}

		#endregion

		#region ZIndex

		[XmlColumnProperty]
		public ZDecimal ZIndex
		{
			get { return GetXmlColumnPropertyValue<ZDecimal>(ZIndexInfo); }
			set { SetXmlColumnPropertyValue(ZIndexInfo, value); }
		}

		public ZPropertyInfo ZIndexInfo => GetZPropertyInfo(nameof(ZIndex));

		#endregion

		#endregion

		#region ScrollPosition

		[XmlColumnProperty(DefaultValue = ScrollPositionList.Codes.Default, SerialiseDefaultValues = false)]
		[List("Lookups.ScrollPositions")]
		public ZString ScrollPosition
		{
			get { return GetXmlColumnPropertyValue<ZString>(ScrollPositionInfo); }
			set { SetXmlColumnPropertyValue(ScrollPositionInfo, value); }
		}

		public ZPropertyInfo ScrollPositionInfo
		{
			get { return GetZPropertyInfo(nameof(ScrollPosition)); }
		}

		#endregion

		#region BackColor

		[MaxLength(50)]
		[XmlColumnProperty]
		public ZString BackColor
		{
			get { return GetXmlColumnPropertyValue<ZString>(BackColorInfo); }
			set { SetXmlColumnPropertyValue(BackColorInfo, value); }
		}

		public ZPropertyInfo BackColorInfo
		{
			get { return GetZPropertyInfo(nameof(BackColor)); }
		}

		#endregion

		#region ForeColor

		[MaxLength(50)]
		[XmlColumnProperty(DefaultValue = "Black")]
		public ZString ForeColor
		{
			get { return GetXmlColumnPropertyValue<ZString>(ForeColorInfo); }
			set { SetXmlColumnPropertyValue(ForeColorInfo, value); }
		}

		public ZPropertyInfo ForeColorInfo
		{
			get { return GetZPropertyInfo(nameof(ForeColor)); }
		}

		#endregion

		#region CornerRadius

		[XmlColumnProperty(DefaultValue = 10)]
		public ZInt CornerRadius
		{
			get { return GetXmlColumnPropertyValue<ZInt>(CornerRadiusInfo); }
			set { SetXmlColumnPropertyValue(CornerRadiusInfo, value); }
		}

		public ZPropertyInfo CornerRadiusInfo
		{
			get { return GetZPropertyInfo(nameof(CornerRadius)); }
		}

		#endregion

		#region JobName

		[XmlColumnProperty]
		[MaxLength(int.MaxValue)]
		public ZString JobName
		{
			get { return GetXmlColumnPropertyValue<ZString>(JobNameInfo); }
			set { SetXmlColumnPropertyValue(JobNameInfo, value); }
		}

		public ZPropertyInfo JobNameInfo
		{
			get { return GetZPropertyInfo(nameof(JobName)); }
		}

		#endregion

		#region Shape Notes

		[XmlColumnProperty]
		[MaxLength(int.MaxValue)]
		public ZString ShapeNotes
		{
			get { return GetXmlColumnPropertyValue<ZString>(ShapeNotesInfo); }
			set
			{
				SetXmlColumnPropertyValue(ShapeNotesInfo, value);
				OnPropertyChanged("AdditionalDetail");
			}
		}

		public ZPropertyInfo ShapeNotesInfo
		{
			get { return GetZPropertyInfo(nameof(ShapeNotes)); }
		}

		#endregion

		#region IsReadOnly

		[XmlColumnProperty]
		public ZBool IsReadOnly
		{
			get { return GetXmlColumnPropertyValue<ZBool>(IsReadOnlyInfo); }
			set { SetXmlColumnPropertyValue(IsReadOnlyInfo, value); }
		}

		public ZPropertyInfo IsReadOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(IsReadOnly)); }
		}

		#endregion

		#region IsPinned

		[XmlColumnProperty]
		[BusinessObjectTestExclude] // Because setting this property directly is not supported.
		public ZBool IsPinned
		{
			get { return GetXmlColumnPropertyValue<ZBool>(IsPinnedInfo); }
			set
			{
				if (!isChangingPinnedStatusAllowed)
				{
					throw new NotSupportedException(FormattableString.Invariant($"Cannot change pinned state of this item directly. Use {nameof(PinShapeAction)} or {nameof(UnpinShapeAction)} instead (or PinShape and UnPinShape extension methods in unit tests)."));
				}

				var valueHasChanged = value != IsPinned;
				SetXmlColumnPropertyValue(IsPinnedInfo, value);

				if (valueHasChanged)
				{
					OnPropertyChanged(nameof(Business.ShapeNetworkEntity.EntityState));
				}
			}
		}

		public ZPropertyInfo IsPinnedInfo => GetZPropertyInfo(nameof(IsPinned));

#if DEBUG
		public IDisposable AllowChangingPinnedStatus_ForTest() => AllowChangingPinnedStatus();
#endif

		internal IDisposable AllowChangingPinnedStatus()
		{
			isChangingPinnedStatusAllowed = true;

			return new DisposableAction(() => isChangingPinnedStatusAllowed = false);
		}

		bool isChangingPinnedStatusAllowed;

		#endregion

		#region Active

		[XmlColumnProperty(DefaultValue = true)]
		public ZBool Active
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ActiveInfo); }
			set
			{
				var originalValue = Active;

				SetXmlColumnPropertyValue(ActiveInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateActive();
				}

				if (!value && originalValue && IsBufferShape)
				{
					UnsetIsBufferedFlag();
				}
			}
		}

		public ZPropertyInfo ActiveInfo
		{
			get { return GetZPropertyInfo(nameof(Active)); }
		}

		#endregion

		#region DisplayCompletenessIndicator

		[XmlColumnProperty(DefaultValue = true)]
		public ZBool DisplayCompletenessIndicator
		{
			get { return GetXmlColumnPropertyValue<ZBool>(DisplayCompletenessIndicatorInfo); }
			set { SetXmlColumnPropertyValue(DisplayCompletenessIndicatorInfo, value); }
		}

		public ZPropertyInfo DisplayCompletenessIndicatorInfo
		{
			get { return GetZPropertyInfo(nameof(DisplayCompletenessIndicator)); }
		}

		#endregion

		#region ShouldSynchroniseScheduleWithLinkedEntity

		[XmlColumnProperty(DefaultValue = true)]
		[ResourceStringData("BMNCNShape.ShouldSynchroniseScheduleWithLinkedEntity", Caption = "Synchronize Schedule with Linked Entity", FullDescription = "When ticked, the scaled diagram linked to this shape will have its Scheduled Start and Scheduled Finish times kept in sync with the Scheduled Start and Scheduled Finish of this shape.")]
		public ZBool ShouldSynchroniseScheduleWithLinkedEntity
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShouldSynchroniseScheduleWithLinkedEntityInfo); }
			set
			{
				SetXmlColumnPropertyValue(ShouldSynchroniseScheduleWithLinkedEntityInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateLinkedDiagramSchedulePropagation();
				}
			}
		}

		public ZPropertyInfo ShouldSynchroniseScheduleWithLinkedEntityInfo => GetZPropertyInfo(nameof(ShouldSynchroniseScheduleWithLinkedEntity));

		protected bool ShouldSynchroniseScheduleWithLinkedEntity_ReadOnly => !RootShape?.IsScaled ?? true;

		#endregion

		#region ShapeAffinities

		[ChildEditable]
		[XmlColumnProperty]
		public ShapeAffinityCollection ShapeAffinities
		{
			get
			{
				if (shapeAffinities == null)
				{
					shapeAffinities = new ShapeAffinityCollection(this.Factory, this);
					RegisterEditableChildObject(shapeAffinities);
				}
				return shapeAffinities;
			}
		}

		ShapeAffinityCollection shapeAffinities;

		[ChildEditable]
		[XmlColumnProperty]
		public ShapeAffinityLinkCollection ShapeAffinityLinks
		{
			get
			{
				if (shapeAffinityLinks == null)
				{
					shapeAffinityLinks = new ShapeAffinityLinkCollection(Factory);
					RegisterEditableChildObject(shapeAffinityLinks);
				}
				return shapeAffinityLinks;
			}
		}

		ShapeAffinityLinkCollection shapeAffinityLinks;

		#endregion

		#region BufferType

		[XmlColumnProperty]
		[List("Lookups.BufferTypes")]
		[ReadOnlyMember(nameof(BufferType_ReadOnly))]
		[ResourceStringData("BMNCNShape|BufferType", Caption = "Buffer Type")]
		public ZString BufferType
		{
			get { return GetXmlColumnPropertyValue<ZString>(BufferTypeInfo); }
			set { SetXmlColumnPropertyValue(BufferTypeInfo, value); }
		}

		public ZPropertyInfo BufferTypeInfo
		{
			get { return GetZPropertyInfo(nameof(BufferType)); }
		}

		public bool BufferType_ReadOnly
		{
			get { return !IsBufferShape; }
		}

		#endregion

		#region IsNonScheduled

		[XmlColumnProperty]
		public ZBool IsNonScheduled
		{
			get { return GetXmlColumnPropertyValue<ZBool>(IsNonScheduledInfo); }
			set
			{
				SetXmlColumnPropertyValue(IsNonScheduledInfo, value);

				if (value)
				{
					RemoveSchedule();
				}
			}
		}

		public ZPropertyInfo IsNonScheduledInfo => GetZPropertyInfo(nameof(IsNonScheduled));

		void INetworkEntity.SwapNonScheduledState()
		{
			IsNonScheduled = !IsNonScheduled;
		}

		#endregion

		#region ShapeInspectorVisible

		[XmlColumnProperty(DefaultValue = false)]
		public ZBool ShapeInspectorVisible
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShapeInspectorVisibleInfo); }
			set { SetXmlColumnPropertyValue(ShapeInspectorVisibleInfo, value); }
		}

		public ZPropertyInfo ShapeInspectorVisibleInfo
		{
			get { return GetZPropertyInfo(nameof(ShapeInspectorVisible)); }
		}

		#endregion

		#endregion

		#region Estimates

		public ZDecimal RemainingEstimateHoursIncludingChildren => GetRemainingEstimateHoursIncludingChildren(new List<BMNCNShape>());

		ZDecimal GetRemainingEstimateHoursIncludingChildren(List<BMNCNShape> traversedShapes)
		{
			if (traversedShapes.Contains(this))
			{
				return 0;
			}

			traversedShapes.Add(this);

			if (BNS_ShapeType == ShapeTypeList.Codes.Diagram)
			{
				AddFetchHintsForChildShapesRelatedProcessHeadersAndTasks();
				return ChildShapes.Sum(s => s.GetRemainingEstimateHoursIncludingChildren(traversedShapes));
			}
			else if (RelatedShape != null)
			{
				return RelatedShape.GetRemainingEstimateHoursIncludingChildren(traversedShapes);
			}
			else if (ProcessHeader != null)
			{
				return ProcessHeader.RemainingEstimateHoursIncludingChildren;
			}
			else if (IsScaled)
			{
				return BNS_Status != ShapeStatusList.Codes.Closed && BNS_Status != ShapeStatusList.Codes.Cancelled ? ExplicitDurationHours : 0;
			}
			return 0;
		}

		public ZDecimal TotalNonCancelledEstimatedHoursIncludingChildren => GetTotalNonCancelledEstimatedHoursIncludingChildren(new List<BMNCNShape>());

		ZDecimal GetTotalNonCancelledEstimatedHoursIncludingChildren(List<BMNCNShape> traversedShapes)
		{
			if (traversedShapes.Contains(this))
			{
				return 0;
			}

			traversedShapes.Add(this);

			if (BNS_ShapeType == ShapeTypeList.Codes.Diagram)
			{
				AddFetchHintsForChildShapesRelatedProcessHeadersAndTasks();
				return ChildShapes.Sum(s => s.GetTotalNonCancelledEstimatedHoursIncludingChildren(traversedShapes));
			}
			else if (RelatedShape != null)
			{
				return RelatedShape.GetTotalNonCancelledEstimatedHoursIncludingChildren(traversedShapes);
			}
			else if (ProcessHeader != null)
			{
				return ProcessHeader.TotalNonCancelledEstimatedHoursIncludingChildren;
			}
			else if (IsScaled)
			{
				return BNS_Status != ShapeStatusList.Codes.Cancelled ? ExplicitDurationHours : 0;
			}
			return 0;
		}

		void AddFetchHintsForChildShapesRelatedProcessHeadersAndTasks()
		{
			foreach (var shape in ChildShapes)
			{
				if (shape.BNS_RelatedEntityTableCode == ProcessHeaderSchema.Constants.Prefix)
				{
					Factory.AddFetchHint(ProcessHeaderSchema.PK, shape.BNS_RelatedEntityID);
					Factory.AddFetchHint(ProcessHeaderLinkSchema.FP_FH_HeaderTo, shape.BNS_RelatedEntityID); // ChildLinks on related ProcessHeader
					Factory.AddFetchHint(ProcessTasksSchema.P9_FH_ProcessHeader, shape.BNS_RelatedEntityID); // GetRelevantTasksForEstimates on related ProcessHeader
				}
			}
		}

		#endregion

		#region Schedule Properties

		#region IsScaled

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		[ResourceStringData("BMNCNShape.IsScaled", Caption = "Scaled", FullDescription = "Whether this diagram is scaled. Entities in scaled diagrams have a Planned Duration relative to their size, and a starting time relative to their position.")]
		public ZBool IsScaled
		{
			get { return MaybeGetFromSchedule(s => s.BNC_IsScaled); }
			set { SetOnSchedule(s => s.BNC_IsScaledInfo, value, IsScaledInfo); }
		}

		public ZPropertyInfo IsScaledInfo
		{
			get { return GetZPropertyInfo(nameof(IsScaled)); }
		}

		#endregion

		#region NCNReleaseOffsetMinutes

		[CopiedFromDiagramShapeOnCreation]
		public ZInt NCNReleaseOffsetMinutes
		{
			get { return MaybeGetFromSchedule(s => s.BNC_NCNReleaseOffsetMinutes); }
			set { SetOnSchedule(s => s.BNC_NCNReleaseOffsetMinutesInfo, value, NCNReleaseOffsetMinutesInfo); }
		}

		public ZPropertyInfo NCNReleaseOffsetMinutesInfo
		{
			get { return GetZPropertyInfo(nameof(NCNReleaseOffsetMinutes)); }
		}

		[BusinessObjectTestExclude] // Test doesn't work well for timespans
		[ReadOnlyMember(nameof(NCNReleaseOffsetTime_ReadOnly))]
		[ResourceStringData("BMNCNShape.NCNReleaseOffsetTime", Caption = "NCN Release Offset", FullDescription = "The amount of time which indicates how early all items on this plan can be started before their scheduled start time.")]
		public ZDateTime NCNReleaseOffsetTime
		{
			get { return NCNReleaseOffsetMinutes.GetDateTimeFromMinutes(); }
			set
			{
				NCNReleaseOffsetMinutes = (int)value.GetMinutesFromDateTimeSpan();
				NCNReleaseOffsetTimeInfo.RefreshBinding();
				UpdateNCNReleaseOffsetOnChildren();
			}
		}

		public ZPropertyInfo NCNReleaseOffsetTimeInfo
		{
			get { return GetZPropertyInfo(nameof(NCNReleaseOffsetTime)); }
		}

		public bool NCNReleaseOffsetTime_ReadOnly
		{
			get { return ApprovedDiagramType == ApprovedDiagramTypeList.Codes.CCPMApprovedDiagram || !IsScaled; }
		}

		void UpdateNCNReleaseOffsetOnChildren()
		{
			foreach (var shape in this.GetShapesWithinSameDiagram().Where(s => s.PK != PK))
			{
				shape.NCNReleaseOffsetMinutes = NCNReleaseOffsetMinutes;
			}
		}

		#endregion

		#region BufferPenetration

		[ReadOnly(true)]
		[ResourceStringData("BMNCNShape|BufferPenetration", Caption = "Buffer Penetration")]
		public ZDecimal BufferPenetration
		{
			get { return MaybeGetFromSchedule(s => s.BNC_BufferPenetrationPercent / 100m); }
			set { SetOnSchedule(s => s.BNC_BufferPenetrationPercentInfo, (ZDecimal)(value * 100), BufferPenetrationInfo); }
		}

		public ZPropertyInfo BufferPenetrationInfo
		{
			get { return GetZPropertyInfo(nameof(BufferPenetration)); }
		}

		[ReadOnly(true)]
		public ZInt PenetratingBufferSizeInMinutes
		{
			get { return MaybeGetFromSchedule(s => s.BNC_PenetratingBufferSizeInMinutes); }
			set { SetOnSchedule(s => s.BNC_PenetratingBufferSizeInMinutesInfo, value, PenetratingBufferSizeInMinutesInfo); }
		}

		public ZPropertyInfo PenetratingBufferSizeInMinutesInfo
		{
			get { return GetZPropertyInfo(nameof(PenetratingBufferSizeInMinutes)); }
		}

		public bool UpdateBufferPenetration(WorkingTimeContext context)
		{
			var penetration = GetBufferPenetration(context);

			if (GetRoundedPenetration(penetration.Penetration) != GetRoundedPenetration(BufferPenetration))
			{
				using (SuspendSettingHasChanges())
				{
					BufferPenetration = penetration.Penetration;

					if (!IsBufferShape)
					{
						PenetratingBufferSizeInMinutes = penetration.PenetratingBuffer.SizeInMinutes;
					}
				}

				return true;
			}

			return false;
		}

		protected virtual BufferPenetrationResult GetBufferPenetration(WorkingTimeContext context)
		{
			return BufferPenetrationCalculator.CalculatePenetrationPercentage(this, context, Factory);
		}

		static decimal GetRoundedPenetration(decimal penetration)
		{
			return Utilities.Round(penetration, BMNCNScheduleSchema.BNC_BufferPenetrationPercent.Scale);
		}

		#endregion

		#region EarliestStartHours

		[ReadOnly(true)]
		[ResourceStringData("BMNCNShape.EarliestStartHours", Caption = "Earliest Start", FullDescription = "The earliest this item can be started.")]
		public ZDecimal EarliestStartHours
		{
			get { return MaybeGetFromSchedule(s => s.BNC_EarliestStartOffsetMinutes / 60m); }
			set { SetOnSchedule(s => s.BNC_EarliestStartOffsetMinutesInfo, (ZInt)(value * 60m), EarliestStartHoursInfo); }
		}

		public ZPropertyInfo EarliestStartHoursInfo
		{
			get { return GetZPropertyInfo(nameof(EarliestStartHours)); }
		}

		#endregion

		#region EarliestFinishHours

		[ReadOnly(true)]
		[ResourceStringData("BMNCNShape.EarliestFinishHours", Caption = "Earliest Finish", FullDescription = "The earliest this item can be finished.")]
		public ZDecimal EarliestFinishHours
		{
			get { return MaybeGetFromSchedule(s => (s.BNC_EarliestStartOffsetMinutes + s.BNC_DurationMinutes) / 60m); }
		}

		public ZPropertyInfo EarliestFinishHoursInfo
		{
			get { return GetZPropertyInfo(nameof(EarliestFinishHours)); }
		}

		#endregion

		#region LatestStartHours

		[ReadOnly(true)]
		[ResourceStringData("BMNCNShape.LatestStartHours", Caption = "Latest Start", FullDescription = "The latest this item can be started.")]
		public ZDecimal LatestStartHours
		{
			get { return MaybeGetFromSchedule(s => s.BNC_LatestStartOffsetMinutes / 60m); }
			set { SetOnSchedule(s => s.BNC_LatestStartOffsetMinutesInfo, (ZInt)(value * 60m), LatestStartHoursInfo); }
		}

		public ZPropertyInfo LatestStartHoursInfo
		{
			get { return GetZPropertyInfo(nameof(LatestStartHours)); }
		}

		#endregion

		#region LatestFinishHours

		[ReadOnly(true)]
		[ResourceStringData("BMNCNShape.LatestFinishHours", Caption = "Latest Finish", FullDescription = "The latest this item can be finished.")]
		public ZDecimal LatestFinishHours
		{
			get { return MaybeGetFromSchedule(s => (s.BNC_LatestStartOffsetMinutes + s.BNC_DurationMinutes) / 60m); }
		}

		public ZPropertyInfo LatestFinishHoursInfo
		{
			get { return GetZPropertyInfo(nameof(LatestFinishHours)); }
		}

		#endregion

		#region EarliestStartTimeUtc

		public ZDateTime EarliestStartTimeUtc
		{
			get { return MaybeGetFromSchedule(s => s.BNC_EarliestStartUtc); }
			set { SetOnSchedule(s => s.BNC_EarliestStartUtcInfo, value, EarliestStartTimeUtcInfo); }
		}

		public ZPropertyInfo EarliestStartTimeUtcInfo
		{
			get { return GetZPropertyInfo(nameof(EarliestStartTimeUtc)); }
		}

		[ResourceStringData("BMNCNShape.EarliestStartTimeLocal", Caption = "Earliest Start", FullDescription = "The earliest time this item can be started.")]
		public ZDateTime EarliestStartTimeLocal
		{
			get { return EarliestStartTimeUtc.ToLocalBranchTime(); }
			set { EarliestStartTimeUtc = value.ToUniversalBranchTime(); }
		}

		public ZPropertyInfo EarliestStartTimeLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(EarliestStartTimeLocal), _ => EarliestStartTimeUtcInfo); }
		}

		#endregion

		#region EarliestFinishTimeUtc

		[ReadOnly(true)]
		public ZDateTime EarliestFinishTimeUtc
		{
			get { return MaybeGetFromSchedule(s => s.BNC_EarliestFinishUtc); }
			set { SetOnSchedule(s => s.BNC_EarliestFinishUtcInfo, value, EarliestFinishTimeUtcInfo); }
		}

		public ZPropertyInfo EarliestFinishTimeUtcInfo
		{
			get { return GetZPropertyInfo(nameof(EarliestFinishTimeUtc)); }
		}

		[ResourceStringData("BMNCNShape.EarliestFinishTimeLocal", Caption = "Earliest Finish", FullDescription = "The earliest time this item can be finished.")]
		public ZDateTime EarliestFinishTimeLocal
		{
			get { return EarliestFinishTimeUtc.ToLocalBranchTime(); }
		}

		public ZPropertyInfo EarliestFinishTimeLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(EarliestFinishTimeLocal), _ => EarliestFinishTimeUtcInfo); }
		}

		#endregion

		#region LatestStartTimeUtc

		[ReadOnly(true)]
		public ZDateTime LatestStartTimeUtc
		{
			get { return MaybeGetFromSchedule(s => s.BNC_LatestStartUtc); }
			set { SetOnSchedule(s => s.BNC_LatestStartUtcInfo, value, LatestStartTimeUtcInfo); }
		}

		public ZPropertyInfo LatestStartTimeUtcInfo
		{
			get { return GetZPropertyInfo(nameof(LatestStartTimeUtc)); }
		}

		[ReadOnly(true)]
		[ResourceStringData("BMNCNShape.LatestStartTimeLocal", Caption = "Latest Start", FullDescription = "The latest time this item can be started.")]
		public ZDateTime LatestStartTimeLocal
		{
			get { return LatestStartTimeUtc.ToLocalBranchTime(); }
		}

		public ZPropertyInfo LatestStartTimeLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LatestStartTimeLocal), _ => LatestStartTimeUtcInfo); }
		}

		#endregion

		#region LatestFinishTimeUtc

		public ZDateTime LatestFinishTimeUtc
		{
			get { return MaybeGetFromSchedule(s => s.BNC_LatestFinishUtc); }
			set { SetOnSchedule(s => s.BNC_LatestFinishUtcInfo, value, LatestFinishTimeUtcInfo); }
		}

		public ZPropertyInfo LatestFinishTimeUtcInfo
		{
			get { return GetZPropertyInfo(nameof(LatestFinishTimeUtc)); }
		}

		[ResourceStringData("BMNCNShape.LatestFinishTimeLocal", Caption = "Latest Finish", FullDescription = "The latest time this item can be finished.")]
		public ZDateTime LatestFinishTimeLocal
		{
			get { return LatestFinishTimeUtc.ToLocalBranchTime(); }
			set { LatestFinishTimeUtc = value.ToUniversalBranchTime(); }
		}

		public ZPropertyInfo LatestFinishTimeLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LatestFinishTimeLocal), _ => LatestFinishTimeUtcInfo); }
		}

		#endregion

		#region FloatHours

		[ReadOnly(true)]
		[ResourceStringData("BMNCNShape.FloatHours", Caption = "Float Hours", FullDescription = "The difference between the earliest start and latest start.")]
		public ZDecimal FloatHours
		{
			get { return MaybeGetFromSchedule(s => (s.BNC_LatestStartOffsetMinutes - s.BNC_EarliestStartOffsetMinutes) / 60m); }
		}

		public ZPropertyInfo FloatHoursInfo
		{
			get { return GetZPropertyInfo(nameof(FloatHours)); }
		}

		#endregion

		#region ExplicitDuration

		public ZInt ExplicitDurationMinutes
		{
			get { return MaybeGetFromSchedule(s => s.BNC_DurationMinutes); }
			set { SetOnSchedule(s => s.BNC_DurationMinutesInfo, value, ExplicitDurationMinutesInfo); }
		}

		public ZPropertyInfo ExplicitDurationMinutesInfo => GetZPropertyInfo(nameof(ExplicitDurationMinutes));

		[ReadOnly(true)]
		[ResourceStringData("BMNCNShape.ExplicitDurationHours", Caption = "Planned Duration", FullDescription = "The number of hours planned to complete this item.")]
		public ZDecimal ExplicitDurationHours => ExplicitDurationMinutes / 60m;

		public ZDateTime ExplicitDuration => new ZInt(ExplicitDurationMinutes).GetDateTimeFromMinutes();

		[ResourceStringData("BMNCNShape.ExplicitDurationLabel", Caption = "Planned Duration", FullDescription = "The amount of time planned to complete this item.")]
		public ZDecimal ExplicitDurationLabel => ExplicitDurationHours;

		[ResourceStringData("BMNCNShape.RemainingEstimatedDuration", Caption = "Linked Entity Remaining Standard Estimated Duration", ShortCaption = "Remaining Estimate", FullDescription = "The total of the estimate hours of all open tasks in this workflow.")]
		public ZDecimal RemainingEstimatedDuration => ProcessHeader != null ? ProcessHeader.RemainingEstimateHoursIncludingChildren : ZDecimal.Zero;

		[ResourceStringData("BMNCNShape.TotalActualDuration", Caption = "Linked Entity Completed Actual Duration", ShortCaption = "Actual Duration", FullDescription = "The total actual time recorded against all tasks on this workflow.")]
		public ZDecimal TotalActualDuration => ProcessHeader != null ? ProcessHeader.TotalActualHoursIncludingChildren : ZDecimal.Zero;

		[ResourceStringData("BMNCNShape.TotalEstimatedDuration", Caption = "Linked Entity Total Standard Estimated Duration", ShortCaption = "Total Estimate", FullDescription = "The total estimates of the tasks on this workflow, either the Standard Estimates or the Estimated Time to Complete if specified")]
		public ZDecimal TotalEstimatedDuration => ProcessHeader != null ? ProcessHeader.TotalRelevantEstimatedHours : ZDecimal.Zero;

		#endregion

		#region IsCriticalPath

		[ReadOnly(true)]
		[ResourceStringData("BMNCNShape.IsCriticalPath", Caption = "Critical Chain", FullDescription = "Indicates whether this item is on the critical chain of the approved diagram.")]
		public ZBool IsCriticalPath
		{
			get { return MaybeGetFromSchedule(s => s.BNC_IsCriticalPath); }
			set { SetOnSchedule(s => s.BNC_IsCriticalPathInfo, value, IsCriticalPathInfo); }
		}

		public ZPropertyInfo IsCriticalPathInfo
		{
			get { return GetZPropertyInfo(nameof(IsCriticalPath)); }
		}

		#endregion

		#region IsBuffered

		[ReadOnly(true)]
		[CopiedFromDiagramShapeOnCreation]
		public ZBool IsBuffered
		{
			get { return MaybeGetFromSchedule(s => s.BNC_IsBuffered); }
			set { SetOnSchedule(s => s.BNC_IsBufferedInfo, value, IsBufferedInfo); }
		}

		public ZPropertyInfo IsBufferedInfo
		{
			get { return GetZPropertyInfo(nameof(IsBuffered)); }
		}

		#endregion

		#region ScheduledStartTimeUtc

		[ReadOnly(true)]
		public ZDateTime ScheduledStartTimeUtc
		{
			get { return MaybeGetFromSchedule(s => s.BNC_ScheduledStartUtc); }
			set
			{
				SetOnSchedule(s => s.BNC_ScheduledStartUtcInfo, value, ScheduledStartTimeUtcInfo);
				if (!IsValidationSuspended)
				{
					Validation.ValidateScheduledStartTimeUtc();
					Validation.ValidateLinkedDiagramSchedulePropagation();
				}
			}
		}

		public ZPropertyInfo ScheduledStartTimeUtcInfo
		{
			get { return GetZPropertyInfo(nameof(ScheduledStartTimeUtc)); }
		}

		[ReadOnly(true)]
		[ResourceStringData("BMNCNShape.ScheduledStartTimeLocal", Caption = "Scheduled Start Time", FullDescription = "The time this item has been scheduled to start.")]
		public ZDateTime ScheduledStartTimeLocal
		{
			get { return ScheduledStartTimeUtc.ToLocalBranchTime(); }
			set { ScheduledStartTimeUtc = value.ToUniversalBranchTime(); }
		}

		public ZPropertyInfo ScheduledStartTimeLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ScheduledStartTimeLocal), _ => ScheduledStartTimeUtcInfo); }
		}

		#endregion

		#region ScheduledFinishTimeUtc

		[ReadOnly(true)]
		public ZDateTime ScheduledFinishTimeUtc
		{
			get { return MaybeGetFromSchedule(s => s.BNC_ScheduledFinishUtc); }
			set
			{
				SetOnSchedule(s => s.BNC_ScheduledFinishUtcInfo, value, ScheduledFinishTimeUtcInfo);

				if (!IsValidationSuspended)
				{
					Validation.ValidateDoNotStartBeforeDateViolation();
					Validation.ValidateScheduledFinishTimeUtc();
					Validation.ValidateLinkedDiagramSchedulePropagation();
				}
			}
		}

		public ZPropertyInfo ScheduledFinishTimeUtcInfo
		{
			get { return GetZPropertyInfo(nameof(ScheduledFinishTimeUtc)); }
		}

		[ReadOnly(true)]
		[ResourceStringData("BMNCNShape.ScheduledFinishTimeLocal", Caption = "Scheduled Finish Time", FullDescription = "The time this item has been scheduled to finish.")]
		public ZDateTime ScheduledFinishTimeLocal
		{
			get { return ScheduledFinishTimeUtc.ToLocalBranchTime(); }
			set { ScheduledFinishTimeUtc = value.ToUniversalBranchTime(); }
		}

		public ZPropertyInfo ScheduledFinishTimeLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ScheduledFinishTimeLocal), _ => ScheduledFinishTimeUtcInfo); }
		}

		#endregion

		#region Scaling

		#region Scale

		[BusinessObjectTestExclude] // This property is only used for minutes as a timespan, but the bizo property test tries to assert year as well
		[ResourceStringData("BMNCNShape.Scale", Caption = "Scale", FullDescription = "The scale in hours and minutes of the diagram.")]
		public ZDateTime Scale
		{
			get => GetScheduleScaleAsDateTime();
			set => SetScheduleScale(value);
		}

		ZDateTime GetScheduleScaleAsDateTime() => RootShape.MaybeGetFromSchedule(schedule => schedule.BNC_ScaleMagnitude).GetDateTimeFromMinutes();

		void SetScheduleScale(ZDateTime scale)
		{
			var scaleMinutes = new ZInt(SafeConvertToInt(scale.GetMinutesFromDateTimeSpan()));
			RootShape.SetOnSchedule(schedule => schedule.BNC_ScaleMagnitudeInfo, scaleMinutes, ScaleInfo);

			if (!RootShape.MaybeGetFromSchedule(schedule => schedule.BNC_ScaleUnit).Equals(ScaleUnitList.Codes.Hour))
			{
				RootShape.SetOnSchedule(schedule => schedule.BNC_ScaleUnitInfo, new ZString(ScaleUnitList.Codes.Hour), ScaleInfo);
			}
		}

		public ZPropertyInfo ScaleInfo
		{
			get { return GetZPropertyInfo(nameof(Scale)); }
		}

		#endregion

		int SafeConvertToInt(double value)
		{
			if (value >= (double)decimal.MaxValue)
			{
				return int.MaxValue;
			}
			if (value <= (double)decimal.MinValue)
			{
				return int.MinValue;
			}

			return Convert.ToInt32(value);
		}

		#region ResolutionIncrement

		[BusinessObjectTestExclude] // This property is only used for minutes as a timespan, but the bizo property test tries to assert year as well
		[ResourceStringData("BMNCNShape.ResolutionIncrement", Caption = "Resolution Increment", FullDescription = "The resolution increment that all shapes must abide to.")]
		public ZDateTime ResolutionIncrement
		{
			get => GetResolutionIncrementAsDateTime();
			set => SetScheduleResolutionIncrement(value);
		}

		ZDateTime GetResolutionIncrementAsDateTime() => RootShape.MaybeGetFromSchedule(schedule => schedule.BNC_ResolutionIncrement).GetDateTimeFromMinutes();

		void SetScheduleResolutionIncrement(ZDateTime scale)
		{
			var scaleMinutes = new ZInt(SafeConvertToInt(scale.GetMinutesFromDateTimeSpan()));
			RootShape.SetOnSchedule(schedule => schedule.BNC_ResolutionIncrementInfo, scaleMinutes, ScaleInfo);

			if (!RootShape.MaybeGetFromSchedule(schedule => schedule.BNC_ScaleUnit).Equals(ScaleUnitList.Codes.Hour))
			{
				RootShape.SetOnSchedule(schedule => schedule.BNC_ScaleUnitInfo, new ZString(ScaleUnitList.Codes.Hour), ScaleInfo);
			}
		}

		public ZPropertyInfo ResolutionIncrementInfo
		{
			get { return GetZPropertyInfo(nameof(ResolutionIncrement)); }
		}

		#endregion

		#endregion

		internal bool ShouldPropagateScheduleToLinkedDiagramNow()
		{
			if (CanPotentiallyPropagateScheduleToLinkedDiagram)
			{
				var startTimeUtc = ScheduledStartTimeUtc;
				var finishTimeUtc = ScheduledFinishTimeUtc;

				if (startTimeUtc.IsValid || finishTimeUtc.IsValid)
				{
					var linkedDiagram = RelatedShape;

					return startTimeUtc != linkedDiagram.ScheduledStartTimeUtc
						|| finishTimeUtc != linkedDiagram.ScheduledFinishTimeUtc;
				}
			}

			return false;
		}

		internal bool PropagationWouldChangePinnedShapeSchedules()
		{
			if (CanPotentiallyPropagateScheduleToLinkedDiagram)
			{
				var linkedDiagram = LinkedDiagram;

				return linkedDiagram != null
					&& linkedDiagram.ScheduledStartTimeUtc != ScheduledStartTimeUtc
					&& linkedDiagram.AllNestedShapes.Any(s => s.IsPinned);
			}

			return false;
		}

		internal bool CanPotentiallyPropagateScheduleToLinkedDiagram => CanHaveSchedule && ShouldSynchroniseScheduleWithLinkedEntity && IsScaled && IsLinkedToScaledDiagram;

		bool IsLinkedToScaledDiagram => LinkedDiagram?.IsScaled ?? false;

		internal static IEnumerable<BMNCNShape> FindShapesWhichCanPropagateScheduleToLinkedEntity(BusinessObjectFactory factory, ZGuid linkedEntityPK)
		{
			if (!linkedEntityPK.IsEmpty)
			{
				var query = new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, linkedEntityPK);

				return factory.Load<BMNCNShape>(query).Where(s => s.CanPotentiallyPropagateScheduleToLinkedDiagram);
			}

			return Enumerable.Empty<BMNCNShape>();
		}

		#endregion

		#region New Properties

		public string ApprovedDiagramType
		{
			get
			{
				if (IsApproved)
				{
					return IsBuffered
						? ApprovedDiagramTypeList.Codes.CCPMApprovedDiagram
						: ApprovedDiagramTypeList.Codes.NCNApprovedDiagram;
				}
				else
				{
					return ApprovedDiagramTypeList.Codes.NonApprovedDiagram;
				}
			}
		}

		public void RequireDiagram()
		{
			if (!IsDiagram)
			{
				throw new InvalidOperationException("Shape must be a diagram");
			}
		}

		public bool IsDiagram
		{
			get
			{
				switch (BNS_ShapeType)
				{
					case ShapeTypeList.Codes.DefaultDiagram:
					case ShapeTypeList.Codes.Diagram:
						return true;

					default:
						return false;
				}
			}
		}

		public bool IsEntityShape
		{
			get
			{
				switch (BNS_ShapeType)
				{
					case ShapeTypeList.Codes.Shape:
					case ShapeTypeList.Codes.Diagram:
						return true;

					default:
						return false;
				}
			}
		}

		public bool IsDefaultDiagram
		{
			get { return BNS_ShapeType == ShapeTypeList.Codes.DefaultDiagram; }
		}

		public bool IsDefaultDiagramChild
		{
			get { return this.IsDefaultDiagramChild(); }
		}

		public bool IsBufferShape
		{
			get { return BNS_ShapeType == ShapeTypeList.Codes.Buffer; }
		}

		public bool IsAnnotation
		{
			get { return BNS_ShapeType == ShapeTypeList.Codes.Annotation; }
		}

		[ResourceStringData("BMNCNShape.ShapeTypeDescription", Caption = "Shape Type")]
		public ZString ShapeTypeDescription
		{
			get { return Lookups.ShapeTypes.GetDescriptionFromCode(BNS_ShapeType); }
		}

		[ResourceStringData("BMNCNShape.JobTypeDescription", Caption = "Job Type")]
		public ZString JobTypeDescription
		{
			get { return Lookups.JobTypes.GetDescriptionFromCode(BNS_JobType); }
		}

		public bool IsLinkedToRealEntity
		{
			get
			{
				var processHeader = ProcessHeader;
				return processHeader != null && processHeader.FH_ParentId.IsValid;
			}
		}

		public bool IsLinkedToRealEntityEnabledForBMS
		{
			get
			{
				return IsLinkedToRealEntity
					&& (!(ProcessHeader is ProcessJobHeader jobHeader)
						|| ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(jobHeader.Parent, Factory));
			}
		}

		public bool SupportsChanneling => SupportsChannelingCore();

		protected virtual bool SupportsChannelingCore()
		{
			return false;
		}

		#endregion

		#region Related Business Objects

		#region Shapes/Attachments

		#region Shapes

		public bool HasParent => BNS_BNS_ParentShape.IsValid;

		public BMNCNShape ParentShape => Factory.Load<BMNCNShape>(BNS_BNS_ParentShape);

		public BMNCNShape RootShape => Factory.Load<BMNCNShape>(RootShapePK);

		public ZGuid RootShapePK => BNS_BNS_RootShape.IsValid ? BNS_BNS_RootShape : PK;

		public BMNCNShapeCollection ChildShapes
		{
			get
			{
				if (childShapes == null)
				{
					childShapes = BMNCNShapeCollection.ForChildShapes(this);
				}

				return childShapes;
			}
		}

		BMNCNShapeCollection childShapes;

		public IEnumerable<BMNCNShape> PrerequisiteShapes
		{
			get
			{
				return from a in DependencyAttachments
					   where a.ToShape == this
					   select a.FromShape;
			}
		}

		public IEnumerable<BMNCNShape> PostrequisiteShapes
		{
			get
			{
				return from a in DependencyAttachments
					   where a.BNA_BNS_FromShape == PK
					   select a.ToShape;
			}
		}

		public RelatedDiagramViewCollection RelatedDiagrams
		{
			get
			{
				if (relatedDiagrams == null)
				{
					relatedDiagrams = new RelatedDiagramViewCollection(this);
				}
				return relatedDiagrams;
			}
		}
		RelatedDiagramViewCollection relatedDiagrams;

		#endregion

		#region Attachments

		[ChildEditable]
		public BMNCNAttachmentCollection AllAttachments
		{
			get
			{
				if (allAttachments == null)
				{
					allAttachments = new BMNCNAttachmentCollection(this);
					RegisterEditableChildObject(allAttachments);
				}

				return allAttachments;
			}
		}

		BMNCNAttachmentCollection allAttachments;

		BMNCNAttachment[] AllAttachmentsToDelete => Factory.Load<BMNCNAttachment>(GetAllAttachmentQuery());

		internal ZQuery GetAllAttachmentQuery()
		{
			var query = new ZQuery(BMNCNAttachmentSchema.BNA_BNS_Owner, PK);
			query.AddToFilter(JoinCondition.Or, BMNCNAttachmentSchema.BNA_BNS_ToShape, PK);
			query.AddToFilter(JoinCondition.Or, BMNCNAttachmentSchema.BNA_BNS_FromShape, PK);
			query.AddToFilter(BMNCNAttachmentSchema.BNA_Type, SQLComparisonOperator.NotEqual, AttachmentTypeList.Codes.RelatedBuffer);
			query.FetchOnlyFromLocalCache = true; // Attachments should have been loaded efficiently in one hit within JobNetwork.InitialiseShapesAndAttachments. This is here to avoid O(n)+ hits.

			return query;
		}

		public BMNCNAttachmentCollection ChildDependencyAttachments
		{
			get
			{
				if (childDependencyAttachments == null)
				{
					var query = new ZQuery(BMNCNAttachmentSchema.BNA_BNS_Owner, PK);
					query.AddToFilter(BMNCNAttachmentSchema.BNA_FP_ProcessHeaderLink, SQLComparisonOperator.NotEqual, null);

					childDependencyAttachments = new BMNCNAttachmentCollection(Factory, query);
				}

				return childDependencyAttachments;
			}
		}

		BMNCNAttachmentCollection childDependencyAttachments;

		public BMNCNAttachmentCollection DependencyAttachments
		{
			get
			{
				if (dependencyAttachments == null)
				{
					var query = GetDependencyAttachmentQuery(this);
					dependencyAttachments = new BMNCNAttachmentCollection(Factory, query);
				}

				return dependencyAttachments;
			}
		}

		BMNCNAttachmentCollection dependencyAttachments;

		static ZQuery GetDependencyAttachmentQuery(BMNCNShape shape, params BMNCNShape[] owners)
		{
			var pk = shape.PK;
			var query = new ZQuery();
			var shapePKQuery = new ZQuery();
			shapePKQuery.AddToFilter(BMNCNAttachmentSchema.BNA_BNS_FromShape, pk);
			shapePKQuery.AddToFilter(new ZQuery(BMNCNAttachmentSchema.BNA_BNS_ToShape, pk), JoinCondition.Or);

			query.AddToFilter(shapePKQuery);
			query.AddToFilter(BMNCNAttachmentSchema.BNA_BNS_FromShape, SQLComparisonOperator.NotEqual, null);

			if (owners.Length > 0)
			{
				query.AddToFilter(BMNCNAttachmentSchema.BNA_BNS_Owner, owners.Select(s => s.PK).ToArray());
			}
			query.FetchOnlyFromLocalCache = !shape.IsInDatabase;

			return query;
		}

		public static void AddDependencyAttachmentsFetchHints(IEnumerable<BMNCNShape> shapes)
		{
			if (shapes == null || !shapes.Any())
			{
				return;
			}
			var factory = shapes.First().Factory;

			foreach (var shape in shapes)
			{
				factory.AddFetchHint(BMNCNAttachmentSchema.Instance, GetDependencyAttachmentQuery(shape));
			}
		}

		#endregion

		#region Is/Make

		public bool IsChildOf(IBMNCNShape parentShape)
		{
			return BNS_BNS_ParentShape == parentShape.Identifier;
		}

		public void MakeChildOf(IBMNCNShape parentShape)
		{
			CloneParentProperties(parentShape);
			IsNonScheduled = parentShape.AsShape().IsNonScheduled;
		}

		#endregion

		#region Related Buffers

		public IEnumerable<BMNCNBufferShape> GetRelatedBuffers()
		{
			foreach (var attachment in GetRelatedBufferAttachments())
			{
				var buffer = attachment.OwnerShape as BMNCNBufferShape;
				if (buffer != null)
				{
					yield return buffer;
				}
			}
		}

		internal IEnumerable<BMNCNAttachment> GetRelatedBufferAttachments()
		{
			return AllAttachments.Where(a => a.BNA_Type == AttachmentTypeList.Codes.RelatedBuffer && a.BNA_BNS_Owner != PK);
		}

		#endregion

		#endregion

		#region LinkedEntity

		/// <summary>
		/// Returns null if this object IsDeleted. Otherwise, returns the corresponding LinkedEntity
		/// </summary>
		public BusinessObject LinkedEntity => IsDeleted ? null : Factory.Load(BNS_RelatedEntityTableCode, BNS_RelatedEntityID);

		public ProcessHeader ProcessHeader => LinkedEntity as ProcessHeader;

		public ZString? LinkedProcessHeaderDescription => ProcessHeader?.FH_CompletionStatement;

		public BMNCNShape RelatedShape => LinkedEntity as BMNCNShape;

		public BMNCNRootDiagramShape LinkedDiagram => LinkedEntity as BMNCNRootDiagramShape;

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		public ControllerID GetLinkedEntityControllerID()
		{
			switch (LinkedEntity)
			{
				case ProcessHeader x:
					return ControllerIDs.ProcessHeader;
				case BMNCNShape x:
					return ControllerIDs.NetworkDiagram;

				default:
					return null;
			}
		}

		#endregion

		#region ProcessJobHeader

		public ProcessJobHeader ProcessJobHeader
		{
			get { return Factory.Load(ProcessHeaderSchema.Constants.Prefix, BNS_RelatedEntityID) as ProcessJobHeader; }
		}

		#endregion

		#region Workflow And Job Related To The Root Diagram

		public ZString? RootShapeWorkflowDescription => RootShape.LinkedProcessHeaderDescription;

		public ZString? RootShapeJobCodeAndDescription => RootShape.ProcessHeader?.ParentJobDescription;

		#endregion

		#region Schedule

		public BMNCNSchedule ScheduleBizo
		{
			get
			{
				if (scheduleBizo_DoNotTouch == null || scheduleBizo_DoNotTouch.IsDeleted || scheduleBizo_DoNotTouch.BNC_BNS_Shape != PK)
				{
					scheduleBizo_DoNotTouch = Factory.LoadTop1<BMNCNSchedule>(new ZQuery(BMNCNScheduleSchema.BNC_BNS_Shape, PK)
					{
						FetchOnlyFromLocalCache = !IsInDatabase
					});
					RegisterEditableChildObject(scheduleBizo_DoNotTouch);
				}
				return scheduleBizo_DoNotTouch;
			}
		}

		BMNCNSchedule scheduleBizo_DoNotTouch;

		public BMNCNSchedule GetOrCreateSchedule()
		{
			var schedule = ScheduleBizo;

			if (!CanHaveSchedule)
			{
				ErrorReporter.ReportOnce("20228bab-639a-4c92-935f-c5442d3e8fa2", "Trying to get a BMNCNSchedule for a shape that unambiguously shouldn't have one.");
			}
			else if (schedule == null)
			{
				schedule = Factory.New<BMNCNSchedule>();

				using (schedule.SuspendSettingHasChanges())
				{
					schedule.BNC_BNS_Shape = PK;
				}

				RegisterEditableChildObject(schedule);
			}

			return schedule;
		}

		internal bool CanHaveSchedule
		{
			get
			{
				if (IsDeleted || IsNonScheduled)
				{
					return false;
				}
				else
				{
					switch (BNS_ShapeType)
					{
						case ShapeTypeList.Codes.Annotation:
						case ShapeTypeList.Codes.DefaultDiagram:
							return false;

						default:
							return true;
					}
				}
			}
		}

		T MaybeGetFromSchedule<T>(Func<BMNCNSchedule, T> func)
		{
			var schedule = ScheduleBizo;
			return schedule != null ? func(schedule) : default(T);
		}

		void SetOnSchedule(Func<BMNCNSchedule, ZPropertyInfo> infoGetter, IZType value, ZPropertyInfo propertyInfo)
		{
			if (CanHaveSchedule)
			{
				var schedule = GetOrCreateSchedule();
				var info = infoGetter(schedule);
				if (!info.Value.Equals(value))
				{
					info.Value = value;
					propertyInfo.RefreshBinding();
				}
			}
		}

		void RemoveSchedule()
		{
			ScheduleBizo?.Delete();
			scheduleBizo_DoNotTouch = null;
		}

		#endregion

		#endregion

		#region IProposedNetworkEntity Members

		#region BMNCNShapeNetworkEntity

		class BMNCNShapeNetworkEntity : IProposedNetworkEntity
		{
			internal BMNCNShapeNetworkEntity(BMNCNShape shape)
			{
				this.shape = shape;
			}

			readonly BMNCNShape shape;

			public string Description
			{
				get { return shape.IsBufferShape && !shape.Active ? Res.GetString("adf2d209-9a64-488b-aafa-418d1aa650f5", "This buffer has not been accepted yet") : string.Empty; }
			}

			public string EstimateSummary
			{
				get { return string.Empty; }
			}

			public bool IsOnCriticalPath
			{
				get { return false; }
			}

			public bool CanUnlinkEntity
			{
				get { return false; }
			}

			public bool IsStartable
			{
				get { return true; }
			}

			public IEnumerable<IEntityRelationship> Links
			{
				get { return links ?? (links = new EntityCollectionWrapper<IEntityRelationship>(shape.DependencyAttachments)); }
			}

			EntityCollectionWrapper<IEntityRelationship> links;

			public string Name
			{
				get { return shape.BNS_Name; }
				set { shape.BNS_Name = value; }
			}

			public string JobNumber
			{
				get { return string.Empty; }
			}

			public string JobName
			{
				get { return shape.JobName; }
				set { shape.JobName = value; }
			}

			bool IProposedNetworkEntity.JobName_ReadOnly
			{
				get { return false; }
			}

			IProposedNetworkEntity IProposedNetworkEntity.Parent
			{
				get { return null; }
			}

			public IEnumerable<IEntityRelationship> PostRequisiteLinks
			{
				get { return Links.Where(r => r.From == shape); }
			}

			public int PreRequisiteDepth
			{
				get { return shape.GetPreRequisiteDepth(); }
			}

			public IEnumerable<IEntityRelationship> PreRequisiteLinks
			{
				get { return Links.Where(r => r.To == shape); }
			}

			public WorkStatus Status
			{
				get
				{
					switch (shape.BNS_Status)
					{
						case ShapeStatusList.Codes.Suspended:
							return WorkStatus.Suspended;

						case ShapeStatusList.Codes.Working:
							return WorkStatus.Working;

						case ShapeStatusList.Codes.Closed:
							return WorkStatus.Complete;

						case ShapeStatusList.Codes.Cancelled:
							return WorkStatus.Cancelled;

						case ShapeStatusList.Codes.Assigned:
						case ShapeStatusList.Codes.Open:
						case ShapeStatusList.Codes.Unknown:
							return WorkStatus.None;

						default:
							throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unexpected Status was found [{0}]", shape.BNS_Status));
					}
				}
			}

			public string StatusDescription
			{
				get { return string.Empty; }
			}

			public string StatusName
			{
				get { return string.Empty; }
			}

			public bool IsSameEntity(IProposedNetworkEntity other)
			{
				return shape.IsSameEntity(other);
			}

			event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
			{
				add { }
				remove { }
			}

			public bool CanDeleteUnderlyingEntity
			{
				get { return false; }
			}

			public string CompletionCriteria
			{
				get { return shape.BNS_CompletionStatements; }
				set { shape.BNS_CompletionStatements = value; }
			}
		}

		#endregion

		IProposedNetworkEntity NetworkEntity => (IProposedNetworkEntity)ProcessHeader ?? ShapeNetworkEntity;

		BMNCNShapeNetworkEntity ShapeNetworkEntity
		{
			get { return shapeNetworkEntity ?? (shapeNetworkEntity = new BMNCNShapeNetworkEntity(this)); }
		}

		BMNCNShapeNetworkEntity shapeNetworkEntity;

		public bool IsOnCriticalPath
		{
			get { return NetworkEntity.IsOnCriticalPath; }
		}

		public bool CanUnlinkEntity
		{
			get
			{
				return !IsDefaultDiagram
					&& !IsDefaultDiagramChild
					&& LinkedEntity != null;
			}
		}

		public string Name
		{
			get { return BNS_Name; }
			set
			{
				var wereValuesInSync = NetworkEntity.Name == Name;

				BNS_Name = value;

				if (wereValuesInSync && NetworkEntity.Name != value)
				{
					NetworkEntity.Name = value;
				}

				OnPropertyChanged();
			}
		}

		public string JobNumber
		{
			get { return NetworkEntity.JobNumber; }
		}

		public bool IsSameEntity(IProposedNetworkEntity other)
		{
			var entity = other as INetworkEntity;
			if (entity != null)
			{
				return ((INetworkEntity)this).EntityPK == entity.EntityPK;
			}

			var processHeader = other as ProcessHeader;
			if (processHeader != null)
			{
				return processHeader.PK == BNS_RelatedEntityID;
			}

			return false;
		}

		public bool CanDeleteUnderlyingEntity => NetworkEntity.CanDeleteUnderlyingEntity;

		#region OnPropertyChanged

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
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

		public event PropertyChangedEventHandler PropertyChanged;

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

		#endregion

		#region INetworkEntity Members

		public string ShapeType
		{
			get
			{
				switch (BNS_ShapeType)
				{
					case ShapeTypeList.Codes.Annotation:
						return ShapeTypes.Annotation;

					case ShapeTypeList.Codes.Buffer:
						return ShapeTypeList.Codes.Buffer;

					default:
						return ShapeTypes.Shape;
				}
			}
		}

		bool INetworkEntity.IsNonScheduled
		{
			get => IsNonScheduled;
			set => IsNonScheduled = value;
		}

		public int ScaleUnitPixelSize
		{
			get { return (int)CCPMConstants.ScaledModeDiagramPixelsPerScaleUnit; }
		}

		#region Supported Actions

		public NetworkActions SupportedActions
		{
			get
			{
				var result = NetworkActions.GenericActions;

				if (!IsDefaultDiagram && !IsDefaultDiagramChild)
				{
					result |= NetworkActions.Hide
						| NetworkActions.Show
						| NetworkActions.StyleDiagram
						| NetworkActions.EditEntity
						| NetworkActions.AddChildEntities;
				}

				if (!IsAnnotation && !IsBufferShape)
				{
					result |= NetworkActions.Affinities;
				}

				return result;
			}
		}

		#endregion

		public bool CanCreateRelationship(INetworkEntity other)
		{
			return ShapeType != ShapeTypeList.Codes.Buffer && (other == null || other.ShapeType != ShapeTypeList.Codes.Buffer);
		}

		public new bool HasNotifications
		{
			get { return !IsDeleted && base.HasNotifications(); }
		}

		void Shape_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			OnPropertyChanged(nameof(HasNotifications));
		}

		#endregion

		#region INetworkEntityWithChildren Members

		public bool CanHaveChildren => !IsBufferShape && !IsAnnotation;

		#endregion

		#region Scaled Mode

		public void SwitchToScaled()
		{
			IsScaled = ZBool.True;
			Scale = ResolutionIncrement = GetDefaultScale();
		}

		static ZDateTime GetDefaultScale()
		{
			return new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
		}

		#endregion

		#region ModuleFilterConstants

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Module filter 'name'")]
		public static class ModuleFilterConstants
		{
			public const string DiagramType = "Diagram Type";
			public const string Name = "Name";
			public const string JobType = "Job Type";
			public const string CompletionStatements = "Completion Statements";
			public const string LinkedEntity = "Linked Entity";
			public const string ChildEntity = "Contains Child Entity";
			public const string ChildName = "Contains Child Name";
			public const string ApprovedBy = "Approved By";
			public const string Status = "Status";
			public const string Scaled = "Scaled";
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.NetworkDiagram)); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region Clone

		public ZGuid ClonedFromPK { get; private set; }

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[]
			{
				BMNCNShapeSchema.BNS_GS_NKApprovedBy.Name
			});

			var clone = (BMNCNShape)base.CloneInternal(args);
			clone.ClonedFromPK = PK;
			clone.BNS_RelatedEntityID = BNS_RelatedEntityID;
			clone.ShapeAffinityLinks.RemoveAndDeleteAll();
			if (clone.BNS_ShapeType == ShapeTypeList.Codes.DefaultWorkflow)
			{
				clone.Name = ProcessHeader.FH_CompletionStatement;
				clone.BNS_ShapeType = ShapeTypeList.Codes.Shape;
			}

			var schedule = ScheduleBizo;

			if (schedule != null)
			{
				var clonedSchedule = clone.ScheduleBizo;
				if (clonedSchedule == null)
				{
					var scheduleCloneArgs = new BusinessObjectCloneArgs(args.AlternativeFactoryToInstantiateCloneIn, Array.Empty<string>(), typeof(BMNCNSchedule), false);
					clonedSchedule = (BMNCNSchedule)schedule.Clone(scheduleCloneArgs);
					clonedSchedule.BNC_BNS_Shape = clone.PK;
				}
				else
				{
					clonedSchedule.CopyPersistentValuesFrom(schedule, new BusinessObjectCloneArgs(new[] { BMNCNScheduleSchema.Constants.BNC_BNS_Shape }));
				}
			}

			return clone;
		}

		public void CloneParentProperties(IBMNCNShape parentShape)
		{
			var owner = parentShape.AsShape();
			if (IsScaled != owner.IsScaled)
			{
				if (IsInDatabase)
				{
					throw new InvalidOperationException("Child scale doesn't match parents scale value. This import operation is by-passing validation somehow.");
				}
				else
				{
					IsScaled = owner.IsScaled;
					Scale = owner.Scale;
					ResolutionIncrement = owner.ResolutionIncrement;
				}
			}
			BNS_BNS_ParentShape = parentShape.Identifier;
			BNS_BNS_RootShape = ((BMNCNShape)parentShape).RootShapePK;
		}

		#endregion

		#region IBMNCNShape Members

		IBMNCNShape IBMNCNShape.RootDiagram
		{
			get { return FindTopmostDiagramShape(); }
		}

		void IBMNCNShape.AddFetchHintsForBoardLoad(IEnumerable<IBMNCNShape> shapes)
		{
			foreach (var shape in shapes.Cast<BMNCNShape>())
			{
				Factory.AddFetchHint(BMNCNAttachmentSchema.Instance, shape.AllAttachments.CompleteFilter);
			}

			foreach (var attachment in shapes.Cast<BMNCNShape>().SelectMany(s => s.GetRelatedBufferAttachments()))
			{
				Factory.AddFetchHint(BMNCNShapeSchema.PK, attachment.BNA_BNS_Owner);
			}
		}

		void IBMNCNShape.DisconnectRelatedEntity()
		{
			if (BNS_RelatedEntityID.IsValid)
			{
				foreach (var attachment in AllAttachments)
				{
					if (attachment.BNA_FP_ProcessHeaderLink.IsValid)
					{
						var link = attachment.ProcessHeaderLink;

						if (link.FP_FH_HeaderFrom == BNS_RelatedEntityID || link.FP_FH_HeaderTo == BNS_RelatedEntityID)
						{
							attachment.BNA_FP_ProcessHeaderLink = ZGuid.Empty;
						}
					}
				}

				BNS_RelatedEntityID = ZGuid.Empty;
			}
		}

		#endregion

		#region ILinkEntity Members

		Guid ILinkEntity.PK
		{
			get { return PK.ToGuid(); }
		}

		bool ILinkEntity.IsLeaf
		{
			get { return !ChildShapes.Any(); }
		}

		DateTime ILinkEntity.AgreedDeliveryDateInUtc
		{
			get
			{
				var processHeader = ProcessHeader;
				return processHeader != null && processHeader.ApplicableAgreedDeliveryDateUtc.IsValid && !processHeader.ApplicableAgreedDeliveryDateUtc.IsEmpty ? processHeader.ApplicableAgreedDeliveryDateUtc.ToDateTime() : default(DateTime);
			}
		}

		string ILinkEntity.DisplayName
		{
			get { return Name; }
		}

		IReadOnlyCollection<ILink> ILinkEntity.Links
		{
			get { return AllAttachments.Where(BMNCNAttachment.IsApplicableForScheduling).ToList(); }
		}

		ILinkDescendantsStrategy ILinkEntity.GetDefaultDescendantsStrategy()
		{
			return new BMNCNShapeDescendantsStrategy();
		}

		#endregion

		#region IApprovable Members

		public bool IsApproved
		{
			get { return !BNS_GS_NKApprovedBy.IsEmpty && CanApprove; }
		}

		public bool HasApprovedParent => ParentShape?.IsApproved ?? false;

		[SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		internal bool CanApprove
		{
			get
			{
				if (IsNonScheduled)
				{
					return false;
				}

				switch (BNS_ShapeType)
				{
					case ShapeTypeList.Codes.Diagram:
					case ShapeTypeList.Codes.Shape:
					case ShapeTypeList.Codes.Buffer:
						return true;
					case ShapeTypeList.Codes.DefaultDiagram:
					case ShapeTypeList.Codes.DefaultWorkflow:
					case ShapeTypeList.Codes.Annotation:
						return false;
					default:
						ErrorReporter.ReportOnce(string.Format(CultureInfo.CurrentCulture, "Unhandled type [{0}]", BNS_ShapeType));
						return false;
				}
			}
		}

		void IApprovable.Approve(string approverCode)
		{
			ApproveCore(approverCode);
		}

		public void UnApprove()
		{
			ApproveCore(ZString.Empty);
		}

		protected virtual void ApproveCore(string approverCode)
		{
			using (AllowChangingApproval())
			{
				BNS_GS_NKApprovedBy = approverCode;
			}
		}

		internal BMNCNShape FindApprovedRoot()
		{
			if (!IsApproved)
			{
				throw new InvalidOperationException("No approved root possible for unapproved diagram.");
			}
			else
			{
				var rootShape = RootShape;

				return rootShape.IsApproved ? rootShape : null;
			}
		}

		public string TopmostDiagramShapeName => FindTopmostDiagramShape().Name;

		#endregion

		#region IBufferedItem Members

		IReadOnlyCollection<IBuffer> IBufferedItem.GetRelatedBuffers(bool includeNetworkBuffers)
		{
			return GetRelatedBuffers().ToList();
		}

		IReadOnlyCollection<IBuffer> IBufferedItem.RelatedSubBuffers
		{
			get { return new List<IBuffer>(); }
		}

		DateTime IBufferedItem.StartableTime
		{
			get { return ScheduledStartTimeUtc.IsValid && !ScheduledStartTimeUtc.IsEmpty ? ScheduledStartTimeUtc.ToDateTime() : default(DateTime); }
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		int IBufferedItem.RemainingEstimateInMinutes
		{
			get
			{
				var processHeader = ProcessHeader;
				if (processHeader != null)
				{
					return (int)(processHeader.RemainingEstimateHoursIncludingChildren * 60);
				}
				else
				{
					return ExplicitDurationMinutes;
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		int IBufferedItem.PlannedDurationInMinutes
		{
			get { return ExplicitDurationMinutes; }
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		int IBufferedItem.PenetrationMinutesWithoutAging
		{
			get { return this.CalculatePenetrationMinutesWithoutAging(); }
		}

		WorkStatus IBufferedItem.WorkStatus
		{
			get { return ((IProposedNetworkEntity)this).Status; }
		}

		bool IBufferedItem.IsClosed
		{
			get
			{
				var processHeader = ProcessHeader;

				if (processHeader != null)
				{
					return processHeader.IsClosed;
				}

				var status = ((IBufferedItem)this).WorkStatus;

				switch (status)
				{
					case WorkStatus.Cancelled:
					case WorkStatus.Complete:
						return true;

					default:
						return false;
				}
			}
		}

		#endregion

		#region IBufferedItemWithRelatedEntity Members

		public string RelatedEntityName
		{
			get { return ProcessHeader != null ? ProcessHeader.ProviderJobDescription : ZString.Empty; }
		}

		public bool HasBufferPenetrationWhenClosed => false;

		#endregion

		#region IBranchDepartmentProvider Members

		IGlbBranch IBranchDepartmentProvider.GetBranch(BusinessObjectFactory factory)
		{
			return ScheduleBizo?.GetBranch(factory ?? Factory);
		}

		IGlbDepartment IBranchDepartmentProvider.GetDepartment(BusinessObjectFactory factory)
		{
			return ScheduleBizo?.GetDepartment(factory ?? Factory);
		}

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(BMNCNShapeSchema.BNS_BNS_RootShape, null);
			}
		}

		#endregion

		#region Top most

		public BMNCNShape FindTopmostDiagramShape()
		{
			return RootShape ?? this;
		}

		#endregion

		#region Validation

		public bool ShouldValidateLoopsOnOpen { get; set; }

		#endregion

		#region For Test
#if DEBUG

		public override string ToString() => DisplayText_ForTest;

		public string DisplayText_ForTest => string.Format(CultureInfo.InvariantCulture, "Name: {0}, LinkedEntity: {1}", BNS_Name, LinkedEntity?.HumanReadableName ?? "None");

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (!BNS_BNS_ParentShape.IsEmpty && IsDiagram)
			{
				// Framework tests sometimes create arbitrary records and add them to collections, which sets BNS_BNS_ParentShape and BNS_BNS_RootShape, but doesn't consider that these shouldn't be set for non-diagram shapes.
				// Here we fix this, retaining the parent shape set by the framework.

				BNS_ShapeType = ShapeTypeList.Codes.Shape;
			}
		}

#endif
		#endregion
	}
}
