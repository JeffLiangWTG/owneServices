using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[System.Diagnostics.DebuggerDisplay("{System?.FS_Name}: {FC_DisplaySequence} {FC_Name}")]
	[CodeProperty(BMComponent.Schema.FC_Name), DescriptionProperty(BMComponent.Schema.FC_Name)]
	public class BMComponent : AutoBMComponent,
		IBMComponent,
		IBuffer,
		IBranchDepartmentProvider,
		IAuditParent
	{
		public BMComponent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		[ChildEditable(true)]
		public BMZoneCapacityMultiplierCollection ZoneCapacityMultipliers
		{
			get
			{
				if (zoneCapacityMultipliers == null)
				{
					zoneCapacityMultipliers = new BMZoneCapacityMultiplierCollection(this);
					RegisterEditableChildObject(zoneCapacityMultipliers);
				}
				return zoneCapacityMultipliers;
			}
		}
		BMZoneCapacityMultiplierCollection zoneCapacityMultipliers;

		[ChildEditable(true)]
		public BMComponentLinkDependentCollection FromMeToOthersLinks
		{
			get
			{
				if (fromMeToOthersLinks == null)
				{
					fromMeToOthersLinks = new BMComponentLinkDependentCollection(this, BMComponentLinkSchema.FL_FC_ComponentFrom);
					RegisterEditableChildObject(fromMeToOthersLinks);
				}
				return fromMeToOthersLinks;
			}
		}
		BMComponentLinkDependentCollection fromMeToOthersLinks;

		[ChildEditable(true)]
		public BMComponentLinkDependentCollection FromOthersToMeLinks
		{
			get
			{
				if (fromOthersToMeLinks == null)
				{
					fromOthersToMeLinks = new BMComponentLinkDependentCollection(this, BMComponentLinkSchema.FL_FC_ComponentTo);
					RegisterEditableChildObject(fromOthersToMeLinks);
				}
				return fromOthersToMeLinks;
			}
		}
		BMComponentLinkDependentCollection fromOthersToMeLinks;

		public List<BMComponentLink> GetFromOthersToMeLinksWithinSystem(BMSystem system)
		{
			return FromOthersToMeLinks.
				Where(link => link.ComponentTo.FC_FS_System == system.PK && link.ComponentFrom.FC_FS_System == system.PK)
				.ToList();
		}

		[ChildEditable(true)]
		public BMComponentCollection ChildComponents
		{
			get
			{
				if (childComponents == null)
				{
					childComponents = new BMComponentCollection(this);
					RegisterEditableChildObject(childComponents);
				}
				return childComponents;
			}
		}
		BMComponentCollection childComponents;

		[ChildEditable]
		public BMComponentResourceLinkCollection ResourceLinks
		{
			get
			{
				if (resourceLinks == null)
				{
					resourceLinks = new BMComponentResourceLinkCollection(this);
					RegisterEditableChildObject(resourceLinks);
				}

				return resourceLinks;
			}
		}
		BMComponentResourceLinkCollection resourceLinks;

		[ChildEditable]
		public BMComponentReleaseGroupLinkCollection ReleaseGroupLinks
		{
			get
			{
				if (releaseGroupLinks == null)
				{
					releaseGroupLinks = new BMComponentReleaseGroupLinkCollection(this);
					RegisterEditableChildObject(releaseGroupLinks);
				}

				return releaseGroupLinks;
			}
		}
		BMComponentReleaseGroupLinkCollection releaseGroupLinks;

		public BMComponentAcceptabilityBandCollection AcceptabilityBands
		{
			get
			{
				if (acceptabilityBands == null)
				{
					acceptabilityBands = new BMComponentAcceptabilityBandCollection(this);
				}

				return acceptabilityBands;
			}
		}

		BMComponentAcceptabilityBandCollection acceptabilityBands;

		public override void Delete()
		{
			ZoneCapacityMultipliers.DeleteAll();
			ChildComponents.DeleteAll();
			FromMeToOthersLinks.DeleteAll();
			FromOthersToMeLinks.DeleteAll();
			ReleaseGroupLinks.DeleteAll();
			AcceptabilityBands.DeleteAll();
			ResourceLinks.DeleteAll();

			base.Delete();
		}

		public BMComponentResourceLink GetOrCreateResourceLink(ZString staffCode)
		{
			var componentLink = GetResourceLink(staffCode);
			if (componentLink == null)
			{
				componentLink = Factory.New<BMComponentResourceLink>();
				componentLink.FD_FC_Component = PK;
				componentLink.FD_GS_NKResource = staffCode;
				componentLink.FD_CapacityLimitPercent = 100;
			}

			return componentLink;
		}

		public BMComponentResourceLink GetResourceLink(ZString staffCode)
		{
			var query = GetComponentLinkQuery(staffCode, PK);

			return Factory.LoadTop1<BMComponentResourceLink>(query);
		}

		public static ZQuery GetComponentLinkQuery(ZString staffCode, ZGuid componentPk)
		{
			var query = new ZQuery(BMComponentResourceLinkSchema.FD_FC_Component, componentPk);
			query.AddToFilter(BMComponentResourceLinkSchema.FD_GS_NKResource, staffCode);
			return query;
		}

		#endregion

		#region Properties

		#region TypeDescription

		[ResourceStringData("BMComponent.TypeDescription", ShortCaption = "Type Desc.", Caption = "Type Description")]
		public ZString TypeDescription
		{
			get { return Lookups.Types.GetDescriptionFromCode(this.FC_Type); }
		}

		#endregion

		#region Buffer Timespan

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsBufferPropertyReadOnly))]
		[ResourceStringData("BMComponent.BufferTimespan", Caption = "Buffer Timespan")]
		public ZDateTime BufferTimespan
		{
			get { return FC_BufferTimespanInMinutes.GetDateTimeFromMinutes(); }
			set
			{
				FC_BufferTimespanInMinutes = (ZInt)value.GetMinutesFromDateTimeSpan();
				BufferTimespanInfo.RefreshBinding();
				FC_BufferTimespanInMinutesInfo.RefreshBinding();
			}
		}

		public ZWrappedPropertyInfo BufferTimespanInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(BufferTimespan), o => FC_BufferTimespanInMinutesInfo); }
		}

		public double BufferTimeSpanHours
		{
			get { return FC_BufferTimespanInMinutes / 60.0; }
		}

		#endregion

		#region Offset Minutes

		[ResourceStringData("BMComponent.ComponentOffset", Caption = "Component Offset")]
		public ZDateTime ComponentOffset
		{
			get { return FC_OffsetInMinutes.GetDateTimeFromMinutes(); }
			set
			{
				FC_OffsetInMinutes = (ZInt)value.GetMinutesFromDateTimeSpan();
				ComponentOffsetInfo.RefreshBinding();
				FC_OffsetInMinutesInfo.RefreshBinding();
			}
		}

		public ZWrappedPropertyInfo ComponentOffsetInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ComponentOffset), o => FC_OffsetInMinutesInfo); }
		}

		#endregion

		#region FC_FC_ParentComponent

		[RelatedBusinessObject("ParentComponent")]
		[List("Lookups.ParentComponents")]
		public override ZGuid FC_FC_ParentComponent
		{
			get { return base.FC_FC_ParentComponent; }
			set
			{
				base.FC_FC_ParentComponent = value;
				var parent = ParentComponent;
				if (parent != null)
				{
					FC_FS_System = parent.FC_FS_System;
				}
			}
		}

		public virtual BMComponent ParentComponent
		{
			get { return Factory.Load<BMComponent>(FC_FC_ParentComponent); }
		}

		#endregion

		#region FC_FS_System

		[RelatedBusinessObject("System")]
		[List("Lookups.Systems")]
		public override ZGuid FC_FS_System
		{
			get { return base.FC_FS_System; }
			set { base.FC_FS_System = value; }
		}

		public virtual BMSystem System
		{
			get { return Factory.Load<BMSystem>(FC_FS_System); }
		}

		#endregion

		#region FC_Type

		[List("Lookups.Types")]
		public override ZString FC_Type
		{
			get { return base.FC_Type; }
			set
			{
				base.FC_Type = value;

				if (FC_Type == BMComponentTypeList.Codes.Buffer)
				{
					if (FC_BufferLoadLimitPercent == 0)
					{
						FC_BufferLoadLimitPercent = BMConstants.DefaultBufferLoadLimitPercent;
					}
					if (FC_BufferTimeCapacityConstraintThresholdMultiple == DefaultNonBufferMultiplierValue)
					{
						FC_BufferTimeCapacityConstraintThresholdMultiple = BMConstants.DefaultBufferTimeCapacityConstraintThresholdMultiple;
					}
					if (FC_NonCCRTemporaryOverloadLimitMultiplier == DefaultNonBufferMultiplierValue)
					{
						FC_NonCCRTemporaryOverloadLimitMultiplier = BMConstants.DefaultNonCCRTemporaryOverloadLimitMultiplier;
					}
				}
				else
				{
					FC_BufferLoadLimitPercent = 0;
					FC_BufferTimeCapacityConstraintThresholdMultiple = DefaultNonBufferMultiplierValue;
					FC_NonCCRTemporaryOverloadLimitMultiplier = DefaultNonBufferMultiplierValue;
					FC_BufferTimespanInMinutes = 0;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateFC_BufferLoadLimitPercent();
					Validation.ValidateFC_BufferTimespanInMinutes();
					Validation.ValidateFC_BufferTimeCapacityConstraintThresholdMultiple();
					Validation.ValidateFC_NonCCRTemporaryOverloadLimitMultiplier();
				}
			}
		}

		const int DefaultNonBufferMultiplierValue = 1;

		#endregion

		#region FC_AutoAssignTasksAge

		[ZDateTimeDurationValue]
		[ReadOnlyMember(nameof(IsBufferPropertyReadOnly))]
		public override ZDateTime FC_AutoAssignTasksAge
		{
			get { return base.FC_AutoAssignTasksAge; }
			set { base.FC_AutoAssignTasksAge = value.ConvertToDurationBasedDate(FC_AutoAssignTasksAgeInfo); }
		}

		#endregion

		#region FC_BufferLoadLimitPercent

		[ReadOnlyMember(nameof(IsBufferPropertyReadOnly))]
		public override ZByte FC_BufferLoadLimitPercent
		{
			get { return base.FC_BufferLoadLimitPercent; }
			set { base.FC_BufferLoadLimitPercent = value; }
		}

		#endregion

		#region FC_BufferTimeCapacityConstraintThresholdMultiple

		[ReadOnlyMember(nameof(IsBufferPropertyReadOnly))]
		public override ZDecimal FC_BufferTimeCapacityConstraintThresholdMultiple
		{
			get { return base.FC_BufferTimeCapacityConstraintThresholdMultiple; }
			set { base.FC_BufferTimeCapacityConstraintThresholdMultiple = value; }
		}

		#endregion

		#region FC_NonCCRTemporaryOverloadLimitMultiplier

		[ReadOnlyMember(nameof(IsBufferPropertyReadOnly))]
		public override ZDecimal FC_NonCCRTemporaryOverloadLimitMultiplier
		{
			get { return base.FC_NonCCRTemporaryOverloadLimitMultiplier; }
			set { base.FC_NonCCRTemporaryOverloadLimitMultiplier = value; }
		}

		#endregion

		#endregion

		#region New Properties

		public bool IsBuffer
		{
			get { return FC_Type == BMComponentTypeList.Codes.Buffer; }
		}

		public bool IsBucket
		{
			get { return FC_Type == BMComponentTypeList.Codes.Bucket; }
		}

		public bool IsChildComponent
		{
			get { return !FC_FC_ParentComponent.IsEmpty; }
		}

		public bool IsChildBuffer
		{
			get { return IsBuffer && IsChildComponent; }
		}

		public bool IsConstraint
		{
			get { return FC_Type == BMComponentTypeList.Codes.Constraint; }
		}

		protected bool IsBufferPropertyReadOnly
		{
			get { return !IsBuffer; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int CCRThresholdMinutes
		{
			get { return (int)(FC_BufferTimespanInMinutes * FC_BufferTimeCapacityConstraintThresholdMultiple); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int PersistentlyOverloadedMinutes
		{
			get
			{
				var overloadFactor = (double)BMSRegistry.Instance.PersistentlyOverloadedBufferFactor.Value;
				return (int)(overloadFactor * FC_BufferTimespanInMinutes);
			}
		}

		public ConstraintStatus? CCRStatus
		{
			get
			{
				return ccrStatus = ccrStatus ?? ConstrainedModeHelper.GetConstraintStatus(this);
			}
		}

		ConstraintStatus? ccrStatus;

		#endregion

		#region IBMComponent

		IBusinessObjectCollection IBMComponent.FromMeToOthersLinks
		{
			get { return FromMeToOthersLinks; }
		}

		IBusinessObjectCollection IBMComponent.FromOthersToMeLinks
		{
			get { return FromOthersToMeLinks; }
		}

		IBMComponentReleaseGroupLinkCollection IBMComponent.ReleaseGroupLinks
		{
			get { return ReleaseGroupLinks; }
		}

		IBMComponent IBMComponent.ParentComponent
		{
			get { return ParentComponent; }
		}

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			FC_BufferTimeCapacityConstraintThresholdMultiple = DefaultNonBufferMultiplierValue;
			FC_NonCCRTemporaryOverloadLimitMultiplier = DefaultNonBufferMultiplierValue;
			FC_GB_AgingBranch = Env.CurrentBranchPK;
			FC_GE_AgingDepartment = Env.CurrentDepartmentPK;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (BMComponent)base.CloneInternal(args);

			var groupArgs = new BusinessObjectCloneArgs(columnNamesToExcludeFromCopy: new[] { BMComponentReleaseGroupLinkSchema.FO_FC_Component.Name });
			foreach (var groupLink in ReleaseGroupLinks.Select(rgl => (BMComponentReleaseGroupLink)rgl.Clone(groupArgs)))
			{
				groupLink.FO_FC_Component = clone.PK;
			}

			var multiplerArgs = new BusinessObjectCloneArgs(columnNamesToExcludeFromCopy: new[] { BMZoneCapacityMultiplierSchema.BZC_FC_Component.Name });
			foreach (var multiplier in ZoneCapacityMultipliers.Select(zcm => (BMZoneCapacityMultiplier)zcm.Clone(multiplerArgs)))
			{
				multiplier.BZC_FC_Component = clone.PK;
			}

			var componentArgs = new BusinessObjectCloneArgs(columnNamesToExcludeFromCopy: new[]
			{
				BMComponentSchema.FC_FC_ParentComponent.Name,
				BMComponentSchema.FC_FS_System.Name
			});
			foreach (var child in ChildComponents.Select(child => (BMComponent)child.Clone(componentArgs)))
			{
				child.FC_FC_ParentComponent = clone.PK;
			}

			return clone;
		}

		#endregion

		#region Object Overrides

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}: {1} - {2}", System != null ? System.FS_Name : ZString.Empty, FC_DisplaySequence, FC_Name);
		}

		#endregion

		#region IBuffer Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IBuffer.SizeInMinutes
		{
			get { return FC_BufferTimespanInMinutes; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IBuffer.OffsetInMinutes
		{
			get { return FC_OffsetInMinutes; }
		}

		string IBuffer.Name
		{
			get { return FC_Name; }
		}

		BufferType IBuffer.Type
		{
			get { return BufferType.Operational; }
		}

		IReadOnlyCollection<IBufferedItem> IBuffer.RelatedBufferedItems
		{
			get { return new List<IBufferedItem>(); } // We don't need to calculate overall penetration on an operational buffer
		}

		#endregion

		#region IBranchDepartmentProvider Members

		IGlbBranch IBranchDepartmentProvider.GetBranch(BusinessObjectFactory factory)
		{
			var branch = ParentComponent?.AgingBranch ?? AgingBranch;

			if (branch == null)
			{
				if (!IsBuffer)
				{
					branch = GlbBranch.GetCurrentBranch(factory ?? Factory);

					if (branch == null)
					{
						ErrorReporter.ReportOnce("GlbBranch.GetCurrentBranch returned null branch",
$@"CurrentBranch:{GlbBranch.CurrentBranch}
CurrentBranchPK:{GlbBranch.CurrentBranch?.PK}
Env.CurrentBranch:{Env.CurrentBranch}
Env.CurrentBranchPK:{Env.CurrentBranchPK}
factory:{factory.NameForDebugging},{factory}
Factory:{Factory.NameForDebugging},{Factory}
GlbBranch.CurrentBranch.Factory:{GlbBranch.CurrentBranch?.Factory}
" + (GlbBranch.CurrentBranch == null ? "" : $"GlbBranch In factory:{(factory ?? Factory).Load<GlbBranch>(GlbBranch.CurrentBranch.PK)}"));
					}
				}
				else
				{
					ErrorReporter.ReportOnce("The buffer has no aging branch which should not be allowed by validation");
				}
			}

			return branch;
		}

		IGlbDepartment IBranchDepartmentProvider.GetDepartment(BusinessObjectFactory factory)
		{
			var department = ParentComponent?.AgingDepartment ?? AgingDepartment;

			if (department == null)
			{
				if (!IsBuffer)
				{
					department = GlbDepartment.GetCurrentDepartment(factory ?? Factory);

					if (department == null)
					{
						ErrorReporter.ReportOnce("GlbBranch.GetCurrentDepartment returned null department",
$@"CurrentDepartment:{GlbDepartment.CurrentDepartment}
CurrentDepartment.PK:{GlbDepartment.CurrentDepartment?.PK}
Env.CurrentDepartment:{Env.CurrentDepartment}
Env.CurrentDepartmentPK:{Env.CurrentDepartmentPK}
factory:{factory.NameForDebugging}
Factory:{Factory.NameForDebugging}
GlbDepartment.CurrentDepartment.Factory:{GlbDepartment.CurrentDepartment?.Factory}
" + (GlbDepartment.CurrentDepartment == null ? "" : $"GlbDepartment In factory:{(factory ?? Factory).Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK)}"));
					}
				}
				else
				{
					ErrorReporter.ReportOnce("The buffer has no aging department which should not be allowed by validation");
				}
			}

			return department;
		}

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(BMComponentLinkSchema.FL_FC_ComponentFrom, null);
				yield return new AuditChildInfo(BMComponentLinkSchema.FL_FC_ComponentTo, null);
				yield return new AuditChildInfo(BMComponentResourceLinkSchema.FD_FC_Component, null);
				yield return new AuditChildInfo(BMZoneCapacityMultiplierSchema.BZC_FC_Component, null);
			}
		}

		#endregion

		#region Methods

		public WorkingTimeContext GetRelevantContext(GlbBranch branchOverride = null, GlbDepartment departmentOverride = null)
		{
			var componentForContext = IsChildBuffer ? ParentComponent : this;
			return WorkingTimeContext.CreateWithOverride(componentForContext ?? this, branchOverride, departmentOverride);
		}

		public bool IsOverlapped(double ageInMinutes)
		{
			return ageInMinutes >= FC_OffsetInMinutes
				&& ageInMinutes < FC_OffsetInMinutes + FC_BufferTimespanInMinutes;
		}

		public bool IsOverlapped(double ageInMinutes, BMComponentSectionConfiguration configuration)
		{
			var minutesPerCell = configuration.TimePerCell.GetMinutesFromDateTimeSpan();
			var offset = GetTimeAsCellUnit(minutesPerCell, FC_OffsetInMinutes);
			var timespan = GetTimeAsCellUnit(minutesPerCell, FC_BufferTimespanInMinutes);

			return ageInMinutes >= offset
				&& ageInMinutes < offset + timespan;
		}

		public int GetTimeAsCellUnit(double minutesPerCell, int time)
		{
			return time != 0
				? time - ((int)(time % minutesPerCell))
				: time;
		}

		#endregion

		#region TypeDecider

		class BMComponentTypeDecider : TypeDecider
		{
			public override Type GetTypeForBinding()
			{
				return typeof(BMComponent);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				return (string)row[BMComponentSchema.FC_Type.Name] == BMComponentTypeList.Codes.ComponentRelationship ? typeof(ComponentRelationship) : typeof(BMComponent);
			}

			public override Type GetTypeForNew()
			{
				return typeof(BMComponent);
			}
		}

		public static readonly TypeDecider TypeDecider = new BMComponentTypeDecider();

		#endregion

		#region Test Data

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			AddSystemForTest();
		}

		protected virtual void AddSystemForTest()
		{
			FC_FS_System = Factory.NewWithValidTestData<BMSystem>().PK;
		}

#endif

		#endregion
	}
}
