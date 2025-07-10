using System;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(EnableGroupByCustomAttributesRegistry))]
	class EnableGroupByCustomAttributesRegistryTest : RegistryDataTransformationTestCase
	{
		const string RegistryName = "GroupOrderedInventoryByCustomAttributes";

		protected override DataTransformation GetNewTestTransformationInstance() => new EnableGroupByCustomAttributesRegistry();

		#region AssertAndRunTwice

		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW", branchPK: branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var yesterday = DateTime.Today.AddDays(-1);
			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = yesterday }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(receive, product.PK, 10m, location.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_CustomAttrib1 = "ABC",
				WE_StockOnHand = 10m,
				WE_UnloadedTime = new DateTimeOffset(today),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = yesterday,
				WE_SystemCreateTimeUtc = yesterday,
				WE_SystemLastEditTimeUtc = yesterday,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			var branch2 = new GlbBranch("BR2").AppendInsertAndReturnObject(sql);
			var whs2 = new WhsWarehouse("WH2", "PRW", branchPK: branch2.PK).WithDockDoor(sql);
			var row2 = new WhsRow(whs2, "Row7") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area2 = new WhsArea(whs2.PK, "Area6").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var receive2 = new WhsDocket(client.PK, whs2.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = yesterday }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(receive2, product.PK, 10m, location2.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_CustomDate1 = today,
				WE_StockOnHand = 10m,
				WE_UnloadedTime = new DateTimeOffset(today),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = yesterday,
				WE_SystemCreateTimeUtc = yesterday,
				WE_SystemLastEditTimeUtc = yesterday,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			var branch3 = new GlbBranch("BR3").AppendInsertAndReturnObject(sql);
			var whs3 = new WhsWarehouse("WH3", "PRW", branchPK: branch3.PK).WithDockDoor(sql);
			var row3 = new WhsRow(whs3, "Row5") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area3 = new WhsArea(whs3.PK, "Area9").AppendInsertAndReturnObject(sql);
			var location3 = new WhsLocation(row3.PK, area3.PK, area3.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var receive3 = new WhsDocket(client.PK, whs3.PK, "INW", "REC", "FIN", "R3") { WD_FinalisedDate = yesterday }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(receive3, product.PK, 10m, location3.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 10m,
				WE_UnloadedTime = new DateTimeOffset(today),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = yesterday,
				WE_SystemCreateTimeUtc = yesterday,
				WE_SystemLastEditTimeUtc = yesterday,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			ExecuteQuery(sql);

			AssertEquals(0, Helper.GetStmDataRowCount(RegistryName));
		}

		protected override void AssertTransformationResults()
		{
			var branch1 = GlbBranch.ShallowLoadFromDB(TestConnection, d => d.GB_Code == "BR1").Single();
			var branch2 = GlbBranch.ShallowLoadFromDB(TestConnection, d => d.GB_Code == "BR2").Single();
			var branch3 = GlbBranch.ShallowLoadFromDB(TestConnection, d => d.GB_Code == "BR3").Single();
			AssertEquals(2, Helper.GetStmDataRowCount(RegistryName));
			AssertEquals("Branch1 registry item", bool.TrueString, Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName, branch1.PK)));
			AssertEquals("Branch2 registry item", bool.TrueString, Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName, branch2.PK)));
			AssertEquals("Branch3 registry item", null, Helper.GetStmDataValue(RegistryName, branch3.PK));
		}

		#endregion

		#region TestTransformation_NoDockets

		public void TestTransformation_NoDockets()
		{
			var sql = new StringBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW", branchPK: branch1.PK).WithDockDoor(sql);
			ExecuteQuery(sql);

			AssertEquals(0, Helper.GetStmDataRowCount(RegistryName));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals(0, Helper.GetStmDataRowCount(RegistryName));
		}

		#endregion

		#region TestTransformation_Adjustment

		public void TestTransformation_Adjustment()
		{
			var sql = new StringBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW", branchPK: branch1.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var yesterday = DateTime.Today.AddDays(-1);
			var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);
			new WhsDocketLine(adjustment, product.PK, 10m, location.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_CustomAttrib1 = "ABC",
				WE_AdjustmentArrivalDate = yesterday,
				WE_SystemCreateTimeUtc = yesterday,
				WE_SystemLastEditTimeUtc = yesterday,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			ExecuteQuery(sql);

			AssertEquals(0, Helper.GetStmDataRowCount(RegistryName));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals(1, Helper.GetStmDataRowCount(RegistryName));
			AssertEquals("Branch1 registry item", bool.TrueString, Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName, branch1.PK)));
		}

		#endregion

		#region TestTransformation_Transfer

		public void TestTransformation_Transfer()
		{
			var sql = new StringBuilder();

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var branch1 = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2") { GB_RL_NKHomePort = "MAGIC" }.InsertAndReturnObject(TestConnection);

			var whs1 = new WhsWarehouse("WHS", "PRW", branch1.PK).WithDockDoor(TestConnection);
			var row1 = new WhsRow(whs1, "Row1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area1 = new WhsArea(whs1.PK, "Area1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var whs2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(TestConnection);
			var row2 = new WhsRow(whs2, "Row2") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area2 = new WhsArea(whs2.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			PrepareInterWhsPutawayTransfer(sql, whs1, whs2, new DateTimeOffset(2022, 6, 6, 0, 0, 0, TimeSpan.FromHours(0)), docketID: "1", client, product, location1, location2);

			ExecuteQuery(sql);

			AssertEquals(0, Helper.GetStmDataRowCount(RegistryName));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals(2, Helper.GetStmDataRowCount(RegistryName));
			AssertEquals("Branch1 registry item", bool.TrueString, Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName, branch1.PK)));
			AssertEquals("Branch2 registry item", bool.TrueString, Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName, branch2.PK)));
		}

		void PrepareInterWhsPutawayTransfer(
			StringBuilder sql,
			WhsWarehouse sourceWhs,
			WhsWarehouse destWhs,
			DateTimeOffset adjustmentArrivalDate,
			string docketID,
			OrgHeader client,
			OrgSupplierPart product,
			WhsLocation location,
			WhsLocation location2)
		{
			var receive = new WhsDocket(client.PK, sourceWhs.PK, "INW", "REC", "FIN", $"R{docketID}") { WD_FinalisedDate = adjustmentArrivalDate.DateTime }.AppendInsertAndReturnObject(sql);
			var inventory = new WhsDocketLine(receive, product.PK, 10m, location.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 0m,
				WE_AdjustmentArrivalDate = adjustmentArrivalDate,
				WE_FinalisedDate = adjustmentArrivalDate.DateTime,
				WE_CustomFlag1 = true,
			}.InsertAsDateTimeForColumn(d => d.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var sourceTransfer = new WhsDocket(client.PK, sourceWhs.PK, "TFR", "IWS", "FIN", $"TS{docketID}") { WD_FinalisedDate = adjustmentArrivalDate.DateTime }.AppendInsertAndReturnObject(sql);
			var sourceTransferLine = new WhsDocketLine(sourceTransfer, product.PK, 10m, location2.PK)
			{
				WE_LineNo = 1,
				WE_WL_TransferFrom = location.PK,
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "STA",
				WE_DocketLineStatus = "FIN",
				WE_IsOriginalInventory = true,
				WE_GS_NKPutawayBy = "E",
				WE_PutawayTime = adjustmentArrivalDate,
				WE_AdjustmentArrivalDate = adjustmentArrivalDate,
				WE_FinalisedDate = adjustmentArrivalDate.DateTime,
				WE_StockOnHand = 0m,
				WE_CustomFlag1 = true,
			}.InsertAsDateTimeForColumn(d => d.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			var destTransfer = new WhsDocket(client.PK, destWhs.PK, "TFR", "IWD", "FIN", $"TD{docketID}")
			{
				WD_FinalisedDate = adjustmentArrivalDate.DateTime,
				WD_WD_ParentDocket = sourceTransfer
			}.AppendInsertAndReturnObject(sql);
			var destTransferLine = new WhsDocketLine(destTransfer, product.PK, 10m, location2.PK)
			{
				WE_LineNo = 1,
				WE_WL_TransferFrom = location.PK,
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "STA",
				WE_DocketLineStatus = "FIN",
				WE_IsOriginalInventory = true,
				WE_GS_NKPutawayBy = "E",
				WE_PutawayTime = adjustmentArrivalDate,
				WE_AdjustmentArrivalDate = adjustmentArrivalDate,
				WE_FinalisedDate = adjustmentArrivalDate.DateTime,
				WE_StockOnHand = 10m,
				WE_WE_ParentDocketLine = sourceTransferLine,
				WE_CustomFlag1 = true,
			}.InsertAsDateTimeForColumn(d => d.WE_AdjustmentArrivalDate).AppendToInsert(sql);

			new WhsPickLine(inventory, sourceTransferLine, 10m)
			{
				WZ_GS_NKAssignedTo = "E",
				WZ_PickedDateTime = adjustmentArrivalDate.DateTime,
			}.AppendInsertAndReturnObject(sql);
		}

		#endregion

		#region TestTransformation_Order

		public void TestTransformation_Order()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW", branchPK: branch.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var yesterday = DateTime.Today.AddDays(-1);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order, product.PK, 10m)
			{
				WE_CustomAttrib1 = "ABC",
				WE_SystemCreateTimeUtc = yesterday,
				WE_SystemLastEditTimeUtc = yesterday,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			ExecuteQuery(sql);

			AssertEquals(0, Helper.GetStmDataRowCount(RegistryName));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals(1, Helper.GetStmDataRowCount(RegistryName));
			AssertEquals("Branch1 registry item", bool.TrueString, Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName, branch.PK)));
		}

		#endregion

		#region TestTransformation_Columns

		public void TestTransformation_Columns_WE_CustomAttrib2()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomAttrib2 = "test");
		}

		public void TestTransformation_Columns_WE_CustomAttrib3()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomAttrib3 = "test");
		}

		public void TestTransformation_Columns_WE_CustomAttrib4()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomAttrib4 = "test");
		}

		public void TestTransformation_Columns_WE_CustomAttrib5()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomAttrib5 = "test");
		}

		public void TestTransformation_Columns_WE_CustomAttrib6()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomAttrib6 = "test");
		}

		public void TestTransformation_Columns_WE_CustomDate1()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomDate1 = DateTime.UtcNow);
		}

		public void TestTransformation_Columns_WE_CustomDate2()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomDate2 = DateTime.UtcNow);
		}

		public void TestTransformation_Columns_WE_CustomDate3()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomDate3 = DateTime.UtcNow);
		}

		public void TestTransformation_Columns_WE_CustomDate4()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomDate4 = DateTime.UtcNow);
		}

		public void TestTransformation_Columns_WE_CustomDate5()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomDate5 = DateTime.UtcNow);
		}

		public void TestTransformation_Columns_WE_CustomDecimal1()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomDecimal1 = 7.7m);
		}

		public void TestTransformation_Columns_WE_CustomDecimal2()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomDecimal2 = 7.7m);
		}

		public void TestTransformation_Columns_WE_CustomDecimal3()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomDecimal3 = 7.7m);
		}

		public void TestTransformation_Columns_WE_CustomDecimal4()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomDecimal4 = 7.7m);
		}

		public void TestTransformation_Columns_WE_CustomDecimal5()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomDecimal5 = 7.7m);
		}

		public void TestTransformation_Columns_WE_CustomFlag1()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomFlag1 = true);
		}

		public void TestTransformation_Columns_WE_CustomFlag2()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomFlag2 = true);
		}

		public void TestTransformation_Columns_WE_CustomFlag3()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomFlag3 = true);
		}

		public void TestTransformation_Columns_WE_CustomFlag4()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomFlag4 = true);
		}

		public void TestTransformation_Columns_WE_CustomFlag5()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomFlag5 = true);
		}

		public void TestTransformation_Columns_WE_CustomTextBlob1()
		{
			TestTransformation_ColumnsCore((docketLine) => docketLine.WE_CustomTextBlob1 = "test blob");
		}

		void TestTransformation_ColumnsCore(Action<WhsDocketLine> setColumn)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW", branchPK: branch.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var yesterday = DateTime.Today.AddDays(-1);
			var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);
			var docketLine = new WhsDocketLine(adjustment, product.PK, 10m, location.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_AdjustmentArrivalDate = yesterday,
				WE_SystemCreateTimeUtc = yesterday,
				WE_SystemLastEditTimeUtc = yesterday,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			};

			setColumn(docketLine);
			docketLine.AppendInsertAndReturnObject(sql);

			ExecuteQuery(sql);

			AssertEquals(0, Helper.GetStmDataRowCount(RegistryName));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals(1, Helper.GetStmDataRowCount(RegistryName));
			AssertEquals("Branch registry item", bool.TrueString, Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName, branch.PK)));
		}

		#endregion

		#region TestTransformation_Batching

		public void TestTransformation_Batching()
		{
			var sql = new StringBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs1 = new WhsWarehouse("WH1", "PRW", branchPK: branch1.PK).WithDockDoor(sql);
			var row1 = new WhsRow(whs1, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area1 = new WhsArea(whs1.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var branch2 = new GlbBranch("BR2").AppendInsertAndReturnObject(sql);
			var whs2 = new WhsWarehouse("WH2", "PRW", branchPK: branch2.PK).WithDockDoor(sql);
			var row2 = new WhsRow(whs2, "Row7") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area2 = new WhsArea(whs2.PK, "Area6").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var yesterday = DateTime.Today.AddDays(-1);
			var dockets = new WhsDocket[3005];
			var docketLines = new WhsDocketLine[dockets.Length];
			for (var i = 0; i < 3005; i++)
			{
				var whsPK = (i % 2 == 0) ? whs1.PK : whs2.PK;
				var locationPK = (i % 2 == 0) ? location1.PK : location2.PK;

				dockets[i] = new WhsDocket(client.PK, whsPK, "ADJ", "NEA", "ENT", $"A{i}");
				docketLines[i] = new WhsDocketLine(dockets[i], product.PK, 10m, locationPK)
				{
					WE_OriginalInventoryStatus = "AVL",
					WE_CurrentInventoryStatus = "AVL",
					WE_AdjustmentArrivalDate = yesterday,
					WE_SystemCreateTimeUtc = yesterday,
					WE_SystemLastEditTimeUtc = yesterday,
					WE_SystemCreateUser = "A",
					WE_SystemLastEditUser = "A",
					WE_CustomAttrib1 = "g",
				};
			}

			sql.AppendLine(dockets.GetBulkInsertStatement());
			sql.AppendLine(docketLines.GetBulkInsertStatement());

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertEquals(3005, WhsDocket.ShallowLoadFromDB(TestConnection).Length);
			AssertEquals(3005, WhsDocketLine.ShallowLoadFromDB(TestConnection, p => p.WE_CustomAttrib1 == "g").Length);
			AssertEquals(0, Helper.GetStmDataRowCount(RegistryName));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals(2, Helper.GetStmDataRowCount(RegistryName));
			AssertEquals("Branch1 registry item enabled", bool.TrueString, Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName, branch1.PK)));
			AssertEquals("Branch1 registry item enabled", bool.TrueString, Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName, branch2.PK)));
		}

		#endregion

		#region TestTransformation_CancellationTokenRespected

		public void TestTransformation_CancellationTokenRespected()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW", branchPK: branch.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var yesterday = DateTime.Today.AddDays(-1);
			var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);
			var docketLine = new WhsDocketLine(adjustment, product.PK, 10m, location.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_AdjustmentArrivalDate = yesterday,
				WE_SystemCreateTimeUtc = yesterday,
				WE_SystemLastEditTimeUtc = yesterday,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A",
				WE_CustomAttrib1 = "g",
			}.AppendInsertAndReturnObject(sql);

			ExecuteQuery(sql);

			AssertEquals(0, Helper.GetStmDataRowCount(RegistryName));

			var transform = GetNewTestTransformationInstance();
			AssertExceptionThrown<OperationCanceledException>(() => transform.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(canceled: true)));

			AssertEquals(1, Helper.GetStmDataRowCount(RegistryName));
			AssertEquals("Branch1 registry item enabled", bool.TrueString, Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName, branch.PK)));
		}

		#endregion

		#region Implementation

		void ExecuteQuery(StringBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}
		}

		#endregion
	}
}
