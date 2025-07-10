using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

using SubGroupRelationships = System.Collections.Generic.Dictionary<Enterprise.ZArchitecture.Business.BlueprintModuleFilterSubGroup, System.Collections.Generic.List<Enterprise.ZArchitecture.Business.BlueprintModuleFilterSubGroup>>;

namespace Enterprise.ZArchitecture.Business
{
	#region ISupportMultiValuesFilter

	public interface ISupportMultiValuesFilter
	{
		bool CanGroup { get; }
		/// <summary>
		/// Group key inside a set of same filters in same Or Category - no need to include filter code and Or Category.
		/// </summary>
		string GroupKey { get; }
		object GetCombinedValue(IEnumerable<ModuleFilter> filters);
		ZQuery GetCombinedQuery(object combinedValue);
	}

	#endregion

	/// <summary>
	/// Modify distinct queries by group in filter strips.
	/// </summary>
	public delegate void FilterGroupQueryMapAction(ZQuery query, IEnumerable<ModuleFilter> filtersInGroup);

	public class ModuleFilterCombiner
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public ZQuery GetCombinedTemplateFilter(IEnumerable<ModuleFilter> activeModuleFiltersForQuery)
		{
			var combinedQueries = new ZQuery();
			var filtersByGroupOrCategory = ExtractAndGroupFilterSets(activeModuleFiltersForQuery).GroupBy(s => s.Key.GroupOrCategory).ToArray();

			foreach (var filtersWithinGroupOrCategory in filtersByGroupOrCategory)
			{
				var filtersWithinGroupOrCategoryQueries = new ZQuery();

				foreach (var keyValuePair in filtersWithinGroupOrCategory)
				{
					var keyValuePairQueries = new ZQuery();

					foreach (var filtersByOrCategory in keyValuePair.Value)
					{
						var filtersByOrCategoryQueries = new ZQuery();

						foreach (var filter in filtersByOrCategory.Value)
						{
							var query = filter.XQuery.ShallowClone();
							filtersByOrCategoryQueries.AddToFilter(query, filtersByOrCategory.Key == FilterOrCategory.None ? JoinCondition.And : JoinCondition.Or);
						}

						keyValuePairQueries.AddToFilter(filtersByOrCategoryQueries);
					}

					filtersWithinGroupOrCategoryQueries.AddToFilter(keyValuePairQueries, keyValuePair.Key.GroupOrCategory == FilterOrCategory.None ? JoinCondition.And : JoinCondition.Or);
				}

				combinedQueries.AddToFilter(filtersWithinGroupOrCategoryQueries);
			}

			return combinedQueries;
		}

		Dictionary<GroupIdentifier, Dictionary<FilterOrCategory, List<ModuleFilter>>> ExtractAndGroupFilterSets(IEnumerable<ModuleFilter> activeModuleFiltersForQuery)
		{
			var result = new Dictionary<GroupIdentifier, Dictionary<FilterOrCategory, List<ModuleFilter>>>();

			foreach (var filter in activeModuleFiltersForQuery)
			{
				if (!filter.XQuery.IsEmpty)
				{
					var groupIdentifier = new GroupIdentifier { GroupName = filter.GroupName, GroupOrCategory = filter.GroupOrCategory };
					Dictionary<FilterOrCategory, List<ModuleFilter>> orCategoryLookup;
					if (!result.TryGetValue(groupIdentifier, out orCategoryLookup))
					{
						orCategoryLookup = new Dictionary<FilterOrCategory, List<ModuleFilter>>();
						result.Add(groupIdentifier, orCategoryLookup);
					}

					List<ModuleFilter> subGroupLookup;
					if (!orCategoryLookup.TryGetValue(filter.OrCategory, out subGroupLookup))
					{
						subGroupLookup = new List<ModuleFilter>();
						orCategoryLookup.Add(filter.OrCategory, subGroupLookup);
					}

					subGroupLookup.Add(filter);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZQuery GetCombinedFilter(IEnumerable<ModuleFilter> activeModuleFiltersForQuery, FilterGroupQueryMapAction forGroups = null, IEnumerable<BusinessObject> bizosToApplyFiltersTo = null)
		{
			/*
			 * Dear future readers of this code.
			 * 'Group OR Category' refers to the OR category (colour dropdown arrow) that can be specified when you use the Group option on module filter strips to group a set of filter strips in order to produce mixed AND/OR queries.
			 *		For example, find staff where ( GS_FullName like 'Benedict%' AND GS_IsActive = 1 ) OR ( GS_FullName like 'Martin%' AND GS_IsActive = 0 )
			 *		This isn't possible using standard filters and OR categories so Groups are required. Using a 'group OR category' makes the above statement have an OR between bracketed bits.
			 * 'OR Category' refers to the standard colour dropdown arrow that appears next to filter strips.
			 * The naming of these things is really problematic.
			 * 
			 */
			var combinedQuery = new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And);

			var colourToUnion = new Dictionary<FilterOrCategory, bool>();
			var nonUnionFilters = new List<ModuleFilter>();
			var queryHintFilters = new List<ModuleFlagsFilter>();
			var isUnion = false;
			foreach (var filter in activeModuleFiltersForQuery)
			{
				if (filter is ModuleUnionOrOrFilter unionOrOrFilter)
				{
					colourToUnion.Add(unionOrOrFilter.OrCategory, unionOrOrFilter.Property0);
					isUnion |= unionOrOrFilter.Property0;
				}
				else
				{
					if (filter is ModuleRecompileFilter or ModuleCardinalityFilter)
					{
						queryHintFilters.Add((ModuleFlagsFilter)filter);
					}

					nonUnionFilters.Add(filter);
				}
			}
			if (!colourToUnion.ContainsKey(FilterOrCategory.None))
			{
				colourToUnion.Add(FilterOrCategory.None, false);
			}

			var filtersByGroupOrCategory = ExtractAndGroupFilterSetsByOuterGroup(
				nonUnionFilters,
				forGroups,
				bizosToApplyFiltersTo).GroupBy(s => s.Key.GroupOrCategory).ToArray();

			foreach (var filtersWithinGroupOrCategory in filtersByGroupOrCategory)
			{
				var groupQuery = new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And);

				foreach (var keyValuePair in filtersWithinGroupOrCategory)
				{
					var filtersByOrCategory = keyValuePair.Value;
					FilterSubGroupLookup initialFilterSubGroupLookup;
					ZQuery query;

					if (filtersByOrCategory.TryGetValue(FilterOrCategory.None, out initialFilterSubGroupLookup))
					{
						filtersByOrCategory.Remove(FilterOrCategory.None);

						FilterGroup filterGroup;
						if (initialFilterSubGroupLookup.TryGetValue(ModuleFilterSubGroup.Default, out filterGroup))
						{
							initialFilterSubGroupLookup.Remove(ModuleFilterSubGroup.Default);
							query = filterGroup.Query.ShallowClone();
						}
						else
						{
							query = new ZQuery();
						}
					}
					else
					{
						initialFilterSubGroupLookup = new FilterSubGroupLookup();
						query = new ZQuery();
					}
					var queryBlueprintPart = new QueryBlueprintPart(query, keyValuePair.Key.GroupOrCategory,
						keyValuePair.Key.GroupOrCategory == FilterOrCategory.None ? JoinCondition.And : JoinCondition.Or);

					var categoriesToIgnore = GetCategoriesToIgnore(keyValuePair, filtersByOrCategory, query, forGroups);
					var subGroupLookupList = MergeCategorisedSubGroups(filtersByOrCategory, initialFilterSubGroupLookup, forGroups);

					queryBlueprintPart.Add(GenerateQueryFromFilterSets(subGroupLookupList, keyValuePair.Key.GroupName, FilterOrCategory.None, categoriesToIgnore)); //need to modify this

					groupQuery.Add(queryBlueprintPart);
				}

				combinedQuery.Add(groupQuery);
			}

			/*
			* 1) construct a QueryBlueprint, which is a tree of Queries, their children and whether they're AND or OR and their colour
			* 2) Deterministically mark the location of each branching point (such as with a series of indices) and how many branches it has.
			* 3) Modulo loop over all possible combinatronic combinations (with some sanity check to not be ridiculous, limit 100 e.g.) 
			* 4) and assemble queries from parts in those specific branches that we currently have the indices for
			* 5) and UNION them together and serve that
			*/

			if (!isUnion)
			{
				var query = combinedQuery.Construct();
				return queryHintFilters.Count == 0 ? query : AddQueryHintFiltersToQuery(queryHintFilters, query);
			}
			else
			{
				List<QueryBlueprintPart> orJunctions = combinedQuery.FindOrJunctions(colourToUnion);
				if (orJunctions.Count == 0)
				{
					//normal case
					return combinedQuery.Construct();
				}

				var unionQuery = new ZQuery();

				List<int> orCounts = new List<int>(orJunctions.Count);
				for (var i = 0; i < orJunctions.Count; ++i)
				{
					orCounts.Add(0);
				}

				int totalCount = 0;
				try
				{
					totalCount = orJunctions.Select(x => x.OrCount).Aggregate((x, y) => x * y);
				}
				catch (ArithmeticException) //integer overflow
				{
					return combinedQuery.Construct();
				}

				if (totalCount > 100) //TODO: someone who knows SQL pick this magic number please - how many unions do you think is too absurd?
				{
					//that's too many unions - abort!
					return combinedQuery.Construct();
				}

				for (var i = 0; i < totalCount; ++i)
				{
					//increment orCounts

					for (var j = 0; j < orCounts.Count; ++j)
					{
						orCounts[j] += 1;
						if (orCounts[j] >= orJunctions[j].OrCount)
						{
							orCounts[j] = 0;
						}
						else
						{
							break;
						}
					}

					//assign orCounts to orPaths
					for (var j = 0; j < orJunctions.Count; ++j)
					{
						var orJunction = orJunctions[j];
						orJunction.OrPath = orCounts[j];
					}

					//construct and union
					var unionPart = combinedQuery.Construct();
					unionQuery.AddToFilter(unionPart, JoinCondition.Union);
				}
				return queryHintFilters.Count == 0 ? unionQuery : AddQueryHintFiltersToQuery(queryHintFilters, unionQuery);
			}
		}

		static ZQuery AddQueryHintFiltersToQuery(IEnumerable<ModuleFlagsFilter> activeQueryHintFilters, ZQuery query)
		{
			foreach (var filter in activeQueryHintFilters)
			{
				if (filter.Property0)
				{
					if (filter is ModuleRecompileFilter)
					{
						query.QueryHints |= QueryHints.RECOMPILE;
					}
					else if (filter is ModuleCardinalityFilter)
					{
						query.QueryHints |= QueryHints.CARDINALITY;
					}
				}
			}
			return query;
		}

		static List<FilterOrCategory> GetCategoriesToIgnore(KeyValuePair<GroupIdentifier, FilterOrCategoryLookup> keyValuePair, FilterOrCategoryLookup filtersByOrCategory, ZQuery query, FilterGroupQueryMapAction forGroups)
		{
			var categoriesToIgnore = new List<FilterOrCategory>();

			foreach (var category in filtersByOrCategory)
			{
				var categoryLookup = filtersByOrCategory.Single(c => c.Key == category.Key);
				if (categoryLookup.Value.Count >= 2)
				{
					var preFilterOrCategoryLookup = new FilterOrCategoryLookup { { categoryLookup.Key, categoryLookup.Value } };
					var presubGroupList = MergeCategorisedSubGroups(preFilterOrCategoryLookup, new FilterSubGroupLookup(), forGroups);
					query.AddToFilter(GenerateQueryFromFilterSets(presubGroupList, keyValuePair.Key.GroupName, categoryLookup.Key, null).Construct());
					categoriesToIgnore.Add(categoryLookup.Key);
				}
			}

			foreach (var entry in categoriesToIgnore)
			{
				filtersByOrCategory.Remove(entry);
			}

			return categoriesToIgnore;
		}

		static Dictionary<GroupIdentifier, FilterOrCategoryLookup> ExtractAndGroupFilterSetsByOuterGroup(IEnumerable<ModuleFilter> activeModuleFiltersForQuery, FilterGroupQueryMapAction forGroups, IEnumerable<BusinessObject> bizosToApplyFiltersTo)
		{
			var result = new Dictionary<GroupIdentifier, FilterOrCategoryLookup>();

			foreach (var filter in activeModuleFiltersForQuery)
			{
				ZQuery filterQuery;

				if (bizosToApplyFiltersTo != null)
				{
					filterQuery = filter.GetQueryForParentBusinessObjects(bizosToApplyFiltersTo);
				}
				else
				{
					filterQuery = filter.Query;
				}

				if (!filterQuery.IsEmpty || filterQuery.IgnoreActiveFilter || filter.ForceProcessingGroup)
				{
					var groupIdentifier = new GroupIdentifier { GroupName = filter.GroupName, GroupOrCategory = filter.GroupOrCategory };
					FilterOrCategoryLookup orCategoryLookup;
					if (!result.TryGetValue(groupIdentifier, out orCategoryLookup))
					{
						orCategoryLookup = new FilterOrCategoryLookup();
						result.Add(groupIdentifier, orCategoryLookup);
					}

					FilterSubGroupLookup subGroupLookup;
					if (!orCategoryLookup.TryGetValue(filter.OrCategory, out subGroupLookup))
					{
						subGroupLookup = new FilterSubGroupLookup();
						orCategoryLookup.Add(filter.OrCategory, subGroupLookup);
					}

					var subGroup = filter.SubGroup ?? ModuleFilterSubGroup.Default;
					FilterGroup filterGroup;
					if (!subGroupLookup.TryGetValue(subGroup, out filterGroup))
					{
						filterGroup = new FilterGroup(forGroups);
						subGroupLookup.Add(subGroup, filterGroup);
					}

					filterGroup.AddFilter(filter);
				}
			}

			return result;
		}

		static FilterSubGroupLookup[] MergeCategorisedSubGroups(Dictionary<FilterOrCategory, FilterSubGroupLookup> categorisedFilterSubGroupLookups, FilterSubGroupLookup initialFilterSubGroupLookup, FilterGroupQueryMapAction forGroups)
		{
			FilterSubGroupLookup[] subGroupLookupList = { initialFilterSubGroupLookup };

			foreach (var categoryFilterSetPair in categorisedFilterSubGroupLookups)
			{
				if (categoryFilterSetPair.Value.Count != 0)
				{
					var filterGroupsToAdd = new List<KeyValuePair<BlueprintModuleFilterSubGroup, FilterGroup>>(categoryFilterSetPair.Value.Count);
					filterGroupsToAdd.AddRange(categoryFilterSetPair.Value);

					var newFilterSubGroupLookupList = (filterGroupsToAdd.Count == 1) ? subGroupLookupList : new FilterSubGroupLookup[subGroupLookupList.Length * filterGroupsToAdd.Count];

					for (var i = 0; i < subGroupLookupList.Length; i++)
					{
						for (var j = filterGroupsToAdd.Count - 1; j >= 0; j--)
						{
							var filterSubGroupLookup = (j == 0) ? subGroupLookupList[i] : subGroupLookupList[i].Clone();

							FilterGroup filterGroup;
							if (!filterSubGroupLookup.TryGetValue(filterGroupsToAdd[j].Key, out filterGroup))
							{
								filterGroup = new FilterGroup(forGroups);
								filterSubGroupLookup.Add(filterGroupsToAdd[j].Key, filterGroup);
							}
							filterGroup.Query.AddToFilter(filterGroupsToAdd[j].Value.Query, JoinCondition.And); //this needs to also futz with the QueryBlueprint
							filterGroup.QueryBlueprint.Add(filterGroupsToAdd[j].Value.QueryBlueprint); //like so?

							newFilterSubGroupLookupList[i + j * subGroupLookupList.Length] = filterSubGroupLookup;
						}
					}

					subGroupLookupList = newFilterSubGroupLookupList;
				}
			}

			return subGroupLookupList;
		}

		static QueryBlueprintPart GenerateQueryFromFilterSets(IEnumerable<FilterSubGroupLookup> filterSubGroupLookupList, string groupName, FilterOrCategory filterOrCategory, List<FilterOrCategory> ignoreCategory)
		{
			var query = new ZQuery();
			var result = new QueryBlueprintPart(query, filterOrCategory, JoinCondition.And); //And because it used to be ANDed in

			foreach (var section in filterSubGroupLookupList)
			{
				var sectionQuery = GenerateQueryFromFilterSubGroup(section, ExtractFilterSubGroupRelationships(section.Keys), ModuleFilterSubGroup.Default, groupName, filterOrCategory, ignoreCategory);
				result.Add(sectionQuery);
			}

			return result;
		}

		static QueryBlueprintPart GenerateQueryFromFilterSubGroup(FilterSubGroupLookup filterSubGroupLookup, SubGroupRelationships relationships, BlueprintModuleFilterSubGroup filterSubGroup, string groupName, FilterOrCategory filterOrCategory, List<FilterOrCategory> ignoreCategory)
		{
			var isBase = filterSubGroup == ModuleFilterSubGroup.Default;
			var query = new ZQuery();
			var result = new QueryBlueprintPart(query, filterOrCategory, isBase ? JoinCondition.Or : JoinCondition.And); //whichever one used to happen

			filterSubGroup.CurrentlyProcessedGroup = groupName;
			filterSubGroup.CurrentlyProcessedFilterOrCategory = filterOrCategory;
			filterSubGroup.CategoriesToIgnore = ignoreCategory;

			FilterGroup filterGroup;
			if (filterSubGroupLookup.TryGetValue(filterSubGroup, out filterGroup))
			{
				result.Add(filterGroup.QueryBlueprint);
			}

			List<BlueprintModuleFilterSubGroup> children;
			if (relationships.TryGetValue(filterSubGroup, out children))
			{
				foreach (var child in children)
				{
					result.Add(GenerateQueryFromFilterSubGroup(filterSubGroupLookup, relationships, child, groupName, filterOrCategory, ignoreCategory));
				}
			}

			if (!isBase)
			{
				result = filterSubGroup.GetSubQuery(result);
			}

			return result;
		}

		static SubGroupRelationships ExtractFilterSubGroupRelationships(IEnumerable<BlueprintModuleFilterSubGroup> subGroups)
		{
			var result = new SubGroupRelationships();
			result.Add(ModuleFilterSubGroup.Default, new List<BlueprintModuleFilterSubGroup>());

			foreach (var subGroup in subGroups)
			{
				var child = subGroup;

				while (child != null && child != ModuleFilterSubGroup.Default)
				{
					var parent = child.Parent ?? ModuleFilterSubGroup.Default;
					List<BlueprintModuleFilterSubGroup> list;

					if (!result.TryGetValue(parent, out list))
					{
						list = new List<BlueprintModuleFilterSubGroup>();
						list.Add(child);

						result.Add(parent, list);

						child = parent;
					}
					else
					{
						if (!list.Contains(child))
						{
							list.Add(child);
						}
						break;
					}
				}
			}

			return result;
		}

		#region FilterGroup

		sealed class FilterGroup
		{
			public FilterGroup(FilterGroupQueryMapAction forGroups)
			{
				this.forGroups = forGroups;
			}
			readonly FilterGroupQueryMapAction forGroups;

			public void AddFilter(ModuleFilter filter)
			{
				if (query != null)
				{
					throw new InvalidOperationException("Cannot add new filter to the group after existing filters have been combined into a query.");
				}

				if (filter is ISupportMultiValuesFilter multiValueFilter && CanGroupMultiColumnsFilter(multiValueFilter))
				{
					var identifier = new GroupIdentifier { GroupOrCategory = filter.OrCategory, GroupName = filter.GetType().Name + "|" + filter.OriginalCode + "|" + multiValueFilter.GroupKey };
					if (!filterValueGroups.TryGetValue(identifier, out var valuesGroup))
					{
						valuesGroup = new ValuesGroup();
						filterValueGroups.Add(identifier, valuesGroup);
					}
					valuesGroup.Filters.Add(filter);
				}
				else
				{
					var identifier = new GroupIdentifier { GroupOrCategory = filter.OrCategory, GroupName = filter.GetType().Name + "|" + filter.OriginalCode + "|" + filterValueGroups.Count };
					var group = new ValuesGroup();
					group.Filters.Add(filter);

					filterValueGroups.Add(identifier, group);
				}
			}

			bool CanGroupMultiColumnsFilter(ISupportMultiValuesFilter multiValueFilter)
			{
				var result = multiValueFilter.CanGroup;
				if (result && multiValueFilter is ModuleTextFilterForMultipleColumns filterForMultipleColumns && filterForMultipleColumns.AdditionalColumnsLength > 0)
				{
					result = false;
				}
				return result;
			}

			internal QueryBlueprintPart QueryBlueprint
			{
				get
				{
					if (queryBlueprint == null)
					{
						var result = new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And);
						if (filterValueGroups.Count == 1)
						{
							var child = new QueryBlueprintPart(filterValueGroups.First().Value.Query, FilterOrCategory.None, JoinCondition.And);
							result.Add(child);
						}
						else
						{
							foreach (var group in filterValueGroups)
							{
								var child = new QueryBlueprintPart(group.Value.Query, group.Key.GroupOrCategory, group.Key.GroupOrCategory == FilterOrCategory.None ? JoinCondition.And : JoinCondition.Or);
								result.Add(child);
							}
						}
						queryBlueprint = result;
					}
					return queryBlueprint;
				}
			}
			QueryBlueprintPart queryBlueprint;

			public ZQuery Query
			{
				get
				{
					if (query == null)
					{
						query = QueryBlueprint.Construct();
					}
					return query;
				}
			}
			ZQuery query;

			public FilterGroup Clone()
			{
				var result = new FilterGroup(forGroups);
				result.queryBlueprint = QueryBlueprint;
				result.query = Query.ShallowClone();
				return result;
			}

			#region ValuesGroup

			class ValuesGroup
			{
				public ZQuery Query
				{
					get
					{
						if (Filters.Count > 1)
						{
							var multiValueFilter = (ISupportMultiValuesFilter)Filters[0];
							var combinedValue = multiValueFilter.GetCombinedValue(Filters);
							return multiValueFilter.GetCombinedQuery(combinedValue);
						}
						else if (Filters.Count == 1)
						{
							return Filters[0].Query;
						}
						else
						{
							return new ZQuery();
						}
					}
				}

				public readonly List<ModuleFilter> Filters = new List<ModuleFilter>();
			}

			readonly Dictionary<GroupIdentifier, ValuesGroup> filterValueGroups = new Dictionary<GroupIdentifier, ValuesGroup>();

			#endregion
		}

		#endregion

		#region class FilterSubGroupLookup

		sealed class FilterSubGroupLookup : Dictionary<BlueprintModuleFilterSubGroup, FilterGroup>
		{
			public FilterSubGroupLookup Clone()
			{
				var result = new FilterSubGroupLookup();

				foreach (var pair in this)
				{
					result.Add(pair.Key, pair.Value.Clone());
				}

				return result;
			}
		}

		#endregion

		#region class FilterOrCategoryLookup

		sealed class FilterOrCategoryLookup : Dictionary<FilterOrCategory, FilterSubGroupLookup> { }

		#endregion

		#region struct GroupIdentifier

		struct GroupIdentifier
		{
			public string GroupName;
			public FilterOrCategory GroupOrCategory;
		}

		#endregion
	}
}
