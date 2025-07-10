using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
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
	[TestedType(typeof(RemovePackingNumberFountainsForFinalizedWarehousePackageJobs))]
	class RemovePackingNumberFountainsForFinalizedWarehousePackageJobsTest : DataTransformationTestCase
	{
		#region OnlinePreUpgradeTransform

		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true);
			var packageJob = SetupPackageWithPackingParent(sql, order.PK, "WD", "JOB1");

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			SetupNumberFountain(GetPackingIdNumberFountainName(order.WD_ExternalReference), packageJob.PK);
			AssertEquals("Precondition", 1, GetNumberFountainsCountForPackageJob(packageJob.PK));
		}

		protected override void AssertTransformationResults()
		{
			var order = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketID == "O00000001").Single();
			var packageJob = PkgPackageJob.ShallowLoadFromDB(TestConnection, p => p.KJ_ParentID == order.PK).Single();
			AssertEquals("Number fountains deleted for finalized packing job.", 0, GetNumberFountainsCountForPackageJob(packageJob.PK));
		}

		public void TestOnlinePreUpgrade_NotFinalisedPackageJob()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: false);
			var packageJob = SetupPackageWithPackingParent(sql, order.PK, "WD", "JOB1");

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var fountainName = GetPackingIdNumberFountainName(order.WD_ExternalReference);
			SetupNumberFountain(fountainName, packageJob.PK);
			AssertEquals("Precondition", 1, GetNumberFountainsCountForPackageJob(packageJob.PK));

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Number fountains not deleted for not finalized packing job.", 1, GetNumberFountainsCountForPackageJob(packageJob.PK));

			var maxPickFinalizedDateString = ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate);
			AssertNull(maxPickFinalizedDateString);
		}

		public void TestOnlinePreUpgrade_FinalisedPackageJob_MultipleNumberFountains()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var pickFinalisedDate = new DateTime(2025, 2, 14);
			var order1 = SetupWhsOrderPackingParentWithSpecificFinalisedDate(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", pickFinalisedDate, isFinalised: true, index: 1);
			var packageJob1 = SetupPackageWithPackingParent(sql, order1.PK, "WD", "JOB1");

			var order2 = SetupWhsOrderPackingParentWithSpecificFinalisedDate(sql, client, whs, product.PK, location.PK, "O00000002", "O00000002", pickFinalisedDate, isFinalised: true, index: 2);
			var packageJob2 = SetupPackageWithPackingParent(sql, order2.PK, "WD", "JOB2");

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var fountainName = GetPackingIdNumberFountainName(order1.WD_ExternalReference);
			SetupNumberFountain(fountainName, packageJob1.PK, sequence: 1);
			SetupNumberFountain(fountainName, packageJob1.PK, sequence: 2);
			SetupNumberFountain(fountainName, packageJob1.PK, sequence: 3);
			SetupNumberFountain(GetPackingIdNumberFountainName(order2.WD_ExternalReference), packageJob2.PK);
			AssertEquals("Precondition", 4, GetPackingNumberFountainsCount());

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Number fountains deleted for finalized packing job.", 0, GetPackingNumberFountainsCount());

			var maxPickFinalizedDateString = ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate);
			DateTime.TryParse(maxPickFinalizedDateString, out var maxPickFinalizedDate);
			AssertEquals(pickFinalisedDate, maxPickFinalizedDate);
		}

		public void TestOnlinePreUpgrade_FinalisedPackageJob_MultipleNumberFountains_WithNonPackingFountain()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true);
			var packageJob = SetupPackageWithPackingParent(sql, order.PK, "WD", "JOB1");

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var fountainName = GetPackingIdNumberFountainName(order.WD_ExternalReference);
			SetupNumberFountain(fountainName, packageJob.PK, sequence: 1);
			SetupNumberFountain(fountainName, packageJob.PK, sequence: 2);
			SetupNumberFountain("SomeFountain", packageJob.PK, sequence: 1);
			AssertEquals("Precondition", 3, GetNumberFountainsCountForPackageJob(packageJob.PK));
			AssertEquals("Precondition", 2, GetPackingNumberFountainsCount());

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Only packing number fountains deleted for finalized packing job.", 1, GetNumberFountainsCountForPackageJob(packageJob.PK));

			var maxPickFinalizedDateString = ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate);
			AssertNotNull(maxPickFinalizedDateString);
		}

		public void TestBatching()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var orderRefToPkgJobPkDictionary = new Dictionary<Guid, string>();
			BatchInsertDataPrep(createCount: 1005);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			InsertNumberFountains();
			AssertEquals("Number Fountains count for finalized package jobs before transform.", 1005, GetPackingNumberFountainsCount());
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Number Fountains count for finalized package jobs after transform.", 0, GetPackingNumberFountainsCount());

			var maxPickFinalizedDateString = ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate);
			AssertNotNull(maxPickFinalizedDateString);

			void InsertNumberFountains()
			{
				var numFountainInsertSql = new StringBuilder();
				foreach(var orderToPkgJob in orderRefToPkgJobPkDictionary)
				{
					numFountainInsertSql.AppendLine(@$"INSERT dbo.StmNums (SN_Name, SN_Owner, SN_MinimumValue, SN_Value, SN_MaximumValue, SN_Sequence) VALUES
	('{orderToPkgJob.Value}', '{orderToPkgJob.Key.ToString()}', 1, 1, 9220000000000000000, 1);");
				}

				Db.Connection.ExecuteNonQuery(numFountainInsertSql.ToString());
			}

			void BatchInsertDataPrep(int createCount)
			{
				var receives = new WhsDocket[createCount];
				var receiveLines = new WhsDocketLine[createCount];
				var picks = new WhsPick[createCount];
				var orders = new WhsDocket[createCount];
				var orderLines = new WhsDocketLine[createCount];
				var pickLines = new WhsPickLine[createCount];
				var packageJobs = new PkgPackageJob[createCount];
				var packageHeaders = new PkgPackageHeader[createCount];
				var packages = new PkgPackage[createCount];

				for (var i = 0; i < createCount; i++)
				{
					var index = i + 1;
					var today = DateTime.Today;
					var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"R{index}") { WD_FinalisedDate = today };
					receives[i] = receive;

					var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK)
					{
						WE_OriginalInventoryStatus = "AVL",
						WE_CurrentInventoryStatus = "AVL",
						WE_PalletID = "ABC",
						WE_StockOnHand = 0m,
						WE_UnloadedTime = new DateTimeOffset(today),
						WE_GS_NKUnloadedBy = "A",
						WE_AdjustmentArrivalDate = today,
						WE_SystemCreateTimeUtc = DateTime.UtcNow,
						WE_SystemLastEditTimeUtc = DateTime.UtcNow,
						WE_SystemCreateUser = "A",
						WE_SystemLastEditUser = "A"
					};
					receiveLines[i] = receiveLine;

					var pick = new WhsPick(whs, $"P{index}", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" };
					picks[i] = pick;

					var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", $"O{index}", $"O{index}") { WD_FinalisedDate = today, WD_WP = pick.PK, WD_GS_NKFinalizedBy = "E" };
					orders[i] = order;

					var orderLine = new WhsDocketLine(order, product.PK, 10m) { WE_FinalisedDate = today, WE_DocketLineStatus = "DEP" };
					orderLines[i] = orderLine;

					pickLines[i] = new WhsPickLine(receiveLine, orderLine, 10m)
					{
						WZ_PickedDateTime = today,
						WZ_GS_NKAssignedTo = "US1",
						WZ_SystemCreateTimeUtc = DateTime.UtcNow,
						WZ_SystemLastEditTimeUtc = DateTime.UtcNow,
						WZ_SystemCreateUser = "A",
						WZ_SystemLastEditUser = "A"
					};

					var packageJob = new PkgPackageJob(order.PK) { KJ_ParentTableCode = "WD", KJ_JobID = $"JOB{index}" };
					packageJobs[i] = packageJob;

					var packageHeader = new PkgPackageHeader($"PKG{index}", DateTime.UtcNow, "~BP");
					packageHeaders[i] = packageHeader;

					packages[i] = new PkgPackage(packageJob, "CNT", 1) { KP_KPH_PackageHeader = packageHeader };

					orderRefToPkgJobPkDictionary[packageJob.PK] = GetPackingIdNumberFountainName(order.WD_ExternalReference);
				}

				sql.AppendLine(WhsDocket.GetBulkInsertStatement(receives));
				sql.AppendLine(WhsDocketLine.GetBulkInsertStatement(receiveLines));
				sql.AppendLine(WhsPick.GetBulkInsertStatement(picks));
				sql.AppendLine(WhsDocket.GetBulkInsertStatement(orders));
				sql.AppendLine(WhsDocketLine.GetBulkInsertStatement(orderLines));
				sql.AppendLine(WhsPickLine.GetBulkInsertStatement(pickLines));
				sql.AppendLine(PkgPackageJob.GetBulkInsertStatement(packageJobs));
				sql.AppendLine(PkgPackageHeader.GetBulkInsertStatement(packageHeaders));
				sql.AppendLine(PkgPackage.GetBulkInsertStatement(packages));
			}
		}

		public void TestOnlinePreUpgrade_LastPackingFountainsDeleteTimeUtcRegistryHasValue()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true);
			var packageJob = SetupPackageWithPackingParent(sql, order.PK, "WD", "JOB1");

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			RegistryHelper.InsertStmDataRow(RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastPackingFountainsDeleteTimeUtcRegistryName, "DT", Encoding.Unicode.GetBytes(DateTime.Today.ToString()));
			SetupNumberFountain(GetPackingIdNumberFountainName(order.WD_ExternalReference), packageJob.PK);
			AssertEquals("Precondition", 1, GetNumberFountainsCountForPackageJob(packageJob.PK));

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Number fountains not deleted.", 1, GetNumberFountainsCountForPackageJob(packageJob.PK));

			var maxPickFinalizedDateString = ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate);
			AssertNull(maxPickFinalizedDateString);
		}

		public void TestOnlinePreUpgrade_LastPackingFountainsDeleteTimeUtcRegistryWithDefaultValue()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true);
			var packageJob = SetupPackageWithPackingParent(sql, order.PK, "WD", "JOB1");

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			RegistryHelper.InsertStmDataRow(RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastPackingFountainsDeleteTimeUtcRegistryName, "DT", Encoding.Unicode.GetBytes(new DateTime(1900, 1, 1).ToString()));
			SetupNumberFountain(GetPackingIdNumberFountainName(order.WD_ExternalReference), packageJob.PK);
			AssertEquals("Precondition", 1, GetNumberFountainsCountForPackageJob(packageJob.PK));

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Number fountains deleted.", 0, GetNumberFountainsCountForPackageJob(packageJob.PK));

			var maxPickFinalizedDateString = ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate);
			AssertNotNull(maxPickFinalizedDateString);
		}

		public void TestOnlinePreUpgrade_ContinuesFromLastChunkedPK()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order1 = SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true, index: 1);
			var packageJob1 = SetupPackageWithPackingParent(sql, order1.PK, "WD", "JOB1");

			var order2 = SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000002", "O00000002", isFinalised: true, index: 2);
			var packageJob2 = SetupPackageWithPackingParent(sql, order2.PK, "WD", "JOB2");

			var order3 = SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000003", "O00000003", isFinalised: true, index: 3);
			var packageJob3 = SetupPackageWithPackingParent(sql, order3.PK, "WD", "JOB3");

			var order4 = SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000004", "O00000004", isFinalised: true, index: 4);
			var packageJob4 = SetupPackageWithPackingParent(sql, order4.PK, "WD", "JOB4");

			var order5 = SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000005", "O00000005", isFinalised: true, index: 5);
			var packageJob5 = SetupPackageWithPackingParent(sql, order5.PK, "WD", "JOB5");

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var minPK = (Guid)TestConnection.ExecuteScalar("SELECT MIN(KJ_PK) FROM PkgPackageJob");
			ExtProperty.Database.Update(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastProcessedChunkPKName, minPK.ToString());

			SetupNumberFountain(GetPackingIdNumberFountainName(order1.WD_ExternalReference), packageJob1.PK);
			SetupNumberFountain(GetPackingIdNumberFountainName(order2.WD_ExternalReference), packageJob2.PK);
			SetupNumberFountain(GetPackingIdNumberFountainName(order3.WD_ExternalReference), packageJob3.PK);
			SetupNumberFountain(GetPackingIdNumberFountainName(order4.WD_ExternalReference), packageJob4.PK);
			SetupNumberFountain(GetPackingIdNumberFountainName(order5.WD_ExternalReference), packageJob5.PK);
			AssertEquals("Precondition", 5, GetPackingNumberFountainsCount());

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals(1, GetPackingNumberFountainsCount());

			var maxPickFinalizedDateString = ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate);
			AssertNotNull(maxPickFinalizedDateString);
		}

		public void TestOnlinePreUpgrade_TableDoesNotExist_WhsPick()
		{
			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, WhsPickSchema.Constants.TableName, WhsPickSchema.Constants.PK).DropRelateObjects(Db.Connection);
			DbObjectCreator.DropTableIfExists(Db.Connection, WhsPickSchema.Constants.TableName);

			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));
		}

		public void TestOnlinePreUpgrade_TableDoesNotExist_PkgPackageJob()
		{
			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, PkgPackageJobSchema.Constants.TableName, PkgPackageJobSchema.Constants.PK).DropRelateObjects(Db.Connection);
			DbObjectCreator.DropTableIfExists(Db.Connection, PkgPackageJobSchema.Constants.TableName);

			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));
		}

		public void TestOnlinePreUpgrade_TableDoesNotExist_StmNums()
		{
			TestConnection.ExecuteNonQuery(@"
DROP PROCEDURE IF EXISTS dbo.FountainGetNexts;
DROP PROCEDURE IF EXISTS dbo.FountainReleaseAppLock;
DROP PROCEDURE IF EXISTS dbo.FountainSetValues;
DROP PROCEDURE IF EXISTS dbo.FountainSetValuesStrategy;
DROP VIEW IF EXISTS dbo.ViewStmNums;
DROP TRIGGER IF EXISTS dbo.TG_StmNums_Delete;
DROP TRIGGER IF EXISTS dbo.TG_StmNums_Insert;
DROP TRIGGER IF EXISTS dbo.TG_StmNums_Update;
DROP TRIGGER IF EXISTS dbo.TG_StmNums_PreventChangeUsedRows_Update;
");
			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "StmNums", "SN_Id").DropRelateObjects(Db.Connection);
			DbObjectCreator.DropTableIfExists(Db.Connection, "StmNums");

			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));
		}

		#endregion

		#region OfflinePostUpgradeTransform

		public void TestOfflinePreUpgrade_PopulatesPackingFountainsDeleteTimeUtcRegistryValue()
		{
			var newLastPackingFountainsDeleteTimeUtc = new DateTime(2025, 2, 14);
			ExtProperty.Database.Update(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate, newLastPackingFountainsDeleteTimeUtc.ToString());
			ExtProperty.Database.Update(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastProcessedChunkPKName, Guid.Empty.ToString());

			AssertEquals("Precondition", 0, RegistryHelper.GetStmDataRowCount(RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastPackingFountainsDeleteTimeUtcRegistryName));

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals(1, RegistryHelper.GetStmDataRowCount(RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastPackingFountainsDeleteTimeUtcRegistryName));
			var binValue = new RegistryTransformationHelper().GetStmDataValue(RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastPackingFountainsDeleteTimeUtcRegistryName);
			AssertNotNull(binValue);
			DateTime.TryParse(Encoding.Unicode.GetString(binValue), out var result);
			AssertEquals(result, newLastPackingFountainsDeleteTimeUtc);

			AssertNull(ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate));
			AssertNull(ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastProcessedChunkPKName));
		}

		public void TestOfflinePreUpgrade_UpdatesPackingFountainsDeleteTimeUtcRegistryValue()
		{
			RegistryHelper.InsertStmDataRow(RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastPackingFountainsDeleteTimeUtcRegistryName, "DT", Encoding.Unicode.GetBytes(new DateTime(1900, 1, 1).ToString()));

			var newLastPackingFountainsDeleteTimeUtc = new DateTime(2025, 2, 14);
			ExtProperty.Database.Update(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate, newLastPackingFountainsDeleteTimeUtc.ToString());
			ExtProperty.Database.Update(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastProcessedChunkPKName, Guid.Empty.ToString());

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			var binValue = new RegistryTransformationHelper().GetStmDataValue(RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastPackingFountainsDeleteTimeUtcRegistryName);
			AssertNotNull(binValue);
			DateTime.TryParse(Encoding.Unicode.GetString(binValue), out var result);
			AssertEquals(result, newLastPackingFountainsDeleteTimeUtc);

			AssertNull(ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate));
			AssertNull(ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastProcessedChunkPKName));
		}

		public void TestOfflinePreUpgrade_NoStoredMaxPickFinalizedDate()
		{
			AssertEquals("Precondition", 0, RegistryHelper.GetStmDataRowCount(RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastPackingFountainsDeleteTimeUtcRegistryName));
			AssertNull("Precondition", ExtProperty.Database.Select(Db.Connection, RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate));

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals(0, RegistryHelper.GetStmDataRowCount(RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastPackingFountainsDeleteTimeUtcRegistryName));
		}

		#endregion

		#region Implementation

		WhsDocket SetupWhsOrderPackingParentWithSpecificFinalisedDate(SqlQueryBuilder sql, OrgHeader client, WhsWarehouse whs, Guid productPK, Guid locationPK, string orderReference, string docketId, DateTime pickFinalisedDate, bool isFinalised, int index = 1)
		{
			return SetupWhsOrderPackingParentCore(sql, client, whs, productPK, locationPK, orderReference, docketId, pickFinalisedDate, isFinalised, index);
		}

		WhsDocket SetupWhsOrderPackingParent(SqlQueryBuilder sql, OrgHeader client, WhsWarehouse whs, Guid productPK, Guid locationPK, string orderReference, string docketId, bool isFinalised = false, int index = 1)
		{
			return SetupWhsOrderPackingParentCore(sql, client, whs, productPK, locationPK, orderReference, docketId, null, isFinalised, index);
		}

		WhsDocket SetupWhsOrderPackingParentCore(SqlQueryBuilder sql, OrgHeader client, WhsWarehouse whs, Guid productPK, Guid locationPK, string orderReference, string docketId, DateTime? pickFinalisedDate, bool isFinalised = false, int index = 1)
		{
			var today = pickFinalisedDate ?? DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"R{index}") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, productPK, 10m, locationPK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_PalletID = "ABC",
				WE_StockOnHand = 0m,
				WE_UnloadedTime = new DateTimeOffset(today),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = today,
				WE_SystemCreateTimeUtc = DateTime.UtcNow,
				WE_SystemLastEditTimeUtc = DateTime.UtcNow,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			var pickWithOrder = isFinalised
				? new WhsPick(whs, $"P{index}", "FIN") { WP_FinalizedDateUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql)
				: new WhsPick(whs, $"P{index}", "NEW").AppendInsertAndReturnObject(sql);

			var order = isFinalised
				? new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", docketId, orderReference) { WD_FinalisedDate = today, WD_WP = pickWithOrder.PK, WD_GS_NKFinalizedBy = "E" }.AppendInsertAndReturnObject(sql)
				: new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", docketId, orderReference) { WD_WP = pickWithOrder.PK }.AppendInsertAndReturnObject(sql);

			var orderLine = isFinalised
				? new WhsDocketLine(order, productPK, 10m) { WE_FinalisedDate = today, WE_DocketLineStatus = "DEP" }.AppendInsertAndReturnObject(sql)
				: new WhsDocketLine(order, productPK, 10m).AppendInsertAndReturnObject(sql);

			new WhsPickLine(receiveLine, orderLine, 10m)
			{
				WZ_PickedDateTime = today,
				WZ_GS_NKAssignedTo = "US1",
				WZ_SystemCreateTimeUtc = DateTime.UtcNow,
				WZ_SystemLastEditTimeUtc = DateTime.UtcNow,
				WZ_SystemCreateUser = "A",
				WZ_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			return order;
		}

		void SetupNumberFountain(string name, Guid owner, int sequence = 1)
		{
			var sql = @"-- InsertStmNums
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_MinimumValue, SN_Value, SN_MaximumValue, SN_Sequence) VALUES
	(@Name, @Owner, 1, 1, 9220000000000000000, @Sequence);";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@Name", SqlDbType.VarChar, name);
				cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
				cmd.AddParameter("@Sequence", SqlDbType.SmallInt, sequence);
				cmd.ExecuteNonQuery();
			}
		}

		PkgPackageJob SetupPackageWithPackingParent(SqlQueryBuilder sql, Guid packingParent, string parentTableCode, string jobId, int index = 1)
		{
			var packageJob = new PkgPackageJob(packingParent) { KJ_ParentTableCode = parentTableCode, KJ_JobID = jobId }.AppendInsertAndReturnObject(sql);
			var packageHeader = new PkgPackageHeader($"PKG{index}", DateTime.UtcNow, "~BP").AppendInsertAndReturnObject(sql);
			new PkgPackage(packageJob, "CNT", 1) { KP_KPH_PackageHeader = packageHeader }.AppendInsertAndReturnObject(sql);

			return packageJob;
		}

		string GetPackingIdNumberFountainName(string jobId) => $"GeneratorFountain-PKGID-{jobId}";

		int GetNumberFountainsCountForPackageJob(Guid packageJobPK)
		{
			AssertNotEquals(Guid.Empty, packageJobPK);
			return (int)TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.StmNums WHERE SN_Owner = '{packageJobPK}'");
		}

		int GetPackingNumberFountainsCount()
			=> (int)TestConnection.ExecuteScalar($"SELECT COUNT(SN_Name) FROM dbo.StmNums WHERE SN_Name LIKE 'GeneratorFountain-PKGID-%'");

		protected override DataTransformation GetNewTestTransformationInstance() => new RemovePackingNumberFountainsForFinalizedWarehousePackageJobs();

		protected RegistryTransformationTestHelper RegistryHelper => new RegistryTransformationTestHelper();

		protected override void SetUp()
		{
			base.SetUp();
			ClearUndeletedPackageIDFountain();

			// To be properly fixed in WI00895904 - Investigate Leaking Of StmNums From Packing ID Generation
			void ClearUndeletedPackageIDFountain()
	=> TestConnection.ExecuteNonQuery(@"
TRUNCATE TABLE dbo.StmNumberCache;
DELETE dbo.StmNums WHERE SN_Name LIKE 'GeneratorFountain-PKGID%';");
		}

		#endregion
	}
}
