using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.BufferManagement.Module
{
	public class AcceptabilityBandFilterBusinessObject : FilterStripBusinessObject
	{
		#region FilterStripBusinessObject Overrides

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddBoolFilters(filters);
			AddNumberFilters(filters);
			AddTextFilters(filters);
			AddGuidFilters(filters);

			return filters;
		}

		#endregion

		#region Add Filters

		readonly MultilingualString FiltersByReleaseGroupDescription = ResString.GetMultilingualString("d4354df7-160b-4bea-bead-80c7bb1022e4", "Filters by Release Group");
		readonly MultilingualString FiltersByBoardSectionDescription = ResString.GetMultilingualString("61627c0d-9b83-4fcc-8d7d-7ac480eb1e1e", "Filters by Board Section");
		readonly MultilingualString TypeDescription = ResString.GetMultilingualString("BufferManagement|AcceptabilityBandFilterBusinessObject|Type", "Type");

		void AddBoolFilters(ModuleFilterCollection filters)
		{
			var releaseGroupFilter = filters.AddTextFilter("Filters by Release Group", GetReleaseGroupFilteringQuery, new ReleaseGroupFilteringOptions());
			releaseGroupFilter.MultilingualDescription = FiltersByReleaseGroupDescription;
			releaseGroupFilter.Category = FilterCategories.StatusAndFlags;

			var sectionGroupFilter = filters.AddTextFilter("Filters by Board Section", GetSectionFilteringQuery, new SectionFilteringOptions());
			sectionGroupFilter.MultilingualDescription = FiltersByBoardSectionDescription;
			sectionGroupFilter.Category = FilterCategories.StatusAndFlags;
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberRangeFilter("Minimum Value", BMComponentAcceptabilityBandSchema.BAB_CautionLowerBound).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|AcceptabilityBandFilterBusinessObject|MinimumValue", "Minimum Value");
			filters.AddNumberRangeFilter("Maximum Value", BMComponentAcceptabilityBandSchema.BAB_CautionUpperBound).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|AcceptabilityBandFilterBusinessObject|MaximumValue", "Maximum Value");
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Name", BMComponentAcceptabilityBandSchema.BAB_Name).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|AcceptabilityBandFilterBusinessObject|Name", "Name");
			filters.AddTextFilter("Type", BMComponentAcceptabilityBandSchema.BAB_Type, () => new AcceptabilityBandTypes()).MultilingualDescription = TypeDescription;
			filters.AddTextFilter("SQL Text", BMComponentAcceptabilityBandSchema.BAB_SqlText).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|AcceptabilityBandFilterBusinessObject|SqlText", "SQL Text");
		}

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Component", ModuleIDs.BMComponent, BMComponentAcceptabilityBandSchema.BAB_FC_Component, () => new BMComponentCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|AcceptabilityBandFilterBusinessObject|Component", "Component");
		}

		#endregion

		#region Queries

		static ZQuery GetReleaseGroupFilteringQuery(ZString option)
		{
			var query = new ZQuery();

			if (string.Equals(option, ReleaseGroupFilteringOptions.Codes.Enabled, StringComparison.OrdinalIgnoreCase))
			{
				query.AddToFilter(BMComponentAcceptabilityBandSchema.BAB_FiltersByReleaseGroup, true);
			}
			else if (string.Equals(option, ReleaseGroupFilteringOptions.Codes.Disabled, StringComparison.OrdinalIgnoreCase))
			{
				query.AddToFilter(BMComponentAcceptabilityBandSchema.BAB_FiltersByReleaseGroup, false);
			}

			return query;
		}

		static ZQuery GetSectionFilteringQuery(ZString option)
		{
			var query = new ZQuery();

			if (string.Equals(option, SectionFilteringOptions.Codes.Enabled, StringComparison.OrdinalIgnoreCase))
			{
				query.AddToFilter(BMComponentAcceptabilityBandSchema.BAB_FiltersBySection, true);
			}
			else if (string.Equals(option, SectionFilteringOptions.Codes.Disabled, StringComparison.OrdinalIgnoreCase))
			{
				query.AddToFilter(BMComponentAcceptabilityBandSchema.BAB_FiltersBySection, false);
			}

			return query;
		}

		#endregion

		#region Index Search Filters
		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case SearchFieldConstants.FiltersByReleaseGroup:
					return AddFiltersByReleaseGroupFilter(searchField);
				case SearchFieldConstants.FiltersByBoardSection:
					return AddFiltersByBoardSectionFilter(searchField);
				case SearchFieldConstants.Type:
					return AddTypeFilter(searchField);
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		SearchField AddFiltersByReleaseGroupFilter(SearchField searchField)
		{
			var filter = new IndexSearchModuleTextFilter("Filters by Release Group", searchField, GetFiltersByReleaseGroupQuery, new ReleaseGroupFilteringOptions(), FilterCategories.StatusAndFlags);
			filter.MultilingualDescription = FiltersByReleaseGroupDescription;
			return new SearchFieldOverride(searchField, filter);
		}

		IGlowQuery GetFiltersByReleaseGroupQuery(SearchField searchField, ZString status)
		{
			status = status.Trim();
			if (ReleaseGroupFilteringOptions.Codes.Enabled.Equals(status, StringComparison.OrdinalIgnoreCase))
			{
				return IndexSearchModuleFlagsFilter.GetGlowIndexQueryCore(searchField, true);
			}
			else if (ReleaseGroupFilteringOptions.Codes.Disabled.Equals(status, StringComparison.OrdinalIgnoreCase))
			{
				return IndexSearchModuleFlagsFilter.GetGlowIndexQueryCore(searchField, false);
			}
			return new EmptyQuery();
		}

		SearchField AddFiltersByBoardSectionFilter(SearchField searchField)
		{
			var filter = new IndexSearchModuleTextFilter("Filters by Board Section", searchField, GetFiltersByBoardSectionQuery, new SectionFilteringOptions(), FilterCategories.StatusAndFlags);
			filter.MultilingualDescription = FiltersByBoardSectionDescription;
			return new SearchFieldOverride(searchField, filter);
		}

		IGlowQuery GetFiltersByBoardSectionQuery(SearchField searchField, ZString status)
		{
			status = status.Trim();
			if (SectionFilteringOptions.Codes.Enabled.Equals(status, StringComparison.OrdinalIgnoreCase))
			{
				return IndexSearchModuleFlagsFilter.GetGlowIndexQueryCore(searchField, true);
			}
			else if (SectionFilteringOptions.Codes.Disabled.Equals(status, StringComparison.OrdinalIgnoreCase))
			{
				return IndexSearchModuleFlagsFilter.GetGlowIndexQueryCore(searchField, false);
			}
			return new EmptyQuery();
		}

		SearchField AddTypeFilter(SearchField searchField)
		{
			var filter = new IndexSearchModuleTextFilter("Type", searchField, GetTypeQuery, new AcceptabilityBandTypes(), FilterCategories.TextSearch);
			filter.MultilingualDescription = TypeDescription;
			return new SearchFieldOverride(searchField, filter);
		}

		IGlowQuery GetTypeQuery(SearchField searchField, ZString status)
		{
			return new EqualQuery(new Term(searchField.FieldName, status.Trim().ToUpperInvariant()));
		}

		static class SearchFieldConstants
		{
			public const string FiltersByReleaseGroup = IndexSearchFilterHelper.DefaultHiddenPrefix + "FILTERSBYRELEASEGROUP";
			public const string FiltersByBoardSection = IndexSearchFilterHelper.DefaultHiddenPrefix + "FILTERSBYBOARDSECTION";
			public const string Type = IndexSearchFilterHelper.DefaultHiddenPrefix + "TYPE";
		}

		#endregion
	}
}
