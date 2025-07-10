using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class MigrateMilestoneCompletionPivotToSoftKey : DataTransformation
	{
		public override string UserDescription => "Migrate the MilestoneCompletionPivot table to use soft keys on ProcessTasks and ProcessHeader.";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			if (!DbObjectCreator.TableExists(Db.Connection, "MilestoneCompletionPivot"))
			{
				return;
			}

			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, ProcessTasksSchema.Constants.TableName, ProcessTasksSchema.Constants.P9_MilestoneCompletionPivotKey, "NVARCHAR(200) SPARSE", "NULL");

			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, ProcessHeaderSchema.Constants.TableName, ProcessHeaderSchema.Constants.FH_MilestoneCompletionPivotKey, "NVARCHAR(200) SPARSE", "NULL");

			var updateTasks = @"
UPDATE task
SET
    task.P9_MilestoneCompletionPivotKey = SUBSTRING(milestone.P9_SE_NKMilestoneEvent + ' - ' + milestone.P9_Description, 0, 200),
    task.P9_SystemLastEditTimeUtc = GETUTCDATE(),
    task.P9_SystemLastEditUser = '~BP'
FROM ProcessTasks task
JOIN MilestoneCompletionPivot mcp
    ON task.P9_PK = mcp.MCP_ParentId
    AND mcp.MCP_ParentTableCode = 'P9'
JOIN ProcessTasks milestone
    ON milestone.P9_PK = mcp.MCP_P9_Milestone";

			var updateHeaders = @"
UPDATE header
SET
    header.FH_MilestoneCompletionPivotKey = SUBSTRING(milestone.P9_SE_NKMilestoneEvent + ' - ' + milestone.P9_Description, 0, 200),
    header.FH_SystemLastEditTimeUtc = GETUTCDATE(),
    header.FH_SystemLastEditUser = '~BP'
FROM ProcessHeader header
JOIN MilestoneCompletionPivot mcp
    ON header.FH_PK = mcp.MCP_ParentId
    AND mcp.MCP_ParentTableCode = 'FH'
JOIN ProcessTasks milestone
    ON milestone.P9_PK = mcp.MCP_P9_Milestone";
			Db.Connection.ExecuteNonQuery(updateTasks);
			Db.Connection.ExecuteNonQuery(updateHeaders);
		}
	}
}
