using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.ProductWarehouse
{
	[TestedType(typeof(TransformStmALogToWhsInventoryHoldChangeLog))]
	class TransformStmALogToWhsInventoryHoldChangeLogTest : DataTransformationTestCase
	{
		#region DataTransformationTestCase

		protected override void PrepareTestData()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000002") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive2, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000002-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var transferForRec2 = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "W00000002TFR")
			{
				WD_FinalisedDate = now,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditUser = "Bob"
			}.AppendInsertAndReturnObject(sql);
			var transferLineForRec2 = new WhsDocketLine(transferForRec2, product.PK, 10, locationA2.PK)
			{
				WE_WL_TransferFrom = locationA1.PK,
				WE_StockOnHand = 10m
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);
			var pickLineForRec2 = new WhsPickLine(receiveLine2, transferLineForRec2, 10m)
			{
				WZ_PickedDateTime = now,
				WZ_GS_NKAssignedTo = "E"
			}.AppendInsertAndReturnObject(sql);

			var receive3 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000003") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine3 = new WhsDocketLine(receive3, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000003-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var receive4 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000004") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine4 = new WhsDocketLine(receive4, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000004-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=LCC|RES=Wrong|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T12:34:17|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T13:17:59|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M03" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T13:19:16|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-1), SL_GS_NKUser = "M04" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine2.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "|FDT=2017-03-01T08:44:26|TYP=Hold Code", SL_EventTime = now.AddMonths(-8), SL_GS_NKUser = "M05" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine2.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2017-03-01T08:58:20|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-7), SL_GS_NKUser = "M06" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine2.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2017-03-01T09:00:35|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-6), SL_GS_NKUser = "M07" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine3.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-07-20T12:35:05|NEW=HEL|RES=TEST|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M08" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		protected override void AssertTransformationResults()
		{
			// convert back to current schema now that transform has run.
			new DbColumnDependencyRemover(WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.WE_AdjustmentArrivalDate).DropRelateObjects(TestConnection);
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsDocketLine ALTER COLUMN WE_AdjustmentArrivalDate datetimeoffset(0) NULL");

			var receiveLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, r => r.WE_PackageGroupId == "W00000001-001").Single();

			var holdChangeLog1 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog1
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "Wrong")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
				.VerifyAll();

			var holdChangeLog2 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 2).Single();
			holdChangeLog2
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();

			var holdChangeLog3 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 3).Single();
			holdChangeLog3
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M03")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M03")
				.VerifyAll();

			var holdChangeLog4 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 4).Single();
			holdChangeLog4
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M04")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M04")
				.VerifyAll();

			var receiveLine2 = WhsDocketLine.ShallowLoadFromDB(TestConnection, r => r.WE_PackageGroupId == "W00000002-001").Single();
			var holdChangeLog5 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine2.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog5
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M05")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M05")
				.VerifyAll();

			var holdChangeLog6 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine2.PK && r.WHL_LogVersion == 2).Single();
			holdChangeLog6
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M06")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M06")
				.VerifyAll();

			var holdChangeLog7 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine2.PK && r.WHL_LogVersion == 3).Single();
			holdChangeLog7
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M07")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M07")
				.VerifyAll();

			var receiveLine3 = WhsDocketLine.ShallowLoadFromDB(TestConnection, r => r.WE_PackageGroupId == "W00000003-001").Single();
			var holdChangeLog8 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine3.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog8
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "TEST")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M08")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M08")
				.VerifyAll();

			var receiveLine4 = WhsDocketLine.ShallowLoadFromDB(TestConnection, r => r.WE_PackageGroupId == "W00000004-001").Single();
			AssertEquals("Do not add hold change log whene there is no associated HCC StmALogs", 0, WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine4.PK).Length);
		}

		public void TestUserDescription()
		{
			AssertEquals("Transform StmALog to WhsInventoryHoldChangeLog.", GetNewTestTransformationInstance().UserDescription);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new TransformStmALogToWhsInventoryHoldChangeLog();

		#endregion

		#region TestRemappedTransformWorks

		const string LastDocketPKName = "TransformStmALogToWhsInventoryHoldChangeLog.LastDocketPK";
		const string MaxDocketPKName = "TransformStmALogToWhsInventoryHoldChangeLog.MaxDocketPK";
		const string TotalDocketsName = "TransformStmALogToWhsInventoryHoldChangeLog.TotalDockets";
		const string CurrentCountName = "TransformStmALogToWhsInventoryHoldChangeLog.CurrentCount";
		const string FromTimeName = "TransformStmALogToWhsInventoryHoldChangeLog.FromTime";
		const string ToTimeName = "TransformStmALogToWhsInventoryHoldChangeLog.ToTime";
		const string HighWaterMarkName = "TransformStmALogToWhsInventoryHoldChangeLog.HighWaterMark";
		const string IsNewTransform = "TransformStmALogToWhsInventoryHoldChangeLog.IsNewTransform";

		public void TestRemappedTransformWorks()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T12:34:17|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);

			var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive2, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var adjustmentLine = new WhsDocketLine(adjustment, product.PK, -10m, location.PK).AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine2, adjustmentLine, 10m)
			{
				WZ_PickedDateTime = now,
				WZ_GS_NKAssignedTo = "E"
			}.AppendInsertAndReturnObject(sql);

			new StmALog(receiveLine2.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T12:34:17|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var maxDocketPK = (Guid)TestConnection.ExecuteScalar("SELECT MAX(WD_PK) FROM WhsDocket");
			ExtProperty.Database.Update(TestConnection, TotalDocketsName, "100"); // bogus docket count
			ExtProperty.Database.Update(TestConnection, CurrentCountName, "100"); // bogus docket count
			ExtProperty.Database.Update(TestConnection, LastDocketPKName, maxDocketPK.ToString());
			ExtProperty.Database.Update(TestConnection, MaxDocketPKName, maxDocketPK.ToString());
			ExtProperty.Database.Update(TestConnection, HighWaterMarkName, SqlFormatInfo.ToSqlDateTimeString(DateTime.UtcNow.AddDays(1)));
			ExtProperty.Database.Update(TestConnection, FromTimeName, SqlFormatInfo.ToSqlDateTimeString(receiveLine1.WE_AdjustmentArrivalDate.Value.DateTime));
			ExtProperty.Database.Update(TestConnection, ToTimeName, SqlFormatInfo.ToSqlDateTimeString(receiveLine1.WE_AdjustmentArrivalDate.Value.DateTime));
			AssertNull(ExtProperty.Database.Select(TestConnection, IsNewTransform));

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("New Transform running Bool should be set.", bool.TrueString, ExtProperty.Database.Select(TestConnection, IsNewTransform));

			var holdChangeLog1 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK).Single();
			holdChangeLog1
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();

			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertNull("New Transform running Bool should be deleted in offline phase.", ExtProperty.Database.Select(TestConnection, IsNewTransform));

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			var holdChangeLog2 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine2.PK).Single();
			holdChangeLog2
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();

			AssertNull(ExtProperty.Database.Select(TestConnection, FromTimeName));
			AssertNull(ExtProperty.Database.Select(TestConnection, ToTimeName));
			AssertNull(ExtProperty.Database.Select(TestConnection, TotalDocketsName));
			AssertNull(ExtProperty.Database.Select(TestConnection, CurrentCountName));
			AssertNull(ExtProperty.Database.Select(TestConnection, LastDocketPKName));
			AssertNull(ExtProperty.Database.Select(TestConnection, MaxDocketPKName));
			AssertNull(ExtProperty.Database.Select(TestConnection, HighWaterMarkName));
		}

		#endregion

		#region TestOnlinePreupgradeTransform

		public void TestBatchingWorks_OnlinePreupgrade()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var receiveLines = new WhsDocketLine[1500];
			var logs = new List<StmALog>(500);
			var startDate = new DateTime(2015, 01, 01);
			for (var index = 0; index < receiveLines.Length; index++)
			{
				receiveLines[index] = new WhsDocketLine(receive, product.PK, 10m, location.PK)
				{
					WE_StockOnHand = 10m,
					WE_OriginalInventoryStatus = "AVL",
					WE_PackageGroupId = "W00000001-001",
					WE_AdjustmentArrivalDate = new DateTimeOffset(startDate.AddDays(index), TimeSpan.Zero),
				};

				if (index >= 1100)
				{
					logs.Add(new StmALog(receiveLines[index].PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T12:34:17|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" });
				}
			}

			sql.AppendLine(receiveLines.GetBulkInsertStatement(l => l.WE_AdjustmentArrivalDate));
			sql.AppendLine(logs.GetBulkInsertStatement());
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			for (var index = 1100; index < receiveLines.Length; index++)
			{
				var receiveLine = receiveLines[index];
				var holdChangeLog = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK).Single();
				holdChangeLog
					.BuildAssertion(TestConnection)
					.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
					.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
					.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
					.VerifyAll();
			}
		}

		public void TestTransformSetsHighWaterMarkForOfflineSection()
		{
			AssertNull("No High watermark set yet.", ExtProperty.Database.Select(TestConnection, "TransformStmALogToWhsInventoryHoldChangeLog.HighWaterMark"));

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var highWaterMark = SqlFormatInfo.FromSqlDateTime(ExtProperty.Database.Select(TestConnection, "TransformStmALogToWhsInventoryHoldChangeLog.HighWaterMark"));
			AssertEquals("High watermark should be set to roughly 24 hours ago.", true, highWaterMark >= DateTime.UtcNow.AddHours(-25) && highWaterMark <= DateTime.UtcNow.AddHours(-23));

			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("High watermark should not change when running Online Upgrade again.", highWaterMark, SqlFormatInfo.FromSqlDateTime(ExtProperty.Database.Select(TestConnection, "TransformStmALogToWhsInventoryHoldChangeLog.HighWaterMark")));

			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertNull("High watermark should be deleted after running Offline Upgrade.", ExtProperty.Database.Select(TestConnection, "TransformStmALogToWhsInventoryHoldChangeLog.HighWaterMark"));
		}

		public void TestTransformDoesNotRunIfDocketLineTableDoesNotExist()
		{
			var tablesToDrop = new[]
			{
				WhsSerialNumberPivotSchema.Constants.TableName,
				WhsInventoryHoldChangeLogSchema.Constants.TableName,
				WhsPickLineSchema.Constants.TableName,
				WhsPickShortLineSchema.Constants.TableName,
				WhsBOMInventoryPivotSchema.Constants.TableName,
				WhsDocketLineSchema.Constants.TableName,
			};

			foreach (var table in tablesToDrop)
			{
				var dropSQL = UpgraderUtils.GetSchemaBoundObjectsToDropSql("dbo", table);
				new BatchRunner().RunCommandsGeneratedByQuery(TestConnection, dropSQL);
				TestConnection.ExecuteNonQuery($"DROP TABLE {table}");
			}

			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));
			AssertNull("No High watermark set as transform did not run.", ExtProperty.Database.Select(TestConnection, "TransformStmALogToWhsInventoryHoldChangeLog.HighWaterMark"));
		}

		public void TestTransformCreatesHoldCodeTableIfNotExists()
		{
			var dropSQL = UpgraderUtils.GetSchemaBoundObjectsToDropSql("dbo", WhsInventoryHoldChangeLogSchema.Constants.TableName);
			new BatchRunner().RunCommandsGeneratedByQuery(TestConnection, dropSQL);
			TestConnection.ExecuteNonQuery("DROP TABLE dbo.WhsInventoryHoldChangeLog");

			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=LCC|RES=Wrong|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T12:34:17|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T13:17:59|NEW=SHORT|RES=BROKEN|TYP=Hold Code", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M03" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T13:19:16|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-1), SL_GS_NKUser = "M04" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var holdChangeLog1 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog1
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "Wrong")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
				.VerifyAll();

			var holdChangeLog2 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 2).Single();
			holdChangeLog2
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_WHC_NKCode", l => l.WHL_WHC_NKCode, "")
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();

			var holdChangeLog3 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 3).Single();
			holdChangeLog3
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "BROKEN")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M03")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M03")
				.VerifyAll();

			var holdChangeLog4 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 4).Single();
			holdChangeLog4
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M04")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M04")
				.VerifyAll();
		}

		public void TestTransformCreatesWHL_EventTimeIfNotExists()
		{
			new DbColumnDependencyRemover("dbo", WhsInventoryHoldChangeLogSchema.Constants.TableName, WhsInventoryHoldChangeLogSchema.Constants.WHL_EventTime).DropRelateObjects(TestConnection);
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsInventoryHoldChangeLog DROP COLUMN WHL_EventTime");

			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T12:34:17|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var holdChangeLog = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK).Single();
			holdChangeLog
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();
		}

		public void TestTransformHasOldTriggerDefinition()
		{
			TestConnection.ExecuteNonQuery(@"
ALTER TRIGGER [dbo].[TG_WhsInventoryHoldChangeLog_LogVersionCheck]
ON [dbo].[WhsInventoryHoldChangeLog]
AFTER INSERT
AS
BEGIN
    IF (@@ROWCOUNT = 0) RETURN
    SET NOCOUNT ON

	IF EXISTS
	(
		SELECT NULL
		FROM
			inserted
		GROUP BY
			WHL_WE_ParentDocketLine
		HAVING
			COUNT(*) > 1
	)
	BEGIN
		RAISERROR('Only one WHL_LogVersion per ParentDocketLine can be inserted in a single transaction.', 16, 1)
		ROLLBACK
	END

	IF EXISTS (
		SELECT
			WHL_WE_ParentDocketLine
		FROM
			dbo.WhsInventoryHoldChangeLog
		GROUP BY
			WHL_WE_ParentDocketLine
		HAVING
			MAX(WHL_LogVersion) <> COUNT(*)
	)
	BEGIN
		RAISERROR('The WHL_LogVersion must be sequential and must be 1 for each new and unique WhsInventoryHoldChangeLog.', 16, 1)
		ROLLBACK
	END
END");

			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T12:34:17|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-25T12:34:17|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M03" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var holdChangeLog1 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog1
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();

			var holdChangeLog2 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 2).Single();
			holdChangeLog2
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M03")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M03")
				.VerifyAll();
		}

		public void TestOfflineTransformOnlyLooksAtRecordsAfterHighWatermark()
		{
			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var highWaterMark = SqlFormatInfo.FromSqlDateTime(ExtProperty.Database.Select(TestConnection, "TransformStmALogToWhsInventoryHoldChangeLog.HighWaterMark"));

			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
				WE_SystemLastEditTimeUtc = highWaterMark.AddDays(-1),
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T12:34:17|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("Nothing should have been transformed.", false, WhsInventoryHoldChangeLog.ExistsInDB(TestConnection, l => l.WHL_WE_ParentDocketLine == receiveLine1.PK));

			// pretend the HighWatermark is before the last edit time and run offline transform again
			ExtProperty.Database.Update(TestConnection, "TransformStmALogToWhsInventoryHoldChangeLog.HighWaterMark", SqlFormatInfo.ToSqlDateTimeString(highWaterMark.AddDays(-2)));
			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			var holdChangeLog = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK).Single();
			holdChangeLog
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();
		}

		public void TestRowIsUpdatedAfterOnlineTransformIsRun()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T12:34:17|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var holdChangeLog1 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK).Single();
			holdChangeLog1
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-25T12:34:17|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M03" }.Insert(TestConnection);
			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			var holdChangeLog2 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 2).Single();
			holdChangeLog2
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M03")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M03")
				.VerifyAll();
		}

		public void TestTransform_WhenDefaultDateTimeHasBeenSet()
		{
			AssertTestTransform_WhenDefaultDateTimeHasBeenSet(TimeSpan.Zero);
		}

		public void TestTransform_WhenDefaultDateTimePlusLessThanAMinuteHasBeenSet()
		{
			AssertTestTransform_WhenDefaultDateTimeHasBeenSet(TimeSpan.FromSeconds(59));
		}

		public void TestTransform_WhenDefaultDateTimePlusAMinuteHasBeenSet()
		{
			AssertTestTransform_WhenDefaultDateTimeHasBeenSet(TimeSpan.FromMinutes(1));
		}

		void AssertTestTransform_WhenDefaultDateTimeHasBeenSet(TimeSpan timeAdded)
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var arrivalDate = new DateTimeOffset(new DateTime(1900, 1, 1).Add(timeAdded), TimeSpan.Zero);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_ArrivalDate = arrivalDate, WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
				WE_AdjustmentArrivalDate = arrivalDate,
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T12:34:17|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var holdChangeLog = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK).Single();
			holdChangeLog
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();
		}

		#endregion

		#region TestOfflinePostUpgradeTransform

		public void TestOfflinePostUpgradeTransform()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=LCC|RES=Wrong|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T12:34:17|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T13:17:59|NEW=SHORT|RES=BROKEN|TYP=Hold Code", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M03" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T13:19:16|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-1), SL_GS_NKUser = "M04" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			var holdChangeLog1 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog1
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "Wrong")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
				.VerifyAll();

			var holdChangeLog2 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 2).Single();
			holdChangeLog2
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_WHC_NKCode", l => l.WHL_WHC_NKCode, "")
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();

			var holdChangeLog3 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 3).Single();
			holdChangeLog3
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "BROKEN")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M03")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M03")
				.VerifyAll();

			var holdChangeLog4 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 4).Single();
			holdChangeLog4
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M04")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M04")
				.VerifyAll();
		}

		public void TestOfflinePostUpgradeTransform_MissingHoldCodeStmALog()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=LCC|RES=Wrong", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T12:34:17|OLD=HEL", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T13:17:59|NEW=SHORT|RES=BROKEN", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M03" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T13:19:16|OLD=HEL", SL_EventTime = now.AddMonths(-1), SL_GS_NKUser = "M04" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			var changeLogs = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK);
			AssertEquals("WhsDocketLine with missing Hold Code StmALog do not add hold change logs", 0, changeLogs.Length);
		}

		public void TestOfflinePostUpgradeTransform_NoRelatedStmALog()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			var changeLogs = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK);
			AssertEquals("WhsDocketLine with no associated HCC StmALog do not add hold change logs", 0, changeLogs.Length);
		}

		public void TestOfflinePostUpgradeTransform_ZeroStockOnHand()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001")
			{
				WD_FinalisedDate = now,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditUser = "Bob"
			}
			.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "AVL",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "W00000002")
			{
				WD_FinalisedDate = now,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditUser = "Bob"
			}.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, product.PK, 10, locationA2.PK)
			{
				WE_WL_TransferFrom = locationA1.PK,
				WE_StockOnHand = 10m
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine1, 10m)
			{
				WZ_PickedDateTime = now,
				WZ_GS_NKAssignedTo = "E"
			}.AppendInsertAndReturnObject(sql);

			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=LCC|RES=Wrong|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T12:34:17|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T13:17:59|NEW=SHORT|RES=BROKEN|TYP=Hold Code", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M03" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T13:19:16|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-1), SL_GS_NKUser = "M04" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			var changeLogs = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK);
			AssertEquals("WhsDocketLine which WE_StockOnHand = 0 will process in OnlinePostUpgrade", 0, changeLogs.Length);
		}

		#endregion

		#region TestOnlinePostUpgradeTransform

		public void TestOnlinePostUpgradeTransform()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001")
			{
				WD_FinalisedDate = now,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditUser = "Bob"
			}
			.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "AVL",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "W00000002")
			{
				WD_FinalisedDate = now,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditUser = "Bob"
			}.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, product.PK, 10, locationA2.PK)
			{
				WE_WL_TransferFrom = locationA1.PK,
				WE_StockOnHand = 10m
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine1, 10m)
			{
				WZ_PickedDateTime = now,
				WZ_GS_NKAssignedTo = "E"
			}.AppendInsertAndReturnObject(sql);

			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=LCC|RES=Wrong|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T12:34:17|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T13:17:59|NEW=SHORT|RES=BROKEN|TYP=Hold Code", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M03" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T13:19:16|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-1), SL_GS_NKUser = "M04" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			var holdChangeLog1 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog1
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "Wrong")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
				.VerifyAll();

			var holdChangeLog2 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK && r.WHL_LogVersion == 2).Single();
			holdChangeLog2
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();

			var holdChangeLog3 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK && r.WHL_LogVersion == 3).Single();
			holdChangeLog3
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "BROKEN")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M03")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M03")
				.VerifyAll();

			var holdChangeLog4 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK && r.WHL_LogVersion == 4).Single();
			holdChangeLog4
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M04")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M04")
				.VerifyAll();
		}

		public void TestOnlinePostUpgradeTransform_MissingHoldCodeStmALog()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001")
			{
				WD_FinalisedDate = now,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditUser = "Bob"
			}
			.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "AVL",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "W00000002")
			{
				WD_FinalisedDate = now,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditUser = "Bob"
			}.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, product.PK, 10, locationA2.PK)
			{
				WE_WL_TransferFrom = locationA1.PK,
				WE_StockOnHand = 10m
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine1, 10m)
			{
				WZ_PickedDateTime = now,
				WZ_GS_NKAssignedTo = "E"
			}.AppendInsertAndReturnObject(sql);

			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=LCC|RES=Wrong", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T12:34:17|OLD=HEL", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T13:17:59|NEW=SHORT|RES=BROKEN", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M03" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T13:19:16|OLD=HEL", SL_EventTime = now.AddMonths(-1), SL_GS_NKUser = "M04" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			var changeLogs = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK);
			AssertEquals("WhsDocketLine with missing Hold Code StmALog do not add hold change logs", 0, changeLogs.Length);
		}

		public void TestOnlinePostUpgradeTransform_NoRelatedStmALog()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001")
			{
				WD_FinalisedDate = now,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditUser = "Bob"
			}
			.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "AVL",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "W00000002")
			{
				WD_FinalisedDate = now,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditUser = "Bob"
			}.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, product.PK, 10, locationA2.PK)
			{
				WE_WL_TransferFrom = locationA1.PK,
				WE_StockOnHand = 10m
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine1, 10m)
			{
				WZ_PickedDateTime = now,
				WZ_GS_NKAssignedTo = "E"
			}.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			var changeLogs = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK);
			AssertEquals("WhsDocketLine with no associated HCC StmALog do not add hold change logs", 0, changeLogs.Length);
		}

		public void TestOnlinePostUpgradeTransform_AnyStockOnHand()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001")
			{
				WD_FinalisedDate = now,
				WD_GS_NKFinalizedBy = "T",
				WD_SystemLastEditUser = "Bob"
			}
			.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=LCC|RES=Wrong|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T12:34:17|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T13:17:59|NEW=SHORT|RES=BROKEN|TYP=Hold Code", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M03" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T13:19:16|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-1), SL_GS_NKUser = "M04" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			var changeLogs = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK);
			AssertEquals("WhsDocketLine which WE_StockOnHand > 0 will process in OfflinePostUpgrade", 0, changeLogs.Length);
		}

		#endregion

		public void TestBatching()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receives = new WhsDocket[3005];
			var receiveLines = new WhsDocketLine[receives.Length];
			var transfers = new WhsDocket[receives.Length];
			var transferLines = new WhsDocketLine[receives.Length];
			var pickLines = new WhsPickLine[receives.Length];
			var logs = new List<StmALog>((receives.Length + 1) / 2 * 3);

			for (var i = 0; i < 3005; i++)
			{
				receives[i] = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"W{i}")
				{
					WD_FinalisedDate = now,
					WD_GS_NKFinalizedBy = "T",
					WD_SystemLastEditUser = "Bob"
				};
				receiveLines[i] = new WhsDocketLine(receives[i], product.PK, 10m, locationA1.PK)
				{
					WE_StockOnHand = 0m,
					WE_OriginalInventoryStatus = "AVL",
				};

				transfers[i] = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", $"WT{i}")
				{
					WD_FinalisedDate = now,
					WD_GS_NKFinalizedBy = "T",
					WD_SystemLastEditUser = "Bob"
				};
				transferLines[i] = new WhsDocketLine(transfers[i], product.PK, 10, locationA2.PK)
				{
					WE_WL_TransferFrom = locationA1.PK,
					WE_StockOnHand = 10m
				};
				pickLines[i] = new WhsPickLine(receiveLines[i], transferLines[i], 10m)
				{
					WZ_PickedDateTime = now,
					WZ_GS_NKAssignedTo = "E"
				};

				if (i % 2 == 0)
				{
					logs.Add(new StmALog(receiveLines[i].PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=LCC|RES=Wrong|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" });
					logs.Add(new StmALog(receiveLines[i].PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T12:34:17|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" });
					logs.Add(new StmALog(receiveLines[i].PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=LCC|RES=Wrong", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M03" });
				}
			}

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				sql.AppendLine(receives.GetBulkInsertStatement());
				sql.AppendLine(receiveLines.GetBulkInsertStatement(l => l.WE_AdjustmentArrivalDate));
				sql.AppendLine(transfers.GetBulkInsertStatement());
				sql.AppendLine(transferLines.GetBulkInsertStatement(l => l.WE_AdjustmentArrivalDate));
				sql.AppendLine(pickLines.GetBulkInsertStatement());
				sql.AppendLine(logs.GetBulkInsertStatement());

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(4509, StmALog.ShallowLoadFromDB(TestConnection, p => p.SL_Table == "WhsDocketLine" && p.SL_SE_NKEvent == "CID").Length);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals(3006, WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection).Length);
		}

		public void TestBatchingWorksProperly_OnlinePostupgrade()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receives = new WhsDocket[1500];
			var receiveLines = new WhsDocketLine[receives.Length];
			var pickLines = new WhsPickLine[receives.Length];
			var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var adjustmentLine = new WhsDocketLine(adjustment, product.PK, -10m * receiveLines.Length, location.PK).AppendInsertAndReturnObject(sql);

			var startDate = new DateTime(2015, 01, 01);
			for (var index = 0; index < receiveLines.Length; index++)
			{
				receives[index] = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"W{index}") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T" };

				receiveLines[index] = new WhsDocketLine(receives[index], product.PK, 10m, location.PK)
				{
					WE_StockOnHand = 0m,
					WE_OriginalInventoryStatus = "AVL",
					WE_PackageGroupId = "W00000001-001",
					WE_AdjustmentArrivalDate = new DateTimeOffset(startDate.AddDays(index), TimeSpan.Zero),
				};

				pickLines[index] = new WhsPickLine(receiveLines[index], adjustmentLine, 10m)
				{
					WZ_PickedDateTime = now,
					WZ_GS_NKAssignedTo = "E"
				};
			}

			sql.AppendLine(receives.GetBulkInsertStatement());
			sql.AppendLine(receiveLines.GetBulkInsertStatement(l => l.WE_AdjustmentArrivalDate));
			sql.AppendLine(pickLines.GetBulkInsertStatement());

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var receiveLinePK = (Guid)TestConnection.ExecuteScalar(@"
SELECT TOP 1 WE_PK
FROM
(
	SELECT WE_PK, ROW_NUMBER() OVER(ORDER BY WE_WD ASC) as RowNum
	FROM dbo.WhsDocketLine
) Dockets
WHERE RowNum > 1100");

			new StmALog(receiveLinePK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T12:34:17|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.Insert(TestConnection);

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			var holdChangeLog = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLinePK).Single();
			holdChangeLog
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();
		}

		#region TestTransformForSpecialCharater

		public void TestTransformForSpecialCharacter()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=|RES=''|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T12:34:17|OLD=HEL|RES=|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T13:17:59|NEW='SHORT'|RES='BROKEN'|TYP=Hold Code", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T13:19:16|OLD=HEL|RES=''|TYP=Hold Code", SL_EventTime = now.AddMonths(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			var holdChangeLog1 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog1
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
				.VerifyAll();

			var holdChangeLog2 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 2).Single();
			holdChangeLog2
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
				.VerifyAll();

			var holdChangeLog3 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 3).Single();
			holdChangeLog3
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "BROKEN")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
				.VerifyAll();

			var holdChangeLog4 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine1.PK && r.WHL_LogVersion == 4).Single();
			holdChangeLog4
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
				.VerifyAll();
		}

		#endregion

		#region TestTransform_UpdateSplitDocketLineHCCAndStatus

		public void TestTransform_UpdateSplitDocketLineHCCAndStatus_Online()
		{
			TestTransform_UpdateSplitDocketLineHCCAndStatusCore(isOnline: true);
		}

		public void TestTransform_UpdateSplitDocketLineHCCAndStatus_OffLine()
		{
			TestTransform_UpdateSplitDocketLineHCCAndStatusCore(isOnline: false);
		}

		void TestTransform_UpdateSplitDocketLineHCCAndStatusCore(bool isOnline)
		{
			InsertRefUNLOCOUtcOffset();

			var now = new DateTime(2022, 08, 15);
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUBNE" }.AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = 5m,
				WE_OriginalInventoryStatus = "AVL",
				WE_WHC_NKOriginalInventoryHeldCode = "",
				WE_CurrentInventoryStatus = "AVL",
				WE_WHC_NKCurrentInventoryHeldCode = "",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var receiveLineClone = new WhsDocketLine(receive, product.PK, 5m, locationA1.PK)
			{
				WE_WE_ParentDocketLine = receiveLine,
				WE_StockOnHand = isOnline ? 0m : 5m,
				WE_OriginalInventoryStatus = "HEL",
				WE_WHC_NKOriginalInventoryHeldCode = "DAM",
				WE_CurrentInventoryStatus = "HEL",
				WE_WHC_NKCurrentInventoryHeldCode = "DAM",
				WE_PackageGroupId = "W00000001-001",
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = receiveLine,
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			if (isOnline)
			{
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "W00000002")
				{
					WD_FinalisedDate = now,
					WD_GS_NKFinalizedBy = "T",
					WD_SystemLastEditUser = "Bob"
				}.AppendInsertAndReturnObject(sql);
				var transferLine1 = new WhsDocketLine(transfer, product.PK, 5m, locationA2.PK)
				{
					WE_WL_TransferFrom = locationA1.PK,
					WE_StockOnHand = 5m
				}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);
				var pickLine = new WhsPickLine(receiveLineClone, transferLine1, 5m)
				{
					WZ_PickedDateTime = now,
					WZ_GS_NKAssignedTo = "E"
				}.AppendInsertAndReturnObject(sql);
			}

			var eventTime = now.AddHours(-1);
			new StmALog(receiveLineClone.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=DAM|RES=Wanted|TYP=Hold Code", SL_EventTime = eventTime, SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			if (isOnline)
			{
				GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
				GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			}
			else
			{
				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			}

			// convert back to current schema now that transform has run.
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsDocketLine ALTER COLUMN WE_AdjustmentArrivalDate datetimeoffset(0) NULL");

			WhsDocketLine.AssertFromDB(TestConnection, receiveLineClone.PK)
				.ExpectEquals("WE_WE_ParentDocketLine", r => r.WE_WE_ParentDocketLine, receiveLine)
				.ExpectEquals("WE_OriginalInventoryStatus", r => r.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals("WE_WHC_NKOriginalInventoryHeldCode", r => r.WE_WHC_NKOriginalInventoryHeldCode, "")
				.ExpectEquals("WE_CurrentInventoryStatus", r => r.WE_CurrentInventoryStatus, "HEL")
				.ExpectEquals("WE_WHC_NKCurrentInventoryHeldCode", r => r.WE_WHC_NKCurrentInventoryHeldCode, "DAM")
				.VerifyAll();

			var newLog = StmALog.ShallowLoadFromDB(TestConnection, r => r.SL_Parent == receiveLineClone.PK && r.SL_GS_NKUser == "~BP").Single();
			newLog
				.BuildAssertion(TestConnection)
				.ExpectEquals("SL_SE_NKEvent", l => l.SL_SE_NKEvent, "MIS")
				.ExpectEquals("SL_Table", l => l.SL_Table, WhsDocketLineSchema.Constants.TableName)
				.ExpectEquals("SL_Reference", l => l.SL_Reference, "TransformStmALogToWhsInventoryHoldChangeLog: Updated WE_OriginalInventoryStatus from 'HEL' to 'AVL' and WE_WHC_NKOriginalInventoryHeldCode from 'DAM' to ''.")
				.VerifyAll();

			var holdChangeLog = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLineClone.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_WHC_NKCode", l => l.WHL_WHC_NKCode, "DAM")
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "Wanted")
				.ExpectEquals("WHL_EventTime", l => l.WHL_EventTime, new DateTimeOffset(eventTime.Year, eventTime.Month, eventTime.Day, eventTime.Hour, eventTime.Minute, eventTime.Second, TimeSpan.FromHours(10)))
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
				.VerifyAll();
		}

		public void TestTransform_UpdateSplitDocketLineHCCAndStatus_UsesParentCurrentHCC_FromTimeOfSplit_Online()
		{
			TestTransform_UpdateSplitDocketLineHCCAndStatus_UsesParentCurrentHCC_FromTimeOfSplitCore(isOnline: true);
		}

		public void TestTransform_UpdateSplitDocketLineHCCAndStatus_UsesParentCurrentHCC_FromTimeOfSplit_OffLine()
		{
			TestTransform_UpdateSplitDocketLineHCCAndStatus_UsesParentCurrentHCC_FromTimeOfSplitCore(isOnline: false);
		}

		void TestTransform_UpdateSplitDocketLineHCCAndStatus_UsesParentCurrentHCC_FromTimeOfSplitCore(bool isOnline)
		{
			InsertRefUNLOCOUtcOffset();

			var now = new DateTime(2022, 08, 15);
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "CNNJG" }.AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = isOnline ? 0m : 5m,
				WE_OriginalInventoryStatus = "AVL",
				WE_WHC_NKOriginalInventoryHeldCode = "",
				WE_CurrentInventoryStatus = "HEL",
				WE_WHC_NKCurrentInventoryHeldCode = "QC",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var receiveLineClone = new WhsDocketLine(receive, product.PK, 5m, locationA1.PK)
			{
				WE_WE_ParentDocketLine = receiveLine,
				WE_StockOnHand = isOnline ? 0m : 5m,
				WE_OriginalInventoryStatus = "HEL",
				WE_WHC_NKOriginalInventoryHeldCode = "DAM",
				WE_CurrentInventoryStatus = "HEL",
				WE_WHC_NKCurrentInventoryHeldCode = "DAM",
				WE_PackageGroupId = "W00000001-001",
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = receiveLine,
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			if (isOnline)
			{
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "W00000002")
				{
					WD_FinalisedDate = now,
					WD_GS_NKFinalizedBy = "T",
					WD_SystemLastEditUser = "Bob"
				}.AppendInsertAndReturnObject(sql);
				var transferLine1 = new WhsDocketLine(transfer, product.PK, 5m, locationA2.PK)
				{
					WE_WL_TransferFrom = locationA1.PK,
					WE_StockOnHand = 5m
				}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);
				new WhsPickLine(receiveLine, transferLine1, 5m)
				{
					WZ_PickedDateTime = now,
					WZ_GS_NKAssignedTo = "E"
				}.AppendInsertAndReturnObject(sql);
				var transferLine2 = new WhsDocketLine(transfer, product.PK, 5m, locationA2.PK)
				{
					WE_WL_TransferFrom = locationA1.PK,
					WE_StockOnHand = 5m
				}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);
				new WhsPickLine(receiveLineClone, transferLine2, 5m)
				{
					WZ_PickedDateTime = now,
					WZ_GS_NKAssignedTo = "E"
				}.AppendInsertAndReturnObject(sql);
			}

			var eventTime1 = now.AddYears(-2);
			new StmALog(receiveLineClone.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2021-12-22T09:38:47|NEW=DAM|RES=Wanted|TYP=Hold Code", SL_EventTime = eventTime1, SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);

			var eventTime2 = now.AddYears(-1);
			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=QC|RES=Other|TYP=Hold Code", SL_EventTime = eventTime2, SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			if (isOnline)
			{
				GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
				GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			}
			else
			{
				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			}

			// convert back to current schema now that transform has run.
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsDocketLine ALTER COLUMN WE_AdjustmentArrivalDate datetimeoffset(0) NULL");

			WhsDocketLine.AssertFromDB(TestConnection, receiveLineClone.PK)
				.ExpectEquals("WE_WE_ParentDocketLine", r => r.WE_WE_ParentDocketLine, receiveLine)
				.ExpectEquals("WE_OriginalInventoryStatus", r => r.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals("WE_WHC_NKOriginalInventoryHeldCode", r => r.WE_WHC_NKOriginalInventoryHeldCode, "")
				.ExpectEquals("WE_CurrentInventoryStatus", r => r.WE_CurrentInventoryStatus, "HEL")
				.ExpectEquals("WE_WHC_NKCurrentInventoryHeldCode", r => r.WE_WHC_NKCurrentInventoryHeldCode, "DAM")
				.VerifyAll();
			AssertEquals("WE_AutoVersion is correct", (short)1, TestConnection.ExecuteScalar($"SELECT WE_AutoVersion FROM dbo.WhsDocketLine WHERE WE_PK = '{receiveLineClone.PK}'"));

			var newLog = StmALog.ShallowLoadFromDB(TestConnection, r => r.SL_Parent == receiveLineClone.PK && r.SL_GS_NKUser == "~BP").Single();
			newLog
				.BuildAssertion(TestConnection)
				.ExpectEquals("SL_SE_NKEvent", l => l.SL_SE_NKEvent, "MIS")
				.ExpectEquals("SL_Table", l => l.SL_Table, WhsDocketLineSchema.Constants.TableName)
				.ExpectEquals("SL_Reference", l => l.SL_Reference, "TransformStmALogToWhsInventoryHoldChangeLog: Updated WE_OriginalInventoryStatus from 'HEL' to 'AVL' and WE_WHC_NKOriginalInventoryHeldCode from 'DAM' to ''.")
				.VerifyAll();

			var holdChangeLog1 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLineClone.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog1
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_WHC_NKCode", l => l.WHL_WHC_NKCode, "DAM")
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "Wanted")
				.ExpectEquals("WHL_EventTime", l => l.WHL_EventTime, new DateTimeOffset(eventTime1.Year, eventTime1.Month, eventTime1.Day, eventTime1.Hour, eventTime1.Minute, eventTime1.Second, TimeSpan.FromHours(8)))
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
				.VerifyAll();

			var holdChangeLog2 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog2
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_WHC_NKCode", l => l.WHL_WHC_NKCode, "QC")
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "Other")
				.ExpectEquals("WHL_EventTime", l => l.WHL_EventTime, new DateTimeOffset(eventTime2.Year, eventTime2.Month, eventTime2.Day, eventTime2.Hour, eventTime2.Minute, eventTime2.Second, TimeSpan.FromHours(8)))
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();
		}

		public void TestTransform_UpdateSplitDocketLineHCCAndStatus_UsesParentCurrentHCC_IgnoresFullDocketLineHCC()
		{
			var now = new DateTime(2022, 08, 15);
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 10m,
				WE_OriginalInventoryStatus = "AVL",
				WE_WHC_NKOriginalInventoryHeldCode = "",
				WE_CurrentInventoryStatus = "HEL",
				WE_WHC_NKCurrentInventoryHeldCode = "QC",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=QC|RES=Other|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			var holdChangeLog = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_WHC_NKCode", l => l.WHL_WHC_NKCode, "QC")
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "Other")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();
		}

		public void TestTransform_UpdateSplitDocketLineHCCAndStatus_MutlipleSplits_Online()
		{
			TestTransform_UpdateSplitDocketLineHCCAndStatus_MutlipleSplitsCore(isOnline: true);
		}

		public void TestTransform_UpdateSplitDocketLineHCCAndStatus_MutlipleSplits_OffLine()
		{
			TestTransform_UpdateSplitDocketLineHCCAndStatus_MutlipleSplitsCore(isOnline: false);
		}

		void TestTransform_UpdateSplitDocketLineHCCAndStatus_MutlipleSplitsCore(bool isOnline)
		{
			var now = new DateTime(2022, 08, 15);
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "CNNJG" }.AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK)
			{
				WE_StockOnHand = 5m,
				WE_OriginalInventoryStatus = "AVL",
				WE_WHC_NKOriginalInventoryHeldCode = "",
				WE_CurrentInventoryStatus = "AVL",
				WE_WHC_NKCurrentInventoryHeldCode = "",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var receiveLineClone1 = new WhsDocketLine(receive, product.PK, 5m, locationA1.PK)
			{
				WE_WE_ParentDocketLine = receiveLine,
				WE_StockOnHand = isOnline ? 0m : 3m,
				WE_OriginalInventoryStatus = "HEL",
				WE_WHC_NKOriginalInventoryHeldCode = "DAM",
				WE_CurrentInventoryStatus = "HEL",
				WE_WHC_NKCurrentInventoryHeldCode = "DAM",
				WE_PackageGroupId = "W00000001-001",
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = receiveLine,
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var receiveLineClone2 = new WhsDocketLine(receive, product.PK, 2m, locationA1.PK)
			{
				WE_WE_ParentDocketLine = receiveLineClone1,
				WE_StockOnHand = isOnline ? 0m : 2m,
				WE_OriginalInventoryStatus = "HEL",
				WE_WHC_NKOriginalInventoryHeldCode = "QC",
				WE_CurrentInventoryStatus = "HEL",
				WE_WHC_NKCurrentInventoryHeldCode = "QC",
				WE_PackageGroupId = "W00000001-001",
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = receiveLine,
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			if (isOnline)
			{
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "W00000002")
				{
					WD_FinalisedDate = now,
					WD_GS_NKFinalizedBy = "T",
					WD_SystemLastEditUser = "Bob"
				}.AppendInsertAndReturnObject(sql);
				var transferLine1 = new WhsDocketLine(transfer, product.PK, 3m, locationA2.PK)
				{
					WE_WL_TransferFrom = locationA1.PK,
					WE_StockOnHand = 3m
				}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);
				var transferLine2 = new WhsDocketLine(transfer, product.PK, 2m, locationA2.PK)
				{
					WE_WL_TransferFrom = locationA1.PK,
					WE_StockOnHand = 2m
				}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);
				new WhsPickLine(receiveLineClone1, transferLine1, 3m)
				{
					WZ_PickedDateTime = now,
					WZ_GS_NKAssignedTo = "E"
				}.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLineClone2, transferLine2, 2m)
				{
					WZ_PickedDateTime = now,
					WZ_GS_NKAssignedTo = "E"
				}.AppendInsertAndReturnObject(sql);
			}

			new StmALog(receiveLineClone1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=DAM|RES=Wanted|TYP=Hold Code", SL_EventTime = now.AddYears(-2), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
			new StmALog(receiveLineClone2.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "|FDT=2022-12-22T09:38:47|NEW=QC|OLD=DAM|RES=Need|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			if (isOnline)
			{
				GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
				GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			}
			else
			{
				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			}

			// convert back to current schema now that transform has run.
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsDocketLine ALTER COLUMN WE_AdjustmentArrivalDate datetimeoffset(0) NULL");

			WhsDocketLine.AssertFromDB(TestConnection, receiveLineClone1.PK)
				.ExpectEquals("WE_WE_ParentDocketLine", r => r.WE_WE_ParentDocketLine, receiveLine)
				.ExpectEquals("WE_OriginalInventoryStatus", r => r.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals("WE_WHC_NKOriginalInventoryHeldCode", r => r.WE_WHC_NKOriginalInventoryHeldCode, "")
				.ExpectEquals("WE_CurrentInventoryStatus", r => r.WE_CurrentInventoryStatus, "HEL")
				.ExpectEquals("WE_WHC_NKCurrentInventoryHeldCode", r => r.WE_WHC_NKCurrentInventoryHeldCode, "DAM")
				.VerifyAll();

			var newLog1 = StmALog.ShallowLoadFromDB(TestConnection, r => r.SL_Parent == receiveLineClone1.PK && r.SL_GS_NKUser == "~BP").Single();
			newLog1
				.BuildAssertion(TestConnection)
				.ExpectEquals("SL_SE_NKEvent", l => l.SL_SE_NKEvent, "MIS")
				.ExpectEquals("SL_Table", l => l.SL_Table, WhsDocketLineSchema.Constants.TableName)
				.ExpectEquals("SL_Reference", l => l.SL_Reference, "TransformStmALogToWhsInventoryHoldChangeLog: Updated WE_OriginalInventoryStatus from 'HEL' to 'AVL' and WE_WHC_NKOriginalInventoryHeldCode from 'DAM' to ''.")
				.VerifyAll();

			var holdChangeLog1 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLineClone1.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog1
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_WHC_NKCode", l => l.WHL_WHC_NKCode, "DAM")
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "Wanted")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, receiveLineClone2.PK)
				.ExpectEquals("WE_WE_ParentDocketLine", r => r.WE_WE_ParentDocketLine, receiveLineClone1)
				.ExpectEquals("WE_OriginalInventoryStatus", r => r.WE_OriginalInventoryStatus, "HEL")
				.ExpectEquals("WE_WHC_NKOriginalInventoryHeldCode", r => r.WE_WHC_NKOriginalInventoryHeldCode, "DAM")
				.ExpectEquals("WE_CurrentInventoryStatus", r => r.WE_CurrentInventoryStatus, "HEL")
				.ExpectEquals("WE_WHC_NKCurrentInventoryHeldCode", r => r.WE_WHC_NKCurrentInventoryHeldCode, "QC")
				.VerifyAll();

			var newLog2 = StmALog.ShallowLoadFromDB(TestConnection, r => r.SL_Parent == receiveLineClone2.PK && r.SL_GS_NKUser == "~BP").Single();
			newLog2
				.BuildAssertion(TestConnection)
				.ExpectEquals("SL_SE_NKEvent", l => l.SL_SE_NKEvent, "MIS")
				.ExpectEquals("SL_Table", l => l.SL_Table, WhsDocketLineSchema.Constants.TableName)
				.ExpectEquals("SL_Reference", l => l.SL_Reference, "TransformStmALogToWhsInventoryHoldChangeLog: Updated WE_OriginalInventoryStatus from 'HEL' to 'HEL' and WE_WHC_NKOriginalInventoryHeldCode from 'QC' to 'DAM'.")
				.VerifyAll();

			var holdChangeLog2 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLineClone2.PK && r.WHL_LogVersion == 1).Single();
			holdChangeLog2
				.BuildAssertion(TestConnection)
				.ExpectEquals("WHL_WHC_NKCode", l => l.WHL_WHC_NKCode, "QC")
				.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "Need")
				.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
				.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
				.VerifyAll();
		}

		public void TestTransform_UpdateSplitDocketLineHCCAndStatus_LogDoesNotContainHCC()
		{
			var now = new DateTime(2021, 08, 15);
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "T", WD_SystemLastEditUser = "Bob" }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK)
			{
				WE_StockOnHand = 5m,
				WE_OriginalInventoryStatus = "AVL",
				WE_WHC_NKOriginalInventoryHeldCode = "",
				WE_CurrentInventoryStatus = "AVL",
				WE_WHC_NKCurrentInventoryHeldCode = "",
				WE_PackageGroupId = "W00000001-001",
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var receiveLineClone = new WhsDocketLine(receive, product.PK, 5m, location.PK)
			{
				WE_WE_ParentDocketLine = receiveLine,
				WE_StockOnHand = 5m,
				WE_OriginalInventoryStatus = "HEL",
				WE_WHC_NKOriginalInventoryHeldCode = "DAM",
				WE_CurrentInventoryStatus = "HEL",
				WE_WHC_NKCurrentInventoryHeldCode = "DAM",
				WE_PackageGroupId = "W00000001-001",
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = receiveLine,
			}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new StmALog(receiveLineClone.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|RES=Wanted", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("No new WhsInventoryHoldChangeLogs should be added", 0, WhsInventoryHoldChangeLog.ShallowLoadFromDB(TestConnection, r => r.WHL_WE_ParentDocketLine == receiveLineClone.PK).Length);
			AssertEquals("No new StmALogs should be added", 0, StmALog.ShallowLoadFromDB(TestConnection, r => r.SL_Parent == receiveLineClone.PK && r.SL_GS_NKUser == "~BP").Length);

			// convert back to current schema now that transform has run.
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsDocketLine ALTER COLUMN WE_AdjustmentArrivalDate datetimeoffset(0) NULL");

			WhsDocketLine.AssertFromDB(TestConnection, receiveLineClone.PK)
				.ExpectEquals("WE_WE_ParentDocketLine", r => r.WE_WE_ParentDocketLine, receiveLine)
				.ExpectEquals("WE_OriginalInventoryStatus", r => r.WE_OriginalInventoryStatus, "HEL")
				.ExpectEquals("WE_WHC_NKOriginalInventoryHeldCode", r => r.WE_WHC_NKOriginalInventoryHeldCode, "DAM")
				.ExpectEquals("WE_CurrentInventoryStatus", r => r.WE_CurrentInventoryStatus, "HEL")
				.ExpectEquals("WE_WHC_NKCurrentInventoryHeldCode", r => r.WE_WHC_NKCurrentInventoryHeldCode, "DAM")
				.VerifyAll();
		}

		#endregion

		#region TestIndexProvider

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Transform StmALog to WhsInventoryHoldChangeLog._1] ON [dbo].[WhsDocketLine] ([WE_SystemLastEditTimeUtc], [WE_PK]) WHERE (([WE_DocketLineType] IN ('INW', 'ADJ', 'TFR')) AND [WE_TransactionQuantity]>=(0) AND [WE_StockOnHand]>(0) AND [WE_SystemLastEditTimeUtc] IS NOT NULL) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Transform StmALog to WhsInventoryHoldChangeLog._2] ON [dbo].[WhsDocketLine] ([WE_IsOriginalInventory], [WE_PK], [WE_WHC_NKOriginalInventoryHeldCode]) INCLUDE ([WE_AutoVersion], [WE_OriginalInventoryStatus], [WE_StockOnHand], [WE_SystemLastEditTimeUtc], [WE_SystemLastEditUser]) WHERE ([WE_WE_ParentDocketLine] IS NOT NULL AND [WE_IsOriginalInventory]=(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Transform StmALog to WhsInventoryHoldChangeLog._3] ON [dbo].[WhsDocketLine] ([WE_AdjustmentArrivalDate]) INCLUDE ([WE_DocketLineType], [WE_PK], [WE_StockOnHand]) WHERE ([WE_StockOnHand]>(0) AND ([WE_DocketLineType] IN ('INW', 'ADJ', 'TFR')) AND [WE_AdjustmentArrivalDate] IS NOT NULL) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

		public void TestIndexProvider_AfterOnlinePreUpgrade()
		{
			var transformation = (TransformStmALogToWhsInventoryHoldChangeLog)GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertContainsExactElementsInAnyOrder
			(
				expected: new[]
				{
					"NONCLUSTERED INDEX [_WTG__Transform StmALog to WhsInventoryHoldChangeLog._1] ON [dbo].[WhsDocketLine] ([WE_SystemLastEditTimeUtc], [WE_PK]) WHERE (([WE_DocketLineType] IN ('INW', 'ADJ', 'TFR')) AND [WE_TransactionQuantity]>=(0) AND [WE_StockOnHand]>(0) AND [WE_SystemLastEditTimeUtc] IS NOT NULL) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
					"NONCLUSTERED INDEX [_WTG__Transform StmALog to WhsInventoryHoldChangeLog._2] ON [dbo].[WhsDocketLine] ([WE_IsOriginalInventory], [WE_PK], [WE_WHC_NKOriginalInventoryHeldCode]) INCLUDE ([WE_AutoVersion], [WE_OriginalInventoryStatus], [WE_StockOnHand], [WE_SystemLastEditTimeUtc], [WE_SystemLastEditUser]) WHERE ([WE_WE_ParentDocketLine] IS NOT NULL AND [WE_IsOriginalInventory]=(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
					"NONCLUSTERED INDEX [_WTG__Transform StmALog to WhsInventoryHoldChangeLog._3] ON [dbo].[WhsDocketLine] ([WE_AdjustmentArrivalDate]) INCLUDE ([WE_DocketLineType], [WE_PK], [WE_StockOnHand]) WHERE ([WE_StockOnHand]>(0) AND ([WE_DocketLineType] IN ('INW', 'ADJ', 'TFR')) AND [WE_AdjustmentArrivalDate] IS NOT NULL) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
				},
				((ITransformationIndexProvider)transformation).IndexProvider.Select(index => index.Definition)
			);

			// should be robust to running the online transform twice
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertContainsExactElementsInAnyOrder
			(
				expected: new[]
				{
					"NONCLUSTERED INDEX [_WTG__Transform StmALog to WhsInventoryHoldChangeLog._1] ON [dbo].[WhsDocketLine] ([WE_SystemLastEditTimeUtc], [WE_PK]) WHERE (([WE_DocketLineType] IN ('INW', 'ADJ', 'TFR')) AND [WE_TransactionQuantity]>=(0) AND [WE_StockOnHand]>(0) AND [WE_SystemLastEditTimeUtc] IS NOT NULL) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
					"NONCLUSTERED INDEX [_WTG__Transform StmALog to WhsInventoryHoldChangeLog._2] ON [dbo].[WhsDocketLine] ([WE_IsOriginalInventory], [WE_PK], [WE_WHC_NKOriginalInventoryHeldCode]) INCLUDE ([WE_AutoVersion], [WE_OriginalInventoryStatus], [WE_StockOnHand], [WE_SystemLastEditTimeUtc], [WE_SystemLastEditUser]) WHERE ([WE_WE_ParentDocketLine] IS NOT NULL AND [WE_IsOriginalInventory]=(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
					"NONCLUSTERED INDEX [_WTG__Transform StmALog to WhsInventoryHoldChangeLog._3] ON [dbo].[WhsDocketLine] ([WE_AdjustmentArrivalDate]) INCLUDE ([WE_DocketLineType], [WE_PK], [WE_StockOnHand]) WHERE ([WE_StockOnHand]>(0) AND ([WE_DocketLineType] IN ('INW', 'ADJ', 'TFR')) AND [WE_AdjustmentArrivalDate] IS NOT NULL) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
				},
				((ITransformationIndexProvider)transformation).IndexProvider.Select(index => index.Definition)
			);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			new DbColumnDependencyRemover(WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.WE_AdjustmentArrivalDate).DropRelateObjects(TestConnection);
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsDocketLine ALTER COLUMN WE_AdjustmentArrivalDate smalldatetime NULL");
		}

		void InsertRefUNLOCOUtcOffset()
		{
			TestConnection.ExecuteNonQuery($@"
IF EXISTS(SELECT NULL FROM RefUNLOCOUtcOffset WHERE RLO_RL_NKCode = 'CNNJG' AND RLO_StartTimeUtc <= '2010-01-01 00:00:00' AND RLO_EndTimeUtc >= '2032-12-30 00:00:00')
BEGIN
	UPDATE dbo.RefUNLOCOUtcOffset
		SET RLO_OffsetMinutesFromUtc = 480
		WHERE RLO_RL_NKCode = 'CNNJG' AND RLO_StartTimeUtc <= '2010-01-01 00:00:00' AND RLO_EndTimeUtc >= '2022-12-30 00:00:00'
END
ELSE
BEGIN
	INSERT INTO dbo.RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc, RLO_SystemCreateTimeUtc, RLO_SystemCreateUser, RLO_SystemLastEditTimeUtc, RLO_SystemLastEditUser) VALUES
	(newid(), 'CNNJG', '2011-01-01 00:00:00', '2032-01-01 00:00:00', 480, GetUTCDate(), '~BP', GetUTCDate(), '~BP')
END

IF EXISTS(SELECT NULL FROM RefUNLOCOUtcOffset WHERE RLO_RL_NKCode = 'AUSYD' AND RLO_StartTimeUtc <= '2010-01-01 16:00:00' AND RLO_EndTimeUtc >= '2032-12-30 16:00:00')
BEGIN
	UPDATE dbo.RefUNLOCOUtcOffset
		SET RLO_OffsetMinutesFromUtc = 600
		WHERE RLO_RL_NKCode = 'AUBNE' AND RLO_StartTimeUtc <= '2010-01-01 16:00:00' AND RLO_EndTimeUtc >= '2022-12-30 16:00:00'
END
ELSE
BEGIN
	INSERT INTO dbo.RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc, RLO_SystemCreateTimeUtc, RLO_SystemCreateUser, RLO_SystemLastEditTimeUtc, RLO_SystemLastEditUser) VALUES
	(newid(), 'AUBNE', '2011-04-03 16:00:00', '2032-10-02 16:00:00', 600, GetUTCDate(), '~BP', GetUTCDate(), '~BP')
END
");
		}

		#endregion
	}

	class TransformStmALogToWhsInventoryHoldChangeLogTestNoTransaction : TestCase
	{
		[UseSnapshotProtection]
		public void TestTransformWithNoTransaction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				new DbColumnDependencyRemover(WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.WE_AdjustmentArrivalDate).DropRelateObjects(connection);
				connection.ExecuteNonQuery("ALTER TABLE WhsDocketLine ALTER COLUMN WE_AdjustmentArrivalDate smalldatetime NULL");

				var now = DateTime.Now;
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(connection);
				var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
				var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "W00000001")
				{
					WD_FinalisedDate = now,
					WD_GS_NKFinalizedBy = "T",
					WD_SystemLastEditUser = "Bob"
				}
				.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK)
				{
					WE_StockOnHand = 0m,
					WE_OriginalInventoryStatus = "AVL",
				}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);

				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "W00000002")
				{
					WD_FinalisedDate = now,
					WD_GS_NKFinalizedBy = "T",
					WD_SystemLastEditUser = "Bob"
				}.AppendInsertAndReturnObject(sql);
				var transferLine1 = new WhsDocketLine(transfer, product.PK, 10, locationA2.PK)
				{
					WE_WL_TransferFrom = locationA1.PK,
					WE_StockOnHand = 10m,
				}.InsertAsDateTimeForColumn(l => l.WE_AdjustmentArrivalDate).AppendToInsert(sql);
				var pickLine = new WhsPickLine(receiveLine, transferLine1, 10m)
				{
					WZ_PickedDateTime = now,
					WZ_GS_NKAssignedTo = "E"
				}.AppendInsertAndReturnObject(sql);

				new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2022-12-22T09:38:47|NEW=LCC|RES=Wrong|TYP=Hold Code", SL_EventTime = now.AddYears(-1), SL_GS_NKUser = "M01" }.AppendInsertAndReturnObject(sql);
				new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T12:34:17|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);
				new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T13:17:59|NEW=SHORT|RES=BROKEN|TYP=Hold Code", SL_EventTime = now.AddMonths(-2), SL_GS_NKUser = "M03" }.AppendInsertAndReturnObject(sql);
				new StmALog(receiveLine.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'HEL' to 'AVL'|FDT=2016-11-24T13:19:16|OLD=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-1), SL_GS_NKUser = "M04" }.AppendInsertAndReturnObject(sql);
				new StmALog(transferLine1.PK, WhsDocketLineSchema.Constants.TableName, "CID") { SL_Reference = "Status Changed from 'AVL' to 'HEL'|FDT=2016-11-24T12:34:17|NEW=HEL|TYP=Hold Code", SL_EventTime = now.AddMonths(-3), SL_GS_NKUser = "M02" }.AppendInsertAndReturnObject(sql);

				using (connection.BeginTransactionWithManager())
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				{
					connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					connection.CommitTransaction();
				}

				new TransformStmALogToWhsInventoryHoldChangeLog().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

				var holdChangeLogOnTransferLine = WhsInventoryHoldChangeLog.ShallowLoadFromDB(connection, r => r.WHL_WE_ParentDocketLine == transferLine1.PK).Single();
				holdChangeLogOnTransferLine
					.BuildAssertion(connection)
					.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
					.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
					.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
					.VerifyAll();

				new TransformStmALogToWhsInventoryHoldChangeLog().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

				var holdChangeLog1 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(connection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK && r.WHL_LogVersion == 1).Single();
				holdChangeLog1
					.BuildAssertion(connection)
					.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "Wrong")
					.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M01")
					.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M01")
					.VerifyAll();

				var holdChangeLog2 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(connection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK && r.WHL_LogVersion == 2).Single();
				holdChangeLog2
					.BuildAssertion(connection)
					.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
					.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M02")
					.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M02")
					.VerifyAll();

				var holdChangeLog3 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(connection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK && r.WHL_LogVersion == 3).Single();
				holdChangeLog3
					.BuildAssertion(connection)
					.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "BROKEN")
					.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M03")
					.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M03")
					.VerifyAll();

				var holdChangeLog4 = WhsInventoryHoldChangeLog.ShallowLoadFromDB(connection, r => r.WHL_WE_ParentDocketLine == receiveLine.PK && r.WHL_LogVersion == 4).Single();
				holdChangeLog4
					.BuildAssertion(connection)
					.ExpectEquals("WHL_Reason", l => l.WHL_Reason, "")
					.ExpectEquals("WHL_SystemCreateUser", l => l.WHL_SystemCreateUser, "M04")
					.ExpectEquals("WHL_SystemLastEditUser", l => l.WHL_SystemLastEditUser, "M04")
					.VerifyAll();
			}
		}
	}
}
