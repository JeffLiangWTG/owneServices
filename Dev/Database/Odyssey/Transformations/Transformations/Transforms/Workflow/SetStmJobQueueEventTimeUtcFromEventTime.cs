using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Workflow;

public class SetStmJobQueueEventTimeUtcFromEventTime : DataTransformation
{
	public override string UserDescription => @"Set StmJobQueue.SJ_EventTimeUtc = SJ_EventTime if SJ_EventTimeUtc is null";

	protected override void OfflinePreUpgradeTransform()
	{
		if (!DbObjectCreator.TableExists(Db.Connection, "StmJobQueue"))
		{
			return;
		}
		if (!DbObjectCreator.ColumnExists(Db.Connection, "StmJobQueue", "SJ_EventTimeUtc"))
		{
			Db.Connection.ExecuteNonQuery(@"ALTER TABLE dbo.StmJobQueue ADD SJ_EventTimeUtc DATETIME NULL");
		}
		Db.Connection.ExecuteNonQuery(@"
		UPDATE dbo.StmJobQueue SET
			SJ_EventTimeUtc = SJ_EventTime
		WHERE
			SJ_EventTimeUtc IS NULL");
	}
}
