using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	class UpdateCABServiceTaskDescription : DataTransformation
	{
		public override string UserDescription => "Update CAB ServiceTask Description";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = $@"
				UPDATE
					StmScheduleTask
				SET
					S5_ScheduleDescription = 'Canadian Customs CAD Auto-Sending'
					, S5_SystemLastEditTimeUtc = GETUTCDATE()
					, S5_SystemLastEditUser = '~BP'
				WHERE
					S5_ScheduleType = 'CAB'
					AND S5_TypeOfDocument = 'CAC'";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
