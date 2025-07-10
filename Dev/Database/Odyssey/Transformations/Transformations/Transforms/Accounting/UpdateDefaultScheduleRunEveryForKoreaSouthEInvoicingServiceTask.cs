using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Accounting
{
	public class UpdateDefaultScheduleRunEveryForKoreaSouthEInvoicingServiceTask : DataTransformation
	{
		public override string UserDescription => "Update the default schedule run-every hours for Korea South E-Invoicing Service Task.";
		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE
	dbo.StmScheduleTask
SET
	S5_TaskPeriodCount = 1,
	S5_TaskPeriod = 'H'
WHERE
	S5_ScheduleType = 'EKR'";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
