using System;
using System.Linq.Expressions;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.DbUpgrader.Shared.Testing.TestWhsDataSetupHelper;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPickLine_PreventReleaseCapturingIfNotOnAllocatedOrder))]
	class TG_WhsPickLine_PreventReleaseCapturingIfNotOnAllocatedOrderTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_UpdateReleaseCapturingOnAllocatedOrder_AllowUpdate()
		{
			var sql = new SqlQueryBuilder();

			var pickLine = CreateWhsPickLineWithTestData(sql, hasPick: true, isWorkOrder: false);

			SaveDataToDB(sql);

			AssertTriggerUpdate("Changes to Attrib1 should be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedPartAttrib1, "TestAttrib1", TriggerExpectedBehavior.AllowUpdate);
			AssertTriggerUpdate("Changes to Attrib2 should be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedPartAttrib2, "TestAttrib2", TriggerExpectedBehavior.AllowUpdate);
			AssertTriggerUpdate("Changes to Attrib3 should be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedPartAttrib3, "TestAttrib3", TriggerExpectedBehavior.AllowUpdate);
			AssertTriggerUpdate("Changes to SerialNumber should be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedSerialNumber, "TestSerialNumber", TriggerExpectedBehavior.AllowUpdate);
		}

		public void TestTrigger_UpdateReleaseCapturingOnAllocatedOrder_NoPick_ForbidUpdate()
		{
			var sql = new SqlQueryBuilder();

			var pickLine = CreateWhsPickLineWithTestData(sql, hasPick: false, isWorkOrder: false);

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to Attrib1 should not be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedPartAttrib1, "TestAttrib1", TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_UpdateReleaseCapturingOnAllocatedOrder_NoPick_ForbidUpdateAttrib2()
		{
			var sql = new SqlQueryBuilder();

			var pickLine = CreateWhsPickLineWithTestData(sql, hasPick: false, isWorkOrder: false);

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to Attrib2 should not be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedPartAttrib2, "TestAttrib2", TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_UpdateReleaseCapturingOnAllocatedOrder_NoPick_ForbidUpdateAttrib3()
		{
			var sql = new SqlQueryBuilder();

			var pickLine = CreateWhsPickLineWithTestData(sql, hasPick: false, isWorkOrder: false);

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to Attrib3 should not be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedPartAttrib3, "TestAttrib3", TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_UpdateReleaseCapturingOnAllocatedOrder_NoPick_ForbidUpdateSerialNumber()
		{
			var sql = new SqlQueryBuilder();

			var pickLine = CreateWhsPickLineWithTestData(sql, hasPick: false, isWorkOrder: false);

			SaveDataToDB(sql);

			AssertTriggerUpdate("Changes to SerialNumber should not be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedSerialNumber, "TestSerialNumber", TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_UpdateReleaseCapturingOnAllocatedOrder_IsWorkOrder_ForbidUpdateAttrib1()
		{
			var sql = new SqlQueryBuilder();

			var pickLine = CreateWhsPickLineWithTestData(sql, hasPick: true, isWorkOrder: true);

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to Attrib1 should not be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedPartAttrib1, "TestAttrib1", TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_UpdateReleaseCapturingOnAllocatedOrder_IsWorkOrder_ForbidUpdateAttrib2()
		{
			var sql = new SqlQueryBuilder();

			var pickLine = CreateWhsPickLineWithTestData(sql, hasPick: true, isWorkOrder: true);

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to Attrib2 should not be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedPartAttrib2, "TestAttrib2", TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_UpdateReleaseCapturingOnAllocatedOrder_IsWorkOrder_ForbidUpdateAttrib3()
		{
			var sql = new SqlQueryBuilder();

			var pickLine = CreateWhsPickLineWithTestData(sql, hasPick: true, isWorkOrder: true);

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to Attrib3 should not be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedPartAttrib3, "TestAttrib3", TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_UpdateReleaseCapturingOnAllocatedOrder_IsWorkOrder_ForbidUpdateSerialNumber()
		{
			var sql = new SqlQueryBuilder();

			var pickLine = CreateWhsPickLineWithTestData(sql, hasPick: true, isWorkOrder: true);

			SaveDataToDB(sql);

			AssertTriggerUpdate("Changes to SerialNumber should not be allowed.", pickLine.PK,  (l) => l.WZ_ReleaseCapturedSerialNumber, "TestSerialNumber", TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_InsertReleaseCapturingOnAllocatedOrder_AllowInsert()
		{
			var sql = new SqlQueryBuilder();

			CreateWhsPickLineWithTestData(sql, hasPick: true, isWorkOrder: false, attrib1: "TestAttrib1", attrib2: "TestAttrib2", attrib3: "TestAttrib3", serialNumber: "TestSerialNumber");

			AssertSaveDataToDB("Changes to Attrib1, Attrib2, Attrib3 should be allowed.", TriggerExpectedBehavior.AllowUpdate, sql);
		}

		public void TestTrigger_InsertReleaseCapturingOnAllocatedOrder_NoPick_ForbidInsertAttrib1()
		{
			var sql = new SqlQueryBuilder();

			CreateWhsPickLineWithTestData(sql, hasPick: false, isWorkOrder: false, attrib1: "TestAttrib1");

			AssertSaveDataToDB("Changes to Attrib1 should not be allowed.", TriggerExpectedBehavior.ThrowException, sql);
		}

		public void TestTrigger_InsertReleaseCapturingOnAllocatedOrder_NoPick_ForbidInsertAttrib2()
		{
			var sql = new SqlQueryBuilder();

			CreateWhsPickLineWithTestData(sql, hasPick: false, isWorkOrder: false, attrib2: "TestAttrib2");

			AssertSaveDataToDB("Changes to Attrib2 should not be allowed.", TriggerExpectedBehavior.ThrowException, sql);
		}

		public void TestTrigger_InsertReleaseCapturingOnAllocatedOrder_NoPick_ForbidInsertAttrib3()
		{
			var sql = new SqlQueryBuilder();

			CreateWhsPickLineWithTestData(sql, hasPick: false, isWorkOrder: false, attrib3: "TestAttrib3");

			AssertSaveDataToDB("Changes to Attrib3 should not be allowed.", TriggerExpectedBehavior.ThrowException, sql);
		}

		public void TestTrigger_InsertReleaseCapturingOnAllocatedOrder_NoPick_ForbidInsertSerialNumber()
		{
			var sql = new SqlQueryBuilder();

			CreateWhsPickLineWithTestData(sql, hasPick: false, isWorkOrder: false, serialNumber: "TestSerialNumber");

			AssertSaveDataToDB("Changes to SerialNumber should not be allowed.", TriggerExpectedBehavior.ThrowException, sql);
		}

		public void TestTrigger_InsertReleaseCapturingOnAllocatedOrder_IsWorkOrder_ForbidInsertAttrib1()
		{
			var sql = new SqlQueryBuilder();

			CreateWhsPickLineWithTestData(sql, hasPick: true, isWorkOrder: true, attrib1: "TestAttrib1");

			AssertSaveDataToDB("Changes to Attrib1 should not be allowed.", TriggerExpectedBehavior.ThrowException, sql);
		}

		public void TestTrigger_InsertReleaseCapturingOnAllocatedOrder_IsWorkOrder_ForbidInsertAttrib2()
		{
			var sql = new SqlQueryBuilder();

			CreateWhsPickLineWithTestData(sql, hasPick: true, isWorkOrder: true, attrib2: "TestAttrib2");

			AssertSaveDataToDB("Changes to Attrib2 should not be allowed.", TriggerExpectedBehavior.ThrowException, sql);
		}

		public void TestTrigger_InsertReleaseCapturingOnAllocatedOrder_IsWorkOrder_ForbidInsertAttrib3()
		{
			var sql = new SqlQueryBuilder();

			CreateWhsPickLineWithTestData(sql, hasPick: true, isWorkOrder: true, attrib3: "TestAttrib3");

			AssertSaveDataToDB("Changes to Attrib3 should not be allowed.", TriggerExpectedBehavior.ThrowException, sql);
		}

		public void TestTrigger_InsertReleaseCapturingOnAllocatedOrder_IsWorkOrder_ForbidInsertSerialNumber()
		{
			var sql = new SqlQueryBuilder();

			CreateWhsPickLineWithTestData(sql, hasPick: true, isWorkOrder: true, serialNumber: "TestSerialNumber");

			AssertSaveDataToDB("Changes to SerialNumber should not be allowed.", TriggerExpectedBehavior.ThrowException, sql);
		}

		#region TestAssertions

		void AssertTriggerUpdate<T>(string errorMessage, Guid pickLinePK, Expression<Func<WhsPickLine, T>> property, T value, TriggerExpectedBehavior expectedBehavior)
		{
			var updateSQL = WhsPickLine
				.UpdateWhere(pickLinePK)
				.Set(property, value).AsSQL();

			if (expectedBehavior == TriggerExpectedBehavior.AllowUpdate)
			{
				AssertNoExceptionThrown(errorMessage, () => Db.Connection.ExecuteNonQuery(updateSQL));
			}
			else
			{
				AssertExceptionThrown(typeof(SqlException), "Attempt to release capture without an allocated Order.", () => Db.Connection.ExecuteNonQuery(updateSQL), true);
			}
		}

		void AssertSaveDataToDB(string errorMessage, TriggerExpectedBehavior expectedBehavior, SqlQueryBuilder sql)
		{
			if (expectedBehavior == TriggerExpectedBehavior.AllowUpdate)
			{
				AssertNoExceptionThrown(errorMessage, () => SaveDataToDB(sql));
			}
			else
			{
				AssertExceptionThrown(typeof(TestSqlStatementFailedException), "Error occured in test for attempted SQL Command, Error below:\r\nAttempt to release capture without an allocated Order.", () => SaveDataToDB(sql), true);	
			}
		}

		#endregion

		#region TestDataSetupMethods

		WhsPickLine CreateWhsPickLineWithTestData(SqlQueryBuilder sql, bool hasPick, bool isWorkOrder, string attrib1 = "", string attrib2 = "", string attrib3 = "", string serialNumber = "")
		{
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var pickLine = CreateOrderWithPickLine(sql, client, whs, product, location, "ORD1", hasPick, isWorkOrder, attrib1, attrib2, attrib3, serialNumber);

			return pickLine;
		}

		WhsDocketLine CreateInventoryLine(SqlQueryBuilder sql, OrgHeader client, WhsWarehouse whs, OrgSupplierPart product, WhsLocation location, string docketID, decimal units, decimal stockOnHand)
		{
			var today = DateTime.Now;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", docketID) { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, units, location.PK) { WE_StockOnHand = stockOnHand }.AppendInsertAndReturnObject(sql);

			return receiveLine;
		}

		WhsPickLine CreateOrderWithPickLine(SqlQueryBuilder sql, OrgHeader client, WhsWarehouse whs, OrgSupplierPart product, WhsLocation location, string docketID, bool hasPick, bool isWorkOrder, string attrib1, string attrib2, string attrib3, string serialNumber)
		{
			var ordertype = isWorkOrder ? "WOR" : "ORD";
			var subDocketType = isWorkOrder ? "DIS" : "ORD";

			if (hasPick)
			{
				var receiveLine = CreateInventoryLine(sql, client, whs, product, location, "R1", 2m, 1m);
				var pick = new WhsPick(whs, string.Format("P1_{0}", docketID), "PIC").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, ordertype, subDocketType, "PIC", docketID) { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 1m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, orderLine, 1m) { WZ_ReleaseCapturedPartAttrib1 = attrib1, WZ_ReleaseCapturedPartAttrib2 = attrib2, WZ_ReleaseCapturedPartAttrib3 = attrib3, WZ_ReleaseCapturedSerialNumber = serialNumber, WZ_PickedDateTime = DateTime.Now, WZ_GS_NKAssignedTo = "R" }.AppendInsertAndReturnObject(sql);
				return pickLine;
			}
			else
			{
				var receiveLine = CreateInventoryLine(sql, client, whs, product, location, "R1", 1m, 1m);
				var order = new WhsDocket(client.PK, whs.PK, ordertype, subDocketType, "ENT", docketID).AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 1m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, orderLine, 1m) { WZ_ReleaseCapturedPartAttrib1 = attrib1, WZ_ReleaseCapturedPartAttrib2 = attrib2, WZ_ReleaseCapturedPartAttrib3 = attrib3, WZ_ReleaseCapturedSerialNumber = serialNumber, WZ_OriginalReservedQty = 1 }.AppendInsertAndReturnObject(sql);
				return pickLine;
			}
		}

		#endregion

		#region Implementation

		static void SaveDataToDB(SqlQueryBuilder sql)
		{
			// defer triggers to run check in the end
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		enum TriggerExpectedBehavior
		{
			ThrowException,
			AllowUpdate
		}

		#endregion
	}
}
