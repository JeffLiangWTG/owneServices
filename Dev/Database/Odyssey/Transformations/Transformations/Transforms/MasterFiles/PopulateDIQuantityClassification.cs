using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles
{
	sealed class PopulateDIQuantityClassification : DataTransformation
	{
		public override string UserDescription => "Populates the DI_QuantityClassification column in the UNDGDataItem table based on DI_IsLimitedQuantity.";

		protected override void OfflinePostUpgradeTransform()
		{
			base.OfflinePostUpgradeTransform();

			var sql = @"
			UPDATE dbo.UNDGDataItem
			SET 
				DI_QuantityClassification = 'LIM',
				DI_SystemLastEditTimeUtc = GETUTCDATE(),
				DI_SystemLastEditUser = '~BP'
			WHERE 
				DI_IsLimitedQuantity = 1;";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
