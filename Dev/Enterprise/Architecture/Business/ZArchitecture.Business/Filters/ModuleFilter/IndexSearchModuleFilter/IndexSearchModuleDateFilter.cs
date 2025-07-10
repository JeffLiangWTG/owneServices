using System;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Business
{
	public class IndexSearchModuleDateFilter : ModuleDateFilter, IIndexSearchModuleFilter
	{
		public IndexSearchModuleDateFilter(SearchField searchField, FilterCategory filterCategory = null)
			: base(searchField.FieldName, convertFromLocalToUTC: searchField.IsUtcTime)
		{
			MultilingualDescription = (NoResString)searchField.Description;
			SearchField = searchField ?? throw new ArgumentNullException(nameof(searchField));
			if (filterCategory != null)
			{
				Category = filterCategory;
			}
		}

		SearchField SearchField { get; }

		public IGlowQuery GetGlowIndexQuery()
		{
			var begin = new Term(SearchField.FieldName, FromDate.IsValid ? FromDate.ToISO8601String() + "Z" : null);
			var end = new Term(SearchField.FieldName, ToDate.IsValid ? ToDate.ToISO8601String() + "Z" : null);

			if (IsPropertySearchUsingHasDateEntered)
			{
				return new IsNotBlankQuery(begin, includeEmptyString: false);
			}

			if(IsPropertySearchUsingHasNoDateEntered)
			{
				return new IsBlankQuery(begin, includeEmptyString: false);
			}
			return new RangeQuery(begin, end, includeBegin: true, includeEnd: false);
		}
	}
}
