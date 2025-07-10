using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Business
{
	public class IndexSearchModuleNumberRangeFilter : ModuleNumberRangeFilter, IIndexSearchModuleFilter
	{
		public IndexSearchModuleNumberRangeFilter(SearchField searchField, FilterCategory filterCategory = null)
			: base(searchField.FieldName, (v1, v2) => new ZQuery())
		{
			MultilingualDescription = (NoResString)searchField.Description;
			SearchField = searchField ?? throw new ArgumentNullException(nameof(searchField));
			Decimals = (byte)searchField.Scale;
			if (filterCategory != null)
			{
				Category = filterCategory;
			}
		}

		SearchField SearchField { get; }

		public IGlowQuery GetGlowIndexQuery() =>
			GetGlowIndexQueryCore(Property1, Property2, Property1 == Property2, Property1 > MinValue, Property2 < MaxValue);

		public IGlowQuery GetGlowIndexQueryCore<NumberType>(NumberType value1, NumberType value2, bool equal, bool useValue1, bool useValue2)
		{
			var value1Term = new Term(SearchField.FieldName, value1.ToString());
			var value2Term = new Term(SearchField.FieldName, value2.ToString());
			if (equal)
			{
				return new EqualQuery(value1Term, useQuotes: false);
			}
			else
			{
				return new RangeQuery(useValue1 ? value1Term : null, useValue2 ? value2Term : null);
			}
		}
	}
}
