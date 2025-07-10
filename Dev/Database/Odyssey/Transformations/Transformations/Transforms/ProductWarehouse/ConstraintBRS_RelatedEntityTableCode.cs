using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class ConstraintBRS_RelatedEntityTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update BarcodeRuleSet records with invalid BRS_RelatedEntityTableCode to 'OP'";

		public TransformationIndexProvider IndexProvider => BuildIndexProvider();
		TransformationIndexProvider BuildIndexProvider()
		{
			var indexInfo = IndexInfo.Builder
			.New(Db.SqlDbOwnerSchema, BarcodeRuleSetSchema.Constants.TableName, "IX_ConstraintBRS_RelatedEntityTableCode_Invalid_RelatedEntityTableCode")
			.Key(BarcodeRuleSetSchema.BRS_RelatedEntityTableCode.Name)
			.Include(BarcodeRuleSetSchema.BRS_SystemLastEditTimeUtc.Name, BarcodeRuleSetSchema.BRS_SystemLastEditUser.Name)
			.Where("[BRS_RelatedEntityTableCode]<>'' AND [BRS_RelatedEntityTableCode]<>'OP'")
			.GetInfo();

			return new TransformationIndexProvider(this) { indexInfo.Yield() };
		}

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(DeleteInvalidRecordsSQL);
		}

		const string DeleteInvalidRecordsSQL = @"
UPDATE 
	dbo.BarcodeRuleSet 
SET 
	BRS_RelatedEntityTableCode = 'OP',
	BRS_SystemLastEditUser = '~BP',
	BRS_SystemLastEditTimeUtc = GetUtcDate()
WHERE
	BRS_RelatedEntityTableCode NOT IN ('', 'OP')
";
	}
}
