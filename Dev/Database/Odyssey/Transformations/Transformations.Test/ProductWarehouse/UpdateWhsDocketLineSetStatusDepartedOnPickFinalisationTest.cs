using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
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
using static Enterprise.DbUpgrader.Shared.DbObjectCreator;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(UpdateWhsDocketLineSetStatusDepartedOnPickFinalisation))]
	class UpdateWhsDocketLineSetStatusDepartedOnPickFinalisationTest : DataTransformationTestCase
	{
		readonly string TemporaryColumnName = ColumnSynchroniser.ColumnRenamePrefix + WhsDocketLineSchema.WE_DocketLineStatus.Name;
		const string LastProcessedChunkPKName = "UpdateWhsDocketLineSetStatusDepartedOnPickFinalisation.LastProcessedChunkPK";
		const string TransformationHasRunToCompletion = "UpdateWhsDocketLineSetStatusDepartedOnPickFinalisation.TransformationRunToCompletion";
		const string ExtPropertiesAndTempTriggersClearedForNewRun = "UpdateWhsDocketLineSetStatusDepartedOnPickFinalisation.TransformationDefectFix_WI00843593";
		const string WhsDocketLineSyncTriggerName = "TG_WhsDocketLine_KeepDocketLineStatusInSync";
		const string WhsPickSyncTriggerName = "TG_WhsPick_KeepDocketLineStatusInSync";

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
			var today = new DateTime(2024, 10, 17);

			var pick1 = new WhsPick(whs, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick3 = new WhsPick(whs, "P3", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);

			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O2") { WD_WP = pick2.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order2, product.PK, 2m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var orderLine3 = new WhsDocketLine(order2, product.PK, 3m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O3") { WD_WP = pick2.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var orderLine4 = new WhsDocketLine(order3, product.PK, 4m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var order4 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O4").AppendInsertAndReturnObject(sql);
			var orderLine5 = new WhsDocketLine(order4, product.PK, 5m) { WE_DocketLineStatus = "" }.AppendInsertAndReturnObject(sql);
			var workOrder1 = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "FIN", "WO1") { WD_WP = pick3.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var workOrderLine1 = new WhsDocketLine(workOrder1, product.PK, 10m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		protected override void AssertTransformationResults()
		{
			var orderLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, d => d.WE_TransactionQuantity == 1m).Single();
			var orderLine2 = WhsDocketLine.ShallowLoadFromDB(TestConnection, d => d.WE_TransactionQuantity == 2m).Single();
			var orderLine3 = WhsDocketLine.ShallowLoadFromDB(TestConnection, d => d.WE_TransactionQuantity == 3m).Single();
			var orderLine4 = WhsDocketLine.ShallowLoadFromDB(TestConnection, d => d.WE_TransactionQuantity == 4m).Single();
			var orderLine5 = WhsDocketLine.ShallowLoadFromDB(TestConnection, d => d.WE_TransactionQuantity == 5m).Single();
			var workOrderLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, d => d.WE_TransactionQuantity == 10m).Single();
			var workOrder = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "WOR").Single();
			var workOrderPick = WhsPick.ShallowLoadFromDB(TestConnection, d => d.PK == workOrder.WD_WP).Single();

			orderLine1
				.BuildAssertion(TestConnection)
				.ExpectEquals("Because pick status is NOT FIN, its docket line status does NOT need to be changed.", d => d.WE_DocketLineStatus, "")
				.ExpectHasNoLogs()
				.VerifyAll();

			orderLine2
				.BuildAssertion(TestConnection)
				.ExpectEquals("Because pick status is FIN, its docket line status need to be changed to DEP.", d => d.WE_DocketLineStatus, "DEP")
				.ExpectEquals("SystemLastEditTimeUtc should not be empty", d => d.WE_SystemLastEditTimeUtc.HasValue, true)
				.ExpectEquals("SystemLastEditUser should be ~BP", d => d.WE_SystemLastEditUser, "~BP")
				.ExpectHasNoLogs()
				.VerifyAll();
			orderLine3
				.BuildAssertion(TestConnection)
				.ExpectEquals("Because pick status is FIN, its docket line status need to be changed to DEP.", d => d.WE_DocketLineStatus, "DEP")
				.ExpectEquals("SystemLastEditTimeUtc should not be empty", d => d.WE_SystemLastEditTimeUtc.HasValue, true)
				.ExpectEquals("SystemLastEditUser should be ~BP", d => d.WE_SystemLastEditUser, "~BP")
				.ExpectHasNoLogs()
				.VerifyAll();
			orderLine4
				.BuildAssertion(TestConnection)
				.ExpectEquals("Because pick status is FIN, its docket line status need to be changed to DEP.", d => d.WE_DocketLineStatus, "DEP")
				.ExpectEquals("SystemLastEditTimeUtc should not be empty", d => d.WE_SystemLastEditTimeUtc.HasValue, true)
				.ExpectEquals("SystemLastEditUser should be ~BP", d => d.WE_SystemLastEditUser, "~BP")
				.ExpectHasNoLogs()
				.VerifyAll();

			orderLine5
				.BuildAssertion(TestConnection)
				.ExpectEquals("Should NOT change line status for order line which is not picked.", d => d.WE_DocketLineStatus, "")
				.ExpectEquals("SystemLastEditUser should be A", d => d.WE_SystemLastEditUser, "A")
				.ExpectHasNoLogs()
				.VerifyAll();

			workOrderLine1
				.BuildAssertion(TestConnection)
				.ExpectEquals("Although pick status is FIN, but because it is NOT an order line, its docket line status does NOT need to be changed.", d => d.WE_DocketLineStatus, "FIN")
				.ExpectHasNoLogs()
				.VerifyAll();
		}

		public void TestBatching_WorksProperly()
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
			var pisPick = new WhsPick(whs, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var finPick = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = startDate.AddDays(1), WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pickingOrderLines = new WhsDocketLine[1500];
			var pickingOrders = new WhsDocket[50];
			WhsDocket curPickingDocket = null;
			for (var i = 0; i < 1500; i++)
			{
				if (i % 30 == 0)
				{
					curPickingDocket = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "OX" + i) { WD_WP = pisPick.PK };
					pickingOrders[i / 30] = curPickingDocket;
				}
				pickingOrderLines[i] = new WhsDocketLine(curPickingDocket, product.PK, 1m);
			}
			sql.AppendLine(WhsDocket.GetBulkInsertStatement(pickingOrders));
			sql.AppendLine(WhsDocketLine.GetBulkInsertStatement(pickingOrderLines));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			sql.Clear();

			var selectedOrderLine = pickingOrderLines[1100];
			var finOrder = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1") { WD_WP = finPick.PK, WD_FinalisedDate = startDate, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			{
				var updateOrderLine = WhsDocketLine.UpdateWhere(selectedOrderLine.PK)
				.Set(d => d.WE_WD.FK, finOrder.PK)
				.Set(d => d.WE_DocketLineStatus, "FIN")
				.Set(d => d.WE_FinalisedDate, startDate)
				.Post(TestConnection);
			}
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var orderLineStatus = TestConnection.ExecuteScalar($"SELECT {TemporaryColumnName} FROM WhsDocketLine WHERE WE_PK = '{selectedOrderLine.PK}'");
			AssertEquals("Ensure batching worked, and the Order Line status update was not missed.", "DEP", orderLineStatus);
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
			var pick1 = new WhsPick(whs, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = startDate.AddDays(1), WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);

			var pickingOrderLines = new WhsDocketLine[1500];
			var pickingOrders = new WhsDocket[50];
			WhsDocket curPickingDocket = null;
			for (var i = 0; i < 1500; i++)
			{
				if(i % 30 == 0)
				{
					curPickingDocket = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "OX" + i) { WD_WP = pick1.PK };
					pickingOrders[i / 30] = curPickingDocket;
				}
				pickingOrderLines[i] = new WhsDocketLine(curPickingDocket, product.PK, 1m);
			}
			sql.AppendLine(WhsDocket.GetBulkInsertStatement(pickingOrders));
			sql.AppendLine(WhsDocketLine.GetBulkInsertStatement(pickingOrderLines));

			var departedOrderLines = new WhsDocketLine[2000];
			var departedOrders = new WhsDocket[40];
			WhsDocket curDepartedDocket = null;
			for (var i = 0; i < 2000; i++)
			{
				if (i % 50 == 0)
				{
					curDepartedDocket = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "OY" + i) { WD_WP = pick2.PK, WD_FinalisedDate = startDate.AddDays(1), WD_GS_NKFinalizedBy = "T" };
					departedOrders[i / 50] = curDepartedDocket;
				}
				departedOrderLines[i] = new WhsDocketLine(curDepartedDocket, product.PK, 2m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = startDate.AddDays(1) };
			}
			sql.AppendLine(WhsDocket.GetBulkInsertStatement(departedOrders));
			sql.AppendLine(WhsDocketLine.GetBulkInsertStatement(departedOrderLines));
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var departedCount = TestConnection.ExecuteScalar($"SELECT COUNT(WE_PK) FROM WhsDocketLine WHERE {TemporaryColumnName} = 'DEP'");
			var pickingCount = TestConnection.ExecuteScalar($"SELECT COUNT(WE_PK) FROM WhsDocketLine WHERE {TemporaryColumnName} =''");
			AssertEquals(2000, departedCount);
			AssertEquals(1500, pickingCount);
		}

		public void TestNoDocketLineNeedsToBeTransformed()
		{
			var column = WhsDocketLineSchema.WE_DocketLineStatus;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {TemporaryColumnName} should be added even if nothing was transformed.", true, ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));
			AssertEquals("Status should be Finished when OnlinePreUpgrade is complete.", bool.TrueString, ExtProperty.Database.Select(TestConnection, TransformationHasRunToCompletion));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			RunColumnSynchroniser();

			AssertEquals($"Temp Column {TemporaryColumnName} should get renamed even if nothing was transformed.", false, ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));
		}

		public void TestTempColumnIsCreated()
		{
			PrepareTestData();

			var column = WhsDocketLineSchema.WE_DocketLineStatus;
			var transform = GetNewTestTransformationInstance();

			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(CountRowsToConvertSQL);
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRowsToConvert > 0);

			using (TestConnection.TrackExecutedCommands())
			{
				transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			}

			var tableDescriptor = new TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {TemporaryColumnName} should get created.", true, ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));
			AssertEquals($"Temp Column {TemporaryColumnName} should be created with correct varchar type.", "varchar", GetColumnType(TestConnection, column.TableName, TemporaryColumnName));
			AssertEquals($"Temp Column {TemporaryColumnName} should be created with correct varchar precision.", 3, GetExtendedTableColumns(TestConnection, column.TableName).First(c => c.ColumnName == TemporaryColumnName).CharacterMaximumLength);

			var defaultConstraintTemporaryColumnName = GenerateDefaultColumnConstraintName(column.TableName, TemporaryColumnName);
			AssertConstraintExists(tableDescriptor, defaultConstraintTemporaryColumnName, expectedToExists: true);

			AssertEquals("All rows should have been converted or untouched.", countOfRowsToConvert, TestConnection.ExecuteScalar(CountConvertedRowsSQL));

			var countOfAutoVersion = $@"
SELECT COUNT(*)
FROM WhsDocketLine
WHERE
	{TemporaryColumnName} = 'DEP'
	AND WE_AutoVersion = 1";
			AssertEquals("Rows should have been updated once.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfAutoVersion));

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals($"Temp Column {TemporaryColumnName} should still be created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));
			AssertEquals($"Temp Column {TemporaryColumnName} should still have correct varchar type.", "varchar", DbObjectCreator.GetColumnType(TestConnection, column.TableName, TemporaryColumnName));

			AssertEquals("Rows should have been updated once.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfAutoVersion));// Due to TransformationHasRunToCompletion flag

			RunColumnSynchroniser();

			AssertEquals("All rows that needed conversion have the Status Column Populated.", countOfRowsToConvert, TestConnection.ExecuteScalar(CountRowsToConvertSQL));
			AssertEquals($"Temp Column {TemporaryColumnName} should no longer exist.", false, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));
			AssertEquals($"Existing Column {column.Name} should have correct varchar type.", "varchar", DbObjectCreator.GetColumnType(TestConnection, column.TableName, column.Name));
			AssertEquals($"Existing Column {column.Name} should be created with correct varchar precision.", 3, GetExtendedTableColumns(TestConnection, column.TableName).First(c => c.ColumnName == column.Name).CharacterMaximumLength);

			AssertConstraintExists(tableDescriptor, defaultConstraintTemporaryColumnName, expectedToExists: false);

			void AssertConstraintExists(TableDescriptor tableDescriptor, string defaultConstraintName, bool expectedToExists)
			{
				var constraintExistsSQL = $"SELECT COUNT(*) WHERE OBJECT_ID('{tableDescriptor.TableSchema}.{defaultConstraintName}', 'D') IS NOT NULL";
				var constraintExists = (int)TestConnection.ExecuteScalar(constraintExistsSQL);
				AssertEquals($"Default Constraint should {(expectedToExists ? "" : "*not* ")}exist.", expectedToExists ? 1 : 0, constraintExists);
			}
		}

		public void TestDefaultValueForTempColumnIsCreatedCorrectly()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var today = new DateTime(2024, 10, 17);
			var pick1 = new WhsPick(whs, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);

			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 2m).AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O2") { WD_WP = pick2.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order2, product.PK, 3m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var orderLine3 = new WhsDocketLine(order2, product.PK, 4m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var orderLine4 = new WhsDocketLine(order2, product.PK, 5m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var orderLine5 = new WhsDocketLine(order2, product.PK, 6m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
			sql.Clear();

			var mostCommonValue = DataUtils.GetTheMostPopularValueForTheColumn(Db.Connection, Db.DatabaseName, WhsDocketLineSchema.Constants.SqlSchemaName, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.WE_DocketLineStatus.Name);
			AssertEquals("Precondition: Most common value for temp column should be FIN", "FIN", mostCommonValue);

			var transform = GetNewTestTransformationInstance();
			using (TestConnection.TrackExecutedCommands())
			{
				transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			}

			var column = WhsDocketLineSchema.WE_DocketLineStatus;
			var tableDescriptor = new TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			var defaultConstraintTemporaryColumnName = DbObjectCreator.GenerateDefaultColumnConstraintName(column.TableName, TemporaryColumnName);

			AssertEquals("Default Constraint Definition should be correct.", "('FIN')",
				TestConnection.ExecuteScalar($"SELECT definition FROM sys.default_constraints WHERE name = '{defaultConstraintTemporaryColumnName}'"));

			TestConnection.ExecuteNonQuery($@"DROP TRIGGER {WhsDocketLineSyncTriggerName}");
			TestConnection.ExecuteNonQuery($@"DROP TRIGGER {WhsPickSyncTriggerName}");

			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O3").AppendInsertAndReturnObject(sql);
			var orderLine6 = new WhsDocketLine(order3, product.PK, 1m) { WE_DocketLineStatus = "" }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var actualValue = (string)TestConnection.ExecuteScalar($@"SELECT {TemporaryColumnName} FROM dbo.WhsDocketLine where WE_PK = '{orderLine6.PK}' ");
			AssertEquals("Should populate temp column with most common value", "FIN", actualValue);
		}

		public void TestWhsDocketLineSyncTriggerIsCreatedCorrectly()
		{
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSyncTriggerName));
			PrepareTestData();
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(CountRowsToConvertSQL);
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRowsToConvert > 0);
			AssertEquals("All rows should have been populated with status column from Trigger.", countOfRowsToConvert, TestConnection.ExecuteScalar(CountConvertedRowsSQL));
		}

		public void TestWhsDocketLineSyncTriggerIsCreatedCorrectly_HandlesEntriesWithNullAuditData()
		{
			var column = WhsDocketSchema.WD_DocketStatus;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSyncTriggerName));

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
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1") { WD_WP = pick1.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 2m)
			{
				WE_DocketLineStatus = "FIN",
				WE_FinalisedDate = today,
				WE_SystemLastEditTimeUtc = null,
				WE_SystemLastEditUser = "",
			}.AppendInsertAndReturnObject(sql);
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsDocketLine_AuditDetailsAreNotMissing_Insert", WhsDocketLineSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsDocketLine_AuditDetailsAreNotMissing_Update", WhsDocketLineSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var countOfRowsWithAuditDataSet = $@"
SELECT COUNT(*)
FROM WhsDocketLine
WHERE
	{TemporaryColumnName} = 'DEP'
	AND WE_SystemLastEditTimeUtc > DATEADD(day, -1, GetUtcDate())
	AND WE_SystemLastEditUser = '~BP'";

			AssertEquals("Trigger should update docket.", 1, TestConnection.ExecuteScalar(countOfRowsWithAuditDataSet));
		}

		public void TestWhsPickSyncTriggerIsCreatedCorrectly()
		{
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsPickSchema.Constants.TableName, WhsPickSyncTriggerName));

			PrepareTestData();
			var countOfConvertedRows = (int)TestConnection.ExecuteScalar(CountConvertedRowsSQL);

			var columnsPendingPickFinalisationSQL = $@"
SELECT
	COUNT(*)
FROM
	WhsDocketLine
	JOIN WhsDocket ON WE_WD = WD_PK
	JOIN WhsPick ON WD_WP = WP_PK
WHERE
	WD_DocketType = 'ORD'
	AND WP_FinalizedDateUtc IS NULL
";

			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(columnsPendingPickFinalisationSQL);
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRowsToConvert > 0);

			var today = new DateTime(2024, 11, 11);
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_PreventMismatchOnDocketStatusAndDateWithLines", WhsDocketSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			{
				WhsDocket.UpdateWhere(d => d.WD_DocketType == "ORD" && d.WD_FinalisedDate == null)
					.Set(d => d.WD_DocketStatus, "DEP")
					.Set(d => d.WD_FinalisedDate, today)
					.Set(d => d.WD_GS_NKFinalizedBy, "A")
					.Set(d => d.WD_SystemLastEditTimeUtc, today)
					.Set(d => d.WD_SystemLastEditUser, "~BP")
					.Post(TestConnection);
				WhsPick.UpdateWhere(p => p.WP_FinalizedDateUtc == null)
					.Set(p => p.WP_PickStatus, "FIN")
					.Set(p => p.WP_FinalizedDateUtc, today)
					.Set(p => p.WP_GS_NKFinalizedBy, "A")
					.Set(p => p.WP_SystemCreateTimeUtc, today)
					.Set(p => p.WP_SystemLastEditUser, "~BP")
					.Post(TestConnection);
			}
			Assert("Trigger should update docket.", countOfConvertedRows > 0);
			AssertEquals("Trigger should update docket.", countOfConvertedRows + countOfRowsToConvert, TestConnection.ExecuteScalar(CountConvertedRowsSQL));
		}

		public void TestWhsPickSyncTriggerIsCreatedCorrectly_HandlesEntriesWithNullAuditData()
		{
			var column = WhsDocketLineSchema.WE_DocketLineStatus;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsPickSchema.Constants.TableName, WhsPickSyncTriggerName));

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
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1") { WD_WP = pick1.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 2m)
			{
				WE_DocketLineStatus = "FIN",
				WE_FinalisedDate = today,
				WE_SystemLastEditTimeUtc = null,
				WE_SystemLastEditUser = "",
			}.AppendInsertAndReturnObject(sql);
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsDocketLine_AuditDetailsAreNotMissing_Insert", WhsDocketLineSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsDocketLine_AuditDetailsAreNotMissing_Update", WhsDocketLineSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_PreventMismatchOnDocketStatusAndDateWithLines", WhsDocketSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			{
				WhsDocket.UpdateWhere(d => d.WD_DocketType == "ORD" && d.WD_FinalisedDate == null)
					.Set(d => d.WD_DocketStatus, "DEP")
					.Set(d => d.WD_FinalisedDate, today)
					.Set(d => d.WD_GS_NKFinalizedBy, "A")
					.Set(d => d.WD_SystemLastEditTimeUtc, today)
					.Set(d => d.WD_SystemLastEditUser, "~BP")
					.Post(TestConnection);
				WhsPick.UpdateWhere(p => p.WP_FinalizedDateUtc == null)
					.Set(p => p.WP_PickStatus, "FIN")
					.Set(p => p.WP_FinalizedDateUtc, today)
					.Set(p => p.WP_GS_NKFinalizedBy, "A")
					.Set(p => p.WP_SystemCreateTimeUtc, today)
					.Set(p => p.WP_SystemLastEditUser, "~BP")
					.Post(TestConnection);
			}

			var countOfRowsWithAuditDataSet = $@"
SELECT COUNT(*)
FROM WhsDocketLine
WHERE
	{TemporaryColumnName} = 'DEP'
	AND WE_SystemLastEditTimeUtc > DATEADD(day, -1, GetUtcDate())
	AND WE_SystemLastEditUser = '~BP'";

			var totalUpdated = (int)TestConnection.ExecuteScalar(countOfRowsWithAuditDataSet);
			AssertEquals("Trigger should update docket.", 1, TestConnection.ExecuteScalar(countOfRowsWithAuditDataSet));
		}

		public void TestTransformSetsLastEditTimeAndUser()
		{
			var column = WhsDocketLineSchema.WE_DocketLineStatus;
			PrepareTestData();

			var countOfRows = WhsDocketLine.CountInDB(TestConnection);
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRows > 0);
			var today = new DateTime(2024, 1, 1);
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			{
				WhsDocketLine.UpdateWhere(d => true)
					.Set(d => d.WE_SystemLastEditTimeUtc, today)
					.Set(d => d.WE_SystemLastEditUser, "ZZZ")
					.Post(TestConnection);
			}
			var timeColumName = WhsDocketLineSchema.WE_SystemLastEditTimeUtc.Name;
			var userColumName = WhsDocketLineSchema.WE_SystemLastEditUser.Name;

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countOfRowsWithUpdatedTimeAndUserSql = $@"
SELECT COUNT(*)
FROM WhsDocketLine
WHERE
	{TemporaryColumnName} = 'DEP'
	AND {timeColumName} > DATEADD(day, -1, GetUtcDate())
	AND {userColumName} = '~BP'";

			var countOfRowsNotUpdated = $@"
SELECT COUNT(*)
FROM WhsDocketLine
WHERE
	{TemporaryColumnName} <> 'DEP'
	AND {timeColumName} = '2024/01/01'
	AND {userColumName} = 'ZZZ'";

			var totalUpdated = (int)TestConnection.ExecuteScalar(countOfRowsWithUpdatedTimeAndUserSql);
			var totalUnchanged = (int)TestConnection.ExecuteScalar(countOfRowsNotUpdated);
			AssertEquals("All columns updated or unchanged", countOfRows, totalUpdated + totalUnchanged);
			Assert("There should be some updated records", totalUpdated > 0);
			Assert("There should be some unchanged records", totalUnchanged > 0);
		}

		public void TestTransformSetsLastEditTimeAndUser_EvenWhenEmpty()
		{
			var column = WhsDocketLineSchema.WE_DocketLineStatus;
			PrepareTestData();

			var countOfRows = WhsDocketLine.CountInDB(TestConnection);
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRows > 0);

			var timeColumName = WhsDocketLineSchema.WE_SystemLastEditTimeUtc.Name;
			var userColumName = WhsDocketLineSchema.WE_SystemLastEditUser.Name;
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsDocketLine_AuditDetailsAreNotMissing_Update", WhsDocketLineSchema.Constants.TableName))
			{
				WhsDocketLine.UpdateWhere(d => true)
					.Set(d => d.WE_SystemLastEditTimeUtc, null)
					.Set(d => d.WE_SystemLastEditUser, "")
					.Post(TestConnection);
			}

			var countOfRowsWithNullDate = WhsDocketLine.CountInDB(TestConnection, d => d.WE_SystemLastEditTimeUtc == null);
			AssertEquals("Precondition: We have some Status Column's to convert.", true, countOfRows > 0);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countOfRowsWithAuditDataSet = $@"
SELECT COUNT(*)
FROM WhsDocketLine
WHERE
	{TemporaryColumnName} = 'DEP'
	AND {timeColumName} > DATEADD(day, -1, GetUtcDate())
	AND {userColumName} = '~BP'";

			var countOfRowsNotUpdated = $@"
SELECT COUNT(*)
FROM WhsDocketLine
WHERE
	{TemporaryColumnName} = WE_DocketLineStatus
	OR
	(
	{TemporaryColumnName} <> 'DEP'
	AND {timeColumName} > DATEADD(day, -1, GetUtcDate())
	AND {userColumName} = '~BP'
	)";// If TemporaryColumnName = WE_DocketLineStatus, we won't update.

			var totalUpdated = (int)TestConnection.ExecuteScalar(countOfRowsWithAuditDataSet);
			var totalUnchanged = (int)TestConnection.ExecuteScalar(countOfRowsNotUpdated);
			AssertEquals("All columns updated or unchanged", countOfRows, totalUpdated + totalUnchanged);
			Assert("There should be some updated records", totalUpdated > 0);
			Assert("There should be some unchanged records", totalUnchanged > 0);
		}

		public void TestExtendedProperty_LastProcessedPKWorksProperly()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var today = new DateTime(2024, 10, 17);

			var pick1 = new WhsPick(whs, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick3 = new WhsPick(whs, "P3", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);

			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O2") { WD_WP = pick2.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order2, product.PK, 2m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var orderLine3 = new WhsDocketLine(order2, product.PK, 3m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O3") { WD_WP = pick2.PK, WD_FinalisedDate = today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var orderLine4 = new WhsDocketLine(order3, product.PK, 4m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var orderLine5 = new WhsDocketLine(order3, product.PK, 5m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var orderLine6 = new WhsDocketLine(order3, product.PK, 6m) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var order4 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O4").AppendInsertAndReturnObject(sql);
			var orderLine7 = new WhsDocketLine(order4, product.PK, 7m) { WE_DocketLineStatus = "" }.AppendInsertAndReturnObject(sql);

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var minPK = (Guid)TestConnection.ExecuteScalar("SELECT MIN(WD_PK) FROM WhsDocket where WD_DocketType = 'ORD' AND WD_DocketStatus = 'DEP'");
			ExtProperty.Database.Update(TestConnection, ExtPropertiesAndTempTriggersClearedForNewRun, bool.TrueString);
			ExtProperty.Database.Update(TestConnection, LastProcessedChunkPKName, minPK.ToString());

			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(CountRowsToConvertSQL);
			AssertEquals("Precondition: We have some Status Column's to convert.", 5, countOfRowsToConvert);
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countOfConvertedRows = (int)TestConnection.ExecuteScalar(CountConvertedRowsSQL);
			Assert("Should convert some docket lines.", countOfConvertedRows > 0);
			Assert("Should convert less docket lines due to LastProcessedPK.", countOfConvertedRows < countOfRowsToConvert);
			AssertEquals("Extended property 'LastProcessedChunkPKName' should be removed once Transform is complete.", null, ExtProperty.Database.Select(TestConnection, LastProcessedChunkPKName));
			AssertEquals("Extended property 'TransformationHasRunToCompletion' should be set to be true once Transform is complete.", bool.TrueString, ExtProperty.Database.Select(TestConnection, TransformationHasRunToCompletion));
		}

		public void TestExtendedProperty_HandleExtendedPropertiesProperly()
		{
			PrepareTestData();

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Extended property 'LastProcessedChunkPKName' should be removed once Transform is complete.", null, ExtProperty.Database.Select(TestConnection, LastProcessedChunkPKName));
			AssertEquals("Extended property 'TransformationHasRunToCompletion' should be set to be true once Transform is complete.", bool.TrueString, ExtProperty.Database.Select(TestConnection, TransformationHasRunToCompletion));
			AssertEquals("Extended property 'ExtPropertiesAndTempTriggersClearedForNewRun' should be set to be true when Transform is complete.", bool.TrueString, ExtProperty.Database.Select(TestConnection, ExtPropertiesAndTempTriggersClearedForNewRun));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("Extended property 'TransformationHasRunToCompletion' should be removed after OfflinePreUpgrade.", null, ExtProperty.Database.Select(TestConnection, TransformationHasRunToCompletion));
			AssertEquals("Extended property 'ExtPropertiesAndTempTriggersClearedForNewRun' should be removed after OfflinePreUpgrade.", null, ExtProperty.Database.Select(TestConnection, ExtPropertiesAndTempTriggersClearedForNewRun));
		}

		public void TestExtendedProperty_WhenAlreadySetToFinished()
		{
			ExtProperty.Database.Update(TestConnection, ExtPropertiesAndTempTriggersClearedForNewRun, bool.TrueString);
			ExtProperty.Database.Update(TestConnection, TransformationHasRunToCompletion, bool.TrueString);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var column = WhsDocketLineSchema.WE_DocketLineStatus;
			var tableDescriptor = new TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {TemporaryColumnName} will not be added as transform has finished.", false, ColumnExists(TestConnection, tableDescriptor, TemporaryColumnName));
			AssertEquals("Status should remain Finished when OnlinePreUpgrade is complete.", bool.TrueString, ExtProperty.Database.Select(TestConnection, ExtPropertiesAndTempTriggersClearedForNewRun));
			AssertEquals("Status should remain Finished when OnlinePreUpgrade is complete.", bool.TrueString, ExtProperty.Database.Select(TestConnection, TransformationHasRunToCompletion));
		}

		public void TestClearAllOldExtPropertiesAndTriggers_NeverRunThisTransformBefore()
		{
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Extended property 'ExtPropertiesAndTempTriggersClearedForNewRun' should be set to be true during the online upgrade.", bool.TrueString, ExtProperty.Database.Select(TestConnection, ExtPropertiesAndTempTriggersClearedForNewRun));
			AssertEquals("Trigger should be created.", true, TriggerExists(TestConnection, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSyncTriggerName));
			AssertEquals("Trigger Definition is correct", NewWhsDocketLineSyncTrigger, GetTriggerDefinition(TestConnection, WhsDocketLineSyncTriggerName));
		}

		public void TestClearAllOldExtPropertiesAndTriggers_NewTransformRun()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var today = new DateTime(2024, 10, 17);

			var pick1 = new WhsPick(whs, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "P2", "PIS").AppendInsertAndReturnObject(sql);

			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 1m).AppendInsertAndReturnObject(sql);

			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O2") { WD_WP = pick2.PK }.AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order1, product.PK, 2m).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var orderLine1InDB = WhsDocketLine.ShallowLoadFromDB(TestConnection, d => d.WE_TransactionQuantity == 1m).Single();

			orderLine1InDB
				.BuildAssertion(TestConnection)
				.ExpectEquals("Because pick status is NOT FIN, its docket line status does NOT need to be changed.", d => d.WE_DocketLineStatus, "")
				.ExpectHasNoLogs()
				.VerifyAll();

			ExtProperty.Database.Delete(Db.Connection, ExtPropertiesAndTempTriggersClearedForNewRun); // Hack
			ExtProperty.Database.Update(TestConnection, LastProcessedChunkPKName, Guid.NewGuid().ToString());
			DropTriggerIfExists(Db.Connection, WhsDocketLineSyncTriggerName);
			Db.Connection.ExecuteNonQuery(OldWhsDocketLineSyncTrigger);
			AssertEquals("Precondition: Old Trigger Definition", OldWhsDocketLineSyncTrigger, GetTriggerDefinition(TestConnection, WhsDocketLineSyncTriggerName));

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
			{
				WhsDocketLine.UpdateWhere(d => d.WE_TransactionQuantity == 1m)
					.Set(d => d.WE_DocketLineStatus, "FIN")
					.Set(d => d.WE_FinalisedDate, today)
					.Set(d => d.WE_SystemLastEditTimeUtc, today)
					.Set(d => d.WE_SystemLastEditUser, "~BP")
					.Post(TestConnection);
			}
			var tempColumnValueForLine1 = (string)TestConnection.ExecuteScalar($@"SELECT {TemporaryColumnName} FROM dbo.WhsDocketLine WHERE WE_PK = '{orderLine1InDB.PK}'");
			AssertEquals("Precondition: Old trigger could not work for updating docket line status", "", tempColumnValueForLine1);

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(bool.TrueString, ExtProperty.Database.Select(TestConnection, TransformationHasRunToCompletion));
			AssertEquals(bool.TrueString, ExtProperty.Database.Select(TestConnection, ExtPropertiesAndTempTriggersClearedForNewRun));
			AssertNull(ExtProperty.Database.Select(TestConnection, LastProcessedChunkPKName));
			AssertEquals("Docket line trigger should be created.", true, TriggerExists(TestConnection, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSyncTriggerName));
			AssertEquals("Docket line trigger's definition is correct", NewWhsDocketLineSyncTrigger, GetTriggerDefinition(TestConnection, WhsDocketLineSyncTriggerName));
			AssertEquals("Pick trigger should also be created.", true, TriggerExists(TestConnection, WhsPickSchema.Constants.TableName, WhsPickSyncTriggerName));

			tempColumnValueForLine1 = (string)TestConnection.ExecuteScalar($@"SELECT {TemporaryColumnName} FROM dbo.WhsDocketLine WHERE WE_PK = '{orderLine1InDB.PK}'");
			AssertEquals("New trigger should work for updating docket line status", "FIN", tempColumnValueForLine1);
		}

		public void TestClearAllOldExtPropertiesAndTriggers_ClearedFlagShouldWork()
		{
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(bool.TrueString, ExtProperty.Database.Select(TestConnection, ExtPropertiesAndTempTriggersClearedForNewRun));
			DropTriggerIfExists(Db.Connection, WhsDocketLineSyncTriggerName); // Hack
			Db.Connection.ExecuteNonQuery(OldWhsDocketLineSyncTrigger);
			ExtProperty.Database.Delete(TestConnection, TransformationHasRunToCompletion);

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(bool.TrueString, ExtProperty.Database.Select(TestConnection, TransformationHasRunToCompletion));
			AssertEquals(bool.TrueString, ExtProperty.Database.Select(TestConnection, ExtPropertiesAndTempTriggersClearedForNewRun));
			AssertEquals("Docket line trigger's definition should still be the old one due to the cleared flag", OldWhsDocketLineSyncTrigger, GetTriggerDefinition(TestConnection, WhsDocketLineSyncTriggerName));
		}

		#region Implementation

		string OldWhsDocketLineSyncTrigger => $@"
CREATE TRIGGER dbo.{WhsDocketLineSyncTriggerName}
	ON dbo.WhsDocketLine
	AFTER INSERT
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN
		UPDATE tab
		SET
			tab.{TemporaryColumnName} = CASE WHEN NewStatus = 'DEP' THEN NewStatus ELSE tab.WE_DocketLineStatus END,
			tab.WE_SystemLastEditTimeUtc = ISNULL(tab.WE_SystemLastEditTimeUtc, GetUtcDate()),
			tab.WE_SystemLastEditUser = CASE WHEN tab.WE_SystemLastEditUser = '' THEN '~BP' ELSE tab.WE_SystemLastEditUser END
		FROM
			dbo.WhsDocketLine AS tab
			JOIN dbo.WhsDocket ON tab.WE_WD = WD_PK
			JOIN inserted on tab.WE_PK = inserted.WE_PK
			OUTER APPLY
			(
				SELECT
					CASE WHEN tab.WE_DocketLineType = 'ORD' THEN 'DEP' ELSE tab.WE_DocketLineStatus END NewStatus
				FROM
					dbo.WhsPick
				WHERE
					WP_PK = WD_WP
					AND WP_FinalizedDateUtc IS NOT NULL
			) IsDeparted
	END
END";
		string NewWhsDocketLineSyncTrigger => $@"
CREATE TRIGGER dbo.{WhsDocketLineSyncTriggerName}
	ON dbo.WhsDocketLine
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF UPDATE(WE_DocketLineStatus)
	BEGIN
		UPDATE tab
		SET
			tab.{TemporaryColumnName} = CASE WHEN NewStatus = 'DEP' THEN NewStatus ELSE tab.WE_DocketLineStatus END,
			tab.WE_SystemLastEditTimeUtc = ISNULL(tab.WE_SystemLastEditTimeUtc, GetUtcDate()),
			tab.WE_SystemLastEditUser = CASE WHEN tab.WE_SystemLastEditUser = '' THEN '~BP' ELSE tab.WE_SystemLastEditUser END
		FROM
			dbo.WhsDocketLine AS tab
			JOIN dbo.WhsDocket ON tab.WE_WD = WD_PK
			JOIN inserted on tab.WE_PK = inserted.WE_PK
			OUTER APPLY
			(
				SELECT
					CASE WHEN tab.WE_DocketLineType = 'ORD' THEN 'DEP' ELSE tab.WE_DocketLineStatus END NewStatus
				FROM
					dbo.WhsPick
				WHERE
					WP_PK = WD_WP
					AND WP_FinalizedDateUtc IS NOT NULL
			) IsDeparted
	END
END";

		string CountRowsToConvertSQL => @"
SELECT COUNT(*)
FROM
	WhsDocketLine
	JOIN WhsDocket ON WE_WD = WD_PK
	JOIN WhsPick ON WD_WP = WP_PK
WHERE
	WD_DocketType = 'ORD'
	AND WP_FinalizedDateUtc IS NOT NULL";

		string CountConvertedRowsSQL => $@"
SELECT COUNT(*)
FROM WhsDocketLine
WHERE
	{TemporaryColumnName} = 'DEP'";

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateWhsDocketLineSetStatusDepartedOnPickFinalisation();

		protected override void RunTransformation()
		{
			var transform = GetNewTestTransformationInstance();
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
