using System;
using System.Linq.Expressions;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsLocation_EnsurePickingAndPutawayArea))]
	class TG_WhsLocation_EnsurePickingAndPutawayAreaTest : DBCreateTriggerScriptTest
	{
	}

	class TG_WhsLocation_EnsurePickingAndPutawayAreaNonTransactionTest : TestCase
	{
		#region TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea

		[UseSnapshotProtection]
		public void TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var pickingAreaPK = new WhsArea(warehouse.PK, "PIC")
				{
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaPK = new WhsArea(warehouse.PK, "PUT")
				{
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var bothAreaPK = new WhsArea(warehouse.PK, "BTH")
				{
					WA_IsPutawayArea = true,
					WA_IsPickingArea = true
				}.AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);
				var locationPK = new WhsLocation(row.PK, pickingAreaPK, putawayAreaPK).AppendInsertAndReturnObject(sql).PK;

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update_Success("Updating putaway area to a both putaway and picking Area must succeed.", mainConnection, locationPK,  (l) => l.WL_WA_PutawayArea, bothAreaPK);
				AssertTrigger_Update_Fails("Updating putaway area to a picking area must fail.", IncorrectPickingPutawayAreaError, locationPK,  (l) => l.WL_WA_PutawayArea, pickingAreaPK);

				AssertTrigger_Update_Fails("Updating picking area to a putaway area must fail.", IncorrectPickingPutawayAreaError, locationPK,  (l) => l.WL_WA_PickingArea, putawayAreaPK);
				AssertTrigger_Update_Success("Updating picking area to a both putaway and picking Area must succeed.", mainConnection, locationPK,  (l) => l.WL_WA_PickingArea, bothAreaPK);
			}
		}

		#endregion

		#region TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_FREToFRE

		[UseSnapshotProtection]
		public void TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_FREToFRE()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);

				// Non-IPR Areas
				var pickingAreaFREPK = new WhsArea(warehouse.PK, "PIC_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaFREPK = new WhsArea(warehouse.PK, "PUT_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var bothAreaFREPK = new WhsArea(warehouse.PK, "BTH_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = true
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);
				var locationFREPK = new WhsLocation(row.PK, pickingAreaFREPK, putawayAreaFREPK).AppendInsertAndReturnObject(sql).PK;

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update_Success(
					"Updating putaway area FRE to a both putaway and picking FRE Area must succeed.",
					mainConnection,
					locationFREPK,
					 (l) => l.WL_WA_PutawayArea,
					bothAreaFREPK);
				AssertTrigger_Update_Success(
					"Updating picking area FRE to a both putaway and picking FRE Area must succeed.",
					mainConnection,
					locationFREPK,
					 (l) => l.WL_WA_PickingArea,
					bothAreaFREPK);
			}
		}

		#endregion

		#region TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_FREToEXE

		[UseSnapshotProtection]
		public void TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_FREToEXE()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);

				// Non-IPR Areas
				var pickingAreaFREPK = new WhsArea(warehouse.PK, "PIC_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaFREPK = new WhsArea(warehouse.PK, "PUT_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				var pickingAreaEXEPK = new WhsArea(warehouse.PK, "PIC_EXE")
				{
					WA_AreaType = "EXE",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaEXEPK = new WhsArea(warehouse.PK, "PUT_EXE")
				{
					WA_AreaType = "EXE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);
				var locationFREPK = new WhsLocation(row.PK, pickingAreaFREPK, putawayAreaFREPK).AppendInsertAndReturnObject(sql).PK;
				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update_Success(
					"Updating putaway area FRE to a putaway EXE Area must succeed.",
					mainConnection,
					locationFREPK,
					 (l) => l.WL_WA_PutawayArea,
					putawayAreaEXEPK);
				AssertTrigger_Update_Success(
					"Updating picking area FRE to a picking EXE Area must succeed.",
					mainConnection,
					locationFREPK,
					 (l) => l.WL_WA_PickingArea,
					pickingAreaEXEPK);
			}
		}

		#endregion

		#region TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_IPRToIPR

		[UseSnapshotProtection]
		public void TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_IPRToIPR()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);

				var pickingAreaIPRPK = new WhsArea(warehouse.PK, "PIC_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaIPRPK = new WhsArea(warehouse.PK, "PUT_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var bothAreaIPRPK = new WhsArea(warehouse.PK, "BTH_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = true
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);
				var locationIPRPK = new WhsLocation(row.PK, pickingAreaIPRPK, putawayAreaIPRPK).AppendInsertAndReturnObject(sql).PK;

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update_Success(
					"Updating putaway area IPR to a both putaway and picking IPR Area must succeed.",
					mainConnection,
					locationIPRPK,
					 (l) => l.WL_WA_PutawayArea,
					bothAreaIPRPK);
				AssertTrigger_Update_Success(
					"Updating picking area IPR to a both putaway and picking IPR Area must succeed.",
					mainConnection,
					locationIPRPK,
					 (l) => l.WL_WA_PickingArea,
					bothAreaIPRPK);
			}
		}

		#endregion

		#region TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_FREToIPR_UpdateOnlySingleArea

		[UseSnapshotProtection]
		public void TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_FREToIPR_UpdateOnlySingleArea()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);

				// Non-IPR Areas
				var pickingAreaFREPK = new WhsArea(warehouse.PK, "PIC_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaFREPK = new WhsArea(warehouse.PK, "PUT_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				// IPR Areas
				var pickingAreaIPRPK = new WhsArea(warehouse.PK, "PIC_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaIPRPK = new WhsArea(warehouse.PK, "PUT_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);
				var locationFREPK = new WhsLocation(row.PK, pickingAreaFREPK, putawayAreaFREPK).AppendInsertAndReturnObject(sql).PK;

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update_Fails(
					"Updating putaway FRE area to a putaway IPR area must fail.",
					PickingAndPutawayAreaBothNotIPRError,
					locationFREPK,
					 (l) => l.WL_WA_PutawayArea,
					putawayAreaIPRPK);
				AssertTrigger_Update_Fails(
					"Updating picking FRE area to a picking IPR area must fail.",
					PickingAndPutawayAreaBothNotIPRError,
					locationFREPK,
					 (l) => l.WL_WA_PickingArea,
					pickingAreaIPRPK);
			}
		}

		#endregion

		#region TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_IPRToFRE_UpdateOnlySingleArea

		[UseSnapshotProtection]
		public void TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_IPRToFRE_UpdateOnlySingleArea()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);

				// Non-IPR Areas
				var pickingAreaFREPK = new WhsArea(warehouse.PK, "PIC_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaFREPK = new WhsArea(warehouse.PK, "PUT_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				// IPR Areas
				var pickingAreaIPRPK = new WhsArea(warehouse.PK, "PIC_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaIPRPK = new WhsArea(warehouse.PK, "PUT_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);
				var locationIPRPK = new WhsLocation(row.PK, pickingAreaIPRPK, putawayAreaIPRPK).AppendInsertAndReturnObject(sql).PK;

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update_Fails(
					"Updating putaway IPR area to a putaway FRE area must fail.",
					PickingAndPutawayAreaBothNotIPRError,
					locationIPRPK,
					 (l) => l.WL_WA_PutawayArea,
					putawayAreaFREPK);
				AssertTrigger_Update_Fails(
					"Updating picking IPR area to a picking FRE area must fail.",
					PickingAndPutawayAreaBothNotIPRError,
					locationIPRPK,
					 (l) => l.WL_WA_PickingArea,
					pickingAreaFREPK);
			}
		}

		#endregion

		#region TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_FREToIPR_UpdateBothAreas

		[UseSnapshotProtection]
		public void TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_FREToIPR_UpdateBothAreas()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);

				// FRE Areas
				var pickingAreaFREPK = new WhsArea(warehouse.PK, "PIC_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaFREPK = new WhsArea(warehouse.PK, "PUT_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				// IPR Areas
				var pickingAreaIPRPK = new WhsArea(warehouse.PK, "PIC_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaIPRPK = new WhsArea(warehouse.PK, "PUT_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);
				var locationFREPK = new WhsLocation(row.PK, pickingAreaFREPK, putawayAreaFREPK).AppendInsertAndReturnObject(sql).PK;

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertNoExceptionThrown(
					"Updating putaway both FRE areas on location to IPR areas should pass.",
					() => mainConnection.ExecuteNonQuery(
						WhsLocation.UpdateWhere(locationFREPK)
						.Set(l => l.WL_WA_PickingArea, pickingAreaIPRPK)
						.Set(l => l.WL_WA_PutawayArea, putawayAreaIPRPK)
						.AsSQL()));
			}
		}

		#endregion

		#region TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_IPRToFRE_UpdateBothAreas

		[UseSnapshotProtection]
		public void TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_IPRToFRE_UpdateBothAreas()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);

				// FRE Areas
				var pickingAreaFREPK = new WhsArea(warehouse.PK, "PIC_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaFREPK = new WhsArea(warehouse.PK, "PUT_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				// IPR Areas
				var pickingAreaIPRPK = new WhsArea(warehouse.PK, "PIC_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaIPRPK = new WhsArea(warehouse.PK, "PUT_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);
				var locationIPRPK = new WhsLocation(row.PK, pickingAreaIPRPK, putawayAreaIPRPK).AppendInsertAndReturnObject(sql).PK;

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertNoExceptionThrown(
					"Updating putaway both IPR areas on location to FRE areas should pass.",
					() => mainConnection.ExecuteNonQuery(
						WhsLocation.UpdateWhere(locationIPRPK)
						.Set(l => l.WL_WA_PickingArea, pickingAreaFREPK)
						.Set(l => l.WL_WA_PutawayArea, putawayAreaFREPK)
						.AsSQL()));
			}
		}

		#endregion

		#region TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_UpdateBothAreasToDifferent

		[UseSnapshotProtection]
		public void TestWhsLocation_Update_WL_WA_PutawayAreaAndWL_WA_PickingArea_UpdateBothAreasToDifferent()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);

				// Non-IPR Areas
				var putawayAreaFREPK = new WhsArea(warehouse.PK, "PUT_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				// IPR Areas
				var pickingAreaIPRPK1 = new WhsArea(warehouse.PK, "PIC_IPR1")
				{
					WA_AreaType = "IPR",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var pickingAreaIPRPK2 = new WhsArea(warehouse.PK, "PIC_IPR2")
				{
					WA_AreaType = "IPR",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaIPRPK = new WhsArea(warehouse.PK, "PUT_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);
				var locationIPRPK = new WhsLocation(row.PK, pickingAreaIPRPK1, putawayAreaIPRPK).AppendInsertAndReturnObject(sql).PK;

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertExceptionThrown(
					"Updating putaway both IPR areas on location to EXE + FRE areas should fail.",
					typeof(SqlException),
					PickingAndPutawayAreaBothNotIPRError,
					() => mainConnection.ExecuteNonQuery(
						WhsLocation.UpdateWhere(locationIPRPK)
						.Set(l => l.WL_WA_PickingArea, pickingAreaIPRPK2)
						.Set(l => l.WL_WA_PutawayArea, putawayAreaFREPK)
						.AsSQL()),
					assertStartsWith: true);
			}
		}

		#endregion

		#region TestWhsLocation_Insert_WL_WA_PutawayAreaAndWL_WA_PickingArea

		[UseSnapshotProtection]
		public void TestWhsLocation_Insert_WL_WA_PutawayAreaAndWL_WA_PickingArea_DifferentAreaTypes()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);

				// Non-IPR Areas
				var pickingAreaFREPK = new WhsArea(warehouse.PK, "PIC_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaFREPK = new WhsArea(warehouse.PK, "PUT_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				var pickingAreaEXEPK = new WhsArea(warehouse.PK, "PIC_EXE")
				{
					WA_AreaType = "EXE",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaEXEPK = new WhsArea(warehouse.PK, "PUT_EXE")
				{
					WA_AreaType = "EXE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				// IPR Areas
				var pickingAreaIPRPK = new WhsArea(warehouse.PK, "PIC_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaIPRPK = new WhsArea(warehouse.PK, "PUT_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Insert_Fails(
					"Insert must fail since picking area is a FRE type and putaway area is a IPR type.",
					PickingAndPutawayAreaBothNotIPRError,
					row.PK,
					pickingAreaFREPK,
					putawayAreaIPRPK);

				AssertTrigger_Insert_Fails(
					"Insert must fail since picking area is a IPR type and putaway area is a FRE type.",
					PickingAndPutawayAreaBothNotIPRError,
					row.PK,
					pickingAreaIPRPK,
					putawayAreaFREPK);

				AssertTrigger_Insert_Fails(
					"Insert must fail since picking area is a EXE type and putaway area is a IPR type.",
					PickingAndPutawayAreaBothNotIPRError,
					row.PK,
					pickingAreaEXEPK,
					putawayAreaIPRPK);

				AssertTrigger_Insert_Fails(
					"Insert must fail since picking area is a IPR type and putaway area is a EXE type.",
					PickingAndPutawayAreaBothNotIPRError,
					row.PK,
					pickingAreaIPRPK,
					putawayAreaEXEPK);
			}
		}

		#endregion

		#region TestWhsLocation_Insert_WL_WA_PutawayAreaAndWL_WA_PickingArea_SimilarAreaTypes_IPR

		[UseSnapshotProtection]
		public void TestWhsLocation_Insert_WL_WA_PutawayAreaAndWL_WA_PickingArea_SimilarAreaTypes_IPR()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);

				var pickingAreaIPRPK = new WhsArea(warehouse.PK, "PIC_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaIPRPK = new WhsArea(warehouse.PK, "PUT_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var bothAreaIPRPK = new WhsArea(warehouse.PK, "BTH_IPR")
				{
					WA_AreaType = "IPR",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = true
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Insert_Success(
					"Insert must succeed since Picking and putaway areas are IPR Type.",
					mainConnection,
					row.PK,
					1,
					pickingAreaIPRPK,
					putawayAreaIPRPK);

				AssertTrigger_Insert_Success(
					"Insert must succeed since Picking and putaway areas are IPR Type.",
					mainConnection,
					row.PK,
					2,
					pickingAreaIPRPK,
					bothAreaIPRPK);

				AssertTrigger_Insert_Success(
					"Insert must succeed since Picking and putaway areas are IPR Type.",
					mainConnection,
					row.PK,
					3,
					bothAreaIPRPK,
					bothAreaIPRPK);
			}
		}

		#endregion

		#region TestWhsLocation_Insert_WL_WA_PutawayAreaAndWL_WA_PickingArea_SimilarAreaTypes_NonIPR

		[UseSnapshotProtection]
		public void TestWhsLocation_Insert_WL_WA_PutawayAreaAndWL_WA_PickingArea_SimilarAreaTypes_NonIPR()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);

				var pickingAreaFREPK = new WhsArea(warehouse.PK, "PIC_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaFREPK = new WhsArea(warehouse.PK, "PUT_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var bothAreaFREPK = new WhsArea(warehouse.PK, "BTH_FRE")
				{
					WA_AreaType = "FRE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = true
				}.AppendInsertAndReturnObject(sql).PK;

				var pickingAreaEXEPK = new WhsArea(warehouse.PK, "PIC_EXE")
				{
					WA_AreaType = "EXE",
					WA_IsPickingArea = true,
					WA_IsPutawayArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaEXEPK = new WhsArea(warehouse.PK, "PUT_EXE")
				{
					WA_AreaType = "EXE",
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Insert_Success(
					"Insert must succeed since Picking and putaway areas are FRE type.",
					mainConnection,
					row.PK,
					1,
					bothAreaFREPK,
					putawayAreaFREPK);

				AssertTrigger_Insert_Success(
					"Insert must succeed since Picking are is EXE type and putaway area is FRE type.",
					mainConnection,
					row.PK,
					2,
					pickingAreaEXEPK,
					putawayAreaFREPK);

				AssertTrigger_Insert_Success(
					"Insert must succeed since Picking are is FRE type and putaway area is EXE type.",
					mainConnection,
					row.PK,
					3,
					pickingAreaFREPK,
					putawayAreaEXEPK);
			}
		}

		#endregion

		#region TestWhsLocation_Insert_WL_WA_PutawayAreaAndWL_WA_PickingArea_IPR

		[UseSnapshotProtection]
		public void TestWhsLocation_Insert_WL_WA_PutawayAreaAndWL_WA_PickingArea_IPR()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var warehouse = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var pickingAreaPK = new WhsArea(warehouse.PK, "PIC")
				{
					WA_IsPutawayArea = false,
					WA_IsPickingArea = true
				}.AppendInsertAndReturnObject(sql).PK;
				var putawayAreaPK = new WhsArea(warehouse.PK, "PUT")
				{
					WA_IsPutawayArea = true,
					WA_IsPickingArea = false
				}.AppendInsertAndReturnObject(sql).PK;
				var bothAreaPK = new WhsArea(warehouse.PK, "BTH")
				{
					WA_IsPickingArea = true,
					WA_IsPutawayArea = true
				}.AppendInsertAndReturnObject(sql).PK;

				var row = new WhsRow(warehouse, "A").AppendInsertAndReturnObject(sql);

				mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Insert_Fails("Insert must fail since neither putaway nor picking areas are set correctly.", IncorrectPickingPutawayAreaError, row.PK, putawayAreaPK, pickingAreaPK);
				AssertTrigger_Insert_Fails("Insert must fail since WL_WA_PickingArea is set to putaway Area.", IncorrectPickingPutawayAreaError, row.PK, putawayAreaPK, bothAreaPK);
				AssertTrigger_Insert_Fails("Insert must fail since WL_WA_PutawayArea is set to picking Area.", IncorrectPickingPutawayAreaError, row.PK, bothAreaPK, pickingAreaPK);
				AssertTrigger_Insert_Success("Insert must succeed since Picking and putaway areas are set correctly.", mainConnection, row.PK, 1, bothAreaPK, putawayAreaPK);
				AssertTrigger_Insert_Success("Insert must succeed since Picking and putaway areas are set correctly.", mainConnection, row.PK, 2, pickingAreaPK, putawayAreaPK);
				AssertTrigger_Insert_Success("Insert must succeed since Picking and putaway areas are set correctly.", mainConnection, row.PK, 3, pickingAreaPK, bothAreaPK);
				AssertTrigger_Insert_Success("Insert must succeed since Picking and putaway areas are set correctly.", mainConnection, row.PK, 4, bothAreaPK, bothAreaPK);
			}
		}

		#endregion

		#region Implementation

		void AssertTrigger_Insert_Success(string errorDescription, DbConnection connection, Guid rowPK, short column, Guid pickingAreaPK, Guid putawayAreaPK)
		{
			var sql = new SqlQueryBuilder();
			new WhsLocation(rowPK, pickingAreaPK, putawayAreaPK) { WL_Column = column }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(errorDescription, () => connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
		}

		void AssertTrigger_Insert_Fails(string errorDescription, string expectedErrorMessage, Guid rowPK, Guid pickingAreaPK, Guid putawayAreaPK)
		{
			var sql = new SqlQueryBuilder();
			new WhsLocation(rowPK, pickingAreaPK, putawayAreaPK).AppendInsertAndReturnObject(sql);

			AssertTriggerFails(errorDescription, expectedErrorMessage, sql.ToStringWithNewLineBetweenAppends());
		}

		void AssertTrigger_Update_Success<T>(string errorDescription, DbConnection connection, Guid locationPK, Expression<Func<WhsLocation, T>> property, T value)
		{
			AssertNoExceptionThrown(errorDescription,
				() => WhsLocation
				.UpdateWhere(locationPK)
				.Set(property, value).Post(connection));
		}

		void AssertTrigger_Update_Fails<T>(string errorDescription, string expectedErrorMessage, Guid locationPK, Expression<Func<WhsLocation, T>> property, T value)
		{
			var sql = WhsLocation
				.UpdateWhere(locationPK)
				.Set(property, value).AsSQL();

			AssertTriggerFails(errorDescription, expectedErrorMessage, sql);
		}

		static void AssertTriggerFails(string errorDescription, string expectedErrorMessage, string sql)
		{
			using (Db.DisposableActionForDbConnection())
			using (var newConnection = Db.NewExtraConnectionToMainDb())
			{
				AssertExceptionThrown(
					errorDescription,
					typeof(SqlException),
					expectedErrorMessage,
					() => newConnection.ExecuteNonQuery(sql),
					assertStartsWith: true);
			}
		}

		const string IncorrectPickingPutawayAreaError = "Attempt to update or insert picking / putaway area of a location to non-picking / non-putaway Area.";
		const string PickingAndPutawayAreaBothNotIPRError = "Picking area and putaway area of a location must both be IPR type or both not IPR type.";

		#endregion
	}
}

