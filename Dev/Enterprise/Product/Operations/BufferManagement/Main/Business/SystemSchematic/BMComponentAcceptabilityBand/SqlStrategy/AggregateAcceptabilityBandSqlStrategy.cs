using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	class AggregateAcceptabilityBandSqlStrategy : FilterStripAcceptabilityBandSqlStrategy
	{
		internal AggregateAcceptabilityBandSqlStrategy(BMComponentAcceptabilityBand band, AcceptabilityBandSqlBuilderParameters parameters)
			: base(band, parameters)
		{
		}

		protected override (string, ZSqlParameter[]) GetAggregationSqlCore()
		{
			return (Band.BAB_SqlText, Array.Empty<ZSqlParameter>());
		}
	}
}
