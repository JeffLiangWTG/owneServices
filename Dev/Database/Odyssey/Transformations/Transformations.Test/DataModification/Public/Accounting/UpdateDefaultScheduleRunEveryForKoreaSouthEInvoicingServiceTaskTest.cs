using System;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations;
using Enterprise.DbUpgrader.Transformations.Accounting;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Accounting.Testing
{
	[TestedType(typeof(UpdateDefaultScheduleRunEveryForKoreaSouthEInvoicingServiceTask))]
	class UpdateDefaultScheduleRunEveryForKoreaSouthEInvoicingServiceTaskTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateDefaultScheduleRunEveryForKoreaSouthEInvoicingServiceTask();
		}

		protected override void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery("DELETE FROM dbo.StmScheduleTask");

			scheduleTaskPK1 = Guid.NewGuid();
			scheduleTaskPK2 = Guid.NewGuid();
			scheduleTaskPK3 = Guid.NewGuid();
			scheduleTaskPK4 = Guid.NewGuid();

			TestHelper.CreateScheduleTask(scheduleTaskPK1, "H", 6, "EKR", true);
			TestHelper.CreateScheduleTask(scheduleTaskPK2, "M", 5, "EKR", false);
			TestHelper.CreateScheduleTask(scheduleTaskPK3, "H", 3, "XXX", true);
			TestHelper.CreateScheduleTask(scheduleTaskPK4, "S", 3, "YYY", false);
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(4, (int)TestConnection.ExecuteScalar("SELECT COUNT(0) FROM dbo.StmScheduleTask"));

			AssertStmScheduleTask(scheduleTaskPK1, "EKR", "H", 1);
			AssertStmScheduleTask(scheduleTaskPK2, "EKR", "H", 1);
			AssertStmScheduleTask(scheduleTaskPK3, "XXX", "H", 3);
			AssertStmScheduleTask(scheduleTaskPK4, "YYY", "S", 3);
		}

		void AssertStmScheduleTask(Guid pk, string type, string period, int periodCount)
		{
			var sql = @"
SELECT
	1
FROM
	dbo.StmScheduleTask
WHERE
	S5_PK = @PK AND
	S5_ScheduleType = @Type AND
	S5_TaskPeriod = @Period AND
	S5_TaskPeriodCount = @PeriodCount";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@PK", System.Data.SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@Type", System.Data.SqlDbType.VarChar, type);
				command.AddParameter("@Period", System.Data.SqlDbType.VarChar, period);
				command.AddParameter("@PeriodCount", System.Data.SqlDbType.Int, periodCount);

				AssertEquals(1, (int)command.ExecuteScalar());
			}
		}

		TransformationTestDataCreator TestHelper => testHelper ??= new TransformationTestDataCreator();
		TransformationTestDataCreator testHelper;

		Guid scheduleTaskPK1, scheduleTaskPK2, scheduleTaskPK3, scheduleTaskPK4;
	}
}
