using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Architecture
{
	sealed class UpdateCOEEventIsCustomizableProperty : DataTransformation
	{
		public override string UserDescription => "Update COE Event IsCustomizable property.";

		protected override void OfflinePostUpgradeTransform()
		{
			base.OfflinePostUpgradeTransform();

			var sql = $@"
			BEGIN
				UPDATE dbo.StmEvent
				SET
					SE_IsCustomizable = 0,
					SE_SystemLastEditTimeUtc = GetUtcDate(),
					SE_SystemLastEditUser = '~BP'
				WHERE SE_Code = 'COE' AND SE_IsCustomizable = 1
			END";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
