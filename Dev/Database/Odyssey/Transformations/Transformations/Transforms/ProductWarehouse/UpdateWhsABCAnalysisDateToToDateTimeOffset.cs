using System.Data;
using CargoWise.Database.Shared;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class UpdateWhsABCAnalysisDateToToDateTimeOffset : ConvertDateTimeToDateTimeOffsetTransform
	{
		public override string UserDescription => "Update Whs ABC Category AnalysisDateTo from DateTime to DateTimeOffset.";

		protected override SchemaDateTimeOffsetColumn ColumnToConvert => WhsABCCategorySchema.WJ_AnalysisDateTo;

		protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.SmallDateTime;

		protected override string GetSubqueryForTimeZoneColumn(string timeZoneColumnName)
		{
			return Invariant($@"SELECT GB_RL_NKHomePort as {timeZoneColumnName}
	FROM
		dbo.WhsWarehouse
		JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
	WHERE
		WW_PK = WJ_WW_Warehouse");
		}

		protected override IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider)
		{
			return indexProvider.New(WhsABCCategorySchema.Instance)
				.Key(WhsABCCategorySchema.Constants.WJ_AnalysisDateTo)
				.Include(WhsABCCategorySchema.Constants.WJ_WW_Warehouse, "WJ_AutoVersion")
				.Where("[WJ_AnalysisDateTo] IS NOT NULL")
				.GetInfo();
		}
	}
}
