using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Billing.Collectors.ProcessManagement;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.ProcessManagement
{
	[TestedType(typeof(ProcessAndWorkflowCustomerServiceTicket))]
	class ProcessAndWorkflowCustomerServiceTicketTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @BranchPK UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'SYD');
				DECLARE @OrgPK UNIQUEIDENTIFIER = (SELECT TOP 1 OH_PK FROM dbo.OrgHeader);
				DECLARE @ContactPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @PersonPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @Staff1PK UNIQUEIDENTIFIER = NEWID();
				DECLARE @Staff2PK UNIQUEIDENTIFIER = NEWID();

				INSERT dbo.GlbPerson (PER_PK, PER_FullName)
				VALUES (@PersonPK, 'How great is it that we have this extra table?? I''m sure it''l be useful one day. Today is not that day.')

				INSERT dbo.OrgContact (OC_PK, OC_OH, OC_PER)
				VALUES (@ContactPK, @OrgPK, @PersonPK)

				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_GB_HomeBranch, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				VALUES
					(@Staff1PK, 'US1', 'User 1', 'US1 Login', @BranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@Staff2PK, 'US2', 'User 2', 'US2 Login', null, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT dbo.WorkRequest (WKR_PK, WKR_RequestNumber, WKR_Summary, WKR_SystemCreateTimeUtc, WKR_SystemCreateUser, WKR_SystemLastEditTimeUtc, WKR_SystemLastEditUser, WKR_GB_Branch, WKR_OC_Client)
				VALUES
					(NEWID(), 'CST00001001', 'Microverse that generates electricity', '2016-05-31 01:11:00', 'US1', '3008-05-31 01:11:00', 'US1', null, @ContactPK),

					(NEWID(), 'CST00002002', 'Microverse that generates electricity', '2016-06-30 00:00:00', 'US1', '3008-06-30 00:00:00', 'US1', null, @ContactPK),
					(NEWID(), 'CST00002003', 'Microverse that generates electricity', '2016-06-30 01:00:00', 'US1', '3008-06-30 00:00:00', 'US1', null, @ContactPK),

					(NEWID(), 'CST00002004', 'Microverse that generates electricity', '2016-06-30 00:00:00', 'US2', '3008-06-30 00:00:00', 'US2', null, @ContactPK),
					(NEWID(), 'CST00002005', 'Microverse that generates electricity', '2016-06-30 00:01:00', 'US2', '3008-06-30 00:00:00', 'US2', null, @ContactPK),
					(NEWID(), 'CST00002006', 'Microverse that generates electricity', '2016-06-30 00:02:00', 'US2', '3008-06-30 00:00:00', 'US2', null, @ContactPK),

					(NEWID(), 'CST00003007', 'Microverse that generates electricity', '2015-06-12 03:13:00', 'US2', '3008-06-12 03:13:00', 'US2', null, @ContactPK),

					(NEWID(), 'CST00004008', 'Microverse that generates electricity', '2016-06-23 14:24:00', 'US2', '3008-06-23 14:24:00', 'US2', @BranchPK, @ContactPK);
				";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertRowContents(transactions, 1, "EDI", "SYD", "US1", 2);
			AssertRowContents(transactions, 2, null, null, "US2", 3);
			AssertRowContents(transactions, 3, "EDI", "SYD", "US2", 1);

			AssertEquals("Number of transactions", 3, transactions.Count());
		}

		static void AssertRowContents(IEnumerable<IStlTransaction> transactions, int rowNumber, string companyCode, string branchCode, string createUserCode, int expectedCount)
		{
			string assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.GetCompanyCode() == companyCode && t.GetBranchCode() == branchCode && t.ClientStaffCode == createUserCode);

			AssertEquals(assertPrefix + "CompanyCode", companyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "BranchCode", branchCode, transaction.GetBranchCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2016, 6, 1), transaction.ServiceOccuredUTC);
			AssertEquals(assertPrefix + "UserCode", createUserCode, transaction.ClientStaffCode);
			AssertEquals(assertPrefix + "ItemCount", expectedCount, transaction.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2016, 6);

		public void TestScriptShouldUseDateTimeParameters()
		{
			var script = new CreatedWorkflows();
			var parameters = ((IStlScript)new RefStlScriptRetriever(script)).GetInputParameters(AusydMonthRange.New(2016, 6));
			AssertEquals(true, parameters.All(x => x.SchemaColumn.SqlDbType == SqlDbType.SmallDateTime));
		}

		public void TestIsSystemLevelFeature()
		{
			AssertEquals(false, ScriptToTest.IsSystemLevel);
		}
	}
}
