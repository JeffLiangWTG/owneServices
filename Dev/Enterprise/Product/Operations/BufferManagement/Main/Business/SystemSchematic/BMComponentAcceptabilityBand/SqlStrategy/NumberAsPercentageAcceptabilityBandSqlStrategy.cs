using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class NumberAsPercentageAcceptabilityBandSqlStrategy : PercentageAcceptabilityBandSqlStrategy
	{
		internal NumberAsPercentageAcceptabilityBandSqlStrategy(BMComponentAcceptabilityBand band, AcceptabilityBandSqlBuilderParameters parameters)
			: base(band, parameters)
		{
		}

		protected override (string, ZSqlParameter[]) GetAggregationSqlCore()
		{
			var builder = new SqlBuilder();
			var (releaseGroupPredicateSql, releaseGroupPredicateParameters) = GetReleaseGroupPredicate();
			var valueSelectStatement = string.Format(CultureInfo.InvariantCulture,
				@"CONVERT(Decimal(38,2),
	100 *
	(
		SELECT CONVERT(Decimal(38,2), COUNT(*)) FROM Workflows
		WHERE
		{0}
	)
	/
	NULLIF(CONVERT(Decimal(38,2), COUNT(AllWorkflows.FH_PK)), 0)
)
", releaseGroupPredicateSql); // SQL strings do not need translation.

			releaseGroupPredicateParameters.ForEach(builder.ReserveParameter);
			var (sql, percentageParams) = AsPercentageSql(valueSelectStatement, ProcessHeaderSchema.PK.Name);
			builder.Append(sql, percentageParams);
			return (builder.GetSql(), builder.GetParameters());
		}
	}
}
