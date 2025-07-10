using System.Data;
using CargoWise.Database.Shared;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class UpdateWhsDocketFinalisedDateTimeToDateTimeOffset : ConvertDateTimeToDateTimeOffsetTransform
	{
		public override string UserDescription => "Update Whs Docket FinalisedDate Time from DateTime to DateTimeOffset.";

		protected override SchemaDateTimeOffsetColumn ColumnToConvert => WhsDocketSchema.WD_FinalisedDate;

		protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.DateTime;

		protected override string GetSubqueryForTimeZoneColumn(string timeZoneColumnName)
		{
			return Invariant($@"SELECT GB_RL_NKHomePort as {timeZoneColumnName}
	FROM
		dbo.WhsWarehouse
		JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
	WHERE
		WD_WW_Whs = WW_PK");
		}

		protected override IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider) => null;
	}
}
