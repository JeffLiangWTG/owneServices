using System;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsLoad_CannotChangeTransportationUnitIfStarted))]
	class TG_WhsLoad_CannotChangeTransportationUnitIfStartedTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsLoad_CannotChangeTransportationUnitIfStartedTest : TransactionedTestCase
	{
		public void TestTrigger_CannotChangeTransportationUnitIfStarted_TransportationUnitFK()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var areaDDL = new WhsArea(whs.PK, "DDLArea").AppendInsertAndReturnObject(sql);
			var rowDDL = new WhsRow(whs, "DDL").AppendInsertAndReturnObject(sql);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL").First();
			var dockDoorLocation = new WhsLocation(rowDDL.PK, areaDDL.PK, areaDDL.PK, ddlLocationType.PK).AppendInsertAndReturnObject(sql);

			var transportCompany = new OrgHeader("TRANSPORT").AppendInsertAndReturnObject(sql);
			var equipment1 = new RefEquipment("ABC", "ABC")
			{
				RQ_CubicCapacity = 1m,
				RQ_CubicUnit = "M3",
				RQ_WeightCapacity = 1m,
				RQ_WeightUnit = "KG"
			}.AppendInsertAndReturnObject(sql);

			var equipment2 = new RefEquipment("DEF", "DEF")
			{
				RQ_CubicCapacity = 1m,
				RQ_CubicUnit = "M3",
				RQ_WeightCapacity = 1m,
				RQ_WeightUnit = "KG"
			}.AppendInsertAndReturnObject(sql);

			var load = new WhsLoad("WL01", "STD", transportCompany.PK, dockDoorLocation.PK)
			{
				WLO_StartTime = DateTimeOffset.Now,
				WLO_TransportationUnitNumber = "ABC",
				WLO_RQ_TransportationUnit = equipment1.PK
			}.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(
				typeof(SqlException),
				@"Attempted to change the Transportation Unit after the Load is started.
The transaction ended in the trigger. The batch has been aborted.",
				() => WhsLoad.UpdateWhere(load.PK).Set(l => l.WLO_RQ_TransportationUnit, equipment2.PK).Post(TestConnection));
		}

		public void TestTrigger_CannotChangeTransportationUnitIfStarted_TransportationUnitNumber()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var areaDDL = new WhsArea(whs.PK, "DDLArea").AppendInsertAndReturnObject(sql);
			var rowDDL = new WhsRow(whs, "DDL").AppendInsertAndReturnObject(sql);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL").First();
			var dockDoorLocation = new WhsLocation(rowDDL.PK, areaDDL.PK, areaDDL.PK, ddlLocationType.PK).AppendInsertAndReturnObject(sql);

			var transportCompany = new OrgHeader("TRANSPORT").AppendInsertAndReturnObject(sql);
			var load = new WhsLoad("WL01", "STD", transportCompany.PK, dockDoorLocation.PK)
			{
				WLO_StartTime = DateTimeOffset.Now,
				WLO_TransportationUnitNumber = "ABC"
			}.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(
				typeof(SqlException),
				@"Attempted to change the Transportation Unit after the Load is started.
The transaction ended in the trigger. The batch has been aborted.",
				() => WhsLoad.UpdateWhere(load.PK).Set(l => l.WLO_TransportationUnitNumber, "DEF").Post(TestConnection));
		}
	}
}
