using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Schema;
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
	[TestedType(typeof(UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation))]
	class UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisationTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var today = new DateTime(2024, 3, 19);

			var pick1 = new WhsPick(whs, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick3 = new WhsPick(whs, "P3", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);

			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O2") { WD_WP = pick2.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O3") { WD_WP = pick3.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var workOrder1 = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "FIN", "WO1") { WD_WP = pick2.PK , WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_PreventAddingIncorrectOrdersToPicks", WhsDocketSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		protected override void AssertTransformationResults()
		{
			var order1 = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketID == "O1").Single();
			var order2 = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketID == "O2").Single();
			var order3 = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketID == "O3").Single();
			var workOrder1 = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketID == "WO1").Single();
			order1
				.BuildAssertion(TestConnection)
				.ExpectEquals("Because pick status is NOT FIN, its docket status does NOT need to be changed.", d => d.WD_DocketStatus, "PIC")
				.ExpectHasNoLogs()
				.VerifyAll();
			order2
				.BuildAssertion(TestConnection)
				.ExpectEquals("Because pick status is FIN, its docket status need to be changed to DEP.", d => d.WD_DocketStatus, "DEP")
				.ExpectEquals("SystemLastEditTimeUtc should not be empty", p => p.WD_SystemLastEditTimeUtc.HasValue, true)
				.ExpectEquals("SystemLastEditUser should be ~BP", d => d.WD_SystemLastEditUser, "~BP")
				.ExpectHasNoLogs()
				.VerifyAll();
			order3
				.BuildAssertion(TestConnection)
				.ExpectEquals("Because pick status is FIN, its docket status need to be changed to DEP.", d => d.WD_DocketStatus, "DEP")
				.ExpectEquals("SystemLastEditTimeUtc should not be empty", p => p.WD_SystemLastEditTimeUtc.HasValue, true)
				.ExpectEquals("SystemLastEditUser should be ~BP", d => d.WD_SystemLastEditUser, "~BP")
				.ExpectHasNoLogs()
				.VerifyAll();
			workOrder1
				.BuildAssertion(TestConnection)
				.ExpectEquals("Although pick status is FIN, but because it is NOT an order, its docket status does NOT need to be changed.", d => d.WD_DocketStatus, "FIN")
				.ExpectHasNoLogs()
				.VerifyAll();
		}

		public void TestBatchingWorksProperly()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR2").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH2", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row6") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area4").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var startDate = new DateTime(2015, 2, 1);
			var enteredOrders = new WhsDocket[1500];
			for (var i = 0; i < 1500; i++)
			{
				enteredOrders[i] = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O" + i);
			}

			sql.AppendLine(WhsDocket.GetBulkInsertStatement(enteredOrders));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var orderPK = (Guid)TestConnection.ExecuteScalar(@"
SELECT TOP 1 WD_PK
FROM
(
	SELECT WD_PK, ROW_NUMBER() OVER(ORDER BY WD_PK ASC) as RowNum
	FROM dbo.WhsDocket
) as Dockets
WHERE RowNum > 1100
");
			sql.Clear();
			sql.AppendLine($@"
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TG_PreventAddingIncorrectOrdersToPicks' AND parent_id = OBJECT_ID('dbo.WhsDocket'))  
BEGIN  
    EXEC('DISABLE TRIGGER TG_PreventAddingIncorrectOrdersToPicks ON dbo.WhsDocket;');  
END
");
			var finalisedPick = new WhsPick(whs, "P1", "FIN") { WP_FinalizedDateUtc = startDate.AddDays(1), WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var updateStatement = WhsDocket.UpdateWhere(orderPK)
				.Set(d => d.WD_WP, finalisedPick.PK)
				.Set(d => d.WD_DocketStatus, "PIC")
				.Set(d => d.WD_FinalisedDate, startDate.AddDays(1))
				.Set(d => d.WD_GS_NKFinalizedBy, "T")
				.AsSQL();
			sql.AppendLine(updateStatement);
			sql.AppendLine($@"
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TG_PreventAddingIncorrectOrdersToPicks' AND parent_id = OBJECT_ID('dbo.WhsDocket'))  
BEGIN  
    EXEC('ENABLE TRIGGER TG_PreventAddingIncorrectOrdersToPicks ON dbo.WhsDocket;');  
END
");
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var orderStatus = TestConnection.ExecuteScalar($"SELECT {TemporaryColumnName} FROM WhsDocket WHERE WD_PK = '{orderPK}'");
			AssertEquals("Ensure batching worked, and the Order status update was not missed.", "DEP", orderStatus);
		}

		public void TestBashing_ShouldAllBeProcessedDuringOnlinePreUpgrade()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR2").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH2", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row6") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area4").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var startDate = new DateTime(2015, 2, 1);
			var finalisedPicks = new WhsPick[1000];
			var nonDepartedOrders = new WhsDocket[1000];
			for (var i = 0; i < 1000; i++)
			{
				var finDate = startDate.AddDays(i);
				finalisedPicks[i] = new WhsPick(whs, "P" + i, "FIN") { WP_FinalizedDateUtc = finDate, WP_GS_NKFinalizedBy = "A" };
				nonDepartedOrders[i] = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O" + i) { WD_WP = finalisedPicks[i].PK, WD_FinalisedDate = finDate, WD_GS_NKFinalizedBy = "T" };
			}
			sql.AppendLine(WhsPick.GetBulkInsertStatement(finalisedPicks));
			sql.AppendLine(WhsDocket.GetBulkInsertStatement(nonDepartedOrders));

			var pick1 = new WhsPick(whs, "P10001", "PIS").AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O10001") { WD_WP = pick1.PK, WD_FinalisedDate = startDate.AddDays(500), WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_PreventAddingIncorrectOrdersToPicks", WhsDocketSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var departedCount = TestConnection.ExecuteScalar($"SELECT COUNT(WD_PK) FROM WhsDocket WHERE {TemporaryColumnName} = 'DEP'");
			var pickingCount = TestConnection.ExecuteScalar($"SELECT COUNT(WD_PK) FROM WhsDocket WHERE {TemporaryColumnName} ='PIC'");
			AssertEquals(1000, departedCount);
			AssertEquals(1, pickingCount);
		}

		public void TestTempColumnIsCreated()
		{
			PrepareTestData();

			var column = WhsDocketSchema.WD_DocketStatus;
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(CountRowsToConvertSQL);
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRowsToConvert > 0);

			var transform = GetNewTestTransformationInstance();
			using (TestConnection.TrackExecutedCommands())
			{
				transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			}

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {TemporaryColumnName} should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));
			AssertEquals($"Temp Column {TemporaryColumnName} should be created with correct varchar type.", "varchar", DbObjectCreator.GetColumnType(TestConnection, column.TableName, TemporaryColumnName));

			var defaultConstraintTemporaryColumnName = DbObjectCreator.GenerateDefaultColumnConstraintName(column.TableName, TemporaryColumnName);
			AssertConstraintExists(tableDescriptor, defaultConstraintTemporaryColumnName, expectedToExists: true);

			AssertEquals("Default Constraint Definition should be correct.", "('PIC')",
				TestConnection.ExecuteScalar($"SELECT definition FROM sys.default_constraints WHERE name = '{defaultConstraintTemporaryColumnName}'"));
			AssertEquals("All rows should have been converted or untouched.", countOfRowsToConvert, TestConnection.ExecuteScalar(CountConvertedRowsSQL));

			var countOfAutoVersion = $@"
SELECT COUNT(*)
FROM WhsDocket
WHERE
	{TemporaryColumnName} = 'DEP'
	AND WD_AutoVersion = 1";
			AssertEquals("Rows should have been updated once.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfAutoVersion));

			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, FromPKName));
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, ToPKName));

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals($"Temp Column {TemporaryColumnName} should still be created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));
			AssertEquals($"Temp Column {TemporaryColumnName} should still have correct varchar type.", "varchar", DbObjectCreator.GetColumnType(TestConnection, column.TableName, TemporaryColumnName));
			AssertEquals("Rows should have been updated once.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfAutoVersion));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			RunColumnSynchroniser();

			AssertEquals($"Temp Column {TemporaryColumnName} should no longer exist.", false, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));
			AssertEquals($"Existing Column {column.Name} should have correct varchar type.", "varchar", DbObjectCreator.GetColumnType(TestConnection, column.TableName, column.Name));
			AssertEquals("All rows that needed conversion have the Status Column Populated.", countOfRowsToConvert, TestConnection.ExecuteScalar(CountRowsToConvertSQL));

			AssertConstraintExists(tableDescriptor, defaultConstraintTemporaryColumnName, expectedToExists: false);

			var defaultConstraintTargetName = DbObjectCreator.GenerateDefaultColumnConstraintName(column.TableName, column.Name);

			AssertConstraintExists(tableDescriptor, defaultConstraintTargetName, expectedToExists: false);

			void AssertConstraintExists(DbObjectCreator.TableDescriptor tableDescriptor, string defaultConstraintName, bool expectedToExists)
			{
				var constraintExistsSQL = $"SELECT COUNT(*) WHERE OBJECT_ID('{tableDescriptor.TableSchema}.{defaultConstraintName}', 'D') IS NOT NULL";
				var constraintExists = (int)TestConnection.ExecuteScalar(constraintExistsSQL);
				AssertEquals($"Default Constraint should {(expectedToExists ? "" : "*not* ")}exist.", expectedToExists ? 1 : 0, constraintExists);
			}
		}

		public void TestWhsDocketTriggerIsCreatedCorrectly()
		{
			var column = WhsDocketSchema.WD_DocketStatus;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var triggerName = "TG_WhsDocket_KeepWD_DocketStatusInSync";
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, column.TableName, triggerName));

			var schemaName = column.TableSchema.SqlSchemaName;
			var pkColumnName = column.TableSchema.PK.Name;
			var expectedTriggerDefinition = $@"
CREATE TRIGGER {schemaName}.{triggerName}
	ON {schemaName}.WhsDocket
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF UPDATE(WD_DocketStatus)
	BEGIN
		UPDATE tab
		SET
			tab.{TemporaryColumnName} = CASE WHEN NewStatus = 'DEP' THEN NewStatus ELSE tab.WD_DocketStatus END,
			tab.WD_SystemLastEditTimeUtc = ISNULL(tab.WD_SystemLastEditTimeUtc, GetUtcDate()),
			tab.WD_SystemLastEditUser = CASE WHEN tab.WD_SystemLastEditUser = '' THEN '~BP' ELSE tab.WD_SystemLastEditUser END
		FROM
			{schemaName}.WhsDocket AS tab
			OUTER APPLY
			(
				SELECT
					CASE WHEN tab.WD_DocketType = 'ORD' THEN 'DEP' ELSE tab.WD_DocketStatus END NewStatus
				FROM
					{schemaName}.WhsPick
				WHERE
					tab.WD_WP = WP_PK
					AND WP_FinalizedDateUtc IS NOT NULL
			) IsDeparted
			JOIN inserted on tab.WD_PK = inserted.WD_PK
	END
END";
			AssertEquals("Trigger Definition is correct", expectedTriggerDefinition, DbObjectCreator.GetTriggerDefinition(TestConnection, triggerName));

			PrepareTestData();

			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(CountRowsToConvertSQL);
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRowsToConvert > 0);
			AssertEquals("All rows should have been populated with status column from Trigger.", countOfRowsToConvert, TestConnection.ExecuteScalar(CountConvertedRowsSQL));
		}

		public void TestWhsDocketTriggerIsCreatedCorrectly_HandlesEntriesWithNullAuditData()
		{
			var column = WhsDocketSchema.WD_DocketStatus;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var triggerName = "TG_WhsPick_KeepWD_DocketStatusInSync";
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsPickSchema.Constants.TableName, triggerName));

			var sql = new SqlQueryBuilder();

			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var today = new DateTime(2024, 3, 19);

			var pick1 = new WhsPick(whs, "P1", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1")
			{
				WD_WP = pick1.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditTimeUtc = null,
				WD_SystemLastEditUser = "",
			}.AppendInsertAndReturnObject(sql);

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_PreventAddingIncorrectOrdersToPicks", WhsDocketSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsDocket_AuditDetailsAreNotMissing_Insert", WhsDocketSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsDocket_AuditDetailsAreNotMissing_Update", WhsDocketSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var countOfRowsWithAuditDataSet = $@"
SELECT COUNT(*)
FROM WhsDocket
WHERE
	{TemporaryColumnName} = 'DEP'
	AND WD_SystemLastEditTimeUtc > DATEADD(day, -1, GetUtcDate())
	AND WD_SystemLastEditUser = '~BP'";

			var totalUpdated = (int)TestConnection.ExecuteScalar(countOfRowsWithAuditDataSet);
			AssertEquals("Trigger should update docket.", 1, TestConnection.ExecuteScalar(countOfRowsWithAuditDataSet));
		}

		public void TestWhsPickTriggerIsCreatedCorrectly()
		{
			var column = WhsDocketSchema.WD_DocketStatus;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var triggerName = "TG_WhsPick_KeepWD_DocketStatusInSync";
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsPickSchema.Constants.TableName, triggerName));

			var expectedTriggerDefinition = $@"
CREATE TRIGGER dbo.{triggerName}
	ON dbo.WhsPick
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF UPDATE(WP_FinalizedDateUtc)
	BEGIN
		UPDATE docket
		SET
			docket.{TemporaryColumnName} = 'DEP',
			docket.WD_SystemLastEditTimeUtc = ISNULL(docket.WD_SystemLastEditTimeUtc, GetUtcDate()),
			docket.WD_SystemLastEditUser = CASE WHEN docket.WD_SystemLastEditUser = '' THEN '~BP' ELSE docket.WD_SystemLastEditUser END
		FROM
			dbo.WhsDocket AS docket 
			JOIN inserted on docket.WD_WP = inserted.WP_PK
		WHERE
			inserted.WP_FinalizedDateUtc IS NOT NULL
			AND docket.WD_DocketType = 'ORD'
	END
END";
			AssertEquals("Trigger Definition is correct", expectedTriggerDefinition, DbObjectCreator.GetTriggerDefinition(TestConnection, triggerName));

			PrepareTestData();

			var countOfConvertedRows = (int)TestConnection.ExecuteScalar(CountConvertedRowsSQL);

			var columnsPendingPickFinalisationSQL = $@"
SELECT
	COUNT(*)
FROM
	WhsDocket
	JOIN WhsPick ON WD_WP = WP_PK
WHERE
	WD_DocketType = 'ORD'
	AND WP_FinalizedDateUtc IS NULL
";

			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(columnsPendingPickFinalisationSQL);
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRowsToConvert > 0);

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsPick_EnsureDocketStatusDepartedOnPickFinalisation", "WhsPick"))
			{
				TestConnection.ExecuteNonQuery("UPDATE WhsPick SET WP_PickStatus = 'FIN', WP_FinalizedDateUtc = GetUTCDate(), WP_GS_NKFinalizedBy = 'A', WP_SystemLastEditTimeUtc = GetUTCDate(), WP_SystemLastEditUser = '~BP' WHERE WP_FinalizedDateUtc IS NULL");
			}

			AssertEquals("Trigger should update docket.", countOfConvertedRows + countOfRowsToConvert, TestConnection.ExecuteScalar(CountConvertedRowsSQL));
		}

		public void TestWhsPickTriggerIsCreatedCorrectly_HandlesEntriesWithNullAuditData()
		{
			var column = WhsDocketSchema.WD_DocketStatus;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var triggerName = "TG_WhsPick_KeepWD_DocketStatusInSync";
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsPickSchema.Constants.TableName, triggerName));

			var sql = new SqlQueryBuilder();

			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var today = new DateTime(2024, 3, 19);

			var pick1 = new WhsPick(whs, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1")
			{
				WD_WP = pick1.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditTimeUtc = null,
				WD_SystemLastEditUser = "",
			}.AppendInsertAndReturnObject(sql);

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_PreventAddingIncorrectOrdersToPicks", WhsDocketSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsDocket_AuditDetailsAreNotMissing_Insert", WhsDocketSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsDocket_AuditDetailsAreNotMissing_Update", WhsDocketSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsPick_EnsureDocketStatusDepartedOnPickFinalisation", "WhsPick"))
			{
				TestConnection.ExecuteNonQuery("UPDATE WhsPick SET WP_PickStatus = 'FIN', WP_FinalizedDateUtc = GetUTCDate(), WP_SystemLastEditTimeUtc = GetUTCDate(), WP_GS_NKFinalizedBy = 'A', WP_SystemLastEditUser = '~BP' WHERE WP_FinalizedDateUtc IS NULL");
			}

			var countOfRowsWithAuditDataSet = $@"
SELECT COUNT(*)
FROM WhsDocket
WHERE
	{TemporaryColumnName} = 'DEP'
	AND WD_SystemLastEditTimeUtc > DATEADD(day, -1, GetUtcDate())
	AND WD_SystemLastEditUser = '~BP'";

			var totalUpdated = (int)TestConnection.ExecuteScalar(countOfRowsWithAuditDataSet);
			AssertEquals("Trigger should update docket.", 1, TestConnection.ExecuteScalar(countOfRowsWithAuditDataSet));
		}

		public void TestTransformSetsLastEditTimeAndUser()
		{
			var column = WhsDocketSchema.WD_DocketStatus;
			PrepareTestData();

			var countOfRows = (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM WhsDocket");
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRows > 0);

			var timeColumName =  WhsDocketSchema.WD_SystemLastEditTimeUtc.Name;
			var userColumName = WhsDocketSchema.WD_SystemLastEditUser.Name;
			var updateSQL = $@"
UPDATE WhsDocket
SET
	{timeColumName} = '2024/01/01',
	{userColumName} = 'ZZZ'";

			TestConnection.ExecuteNonQuery(updateSQL);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countOfRowsWithUpdatedTimeAndUserSql = $@"
SELECT COUNT(*)
FROM WhsDocket
WHERE
	{TemporaryColumnName} = 'DEP'
	AND {timeColumName} > DATEADD(day, -1, GetUtcDate())
	AND {userColumName} = '~BP'";

			var countOfRowsNotUpdated = $@"
SELECT COUNT(*)
FROM WhsDocket
WHERE
	{TemporaryColumnName} <> 'DEP'
	AND {timeColumName} = '2024/01/01'
	AND {userColumName} = 'ZZZ'";

			var totalUpdated = (int)TestConnection.ExecuteScalar(countOfRowsWithUpdatedTimeAndUserSql);
			var totalUnchanged = (int)TestConnection.ExecuteScalar(countOfRowsNotUpdated);
			AssertEquals("All columns updated or unchanged", countOfRows, totalUpdated + totalUnchanged);
		}

		public void TestTransformSetsLastEditTimeAndUser_EvenWhenEmpty()
		{
			var column = WhsDocketSchema.WD_DocketStatus;
			PrepareTestData();

			var countOfRows = (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM WhsDocket");
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRows > 0);

			var timeColumName = WhsDocketSchema.WD_SystemLastEditTimeUtc.Name;
			var userColumName = WhsDocketSchema.WD_SystemLastEditUser.Name;
			var updateSQL = $@"
UPDATE WhsDocket
SET
	{timeColumName} = null,
	{userColumName} = ''";

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsDocket_AuditDetailsAreNotMissing_Update", WhsDocketSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(updateSQL);
			}

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countOfRowsWithAuditDataSet = $@"
SELECT COUNT(*)
FROM WhsDocket
WHERE
	{timeColumName} > DATEADD(day, -1, GetUtcDate())
	AND {userColumName} = '~BP'";

			var countOfRowsNotUpdated = $@"
SELECT COUNT(*)
FROM WhsDocket
WHERE
	{TemporaryColumnName} <> 'DEP'
	AND {timeColumName} IS NOT NULL
	AND {userColumName} <> ''";

			var totalUpdated = (int)TestConnection.ExecuteScalar(countOfRowsWithAuditDataSet);
			var totalUnchanged = (int)TestConnection.ExecuteScalar(countOfRowsNotUpdated);
			AssertEquals("All columns updated or unchanged", countOfRows, totalUpdated + totalUnchanged);
		}

		public void TestStatusExtendedProperty_WhenAlreadySetToFinished()
		{
			ExtProperty.Database.Update(TestConnection, ExtPropertiesClearedForNewRun, bool.TrueString);
			ExtProperty.Database.Update(TestConnection, TransformationStatusName, bool.TrueString);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var column = WhsDocketSchema.WD_DocketStatus;
			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {TemporaryColumnName} will not be added as transform has finished.", false, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));
			AssertEquals("Status should be updated to Finished once Transform is complete.", bool.TrueString, ExtProperty.Database.Select(TestConnection, TransformationStatusName));
		}

		public void TestStatusExtendedProperty_DeletesAllExtendedPropertiesWhenFinished()
		{
			PrepareTestData();

			var countOfRows = (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM WhsDocket");
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRows > 0);

			RunTransformation();

			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, FromPKName));
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, ToPKName));
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, ExtPropertiesClearedForNewRun));
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, TransformationStatusName));
		}

		public void TestTransformWithNothingToTransformWorks()
		{
			var column = WhsDocketSchema.WD_DocketStatus;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {TemporaryColumnName} should be added even if nothing was transformed.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			RunColumnSynchroniser();

			AssertEquals($"Temp Column {TemporaryColumnName} should get renamed even if nothing was transformed.", false, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));
			AssertEquals($"Column {column.Name} should have its type updated.", "varchar", DbObjectCreator.GetColumnType(TestConnection, column.TableName, column.Name));
		}

		public void TestOldTemporaryColumnName_WhenOldTransferCreateColumnButNotFinished()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var today = DateTime.Today;

			var pick = new WhsPick(whs, "P1", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_PreventAddingIncorrectOrdersToPicks", WhsDocketSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var targetColumn = WhsDocketSchema.WD_DocketStatus;
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsDocketSchema.Constants.TableName, OldTemporaryColumnName, targetColumn.SqlDbTypeDeclaration, defaultValue: "''");
			var tableDescriptor = new DbObjectCreator.TableDescriptor(targetColumn.TableSchema.SqlSchemaName, targetColumn.TableName);

			TestConnection.ExecuteNonQuery($"CREATE NONCLUSTERED INDEX _on_TEST ON WhsDocket ([{OldTemporaryColumnName}] ASC)");

			// Add old sync triggers
			Db.Connection.ExecuteNonQuery($@"
CREATE TRIGGER dbo.TG_WhsDocket_KeepWD_DocketStatusInSync
	ON dbo.WhsDocket
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF UPDATE(WD_DocketStatus)
	BEGIN
		UPDATE tab
		SET
			tab.{OldTemporaryColumnName} = CASE WHEN NewStatus = 'DEP' THEN NewStatus ELSE tab.WD_DocketStatus END,
			tab.WD_SystemLastEditTimeUtc = ISNULL(tab.WD_SystemLastEditTimeUtc, GetUtcDate()),
			tab.WD_SystemLastEditUser = CASE WHEN tab.WD_SystemLastEditUser = '' THEN '~BP' ELSE tab.WD_SystemLastEditUser END
		FROM
			dbo.WhsDocket AS tab
			OUTER APPLY
			(
				SELECT
					CASE WHEN tab.WD_DocketType = 'ORD' THEN 'DEP' ELSE tab.WD_DocketStatus END NewStatus
				FROM
					dbo.WhsPick
				WHERE
					tab.WD_WP = WP_PK
					AND WP_FinalizedDateUtc IS NOT NULL
			) IsDeparted
			JOIN inserted on tab.WD_PK = inserted.WD_PK
	END
END");
			Db.Connection.ExecuteNonQuery($@"
CREATE TRIGGER dbo.TG_WhsPick_KeepWD_DocketStatusInSync
	ON dbo.WhsPick
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF UPDATE(WP_FinalizedDateUtc)
	BEGIN
		UPDATE docket
		SET
			docket.{OldTemporaryColumnName} = 'DEP',
			docket.WD_SystemLastEditTimeUtc = ISNULL(docket.WD_SystemLastEditTimeUtc, GetUtcDate()),
			docket.WD_SystemLastEditUser = CASE WHEN docket.WD_SystemLastEditUser = '' THEN '~BP' ELSE docket.WD_SystemLastEditUser END
		FROM
			dbo.WhsDocket AS docket 
			JOIN inserted on docket.WD_WP = inserted.WP_PK
		WHERE
			inserted.WP_FinalizedDateUtc IS NOT NULL
			AND docket.WD_DocketType = 'ORD'
	END
END");

			Assert("Precondition.", DbObjectCreator.ColumnExists(Db.Connection, tableDescriptor, OldTemporaryColumnName));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			Assert("Should delete this column in online transformation and use new name.", !DbObjectCreator.ColumnExists(Db.Connection, tableDescriptor, OldTemporaryColumnName));
			Assert("Should create new column in online transformation.", DbObjectCreator.ColumnExists(Db.Connection, tableDescriptor, TemporaryColumnName));

			var trigger1Name = "TG_WhsDocket_KeepWD_DocketStatusInSync";
			var trigger2Name = "TG_WhsPick_KeepWD_DocketStatusInSync";
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, trigger1Name));
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsPickSchema.Constants.TableName, trigger2Name));
			AssertEquals("Trigger Definition is correct", false, DbObjectCreator.GetTriggerDefinition(TestConnection, trigger1Name).Contains(OldTemporaryColumnName));
			AssertEquals("Trigger Definition is correct", false, DbObjectCreator.GetTriggerDefinition(TestConnection, trigger2Name).Contains(OldTemporaryColumnName));

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			RunTransformation();

			WhsDocket.ShallowLoadFromDB(TestConnection, order.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("Should be updated to DEP even if it is not succeeded in old transformation.", d => d.WD_DocketStatus, "DEP")
				.VerifyAll();
		}

		public void TestStatusExtendedProperty_WhenOldTransferCreateColumnButNotFinished()
		{
			var targetColumn = WhsDocketSchema.WD_DocketStatus;
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsDocketSchema.Constants.TableName, OldTemporaryColumnName, targetColumn.SqlDbTypeDeclaration, defaultValue: "''");
			var tableDescriptor = new DbObjectCreator.TableDescriptor(targetColumn.TableSchema.SqlSchemaName, targetColumn.TableName);

			Assert("Precondition.", DbObjectCreator.ColumnExists(Db.Connection, tableDescriptor, OldTemporaryColumnName));

			ExtProperty.Database.Update(TestConnection, FromPKName, Guid.NewGuid().ToString());
			ExtProperty.Database.Update(TestConnection, ToPKName, Guid.NewGuid().ToString());
			ExtProperty.Database.Update(TestConnection, TransformationStatusName, bool.FalseString);
			ExtProperty.Database.Update(TestConnection, ExtPropertiesClearedForNewRun, bool.FalseString);

			AssertNotEquals("Precondition.", null, ExtProperty.Database.Select(TestConnection, FromPKName));
			AssertNotEquals("Precondition.", null, ExtProperty.Database.Select(TestConnection, ToPKName));
			AssertEquals("Precondition.", bool.FalseString, ExtProperty.Database.Select(TestConnection, TransformationStatusName));
			AssertEquals("Precondition.", bool.FalseString, ExtProperty.Database.Select(TestConnection, ExtPropertiesClearedForNewRun));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Extended Properties should be cleared if in old transformation created.", null, ExtProperty.Database.Select(TestConnection, FromPKName));
			AssertEquals("Extended Properties should be cleared if in old transformation created.", null, ExtProperty.Database.Select(TestConnection, ToPKName));
			AssertEquals("Status should be updated once OnlinePreUpgrade is complete.", bool.TrueString, ExtProperty.Database.Select(TestConnection, TransformationStatusName));
			AssertEquals("Status should be updated once OnlinePreUpgrade is complete.", bool.TrueString, ExtProperty.Database.Select(TestConnection, ExtPropertiesClearedForNewRun));
		}

		public void TestTransformSkipsPreviouslyProcessedEntries()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var today = new DateTime(2024, 3, 19);

			var pick1 = new WhsPick(whs, "P1", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick3 = new WhsPick(whs, "P3", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick4 = new WhsPick(whs, "P4", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);

			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O2") { WD_WP = pick2.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O3") { WD_WP = pick3.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var order4 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O4") { WD_WP = pick4.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_PreventAddingIncorrectOrdersToPicks", WhsDocketSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			ExtProperty.Database.Update(TestConnection, ExtPropertiesClearedForNewRun, bool.TrueString);

			var minPK = (Guid)TestConnection.ExecuteScalar("SELECT MIN(WD_PK) FROM WhsDocket");
			var maxPK = (Guid)TestConnection.ExecuteScalar("SELECT MAX(WD_PK) FROM WhsDocket");
			var secondMaxPK = (Guid)TestConnection.ExecuteScalar($"SELECT MAX(WD_PK) FROM WhsDocket WHERE WD_PK <> '{maxPK.ToString()}'");

			ExtProperty.Database.Update(TestConnection, FromPKName, minPK.ToString());
			ExtProperty.Database.Update(TestConnection, ToPKName, secondMaxPK.ToString());

			RunTransformation();

			WhsDocket.ShallowLoadFromDB(TestConnection, order1.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("Only Dockets within boundary range should be updated to DEP.", d => d.WD_DocketStatus, GetExpectedStatus(order1.PK))
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, order2.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("Only Dockets within boundary range should be updated to DEP.", d => d.WD_DocketStatus, GetExpectedStatus(order2.PK))
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, order3.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("Only Dockets within boundary range should be updated to DEP.", d => d.WD_DocketStatus, GetExpectedStatus(order3.PK))
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, order4.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("Only Dockets within boundary range should be updated to DEP.", d => d.WD_DocketStatus, GetExpectedStatus(order4.PK))
				.VerifyAll();

			string GetExpectedStatus(Guid pk) => pk == minPK || pk == maxPK ? "PIC" : "DEP";
		}

		public void TestTransformSkipsPreviouslyProcessedEntries_ClearsExtPropertiesIfNewTransformationInstance()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var today = new DateTime(2024, 3, 19);

			var pick1 = new WhsPick(whs, "P1", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick3 = new WhsPick(whs, "P3", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick4 = new WhsPick(whs, "P4", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);

			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O2") { WD_WP = pick2.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O3") { WD_WP = pick3.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var order4 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O4") { WD_WP = pick4.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_PreventAddingIncorrectOrdersToPicks", WhsDocketSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var fromPKName = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.FromPK";
			var toPKName = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.ToPK";

			var minPK = (Guid)TestConnection.ExecuteScalar("SELECT MIN(WD_PK) FROM WhsDocket");
			var maxPK = (Guid)TestConnection.ExecuteScalar("SELECT MAX(WD_PK) FROM WhsDocket");
			var secondMaxPK = (Guid)TestConnection.ExecuteScalar($"SELECT MAX(WD_PK) FROM WhsDocket WHERE WD_PK <> '{maxPK.ToString()}'");

			ExtProperty.Database.Update(TestConnection, fromPKName, minPK.ToString());
			ExtProperty.Database.Update(TestConnection, toPKName, secondMaxPK.ToString());

			RunTransformation();

			WhsDocket.ShallowLoadFromDB(TestConnection, order1.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("All Dockets should be updated to DEP.", d => d.WD_DocketStatus, "DEP")
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, order2.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("All Dockets should be updated to DEP.", d => d.WD_DocketStatus, "DEP")
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, order3.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("All Dockets should be updated to DEP.", d => d.WD_DocketStatus, "DEP")
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, order4.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("All Dockets should be updated to DEP.", d => d.WD_DocketStatus, "DEP")
				.VerifyAll();
		}

		public void TestTransformSkipsPreviouslyProcessedEntries_FullRange()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var today = new DateTime(2024, 3, 19);

			var pick1 = new WhsPick(whs, "P1", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick3 = new WhsPick(whs, "P3", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick4 = new WhsPick(whs, "P4", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);

			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O2") { WD_WP = pick2.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O3") { WD_WP = pick3.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var order4 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O4") { WD_WP = pick4.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_PreventAddingIncorrectOrdersToPicks", WhsDocketSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			ExtProperty.Database.Update(TestConnection, ExtPropertiesClearedForNewRun, bool.TrueString);

			var fromPKName = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.FromPK";
			var toPKName = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.ToPK";

			var maxPK = (Guid)TestConnection.ExecuteScalar("SELECT MAX(WD_PK) FROM WhsDocket");

			ExtProperty.Database.Update(TestConnection, fromPKName, Guid.Empty.ToString());
			ExtProperty.Database.Update(TestConnection, toPKName, maxPK.ToString());

			RunTransformation();

			WhsDocket.ShallowLoadFromDB(TestConnection, order1.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("Dockets within boundary range should be updated to DEP.", d => d.WD_DocketStatus, "DEP")
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, order2.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("Dockets within boundary range should be updated to DEP.", d => d.WD_DocketStatus, "DEP")
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, order3.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("Dockets within boundary range should be updated to DEP.", d => d.WD_DocketStatus, "DEP")
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, order4.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("Dockets within boundary range should be updated to DEP.", d => d.WD_DocketStatus, "DEP")
				.VerifyAll();
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestOnlineTransformWorksWithoutTransaction()
		{
			using (var manager = TestConnection.BeginTransactionWithManager())
			{
				PrepareTestData();
				manager.CommitTransaction();
			}

			var column = WhsDocketSchema.WD_DocketStatus;
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(CountRowsToConvertSQL);
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRowsToConvert > 0);

			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));
		}

		#region ExtPropertiesAndConstants

		readonly string TemporaryColumnName = ColumnSynchroniser.ColumnRenamePrefix + WhsDocketSchema.WD_DocketStatus.Name;
		const string OldTemporaryColumnName = "_ODS_WD_DocketStatus";
		const string TransformationStatusName = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.TransformationRunToCompletion";
		const string FromPKName = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.FromPK";
		const string ToPKName = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.ToPK";
		const string ExtPropertiesClearedForNewRun = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.TransformationDefectFix_WI00748138";

		#endregion

		#region Implementation

		string CountRowsToConvertSQL => @"
SELECT COUNT(*)
FROM
	WhsDocket
	JOIN WhsPick ON WD_WP = WP_PK
WHERE
	WD_DocketType = 'ORD'
	AND WP_FinalizedDateUtc IS NOT NULL";

		string CountConvertedRowsSQL => $@"
SELECT COUNT(*)
FROM WhsDocket
WHERE
	{TemporaryColumnName} = 'DEP'";

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation();

		protected override void RunTransformation()
		{
			var transform = new UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation();
			AssertNoExceptionThrown(transform.Run);

			RunColumnSynchroniser();
		}

		void RunColumnSynchroniser()
		{
			var columnSynchroniser = new ColumnSynchroniser(TestConnection, new DummyUpgradeManager(), Db.DatabaseName, Db.DatabaseName);
			AssertNoExceptionThrown(columnSynchroniser.DropAlterAndAddColumns);
		}

		#endregion
	}
}
