using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.ContainerYard;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.ContainerYard
{
	[TestedType(typeof(TG_JobDocAddress_ValidateYardUnitStateOverlap))]
	class TG_JobDocAddress_ValidateYardUnitStateOverlapTest : DBCreateTriggerScriptTest
	{
		#region TestTrigger_Insert

		void TestPreventInsertIfDuplicate_SameClientAndPeriod(DateTime fromDate, DateTime toDate, string message)
		{
			SetupData();
			var sql = new StringBuilder();

			new JobDocAddress(receiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var insertYardUnitStateSQL = new StringBuilder();
			new JobDocAddress(receiveAdvice1.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(insertYardUnitStateSQL);

			AssertExceptionThrown(
				message,
				typeof(SqlException),
	$"Same unit number already exists in Pre-Arrival Instruction ID: {receiveAdvice.YRA_JobNumber}, Client: {clientCode} - {clientFullName}, From date: {receiveAdvice.YRA_FromDate.ToString("dd/MM/yyyy")}, To date: {receiveAdvice.YRA_ToDate.ToString("dd/MM/yyyy")}",
				() => TestConnection.ExecuteNonQuery(insertYardUnitStateSQL.ToString()),
				true
			);
		}

		public void TestPreventInsertIfDuplicate_SameClientHasTimeIntersection1()
		{
			TestPreventInsertIfDuplicate_SameClientAndPeriod(fromDate, toDate, "Should prevent Insert if same client and time range intersection");
		}

		public void TestPreventInsertIfDuplicate_SameClientHasTimeIntersection2()
		{
			TestPreventInsertIfDuplicate_SameClientAndPeriod(fromDate, fromDate, "Should prevent Insert if time range intersects only on the first day");
		}

		public void TestPreventInsertIfDuplicate_SameClientHasTimeIntersection3()
		{
			TestPreventInsertIfDuplicate_SameClientAndPeriod(toDate, toDate, "Should prevent Insert if time range intersects only on the last day");
		}

		public void TestPreventInsertIfDuplicate_SameReceiveInstruction()
		{
			SetupData();
			var sql = new StringBuilder();

			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var insertYardUnitStateSQL = new StringBuilder();
			new JobDocAddress(receiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(insertYardUnitStateSQL);

			AssertExceptionThrown(
				"Should prevent Insert if same Receive Instruction",
				typeof(SqlException),
				$"Same unit number already exists in Pre-Arrival Instruction ID: {receiveAdvice.YRA_JobNumber}, Client: {clientCode} - {clientFullName}, From date: {receiveAdvice.YRA_FromDate.ToString("dd/MM/yyyy")}, To date: {receiveAdvice.YRA_ToDate.ToString("dd/MM/yyyy")}",
				() => TestConnection.ExecuteNonQuery(insertYardUnitStateSQL.ToString()),
				true
			);
		}

		public void TestAllowInsertIfDuplicate_DifferentClient()
		{
			SetupData();
			var sql = new StringBuilder();
			new JobDocAddress(receiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			var orgHeaderPK1 = Guid.NewGuid();
			var orgAddressPK1 = Guid.NewGuid();
			var insertSql = $@"
				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
				VALUES ('{orgHeaderPK1}', 'clientCode', 'clientFullName', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Code, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
				VALUES ('{orgAddressPK1}', '{orgHeaderPK1}', 'AAA', '2 CHOME', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			sql.AppendLine(insertSql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var insertYardUnitStateSQL = new StringBuilder();
			var jobDocAddress1 = new JobDocAddress(receiveAdvice1.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK1 }.AppendInsertAndReturnObject(insertYardUnitStateSQL);

			AssertNoExceptionThrown(
				"Should allow Insert if client is different",
				() => TestConnection.ExecuteNonQuery(insertYardUnitStateSQL.ToString())
			);
		}

		void TestAllowInsertIfDuplicate_SameClientDifferentPeriod(DateTime fromDate, DateTime toDate, string message)
		{
			SetupData();
			var sql = new StringBuilder();

			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var insertYardUnitStateSQL = new StringBuilder();
			new JobDocAddress(receiveAdvice1.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(insertYardUnitStateSQL);

			AssertNoExceptionThrown(
				message,
				() => TestConnection.ExecuteNonQuery(insertYardUnitStateSQL.ToString())
			);
		}

		public void TestAllowInsertIfDuplicate_SameClientDifferentPeriodLater()
		{
			TestAllowInsertIfDuplicate_SameClientDifferentPeriod(new DateTime(2025, 4, 26), new DateTime(2025, 4, 28), "Should allow Insert if same client and time period is later");
		}

		public void TestAllowInsertIfDuplicate_SameClientDifferentPeriodEarlier()
		{
			TestAllowInsertIfDuplicate_SameClientDifferentPeriod(new DateTime(2025, 4, 1), new DateTime(2025, 4, 2), "Should allow Insert if same client and time period is earlier");
		}

		public void TestAllowInsertIfDuplicate_UnitHasDifferentType()
		{
			SetupData();
			var sql = new StringBuilder();

			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem(type: "GEN").AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			new JobDocAddress(receiveAdvice1.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var insertYardUnitStateSQL = new StringBuilder();
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(insertYardUnitStateSQL);

			AssertNoExceptionThrown(
				"Should allow Insert if unit has a different type",
				() => TestConnection.ExecuteNonQuery(insertYardUnitStateSQL.ToString())
			);
		}

		public void TestPreventInsertIfDuplicate_UnitNotGatedOut()
		{
			SetupData();
			var sql = new StringBuilder();
			new JobDocAddress(receiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);

			var dispatchTransportationUnit = new CYDTransportationUnit(warehouse, "TPU123").AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine, YUS_YTU_DispatchTransportationUnit = dispatchTransportationUnit }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var insertYardUnitStateSQL = new StringBuilder();
			var jobDocAddress1 = new JobDocAddress(receiveAdvice1.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(insertYardUnitStateSQL);

			AssertExceptionThrown(
				"Should prevent Insert if unit not gated out",
				typeof(SqlException),
				$"Same unit number already exists in Pre-Arrival Instruction ID: {receiveAdvice.YRA_JobNumber}, Client: {clientCode} - {clientFullName}, From date: {receiveAdvice.YRA_FromDate.ToString("dd/MM/yyyy")}, To date: {receiveAdvice.YRA_ToDate.ToString("dd/MM/yyyy")}",
				() => TestConnection.ExecuteNonQuery(insertYardUnitStateSQL.ToString()),
				true
			);
		}

		public void TestAllowInsertIfDuplicate_UnitGatedOut()
		{
			SetupData();

			var sql = new StringBuilder();

			var warehouseLocations = warehouse.CreateLocations(sql, 1);
			var dispatchTransportationUnit = new CYDTransportationUnit(warehouse, "TPU123") { YTU_WL_WaitingBayLocation = warehouseLocations[0], YTU_GateInTime = new DateTime(2025, 4, 19), YTU_GateOutTime = new DateTime(2025, 4, 20) }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine, YUS_YTU_DispatchTransportationUnit = dispatchTransportationUnit }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var insertYardUnitStateSQL = new StringBuilder();
			new JobDocAddress(receiveAdvice1.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(insertYardUnitStateSQL);

			AssertNoExceptionThrown(
				"Should allow Insert if unit gated out",
				() => TestConnection.ExecuteNonQuery(insertYardUnitStateSQL.ToString())
			);
		}

		#endregion

		#region TestTrigger_Update

		public void TestPreventUpdateIfDuplicate_SameClientAndPeriod(DateTime fromDate, DateTime toDate, string message)
		{
			SetupData();
			var sql = new StringBuilder();
			new JobDocAddress(receiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			var yardUnitState = new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			var receiveAdvice2 = new CYDReceiveAdvice(warehouse, "JOB003") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var jobDocAddress = new JobDocAddress(receiveAdvice2.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertExceptionThrown(
				message,
				typeof(SqlException),
				$"Same unit number already exists in Pre-Arrival Instruction ID: {receiveAdvice.YRA_JobNumber}, Client: {clientCode} - {clientFullName}, From date: {receiveAdvice.YRA_FromDate.ToString("dd/MM/yyyy")}, To date: {receiveAdvice.YRA_ToDate.ToString("dd/MM/yyyy")}",
				() => JobDocAddress.UpdateWhere(jobDocAddress.PK).Set(u => u.E2_ParentID, receiveAdvice1.PK).Post(TestConnection),
				true
			);
		}

		public void TestPreventUpdateIfDuplicate_SameClientHasTimeIntersection1()
		{
			TestPreventUpdateIfDuplicate_SameClientAndPeriod(fromDate, toDate, "Should prevent Update if same client and time range intersection");
		}

		public void TestPreventUpdateIfDuplicate_SameClientHasTimeIntersection2()
		{
			TestPreventUpdateIfDuplicate_SameClientAndPeriod(fromDate, fromDate, "Should prevent Update if time range intersects only on the first day");
		}

		public void TestPreventUpdateIfDuplicate_SameClientHasTimeIntersection3()
		{
			TestPreventUpdateIfDuplicate_SameClientAndPeriod(toDate, toDate, "Should prevent Update if time range intersects only on the last day");
		}

		public void TestPreventUpdateIfDuplicate_SameReceiveInstruction()
		{
			SetupData();
			var sql = new StringBuilder();
			new JobDocAddress(receiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);

			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			var yardUnitState = new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			var receiveAdvice2 = new CYDReceiveAdvice(warehouse, "JOB003") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var jobDocAddress = new JobDocAddress(receiveAdvice2.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertExceptionThrown(
				"Should prevent Update if same Receive Instruction",
				typeof(SqlException),
				$"Same unit number already exists in Pre-Arrival Instruction ID: {receiveAdvice.YRA_JobNumber}, Client: {clientCode} - {clientFullName}, From date: {receiveAdvice.YRA_FromDate.ToString("dd/MM/yyyy")}, To date: {receiveAdvice.YRA_ToDate.ToString("dd/MM/yyyy")}",
				() => JobDocAddress.UpdateWhere(jobDocAddress.PK).Set(u => u.E2_ParentID, receiveAdvice1.PK).Post(TestConnection),
				true
			);
		}

		public void TestAllowUpdateIfDuplicate_DifferentClient()
		{
			SetupData();
			var sql = new StringBuilder();

			var orgHeaderPK1 = Guid.NewGuid();
			var orgAddressPK1 = Guid.NewGuid();
			var insertSql = $@"
				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
				VALUES ('{orgHeaderPK1}', 'clientCode', 'clientFullName', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Code, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
				VALUES ('{orgAddressPK1}', '{orgHeaderPK1}', 'AAA', '2 CHOME', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			sql.AppendLine(insertSql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			var yardUnitState = new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			var receiveAdvice2 = new CYDReceiveAdvice(warehouse, "JOB003") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var jobDocAddress = new JobDocAddress(receiveAdvice2.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertNoExceptionThrown(
				"Should allow Update if different client",
				() => JobDocAddress.UpdateWhere(jobDocAddress.PK).Set(u => u.E2_ParentID, receiveAdvice1.PK).Post(TestConnection)
			);
		}

		public void TestAllowUpdateIfDuplicate_SameClientDifferentPeriod(DateTime fromDate, DateTime toDate, string message)
		{
			SetupData();
			var sql = new StringBuilder();
			var jobDocAddress1 = new JobDocAddress(receiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			var yardUnitState = new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			var receiveAdvice2 = new CYDReceiveAdvice(warehouse, "JOB003") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var jobDocAddress = new JobDocAddress(receiveAdvice2.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertNoExceptionThrown(
				message,
				() => JobDocAddress.UpdateWhere(jobDocAddress.PK).Set(u => u.E2_ParentID, receiveAdvice1.PK).Post(TestConnection)
			);
		}

		public void TestAllowUpdateIfDuplicate_SameClientDifferentPeriodLater()
		{
			TestAllowUpdateIfDuplicate_SameClientDifferentPeriod(new DateTime(2025, 4, 26), new DateTime(2025, 4, 28), "Should allow Update if same client and time period is later");
		}

		public void TestAllowUpdateIfDuplicate_SameClientDifferentPeriodEarlier()
		{
			TestAllowUpdateIfDuplicate_SameClientDifferentPeriod(new DateTime(2025, 4, 1), new DateTime(2025, 4, 2), "Should allow Update if same client and time period is earlier");
		}

		public void TestAllowUpdateIfDuplicate_UnitHasDifferentType()
		{
			SetupData();
			var sql = new StringBuilder();

			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem(type: "GEN").AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			var yardUnitState = new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			var receiveAdvice2 = new CYDReceiveAdvice(warehouse, "JOB003") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var jobDocAddress = new JobDocAddress(receiveAdvice2.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertNoExceptionThrown(
				"Should allow Update if unit has a different type",
				() => JobDocAddress.UpdateWhere(jobDocAddress.PK).Set(u => u.E2_ParentID, receiveAdvice1.PK).Post(TestConnection)
			);
		}

		public void TestPreventUpdateIfDuplicate_UnitNotGatedOut()
		{
			SetupData();
			var sql = new StringBuilder();

			var dispatchTransportationUnit = new CYDTransportationUnit(warehouse, "TPU123").AppendInsertAndReturnObject(sql);
			new JobDocAddress(receiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine, YUS_YTU_DispatchTransportationUnit = dispatchTransportationUnit }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			var yardUnitState = new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			var receiveAdvice2 = new CYDReceiveAdvice(warehouse, "JOB003") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var jobDocAddress = new JobDocAddress(receiveAdvice2.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertExceptionThrown(
				"Should prevent Update if unit not gated out",
				typeof(SqlException),
				$"Same unit number already exists in Pre-Arrival Instruction ID: {receiveAdvice.YRA_JobNumber}, Client: {clientCode} - {clientFullName}, From date: {receiveAdvice.YRA_FromDate.ToString("dd/MM/yyyy")}, To date: {receiveAdvice.YRA_ToDate.ToString("dd/MM/yyyy")}",
				() => JobDocAddress.UpdateWhere(jobDocAddress.PK).Set(u => u.E2_ParentID, receiveAdvice1.PK).Post(TestConnection),
				true
			);
		}

		public void TestAllowUpdateIfDuplicate_UnitGatedOut()
		{
			SetupData();
			var sql = new StringBuilder();
			new JobDocAddress(receiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);

			var warehouseLocations = warehouse.CreateLocations(sql, 1);
			var dispatchTransportationUnit = new CYDTransportationUnit(warehouse, "TPU123") { YTU_WL_WaitingBayLocation = warehouseLocations[0], YTU_GateInTime = new DateTime(2025, 4, 19), YTU_GateOutTime = new DateTime(2025, 4, 20) }.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine, YUS_YTU_DispatchTransportationUnit = dispatchTransportationUnit }.AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			var receiveAdvice1 = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice1) { YRL_YLI_UnitLineItem = unitLineItem1 }.AppendInsertAndReturnObject(sql);
			var yardUnitState = new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem1) { YUS_UnitID = unitID, YUS_WW_CurrentYard = warehouse, YUS_YRL_ReceiveLine = receiveAdviceLine1 }.AppendInsertAndReturnObject(sql);

			var receiveAdvice2 = new CYDReceiveAdvice(warehouse, "JOB003") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var jobDocAddress = new JobDocAddress(receiveAdvice2.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertNoExceptionThrown(
				"Should allow Update if unit gated out",
				() => JobDocAddress.UpdateWhere(jobDocAddress.PK).Set(u => u.E2_ParentID, receiveAdvice1.PK).Post(TestConnection)
			);
		}
		#endregion

		void SetupData()
		{
			var sql = new StringBuilder();
			var orgHeaderPK = Guid.NewGuid();

			warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
			unitLineItem = new CYDUnitLineItem().AppendInsertAndReturnObject(sql);
			receiveAdvice = new CYDReceiveAdvice(warehouse, "JOB001", "AccNumber") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			receiveAdviceLine = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLineItem }.AppendInsertAndReturnObject(sql);

			var insertSql = $@"
				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
				VALUES ('{orgHeaderPK}', '{clientCode}', '{clientFullName}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Code, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
				VALUES ('{orgAddressPK}', '{orgHeaderPK}', 'AAA', '2 CHOME', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			sql.AppendLine(insertSql);
			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		WhsWarehouse warehouse;
		CYDReceiveAdvice receiveAdvice;
		CYDUnitLineItem unitLineItem;
		CYDReceiveAdviceLine receiveAdviceLine;
		readonly string unitID = "UnitID1";
		readonly string clientCode = "TNT TEST";
		readonly string clientFullName = "TNT EXPRESS TEST";
		readonly Guid orgAddressPK = Guid.NewGuid();
		readonly DateTime fromDate = new DateTime(2025, 4, 15);
		readonly DateTime toDate = new DateTime(2025, 4, 25);
	}
}
