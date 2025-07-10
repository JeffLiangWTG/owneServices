using System.Collections.Generic;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Business
{
	public class SearchFieldOverride : SearchField
	{
		public SearchFieldOverride(SearchField searchField, ModuleFilter indexFilterOverride) : this(searchField, new List<ModuleFilter> { indexFilterOverride })
		{
		}

		public SearchFieldOverride(SearchField searchField, IList<ModuleFilter> indexFiltersOverride) : this(searchField)
		{
			IndexFiltersOverride = indexFiltersOverride;
		}

		public SearchFieldOverride(SearchField searchField, FilterCategory category) : this(searchField)
		{
			Category = category;
		}

		SearchFieldOverride(SearchField searchField) : base(searchField.FieldName, searchField.Description, searchField.DataType, searchField.UIHidden, searchField.IsUtcTime, searchField.Scale)
		{
			_entityLookup = searchField.EntityLookup;
			_ruleLookup = searchField.RuleLookup;
		}

		public FilterCategory Category { get; }
		public IList<ModuleFilter> IndexFiltersOverride { get; }
	}
}
