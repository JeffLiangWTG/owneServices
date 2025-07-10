using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Workflow
{
	public class UpdateStmJobQueueWhenItContainsInvalidStatus : DataTransformation
	{
		public override string UserDescription => "Update StmJobQueue when it contains an invalid status";

		protected override void OfflinePostUpgradeTransform()
		{
			base.OfflinePostUpgradeTransform();
			var command = "UPDATE dbo.StmJobQueue SET SJ_Status='FAI' WHERE SJ_Status NOT IN ('FAI', 'PRS', 'QUE')";
			Db.Connection.ExecuteNonQuery(command);
		}
	}
}
