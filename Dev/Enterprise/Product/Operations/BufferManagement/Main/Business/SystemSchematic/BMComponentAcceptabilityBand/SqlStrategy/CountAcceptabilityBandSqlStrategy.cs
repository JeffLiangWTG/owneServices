using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	class CountAcceptabilityBandSqlStrategy : FilterStripAcceptabilityBandSqlStrategy
	{
		internal CountAcceptabilityBandSqlStrategy(BMComponentAcceptabilityBand band, AcceptabilityBandSqlBuilderParameters parameters)
			: base(band, parameters)
		{
		}

		protected override (string, ZSqlParameter[]) GetAggregationSqlCore()
		{
			return (@"
	SELECT COALESCE(Value, 0) Value, FH_FC_CurrentComponent Component, FH_GG_ReleaseGroup ReleaseGroup 
	FROM (
		SELECT COUNT(*) Value, FH_FC_CurrentComponent, FH_GG_ReleaseGroup 
		FROM Workflows
		GROUP BY FH_FC_CurrentComponent, FH_GG_ReleaseGroup
	) headersCount
	", Array.Empty<ZSqlParameter>());
		}

		protected override bool ShouldConvertNullResultToZero => true;
	}
}
