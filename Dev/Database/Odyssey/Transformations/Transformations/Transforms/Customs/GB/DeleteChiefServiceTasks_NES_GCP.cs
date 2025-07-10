using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.GB;
public class DeleteChiefServiceTasks_NES_GCP : DataTransformation
{
	public override string UserDescription => "Remove Chief GCP, NES/GNE service task schedule as Chief is demise";

	protected override void OfflinePostUpgradeTransform()
	{
		var sql = @"
				DELETE dbo.StmScheduleTask
				WHERE S5_ScheduleType IN ('GCP', 'GNE') AND S5_TypeOfDocument = 'GBC'";

		Db.Connection.ExecuteNonQuery(sql);
	}
}

