using System;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(InsertStmALogFromDTUReturnHistory))]
	public class InsertStmALogFromDTUReturnHistoryTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(4, StmALog.CountInDB(TestConnection, t => t.SL_SE_NKEvent == "RET" && t.SL_Table == "WhsItemDispatchTransportationUnit"));

			AssertEquals($"StmALog For DTU1 has been populated.", 1, StmALog.CountInDB(TestConnection, t =>
				t.SL_SE_NKEvent == "RET" &&
				t.SL_Parent == dtuWithReturn.PK &&
				!t.SL_IsCancelled &&
				!t.SL_IsEstimate &&
				t.SL_GB_NKBranch == "BR1" &&
				t.SL_GE_NKDepartment == "DEP" &&
				t.SL_Table == "WhsItemDispatchTransportationUnit" &&
				t.SL_EventTime == new DateTime(2022, 3, 1) &&
				t.SL_PostedTimeUtc == new DateTime(2022, 2, 28, 16, 0, 0) &&
				t.SL_EventTimeUtc == new DateTime(2022, 2, 28, 16, 0, 0) &&
				t.SL_Reference == "|FAC=TW|EQN=DTU0001|RES=R1|FRN=DTU1|"));

			AssertEquals($"StmALog For DTU3 has been populated.", 1, StmALog.CountInDB(TestConnection, t =>
				t.SL_SE_NKEvent == "RET" &&
				t.SL_Parent == dtuWith2Returns.PK &&
				!t.SL_IsCancelled &&
				!t.SL_IsEstimate &&
				t.SL_GB_NKBranch == "BR1" &&
				t.SL_GE_NKDepartment == "DEP" &&
				t.SL_Table == "WhsItemDispatchTransportationUnit" &&
				t.SL_EventTime == new DateTime(2022, 5, 1) &&
				t.SL_PostedTimeUtc == new DateTime(2022, 4, 30, 16, 0, 0) &&
				t.SL_EventTimeUtc == new DateTime(2022, 4, 30, 16, 0, 0) &&
				t.SL_Reference == "|FAC=TW|EQN=DTU0003|RES=R3|FRN=DTU3|"));

			AssertEquals($"StmALog For DTU3 has been populated.", 1, StmALog.CountInDB(TestConnection, t =>
				t.SL_SE_NKEvent == "RET" &&
				t.SL_Parent == dtuWith2Returns.PK &&
				!t.SL_IsCancelled &&
				!t.SL_IsEstimate &&
				t.SL_GB_NKBranch == "BR1" &&
				t.SL_GE_NKDepartment == "DEP" &&
				t.SL_Table == "WhsItemDispatchTransportationUnit" &&
				t.SL_EventTime == new DateTime(2022, 6, 1) &&
				t.SL_PostedTimeUtc == new DateTime(2022, 5, 31, 16, 0, 0) &&
				t.SL_EventTimeUtc == new DateTime(2022, 5, 31, 16, 0, 0) &&
				t.SL_Reference == "|FAC=TW|EQN=DTU0003|RES=R4|FRN=DTU3|"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new InsertStmALogFromDTUReturnHistory();

		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();

			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "CNNJG" }.InsertAndReturnObject(TestConnection);
			var warehouse = new WhsWarehouseOld_V01("WH1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(warehouse, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(warehouse.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);
			var department = new GlbDepartment("DEP").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("TS", "TestStaff") { GS_GE_HomeDepartment = department.PK }.AppendInsertAndReturnObject(sql);
			dtuWithReturn = new WhsItemDispatchTransportationUnit(warehouse, "DTU1")
			{
				WDH_VehicleReference = "DTU0001",
				WDH_GateInTime = new DateTimeOffset(2022, 2, 7, 10, 0, 0, TimeSpan.FromMinutes(480)),
				WDH_LoadCompleteTime = new DateTimeOffset(2022, 2, 7, 11, 0, 0, TimeSpan.FromMinutes(480)),
				WDH_GateOutTime = new DateTimeOffset(2022, 2, 7, 12, 0, 0, TimeSpan.FromMinutes(480))
			}.AppendInsertAndReturnObject(sql);

			var dtuWithReturnAndExistingLog = new WhsItemDispatchTransportationUnit(warehouse, "DTU2")
			{
				WDH_VehicleReference = "DTU0002",
				WDH_GateInTime = new DateTimeOffset(2022, 2, 7, 10, 0, 0, TimeSpan.FromMinutes(480)),
				WDH_LoadCompleteTime = new DateTimeOffset(2022, 2, 7, 11, 0, 0, TimeSpan.FromMinutes(480)),
				WDH_GateOutTime = new DateTimeOffset(2022, 2, 7, 12, 0, 0, TimeSpan.FromMinutes(480))
			}.AppendInsertAndReturnObject(sql);

			dtuWith2Returns = new WhsItemDispatchTransportationUnit(warehouse, "DTU3")
			{
				WDH_VehicleReference = "DTU0003",
				WDH_GateInTime = new DateTimeOffset(2022, 2, 7, 10, 0, 0, TimeSpan.FromMinutes(480)),
				WDH_LoadCompleteTime = new DateTimeOffset(2022, 2, 7, 11, 0, 0, TimeSpan.FromMinutes(480)),
				WDH_GateOutTime = new DateTimeOffset(2022, 2, 7, 12, 0, 0, TimeSpan.FromMinutes(480))
			}.AppendInsertAndReturnObject(sql);

			var dtuWithoutReturn = new WhsItemDispatchTransportationUnit(warehouse, "DTU4")
			{
				WDH_VehicleReference = "DTU0004",
				WDH_GateInTime = new DateTimeOffset(2022, 2, 7, 10, 0, 0, TimeSpan.FromMinutes(480)),
				WDH_LoadCompleteTime = new DateTimeOffset(2022, 2, 7, 11, 0, 0, TimeSpan.FromMinutes(480)),
				WDH_GateOutTime = new DateTimeOffset(2022, 2, 7, 12, 0, 0, TimeSpan.FromMinutes(480))
			}.AppendInsertAndReturnObject(sql);

			var dtuReturn1 = new WhsItemDTUReturnDetail(dtuWithReturn, location)
			{
				WDR_Reason = "R1",
				WDR_ReturnTime = new DateTimeOffset(2022, 3, 1, 0, 0, 0, TimeSpan.FromMinutes(480)),
				WDR_SystemCreateUser = staff.GS_Code,
			}.AppendInsertAndReturnObject(sql);

			var dtuReturn2 = new WhsItemDTUReturnDetail(dtuWithReturnAndExistingLog, location)
			{
				WDR_Reason = "R2",
				WDR_ReturnTime = new DateTimeOffset(2022, 4, 1, 0, 0, 0, TimeSpan.FromMinutes(480)),
				WDR_SystemCreateUser = staff.GS_Code,
			}.AppendInsertAndReturnObject(sql);

			var dtuReturn3_1 = new WhsItemDTUReturnDetail(dtuWith2Returns, location)
			{
				WDR_Reason = "R3",
				WDR_ReturnTime = new DateTimeOffset(2022, 5, 1, 0, 0, 0, TimeSpan.FromMinutes(480)),
				WDR_SystemCreateUser = staff.GS_Code,
			}.AppendInsertAndReturnObject(sql);

			var dtuReturn3_2 = new WhsItemDTUReturnDetail(dtuWith2Returns, location)
			{
				WDR_Reason = "R4",
				WDR_ReturnTime = new DateTimeOffset(2022, 6, 1, 0, 0, 0, TimeSpan.FromMinutes(480)),
				WDR_SystemCreateUser = staff.GS_Code,
			}.AppendInsertAndReturnObject(sql);

			var existingLogForDTU2 = new StmALog(dtuWithReturnAndExistingLog.PK, "WhsItemDispatchTransportationUnit", "RET")
			{
				SL_Reference = "|FAC=TW|EQN=DTU0002|RES=R2|FRN=DTU2|",
				SL_EventTime = new DateTime(2022, 3, 1),
				SL_GS_NKUser = "TS",
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override string ReasonNotToBeMapped => "The transformation shouldn't run right now as it relies upon other features that will be implemented later";

		WhsItemDispatchTransportationUnit dtuWithReturn;
		WhsItemDispatchTransportationUnit dtuWith2Returns;
	}
}
