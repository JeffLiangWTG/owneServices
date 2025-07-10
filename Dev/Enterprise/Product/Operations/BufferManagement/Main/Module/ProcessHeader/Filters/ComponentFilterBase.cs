using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public abstract class ComponentFilterBase : ModuleGuidFilter
	{
		public ComponentFilterBase(ZString description, ModuleIdentifier id, SchemaGuidColumn schemaColumn, GetList listDelegate, MultilingualString multilingualDescription)
			: base(description, id, schemaColumn, listDelegate)
		{
			MultilingualDescription = multilingualDescription;
			SupportsFiltersMatchComparisonOperator = true;
			QueryDelegate = GetEmptyQuery(); // to make OR category combining possible
			MultiValueQueryDelegate = (values, sqlComparisonOperator) => GetQueryCore((List<ZGuid>)values, sqlComparisonOperator);
		}

		static GetGuidQuery GetEmptyQuery()
		{
			return value =>
			{
				throw new InvalidOperationException("This query should never actually be executed since GetQuery is overridden, and yet...");
			};
		}

		ZQuery GetQueryCore(List<ZGuid> properties, SQLComparisonOperator sqlComparisonOperator)
		{
			var isFiltersMatch = ComparisonOperator == ModuleTextFilter.ComparisonConstants.FiltersMatch;
			var isBlank = ComparisonOperator == ModuleTextFilter.ComparisonConstants.IsBlank;
			var isNotBlank = ComparisonOperator == ModuleTextFilter.ComparisonConstants.IsNotBlank;

			var allHeaders = new ZDBOnlyQuery(typeof(ProcessHeader));
			var jobHeaderQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);

			if (BMFilterStripsHelper.ShouldOptimiseQueryForWorkflowOnly(this))
			{
				if (isBlank)
				{
					allHeaders.AddToFilter(FilterColumn, SQLComparisonOperator.Equal, DBNull.Value);
				}
				else if (isNotBlank)
				{
					allHeaders.AddToFilter(FilterColumn, SQLComparisonOperator.NotEqual, DBNull.Value);
				}
				else if (isFiltersMatch)
				{
					allHeaders.AddToFilter(GetQueryForSelectedFilters());
				}
				else
				{
					allHeaders.AddToFilter(FilterColumn, sqlComparisonOperator, properties);
				}
			}
			else
			{
				var processHeaderSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.FH_FH_ParentHeader);

				if (isBlank)
				{
					processHeaderSubQuery.AddToFilter(FilterColumn, SQLComparisonOperator.Equal, DBNull.Value);
				}
				else if (isNotBlank)
				{
					processHeaderSubQuery.AddToFilter(FilterColumn, SQLComparisonOperator.NotEqual, DBNull.Value);
				}
				else if (isFiltersMatch)
				{
					processHeaderSubQuery.AddToFilter(GetQueryForSelectedFilters());
				}
				else
				{
					processHeaderSubQuery.AddToFilter(FilterColumn, sqlComparisonOperator, properties);
				}
				jobHeaderQuery.AddSubQuery(processHeaderSubQuery, JoinCondition.And);

				var processHeaderQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);

				if (isBlank)
				{
					processHeaderQuery.AddToFilter(FilterColumn, SQLComparisonOperator.Equal, DBNull.Value);
				}
				else if (isNotBlank)
				{
					processHeaderQuery.AddToFilter(FilterColumn, SQLComparisonOperator.NotEqual, DBNull.Value);
				}
				else if (isFiltersMatch)
				{
					processHeaderQuery.AddToFilter(GetQueryForSelectedFilters());
				}
				else
				{
					processHeaderQuery.AddToFilter(FilterColumn, sqlComparisonOperator, properties);
				}

				jobHeaderQuery.AddAsUnionQuery(processHeaderQuery, addAsUnionAll: true);
				allHeaders.AddSubQuery(jobHeaderQuery, JoinCondition.And);
			}

			return allHeaders;
		}

		protected override ZQuery GetQuery()
		{
			var properties = new List<ZGuid> { Property };
			return GetQueryCore(properties, SqlComparisonOperator);
		}

		protected override SchemaColumn SubQueryColumn => FilterColumn;
		public override bool HasComparisonOperator => true;
	}
}
