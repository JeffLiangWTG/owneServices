using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ShownOnDiagramFilter : ModuleGuidFilter
	{
		public ShownOnDiagramFilter(ZString description, ModuleIdentifier id, GetList listDelegate)
			: base(description, id, EmptyQuery, listDelegate)
		{
			this.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|ShownOnDiagram", "Shown on Network Diagram");
			this.SupportsFiltersMatchComparisonOperator = true;
		}

		static ZQuery EmptyQuery(ZGuid pk)
		{
			throw new InvalidOperationException();
		}

		public override bool HasComparisonOperator => true;

		protected override ZQuery GetQuery()
		{
			var isFiltersMatch = ComparisonOperator == ModuleTextFilter.ComparisonConstants.FiltersMatch;
			var isBlankOrNotBlank = ComparisonOperator == ModuleTextFilter.ComparisonConstants.IsBlank || ComparisonOperator == ModuleTextFilter.ComparisonConstants.IsNotBlank;

			if (Property.IsEmpty && !isFiltersMatch && !isBlankOrNotBlank)
			{
				return new ZQuery();
			}

			return GetQueryCore(isFiltersMatch, isBlankOrNotBlank);
		}

		ZQuery GetQueryCore(bool isFiltersMatch, bool isBlankOrNotBlank)
		{
			if (isFiltersMatch)
			{
				return GetFilterMatchQuery();
			}
			else if (ComparisonOperator == ModuleTextFilter.ComparisonConstants.Exact ||
				ComparisonOperator == ModuleTextFilter.ComparisonConstants.NotEqual)
			{
				return GetExactNotEqualQuery();
			}
			else if (isBlankOrNotBlank)
			{
				return GetBlankOrNotBlankQuery();
			}

			return new ZQuery();
		}

		ZQuery GetFilterMatchQuery()
		{
			var parameters = new ZSqlParameterCollection();
			var sqlPart = BMFilterStripsHelper.GetFiltersMatchSql(this, "@Diag", parameters);

			var sql = string.Format(Culture.Invariant, BaseQuery, sqlPart.Item1 + sqlPart.Item2);

			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			query.AddFilterAndZSQLParameterCollection(sql, parameters, JoinCondition.And);
			return query;
		}

		const string BaseQuery = @"
		FH_PK IN
		(
			SELECT BNS_RelatedEntityID
			FROM dbo.BMNCNShape
			CROSS APPLY GetShapeAscendingHierarchy(BNS_PK)
			WHERE Parent IN
			(
				SELECT BNS_PK FROM dbo.BMNCNShape
				{0}
			)
		)";// SQL constant

		ZQuery GetExactNotEqualQuery()
		{
			var isIn = ComparisonOperator == ModuleTextFilter.ComparisonConstants.Exact;

			var query = new ZDBOnlyQuery(typeof(ProcessHeader));

			var rootShapeQuery = new ZDBOnlySubQuery(typeof(BMNCNShape), BMNCNShapeSchema.BNS_RelatedEntityID, !isIn);
			rootShapeQuery.AddToFilter(BMNCNShapeSchema.PK, Property);
			query.AddSubQuery(rootShapeQuery, JoinCondition.And);

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@RootShapePK", Property, BMNCNShapeSchema.PK);

			string baseQuery = @"
				FH_PK {0}
				(
						SELECT ProcessHeaderPK
						FROM GetShapeDescendentHierarchy(@RootShapePK)
						WHERE ProcessHeaderPK IS NOT NULL
				)";// SQL constant

			var sql = string.Format(Culture.Invariant, baseQuery, isIn ? "IN" : (NoResString)"NOT IN");// SQL constant

			query.AddFilterAndZSQLParameterCollection(sql, parameters, (isIn ? JoinCondition.Or : JoinCondition.And));

			return query;
		}

		ZQuery GetBlankOrNotBlankQuery()
		{
			var notIn = ComparisonOperator == ModuleTextFilter.ComparisonConstants.IsBlank;

			var query = new ZDBOnlyQuery(typeof(ProcessHeader));

			var rootShapeQuery = new ZDBOnlySubQuery(typeof(BMNCNShape), BMNCNShapeSchema.BNS_RelatedEntityID, notIn);
			query.AddSubQuery(rootShapeQuery, JoinCondition.And);

			return query;
		}

		protected override SchemaColumn SubQueryColumn => BMNCNShapeSchema.BNS_RelatedEntityID;
	}
}
