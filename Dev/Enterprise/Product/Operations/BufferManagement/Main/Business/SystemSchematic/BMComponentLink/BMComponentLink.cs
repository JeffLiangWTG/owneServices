using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[DebuggerDisplay("{" + nameof(DisplayText) + "}")]
	[DescriptionProperty(nameof(DisplayText))]
	public class BMComponentLink : AutoBMComponentLink,
		IBMComponentLink,
		IComponentLink,
		IFilterPreviewable,
		IRelatedModuleFilterSupportable,
		IAuditParent
	{
		public BMComponentLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new BMComponentLinkFetchStrategy(this);
		}

		class BMComponentLinkFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			internal BMComponentLinkFetchStrategy(BMComponentLink parent)
				: base(parent)
			{
				this.parent = parent;
			}

			readonly BMComponentLink parent;

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();

				Factory.AddFetchHint(StmModuleFilterSchema.Instance, parent.FilterRuleProvider.GetLoadQuery());
			}
		}

		#endregion

		#region Related Business Objects

		#region Filter

		FilterRuleProvider FilterRuleProvider => filterRuleProvider ?? (filterRuleProvider = new BMFilterRuleProvider(this));
		FilterRuleProvider filterRuleProvider;

		public StmModuleFilter FilterRule => FilterRuleProvider.GetOrCreateAndCacheFilter();

		public bool HasNoFilterWhenRequired => FL_TransferRulesEnabled && !FL_IsReleaseGateRuleApplied && !HasFilters && ComponentFrom != System.GetFirstBMComponent();

		bool HasFilters => RelatedModuleFiltersHelper.HasFilterStrips(FilterRule, ModuleIDs.BMFilterRule);

		#endregion

		BMSystem System => Factory.Load<BMSystem>(ComponentToSystemPK);

		#endregion

		#region Business Object Overrides

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (!HasChanges)
			{
				var filter = FilterRuleProvider.IsFilterCached ? FilterRuleProvider.GetOrCreateAndCacheFilter() : null;

				if (filter != null && filter.HasChanges)
				{
					HasChanges = true;
				}
			}
		}

		public override void Delete()
		{
			FilterRuleProvider.DeleteFilter();

			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			FL_IsReleaseGateRuleApplied = false;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (BMComponentLink)base.CloneInternal(args);

			FilterRuleProvider.CopyFilterStrips(() => clone.FilterRule);

			return clone;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("92fc3a2e-2d20-4dc1-8c9a-a68910d7cfb9", "Buffer Management System: {0}, Component Link: {1}", System?.FS_Name, DisplayText);

		#endregion

		#region Properties

		#region FL_FC_ComponentTo

		[List("Lookups.ComponentTos")]
		[RelatedBusinessObject("ComponentTo")]
		public override ZGuid FL_FC_ComponentTo
		{
			get { return base.FL_FC_ComponentTo; }
			set
			{
				base.FL_FC_ComponentTo = value;

				OnSetComponentTo();

				if (!IsValidationSuspended)
				{
					Validation.ValidateComponentToSystemPK();
				}

				TransferHintInfo.RefreshBinding();
			}
		}

		protected virtual void OnSetComponentTo()
		{
			var destinationComponent = ComponentTo;
			if (destinationComponent != null && destinationComponent.IsBuffer)
			{
				FL_IsReleaseGateRuleApplied = true;
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateFL_IsReleaseGateRuleApplied();
			}
		}

		public virtual BMComponent ComponentTo
		{
			get { return Factory.Load<BMComponent>(FL_FC_ComponentTo); }
		}

		ZGuid ComponentSystemPK { get; set; }

		[List("Lookups.Systems")]
		public ZGuid ComponentToSystemPK
		{
			get { return (ComponentTo != null) ? ComponentTo.FC_FS_System : ComponentSystemPK; }
			set
			{
				if (ComponentTo != null && ComponentTo.FC_FS_System != value)
				{
					FL_FC_ComponentTo = ZGuid.Invalid;
				}
				ComponentSystemPK = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateComponentToSystemPK();
				}
				ComponentToSystemPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ComponentToSystemPKInfo
		{
			get { return GetZPropertyInfo(nameof(ComponentToSystemPK)); }
		}

		#endregion

		#region FL_FC_ComponentFrom

		[List("Lookups.ComponentFroms")]
		[RelatedBusinessObject("ComponentFrom")]
		public override ZGuid FL_FC_ComponentFrom
		{
			get { return base.FL_FC_ComponentFrom; }
			set
			{
				MaybeErrorReportOnComponentFromChange(value);
				base.FL_FC_ComponentFrom = value;
			}
		}

		void MaybeErrorReportOnComponentFromChange(ZGuid newComponentFromPK)
		{
			if (IsInDatabase && newComponentFromPK != (ZGuid)FL_FC_ComponentFromInfo.OriginalValue)
			{
				ErrorReporter.ReportOnce("Unexpected change of ComponentFrom on a BMComponentLink");
			}
		}

		public virtual BMComponent ComponentFrom
		{
			get { return Factory.Load<BMComponent>(FL_FC_ComponentFrom); }
		}

		#endregion

		#region FL_Name

		[ResourceStringData("006a6b6c-0692-47f2-a923-b09148bc06c8", Caption = "Name", FullDescription = "Optional: A friendly name that helps to uniquely identify this link in grids and service task logs.")]
		public override ZString FL_Name
		{
			get => base.FL_Name;
			set => base.FL_Name = value;
		}

		#endregion

		#endregion

		#region New Properties

		#region DisplayText

		public ZString DisplayText
		{
			get
			{
				Factory.AddFetchHint(typeof(BMComponent), FL_FC_ComponentFrom);
				Factory.AddFetchHint(typeof(BMComponent), FL_FC_ComponentTo);
				return FL_Name.IsEmpty ? new ZString(FormattableString.Invariant($"{ComponentFrom?.FC_Name} -> {ComponentTo?.FC_Name}, Sequence: {FL_Sequence}")) : FL_Name;
			}
		}

		#endregion

		#region Transfer Hint

		public ZString TransferHint
		{
			get
			{
				if (ComponentFrom != null && ComponentTo != null)
				{
					return GetKnownToComponentTransferHintString(ComponentFrom.FC_Name, ComponentTo.FC_Name);
				}

				return UnknownToComponentTransferHint;
			}
		}

		static MultilingualString GetKnownToComponentTransferHintString(ZString fromComponentName, ZString toComponentName)
		{
			return ResString.GetMultilingualString("e38d26c1-5668-41dd-b6c0-b31469c446be", @"Workflows in the Component [{0}] will move to the Component [{1}] when they match the below filters:", fromComponentName, toComponentName);
		}

		public static MultilingualString UnknownToComponentTransferHint => ResString.GetMultilingualString("3ad2da40-c82e-474b-ad2a-6a465cc53103", @"Please create a Component Link to begin configuring Component Link Filter Rules.");

		public ZPropertyInfo TransferHintInfo
		{
			get { return GetZPropertyInfo(nameof(TransferHint)); }
		}

		#endregion

		#endregion

		#region IFilterPreviewable Members

		public ZQuery GetAdditionalPreviewFilter(string moduleId, string dropDownCode)
		{
			var query = new ZQuery();
			query.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, this.FL_FC_ComponentFrom);

			return query;
		}

		#endregion

		#region IComponentLink Members

		Guid IComponentLink.PK => PK.ToGuid();

		string IComponentLink.DisplayText => DisplayText;

		Guid IComponentLink.ComponentFrom => FL_FC_ComponentFrom.ToGuid();

		Guid IComponentLink.ComponentTo => FL_FC_ComponentTo.ToGuid();

		string IComponentLink.GetComponentFromName() => ComponentFrom.FC_Name;

		string IComponentLink.GetComponentToName() => ComponentTo.FC_Name;

		bool IComponentLink.IsReleaseGateRuleApplied => FL_IsReleaseGateRuleApplied;

		bool IComponentLink.TransferRulesEnabled => FL_TransferRulesEnabled;

		bool IComponentLink.SkipTransfer
		{
			get
			{
				if (!(RelatedModuleFiltersHelper.GetNewFilterBusinessObject(FilterRule) is IBMFilterRuleFilterBusinessObject filterRuleFilterBusinessObject)
					|| !filterRuleFilterBusinessObject.LoadFilterRuleLayout(FilterRule))
				{
					return false;
				}

				var activeFilterIdentifiers = filterRuleFilterBusinessObject.ActiveFilterIdentifiersIncludingUserDefined.ToHashSet();

				if (activeFilterIdentifiers.Count == 0)
				{
					return true;
				}

				return activeFilterIdentifiers.All(f => filtersAllowedToSkip.Contains(f));
			}
		}

		string IComponentLink.SystemName => System.FS_Name;

		public Guid Branch
		{
			get
			{
				var componentTo = ComponentTo;
				if (componentTo.FC_GB_AgingBranch.IsValid)
				{
					return componentTo.FC_GB_AgingBranch.ToGuid();
				}

				var componentFrom = ComponentFrom;
				if (componentFrom.FC_GB_AgingBranch.IsValid)
				{
					return componentFrom.FC_GB_AgingBranch.ToGuid();
				}

				return EnvProxy.Instance.CurrentBranchPK;
			}
		}

		public Guid Department
		{
			get
			{
				var componentTo = ComponentTo;
				if (componentTo.FC_GE_AgingDepartment.IsValid)
				{
					return componentTo.FC_GE_AgingDepartment.ToGuid();
				}

				var componentFrom = ComponentFrom;
				if (componentFrom.FC_GE_AgingDepartment.IsValid)
				{
					return componentFrom.FC_GE_AgingDepartment.ToGuid();
				}

				return EnvProxy.Instance.CurrentDepartmentPK;
			}
		}

		static readonly ImmutableHashSet<string> filtersAllowedToSkip = new BMComponentLinkHelper().GetFiltersAllowedToSkip();

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
