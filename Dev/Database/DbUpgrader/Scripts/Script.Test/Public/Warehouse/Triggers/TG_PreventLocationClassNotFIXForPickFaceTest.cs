using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventLocationClassNotFIXForPickFace))]
	class TG_PreventLocationClassNotFIXForPickFaceTest : DBCreateTriggerScriptTest
	{
		public void TestTG_PreventPickFaceUpdateToNotFIXLocation()
		{
			var testData = new PickFaceTestData();
			testData.SetupData(TestConnection);

			AssertNoExceptionThrown(
				() => WhsPickFace
				.UpdateWhere(testData.PickFace.PK)
				.Set(pf => pf.WF_WL, testData.LocationPF2.PK).Post(TestConnection));

			AssertExceptionThrown(typeof(SqlException), "A Pick Face must belong to a location that has a class of FIX",
				() => WhsPickFace
				.UpdateWhere(testData.PickFace.PK)
				.Set(pf => pf.WF_WL, testData.LocationTST.PK).Post(TestConnection), true);
		}

		public void TestTG_PreventPickFaceInsertToNotFIXLocation()
		{
			var testData = new PickFaceTestData();
			testData.SetupData(TestConnection);

			var sqlStringBuilder = new SqlQueryBuilder();
			var pickFace = new WhsPickFace(testData.Product.PK, testData.Client.PK, testData.LocationPF2.PK) { WF_ReplenishmentMultiple = 1, WF_ReplenishMinimum = 0, WF_ReplenishMaximum = 1 }.AppendInsertAndReturnObject(sqlStringBuilder);

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(sqlStringBuilder.ToStringWithNewLineBetweenAppends()));

			sqlStringBuilder = new SqlQueryBuilder();
			pickFace = new WhsPickFace(testData.Product.PK, testData.Client.PK, testData.LocationTST.PK) { WF_ReplenishMaximum = 1, WF_ReplenishmentMultiple = 1, WF_ReplenishMinimum = 0 }.AppendInsertAndReturnObject(sqlStringBuilder);

			AssertExceptionThrown(typeof(SqlException), "A Pick Face must belong to a location that has a class of FIX", () => TestConnection.ExecuteNonQuery(sqlStringBuilder.ToStringWithNewLineBetweenAppends()), true);
		}

		class PickFaceTestData
		{
			public void SetupData(DbConnection connection)
			{
				var sql = new SqlQueryBuilder();
				var locationTypeFIX = new WhsLocationType("PF1") { WLT_LocationClass = "FIX", WLT_MaximumNumberOfProducts = 1 }.AppendInsertAndReturnObject(sql);
				var locationTypeNOR = new WhsLocationType("TST") { WLT_LocationClass = "NOR" }.AppendInsertAndReturnObject(sql);

				Client = new OrgHeader("CLT001").AppendInsertAndReturnObject(sql);
				Product = new OrgSupplierPart("CLT001").AppendInsertAndReturnObject(sql);

				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(connection);
				var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row1 = new WhsRow(whs1, "B") { WR_Columns = 3 }.AppendInsertAndReturnObject(sql);
				LocationPF1 = new WhsLocation(row1.PK, area1.PK, area1.PK, locationTypeFIX.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
				LocationPF2 = new WhsLocation(row1.PK, area1.PK, area1.PK, locationTypeFIX.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
				LocationTST = new WhsLocation(row1.PK, area1.PK, area1.PK, locationTypeNOR.PK) { WL_Column = 3 }.AppendInsertAndReturnObject(sql);

				PickFace = new WhsPickFace(Product.PK, Client.PK, LocationPF1.PK).AppendInsertAndReturnObject(sql);
				connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			public OrgHeader Client;
			public OrgSupplierPart Product;
			public WhsLocation LocationPF1;
			public WhsLocation LocationPF2;
			public WhsLocation LocationTST;
			public WhsPickFace PickFace;
		}
	}
}
