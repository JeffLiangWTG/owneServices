using System.Data;
using CargoWise.Database.Shared;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class UpdateWhsPickLinePickedDateToDateTimeOffset : ConvertDateTimeToDateTimeOffsetTransform
	{
		public override string UserDescription => "Update WhsPickLine PickedDate Time from DateTime to DateTimeOffset.";

		protected override SchemaDateTimeOffsetColumn ColumnToConvert => WhsPickLineSchema.WZ_PickedDateTime;

		protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.DateTime;

		protected override bool PerformOffsetConversionAsTransaction => true;

		protected override string GetSubqueryForTimeZoneColumn(string timeZoneColumnName)
		{
			return Invariant($@"SELECT GB_RL_NKHomePort as {timeZoneColumnName}
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
		JOIN dbo.WhsWarehouse ON WD_WW_Whs = WW_PK
		JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
	WHERE
		WZ_WE_TransactionLine = WE_PK");
		}

		protected override IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider)
		{
			return indexProvider.New(WhsPickLineSchema.Instance)
				.Key(WhsPickLineSchema.Constants.PK)
				.Include(WhsPickLineSchema.Constants.WZ_PickedDateTime, "WZ_AutoVersion")
				.GetInfo();
		}
	}
}
