using System.Data;
using CargoWise.Database.Shared;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class UpdateWhsDocketLineFinalisedDateTimeToDateTimeOffset : ConvertDateTimeToDateTimeOffsetTransform
	{
		public override string UserDescription => "Update Whs Docket Line FinalisedDate Time from DateTime to DateTimeOffset.";

		protected override SchemaDateTimeOffsetColumn ColumnToConvert => WhsDocketLineSchema.WE_FinalisedDate;

		protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.DateTime;

		protected override bool PerformOffsetConversionAsTransaction => true;

		protected override string GetSubqueryForTimeZoneColumn(string timeZoneColumnName)
		{
			return Invariant($@"SELECT GB_RL_NKHomePort as {timeZoneColumnName}
	FROM
		dbo.WhsDocket
		JOIN dbo.WhsWarehouse ON WD_WW_Whs = WW_PK
		JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
	WHERE
		WE_WD = WD_PK");
		}

		protected override IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider)
		{
			return indexProvider.New(WhsDocketLineSchema.Instance)
				.Key(WhsDocketLineSchema.Constants.WE_FinalisedDate)
				.Include(WhsDocketLineSchema.Constants.WE_WD, WhsDocketLineSchema.Constants.WE_StockOnHand, WhsDocketLineSchema.Constants.WE_DocketLineStatus)
				.Where("[WE_FinalisedDate] IS NOT NULL")
				.GetInfo();
		}
	}
}
