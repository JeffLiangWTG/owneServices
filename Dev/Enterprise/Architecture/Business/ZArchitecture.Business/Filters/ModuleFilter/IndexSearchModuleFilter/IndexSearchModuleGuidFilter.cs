using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Business
{
	public class IndexSearchModuleGuidFilter : ModuleGuidFilter, IIndexSearchModuleFilter
	{
		public IndexSearchModuleGuidFilter(SearchField searchField, ModuleIdentifier moduleId, IBusinessObjectCollection list, FilterCategory category = null) : base(searchField.Description, moduleId, (guid) => new ZQuery(), list)
		{
			SupportsFiltersMatchComparisonOperator = false;
			SearchField = searchField ?? throw new ArgumentNullException(nameof(searchField));
			((IModuleFilterForStrategyInternal)this).Description = searchField.FieldName;
			MultilingualDescription = (NoResString)searchField.Description;
			if (category != null)
			{
				Category = category;
			}
		}

		SearchField SearchField { get; }

		public override bool HasComparisonOperator => true;

		public IGlowQuery GetGlowIndexQuery()
		{
			if (ComparisonOperator != ComparisonConstants.IsBlank
				&& ComparisonOperator != ComparisonConstants.IsNotBlank
				&& !Property.IsValid)
			{
				return new EmptyQuery();
			}
			var term = new Term(SearchField.FieldName, Property.ToString());
			return (string)ComparisonOperator switch
			{
				ComparisonConstants.Exact => new EqualQuery(term, useQuotes: false),
				ComparisonConstants.NotEqual => new NotEqualQuery(term, useQuotes: false),
				ComparisonConstants.IsBlank => new IsBlankQuery(term, false),
				ComparisonConstants.IsNotBlank => new IsNotBlankQuery(term, false),
				_ => new EmptyQuery(),
			};
		}
	}
}
