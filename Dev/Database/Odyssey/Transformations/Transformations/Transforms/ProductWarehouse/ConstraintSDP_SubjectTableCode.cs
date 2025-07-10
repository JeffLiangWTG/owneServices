using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class ConstraintSDP_SubjectTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Delete StmDefaultPrinter records with invalid SDP_SubjectTableCode";

		public TransformationIndexProvider IndexProvider => BuildIndexProvider();
		TransformationIndexProvider BuildIndexProvider()
		{
			var indexInfo = IndexInfo.Builder
			.New(Db.SqlDbOwnerSchema, StmDefaultPrinterSchema.Constants.TableName, "IX_ConstraintSDP_SubjectTableCode_Invalid_SubjectTableCode")
			.Key(StmDefaultPrinterSchema.SDP_SubjectTableCode.Name)
			.Include(StmDefaultPrinterSchema.SDP_SystemCreateTimeUtc.Name, StmDefaultPrinterSchema.SDP_SystemLastEditTimeUtc.Name)
			.Where("[SDP_SubjectTableCode]<>'GS' AND [SDP_SubjectTableCode]<>'KP' AND [SDP_SubjectTableCode]<>'WA' AND [SDP_SubjectTableCode]<>'WW'")
			.GetInfo();

			return new TransformationIndexProvider(this) { indexInfo.Yield() };
		}

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(DeleteInvalidRecordsSQL);
		}

		const string DeleteInvalidRecordsSQL = @"
DELETE
	dbo.StmDefaultPrinter
WHERE
	SDP_SubjectTableCode NOT IN ('GS', 'KP', 'WA', 'WW')
";
	}
}
