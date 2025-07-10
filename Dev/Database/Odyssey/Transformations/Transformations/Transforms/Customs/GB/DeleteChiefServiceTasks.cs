using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.GB;
public class DeleteChiefServiceTasks : DataTransformation
{
	public override string UserDescription => "Remove Chief service task schedule as Chief is demise ";

	protected override void OfflinePostUpgradeTransform()
	{
		var sql = @"
				DELETE dbo.StmScheduleTask
				WHERE S5_ScheduleType IN ('MDD', 'MUD', 'CDD', 'CUD', 'PNT') AND S5_TypeOfDocument = 'GBC'";

		Db.Connection.ExecuteNonQuery(sql);
	}
}

