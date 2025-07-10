using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public static class RelatedModuleFiltersHelper
	{
		public static ZQuery GetFilterQuery(StmModuleFilter filter, Action<IRelatedModuleFilterBusinessObject> actionToDoBeforeCreatingZQuery = null)
		{
			if (filter == null)
			{
				return ZQuery.NoResultQuery;
			}

			var filterBizo = GetNewFilterBusinessObject(filter);

			return filterBizo.GetFilter(filter, actionToDoBeforeCreatingZQuery: actionToDoBeforeCreatingZQuery);
		}

		public static ZQuery GetFilterQuerySafe(StmModuleFilter filter, Action<IRelatedModuleFilterBusinessObject> actionToDoBeforeCreatingZQuery = null)
		{
			return GetFilterQuery(filter, actionToDoBeforeCreatingZQuery) ?? ZQuery.NoResultQuery;
		}

		public static ZQuery GetFilterQuerySafeWhere(StmModuleFilter filter, Func<IModuleFilter, bool> filtersToIncludeFunc)
		{
			var filterBizo = GetNewFilterBusinessObject(filter);

			return filterBizo.GetFilterWhere(filter, filtersToIncludeFunc) ?? ZQuery.NoResultQuery;
		}

		public static StmModuleFilter LoadFilter(IRelatedModuleFilterSupportable parent, string filterName, bool reloadExistingRows = false)
		{
			var query = GetLoadQuery(parent, filterName, reloadExistingRows);

			return parent.Factory.LoadTop1<StmModuleFilter>(query);
		}

		public static StmModuleFilter[] LoadFilters(IEnumerable<ZGuid> parentPKs, string parentTableCode, BusinessObjectFactory factory)
		{
			var query = GetLoadQuery(parentPKs, parentTableCode, filterName: null, shouldFilterByName: false, reloadExistingRows: false, fetchOnlyFromLocalCache: false);

			return factory.Load<StmModuleFilter>(query);
		}

		public static StmModuleFilter[] LoadFilters(ZGuid parentPK, string parentTableCode, BusinessObjectFactory factory)
		{
			return LoadFilters(new[] { parentPK }, parentTableCode, factory);
		}

		internal static ZQuery GetLoadQuery(IRelatedModuleFilterSupportable parent, string filterName, bool reloadExistingRows)
		{
			return GetLoadQuery(parent.Identifier, parent.TablePrefix, filterName, shouldFilterByName: true, reloadExistingRows: reloadExistingRows, fetchOnlyFromLocalCache: !parent.IsInDatabase);
		}

		static ZQuery GetLoadQuery(object parentIds, string parentTableCode, string filterName, bool shouldFilterByName, bool reloadExistingRows, bool fetchOnlyFromLocalCache)
		{
			var query = new ZQuery(StmModuleFilterSchema.S9_ParentID, parentIds) { FetchOnlyFromLocalCache = fetchOnlyFromLocalCache, ReLoadExistingRows = reloadExistingRows };
			query.AddToFilter(StmModuleFilterSchema.S9_ParentTableCode, parentTableCode);
			query.AddToFilter(StmModuleFilterSchema.S9_FilterType, StmModuleFilterTypes.Codes.FilterRule);

			if (shouldFilterByName)
			{
				query.AddToFilter(StmModuleFilterSchema.S9_FilterName, filterName ?? string.Empty);
			}

			return query;
		}

		public static string GetFilterQueryParameterized(StmModuleFilter filter, ZSqlParameterCollection parameters)
		{
			var query = GetFilterQueryParameterized(filter, "@" + filter.PK.ToString().Replace("-", string.Empty), parameters);

			return query.IsNullOrEmpty() ? "1 = 2" : query;
		}

		static string GetFilterQueryParameterized(StmModuleFilter selectedFilters, string paramName, ZSqlParameterCollection parameters)
		{
			var query = GetFilterQuery(selectedFilters);
			var sql = query.ParameterisedText.ParameterisedQueryText;

			var i = 0;
			foreach (var parameter in query.Params.OrderByDescending(p => p.ParameterName))
			{
				var newName = paramName + (i++).ToString(CultureInfo.InvariantCulture);
				ZSqlParameterHelper.CreateParameterWithNewNameAndAddToCollection(parameter, parameters, newName);
				sql = sql.Replace(parameter.ParameterName, newName);
			}

			return sql;
		}

		internal static StmModuleFilter CreateFilter(IRelatedModuleFilterSupportable parent, string filterName, ModuleIdentifier moduleId)
		{
			var filter = parent.Factory.New<StmModuleFilter>();

			using (filter.SuspendSettingHasChanges())
			{
				filter.S9_ModuleID = moduleId.Name;
				filter.S9_FilterType = StmModuleFilterTypes.Codes.FilterRule;
				filter.S9_RelatedEntityID = ZGuid.Empty;
				filter.S9_ParentID = parent.Identifier;
				filter.S9_ParentTableCode = parent.TablePrefix;
				filter.S9_FilterName = filterName;
				filter.S9_IsPublished = true;
			}

			return filter;
		}

		internal static StmModuleFilter GetOrCreateFilter(IRelatedModuleFilterSupportable parent, string filterName, StmModuleFilter cachedFilter, ModuleIdentifier moduleId, bool reloadExistingRows = false)
		{
			var result = cachedFilter;

			if (result == null || result.IsDeleted)
			{
				result = LoadFilter(parent, filterName, reloadExistingRows) ?? CreateFilter(parent, filterName, moduleId);
				ConcurrencyInfo.SetConcurrencyPolicy(result, nameof(StmModuleFilter.S9_FilterData), ConcurrencyPolicy.Strict);

				((BusinessObject)parent).RegisterEditableChildObject(result);
			}

			return result;
		}

		public static void ValidateFilterStrips(StmModuleFilter filter, ModuleIdentifier moduleIdOverride = null)
		{
			var filterBizo = GetNewFilterBusinessObject(filter, moduleIdOverride);
			filterBizo.ValidateFilterStrips(filter);
		}

		public static bool HasFilterStrips(StmModuleFilter filter, ModuleIdentifier moduleId)
		{
			var filterBizo = GetNewFilterBusinessObject(moduleId);
			return filterBizo.GetFilterStripsCount(filter) > 0;
		}

		public static void CopyFilterStrips(StmModuleFilter fromFilter, StmModuleFilter toFilter)
		{
			toFilter.S9_FilterData = fromFilter.S9_FilterData;
			toFilter.S9_ColumnLayoutData = fromFilter.S9_ColumnLayoutData;
			var oldUserFilterData = fromFilter.GetOrCreateLayoutUserData(new EmptyLayoutsHelper());
			var userFilterData = toFilter.GetOrCreateLayoutUserData(new EmptyLayoutsHelper());
			userFilterData.S0_FilterDataValues = oldUserFilterData.S0_FilterDataValues;
		}

		public static IRelatedModuleFilterBusinessObject GetNewFilterBusinessObject(ModuleIdentifier moduleId, params object[] args)
		{
			return GetNewFilterBusinessObjectWithOverridenModuleId(moduleId, moduleId, args);
		}

		public static IRelatedModuleFilterBusinessObject GetNewFilterBusinessObjectWithOverridenModuleId(ModuleIdentifier moduleIdForFilterBusinessObjectCreation, ModuleIdentifier moduleIdForFilterBusinessObjectProperties, params object[] args)
		{
			var reference = GetFilterBizoObjectFactoryReference(moduleIdForFilterBusinessObjectCreation);
			var filterBizo = ObjectFactory.Get<IRelatedModuleFilterBusinessObject>(reference, args);
			filterBizo.IsInFilterRuleMode = true;

			var filterStripBusinessObject = (IFilterStripBusinessObject)filterBizo;
			if (string.IsNullOrEmpty(filterStripBusinessObject.LayoutContext))
			{
				filterStripBusinessObject.LayoutContext = moduleIdForFilterBusinessObjectCreation.Name;
			}

			using (var module = ObjectFactory.Get<IModuleFactory>().Create(moduleIdForFilterBusinessObjectProperties))
			{
				if (module != null)
				{
					var filterModule = module as IZFilterModule;
					if (filterModule != null)
					{
						filterModule.DoNotCheckOrSaveChanges = true;
						filterBizo.ModuleType = filterModule.GetType();
					}
				}
			}

			return filterBizo;
		}

		public static IRelatedModuleFilterBusinessObject GetNewFilterBusinessObject(StmModuleFilter filter, ModuleIdentifier moduleIdOverride = null)
		{
			var moduleIdForFilterBusinessObjectCreation = GetModuleId(filter);

			return filter.S9_IsIndexSearch
				? GetNewFilterBusinessObjectWithOverridenModuleId(moduleIdForFilterBusinessObjectCreation, moduleIdOverride ?? moduleIdForFilterBusinessObjectCreation, true)
				: GetNewFilterBusinessObjectWithOverridenModuleId(moduleIdForFilterBusinessObjectCreation, moduleIdOverride ?? moduleIdForFilterBusinessObjectCreation);
		}

		static string GetFilterBizoObjectFactoryReference(ModuleIdentifier moduleId)
		{
			return moduleId.Name + "_FilterStripBusinessObject";
		}

		static ModuleIdentifier GetModuleId(StmModuleFilter filter)
		{
			var moduleIds = string.IsNullOrEmpty(filter.S9_ModuleID) ? Array.Empty<ModuleIdentifier>() : ModuleIDs.AllIncludingClientModules.Where(x => string.Equals(x.Name, filter.S9_ModuleID, StringComparison.OrdinalIgnoreCase)).ToArray();

			if (moduleIds.Length != 1)
			{
				ErrorReporter.ReportOnce("RelatedModuleFiltersHelper.GetLoadedFilterStripBusinessObject",
					string.Format(CultureInfo.InvariantCulture, "Could not find just one module matching the name ({0}) in S9_ModuleID. You probably can't use this helper method with this module.", filter.S9_ModuleID));
			}

			return moduleIds.Single();
		}
	}
}
