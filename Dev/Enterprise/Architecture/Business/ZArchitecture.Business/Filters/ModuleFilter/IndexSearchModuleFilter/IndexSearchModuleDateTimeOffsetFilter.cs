using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Business
{
	public class IndexSearchModuleDateTimeOffsetFilter : ModuleDateTimeOffsetFilter, IIndexSearchModuleFilter
	{
		public IndexSearchModuleDateTimeOffsetFilter(SearchField searchField)
			: base(searchField.FieldName, convertFromLocalToUTC: searchField?.IsUtcTime ?? false)
		{
			MultilingualDescription = (NoResString)searchField.Description;
			SearchField = searchField;
		}

		public SearchField SearchField { get; }

		public IGlowQuery GetGlowIndexQuery()
		{
			var begin = new Term(SearchField.FieldName, FromDate.IsValid ? FromDate.ToISO8601String() + "Z" : null);
			var end = new Term(SearchField.FieldName, ToDate.IsValid ? ToDate.ToISO8601String() + "Z" : null);
			return new RangeQuery(begin, end, includeBegin: true, includeEnd: false);
		}
	}
}
