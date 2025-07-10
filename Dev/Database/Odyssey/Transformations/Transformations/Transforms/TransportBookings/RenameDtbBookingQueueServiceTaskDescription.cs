using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings
{
	class RenameDtbBookingQueueServiceTaskDescription : DataTransformation
	{
		public override string UserDescription => "Rename description for DtbBookingQueueServiceTask";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var scheduleDescription = "Transport Job Generator";
			var scheduleType = "KMQ";
			var typeOfDocument = "DOM";

			var sqlCmd = $@"
				UPDATE dbo.StmScheduleTask
				SET S5_ScheduleDescription = '{scheduleDescription}'
					, S5_SystemLastEditTimeUtc = GETUTCDATE()
					, S5_SystemLastEditUser = '~BP'
				WHERE
					S5_ScheduleType = '{scheduleType}' AND
					S5_TypeOfDocument = '{typeOfDocument}' AND
					S5_ScheduleDescription != '{scheduleDescription}'";

			using (var cmd = Db.Connection.Command(sqlCmd))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
