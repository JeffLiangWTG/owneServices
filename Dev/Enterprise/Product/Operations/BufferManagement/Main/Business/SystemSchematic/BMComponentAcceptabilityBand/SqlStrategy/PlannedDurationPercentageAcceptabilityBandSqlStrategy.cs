using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class PlannedDurationPercentageAcceptabilityBandSqlStrategy : PercentageAcceptabilityBandSqlStrategy
	{
		internal PlannedDurationPercentageAcceptabilityBandSqlStrategy(BMComponentAcceptabilityBand band, AcceptabilityBandSqlBuilderParameters parameters)
			: base(band, parameters)
		{
		}

		protected override (string, ZSqlParameter[]) GetAggregationSqlCore()
		{
			var (releaseGroupPredicateSql, releaseGroupPredicateParams) = GetReleaseGroupPredicate();
			var valueSelectStatement = string.Format(CultureInfo.InvariantCulture,
				@"CONVERT(decimal(38,2),
	100 *
	(
		SELECT CONVERT(DECIMAL(38,2), SUM(FH_PlannedDurationInMinutes)) FROM {0}
		WHERE
		{1}
	)
	/
	NULLIF(CONVERT(Decimal(38,2), SUM(AllWorkflows.FH_PlannedDurationInMinutes)), 0)
)
", BMComponentAcceptabilityBand.FilteredWorkflowsResultSetName, releaseGroupPredicateSql); // SQL strings do not need translation.

			var (percentageSql, percentageParams) = AsPercentageSql(valueSelectStatement, ProcessHeaderSchema.FH_PlannedDurationInMinutes.Name);
			return (percentageSql, percentageParams.Concat(releaseGroupPredicateParams).ToArray());
		}
	}
}
