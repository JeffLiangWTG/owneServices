using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventOverfillLocationWithUnitsCapacity))]
	class TG_PreventOverfillLocationWithUnitsCapacityTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_PreventOverfillLocationWithUnitsCapacityTest : TestCase
	{
		#region TestTriggerPreventOverfillLocationWithMaxQuantityLimit

		[UseSnapshotProtection]
		public void TestTriggerPreventOverfillLocationWithMaxQuantityLimit()
		{
			using (var con1 = Db.NewExtraConnectionToMainDb())
			{
				// set up data
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(con1);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(con1);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row1 = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK) { WL_MaxQuantity = 10m, WL_MaxQuantityUnit = "UNT", WL_Column = 1 }.AppendInsertAndReturnObject(sql);
				var locationA2 = new WhsLocation(row1.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var now = DateTime.Now;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, product.PK, 9m, locationA1.PK) { WE_StockOnHand = 9m }.AppendInsertAndReturnObject(sql);

				var pickForUnloadReceive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "PUT", "R2").AppendInsertAndReturnObject(sql);
				var pickForUnloadLine = new WhsDocketLine(pickForUnloadReceive, product.PK, 9m, locationA1.PK) { WE_DocketLineStatus = "PFU", WE_OriginalInventoryStatus = "PUT", WE_CurrentInventoryStatus = "PUT", WE_StockOnHand = 0m }.AppendInsertAndReturnObject(sql);

				var putawayTransfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "HFT", "T1") { WD_IsPutawayTransfer = true }.AppendInsertAndReturnObject(sql);
				var putawayTransferLine = new WhsDocketLine(putawayTransfer, product.PK, 9m, locationA2.PK)
				{
					WE_StockOnHand = 9m,
					WE_DocketLineStatus = "HFT",
					WE_WL_TransferFrom = locationA1.PK,
					WE_OriginalInventoryStatus = "PTA",
					WE_CurrentInventoryStatus = "PTA",
					WE_AdjustmentArrivalDate = now
				}.AppendInsertAndReturnObject(sql);
				new WhsPickLine(pickForUnloadLine, putawayTransferLine, 9m) { WZ_PickedDateTime = now, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(sql);

				SaveDataToDB(con1, sql);

				WhsDocketLine receiveLine2;
				using (var firstConnection = Db.NewExtraConnectionToMainDb())
				{
					var sql2 = new SqlQueryBuilder();
					var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R3") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(sql2);
					receiveLine2 = new WhsDocketLine(receive, product.PK, 5m, locationA1.PK) { WE_StockOnHand = 5m }.AppendInsertAndReturnObject(sql2);

					using (var command = firstConnection.Command(sql2.ToStringWithNewLineBetweenAppends()))
					{
						AssertExceptionThrown("Attempt to overflow the location unit capacity.", typeof(SqlException), () => command.ExecuteNonQuery());
					}
				}

				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, receiveLine.PK, locationA1.PK, 9));
				AssertEquals(false, AssertDocketLineWithLocationAndUnits(con1, receiveLine2.PK, locationA1.PK, 5));
			}
		}

		[UseSnapshotProtection]
		public void TestTriggerPreventOverfillLocationWithMaxQuantityLimit_DoesNotTrigerForCancelledReceives()
		{
			using (var con1 = Db.NewExtraConnectionToMainDb())
			{
				// set up data
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(con1);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(con1);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK) { WL_MaxQuantity = 10m, WL_MaxQuantityUnit = "UNT" }.AppendInsertAndReturnObject(sql);

				var areaDDL = new WhsArea(whs.PK, "DDLArea").AppendInsertAndReturnObject(sql);
				var rowDDL = new WhsRow(whs, "DDL").AppendInsertAndReturnObject(sql);
				var ddlLocationType = WhsLocationType.ShallowLoadFromDB(con1, lt => lt.WLT_LocationClass == "DDL").First();
				var dockDoorLocation = new WhsLocation(rowDDL.PK, areaDDL.PK, areaDDL.PK, ddlLocationType.PK) { WL_MaxQuantity = 10m, WL_MaxQuantityUnit = "UNT" }.AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var receiveOnNormalLocation = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTime.Now }.AppendInsertAndReturnObject(sql);
				var receiveLineOnNormalLocation = new WhsDocketLine(receiveOnNormalLocation, product.PK, 10m, locationA1.PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var receiveOnDDLocation = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R2").AppendInsertAndReturnObject(sql);
				var receiveLineOnDDLocation = new WhsDocketLine(receiveOnDDLocation, product.PK, 10m, dockDoorLocation.PK)
				{
					WE_StockOnHand = 10m,
					WE_OriginalInventoryStatus = "REC",
					WE_CurrentInventoryStatus = "REC",
					WE_UnloadedTime = DateTimeOffset.Now,
					WE_GS_NKUnloadedBy = "A"
				}.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(con1, sql.ToStringWithNewLineBetweenAppends());

				WhsDocketLine cancelledReceiveLineOnNormalLocation;
				WhsDocketLine cancelledReceiveLineOnDDLocation;
				using (var firstConnection = Db.NewExtraConnectionToMainDb())
				{
					var sql2 = new SqlQueryBuilder();

					// not possible functionally but possible if data is inserted directly to DB
					var cancelledReceiveOnNormalLocation = new WhsDocket(client.PK, whs.PK, "INW", "REC", "CAN", "R3") { WD_GS_NKCanceledBy = "~BP", WD_CanceledTimeUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(sql2);
					cancelledReceiveLineOnNormalLocation = new WhsDocketLine(cancelledReceiveOnNormalLocation, product.PK, 5m, locationA1.PK)
					{
						WE_OriginalInventoryStatus = "PUT",
						WE_CurrentInventoryStatus = "PUT",
						WE_StockOnHand = 0m,
						WE_DocketLineStatus = "CAN"
					}.AppendInsertAndReturnObject(sql2);

					var cancelledReceiveOnDDLocation = new WhsDocket(client.PK, whs.PK, "INW", "REC", "CAN", "R4") { WD_GS_NKCanceledBy = "~BP", WD_CanceledTimeUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(sql2);
					cancelledReceiveLineOnDDLocation = new WhsDocketLine(cancelledReceiveOnDDLocation, product.PK, 5m, dockDoorLocation.PK)
					{
						WE_OriginalInventoryStatus = "REC",
						WE_CurrentInventoryStatus = "REC",
						WE_StockOnHand = 0m,
						WE_DocketLineStatus = "CAN",
						WE_UnloadedTime = DateTimeOffset.Now,
						WE_GS_NKUnloadedBy = "A"
					}.AppendInsertAndReturnObject(sql2);

					AssertNoExceptionThrown("No attempt to overflow the location unit capacity exception as new receives are cancelled.",
						() => ExecuteSqlInTransaction(firstConnection, sql2.ToStringWithNewLineBetweenAppends()));
				}

				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, receiveLineOnNormalLocation.PK, locationA1.PK, 10));
				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, receiveLineOnDDLocation.PK, dockDoorLocation.PK, 10));
				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, cancelledReceiveLineOnNormalLocation.PK, locationA1.PK, 5));
				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, cancelledReceiveLineOnDDLocation.PK, dockDoorLocation.PK, 5));
			}
		}

		void SaveDataToDB(DbConnection con1, SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.WZ_WE_TransactionLine, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName))
			{
				ExecuteSqlInTransaction(con1, sql.ToStringWithNewLineBetweenAppends());
			}
		}

		bool AssertDocketLineWithLocationAndUnits(DbConnection connection, Guid docketLinePK, Guid locationPK, int units)
		{
			return connection.ExecuteScalar<int>($"SELECT COUNT(WE_PK)FROM dbo.WhsDocketLine WHERE WE_PK = '{docketLinePK}' AND WE_WL = '{locationPK}' AND WE_TransactionQuantity = {units}") == 1;
		}

		#endregion

		#region TestTriggerPreventOverfillLocationWithMaxQuantityLimit_ByUpdatingExistingDocketLineUnit

		[UseSnapshotProtection]
		public void TestTriggerPreventOverfillLocationWithMaxQuantityLimit_ByUpdatingExistingDocketLineUnit_UnfinalisedDocketLine()
		{
			AsssertTriggerPreventOverfillLocationWithMaxQuantityLimit_ByUpdatingExistingDocketLineUnit(finalised: false);
		}

		[UseSnapshotProtection]
		public void TestTriggerPreventOverfillLocationWithMaxQuantityLimit_ByUpdatingExistingDocketLineUnit_FinalisedDocketLine()
		{
			AsssertTriggerPreventOverfillLocationWithMaxQuantityLimit_ByUpdatingExistingDocketLineUnit(finalised: true);
		}

		void AsssertTriggerPreventOverfillLocationWithMaxQuantityLimit_ByUpdatingExistingDocketLineUnit(bool finalised)
		{
			using (var con1 = Db.NewExtraConnectionToMainDb())
			{
				// set up data
				var insertDocketLineWith5UnitSql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(con1);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(con1);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(insertDocketLineWith5UnitSql);
				var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(insertDocketLineWith5UnitSql);
				var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK) { WL_MaxQuantity = 10m, WL_MaxQuantityUnit = "UNT" }.AppendInsertAndReturnObject(insertDocketLineWith5UnitSql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(insertDocketLineWith5UnitSql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(insertDocketLineWith5UnitSql);

				var now = DateTime.Now;
				var receive = finalised
					? new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(insertDocketLineWith5UnitSql)
					: new WhsDocket(client.PK, whs.PK, "INW", "REC", "PUT", "R1").AppendInsertAndReturnObject(insertDocketLineWith5UnitSql);

				var receiveLine = finalised
					? new WhsDocketLine(receive, product.PK, 5m, locationA1.PK) { WE_StockOnHand = 5m }.AppendInsertAndReturnObject(insertDocketLineWith5UnitSql)
					: new WhsDocketLine(receive, product.PK, 5m, locationA1.PK) { WE_StockOnHand = 5m, WE_OriginalInventoryStatus = "PUT", WE_CurrentInventoryStatus = "PUT" }.AppendInsertAndReturnObject(insertDocketLineWith5UnitSql);

				ExecuteSqlInTransaction(con1, insertDocketLineWith5UnitSql.ToStringWithNewLineBetweenAppends());

				WhsDocketLine receiveLine2;
				using (var connection1 = Db.NewExtraConnectionToMainDb())
				{
					var insertDocketLineWith4UnitSql = new SqlQueryBuilder();
					var receive2 = finalised
						? new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(insertDocketLineWith4UnitSql)
						: new WhsDocket(client.PK, whs.PK, "INW", "REC", "PUT", "R2").AppendInsertAndReturnObject(insertDocketLineWith4UnitSql);

					receiveLine2 = finalised
						? new WhsDocketLine(receive2, product.PK, 4m, locationA1.PK) { WE_StockOnHand = 4m }.AppendInsertAndReturnObject(insertDocketLineWith4UnitSql)
						: new WhsDocketLine(receive2, product.PK, 4m, locationA1.PK) { WE_StockOnHand = 4m, WE_OriginalInventoryStatus = "PUT", WE_CurrentInventoryStatus = "PUT" }.AppendInsertAndReturnObject(insertDocketLineWith4UnitSql);

					connection1.BeginTransaction();
					connection1.ExecuteNonQuery(insertDocketLineWith4UnitSql.ToStringWithNewLineBetweenAppends());

					var taskUpdateDocket1Units = new Task(() =>
					{
						using (var connection2 = Db.NewExtraConnectionToMainDb())
						{
							var sqlUpdate = finalised ? $"UPDATE dbo.WhsDocketLine SET WE_StockOnHand = 7 WHERE WE_PK = {receiveLine.PK}" : $"UPDATE dbo.WhsDocketLine SET WE_TransactionQuantity = 7 WHERE WE_PK = {receiveLine.PK}";
							using (var command2 = connection2.Command(sqlUpdate))
							{
								AssertExceptionThrown("Attempt to overflow the location unit capacity.", typeof(SqlException), () => command2.ExecuteNonQuery());
							}
						}
					});
					taskUpdateDocket1Units.Start();

					taskUpdateDocket1Units.Wait(1000);
					connection1.CommitTransaction();
					taskUpdateDocket1Units.Wait();
				}

				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, receiveLine.PK, locationA1.PK, 5));
				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, receiveLine2.PK, locationA1.PK, 4));
			}
		}

		#endregion

		#region TestTriggerPreventOverfillLocationWithMaxQuantityLimit_ByConcurrentUsers

		[UseSnapshotProtection]
		public void TestTriggerPreventOverfillLocationWithMaxQuantityLimit_ByConcurrentUsers()
		{
			using (var con1 = Db.NewExtraConnectionToMainDb())
			{
				// set up data
				var insertLocationWith10CapacitySql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(con1);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(con1);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var row2 = new WhsRow(whs, "B").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK) { WL_MaxQuantity = 10m, WL_MaxQuantityUnit = "UNT" }.AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var locationB1 = new WhsLocation(row2.PK, area.PK, area.PK) { WL_MaxQuantity = 10m, WL_MaxQuantityUnit = "UNT" }.AppendInsertAndReturnObject(insertLocationWith10CapacitySql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);

				var now = DateTime.Now;
				using (con1.BeginTransactionWithManager())
				{
					con1.ExecuteNonQuery(insertLocationWith10CapacitySql.ToStringWithNewLineBetweenAppends());

					#region Add Many Locations/Dockets To Prevent Dead-Lock

					/*
					 * Following the implementation of WhsLocationView (Noexpand) this test would fail due a deadlock with TG_WhsDocketLine_LocationIsInCorrectWarehouse.
					 * It was discovered that this test does not fail on a client database due the amount of records in WhsLocation & WhsDocket. 
					 * This prevents SQL from page locking instead of row locking the affected WhsLocations & WhsDockets allowing the tests to run smoothly. 
					 * Thus this section of the test adds enough locations & dockets to allow the test to pass in an empty test database.
					 */
					var docketCount = 250;
					CreateManyRecords(whs, client, now, area, docketCount, con1);
					AssertEquals("Precondition: Db has appropriate WhsDocket count to stop efficient db page locks.", docketCount, con1.ExecuteScalar("select COUNT(*) from dbo.WhsDocket;"));
					con1.CommitTransaction();
				}

				#endregion

				WhsDocketLine receiveLine1 = null;
				WhsDocketLine receiveLine2 = null;
				WhsDocketLine receiveLine3 = null;

				DisablePageLocks(con1);

				// for some reason the following triggers are improperly locking the DB records
				// we disable them for the test so we can test locking of the changed triggers
				TestWhsDataSetupHelper.SuspendTrigger_ForPreUpgradeTransformations(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName);
				TestWhsDataSetupHelper.SuspendTrigger_ForPreUpgradeTransformations(TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsDocketLineSchema.Constants.TableName);

				using (var connection1 = Db.NewExtraConnectionToMainDb())
				{
					var insertDLWithLoc1TenUnits = new SqlQueryBuilder();
					var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(insertDLWithLoc1TenUnits);
					receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, locationA1.PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(insertDLWithLoc1TenUnits);

					connection1.BeginTransaction();
					connection1.ExecuteNonQuery(insertDLWithLoc1TenUnits.ToStringWithNewLineBetweenAppends());      // 1st insert 10 units at location 1

					var taskInsertLocation1WithAnother10Unit = new Task(() =>
					{
						using (var connection2 = Db.NewExtraConnectionToMainDb())
						{
							var anotherInsertDLWithLoc1TenUnits = new SqlQueryBuilder();
							var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(anotherInsertDLWithLoc1TenUnits);
							receiveLine2 = new WhsDocketLine(receive2, product.PK, 10m, locationA1.PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(anotherInsertDLWithLoc1TenUnits);

							using (var command2 = connection2.Command(anotherInsertDLWithLoc1TenUnits.ToStringWithNewLineBetweenAppends()))
							{
								ReduceTimeOutToSpeedUpTest(command2);
								AssertExceptionThrown("Attempt to overflow the location unit capacity.", typeof(SqlException), () => command2.ExecuteNonQuery());
							}
						}
					});
					taskInsertLocation1WithAnother10Unit.Start();

					var taskInsertLocation2With10Unit = new Task(() =>
					{
						using (var connection3 = Db.NewExtraConnectionToMainDb())
						{
							var insertDLWithLoc2TenUnits = new SqlQueryBuilder();
							var receive3 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R3") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(insertDLWithLoc2TenUnits);
							receiveLine3 = new WhsDocketLine(receive3, product.PK, 10m, locationB1.PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(insertDLWithLoc2TenUnits);

							using (connection3.BeginTransactionWithManager())
							using (var command3 = connection3.Command(insertDLWithLoc2TenUnits.ToStringWithNewLineBetweenAppends()))
							{
								ReduceTimeOutToSpeedUpTest(command3);
								AssertNoExceptionThrown("Since insert docket line at a different location, should not be locked.", () =>
								{
									command3.ExecuteNonQuery();
									connection3.CommitTransaction();
								});
							}
						}
					});
					taskInsertLocation2With10Unit.Start();

					taskInsertLocation1WithAnother10Unit.Wait();
					taskInsertLocation2With10Unit.Wait();
					connection1.CommitTransaction();
				}

				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, receiveLine1.PK, locationA1.PK, 10));
				AssertEquals(false, AssertDocketLineWithLocationAndUnits(con1, receiveLine2.PK, locationA1.PK, 10));
				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, receiveLine3.PK, locationB1.PK, 10));
			}
		}

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		void ReduceTimeOutToSpeedUpTest(DbCommand command)
		{
			command.CommandTimeout = 5; // 5 seconds instead of 5 minutes
		}

		void CreateManyRecords(WhsWarehouse warehouse, OrgHeader client, DateTime now, WhsArea area, int count, DbConnection connection)
		{
			var insertManyUnits = new SqlQueryBuilder();
			var row = new WhsRow(warehouse, "Row1") { WR_Columns = (short)count }.AppendInsertAndReturnObject(insertManyUnits);

			for (int i = 1; i <= count; i++)
			{
				new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = (short)i }.AppendInsertAndReturnObject(insertManyUnits);
				new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "FIN", $"W00000{i}") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(insertManyUnits);
			}

			connection.ExecuteNonQuery(insertManyUnits.ToStringWithNewLineBetweenAppends());
		}

		void DisablePageLocks(DbConnection connection)
		{
			// Page locks have caused interesting locking behaviour in our tests in the past.
			connection.ExecuteNonQuery(@"
				ALTER INDEX ALL ON WhsDocketLine SET(ALLOW_PAGE_LOCKS = OFF)

				ALTER INDEX ALL ON WhsDocket SET(ALLOW_PAGE_LOCKS = OFF)

				ALTER INDEX ALL ON WhsLocation SET (ALLOW_PAGE_LOCKS = OFF)");
		}

		#endregion

		#region TestTriggerPreventOverfillLocationWithMaxQuantityLimit_AllowPendingAdjustments

		[UseSnapshotProtection]
		public void TestTriggerPreventOverfillLocationWithMaxQuantityLimit_AllowPendingAdjustments()
		{
			using (var con1 = Db.NewExtraConnectionToMainDb())
			{
				// set up data
				var insertLocationWith10CapacitySql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(con1);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(con1);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK) { WL_MaxQuantity = 10m, WL_MaxQuantityUnit = "UNT" }.AppendInsertAndReturnObject(insertLocationWith10CapacitySql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);

				var now = DateTime.Now;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				WhsDocketLine adjustmentLine;

				ExecuteSqlInTransaction(con1, insertLocationWith10CapacitySql.ToStringWithNewLineBetweenAppends());

				using (var con2 = Db.NewExtraConnectionToMainDb())
				{
					var adjustmentDLWithLoc15Units = new SqlQueryBuilder();
					var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "AD1").AppendInsertAndReturnObject(adjustmentDLWithLoc15Units);
					adjustmentLine = new WhsDocketLine(adjustment, product.PK, 5m, locationA1.PK).AppendInsertAndReturnObject(adjustmentDLWithLoc15Units);

					using (con2.BeginTransactionWithManager())
					using (var command1 = con2.Command(adjustmentDLWithLoc15Units.ToStringWithNewLineBetweenAppends()))
					{
						AssertNoExceptionThrown("Trigger should ignore inserted pending Adjustments.", () =>
						{
							command1.ExecuteNonQuery();
							con2.CommitTransaction();
						});
					}
				}

				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, receiveLine.PK, locationA1.PK, 10));
				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, adjustmentLine.PK, locationA1.PK, 5));
			}
		}

		[UseSnapshotProtection]
		public void TestTriggerPreventOverfillLocationWithMaxQuantityLimit_AllowWithExistingPendingAdjustments()
		{
			using (var con1 = Db.NewExtraConnectionToMainDb())
			{
				// set up data
				var insertLocationWith10CapacitySql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(con1);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(con1);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK) { WL_MaxQuantity = 10m, WL_MaxQuantityUnit = "UNT" }.AppendInsertAndReturnObject(insertLocationWith10CapacitySql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);

				// Create exisiting pending adjustment, this should be iggnored by the trigger.
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "AD1").AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				var adjustmentLine = new WhsDocketLine(adjustment, product.PK, 10m, locationA1.PK).AppendInsertAndReturnObject(insertLocationWith10CapacitySql);
				WhsDocketLine receiveLine;

				AssertNoExceptionThrown("Preconditon: Trigger should ignore inserted pending Adjustments.", () =>
				{
					ExecuteSqlInTransaction(con1, insertLocationWith10CapacitySql.ToStringWithNewLineBetweenAppends());
				});

				using (var con2 = Db.NewExtraConnectionToMainDb())
				{
					var dLWithLoc15Units = new SqlQueryBuilder();
					var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(dLWithLoc15Units);
					receiveLine = new WhsDocketLine(receive, product.PK, 5m, locationA1.PK) { WE_StockOnHand = 5m, WE_OriginalInventoryStatus = "PUT", WE_CurrentInventoryStatus = "PUT" }.AppendInsertAndReturnObject(dLWithLoc15Units);

					using (con2.BeginTransactionWithManager())
					using (var command1 = con2.Command(dLWithLoc15Units.ToStringWithNewLineBetweenAppends()))
					{
						AssertNoExceptionThrown("Trigger should ignore existing pending Adjustments.", () =>
						{
							command1.ExecuteNonQuery();
							con2.CommitTransaction();
						});
					}
				}

				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, adjustmentLine.PK, locationA1.PK, 10));
				AssertEquals(true, AssertDocketLineWithLocationAndUnits(con1, receiveLine.PK, locationA1.PK, 5));
			}
		}
		#endregion
	}
}

