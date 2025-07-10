using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Business
{
	public class GridColourStripBusinessObject : FilterStripBusinessObject
	{
		public GridColourStripBusinessObject()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public GridColourStripBusinessObject(FilterStripBusinessObject parentFilterStrip, GridColourScheme colourScheme, Type businessEntityType, bool editable = false)
			: base(parentFilterStrip.Factory)
		{
			QueryObjectType = businessEntityType;
			this.colourScheme = colourScheme;

			ParentFilterStrip = !editable || parentFilterStrip is GridFilterStripBusinessObject
				? parentFilterStrip
				: CreateBusinessObject(parentFilterStrip);

			ParentFilterStrip.QueryObjectType = QueryObjectType;

			if (!(parentFilterStrip is GridFilterStripBusinessObject))
			{
				ParentFilterStrip.ActiveStatusFilterColumn = parentFilterStrip.ActiveStatusFilterColumn;
			}

			((IFilterStripBusinessObjectInternals)this).LayoutContext = ((IFilterStripBusinessObjectInternals)parentFilterStrip).LayoutContext + GridColourFactory.ColorStripCode;

			LoadFilterModuleStrategy(businessEntityType);

			if (editable)
			{
				foreach (var moduleFilter in GetModuleFilters())
				{
					moduleFilter.ModuleFilterChanged -= ModuleFilterChanged;
					moduleFilter.ModuleFilterChanged += ModuleFilterChanged;
				}
			}
		}

		FilterStripBusinessObject CreateBusinessObject(FilterStripBusinessObject parentFilterStrip)
			=> parentFilterStrip.Clone();

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var gridColourStripBusinessObject = (GridColourStripBusinessObject)base.CloneInternal(args);
			gridColourStripBusinessObject.QueryObjectType = this.QueryObjectType;
			gridColourStripBusinessObject.colourScheme = this.colourScheme;
			gridColourStripBusinessObject.ParentFilterStrip = this.ParentFilterStrip;
			gridColourStripBusinessObject.LayoutContext = this.LayoutContext;
			return gridColourStripBusinessObject;
		}

		internal GridColourScheme colourScheme;

		IEnumerable<FilterModuleStrategy> FilterModuleStrategies
		{
			get { return ((IEnumerable)ObjectFactory.Get("FilterModuleStrategies")).Cast<FilterModuleStrategy>(); }
		}

		internal FilterStripBusinessObject ParentFilterStrip;

		public Color BGColor { get; set; }

		public StmModuleFilter StmModuleFilter
		{
			get
			{
				if (stmModuleFilter == null)
				{
					stmModuleFilter = FindLayout(RuleName, false, colourScheme?.S9_GC);
				}
				return stmModuleFilter;
			}
			set { stmModuleFilter = value; }
		}

		StmModuleFilter stmModuleFilter;

		#region Filters

		public override StmModuleFilter FindLayout(ZString layoutName, ZBool isPublished)
		{
			return FindLayout(layoutName, isPublished, null);
		}

		public override StmModuleFilter FindLayout(ZString layoutName, ZBool isPublished, ZGuid? gcPk)
		{
			ZGuid pk;
			StmModuleFilter filter = null;

			if (ZGuid.TryParse(layoutName, out pk))
			{
				filter = Factory.Load<StmModuleFilter>(pk);
			}

			if (filter == null)
			{
				var query = new ZQuery(StmModuleFilterSchema.S9_FilterName, layoutName);
				query.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, colourScheme != null ? colourScheme.PK : null);
				query.AddToFilter(StmModuleFilterSchema.S9_FilterType, SQLComparisonOperator.NotEqual, StmModuleFilterTypes.Codes.FilterRule);

				if (colourScheme == null)
				{
					query.AddToFilter(StmModuleFilterSchema.S9_ModuleID, ((IFilterStripBusinessObjectInternals)this).LayoutContext);
				}
				else
				{
					query.AddToFilter(StmModuleFilterSchema.S9_ModuleID, SQLComparisonOperator.EndsWith, GridColourFactory.ColorStripCode);
				}

				if (gcPk != null)
				{
					query.AddToFilter(StmModuleFilterSchema.S9_GC, gcPk == ZGuid.Empty ? DBNull.Value : gcPk);
				}

				filter = Factory.LoadTop1<StmModuleFilter>(query);
			}

			if (filter != null)
			{
				StmModuleFilter = filter;
				RuleName = filter.S9_IsSystem ? filter.S9_FilterNameMultilingual : filter.S9_FilterName;

				if (colourScheme != null && filter.S9_RelatedEntityID != colourScheme.PK)
				{
					filter.S9_RelatedEntityID = colourScheme.PK;
				}
			}

			return filter;
		}

		void ModuleFilterChanged(object sender, EventArgs e)
		{
			OnLayoutChanged();
		}

		public override ZQuery Filter
		{
			get { return Filters.GetFilterQuery(ActiveModuleFiltersForQuery, ApplyToFilterGroups); }
		}

		public virtual ZQuery GetFilterForGridItems(IEnumerable<BusinessObject> businessObjects)
		{
			var activeModuleFilters = ActiveModuleFiltersForQuery;

			var nonPersistentFilters = new List<ModuleFilter>();
			var otherFilters = new List<ModuleFilter>();
			foreach (var filter in activeModuleFilters)
			{
				if (filter.FilterColumn != null && filter.FilterColumn.IsNonPersistent)
				{
					nonPersistentFilters.Add(filter);
				}
				else
				{
					otherFilters.Add(filter);
				}
			}

			if (nonPersistentFilters.Any())
			{
				nonPersistentFilterForGridItems = Filters.GetFilterQuery(nonPersistentFilters, ApplyToFilterGroups, businessObjects);
				nonPersistentFilterForGridItems.AddOptionRecompileConditionally = false;
			}
			else
			{
				nonPersistentFilterForGridItems = null;
			}

			return Filters.GetFilterQuery(otherFilters, ApplyToFilterGroups, businessObjects);
		}

		public ZQuery GetNonPersistentFilterForGridItems()
		{
			return nonPersistentFilterForGridItems;
		}
		ZQuery nonPersistentFilterForGridItems;

		public override void ResetModuleFilters()
		{
			base.ResetModuleFilters();
			LoadFilterModuleStrategy(QueryObjectType);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return Filters;
		}

		protected override bool ShouldUseHelperFilter => false;

		ModuleFilterCollection Filters
		{
			get { return fFilters ?? (fFilters = GetFilters()); }
		}

		protected override void OnModuleFiltersReset()
		{
			base.OnModuleFiltersReset();
			fFilters = null;
		}

		ModuleFilterCollection fFilters;

		ModuleFilterCollection GetFilters()
		{
			var moduleFilters = ParentFilterStrip.GetModuleFilters();

			if (ParentFilterStrip.ActiveStatusFilterColumn != null)
			{
				ParentFilterStrip.AddActiveStatusFilter(moduleFilters);
			}

			moduleFilters.CategorySortOrder = ParentFilterStrip.CategorySortOrder != null
				? ParentFilterStrip.CategorySortOrder as FilterCategory[] ?? [.. ParentFilterStrip.CategorySortOrder] : null;

			var filterThatOverridesAllOtherFilters = ParentFilterStrip.GetModuleFilterThatOverridesAllOtherFilters();
			if (filterThatOverridesAllOtherFilters != null)
			{
				var collection = moduleFilters.GetOrCreateFilterCollectionByCategory(filterThatOverridesAllOtherFilters.Category);

				if (filterThatOverridesAllOtherFilters.IsCommon && !collection.ContainsKey(ModuleFilter.CommonFilterDescription(filterThatOverridesAllOtherFilters.Category)))
				{
					moduleFilters.AddFilter(filterThatOverridesAllOtherFilters.NewCommonModuleFilter(filterThatOverridesAllOtherFilters.Category, moduleFilters), collection);
				}
			}

			foreach (var filter in moduleFilters)
			{
				if (filter.Visibility == FilterVisibility.AlwaysVisible)
				{
					filter.Visibility = FilterVisibility.Visible;
				}
			}

			moduleFilters.ElementIsActiveChanged += ModuleFilters_ElementIsActiveChanged;

			return moduleFilters;
		}

		void LoadFilterModuleStrategy(Type businessEntityType)
		{
			if (businessEntityType != null)
			{
				foreach (var strategy in FilterModuleStrategies)
				{
					strategy.RunOnModuleFiltersCreated(ModuleFilters, businessEntityType, Factory);
				}
			}
		}

		void ModuleFilters_ElementIsActiveChanged(object sender, ModuleFilter.IsActiveChangedEventArgs e)
		{
			if (e.ModuleFilter.IsActive)
			{
				RegisterEditableChildObject(e.ModuleFilter);
			}
			else
			{
				UnRegisterEditableChildObject(e.ModuleFilter);
			}
		}

		#endregion

		#region Rule Name

		public ZString RuleName
		{
			get { return fRuleName; }
			set
			{
				if (fRuleName != value)
				{
					CheckMaximumLength(RuleNameInfo, value); // you are likely missing this line!
					fRuleName = value;

					if (!IsValidationSuspended)
					{
						ValidateRuleName();
					}
					RuleNameInfo.RefreshBinding();
				}
			}
		}

		public int RuleName_MaxLength
		{
			get { return StmModuleFilterSchema.S9_FilterName.MaxLength; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Pre-defined rule name")]
		void ValidateRuleName()
		{
			RuleNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RuleNameInfo);

			var query = new ZQuery(StmModuleFilterSchema.S9_FilterName, RuleName);
			query.AddToFilter(StmModuleFilterSchema.S9_ModuleID, ((IFilterStripBusinessObjectInternals)this).LayoutContext);
			query.AddToFilter(StmModuleFilterSchema.S9_FilterType, SQLComparisonOperator.NotEqual, StmModuleFilterTypes.Codes.FilterRule);

			var ownerQuery = new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, EnvProxy.Instance.CurrentUser.PK);
			ownerQuery.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_IsSystem, true);
			ownerQuery.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_IsPublished, true);

			query.AddToFilter(ownerQuery);

			var duplicateFilter = Factory.LoadTop1<StmModuleFilter>(query);

			if (duplicateFilter != null || RuleName.ToLower() == "rule")
			{
				RuleNameInfo.AddError(Res.GetString("GridColourStripBusinessObjectValidation|RuleNameAlreadyExists", "This name has already been used for another strip, please select a different name."));
			}
		}

		ZString fRuleName;

		public ZPropertyInfo RuleNameInfo
		{
			get { return GetZPropertyInfo(nameof(RuleName)); }
		}

		#endregion

		#region LayoutHelper

		protected override FilterStripLayoutsHelper GetNewLayoutsHelper()
		{
			return new GridColourStripLayoutsHelper(this);
		}

		internal
		class GridColourStripLayoutsHelper : FilterStripLayoutsHelper
		{
			public GridColourStripLayoutsHelper(GridColourStripBusinessObject parent)
			{
				this.parent = parent;
			}

			readonly GridColourStripBusinessObject parent;

			protected override ZGuid GetCurrentUserPk()
			{
				return parent != null && parent.colourScheme != null ? parent.colourScheme.PK : ZGuid.Empty;
			}
		}

		#endregion

		public override bool ShouldAddSystemDefinedStatusFilter => ParentFilterStrip?.ShouldAddSystemDefinedStatusFilter ?? base.ShouldAddSystemDefinedStatusFilter;
	}
}
