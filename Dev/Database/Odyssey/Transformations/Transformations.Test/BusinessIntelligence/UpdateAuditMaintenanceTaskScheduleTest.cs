using System;
using System.Globalization;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BusinessIntelligence;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BusinessIntelligence
{
	class UpdateAuditMaintenanceTaskScheduleTest : TestCase
	{
		[TestedType(typeof(UpdateAuditMaintenanceTaskSchedule))]
		class TestUpdateRecordsInStmScheduleTaskAndStmServiceTask : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new UpdateAuditMaintenanceTaskSchedule();
			}

			Guid scheduleTaskPK;
			string originalTime;

			TransformationTestDataCreator TestHelper => testHelper ??= new TransformationTestDataCreator();
			TransformationTestDataCreator testHelper;

			protected override void PrepareTestData()
			{
				using (DataTransformationHelper.SuspendInsertAuditTriggerIfExists("StmScheduleTask"))
				{
					scheduleTaskPK = Guid.NewGuid();
					TestHelper.CreateScheduleTask(scheduleTaskPK, "D", 2, "ADM", isActive: true);
					originalTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
				}
			}

			protected override void AssertTransformationResults()
			{
				var sql = $@"
SELECT
	1
FROM
	dbo.StmScheduleTask
WHERE
	S5_PK = @PK
	AND S5_TaskPeriod = @Period
	AND S5_TaskPeriodCount = @PeriodCount
	AND S5_SystemLastEditUser = @LastEditUser
	AND S5_SystemLastEditTimeUtc > @OriginalTime
";

				using (var command = TestConnection.Command(sql))
				{
					command.AddParameter("@PK", System.Data.SqlDbType.UniqueIdentifier, scheduleTaskPK);
					command.AddParameter("@Period", System.Data.SqlDbType.VarChar, "M");
					command.AddParameter("@PeriodCount", System.Data.SqlDbType.Int, 1);
					command.AddParameter("@LastEditUser", System.Data.SqlDbType.VarChar, "~BP");
					command.AddParameter("@OriginalTime", System.Data.SqlDbType.DateTime, originalTime);

					var result = command.ExecuteNonQuery();
					AssertNotNull(result);
				}
			}
		}
	}
}
