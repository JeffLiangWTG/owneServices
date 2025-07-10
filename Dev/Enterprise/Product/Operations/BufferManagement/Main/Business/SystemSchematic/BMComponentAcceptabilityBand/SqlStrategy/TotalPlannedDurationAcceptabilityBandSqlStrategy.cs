using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	class TotalPlannedDurationAcceptabilityBandSqlStrategy : FilterStripAcceptabilityBandSqlStrategy
	{
		internal TotalPlannedDurationAcceptabilityBandSqlStrategy(BMComponentAcceptabilityBand band, AcceptabilityBandSqlBuilderParameters parameters)
			: base(band, parameters)
		{
		}

		protected override (string, ZSqlParameter[]) GetAggregationSqlCore()
		{
			return (@"
	SELECT
		COALESCE(Value, 0) Value,
		FH_FC_CurrentComponent Component,
		FH_GG_ReleaseGroup ReleaseGroup
	FROM
	(
		SELECT
			SUM(FH_PlannedDurationInMinutes / 60.0) Value,
			FH_FC_CurrentComponent,
			FH_GG_ReleaseGroup
		FROM Workflows
		GROUP BY FH_FC_CurrentComponent, FH_GG_ReleaseGroup
	) headersCount
	", Array.Empty<ZSqlParameter>());
		}

		protected override string GetAdditionalWhereClause()
		{
			return @"
				FH_Status in ('OPN', 'BLK')
				"; // This is SQL
		}

		protected override bool ShouldConvertNullResultToZero => true;
	}
}
