using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BusinessIntelligence
{
	public class UpdateAuditMaintenanceTaskSchedule : DataTransformation
	{
		public override string UserDescription => "Update the schedule of Audit Maintenance task to run only once per month";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
				UPDATE
					dbo.StmScheduleTask
				SET
					S5_TaskPeriodCount = 1,
					S5_TaskPeriod = 'M',
					S5_SystemLastEditTimeUtc = GetUtcDate(),
					S5_SystemLastEditUser = '~BP'
				WHERE
					S5_ScheduleType = 'ADM'
			";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
