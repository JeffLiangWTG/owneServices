using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public static class UserDefinedFilterHelperBusiness
	{
		public static StmModuleFilter[] GetLayoutsForUserDefinedFilters(BusinessObjectFactory factory, FilterStripLayoutsHelper layoutHelper, string moduleName, bool includePublishedOnly, SearchType? searchType = null)
		{
			var query = new StmModuleFilter.Loader(factory).GetQuery(moduleName, layoutHelper);
			query.AddToFilter(StmModuleFilterSchema.S9_FilterType, StmModuleFilterTypes.Codes.UserDefined);

			if (includePublishedOnly)
			{
				query.AddToFilter(StmModuleFilterSchema.S9_IsPublished, true);
			}

			if (searchType != null)
			{
				query.AddToFilter(StmModuleFilterSchema.S9_IsIndexSearch, searchType == SearchType.Index);
			}

			return factory.Load<StmModuleFilter>(query);
		}

		public static void CheckFilterBizoDoesNotContainNestedNonpublishedUserDefinedFilters(FilterStripBusinessObject filterBizo, Func<string, string> singleFilterErrorGetter, Func<string, string> multipleFilterErrorGetter, ZPropertyInfo propertyInfo, string rootFilterDescription = null)
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

		public static string GetFullTrailString(LinkedList<string> completedTrail)
		{
			return string.Join(" -> ", completedTrail);
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

		static IEnumerable<IModuleFilterWithSelectedFilters> GetFiltersToCheckForSelfReference(FilterStripBusinessObject filterBizo)
		{
			return filterBizo.ActiveModuleFilters.OfType<IModuleFilterWithSelectedFilters>().Where(x => x.IsFilterCollectionComparisonOperatorSelected());
		}
	}
}
