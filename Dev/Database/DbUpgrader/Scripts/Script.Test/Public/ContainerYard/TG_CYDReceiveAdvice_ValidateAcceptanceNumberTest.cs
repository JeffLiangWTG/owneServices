using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.ContainerYard;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.ContainerYard
{
	[TestedType(typeof(TG_CYDReceiveAdvice_ValidateAcceptanceNumber))]
	class TG_CYDReceiveAdvice_ValidateAcceptanceNumberTest : DBCreateTriggerScriptTest
	{
		public void TestPreventInsertIfDuplicate_TimePeriodOverlapping1()
		{
			AssertPreventInsertIfDuplicate_TimePeriodOverlapping(DateTime.Now.AddDays(0), DateTime.Now.AddDays(4), "Should prevent Insert if same acceptance number and from date intersects with an existing Release Advice");
		}

		public void TestPreventInsertIfDuplicate_TimePeriodOverlapping2()
		{
			AssertPreventInsertIfDuplicate_TimePeriodOverlapping(DateTime.Now.AddDays(-4), DateTime.Now.AddDays(0), "Should prevent Insert if same acceptance number and to date intersects with an existing Release Advice");
		}

		public void TestPreventInsertIfDuplicate_TimePeriodOverlapping3()
		{
			AssertPreventInsertIfDuplicate_TimePeriodOverlapping(DateTime.Now.AddDays(0), DateTime.Now.AddDays(1), "Should prevent Insert if same acceptance number and dte range is enveloped by an existing Release Advice");
		}

		public void TestPreventInsertIfDuplicate_TimePeriodOverlapping4()
		{
			AssertPreventInsertIfDuplicate_TimePeriodOverlapping(DateTime.Now.AddDays(-4), DateTime.Now.AddDays(4), "Should prevent Insert if same acceptance number and date range envelops an existing Release Advice");
		}

		void AssertPreventInsertIfDuplicate_TimePeriodOverlapping(DateTime fromTime, DateTime toTime, string message)
		{
			SetupData();
			var sql = new StringBuilder();
			var invalidReceiveAdvice = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromTime, YRA_ToDate = toTime }.AppendInsertAndReturnObject(sql);
			var jobDocAddress = new JobDocAddress(invalidReceiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
				message,
				typeof(SqlException),
	$"Same acceptance number already exists in Pre-Arrival Instruction ID: {receiveAdvice.YRA_JobNumber}, Acceptance Number: {receiveAdvice.YRA_AcceptanceNumber}, Client: {clientCode} - {clientFullName}, From date: {receiveAdvice.YRA_FromDate.ToString("yyyy-MM-dd")}, To date: {receiveAdvice.YRA_ToDate.ToString("yyyy-MM-dd")}",
				() => TestConnection.ExecuteNonQuery(sql.ToString()),
				true
			);
		}

		public void TestAllowInsertIfDuplicate_TimePeriodNotOverLapping1()
		{
			AssertAllowInsertIfDuplicate_TimePeriodNotOverLapping(DateTime.Now.AddDays(4), DateTime.Now.AddDays(5));
		}

		public void TestAllowInsertIfDuplicate_TimePeriodNotOverLapping2()
		{
			AssertAllowInsertIfDuplicate_TimePeriodNotOverLapping(DateTime.Now.AddDays(-5), DateTime.Now.AddDays(-4));
		}

		void AssertAllowInsertIfDuplicate_TimePeriodNotOverLapping(DateTime fromTime, DateTime toTime)
		{
			SetupData();
			var sql = new StringBuilder();
			var invalidReceiveAdvice = new CYDReceiveAdvice(warehouse, "JOB002") { YRA_FromDate = fromTime, YRA_ToDate = toTime }.AppendInsertAndReturnObject(sql);
			var jobDocAddress = new JobDocAddress(invalidReceiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(
				"Should allow Insert if same acceptance number and date range does not overlap with an existing Release Advice",
				() => TestConnection.ExecuteNonQuery(sql.ToString())
			);
		}

		void SetupData()
		{
			var sql = new StringBuilder();
			var orgHeaderPK = Guid.NewGuid();
			warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
			receiveAdvice = new CYDReceiveAdvice(warehouse, "JOB001") { YRA_FromDate = fromDate, YRA_ToDate = toDate }.AppendInsertAndReturnObject(sql);
			var insertSql = $@"
				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
				VALUES ('{orgHeaderPK}', '{clientCode}', '{clientFullName}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Code, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
				VALUES ('{orgAddressPK}', '{orgHeaderPK}', 'AAA', '2 CHOME', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			sql.AppendLine(insertSql);
			var jobDocAddress = new JobDocAddress(receiveAdvice.PK, "YRA", "BKD") { E2_OA_Address = orgAddressPK }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		WhsWarehouse warehouse;
		CYDReceiveAdvice receiveAdvice;
		readonly string clientCode = "TNT";
		readonly string clientFullName = "TNTEXPRESS";
		readonly Guid orgAddressPK = Guid.NewGuid();
		readonly DateTime fromDate = DateTime.Now.AddDays(-3);
		readonly DateTime toDate = DateTime.Now.AddDays(3);
	}
}
