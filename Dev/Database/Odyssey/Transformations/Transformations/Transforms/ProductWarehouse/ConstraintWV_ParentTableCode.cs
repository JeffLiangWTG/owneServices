using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class ConstraintWV_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Delete WhsDocketJobPivot records with invalid WV_ParentTableCode";
		public TransformationIndexProvider IndexProvider => BuildIndexProvider();

		protected override void OfflinePostUpgradeTransform()
		{
			base.OfflinePostUpgradeTransform();
			using (var command = Db.Connection.Command(DeleteInvalidRecordsSQL))
			{
				command.ExecuteNonQuery();
			}
		}

		public TransformationIndexProvider BuildIndexProvider()
		{
			var indexInfo = IndexInfo.Builder.New(Db.SqlDbOwnerSchema, "WhsDocketJobPivot", "IX_ConstraintWV_ParentTableCode_Invalid_ParentTableCode")
				.Key(WhsDocketJobPivotSchema.WV_ParentTableCode.Name)
				.Include(WhsDocketJobPivotSchema.WV_SystemCreateTimeUtc.Name, WhsDocketJobPivotSchema.WV_SystemLastEditTimeUtc.Name)
				.Where("[WV_ParentTableCode]<>'BH' AND [WV_ParentTableCode]<>'CH' AND [WV_ParentTableCode]<>'JD' AND [WV_ParentTableCode]<>'JE' AND [WV_ParentTableCode]<>'JS'")
				.GetInfo();

			return new TransformationIndexProvider(this) { indexInfo.Yield() };
		}

		const string DeleteInvalidRecordsSQL = @"
DELETE
	WhsDocketJobPivot
WHERE
	WV_ParentTableCode NOT IN ('BH', 'CH', 'JD', 'JE', 'JS')
";
	}
}
