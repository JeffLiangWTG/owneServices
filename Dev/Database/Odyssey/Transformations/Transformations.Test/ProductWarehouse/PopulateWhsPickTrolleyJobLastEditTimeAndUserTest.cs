using System;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(PopulateWhsPickTrolleyJobLastEditTimeAndUser))]
	class PopulateWhsPickTrolleyJobLastEditTimeAndUserTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateWhsPickTrolleyJobLastEditTimeAndUser();

		const string TriggerName = "TG_WhsPickTrolleyJob_PopulateLastEditTimeAndUser";

		protected override void PrepareTestData()
		{
			SetupWhsPickTrolleyJobTablePriorToSchemaUpdate();

			var sql = new SqlQueryBuilder();
			var refEq1 = new RefEquipment("ABC", "T1").AppendInsertAndReturnObject(sql);
			var refEq2 = new RefEquipment("XYZ", "T2").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var query = @"
INSERT INTO dbo.WhsPickTrolleyJob(WTJ_PK, WTJ_RQ_Equipment, WTJ_Status, WTJ_FinalisedDateUtc, WTJ_SystemCreateTimeUtc, WTJ_SystemCreateUser)
VALUES
	(NEWID(), @refEq1PK, 'FIN', @time1, @time2, 'ABC'),
	(NEWID(), @refEq2PK, 'FIN', @time3, @time4, 'XYZ')";

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsPickTrolleyJob_AuditDetailsAreNotMissing_Insert", WhsPickTrolleyJobSchema.Constants.TableName))
			using (var command = TestConnection.Command(query))
			{
				command.AddParameter("@refEq1PK", SqlDbType.UniqueIdentifier, refEq1.PK);
				command.AddParameter("@refEq2PK", SqlDbType.UniqueIdentifier, refEq2.PK);
				command.AddParameter("@time1", SqlDbType.DateTime, new DateTime(2024, 1, 1, 22, 0, 0));
				command.AddParameter("@time2", SqlDbType.DateTime, new DateTime(2022, 06, 08, 0, 0, 0));
				command.AddParameter("@time3", SqlDbType.DateTime, new DateTime(2023, 06, 01, 12, 0, 0));
				command.AddParameter("@time4", SqlDbType.DateTime, new DateTime(2021, 1, 1, 0, 0, 0));
				command.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			var pickTrolleyJob1 = WhsPickTrolleyJob.ShallowLoadFromDB(TestConnection).Where(wtj => wtj.WTJ_SystemCreateUser == "ABC").Single();
			AssertEquals("WTJ_SystemLastEditTimeUtc should be correct.", new DateTime(2024, 1, 1, 22, 0, 0), pickTrolleyJob1.WTJ_SystemLastEditTimeUtc);
			AssertEquals("WTJ_SystemLastEditUser should be correct.", "ABC", pickTrolleyJob1.WTJ_SystemLastEditUser);

			var pickTrolleyJob2 = WhsPickTrolleyJob.ShallowLoadFromDB(TestConnection).Where(wtj => wtj.WTJ_SystemCreateUser == "XYZ").Single();
			AssertEquals("WTJ_SystemLastEditTimeUtc should be correct.", new DateTime(2023, 06, 01, 12, 0, 0), pickTrolleyJob2.WTJ_SystemLastEditTimeUtc);
			AssertEquals("WTJ_SystemLastEditUser should be correct.", "XYZ", pickTrolleyJob2.WTJ_SystemLastEditUser);
		}

		public void TestTransformSetsLastEditTimeToCreateTime_WhenFinalisedDateIsNull()
		{
			SetupWhsPickTrolleyJobTablePriorToSchemaUpdate();

			var sql = new SqlQueryBuilder();
			var refEq = new RefEquipment("Trolley", "T1").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var query = "INSERT INTO dbo.WhsPickTrolleyJob(WTJ_PK, WTJ_RQ_Equipment, WTJ_SystemCreateTimeUtc, WTJ_SystemCreateUser) VALUES (NEWID(), @refEqPK, @time, 'ABC')";

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsPickTrolleyJob_AuditDetailsAreNotMissing_Insert", WhsPickTrolleyJobSchema.Constants.TableName))
			using (var command = TestConnection.Command(query))
			{
				command.AddParameter("@refEqPK", SqlDbType.UniqueIdentifier, refEq.PK);
				command.AddParameter("@time", SqlDbType.DateTime, new DateTime(2022, 06, 08, 0, 0, 0));
				command.ExecuteNonQuery();
			}

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var pickTrolleyJob = WhsPickTrolleyJob.ShallowLoadFromDB(TestConnection).Single();
			AssertEquals("WTJ_SystemLastEditTimeUtc should be correct.", new DateTime(2022, 06, 08, 0, 0, 0), pickTrolleyJob.WTJ_SystemLastEditTimeUtc);
			AssertEquals("WTJ_SystemLastEditUser should be correct.", "ABC", pickTrolleyJob.WTJ_SystemLastEditUser);
		}

		public void TestTriggerIsCreatedCorrectly()
		{
			SetupWhsPickTrolleyJobTablePriorToSchemaUpdate();

			var transform = GetNewTestTransformationInstance();
			var expectedTriggerDefinition = @"
CREATE TRIGGER dbo.TG_WhsPickTrolleyJob_PopulateLastEditTimeAndUser
	ON dbo.WhsPickTrolleyJob
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE wtj
	SET
		wtj.WTJ_SystemLastEditTimeUtc = GetUtcDate(),
 		wtj.WTJ_SystemLastEditUser = IIF(i.WTJ_SystemLastEditUser = '', wtj.WTJ_SystemCreateUser, i.WTJ_SystemLastEditUser)
	FROM
		dbo.WhsPickTrolleyJob wtj
		JOIN inserted i ON wtj.WTJ_PK = i.WTJ_PK
	WHERE
		wtj.WTJ_SystemLastEditTimeUtc IS NULL
END";
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsPickTrolleyJobSchema.Constants.TableName, TriggerName));
			AssertEquals("Trigger Definition is correct.", expectedTriggerDefinition, DbObjectCreator.GetTriggerDefinition(TestConnection, TriggerName));

			var sql = new SqlQueryBuilder();
			var refEq1 = new RefEquipment("ABC", "T1").AppendInsertAndReturnObject(sql);
			var refEq2 = new RefEquipment("XYZ", "T2").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var query = @"
INSERT INTO dbo.WhsPickTrolleyJob(WTJ_PK, WTJ_RQ_Equipment, WTJ_SystemLastEditUser, WTJ_SystemCreateTimeUtc, WTJ_SystemCreateUser)
VALUES
	(NEWID(), @refEq1PK, 'A',@time1, 'ABC'),
	(NEWID(), @refEq2PK, '', @time2, 'XYZ')";

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsPickTrolleyJob_AuditDetailsAreNotMissing_Insert", WhsPickTrolleyJobSchema.Constants.TableName))
			using (var command = TestConnection.Command(query))
			{
				command.AddParameter("@refEq1PK", SqlDbType.UniqueIdentifier, refEq1.PK);
				command.AddParameter("@refEq2PK", SqlDbType.UniqueIdentifier, refEq2.PK);
				command.AddParameter("@time1", SqlDbType.DateTime, new DateTime(2022, 06, 08, 0, 0, 0));
				command.AddParameter("@time2", SqlDbType.DateTime, new DateTime(2023, 06, 01, 12, 0, 0));
				command.ExecuteNonQuery();
			}

			var countOfMissingLastEditTimeOrUserRows = Db.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(WTJ_PK), 0) FROM dbo.WhsPickTrolleyJob WHERE WTJ_SystemLastEditUser IS NULL OR WTJ_SystemLastEditUser IS NULL");
			var countOfPopulatedLastEditTimeOrUserRows = Db.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(WTJ_PK), 0) FROM dbo.WhsPickTrolleyJob WHERE WTJ_SystemLastEditUser IS NOT NULL OR WTJ_SystemLastEditUser IS NOT NULL");

			AssertEquals("Should have no rows with WTJ_SystemLastEditTimeUtc or WTJ_SystemLastEditUser missing.", 0, countOfMissingLastEditTimeOrUserRows);
			AssertEquals("Trigger should have populated all WTJ_SystemLastEditTimeUtc and WTJ_SystemLastEditUser if missing.", 2, countOfPopulatedLastEditTimeOrUserRows);

			var pickTrolleyJob1 = WhsPickTrolleyJob.ShallowLoadFromDB(TestConnection).Where(wtj => wtj.WTJ_SystemCreateUser == "ABC").Single();
			AssertEquals("WTJ_SystemLastEditTimeUtc should not be null.", pickTrolleyJob1.WTJ_SystemLastEditTimeUtc != null, true);
			AssertEquals("WTJ_SystemLastEditUser should remain the same.", "A", pickTrolleyJob1.WTJ_SystemLastEditUser);

			var pickTrolleyJob2 = WhsPickTrolleyJob.ShallowLoadFromDB(TestConnection).Where(wtj => wtj.WTJ_SystemCreateUser == "XYZ").Single();
			AssertEquals("WTJ_SystemLastEditTimeUtc should not be null.", pickTrolleyJob2.WTJ_SystemLastEditTimeUtc != null, true);
			AssertEquals("WTJ_SystemLastEditUser should be correct.", "XYZ", pickTrolleyJob2.WTJ_SystemLastEditUser);
		}

		public void TestTriggerIsDeletedCorrectly()
		{
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsPickTrolleyJobSchema.Constants.TableName, TriggerName));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("Trigger should be deleted.", false, DbObjectCreator.TriggerExists(TestConnection, WhsPickTrolleyJobSchema.Constants.TableName, TriggerName));
		}

		public void TestOnlinePreUpgrade_TableDoesNotExist()
		{
			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, WhsPickTrolleyJobSchema.Constants.TableName, WhsPickTrolleyJobSchema.Constants.PK).DropRelateObjects(Db.Connection);
			DbObjectCreator.DropTableIfExists(Db.Connection, WhsPickTrolleyJobSchema.Constants.TableName);

			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));
		}

		public void TestOfflinePreUpgrade_TableDoesNotExist()
		{
			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, WhsPickTrolleyJobSchema.Constants.TableName, WhsPickTrolleyJobSchema.Constants.PK).DropRelateObjects(Db.Connection);
			DbObjectCreator.DropTableIfExists(Db.Connection, WhsPickTrolleyJobSchema.Constants.TableName);

			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None));
		}

		void SetupWhsPickTrolleyJobTablePriorToSchemaUpdate()
		{
			DBTransformationTestHelper.DropConstraintIfExists(WhsPickTrolleyJobSchema.Constants.TableName, "Constraint_WTJ_SystemLastEditUser", TestConnection);
			DBTransformationTestHelper.DropIndexIfExists(WhsPickTrolleyJobSchema.Constants.TableName, "NR_RX__WTJ_SystemLastEditTimeUtc");
			TestConnection.ExecuteNonQuery($@"ALTER TABLE {WhsPickTrolleyJobSchema.Constants.TableName} ALTER COLUMN {WhsPickTrolleyJobSchema.Constants.WTJ_SystemLastEditTimeUtc} SMALLDATETIME NULL");
			TestConnection.ExecuteNonQuery($@"CREATE INDEX NR_RX__WTJ_SystemLastEditTimeUtc ON {WhsPickTrolleyJobSchema.Constants.TableName} (WTJ_SystemLastEditTimeUtc) INCLUDE (WTJ_SystemLastEditUser) WHERE WTJ_SystemLastEditTimeUtc IS NOT NULL");
		}
	}
}
