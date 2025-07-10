using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class FilterStripBusinessObject : FilterBusinessObject,
		IEnumerable<ModuleFilter>,
		IFilterStripBusinessObjectInternals,
		IModifyModuleAndGridLayout,
		IActiveModuleFiltersProvider,
		IFilterStripBusinessObject
	{
		protected FilterStripBusinessObject(string nameForDebugging)
			: this(new BusinessObjectFactory { NameForDebugging = "FilterStripBusinessObject (Constructor) " + nameForDebugging }) // Debug information
		{
		}

		protected FilterStripBusinessObject()
			: this(new BusinessObjectFactory { NameForDebugging = "FilterStripBusinessObject (Constructor)" }) // Debug information
		{
		}

		protected FilterStripBusinessObject(BusinessObjectFactory factory)
			: base(factory, new DataTable().NewRow())
		{
			ShouldSetDefaults = true;
			OnGlowIndexQueryErrorAction = ReportGlowIndexQueryError;
		}

		bool ContainsPublicParamlessConstructor(Type type)
		{
			return type.GetConstructors().Any(constructor => constructor.GetParameters().All(parameter => parameter.IsOptional));
		}

		public virtual bool IsGlowIndexSearchAllowed
		{
			get
			{
				return isGlowIndexSearchAllowed && ObjectFactory.Get<IGlowRegistry>().IsGlowIndexSearchAllowedForModule(ParentModule?.ID.Name);
			}
			set { isGlowIndexSearchAllowed = value; }
		}
		protected bool isGlowIndexSearchAllowed = true;

		public override SearchType SearchType
		{
			get
			{
				return base.SearchType;
			}
			set
			{
				if (!IsGlowIndexSearchAllowed)
				{
					value = SearchType.Sql;
				}

				var typeChanged = value != base.SearchType;
				if (typeChanged)
				{
					base.SearchType = value;

					ResetModuleFilters();
					FilterStrips = new FilterStripCollection(ModuleFilters);
					SetExternalDefaults();

					SearchTypeChanged?.Invoke(value);
				}
			}
		}
		public Action<SearchType> SearchTypeChanged;

		protected override bool SupportsCloneCore()
			=> true;

		public new FilterStripBusinessObject Clone()
			=> (FilterStripBusinessObject)base.Clone();

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var type = GetType();
			if (!ContainsPublicParamlessConstructor(type))
			{
				throw new ArgumentException(FormattableString.Invariant($"{type.FullName} must have a public parameterless constructor"));
			}
			var newFilterBizo = (FilterStripBusinessObject)Activator.CreateInstance(type);
			if (args.AlternativeFactoryToInstantiateCloneIn != null)
			{
				//HACK: we can't pass the factory into the constructor (since a factory taking constructor probably doesn't exist),
				//so use reflection to change readonly private field
				typeof(BusinessObject).GetField("factory", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(newFilterBizo, args.AlternativeFactoryToInstantiateCloneIn);
			}
			else
			{
				newFilterBizo.Factory.RefreshEnabled = this.Factory.RefreshEnabled;
			}
			newFilterBizo.QueryObjectType = QueryObjectType;
			newFilterBizo.fModuleFilters = fModuleFilters?.ShallowClone();
			return newFilterBizo;
		}

		public virtual bool NullOutParentModuleOnDispose { get; set; } = true;

		public ModuleIdentifier ParentModuleID { get; set; } = ModuleIDs.NotAssigned;

		public Type ParentType { get; set; }

		#region Not used functionality

		protected sealed override void SetPKAndDefaults()
		{
			// row is empty
		}

		public sealed override bool IsSavedByFactory
		{
			get { return false; }
		}

		protected sealed override void SetDefaultValues()
		{
			base.SetDefaultValues();
		}

		#endregion

		#region Custom SQL Filter

		public bool HasCustomSqlFilter
		{
			get { return ModuleFilters != null && ModuleFilters[CustomSqlFilterDescription] != null; }
		}

		public Type QueryObjectType { get; set; }

		void AddCustomSqlFilter(ModuleFilterCollection filters)
		{
			if (filters[CustomSqlFilterDescription] == null && SafeToAddCustomSqlFilter)
			{
				var sqlFilter = filters.AddSqlFilter(CustomSqlFilterDescription, QueryObjectType);
				sqlFilter.Category = FilterCategories.Other;
				sqlFilter.MultilingualDescription = ResString.GetMultilingualString("14964774-3FF0-4B7B-9ADC-32FA84A23824", "Custom SQL Filter");
			}
		}

		void AddUnionOrOrFilter(ModuleFilterCollection filters)
		{
			if (filters[UnionOrOrDescription] == null && SafeToAddCustomSqlFilter && ObjectFactory.Get<ISystemDataRegistry>().UnionOrOrFilter.Value)
			{
				var sqlFilter = filters.AddUnionOrOrFilter(UnionOrOrDescription, "UNION?", null, null); // Programmatic constant
				sqlFilter.Category = FilterCategories.Other;
				sqlFilter.MultilingualDescription = ResString.GetMultilingualString("553c20f9-4718-46c9-851a-46785435e971", "UNION or OR");
			}
		}

		void AddRecompileFilter(ModuleFilterCollection filters)
		{
			if (filters[RecompileDescription] == null && SafeToAddCustomSqlFilter && ObjectFactory.Get<ISystemDataRegistry>().RecompileFilter.Value)
			{
				var sqlFilter = filters.AddRecompileFilter(RecompileDescription, "Enable Recompile", null, null);
				sqlFilter.Category = FilterCategories.Execution;
				sqlFilter.MultilingualDescription = ResString.GetMultilingualString("4725A91F-2CC3-4F5E-8404-BEDC67F63456", RecompileDescription);
			}
		}

		void AddCardinalityFilter(ModuleFilterCollection filters)
		{
			if (filters[CardinalityDescription] == null && SafeToAddCustomSqlFilter && ObjectFactory.Get<ISystemDataRegistry>().CardinalityFilter.Value)
			{
				var sqlFilter = filters.AddCardinalityFilter(CardinalityDescription, "Enable Legacy Cardinality Estimation", null, null);
				sqlFilter.Category = FilterCategories.Execution;
				sqlFilter.MultilingualDescription = ResString.GetMultilingualString("ADD0B23B-39B8-4F86-9F69-091C3B1A41D0", CardinalityDescription);
			}
		}

		protected virtual bool SafeToAddCustomSqlFilter
		{
			get
			{
				return QueryObjectType != null && BusinessObjectFactory.GetTableNameFromType(QueryObjectType, false) != null
					&& (!QueryObjectType.IsSubclassOf(typeof(NonPersistentBusinessObject)) || QueryObjectType.GetInterfaces().Contains(typeof(IWrapPersistentBizO)));
			}
		}

		public const string CustomSqlFilterDescription = "Custom SQL Filter"; // Programmatic constant

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public const string UnionOrOrDescription = "UNION or OR";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public const string RecompileDescription = "Recompile";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public const string CardinalityDescription = "Legacy Cardinality Estimation";

		public static readonly MultilingualString CustomFieldCategoryDescription = ResString.GetMultilingualString("FilterCategory.CustomFields", "Workflow Custom Fields");

		#endregion

		#region Date Range

		public void AddDateRange(ZQuery query, DateComparisonOperator comparisonOperator, JoinCondition joinCondition, SchemaDateTimeColumn column, ZDate lowerEmptyOk, ZDate upperEmptyOk)
		{
			query.AddToFilter(new DateQueryBuilder().CreateDateRange(comparisonOperator, column, lowerEmptyOk, upperEmptyOk), joinCondition);
		}

		public void AddDateTimeRange(ZQuery query, DateComparisonOperator comparisonOperator, JoinCondition joinCondition, SchemaDateTimeColumn column, ZDateTime lowerEmptyOk, ZDateTime upperEmptyOk, bool convertFromLocalToUtc = false)
		{
			AddDateTimeRange(query, comparisonOperator, joinCondition, column, lowerEmptyOk, upperEmptyOk, false, false, convertFromLocalToUtc);
		}

		protected void AddDateTimeRange(ZQuery query, DateComparisonOperator comparisonOperator, JoinCondition joinCondition, SchemaDateTimeColumn column, ZDateTime lowerEmptyOk, ZDateTime upperEmptyOk, bool lowerDatePartOnly, bool upperDatePartOnly, bool convertFromLocalToUtc = false)
		{
			query.AddToFilter(new DateQueryBuilder(convertFromLocalToUtc).CreateDateTimeRange(comparisonOperator, column, lowerEmptyOk, upperEmptyOk, lowerDatePartOnly, upperDatePartOnly), joinCondition);
		}

		protected void AddDateTimeOffsetRange(ZQuery query, DateComparisonOperator comparisonOperator, JoinCondition joinCondition, SchemaDateTimeOffsetColumn column, ZDateTimeOffset lowerEmptyOk, ZDateTimeOffset upperEmptyOk, bool lowerDatePartOnly, bool upperDatePartOnly)
		{
			query.AddToFilter(new DateQueryBuilder().CreateDateTimeOffsetRange(comparisonOperator, column, lowerEmptyOk, upperEmptyOk, lowerDatePartOnly, upperDatePartOnly), joinCondition);
		}

		#endregion

		#region Category Sort Order

		/// <summary>
		/// Return a IReadOnlyList<FilterCategory> containing FilterCategory elements in the order you wish them
		/// to appear in the filter list. Elements that are omitted will be added at the end of the
		/// list in alphabetical order.
		/// </summary>
		protected virtual IReadOnlyList<FilterCategory> CategorySortOrderCore
		{
			get
			{
				return new FilterCategory[]
				{
					FilterCategories.NumbersAndReferences,
					FilterCategories.StatusAndFlags,
					FilterCategories.Dates,
					FilterCategories.Locations,
					FilterCategories.Organisations,
					FilterCategories.FinancialDetails,
					FilterCategories.ModesAndTypes,
					FilterCategories.TextSearch,
					FilterCategories.AuditInformation,
					FilterCategories.Other,
					FilterCategories.Execution
				};
			}
		}

		public IReadOnlyList<FilterCategory> CategorySortOrder => CategorySortOrderCore;

		#endregion

		public IEnumerable<T> FindFiltersOfTypeInGroup<T>(ModuleFilter sourceFilter) where T : ModuleFilter
		{
			var groupNameFilters = ActiveModuleFilters.Where(f => f.GroupName == sourceFilter.GroupName && f.GroupOrCategory == sourceFilter.GroupOrCategory).ToArray();
			var matches = new List<T>();

			var matchesNotInOrCategories = groupNameFilters.OfType<T>().Where(f => f.OrCategory == FilterOrCategory.None);
			matches.AddRange(matchesNotInOrCategories);

			foreach (var orCategory in groupNameFilters.Where(x => x.OrCategory != FilterOrCategory.None).GroupBy(x => x.OrCategory))
			{
				if (orCategory.Count() == 1 && orCategory.Single() is T matchingFilter)
				{
					matches.Add(matchingFilter);
				}
			}

			return matches;
		}

		#region Filter Strips

		[EditorBrowsable(EditorBrowsableState.Never)]
		[BusinessObjectTestExclude]
		public FilterStripCollection FilterStrips
		{
			get { return filterStrips ?? (FilterStrips = new FilterStripCollection(ModuleFilters)); } // do not change this to set the filterStrips field
			private set
			{
				if (filterStrips != null)
				{
					UnRegisterEditableChildObject(filterStrips);
					filterStrips.RemoveAndDeleteAll();
				}

				filterStrips = value;
				RegisterEditableChildObject(filterStrips);
			}
		}

		FilterStripCollection filterStrips;

		#endregion

		#region Developer-defined Module Filters

		public ModuleFilterCollection GetModuleFilters()
		{
			var result = GetModuleFiltersCore();
			if (ShouldUseHelperFilter)
			{
				AddFiltersFromHelpers(result);
			}

			result.DisableFilterValidation = IsValidationSuspended;

			foreach (var filter in result)
			{
				if (IsValidationSuspended)
				{
					filter.SuspendValidation();
				}
			}

			return result;
		}

		#region For sub-classes to implement

		protected abstract ModuleFilterCollection GetModuleFiltersCore();

		void AddFiltersFromHelpers(ModuleFilterCollection filters)
		{
			foreach (var helper in GetCombinedCustomFilterStripsHelpers())
			{
				if (helper.IsApplicableToBizOTypeIsAssignableFrom())
				{
					helper.AddFilterStrips(filters);
				}
			}
		}

		void AddIndexFiltersFromHelpers(ModuleFilterCollection filters)
		{
			foreach (var helper in GetCombinedCustomFilterStripsHelpers())
			{
				if (helper.IsApplicableToBizOTypeIsAssignableFrom())
				{
					helper.AddFilterStripsForIndexSearch(filters, IndexSearchFields.DefaultHiddenIndexSearchFields);
				}
			}
		}

		protected virtual bool ShouldUseHelperFilter => true;

		public bool GetShouldUseHelperFilter => ShouldUseHelperFilter;

		public List<IFilterStripsHelper> CustomFilterStripsHelpers => customFilterStripsHelpers ?? (customFilterStripsHelpers = GetCustomFilterStripsHelpersCore());
		List<IFilterStripsHelper> customFilterStripsHelpers;

		List<IFilterStripsHelper> GetCombinedCustomFilterStripsHelpers()
		{
			var combinedHelpers = CustomFilterStripsHelpers;

			if (QueryObjectType != null)
			{
				var listedHelpers = ObjectFactory.Get<ListObject>("FilterStripsHelpers").Cast<IFilterStripsHelper>();
				var helpersToUse = listedHelpers.Where(listedHelper => listedHelper != null && !combinedHelpers.Any(additionalHelper => additionalHelper != null && listedHelper.GetType().IsInstanceOfType(additionalHelper))).ToArray();
				combinedHelpers = combinedHelpers.Concat(helpersToUse).Where(x => !FilterStripsHelperTypesToExcludeFromAutomaticAddingOfFilters.Any(excludedType => excludedType.IsAssignableFrom(x.GetType()))).ToList();
				combinedHelpers.ForEach(x => x.Initialise(QueryObjectType, Factory));
			}

			return combinedHelpers;
		}

		protected virtual List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			return new List<IFilterStripsHelper>();
		}

		protected virtual IEnumerable<Type> FilterStripsHelperTypesToExcludeFromAutomaticAddingOfFilters
		{
			get { return Array.Empty<Type>(); }
		}

		public bool IsFilterStripsHelperExcluded(Type helperType)
		{
			return FilterStripsHelperTypesToExcludeFromAutomaticAddingOfFilters.Any(x => x.IsAssignableFrom(helperType));
		}

		protected ModuleFilter ModuleFilterThatOverridesAllOtherFilters
		{
			get
			{
				if (moduleFilterThatOverridesAllOtherFilters == null)
				{
					moduleFilterThatOverridesAllOtherFilters = GetModuleFilterThatOverridesAllOtherFiltersCore();
					if (moduleFilterThatOverridesAllOtherFilters != null)
					{
						moduleFilterThatOverridesAllOtherFilters.IsExclusive = true;
					}
				}
				return moduleFilterThatOverridesAllOtherFilters;
			}
		}

		protected virtual ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return null;
		}

		ModuleFilter moduleFilterThatOverridesAllOtherFilters;

		public ModuleFilter GetModuleFilterThatOverridesAllOtherFilters() => GetModuleFilterThatOverridesAllOtherFiltersCore();

		protected internal virtual void AddDuplicateDefaultFilterStripCollection(FilterStripCollection collection)
		{
		}

		public void AddAuditFiltersIfRequired()
		{
			AddAuditFiltersIfRequiredCore();
		}

		protected virtual void AddAuditFiltersIfRequiredCore()
		{
		}

		protected virtual bool ShouldAddCustomSqlFilter => true;

		public bool ShouldAddUserDefinedFilters
		{
			get
			{
				if (ShouldAddUserDefinedFiltersCore && !Globals.IsWeb)
				{
					if (ParentModule != null)
					{
						return ParentModule is IZFilterModule;
					}

					return ModuleType != null && (typeof(IZFilterModule)).IsAssignableFrom(ModuleType);
				}
				return false;
			}
		}

		protected virtual bool ShouldAddUserDefinedFiltersCore => true;

		public bool GetShouldAddUserDefinedFilters => ShouldAddUserDefinedFiltersCore;

		public Type ModuleType { get; set; }

		#endregion

		public IModuleFilterCollection ModuleFilterCollection
		{
			get { return ModuleFilters; }
		}

		public virtual ModuleFilterCollection ModuleFilters
		{
			get
			{
				if (!AreModuleFiltersLoaded)
				{
					LoadModuleFilters();
					AfterLoadModuleFilters?.Invoke();
				}

				return fModuleFilters;
			}
		}

		public event Action AfterLoadModuleFilters;

		public bool AreModuleFiltersLoaded => fModuleFilters != null;

		public string DefaultFilterLoadMessage;

		#region IndexSearch Indexing Search Filter

		public SearchFieldCollection IndexSearchFields { get; set; }

		public bool HasIndexSearchFields
		{
			get
			{
				if (IndexSearchFields != null)
				{
					return IndexSearchFields.Status == GlowIndexQueryStatus.Success && IndexSearchFields.Value.Length > 0;
				}
				return false;
			}
		}

		public IEnumerable<IGlowQuery> GetIndexSearchQueries()
		{
			var queries = GetActiveFiltersQueries();
			if (AdditionalIndexSearchQuery != null)
			{
				queries = queries.Append(AdditionalIndexSearchQuery);
			}
			return queries;
		}

		public IEnumerable<IGlowQuery> GetActiveFiltersQueries()
		{
			return ActiveModuleFiltersForQuery
				.GroupBy(f => f.GroupOrCategory)
				.SelectMany(g => CreateGroupOrCategoryQuery(g.Key, g.ToImmutableList()));
		}

#if DEBUG
		public IEnumerable<IGlowQuery> GetActiveFiltersQueriesWithoutAlwaysAppliedAndHiddenFilters()
		{
			return ActiveModuleFiltersForQuery
				.Where(filter => filter.Visibility != FilterVisibility.AlwaysAppliedAndHidden)
				.GroupBy(f => f.GroupOrCategory)
				.SelectMany(g => CreateGroupOrCategoryQuery(g.Key, g.ToImmutableList()));
		}
#endif

		IEnumerable<IGlowQuery> CreateGroupOrCategoryQuery(FilterOrCategory groupOrCategory, ImmutableList<ModuleFilter> filtersWithSameGroupOrCategory)
		{
			if (groupOrCategory == FilterOrCategory.None)
			{
				return filtersWithSameGroupOrCategory.GroupBy(f => f.GroupName).Select(g => CreateGroupQuery(g.ToImmutableList()));
			}
			else
			{
				var groupQueries = filtersWithSameGroupOrCategory.GroupBy(f => f.GroupName).Select(g => CreateGroupQuery(g.ToImmutableList()));
				var combinedGroupQuery = new BooleanQuery(BooleanOperator.Or, groupQueries.ToArray());
				return Enumerable.Repeat(combinedGroupQuery, 1);
			}
		}

		IGlowQuery CreateGroupQuery(ImmutableList<ModuleFilter> filtersWithSameGroup)
		{
			var queries = filtersWithSameGroup.GroupBy(f => f.OrCategory).SelectMany(g => CreateQuery(g.Key, g.ToImmutableList()));
			return new BooleanQuery(BooleanOperator.And, queries.ToArray());
		}

		IEnumerable<IGlowQuery> CreateQuery(FilterOrCategory orCategory, ImmutableList<ModuleFilter> filtersWithSameOrCategory)
		{
			if (orCategory == FilterOrCategory.None)
			{
				return filtersWithSameOrCategory.OfType<IIndexSearchModuleFilter>().Select(f => f.GetGlowIndexQuery());
			}
			else
			{
				var queries = filtersWithSameOrCategory.OfType<IIndexSearchModuleFilter>().Select(f => f.GetGlowIndexQuery());
				var combinedQuery = new BooleanQuery(BooleanOperator.Or, queries.ToArray());
				return Enumerable.Repeat(combinedQuery, 1);
			}
		}

		void LoadModuleFiltersFromGlow()
		{
			fModuleFilters = GetModuleFiltersFromGlow();
		}

		public ModuleFilterCollection GetModuleFiltersFromGlow() => GetModuleFiltersFromGlowCore();

		protected virtual ModuleFilterCollection GetModuleFiltersFromGlowCore()
		{
			var moduleFilters = new ModuleFilterCollection();
			if (HasIndexSearchFields)
			{
				foreach (var searchField in IndexSearchFields.Value)
				{
					var searchFieldOverride = ResolveSearchField(searchField);
					if (searchFieldOverride == null)
					{
						continue;
					}

					var indexSearchFilters = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(searchFieldOverride, Factory);
					indexSearchFilters.ForEach(moduleFilters.AddFilter);
				}

				if (ShouldUseHelperFilter)
				{
					AddIndexFiltersFromHelpers(moduleFilters);
				}

				IndexSearchFilterHelper.AddCommonFilter(moduleFilters);
			}
			if (ShouldAddUserDefinedFilters)
			{
				AddUserDefinedFilters(moduleFilters);
			}

			return moduleFilters;
		}

		protected virtual SearchField ResolveSearchField(SearchField searchField)
		{
			if (searchField.UIHidden || searchField.FieldName.StartsWith(IndexSearchFilterHelper.DefaultHiddenPrefix))
			{
				return null;
			}

			if (searchField.IsActiveStatusField || searchField.IsCancelledStatusField)
			{
				var filter = new IndexSearchModuleTextFilter(FilterDescriptions.ActiveStatus, searchField, GetActiveStatusGlowQuery, CancelledStatusList, FilterCategories.StatusAndFlags);
				PrepareActiveStatusFilter(filter);
				return new SearchFieldOverride(searchField, filter);
			}
			return searchField;
		}

		#endregion

		public void LoadModuleFilters()
		{
			if (SearchType == SearchType.Index)
			{
				LoadModuleFiltersFromGlow();
			}
			else
			{
				LoadSqlModuleFilters(out var loadMessage);
				DefaultFilterLoadMessage = loadMessage;
			}
			fModuleFilters.ElementIsActiveChanged += fModuleFilters_ElementIsActiveChanged;

			foreach (var filter in fModuleFilters)
			{
				filter.FilterBusinessObject = this;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		void LoadSqlModuleFilters(out string loadMessage)
		{
			loadMessage = string.Empty;
			if (ParentModule?.LimitedColumns != null)
			{
				fModuleFilters = new ModuleFilterCollection();

				if (ParentModule.LimitedColumns.CodeSchemaColumn != null)
				{
					fModuleFilters.AddTextFilter("Code", (SchemaStringColumn)ParentModule.LimitedColumns.CodeSchemaColumn).Visibility = FilterVisibility.AlwaysVisible;
				}

				if (ParentModule.LimitedColumns.DescriptionSchemaColumn != null)
				{
					fModuleFilters.AddTextFilter("Description", (SchemaStringColumn)ParentModule.LimitedColumns.DescriptionSchemaColumn).Visibility = FilterVisibility.AlwaysVisible;
				}

				var alwaysAppliedFilters = LoadModuleFiltersCore().Where(f => f.Visibility == FilterVisibility.AlwaysApplied || f.Visibility == FilterVisibility.AlwaysAppliedAndHidden);
				foreach (var alwaysAppliedFilter in alwaysAppliedFilters)
				{
					if (!fModuleFilters.Any(filter => filter.FilterColumn == alwaysAppliedFilter.FilterColumn))
					{
						alwaysAppliedFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
						fModuleFilters.AddFilter(alwaysAppliedFilter);
					}
				}
				loadMessage = string.Format(
					"ParentModule: '{0}', CodeSchemaColumn Exists: '{1}', DescriptionSchemaColumn Exists: '{2}', alwaysAppliedFilters Count: '{3}', fModuleFilters Count: '{4}'",
					ParentModule, ParentModule.LimitedColumns.CodeSchemaColumn != null, ParentModule.LimitedColumns.DescriptionSchemaColumn != null, alwaysAppliedFilters.Count(), fModuleFilters.Count());
			}
			else
			{
				fModuleFilters = LoadModuleFiltersCore();

				fModuleFilters.CategorySortOrder = CategorySortOrder != null ? CategorySortOrder as FilterCategory[] ?? [.. CategorySortOrder] : null;

				OnModuleFiltersCreated();
				ModuleFiltersCreated?.Invoke(this);

				foreach (var hook in ModuleFiltersCreatedHooks)
				{
					hook?.Invoke(this);
				}
			}
		}

		ModuleFilterCollection LoadModuleFiltersCore()
		{
			var moduleFilters = GetModuleFilters();

			if (ShouldAddCustomSqlFilter)
			{
				AddCustomSqlFilter(moduleFilters);
				AddUnionOrOrFilter(moduleFilters);
				AddRecompileFilter(moduleFilters);
				AddCardinalityFilter(moduleFilters);
			}

			if (ActiveStatusFilterColumn != null && ShouldAddActiveStatusFilter)
			{
				AddActiveStatusFilter(moduleFilters);
			}

			if (ShouldAddSystemDefinedStatusFilter && SystemDefinedStatusFilterColumn != null)
			{
				AddSystemDefinedStatusFilter(moduleFilters);
			}

			if (ShouldAddUserDefinedFilters)
			{
				AddUserDefinedFilters(moduleFilters);
			}

			if (ModuleFilterThatOverridesAllOtherFilters != null)
			{
				moduleFilters.AddFilter(ModuleFilterThatOverridesAllOtherFilters);
			}

			AddInitialAuditFilters(moduleFilters);

			return moduleFilters;
		}

		protected virtual void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			if (QueryObjectType != null && !QueryObjectType.IsInterface)
			{
				var tableName = BusinessObjectFactory.GetTableNameFromType(QueryObjectType, false);

				if (!string.IsNullOrEmpty(tableName))
				{
					ModuleAuditFilterProvider.AddAuditFilters(filters, EnterpriseSchema.GetTableSchema(tableName), Factory, QueryObjectType);
				}
			}
		}

		public new ModuleFilter this[string description]
		{
			get { return ModuleFilters[description]; }
		}

		public event ModuleFiltersCreatedHandler ModuleFiltersCreated;

		IList<Action<FilterStripBusinessObject>> moduleFiltersCreatedHooks;
		IList<Action<FilterStripBusinessObject>> ModuleFiltersCreatedHooks => moduleFiltersCreatedHooks ?? (moduleFiltersCreatedHooks = new List<Action<FilterStripBusinessObject>>());
		public void AddModuleFiltersCreatedHook(Action<FilterStripBusinessObject> action) => ModuleFiltersCreatedHooks.Add(action);

		public ReadOnlyCollection<ModuleFilter> AlwaysVisibleModuleFilters
		{
			get
			{
				return GetAlwaysVisibleModuleFiltersCore();
			}
		}

		protected virtual ReadOnlyCollection<ModuleFilter> GetAlwaysVisibleModuleFiltersCore()
		{
			var result = new List<ModuleFilter>();
			foreach (var filter in this)
			{
				if (filter.Visibility == FilterVisibility.AlwaysVisible)
				{
					result.Add(filter);
				}
			}
			return result.AsReadOnly();
		}

		public ReadOnlyCollection<ModuleFilter> ActiveModuleFilters
		{
			get
			{
				var result = new List<ModuleFilter>();
				foreach (var filter in this)
				{
					if (filter.IsActive)
					{
						result.Add(filter);
					}
				}
				return result.AsReadOnly();
			}
		}

		IEnumerable<ModuleFilter> IActiveModuleFiltersProvider.ActiveModuleFilters
		{
			get { return ActiveModuleFilters; }
		}

#if DEBUG
		public
#endif
		ReadOnlyCollection<ModuleFilter> AlwaysAppliedModuleFilters
		{
			get
			{
				var result = new List<ModuleFilter>();
				foreach (var filter in ModuleFilters)
				{
					if (filter.Visibility == FilterVisibility.AlwaysApplied || filter.Visibility == FilterVisibility.AlwaysAppliedAndHidden)
					{
						result.Add(filter);
					}
				}
				return result.AsReadOnly();
			}
		}

		IEnumerable<ModuleFilter> IActiveModuleFiltersProvider.AlwaysAppliedModuleFilters
		{
			get { return AlwaysAppliedModuleFilters; }
		}

		string IFilterStripBusinessObject.LayoutContext
		{
			get { return LayoutContext; }
			set { LayoutContext = value; }
		}

		IEnumerable<NonPersistentBusinessObject> IFilterStripBusinessObject.ActiveModuleFiltersForValidation => ActiveModuleFilters;

		public virtual void ResetModuleFilters()
		{
			if (fModuleFilters != null)
			{
				var filters = fModuleFilters.ToArray();
				foreach (var filter in filters)
				{
					UnRegisterEditableChildObject(filter);
				}
				fModuleFilters = null;
				OnModuleFiltersReset();
			}
			moduleFilterThatOverridesAllOtherFilters = null;
		}

		void fModuleFilters_ElementIsActiveChanged(object sender, ModuleFilter.IsActiveChangedEventArgs e)
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

		protected virtual void OnModuleFiltersCreated()
		{
		}

		protected virtual void OnModuleFiltersReset()
		{
		}

		ModuleFilterCollection fModuleFilters;

		public ZQuery GetFilterWhere(ZGuid filterPk, Func<IModuleFilter, bool> filtersToIncludeFunc, BusinessObjectFactory factoryOverride = null)
		{
			var factoryToUse = factoryOverride ?? Factory;
			return GetFilterWhere(factoryToUse.Load<StmModuleFilter>(filterPk), filtersToIncludeFunc);
		}

		public ZQuery GetFilterWhere(StmModuleFilter filter, Func<IModuleFilter, bool> filtersToIncludeFunc)
		{
			var filterRuleSupportable = this as IRelatedModuleFilterBusinessObject;
			var filterLoaded = filterRuleSupportable?.LoadFilterRuleLayout(filter) ?? LoadLayout(filter);

			if (filterLoaded)
			{
				var currentFilters = ActiveModuleFilters;
				var allFilters = GetModuleFilters();

				var filtersToInclude = currentFilters.Where(s => filtersToIncludeFunc(s));

				var filteredFilter = allFilters.GetFilterQuery(filtersToInclude, ApplyToFilterGroups);

				return filteredFilter;
			}

			return null;
		}

		#endregion

		#region Active Status Filter

		public void AddActiveStatusFilters(Type collectionElementType)
		{
			if (typeof(ICancellable).IsAssignableFrom(collectionElementType))
			{
				var reverseBools = false;
				var activeSchemaColumn = GetColumnForActiveStatusFilter(collectionElementType, ref reverseBools);
				if (activeSchemaColumn != null && !DoesNotNeedActiveStatusFilter(activeSchemaColumn))
				{
					SetActiveStatusFilter(activeSchemaColumn, reverseBools);
				}
			}
		}

		SchemaColumn GetColumnForActiveStatusFilter(Type cancellableType, ref bool reverseBools)
		{
			SchemaColumn result = null;
			var tableName = BusinessObjectFactory.GetTableNameFromType(cancellableType, false);
			if (tableName != null)
			{
				var columnPrefix = GetColumnPrefixFromType(cancellableType);
				var tableSchema = EnterpriseSchema.GetTableSchema(tableName);
				var isCancelledColumn = tableSchema.GetSchemaColumn(columnPrefix + "_IsCancelled");
				if (isCancelledColumn != null)
				{
					result = isCancelledColumn;
				}
				else
				{
					var isActiveColumn = tableSchema.GetSchemaColumn(columnPrefix + CargoWise.Schema.Schema.IsActiveColumnSuffix);
					if (isActiveColumn != null)
					{
						reverseBools = true;
						result = isActiveColumn;
					}
				}
			}

			return result;
		}

		protected virtual string GetColumnPrefixFromType(Type cancellableType)
		{
			return BusinessObjectFactory.GetTableCodeFromType(cancellableType);
		}

		// for some reason BO can contain IsCancelled(IsActive) column, but it can belong to business logic - not to Activating
		// then we do not need to add "Active Status" filter
		// F.e. RatingHeader.TH_IsCancelled - is such exceptional case
		bool DoesNotNeedActiveStatusFilter(SchemaColumn column)
		{
			var result = false;

			if (column.Name.Equals(Enterprise.ZArchitecture.Schema.RatingHeaderSchema.TH_IsCancelled.Name))
			{
				result = true;
			}

			return result;
		}

		#endregion

		#region Active Status

		public void SetActiveStatusFilter(SchemaColumn isCancelledColumn, bool reverseBools)
		{
			ActiveStatusFilterColumn = isCancelledColumn;
			ReverseBoolsForActiveStatusFilter = reverseBools;
		}

		public SchemaColumn ActiveStatusFilterColumn;
		protected bool ReverseBoolsForActiveStatusFilter;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
		internal const string LastEdited = "Last Edited";

		public static MultilingualString StatusActive => ResString.GetMultilingualString("5956c1ce-7e47-4c01-939d-9824a55648c2", "Active");
		public static MultilingualString StatusInactive => ResString.GetMultilingualString("85309bfd-8468-411e-bd08-409c43784224", "Inactive");
		public static MultilingualString StatusAll => ResString.GetMultilingualString("48558f63-e01d-4a00-82b7-36c6be8c1796", "All");

		protected virtual bool IsActiveStatusFilterAlwaysApplied()
		{
			return true;
		}
		public virtual bool ShouldAddActiveStatusFilter => true;

		public void AddActiveStatusFilter(ModuleFilterCollection filters)
		{
			if (filters[FilterDescriptions.ActiveStatus] == null)
			{
				var activeStatusFilter = filters.AddTextFilter(FilterDescriptions.ActiveStatus, GetActiveStatusQuery, CancelledStatusList);
				PrepareActiveStatusFilter(activeStatusFilter);
			}
		}

		void PrepareActiveStatusFilter(ModuleTextFilter activeStatusFilter)
		{
			activeStatusFilter.Property = CancelledStatusList[StatusActive].Code;
			activeStatusFilter.Category = FilterCategories.StatusAndFlags;
			if (IsActiveStatusFilterAlwaysApplied())
			{
				activeStatusFilter.Visibility = FilterVisibility.AlwaysApplied;
				activeStatusFilter.DefaultProperty = CancelledStatusList[StatusActive].Code;
			}
			activeStatusFilter.MultilingualDescription = ResString.GetMultilingualString("FilterStrip|Common|ActiveStatus", "Active Status");
		}

		public CodeDescriptionPairList CancelledStatusList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				result.AddPair(StatusActive, Res.GetString("FilterStrip|Common|CancelledStatusList|Active", "Show Active Only"));
				result.AddPair(StatusInactive, Res.GetString("FilterStrip|Common|CancelledStatusList|InActive", "Show Inactive Only"));
				result.AddPair(StatusAll, Res.GetString("FilterStrip|Common|CancelledStatusList|All", "Show all records"));

				return result;
			}
		}

		ZQuery GetActiveStatusQuery(ZString status) => GetActiveStatusQueryCore(status);

		protected virtual ZQuery GetActiveStatusQueryCore(ZString status)
		{
			var query = new ZQuery();

			status = status.Trim();
			if (StatusInactive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(ActiveStatusFilterColumn, !ReverseBoolsForActiveStatusFilter);
				query.IgnoreActiveFilter = true;
			}
			else if (StatusActive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(ActiveStatusFilterColumn, ReverseBoolsForActiveStatusFilter);
				query.IgnoreActiveFilter = false;
			}
			else if (StatusAll.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(ActiveStatusFilterColumn, ReverseBoolsForActiveStatusFilter);
				query.AddToFilter(JoinCondition.Or, ActiveStatusFilterColumn, !ReverseBoolsForActiveStatusFilter);
				query.IgnoreActiveFilter = true;
			}

			return query;
		}

		IGlowQuery GetActiveStatusGlowQuery(SearchField searchField, ZString status)
		{
			status = status.Trim();
			var reverse = searchField.IsActiveStatusField;
			if (StatusAll.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				return new EmptyQuery();
			}
			if (StatusActive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				return IndexSearchModuleFlagsFilter.GetGlowIndexQueryCore(searchField, reverse);
			}
			return IndexSearchModuleFlagsFilter.GetGlowIndexQueryCore(searchField, !reverse);
		}

		#endregion

		#region System Defined Status Filters

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
		const string SystemDefinedStatus = "Is System Defined";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter code")]
		protected const string DefinedStatusAllCode = "All";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter code")]
		protected const string DefinedStatusSystemCode = "System";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter code")]
		protected const string DefinedStatusNotSystemCode = "Not System";
		protected virtual string DefinedStatusAllDescription => Res.GetString("86B03B5D-B1F8-4A46-9787-08BDBA432C4D", "All");
		protected virtual string DefinedStatusSystemDescription => Res.GetString("BC787B02-974E-4E68-8F37-5E473BCC42EC", "System");
		protected virtual string DefinedStatusNotSystemDescription => Res.GetString("030378B6-8597-4F28-8531-C87003F3CD11", "Not System");
		public List<string> SystemKindList => systemKindList ??= new List<string> { CargoWise.Schema.Schema.IsSystemColumnSuffix, CargoWise.Schema.Schema.IsSystemDefinedColumnSuffix };
		List<string> systemKindList;

		public SchemaColumn SystemDefinedStatusFilterColumn
		{
			get
			{
				if (systemDefinedStatusFilterColumn == null && QueryObjectType != null)
				{
					var tableName = BusinessObjectFactory.GetTableNameFromType(QueryObjectType, false);
					if (tableName != null)
					{
						var tableSchema = EnterpriseSchema.GetTableSchema(tableName);
						if (tableSchema != null)
						{
							var columnPrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName);
							var availableColumns = new List<SchemaColumn>();
							SystemKindList.ForEach(kind => availableColumns.Add(tableSchema.GetSchemaColumn(columnPrefix + kind)));
							systemDefinedStatusFilterColumn = availableColumns.FirstOrDefault(column => column != null);
						}
					}
				}
				return systemDefinedStatusFilterColumn;
			}
		}

		SchemaColumn systemDefinedStatusFilterColumn;

		void AddSystemDefinedStatusFilter(ModuleFilterCollection filters)
		{
			var systemDefinedStatusFilter = filters.AddTextFilter(SystemDefinedStatus, GetSystemDefinedStatusQuery, GetSystemDefinedStatusList());
			systemDefinedStatusFilter.DefaultProperty = IsSystemDefinedDefaultProperty;
			systemDefinedStatusFilter.Category = FilterCategories.StatusAndFlags;
			systemDefinedStatusFilter.Visibility = IsSystemDefinedStatusFilterVisibility;
			systemDefinedStatusFilter.MultilingualDescription = ResString.GetMultilingualString("FilterStrip|Common|IsSystemDefined", "Is System Defined");
		}

		CodeDescriptionPairList GetSystemDefinedStatusList()
		{
			var systemDefinedStatusList = new CodeDescriptionPairList();
			systemDefinedStatusList.AddPair(DefinedStatusAllCode, DefinedStatusAllDescription);
			systemDefinedStatusList.AddPair(DefinedStatusSystemCode, DefinedStatusSystemDescription);
			systemDefinedStatusList.AddPair(DefinedStatusNotSystemCode, DefinedStatusNotSystemDescription);
			return systemDefinedStatusList;
		}

		protected virtual FilterVisibility IsSystemDefinedStatusFilterVisibility => FilterVisibility.Visible;
		protected virtual string IsSystemDefinedDefaultProperty => DefinedStatusSystemCode;
		public virtual bool ShouldAddSystemDefinedStatusFilter => true;

		ZQuery GetSystemDefinedStatusQuery(ZString value)
		{
			var query = new ZQuery();

			if (value == DefinedStatusSystemCode)
			{
				query.AddToFilter(SystemDefinedStatusFilterColumn, ZBool.True);
			}
			else if (value == DefinedStatusNotSystemCode)
			{
				query.AddToFilter(SystemDefinedStatusFilterColumn, ZBool.False);
			}

			return query;
		}

		#endregion

		#region User-Defined Filters

		public void RefreshUserDefinedFilters()
		{
			if (QueryObjectType != null)
			{
				AddUserDefinedFilters(ModuleFilters);
				RemoveDeletedUserDefinedFilters();
			}
		}

		void AddUserDefinedFilters(ModuleFilterCollection moduleFilters)
		{
			var layouts = GetLayoutsForUserDefinedFilters();

			if (layouts.Any())
			{
				var pkColumn = GetPkColumnForQueryObjectType();

				if (pkColumn != null)
				{
					foreach (var layout in layouts)
					{
						ModuleUserDefinedFilter newFilter = null;

						if (!moduleFilters.OfType<ModuleUserDefinedFilter>().Any(x => x.Description == ModuleUserDefinedFilter.GetPrefixedDescription(layout.S9_FilterName)))
						{
							newFilter = TryToCreateNewModuleUserDefinedFilter(layout, pkColumn);
						}
						else if (moduleFilters.OfType<ModuleUserDefinedFilter>().Any(x => x.Description == ModuleUserDefinedFilter.GetPrefixedDescription(layout.S9_FilterName) && x.IsLayoutDeleted))
						{
							newFilter = TryToCreateNewModuleUserDefinedFilter(layout, pkColumn);

							if (newFilter != null)
							{
								ModuleFilters.DisableAndRemoveFilterFromCollection(newFilter);
							}
						}

						if (newFilter != null)
						{
							moduleFilters.AddFilter(newFilter);
						}
					}
				}
			}
		}

		ModuleUserDefinedFilter TryToCreateNewModuleUserDefinedFilter(StmModuleFilter layout, SchemaGuidColumn pkColumn)
		{
			var moduleId = ModuleIDs.AllIncludingClientModules.FirstOrDefault(x => string.Equals(x.Name, layout.S9_ModuleID, StringComparison.OrdinalIgnoreCase))
				?? ParentModule?.ID;

			if (moduleId == null)
			{
				return null;
			}

			return new ModuleUserDefinedFilter(layout, moduleId, pkColumn)
			{
				MultilingualDescription = layout.S9_FilterNameMultilingual,
				FilterBusinessObject = this,
			};
		}

		public void AddOrRemoveFavoriteFilter(StmModuleFilter filter)
			=> FavoritesHandler.AddOrRemoveFavoriteFilter(ObjectFactory.Get<ILinkWrapper>("ILinkWrapper", (string)LayoutContext, filter.PK.ToGuid(), null, (string)filter.S9_FilterName));

		protected virtual StmModuleFilter[] GetLayoutsForUserDefinedFilters()
		{
			var relatedModuleFilterBusinessObject = this as IRelatedModuleFilterBusinessObject;

			return UserDefinedFilterHelperBusiness.GetLayoutsForUserDefinedFilters(Factory, LayoutsHelper, LayoutContext, includePublishedOnly: relatedModuleFilterBusinessObject?.IsInFilterRuleMode ?? false, SearchType);
		}

		SchemaGuidColumn GetPkColumnForQueryObjectType()
		{
			if (PKSchemaColumn != CargoWise.Schema.Schema.GenericPkColumn)
			{
				return PKSchemaColumn;
			}

			var schema = BusinessObjectFactory.GetTableSchemaFromType(QueryObjectType, throwOnError: false);
			return schema?.PK;
		}

		void RemoveDeletedUserDefinedFilters()
		{
			var layouts = GetLayoutsForUserDefinedFilters();
			var userDefinedFilters = this.OfType<ModuleUserDefinedFilter>().ToArray();

			foreach (var filter in userDefinedFilters)
			{
				if (!layouts.Any(x => ModuleUserDefinedFilter.GetPrefixedDescription(x) == filter.Description))
				{
					ModuleFilters.DisableAndRemoveFilterFromCollection(filter);

					filter.Delete();
				}
			}
		}

		public bool SupportsUserDefinedFilters => ShouldAddUserDefinedFilters && GetPkColumnForQueryObjectType() != null;

		#endregion

		#region Query

		public bool ReturnNoResultsQuery;
		public override ZQuery Filter
		{
			get
			{
				ZQuery result = null;

				if (SearchType == SearchType.Index)
				{
					result = DoGlowQuery();
				}
				else
				{
					result = CombineModuleFilters();
				}

				result.AddOptionRecompileConditionally = true;

				if (IsAnyModuleFilterThatOverridesAllOtherFiltersActive)
				{
					result.IgnoreActiveFilter = true;
				}

				if (ReturnNoResultsQuery)
				{
					result.IsNoResultQuery = true;
				}
				return result;
			}
		}

		public ZQuery DoGlowQuery()
		{
			if (ReturnNoResultsQuery)
			{
				return new ZQuery { IsNoResultQuery = true, };
			}

			var entityType = IndexSearchFields?.EntityType;
			if (entityType == null)
			{
				// this can happen if we run an index query when indexing is not allowed (i.e. filters are not available)
				return new ZQuery() { IsNoResultQuery = true };
			}

			var glowResults = GetGlowQueryResult();

			if (glowResults == null || glowResults.Status != GlowIndexQueryStatus.Success || !glowResults.ErrorMessage.IsNullOrEmpty())
			{
				var errorMessage = glowResults?.ErrorMessage ?? Res.GetString("1c23d216-d532-485c-ba8e-2a24e33b73bb", "Null Result");
				OnGlowIndexQueryErrorAction.Invoke(errorMessage);
				return new ZQuery() { IsNoResultQuery = true };
			}

			if (!string.IsNullOrEmpty(glowResults?.WarningMessage))
			{
				FilterStrips.Cast<FilterStrip>().ForEach(strip =>
				{
					strip.Validation.AddGlowResultWarningMessage(glowResults.WarningMessage);
				});
			}

			return IndexSearchFilterHelper.ConvertResultToZQuery(glowResults, entityType);
		}

		public Action<string> OnGlowIndexQueryErrorAction;

		void ReportGlowIndexQueryError(string errorMessage)
		{
			var queryParam = new GlowIndexQueryParam(GetIndexSearchQueries()?.ToList(), IndexSearchFields?.EntityType);
			var url = queryParam.GetQueryUri(queryParam.MaxQueryResults);
			ErrorReporter.ReportOnce("Error When Getting FilterStripBusinessObject.Filter for Index Search", $"ErrorMessage: {errorMessage}\r\nGlowQueryUri: {url}");
		}

		protected virtual ZQuery CombineModuleFilters() => ModuleFilters.GetFilterQuery(ActiveModuleFiltersForQuery, ApplyToFilterGroups);

		protected virtual void ApplyToFilterGroups(ZQuery query, IEnumerable<ModuleFilter> filtersInGroup)
		{
		}

		internal bool IsFilterQueryStale => ModuleFilters.IsQueryStale(ActiveModuleFiltersForQuery);

		public List<ModuleFilter> ActiveModuleFiltersForQuery
		{
			get
			{
				var result = new List<ModuleFilter>();

				if (IsAnyModuleFilterThatOverridesAllOtherFiltersActive)
				{
					foreach (var moduleFilter in ActiveModuleFilters)
					{
						if ((moduleFilter.IsExclusive || moduleFilter.IsMandatorySecurityFilter || moduleFilter.Description == UnionOrOrDescription)
							&& !moduleFilter.IsEmpty)
						{
							AddModuleFilter(result, moduleFilter);
						}
					}

					foreach (var moduleFilter in AlwaysAppliedModuleFilters)
					{
						if (!moduleFilter.IsExclusive && moduleFilter.Visibility == FilterVisibility.AlwaysAppliedAndHidden)
						{
							AddModuleFilter(result, moduleFilter);
						}
					}
				}
				else
				{
					foreach (var moduleFilter in ActiveModuleFilters)
					{
						AddModuleFilter(result, moduleFilter);
					}

					foreach (var moduleFilter in AlwaysAppliedModuleFilters)
					{
						var filterIsOverriddenByUser = ActiveModuleFilters != null && (ActiveModuleFilters.Contains(moduleFilter) || ActiveModuleFiltersContainsClonedFilter(moduleFilter.Code));
						if (!filterIsOverriddenByUser)
						{
							AddModuleFilter(result, moduleFilter);
						}
					}
				}

				result.Sort(ModuleFilterComparisonStrategy.CompareModuleFilters);

				return result;
			}
		}

		static void AddModuleFilter(ICollection<ModuleFilter> filters, ModuleFilter moduleFilter)
		{
			moduleFilter.OnAddToActiveFiltersQuery();
			filters.Add(moduleFilter);
		}

		bool ActiveModuleFiltersContainsClonedFilter(string alwaysAppliedFilterCode)
		{
			var clonedFilterRegex = new Regex(alwaysAppliedFilterCode + @" \([1-9]\)$");
			return ActiveModuleFilters.Any(activeFilter => clonedFilterRegex.IsMatch(activeFilter.Code));
		}

		bool IsAnyModuleFilterThatOverridesAllOtherFiltersActive
		{
			get
			{
				var result = false;
				foreach (var moduleFilter in ActiveModuleFilters)
				{
					if (moduleFilter.IsExclusive && !moduleFilter.IsEmpty)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		#region Module Filter Comparison

		public ModuleFilterComparisonStrategy ModuleFilterComparisonStrategy
		{
			get => moduleFilterComparisonStrategy ?? (moduleFilterComparisonStrategy = new ModuleFilterComparisonStrategy());
			set
			{
				moduleFilterComparisonStrategy = value;
			}
		}
		ModuleFilterComparisonStrategy moduleFilterComparisonStrategy;

		#endregion

		#endregion

		#region Expensive Query

		public override bool IsExpensiveQuery
		{
			get { return ExpensiveActiveModuleFiltersForQuery.Any(); }
		}

		internal IEnumerable<ModuleFilter> ExpensiveActiveModuleFiltersForQuery
		{
			get { return ActiveModuleFiltersForQuery.Where(filter => filter.IsExpensiveQuery && !filter.IsEmpty); }
		}

		#endregion

		#region Load Layout

		bool IFilterStripBusinessObject.LoadLayout(StmModuleFilter layout, bool maySkipLayoutIfHasInitialCode, bool disableValidation)
		{
			return LoadLayout(layout, maySkipLayoutIfHasInitialCode, disableValidation, true);
		}

		bool RunPopupSecurityCheck()
		{
			bool result = true;
			ParentModule.ParentModuleID = ParentModuleID;

			var securityCheckpointForPopups = ParentModule.GetSecurityCheckpointForPopups();

			if (securityCheckpointForPopups != null)
			{
				var securityCheckpointDenied = securityCheckpointForPopups.Where(x => !x.IsAllowed).ToArray();
				if (securityCheckpointDenied.Length > 0)
				{
					result = false;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Need to check these two specific one")]
		public bool LoadLayout(StmModuleFilter layout, bool maySkipLayoutIfHasInitialCode = false, bool disableValidation = false, bool shouldReset = true)
		{
			if (maySkipLayoutIfHasInitialCode && !IsCurrentSearchTypeSupportedByCurrentDefaults)
			{
				layout = null;
				SwitchSearchType();
			}
			else if (layout != null && !layout.IsDeleted)
			{
				SearchType = layout.S9_IsIndexSearch ? SearchType.Index : SearchType.Sql;
			}

			//fix LimitedColumns state after it might have been modified
			if (ParentModule != null && !isPreviewing)
			{
				var isAllowed = RunPopupSecurityCheck();
				if (ParentModule.LimitedColumns != null && isAllowed)
				{
					shouldReset = true;
					ParentModule.LimitedColumns = null;
				}
				else if (ParentModule.LimitedColumns == null && !isAllowed)
				{
					var typeOfTopLevelBizo = (ParentModule as IZFilterGridModule)?.TypeOfTopLevelBusinessObject;
					if (typeOfTopLevelBizo != null && typeOfTopLevelBizo.IsSubclassOf(typeof(BusinessObject)))
					{
						shouldReset = true;
						ParentModule.LimitedColumns = ObjectFactory.Get<IZLimitedColumnsProvider>("IZLimitedColumnsProvider", typeOfTopLevelBizo);
					}
				}
			}

			CheckFilterRuleConditions(layout);

			if (disableValidation)
			{
				SuspendValidation();
			}

			if (shouldReset)
			{
				ResetModuleFilters(); // mostly to reset the IsActive state
			}
			else
			{
				foreach (var filter in ModuleFilters)
				{
					//something like this is lightweight and correct hopefully?
					filter.IsActive = false;
				}
			}

			var applyLayout = !maySkipLayoutIfHasInitialCode || initialCode.IsEmpty || initialProperty.IsEmpty;
			var collection = applyLayout && layout != null && !layout.IsDeleted ? GetFilterStrips(layout, ModuleFilters) : new FilterStripCollection(ModuleFilters);

			//pass 1: check if we actually loaded a LimitedColumns setup in non-LimitedColumns context and if so, fix
			for (var x = 0; x < collection.Count; x++)
			{
				if (ModuleFilters[collection[x].FilterDescription] == null)
				{
					if (ParentModule != null && (collection[x].FilterDescription == "Code" || collection[x].FilterDescription == "Description"))
					{
						var typeOfTopLevelBizo = (ParentModule as IZFilterGridModule)?.TypeOfTopLevelBusinessObject;
						if (typeOfTopLevelBizo != null && typeOfTopLevelBizo.IsSubclassOf(typeof(BusinessObject)))
						{
							ParentModule.LimitedColumns = ObjectFactory.Get<IZLimitedColumnsProvider>("IZLimitedColumnsProvider", typeOfTopLevelBizo);
							LoadModuleFilters();
							//have to load filter strips again to grab property values properly
							collection = applyLayout && layout != null && !layout.IsDeleted ? GetFilterStrips(layout, ModuleFilters) : new FilterStripCollection(ModuleFilters);
							break;
						}
					}
				}
			}

			//pass 2: remove filter strips with no backing module filters
			for (var x = 0; x < collection.Count; x++)
			{
				if (ModuleFilters[collection[x].FilterDescription] == null)
				{
					collection.Remove(collection[x]);
					x--;
				}
			}

			if (SearchType == SearchType.Sql && ModuleFilters.Any(v => (v is IIndexSearchModuleFilter && v is not ModuleUserDefinedFilter) || (v is ModuleUserDefinedFilter userDefinedFilter && userDefinedFilter.IsIndexSearch)))
			{
				ErrorReporter.ReportOnce("Cache mismatch for SearchType of FilterStripBO when loading layout", $@"information:
IsWebServiceOrWeb: {Globals.IsWebServiceOrWeb}
,layoutFilterName: {layout?.S9_FilterName}
,layoutModuleId: {layout?.S9_ModuleID}
,layoutSearchType: {layout?.S9_IsIndexSearch}
,filterBOSearchType: {SearchType}
,filterBOParentModuleId: {ParentModule?.ID}
,isInfilterRule: {IsInFilterRuleMode}
,Filters: {string.Join(", ", ModuleFilters.Select(f => f.Description))}
,Active Filters: {string.Join(", ", ActiveModuleFilters.Select(f => f.Description))}
,Index Filters: {string.Join(",", ModuleFilters.Where(v => v is IIndexSearchModuleFilter).Select(f => f.Description))}
,QueryObjectType: {QueryObjectType?.FullName}
");
			}

			if (ContainsDefaults)
			{
				ApplyDefaults(collection.Cast<FilterStrip>().Select(strip => strip.FilterDescription));
			}

			if (!initialCode.IsEmpty && !initialProperty.IsEmpty)
			{
				ApplyInitialCode(initialCode, initialProperty);
			}

			AddAlwaysVisibleFilters(collection);

			//at this point, check security right
			var containsSecurityRightGranted = EnvProxy.Instance.Security.ContainsInNumbersAndReferences.IsAllowed;
			if (!containsSecurityRightGranted)
			{
				foreach (var filter in ModuleFilters)
				{
					//should be `is ModuleFilterWithListAndComparisonOperators<T>` but not sure how to cast to a generic type
					if (filter is ModuleTextBaseFilter textFilter && textFilter.Category == FilterCategories.NumbersAndReferences)
					{
						textFilter.ContainsBanned = true;
						textFilter.Validation.ValidateComparisonOperator();
					}
				}
			}

			FilterStrips = collection;

			lastUsedLayout = layout;
			OnLayoutLoaded();

			return applyLayout;
		}

		void CheckFilterRuleConditions(StmModuleFilter layout)
		{
			if (layout != null && !layout.IsDeleted && layout.S9_FilterType == StmModuleFilterTypes.Codes.FilterRule && ShouldAddUserDefinedFiltersCore)
			{
				if (!IsInFilterRuleMode || (ParentModule == null && ModuleType == null))
				{
					ErrorReporter.ReportOnce("Filter Rule layouts must only be loaded into FilterStripBusinessObjects created via RelatedModuleFiltersHelper. It looks like you've created the FilterStripBusinessObject some other way, and a few things won't be set up correctly as a result. SAD!");
				}
			}
		}

		public FilterStripCollection GetFilterStrips(StmModuleFilter layout)
		{
			return GetFilterStrips(layout, ModuleFilters.ShallowClone());
		}

		FilterStripCollection GetFilterStrips(StmModuleFilter layout, ModuleFilterCollection moduleFilters)
		{
			FilterStripCollection result = null;

			if (layout != null)
			{
				result = ReadFilterStripsFromXml(layout, moduleFilters, LayoutsHelper);
			}
			else
			{
				result = new FilterStripCollection(moduleFilters);
			}
			result.ModuleFilterChanged += delegate
			{ OnLayoutChanged(); };
			return result;
		}

		public int GetFilterStripsCount(StmModuleFilter layout)
		{
			return GetFilterStrips(layout).Count;
		}

		public event EventHandler LayoutChanged;

		public void OnLayoutChanged()
		{
			if (LayoutChanged != null)
			{
				LayoutChanged(this, EventArgs.Empty);
			}
		}

		public void AddAlwaysVisibleFilters()
		{
			AddAlwaysVisibleFilters(FilterStrips);
		}

		void AddAlwaysVisibleFilters(FilterStripCollection collection)
		{
			var alwaysVisibleFilters = new List<ModuleFilter>(AlwaysVisibleModuleFilters);
			var strips = new FilterStrip[alwaysVisibleFilters.Count];

			foreach (FilterStrip strip in collection.ToArray())
			{
				if (strip.CurrentModuleFilter != null && strip.CurrentModuleFilter.Visibility == FilterVisibility.AlwaysVisible)
				{
					var index = alwaysVisibleFilters.IndexOf(strip.CurrentModuleFilter);
					strips[index] = strip;
					collection.Remove(strip);
				}
			}

			for (var index = 0; index < strips.Length; index++)
			{
				if (strips[index] == null)
				{
					var strip = collection.AddNew(alwaysVisibleFilters[index]);

					collection.Remove(strip);
					strips[index] = strip;
				}
			}

			if (ContainsDefaults)
			{
				AddDuplicateDefaultFilterStripCollection(collection);
			}

			for (var index = strips.Length - 1; index >= 0; index--)
			{
				((IList)collection).Insert(0, strips[index]);
			}
		}

		public event EventHandler LayoutLoaded;

		void OnLayoutLoaded()
		{
			if (LayoutLoaded != null)
			{
				LayoutLoaded(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Save Layout

		protected override bool IsDataInRowAccessibleForDelete
		{
			get { return true; } // this bizO does not use the row
		}

		public StmModuleFilter SaveLayout(ZString layoutName, bool global = false)
		{
			StmModuleFilter result = null;
			if (!layoutName.IsEmpty && FilterStrips.Count > 0)
			{
				result = ObjectFactory.Get<IDataGridLayoutManager>().SavePreconfiguredLayout(this, layoutName, false, global, SaveColumnLayout.Ignore);
			}
			return result;
		}

		public void FillLayoutValues(StmModuleFilter layout, ModuleIdentifier moduleId)
		{
			layout.S9_FilterData = GetFilterStripLayoutAsXml();
			var userData = layout.GetOrCreateLayoutUserData(LayoutsHelper);
			layout.S9_ModuleID = moduleId.ToString();
			userData.S0_FilterDataValues = GetFilterStripLayoutValuesAsXml();
		}

		internal ZBlob GetFilterStripLayoutAsXml()
		{
			return FilterStrips.GetLayoutAsXml();
		}

		internal ZBlob GetFilterStripLayoutValuesAsXml()
		{
			return FilterStrips.GetLayoutValuesAsXml();
		}

		#endregion

		#region ReadFilterStripsFromXml / WriteFilterStripsToXml

		public FilterStripCollection ReadFilterStripsFromXml(StmModuleFilter layout, ModuleFilterCollection moduleFilters, FilterStripLayoutsHelper layoutsHelper)
		{
			Argument.NotNull(moduleFilters, "moduleFilters", "Cannot deserialize filterstrips if the ModuleFilterCollection is null."); // This is an exception text
			Argument.NotNull(layoutsHelper, "layoutsHelper", "Cannot deserialize filterstrips if the FilterStripLayoutsHelper is null."); // This is an exception text

			var collection = new FilterStripCollection(moduleFilters);

			try
			{
				if (layout.S9_ModuleID.EndsWith(ObjectFactory.Get<IGridColourFactory>().GridColorStripCode))
				{
					collection.LoadFromXml(layout, GetLayoutUserDataForColorScheme(layout));
				}
				else
				{
					collection.LoadFromXml(layout, layout.GetLayoutUserData(layoutsHelper));
				}
			}
			catch (XmlException)
			{
				// no need to report because we know the collection was already badly saved
			}

			return collection;
		}

		public StmModuleFilterUserData GetLayoutUserDataForColorScheme(StmModuleFilter layout)
		{
			var query = new ZQuery();
			query.AddToFilter(StmModuleFilterUserDataSchema.S0_S9, layout.PK);

			return Factory.LoadTop1<StmModuleFilterUserData>(query);
		}

		public void WriteFilterStripsToXml(StmModuleFilter layout, FilterStripCollection filterStrips, FilterStripLayoutsHelper layoutsHelper)
		{
			Argument.NotNull(filterStrips, "filterStrips", "Cannot serialize null filterstrips."); // This is an exception text
			Argument.NotNull(layoutsHelper, "layoutsHelper", "Cannot serialize filterstrips with a null layouts helper."); // This is an exception text

			var layoutUserData = layout.GetOrCreateLayoutUserData(layoutsHelper);
			layout.S9_FilterData = filterStrips.GetLayoutAsXml();
			layoutUserData.S0_FilterDataValues = filterStrips.GetLayoutValuesAsXml();
		}

		public void CopyDetailsAndFiltersFrom(FilterStripBusinessObject otherFilterBusinessObject)
		{
			IsInFilterRuleMode = otherFilterBusinessObject.IsInFilterRuleMode;
			ReplaceStripsWithOthers(otherFilterBusinessObject);
		}

		void ReplaceStripsWithOthers(FilterStripBusinessObject filterBusinessObjectWithReplacementFilters)
		{
			var thisType = GetType();
			var otherType = filterBusinessObjectWithReplacementFilters.GetType();

			if (thisType != otherType)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot copy filters between FilterStripBusinessObjects of different types. Source type: {0}, Destination type: {1}", otherType, thisType));
			}

			var layout = new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(ReplaceStripsWithOthers) }.New<StmModuleFilter>();
			((IModifyModuleAndGridLayout)filterBusinessObjectWithReplacementFilters).SerialiseLayoutAndWriteTo(layout);
			LoadLayout(layout);
		}

		#endregion

		#region Find Layout

		public virtual StmModuleFilter FindLayout(ZString layoutName, ZBool isPublished, ZGuid? gcPk)
		{
			return FindLayout(layoutName, isPublished, false, gcPk);
		}

		public virtual StmModuleFilter FindLayout(ZString layoutName, ZBool isPublished)
		{
			return FindLayout(layoutName, isPublished, false, null);
		}

		public StmModuleFilter FindLayout(ZString layoutName)
		{
			return FindLayout(layoutName, false, true, null);
		}

		StmModuleFilter FindLayout(ZString layoutName, ZBool isPublished, bool shouldIgnoreIsPublished, ZGuid? gcPk)
		{
			StmModuleFilter result = null;
			var query = new ZQuery(StmModuleFilterSchema.S9_FilterName, layoutName);

			if (!shouldIgnoreIsPublished)
			{
				query.AddToFilter(StmModuleFilterSchema.S9_IsPublished, isPublished);
			}

			if (gcPk != null)
			{
				query.AddToFilter(StmModuleFilterSchema.S9_GC, gcPk == ZGuid.Empty ? DBNull.Value : gcPk);
			}

			if (this is IRelatedModuleFilterBusinessObject && LayoutsHelper.CurrentUserPk == ZGuid.Empty && (shouldIgnoreIsPublished || !isPublished))
			{
				query.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, null);
				result = Factory.LoadTop1<StmModuleFilter>(query);
			}

			return result ?? Layouts.Find(query).FirstOrDefault();
		}

		StmModuleFilter FindLayout(ZGuid filterPK)
		{
			return (StmModuleFilter)Layouts.FindByPK(filterPK);
		}

		#endregion

		#region Layouts

		public StmModuleFilterCollection Layouts => fLayouts ??= new StmModuleFilterCollection(LayoutsFactory, LayoutContext, LayoutsHelper, GetLayoutsQuery());

		ZQuery GetLayoutsQuery()
		{
			var result = new ZQuery(StmModuleFilterSchema.S9_FilterType, SQLComparisonOperator.NotEqual, StmModuleFilterTypes.Codes.FilterRule);
			if (!IsGlowIndexSearchAllowed || !HasIndexSearchFields)
			{
				result.AddToFilter(StmModuleFilterSchema.S9_IsIndexSearch, false);
			}

			return result;
		}

		StmModuleFilterCollection fLayouts;
		IFavoriteLayoutsHandler FavoritesHandler => favoritesHandler ?? (favoritesHandler = ObjectFactory.Get<IFavoriteLayoutsHandler>("IFavoriteLayoutsHandler", LayoutsFactory, (string)LayoutContext));
		IFavoriteLayoutsHandler favoritesHandler;

		public List<StmModuleFilter> FavoriteLayouts
		{
			get
			{
				var favoriteLayouts = FavoritesHandler.FavoriteFilters as IEnumerable<IStmLink>;
				return Layouts.Where(filter => favoriteLayouts.Select(link => (link as AutoStmLink).STL_ItemPK).Contains(filter.PK)).OrderBy(x => x.S9_FilterName).ToList();
			}
		}

		public ReadOnlyCollection<StmModuleFilter> Layouts_PublishedOnly
		{
			get { return GetLayouts(true).Where(x => !x.IsUserDefinedFilter).ToList().AsReadOnly(); }
		}

		public ReadOnlyCollection<StmModuleFilter> Layouts_UnpublishedOnly
		{
			get { return GetLayouts(false).Where(x => !x.IsUserDefinedFilter).ToList().AsReadOnly(); }
		}

		protected virtual ReadOnlyCollection<StmModuleFilter> GetLayouts(bool isPublished)
		{
			var result = new List<StmModuleFilter>();

			foreach (var layout in Layouts)
			{
				if (layout.S9_IsPublished == isPublished)
				{
					result.Add(layout);
				}
			}

			return result.AsReadOnly();
		}

		public ReadOnlyCollection<StmModuleFilter> Layouts_WebCompanyOnly(ZGuid companyPK)
		{
			var result = new List<StmModuleFilter>();

			if (Globals.IsWeb)
			{
				foreach (var layout in Layouts)
				{
					if (layout.IsPublishedForCompanyInWeb && layout.S9_GC == companyPK)
					{
						result.Add(layout);
					}
				}
			}

			return result.AsReadOnly();
		}

		BusinessObjectFactory LayoutsFactory => Factory;

		#region LayoutsHelper

		protected virtual FilterStripLayoutsHelper GetNewLayoutsHelper()
		{
			return new FilterStripLayoutsHelper();
		}

		public FilterStripLayoutsHelper LayoutsHelper
		{
			get { return fLayoutsHelper ?? (LayoutsHelper = GetNewLayoutsHelper()); }
			set
			{
				if (value != fLayoutsHelper)
				{
					fLayouts = null;
					fLayoutsHelper = value;
				}
			}
		}

		FilterStripLayoutsHelper fLayoutsHelper;

		#endregion

		#endregion

		#region Last Used Filter Layout

		public StmModuleFilter LastUsedLayout
		{
			get
			{
				if (!LastUsedLayoutLoaded)
				{
					lastUsedLayout = GetLastUsedLayout();
				}

				return lastUsedLayout;
			}

#if DEBUG
			set
			{
				lastUsedLayout = value;
			}
#endif
		}

		public StmModuleFilter GetLastUsedLayout()
		{
			var lastUsedFilterData = GetLastUsedLayoutName();
			return lastUsedFilterData != null ? FindLayout(lastUsedFilterData.SD_GuidValue) : null;
		}

		public void SaveLastUsedLayout(ZGuid layoutPk)
		{
			var layout = FindLayout(layoutPk);
			if (layout != null)
			{
				lastUsedLayout = layout;
				SaveLastUsedLayout();
			}
		}

		public void SaveLastUsedLayout()
		{
			if (lastUsedLayout != null)
			{
				try
				{
					SaveLastUsedLayoutUnsafe();
				}
				catch (ZSaveException saveException)
				{
					if (saveException.IsCriticalException())
					{
						throw;
					}

					if (!string.IsNullOrEmpty(saveException.IndexNameIfUniqueIndexViolation) || (saveException is ZSaveConcurrencyException))
					{
						CancelAllFilterLayoutsChangesInFactory();
						var tempLastUserLayout = lastUsedLayout;
						ResetLastUsedLayout();
						lastUsedLayout = tempLastUserLayout;
						LastUsedLayoutFactory.ClearQueryCache();

						SaveLastUsedLayoutUnsafe();
					}
				}
			}
		}

		void CancelAllFilterLayoutsChangesInFactory()
		{
			foreach (var changedLayout in LastUsedLayoutFactory.GetChanges().GetChangedObjects())
			{
				if (changedLayout.SessionInstance.GetType() == typeof(StmModuleFilter))
				{
					changedLayout.Delete();
				}
			}
		}

		protected virtual void SetLastUsedLayoutValue(StmData lastUsedFilterStmData)
		{
			lastUsedFilterStmData.SD_GuidValue = lastUsedLayout.PK;
		}

		void SaveLastUsedLayoutUnsafe()
		{
			if (lastUsedLayout != null)
			{
				var lastUsedFilterStmData = GetLastUsedLayoutName();
				if (lastUsedFilterStmData == null)
				{
					lastUsedFilterStmData = LastUsedLayoutFactory.New<StmData>();
					lastUsedFilterStmData.SD_Owner = LayoutsHelper.CurrentUserPk;
					lastUsedFilterStmData.SD_DepartmentGuid = EnvProxy.Instance.CurrentCompany.PK;
					lastUsedFilterStmData.SD_Name = LayoutContext;
				}
				SetLastUsedLayoutValue(lastUsedFilterStmData);
				ConcurrencyInfo.SetConcurrencyPolicy(lastUsedFilterStmData, nameof(StmData.SD_GuidValue), ConcurrencyPolicy.Ignore);
				LastUsedLayoutFactory.Save();
			}
		}

		public void ResetLastUsedLayout()
		{
			var lastUsedLayoutStmData = GetLastUsedLayoutName();
			if (lastUsedLayoutStmData != null)
			{
				lastUsedLayoutStmData.Delete();
				LastUsedLayoutFactory.Save();
			}
			lastUsedLayout = null;
		}

		protected virtual ZQuery LastUsedLayoutNameQuery()
		{
			var query = new ZQuery();
			query.AddToFilter(StmDataSchema.SD_Owner, LayoutsHelper.CurrentUserPk);
			query.AddToFilter(StmDataSchema.SD_DepartmentGuid, EnvProxy.Instance.CurrentCompany.PK);
			query.AddToFilter(StmDataSchema.SD_Name, LayoutContext);
			return query;
		}

		StmData GetLastUsedLayoutName()
		{
			var query = LastUsedLayoutNameQuery();
			return LastUsedLayoutFactory.LoadTop1<StmData>(query);
		}

		public bool LastUsedLayoutLoaded
		{
			get { return (lastUsedLayout != null); }
		}

#if DEBUG
		public
#endif
			BusinessObjectFactory LastUsedLayoutFactory => Factory;

		protected StmModuleFilter lastUsedLayout;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			if (ActiveModuleFilters != null)
			{
				foreach (var filter in ActiveModuleFilters)
				{
					filter.ActiveModuleFiltersProvider = this;
					filter.Validation.ValidateAll();
				}
			}
		}

		#endregion

		#region Filter Defaults

		public bool ContainsDefaults
		{
			get
			{
				return containsDefaultsMap.TryGetValue(SearchType, out var ret) && ret;
			}
			set
			{
				containsDefaultsMap[SearchType] = value;
			}
		}
		readonly Dictionary<SearchType, bool> containsDefaultsMap = new();

		public bool ShouldSetDefaults
		{
			get
			{
				return !shouldSetDefaultsMap.TryGetValue(SearchType, out var ret) || ret;
			}
			set
			{
				shouldSetDefaultsMap[SearchType] = value;
			}
		}
		readonly Dictionary<SearchType, bool> shouldSetDefaultsMap = new();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		protected override void SetExternalDefaultsCore(FilterBusinessObjectDefault filterDefault, IEnumerable<ZString> skippedOrCategory = null)
		{
			if (ParentModule?.LimitedColumns != null)
			{
				return;
			}

			var moduleFilter = GetModuleFilter(filterDefault);
			if (moduleFilter != null)
			{
				if (ShouldSetDefaults)
				{
					if (!moduleFilter.WasClearedWhenSettingDefault)
					{
						moduleFilter.Clear();
						moduleFilter.WasClearedWhenSettingDefault = true;
					}

					moduleFilter[filterDefault.PropertyName] = filterDefault.Value;

					if (moduleFilter is IModuleFilterWithSelectedFilters moduleWithSelectedFilters && filterDefault.ChildDefaults != null)
					{
						moduleWithSelectedFilters.SelectedFilters.SetExternalDefaults(filterDefault.ChildDefaults);
						moduleWithSelectedFilters.SelectedFilters.AddAlwaysVisibleFilters();
					}

					if (moduleFilter is IModuleFilterWithComparisonOperator moduleFilterWithComparisonOperator
						&& !filterDefault.ComparisonOperator.IsEmpty
						&& moduleFilterWithComparisonOperator.AllowedComparisonOperators.Contains((string)filterDefault.ComparisonOperator))
					{
						moduleFilterWithComparisonOperator.ComparisonOperator = filterDefault.ComparisonOperator;
					}
				}

				if (moduleFilter.Visibility != FilterVisibility.AlwaysAppliedAndHidden)
				{
					moduleFilter.Visibility = FilterVisibility.AlwaysVisible;
				}

				if (!(skippedOrCategory?.Contains(GetModuleFilterName(filterDefault)) ?? false))
				{
					moduleFilter.OrCategory = filterDefault.Category;
				}

				if (!filterDefault.IsRemovable)
				{
					moduleFilter.ReadOnly = true;
				}

				ContainsDefaults = true;
			}
			else
			{
				var message = string.Format(
					"Setting defaults for FilterBizO '{0}'. Filter '{1}' with property '{2}' not found. Value '{3}'.",
					GetType().FullName, filterDefault.FilterName, filterDefault.PropertyName, filterDefault.Value);
				message += $"Default Filter Load Message: {DefaultFilterLoadMessage}";

				Globals.Message.ShowDeveloperException(new BadExternalDefaultException(message, null));
			}
		}

		ModuleFilter GetModuleFilter(FilterBusinessObjectDefault filterDefault)
		{
			if (filterDefault.Instance.IsDefault)
			{
				return ModuleFilters[GetModuleFilterName(filterDefault)];
			}
			else
			{
				var key = GetKey(filterDefault);

				if (ModuleFilters.TryGetDuplicateFilter(key, out var filter))
				{
					return filter;
				}
				else
				{
					var newFilter = CreateNewFilterFromDuplicateDefault(filterDefault);
					return ModuleFilters.AddNewDuplicateFilter(key, newFilter);
				}
			}
		}

		ZString GetKey(FilterBusinessObjectDefault filterDefault)
		{
			return GetModuleFilterName(filterDefault) + " (" + filterDefault.Instance + ")";
		}

		ModuleFilter CreateNewFilterFromDuplicateDefault(FilterBusinessObjectDefault filterDefault)
		{
			var filterDescription = GetModuleFilterName(filterDefault);
			var templateFilter = this[filterDescription];
			var newFilter = templateFilter.ShallowCloneAndClearValues(filterDescription);
			newFilter.IsDuplicateDefault = true;
			return newFilter;
		}

		protected virtual ZString GetModuleFilterName(FilterBusinessObjectDefault filterDefault)
		{
			return filterDefault.FilterName;
		}

		#endregion

		#region Initial Code For Search

		public override void SetInitialCodeForSearch(ZString code, Type typeOfElementsToFind)
		{
			initialProperty = CodePropertyAttribute.CodePropertyNameFromType(typeOfElementsToFind);
			initialCode = code;
			ApplyInitialCode(initialCode, initialProperty);
		}

		public void SetInitialCodeForSearch(ZString code, string propertyName)
		{
			SetInitialCodeForSearchCore(code, propertyName);
		}

		protected virtual void SetInitialCodeForSearchCore(ZString code, string propertyName)
		{
			initialProperty = propertyName;
			initialCode = code;
			ApplyInitialCode(code, propertyName);
		}

		ZString initialCode;
		ZString initialProperty;

		protected virtual void ApplyInitialCode(ZString code, string propertyName)
		{
			if (initialCode.Contains(':'))
			{
				foreach (var moduleFilter in ModuleFilters.ToSortedArrayWithIsExclusiveLast())
				{
					var initialCodeToSet = initialCode;
					if (moduleFilter.ShouldSetValueFromInitialCode("FakePropertyToCheckOnlyPrefixesFirst", initialCode))
					{
						//pass current initial code value to SetValueFromInitialCode so all changes to the filter are applied
						if (!ShouldSetDefaults)
						{
							initialCodeToSet = moduleFilter.GetFormattedInitialCode_Prefix();
						}

						moduleFilter.SetValueFromInitialCode("FakePropertyToCheckOnlyPrefixesFirst", initialCodeToSet);

						return;
					}
				}
			}
			foreach (var moduleFilter in ModuleFilters.ToSortedArrayWithIsExclusiveLast())
			{
				if (moduleFilter.ShouldSetValueFromInitialCode(initialProperty, initialCode))
				{
					var initialCodeToSet = initialCode;
					if (!ShouldSetDefaults)
					{
						//pass current initial code value to SetValueFromInitialCode so all changes to the filter are applied
						initialCodeToSet = moduleFilter.GetFormattedInitialCode_FilterColumnName();
					}

					moduleFilter.SetValueFromInitialCode(initialProperty, initialCodeToSet);

					return;
				}
			}
		}

		public bool ShouldLoadFilterBusinessObjectDefaults { get; set; }

		public void SetExternalDefaults()
		{
			if (!ContainsDefaults && ShouldLoadFilterBusinessObjectDefaults &&
				ParentModule is IZFilterModule { GridCollection: IFilterBusinessObjectDefaultsProvider defaultsProvider })
			{
				SetExternalDefaults(defaultsProvider);
			}
			else
			{
				ApplyDefaults();
			}
		}

		#endregion

		#region IEnumerable Members

		IEnumerator<ModuleFilter> IEnumerable<ModuleFilter>.GetEnumerator()
		{
			return ((IEnumerable<ModuleFilter>)ModuleFilters).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ModuleFilter>)this).GetEnumerator();
		}

		#endregion

		#region IFilterStripBusinessObjectInternals Members

		ZString IFilterStripBusinessObjectInternals.LayoutContext
		{
			get => LayoutContext;
			set => LayoutContext = value;
		}

		void IFilterStripBusinessObjectInternals.ClearLayoutsCache()
		{
			fLayouts = null;
		}

		protected ZString LayoutContext { get; set; }

		bool isPreviewing;

		#endregion

		#region AddFilterStrip Methods

		protected internal bool AddFilterStripUsed;

		public T AddFilterStrip<T>(string description)
			where T : ModuleFilter
		{
			AddFilterStripUsed = true;
			return (T)FilterStrips.AddNew(description).CurrentModuleFilter;
		}

		public ModuleTextFilter AddTextFilterStrip(string description, string propertyValue = null)
		{
			var filter = AddFilterStrip<ModuleTextFilter>(description);

			if (propertyValue != null)
			{
				filter.Property = propertyValue;
			}

			return filter;
		}

		public ModuleGuidFilter AddGuidFilterStrip(string description, ZGuid propertyValue = default(ZGuid))
		{
			var filter = AddFilterStrip<ModuleGuidFilter>(description);

			if (propertyValue != default(ZGuid))
			{
				filter.Property = propertyValue;
			}

			return filter;
		}

		public ModuleDateFilter AddDateFilterStrip(string description)
		{
			return AddFilterStrip<ModuleDateFilter>(description);
		}

		public ModuleNkFilter AddNkFilterStrip(string description, string propertyValue = null)
		{
			var filter = AddFilterStrip<ModuleNkFilter>(description);

			if (propertyValue != null)
			{
				filter.Property = propertyValue;
			}

			return filter;
		}

		#endregion

		#region ILayoutManageable Members

		public static string CorrectErrorsBeforeSavingFilterLayout
		{
			get { return Res.GetString("c56d0c4c-96e0-4212-83a6-aa63cb8de284", "There are errors. Please correct these before saving your filter layout."); }
		}

		public static string NothingToSave
		{
			get { return Res.GetString("56d35be6-5da3-4d72-9e7d-c54c4ef488f7", "All filters are empty, there is nothing to save."); }
		}

		string IModifyModuleAndGridLayout.ValidateAndGetErrorsForSavingLayout()
		{
			RunPreSaveValidation();

			var result = "";

			if (HasErrors)
			{
				result = CorrectErrorsBeforeSavingFilterLayout;
			}
			else if (ActiveModuleFilters == null || ActiveModuleFilters.Count == 0)
			{
				result = NothingToSave;
			}

			return result;
		}

		string IModifyModuleAndGridLayout.GetReasonLayoutNameNotAllowed(string layoutName, bool isPublished)
		{
			return string.Empty;
		}

		BusinessObjectFactory IModifyModuleAndGridLayout.Factory
		{
			get { return LayoutsFactory; }
		}

		IGridLayoutStorage IModifyModuleAndGridLayout.LayoutToDefault
		{
			get { return LastUsedLayout; }
		}

		IGridLayoutStorage IModifyModuleAndGridLayout.FindLayout(string layoutName, bool isPublished, ZGuid? gcPk)
		{
			return FindLayout(layoutName, isPublished, gcPk);
		}

		IGridLayoutStorage IModifyModuleAndGridLayout.FindLayout(string layoutName, bool isPublished)
		{
			return FindLayout(layoutName, isPublished);
		}

		IGridLayoutStorage IModifyModuleAndGridLayout.FindLayout(string layoutName)
		{
			return FindLayout(layoutName);
		}

		IGridLayoutStorage IModifyModuleAndGridLayout.FindLayout(ZGuid layoutPk)
		{
			return FindLayout(layoutPk);
		}

		IGridLayoutStorage IModifyModuleAndGridLayout.GetDefaultGridLayout()
		{
			return null;
		}

		string IModifyModuleAndGridLayout.LayoutSetIdentifierToSaveANewLayoutWith
		{
			get { return ((IFilterStripBusinessObjectInternals)this).LayoutContext; }
		}

		string[] IModifyModuleAndGridLayout.LayoutSetIdentifiers
		{
			get { return new string[] { ((IModifyModuleAndGridLayout)this).LayoutSetIdentifierToSaveANewLayoutWith }; }
		}

		void IModifyModuleAndGridLayout.SerialiseLayoutAndWriteTo(StmModuleFilter layoutStorage)
		{
			layoutStorage.S9_IsIndexSearch = SearchType == SearchType.Index;
			WriteFilterStripsToXml(layoutStorage, FilterStrips, LayoutsHelper);
		}

		StmModuleFilter IModifyModuleAndGridLayout.AddNewLayoutStorage()
		{
			return Layouts.AddNew();
		}

		IEnumerable<StmModuleFilter> IModifyModuleAndGridLayout.GetLayouts(bool isPublished)
		{
			return GetLayouts(isPublished);
		}

		IEnumerable<ILayoutDetailTreeNode> IModifyModuleAndGridLayout.GetLayoutDetailTree(StmModuleFilter layout)
		{
			return new TypedEnumerable<ILayoutDetailTreeNode>(GetFilterStrips(layout));
		}

		#endregion

		#region AdditionalDisplayFilter

		public ZQuery GetAdditionalPreviewFilter(BusinessObject dataSource, string moduleId, string dropDownCode)
		{
			if (dataSource == null)
			{
				return null;
			}

			isPreviewing = true;
			return GetAdditionalPreviewFilterCore(dataSource, moduleId, dropDownCode);
		}

		protected virtual ZQuery GetAdditionalPreviewFilterCore(BusinessObject dataSource, string moduleId, string dropDownCode)
		{
			return (dataSource as IFilterPreviewable)?.GetAdditionalPreviewFilter(moduleId, dropDownCode) ?? new ZQuery();
		}

		#endregion

		#region Template Record Filter

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public const string TemplateRecordsDescription = "Template Records";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public const string TemplateRecordsActive = "Template Active";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public const string TemplateRecordsTemplateName = "Template Name";

		public bool AllowTemplateRecords { get; set; }

		protected void AddTemplateRecordFilter(ModuleFilterCollection filters)
		{
			if (AllowTemplateRecords)
			{
				var templateRecordsFilter = filters.AddTextFilter(TemplateRecordsDescription, _ => new ZQuery(), TemplateRecordsFilterList);
				templateRecordsFilter.Category = FilterCategories.Other;
				templateRecordsFilter.SupportsXQuery = true;
				templateRecordsFilter.MultilingualDescription = ResString.GetMultilingualString("TemplateRecords|Visibility", "Template Records");

				var templateActiveFilter = filters.AddTextFilter("Template Active", _ => new ZQuery(), CancelledStatusList);
				templateActiveFilter.Category = FilterCategories.Other;
				templateActiveFilter.SupportsXQuery = true;
				templateActiveFilter.MultilingualDescription = ResString.GetMultilingualString("TemplateRecords|Active", "Template Active");

				var templateNameFilter = filters.AddTextFilter("Template Name", new GetTextQueryWithOperator((SQLComparisonOperator op, ZString str) => new ZQuery()));
				templateNameFilter.Category = FilterCategories.Other;
				templateNameFilter.SupportsXQuery = true;
				templateNameFilter.MultilingualDescription = ResString.GetMultilingualString("TemplateRecords|TemplateName", "Template Name");
			}
		}

		public CodeDescriptionPairList TemplateRecordsFilterList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(TemplateRecordsFilterCodes.TemplatesExcluded, ResString.GetMultilingualString("33cdbdcb-daf2-47d0-89cb-5ecc8b3b9469", "Templates Excluded"));
				list.AddPair(TemplateRecordsFilterCodes.TemplatesIncluded, ResString.GetMultilingualString("1042e359-8234-433e-9475-683ad058e443", "Templates Included"));
				list.AddPair(TemplateRecordsFilterCodes.TemplatesOnly, ResString.GetMultilingualString("432fc7f7-cda4-462b-9f3e-bf9622699b96", "Templates Only"));
				return list;
			}
		}

		public static class TemplateRecordsFilterCodes
		{
			public const string TemplatesExcluded = "TEX";
			public const string TemplatesIncluded = "TIN";
			public const string TemplatesOnly = "TON";
		}

		#endregion
		public IZModuleHelper ParentModule
		{
			get
			{
				return parentModule;
			}
			set
			{
				parentModule = value;
			}
		}

		IZModuleHelper parentModule;

		public void SetGlowFiltersIfAllowed()
		{
			// It should not be called after the module filters have been loaded,  as changing the search type afterwards will not trigger a reload of the filters.
			if (IsGlowIndexSearchAllowed && !AreModuleFiltersLoaded)
			{
				var pkColumn = GetPkColumnForQueryObjectType();
				var moduleId = ObjectFactory.Get<IModuleFactory>().GetRegisteredIdentifierByColumnNamePrefix(pkColumn.ColumnPrefix);

				IndexSearchFields = IndexSearchFilterHelper.GetSearchFields(moduleId);
				if (IndexSearchFields?.Status == GlowIndexQueryStatus.Success)
				{
					base.SearchType = SearchType.Index;
				}
			}
		}

		public virtual IGlowQuery AdditionalIndexSearchQuery => null;

		GlowIndexQueryResultCollection GetGlowQueryResult()
		{
			if (SearchType == SearchType.Index && ActiveModuleFiltersForQuery.Any(f => f is IIndexSearchModuleFilter))
			{
				var entityType = IndexSearchFields?.EntityType;
				var glowQueries = GetIndexSearchQueries().ToList();

				return IndexSearchFilterHelper.GetQueryResult(glowQueries, entityType);
			}

			return null;
		}

		public bool IsInFilterRuleMode { get; set; }
	}

	#region IFilterStripBusinessObjectInternals Interface

	public interface IFilterStripBusinessObjectInternals
	{
		ZString LayoutContext { get; set; }
		void ClearLayoutsCache();
	}

	#endregion

}
