using System;
using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.RunnableTask;

public static class RunnableServiceTaskTestHelper
{
	public static void DbUpdateServiceTaskSchedule(Guid pk, bool isActive = false, string description = "", ZDateTime lastEditTimeUtc = new ())
	{
		Db.Connection.ExecuteNonQuery(@"
UPDATE dbo.StmScheduleTask
SET
	S5_IsActive = @isActive,
	S5_ScheduleDescription = @description,
	S5_SystemLastEditTimeUtc = @lastEditTimeUtc
WHERE S5_PK = @pk
",
			cmd =>
			{
				cmd.AddParameter("@pk", System.Data.SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@isActive", System.Data.SqlDbType.Bit, isActive);
				cmd.AddParameter("@description", System.Data.SqlDbType.VarChar, description);
				cmd.AddParameter("@lastEditTimeUtc", System.Data.SqlDbType.SmallDateTime, !lastEditTimeUtc.IsEmpty ? lastEditTimeUtc : ZDateTime.UtcNow);
			});
	}
}
