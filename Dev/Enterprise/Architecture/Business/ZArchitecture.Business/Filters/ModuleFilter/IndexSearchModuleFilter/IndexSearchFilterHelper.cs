using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using GlowIndexQueryService.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business
{
	public static class IndexSearchFilterHelper
	{
		public static IList<ModuleFilter> GetIndexSearchModuleFilterFromSearchField(SearchField searchField, BusinessObjectFactory factory = null)
		{
			if (searchField is SearchFieldOverride searchFieldOverride && !searchFieldOverride.IndexFiltersOverride.IsNullOrEmpty())
			{
				return searchFieldOverride.IndexFiltersOverride;
			}

			var category = TryGetOverriddenCategory(searchField);
			if (category == null && searchField.IsAuditField)
			{
				category = FilterCategories.AuditInformation;
			}

			var filterType = searchField.DataType;
			if (filterType == typeof(string))
			{
				if (searchField.HasListDelegate)
				{
					return [new IndexSearchModuleTextFilter(searchField, new GetList(searchField.GetListDelegate), category)];
				}
				else if (TryGetModuleIdAndCollection(searchField, factory, out var moduleIdNk, out var boListNk))
				{
					return [new IndexSearchModuleNKFilter(searchField, moduleIdNk, boListNk, category)];
				}
				else
				{
					return [new IndexSearchModuleTextFilter(searchField, filterCategory: category)];
				}
			}
			else if (filterType == typeof(int) || filterType == typeof(long) || filterType == typeof(short) || filterType == typeof(uint) || filterType == typeof(ulong) || filterType == typeof(ushort)
				|| filterType == typeof(double) || filterType == typeof(float) || filterType == typeof(decimal))
			{
				return [new IndexSearchModuleNumberRangeFilter(searchField, category)];
			}
			else if (filterType == typeof(bool))
			{
				return [new IndexSearchModuleFlagsFilter(searchField, category)];
			}
			else if (filterType == typeof(DateTime))
			{
				return [new IndexSearchModuleDateFilter(searchField, category)];
			}
			else if (filterType == typeof(Guid) && TryGetModuleIdAndCollection(searchField, factory, out var moduleId, out var bolist))
			{
				return [new IndexSearchModuleGuidFilter(searchField, moduleId, bolist, category)];
			}
			else if (filterType == typeof(DateTimeOffset))
			{
				return [new IndexSearchModuleDateTimeOffsetFilter(searchField)];
			}

			return [];
		}

		static FilterCategory TryGetOverriddenCategory(SearchField searchField)
		{
			if (searchField is SearchFieldOverride searchFieldOverride)
			{
				return searchFieldOverride.Category;
			}
			return null;
		}

		static bool TryGetModuleIdAndCollection(SearchField searchField, BusinessObjectFactory factory, out ModuleIdentifier moduleId, out IBusinessObjectCollection list)
		{
			if (searchField.HasLookup && factory != null)
			{
				var tableName = searchField.EntityLookup.TableName;
				moduleId = ObjectFactory.Get<IModuleFactory>().GetRegisteredIdentifierByTableName(tableName);
				try
				{
					list = ObjectFactory.Get<IBusinessObjectCollection>($"I{tableName}Collection", factory);
				}
				catch
				{
					list = null;
				}

				if (moduleId == null || list == null)
				{
					ErrorReporter.ReportOnce($"Failed to create index filter with lookup", $"Search Field Name: {searchField.FieldName}, tableName: {tableName}, Description: {searchField.Description}");
				}

				return moduleId != null && list != null;
			}

			moduleId = null;
			list = null;
			return false;
		}

		public static GlowIndexQueryResultCollection GetQueryResult(List<IGlowQuery> queries, string entityType)
		{
			Argument.NotNull(queries, nameof(queries));
			Argument.NotNullOrEmpty(entityType, nameof(entityType));

			if (queries.Count > 0)
			{
				var maxCount = ObjectFactory.Get<IGlowRegistry>().MaximumNumberOfModuleFiltersSearchResults;
				return GlowIndexQueryEngine.Query(new GlowIndexQueryParam(queries, entityType, maxQueryResults: maxCount, includeCount: true));
			}

			return null;
		}

		public static ZQuery ConvertResultToZQuery(GlowIndexQueryResultCollection result, string entityType)
		{
			if (result?.Status == GlowIndexQueryStatus.Success)
			{
				var tableCode = GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode(entityType);
				var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(tableCode);
				var pks = result.Results.Select(r => ZGuid.ParseSafe(r.PK));
				return new ZQuery(tableSchema.PK, pks);
			}
			return new ZQuery();
		}

		public static SearchFieldCollection GetSearchFields(ModuleIdentifier moduleIdentifier)
		{
			if (moduleIdentifier != null)
			{
				var entityType = GlowModuleToCW1ModuleConverter.ConvertModuleIdentifierToEntityType(moduleIdentifier);
				if (!string.IsNullOrEmpty(entityType))
				{
					return GlowIndexQueryEngine.GetSearchFields(entityType);
				}
			}

			return null;
		}

		internal static void AddCommonFilter(ModuleFilterCollection fModuleFilters)
		{
			var description = ResString.GetMultilingualString("5956c1ce-7e47-4c01-939d-9824a55648c1", "Quick Search");
			var commonSearchField = new SearchField(CommonField, description, typeof(string));
			var commonSearchFilter = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(commonSearchField)?.FirstOrDefault();
			if (commonSearchFilter != null)
			{
				commonSearchFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
				fModuleFilters.AddFilter(commonSearchFilter);
			}	
		}

		internal const string CommonField = $"Common";

		public const string DefaultHiddenPrefix = "CWDEFAULTHIDDEN";

		public static IGlowIndexQueryEngine GlowIndexQueryEngine
		{
			get { return glowIndexQueryEngine ??= ObjectFactory.Get<IGlowIndexQueryEngine>(); }
		}
		[ThreadSafe]
		static IGlowIndexQueryEngine glowIndexQueryEngine;

#if DEBUG
		public static void ResetIndexQueryEngine()
		{
			glowIndexQueryEngine = null;
		}
#endif
	}
}
