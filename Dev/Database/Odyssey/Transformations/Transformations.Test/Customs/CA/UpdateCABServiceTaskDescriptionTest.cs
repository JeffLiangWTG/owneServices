using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(UpdateCABServiceTaskDescription))]
	public class UpdateCABServiceTaskDescriptionTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateCABServiceTaskDescription();

		Guid S5_PK_toBeModified = Guid.NewGuid();
		Guid S5_PK_toBeModified2 = Guid.NewGuid();
		Guid S5_PK_notToBeModified = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			var createScheduleTaskSql = $@"
				INSERT INTO dbo.StmScheduleTask
					(S5_PK, S5_ScheduleDescription, S5_TaskPeriod, S5_TaskPeriodCount, S5_ScheduleType, S5_TypeOfDocument, S5_IsActive, S5_ParentTableCode, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser)
				VALUES
					('{S5_PK_toBeModified}', 'Canadian Customs B3 Auto-Sending', 'H', 6, 'CAB', 'CAC', 1, 'SH', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{S5_PK_toBeModified2}', 'Canadian Customs B3/CAD Auto-Sending', 'H', 6, 'CAB', 'CAC', 1, 'SH', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{S5_PK_notToBeModified}', 'Another service task description', 'H', 6, 'XXX', 'CAC', 1, 'SH', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			Db.Connection.ExecuteNonQuery(createScheduleTaskSql);
		}

		protected override void AssertTransformationResults()
		{
			using (var cmd = Db.Connection.Command($@"SELECT S5_PK, S5_ScheduleDescription FROM dbo.StmScheduleTask WHERE S5_PK IN ('{S5_PK_toBeModified}', '{S5_PK_toBeModified2}', '{S5_PK_notToBeModified}')"))
			using (var reader = cmd.ExecuteReader())
			{
				var dt = new DataTable("StmScheduleTask");
				dt.Load(reader);
				dt.PrimaryKey = new[] { dt.Columns["S5_PK"] };

				AssertEquals(3, dt.Rows.Count);

				var description = dt.Rows.Find(S5_PK_toBeModified).Field<string>("S5_ScheduleDescription");
				AssertEquals("Description should be changed", "Canadian Customs CAD Auto-Sending", description);

				description = dt.Rows.Find(S5_PK_toBeModified2).Field<string>("S5_ScheduleDescription");
				AssertEquals("Description should be changed", "Canadian Customs CAD Auto-Sending", description);

				description = dt.Rows.Find(S5_PK_notToBeModified).Field<string>("S5_ScheduleDescription");
				AssertEquals("Description should NOT be changed", "Another service task description", description);
			}
		}
	}
}
