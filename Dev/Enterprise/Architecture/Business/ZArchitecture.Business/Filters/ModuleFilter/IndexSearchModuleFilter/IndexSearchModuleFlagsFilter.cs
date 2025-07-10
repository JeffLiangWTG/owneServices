using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Business
{
	public class IndexSearchModuleFlagsFilter : ModuleFlagsFilter, IIndexSearchModuleFilter
	{
		public IndexSearchModuleFlagsFilter(SearchField searchField, FilterCategory filterCategory = null)
			: base(searchField.FieldName, new string[] { searchField.Description }, new GetFlagsQuery[] { delegate { return new ZQuery(); } })
		{
			MultilingualDescription = (NoResString)searchField.Description;
			SearchField = searchField ?? throw new ArgumentNullException(nameof(searchField));
			if (filterCategory != null)
			{
				Category = filterCategory;
			}
		}

		SearchField SearchField { get; }

		public IGlowQuery GetGlowIndexQuery() => GetGlowIndexQueryCore(SearchField, Property0);

		public static IGlowQuery GetGlowIndexQueryCore(SearchField searchField, ZBool property)
		{
			if (property)
			{
				var trueExpression = new EqualQuery(new Term(searchField.FieldName, $"true"), useQuotes: false);
				var nullExpression = new EqualQuery(new Term(searchField.FieldName, $"null"), useQuotes: false);
				return new BooleanQuery(BooleanOperator.Or, trueExpression, nullExpression);
			}
			else
			{
				return new EqualQuery(new Term(searchField.FieldName, $"false"), useQuotes: false);
			}
		}
	}
}
