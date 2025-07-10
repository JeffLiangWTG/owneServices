using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	public static class UserDefinedFilterHelper
	{
		public static StmModuleFilter[] GetLayoutsForUserDefinedFilters(BusinessObjectFactory factory, FilterStripLayoutsHelper layoutHelper, string moduleName, bool includePublishedOnly)
		{
			var query = new StmModuleFilter.Loader(factory).GetQuery(moduleName, layoutHelper);
			query.AddToFilter(StmModuleFilterSchema.S9_FilterType, StmModuleFilterTypes.Codes.UserDefined);

			if (includePublishedOnly)
			{
				query.AddToFilter(StmModuleFilterSchema.S9_IsPublished, true);
			}

			return factory.Load<StmModuleFilter>(query);
		}

		internal static bool CheckIsOkayToAdjustUserDefinedFilter(string selectedFilterName, MultilingualString selectedFilterNameMultilingual, BusinessObjectFactory factory, LayoutAdjustmentMode mode)
		{
			NotificationTypes messageType;
			var message = GetChangeUserDefinedFilterMessage(selectedFilterName, selectedFilterNameMultilingual, factory, mode, out messageType);

			if (!string.IsNullOrEmpty(message))
			{
				if (messageType == NotificationTypes.Error)
				{
					Globals.Message.ShowError(message);
					return false;
				}
				else if (messageType == NotificationTypes.Warning)
				{
					var result = ShowUserDefinedFilterInUseWarning(message);
					return result == DialogResult.OK;
				}
			}

			return true;
		}

		internal static string GetDirectlyUsedFilterWhichContainsNestedTargetFilter(LinkedList<string> completedTrail)
		{
			return completedTrail.First?.Next?.Value;
		}

		internal static string GetFullTrailString(LinkedList<string> completedTrail)
		{
			return string.Join(" -> ", completedTrail);
		}

		internal static bool FindFilterNestedDownHierarchy(FilterStripBusinessObject filterBizo, string targetFilterDescription, MultilingualString rootFilterMultilingualDescription, bool shouldAddRootFilterToTrail, out LinkedList<string> trail)
		{
			trail = new LinkedList<string>();
			var filters = GetFiltersToCheckForSelfReference(filterBizo);

			foreach (var filter in filters)
			{
				if (HasSelfReference(filter, targetFilterDescription, trail))
				{
					if (shouldAddRootFilterToTrail)
					{
						trail.AddFirst(rootFilterMultilingualDescription);
					}

					return true;
				}
			}

			return false;
		}

		internal static bool FindFilterNestedDownHierarchy(StmModuleFilter layout, string targetFilterDescription, out LinkedList<string> trail)
		{
			var moduleId = GetModuleIdFromModuleName(layout.S9_ModuleID);

			if (moduleId != null && moduleId != ModuleIDs.NotAssigned)
			{
				try
				{
					using (var module = ZModuleFactory.Instance.Create(moduleId))
					{
						if (module != null && module is ZFilterModule filterModule)
						{
							var isFilterRule = layout.S9_FilterType == StmModuleFilterTypes.Codes.FilterRule;
							FilterStripBusinessObject filterBizo;

							if (isFilterRule)
							{
								filterBizo = (FilterStripBusinessObject)RelatedModuleFiltersHelper.GetNewFilterBusinessObject(layout);
								((IFilterStripBusinessObjectInternals)filterBizo).LayoutContext = moduleId.Name;
							}
							else
							{
								filterBizo = filterModule.FilterBusinessObject;
							}

							if (filterBizo != null)
							{
								filterBizo.LoadLayout(layout);
								var shouldAddRootFilterToTrail = !isFilterRule;

								return FindFilterNestedDownHierarchy(filterBizo, targetFilterDescription, layout.S9_FilterNameMultilingual, shouldAddRootFilterToTrail, out trail);
							}
						}
					}
				}
				catch (ModuleGuiNotSupportedException)
				{
				}
			}

			trail = new LinkedList<string>();
			return false;
		}

		internal static void CheckFilterBizoDoesNotContainNestedNonpublishedUserDefinedFilters(FilterStripBusinessObject filterBizo, Func<string, string> singleFilterErrorGetter, Func<string, string> multipleFilterErrorGetter, ZPropertyInfo propertyInfo, string rootFilterDescription = null)
		{
			var trails = new LinkedList<LinkedList<string>>();
			var startingTrail = new LinkedList<string>();

			if (rootFilterDescription != null)
			{
				startingTrail.AddLast(rootFilterDescription);
			}

			if (FindFiltersNestedDownHierarchy(filterBizo, x => x is ModuleUserDefinedFilter userDefinedFilter && !userDefinedFilter.IsPublished, startingTrail, trails))
			{
				var trailStrings = trails
					.OrderBy(x => x.Count)
					.ThenBy(x => x.First.Value)
					.ThenBy(x => x.Last.Value)
					.Select(GetFullTrailString)
					.ToArray();

				var message = trailStrings.Length == 1 ? singleFilterErrorGetter(trailStrings.Single()) : multipleFilterErrorGetter(string.Join(System.Environment.NewLine, trailStrings));
				propertyInfo.AddError(message);
			}
		}

		static bool FindFiltersNestedDownHierarchy(FilterStripBusinessObject filterBizo, Func<IModuleFilterWithSelectedFilters, bool> matchCondition, LinkedList<string> trailForThisFilterLevel, LinkedList<LinkedList<string>> trails)
		{
			var filters = GetFiltersToCheckForSelfReference(filterBizo);
			var wasMatchFound = false;

			foreach (var filter in filters)
			{
				var trailForThisFilter = new LinkedList<string>(trailForThisFilterLevel);
				trailForThisFilter.AddLast(filter.MultilingualDescription ?? filter.Description);

				if (matchCondition(filter))
				{
					trails.AddLast(trailForThisFilter);
					wasMatchFound = true;
				}

				if (FindFiltersNestedDownHierarchy(filter.SelectedFilters, matchCondition, trailForThisFilter, trails))
				{
					wasMatchFound = true;
				}
			}

			return wasMatchFound;
		}

		static bool HasSelfReference(IModuleFilterWithSelectedFilters filter, string layoutName, LinkedList<string> trail)
		{
			if (string.Equals(filter.Description, ModuleUserDefinedFilter.GetPrefixedDescription(layoutName), StringComparison.OrdinalIgnoreCase))
			{
				trail.AddFirst(filter.MultilingualDescription);
				return true;
			}

			var selectedFilters = filter.SelectedFilters;

			if (selectedFilters != null)
			{
				foreach (var subFilter in GetFiltersToCheckForSelfReference(selectedFilters))
				{
					if (HasSelfReference(subFilter, layoutName, trail))
					{
						trail.AddFirst(filter.MultilingualDescription);
						return true;
					}
				}
			}

			return false;
		}

		static IEnumerable<IModuleFilterWithSelectedFilters> GetFiltersToCheckForSelfReference(FilterStripBusinessObject filterBizo)
		{
			return filterBizo.ActiveModuleFilters.OfType<IModuleFilterWithSelectedFilters>().Where(x => x.IsFilterCollectionComparisonOperatorSelected());
		}

		static ModuleIdentifier GetModuleIdFromModuleName(string moduleName)
		{
			return string.IsNullOrEmpty(moduleName)
				? ModuleIDs.NotAssigned
				: ModuleIDs.AllIncludingClientModules.FirstOrDefault(x => string.Equals(x.Name, moduleName, StringComparison.OrdinalIgnoreCase));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL WHERE clause")]
		internal static StmModuleFilter[] GetLayoutsWithFilterDataContainingString(string textToSearchFor, BusinessObjectFactory factory)
		{
			var parameters = new ZSqlParameterCollection();
			var parameter = ZSqlParameter.New("@searchText", textToSearchFor, StmModuleFilterSchema.S9_FilterData, SQLComparisonOperator.Contains);
			parameters.Add(parameter);

			const string whereFormat = "dbo.CLRUncompressAsString({0}) LIKE @searchText ESCAPE '~'";
			var query = new ZDBOnlyQuery(typeof(StmModuleFilter));

			var stmModuleFilterSubQuery = new ZDBOnlySubQuery(typeof(StmModuleFilter), StmModuleFilterSchema.PK);
			stmModuleFilterSubQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, whereFormat, StmModuleFilterSchema.Constants.S9_FilterData), parameters);

			var userDataSubQuery = new ZDBOnlySubQuery(typeof(StmModuleFilterUserData), StmModuleFilterUserDataSchema.S0_S9);
			userDataSubQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, whereFormat, StmModuleFilterUserDataSchema.Constants.S0_FilterDataValues), parameters);
			userDataSubQuery.IncludeBlob(StmModuleFilterUserDataSchema.S0_FilterDataValues);

			stmModuleFilterSubQuery.AddAsUnionQuery(userDataSubQuery, addAsUnionAll: true);
			query.AddSubQuery(stmModuleFilterSubQuery, JoinCondition.And);

			return factory.Load<StmModuleFilter>(query);
		}

		static string GetChangeUserDefinedFilterMessage(string selectedFilterName, MultilingualString selectedFilterNameMultilingual, BusinessObjectFactory factory, LayoutAdjustmentMode mode, out NotificationTypes messageType)
		{
			var layouts = GetLayoutsWithFilterDataContainingString(ModuleUserDefinedFilter.GetPrefixedDescription(selectedFilterName), factory);
			var layoutsThatUseThis = new Dictionary<StmModuleFilter, string>();

			foreach (var layout in layouts.Where(x => x.S9_FilterName != selectedFilterName))
			{
				LinkedList<string> trail;

				if (FindFilterNestedDownHierarchy(layout, selectedFilterName, out trail))
				{
					var trailString = GetFullTrailString(trail);
					layoutsThatUseThis.Add(layout, trailString);
				}
			}

			if (layoutsThatUseThis.Any())
			{
				var sortedLayoutsThatUseThis = layoutsThatUseThis.OrderBy(x => x.Value).ToArray();
				var filterRuleLayouts = sortedLayoutsThatUseThis.Where(x => x.Key.S9_FilterType == StmModuleFilterTypes.Codes.FilterRule).ToArray();

				if (filterRuleLayouts.Any())
				{
					messageType = NotificationTypes.Error;
					var jobDescriptions = new List<string>();

					foreach (var layout in filterRuleLayouts.Select(x => x.Key))
					{
						var jobType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(layout.S9_ParentTableCode);
						var job = factory.Load(jobType, layout.S9_ParentID);
						jobDescriptions.Add(job.HumanReadableName);
					}

					if (filterRuleLayouts.Length == 1)
					{
						var jobDescription = jobDescriptions.Single();
						return GetSingleFilterRuleMessage(mode, selectedFilterNameMultilingual, jobDescription, filterRuleLayouts.Single().Value);
					}

					var trails = new List<string>();

					for (var i = 0; i < filterRuleLayouts.Length; i++)
					{
						trails.Add(string.Format(CultureInfo.InvariantCulture, "{0} -> {1}", jobDescriptions[i], filterRuleLayouts[i].Value));
					}

					return GetMultipleFilterRulesMessage(mode, selectedFilterNameMultilingual, trails);
				}

				messageType = NotificationTypes.Warning;

				if (sortedLayoutsThatUseThis.Length == 1)
				{
					var layoutAndTrail = sortedLayoutsThatUseThis.Single();
					var layoutName = layoutAndTrail.Key.S9_FilterNameMultilingual;
					var layoutTrail = GetTrailWithModuleDescription(layoutAndTrail.Value, layoutAndTrail.Key);

					return GetSingleLayoutMessage(mode, selectedFilterNameMultilingual, layoutName, layoutTrail);
				}
				else
				{
					var trails = string.Join(System.Environment.NewLine, sortedLayoutsThatUseThis.Select(x => GetTrailWithModuleDescription(x.Value, x.Key)));

					return GetMultipleLayoutsMessage(mode, selectedFilterNameMultilingual, trails);
				}
			}

			messageType = NotificationTypes.None;
			return string.Empty;
		}

		static string GetSingleFilterRuleMessage(LayoutAdjustmentMode mode, string filterName, string jobDescription, string trail)
		{
			return mode == LayoutAdjustmentMode.Delete
				? Res.GetString("9943d558-23fb-449b-a481-a49b5fd27702", @"The user-defined filter [{0}] cannot be deleted because it is included in the following filter rule:

{1} -> {2}", filterName, jobDescription, trail)
				: Res.GetString("0abe6fe2-1832-48f5-9700-ba2b6ec2573e", @"The user-defined filter [{0}] cannot be renamed because it is included in the following filter rule:

{1} -> {2}", filterName, jobDescription, trail);
		}

		static string GetMultipleFilterRulesMessage(LayoutAdjustmentMode mode, string filterName, List<string> trails)
		{
			var trailString = string.Join(System.Environment.NewLine, trails);

			return mode == LayoutAdjustmentMode.Delete
				? Res.GetString("5128613f-4ea0-4231-8bac-9ed027c4f0c6", @"The user-defined filter [{0}] cannot be deleted because it is included in the following filter rules:

{1}", filterName, trailString)
				: Res.GetString("98efc6c1-8255-45ab-a1b0-70e327cf0021", @"The user-defined filter [{0}] cannot be renamed because it is included in the following filter rules:

{1}", filterName, trailString);
		}

		static string GetSingleLayoutMessage(LayoutAdjustmentMode mode, string filterName, MultilingualString layoutName, string trail)
		{
			return mode == LayoutAdjustmentMode.Delete
				? Res.GetString("cebd5b09-affc-42a5-ad0f-1a34a262efb5", @"The user-defined filter [{0}] is included in the filter layout [{1}]:

{2}

Deleting [{0}] will remove it from that layout and may affect its query results. Are you sure you want to delete [{0}]?", filterName, layoutName, trail)
				: Res.GetString("81dba1f8-d397-4c0e-8922-9f58332af7e5", @"The user-defined filter [{0}] is included in the filter layout [{1}]:

{2}

Renaming [{0}] will remove it from that layout and may affect its query results. Are you sure you want to rename [{0}]?", filterName, layoutName, trail);
		}

		static string GetMultipleLayoutsMessage(LayoutAdjustmentMode mode, string filterName, string trails)
		{
			return mode == LayoutAdjustmentMode.Delete
				? Res.GetString("f55027f4-47e2-4aaa-a365-a7f30a2d1549", @"The user-defined filter [{0}] is included in the following filter layouts:

{1}

Deleting [{0}] will remove it from those layouts and may affect their query results. Are you sure you want to delete [{0}]?", filterName, trails)
				: Res.GetString("0739677f-0615-49b6-b518-ce4730d019d2", @"The user-defined filter [{0}] is included in the following filter layouts:

{1}

Renaming [{0}] will remove it from those layouts and may affect their query results. Are you sure you want to rename [{0}]?", filterName, trails);
		}

		internal static string GetTrailWithModuleDescription(string trail, StmModuleFilter layout)
		{
			var moduleId = GetModuleIdFromModuleName(layout.S9_ModuleID);

			return string.Format(CultureInfo.InvariantCulture, "{0} -> {1}", moduleId?.Description, trail);
		}

		static DialogResult ShowUserDefinedFilterInUseWarning(string message)
		{
			return Globals.Message.Show(message, Res.GetString("55ecc5ab-0d1e-44f9-ad25-1fae5b2a965f", "User-Defined Filter In Use"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
		}

		internal enum LayoutAdjustmentMode
		{
			Rename,
			Delete
		}
	}
}
