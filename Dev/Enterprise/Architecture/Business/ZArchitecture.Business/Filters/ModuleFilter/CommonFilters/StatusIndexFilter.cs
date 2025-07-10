using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Business
{
	public class StatusIndexFilter : IndexSearchModuleTextFilter
	{
		public StatusIndexFilter(SearchField searchField, CodeDescriptionPairList list)
			: base(searchField, list)
		{
		}

		public override bool HasComparisonOperator => false;

		public override IGlowQuery GetGlowIndexQuery()
		{
			switch (Property)
			{
				case "NCM":
					return new BooleanQuery(BooleanOperator.Or, new[]
					{
						new EqualQuery(new Term(SearchFieldName, "OPN")),
						new EqualQuery(new Term(SearchFieldName, "ASN")),
						new EqualQuery(new Term(SearchFieldName, "WRK")),
						new EqualQuery(new Term(SearchFieldName, "SUS")),
					});
				case "A+S":
					return new BooleanQuery(BooleanOperator.Or, new []
					{
						new EqualQuery(new Term(SearchFieldName, "ASN")),
						new EqualQuery(new Term(SearchFieldName, "SUS")),
					});
				case "W+S":
					return new BooleanQuery(BooleanOperator.Or, new[]
					{
						new EqualQuery(new Term(SearchFieldName, "WRK")),
						new EqualQuery(new Term(SearchFieldName, "SUS")),
					});
				default:
					return base.GetGlowIndexQuery();
			}
		}
	}
}
