using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	class SqlAcceptabilityBandSqlStrategy : AcceptabilityBandSqlStrategyBase
	{
		internal SqlAcceptabilityBandSqlStrategy(BMComponentAcceptabilityBand band, AcceptabilityBandSqlBuilderParameters parameters)
			: base(band, parameters)
		{
		}

		protected override (string, ZSqlParameter[]) GetApplicableFilterSql() => (ProcessHeader.GetBaseSqlQuery(GetSectionWorkflowFilterClause(addAndIfRequired: false)), Array.Empty<ZSqlParameter>());

		protected override (string, ZSqlParameter[]) GetAggregationSqlCore() => (Band.BAB_SqlText, Array.Empty<ZSqlParameter>());

		protected override string GetWorkflowAndSectionWorkflowCTE(string filterSql, bool nullQuery, ZSqlParameter workflowPKs = null) => null;
	}
}
