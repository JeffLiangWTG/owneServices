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
	[TestedType(typeof(ProcessAndWorkflowWorkItem))]
	class ProcessAndWorkflowWorkItemTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @PerPk UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbPerson (PER_PK, PER_FullName) values (@PerPk, 'name')				

				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_GB_HomeBranch, GS_IsActive, GS_IsDevice, GS_IsResource, GS_IsSystemAccount, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES
					(@GsPk01, 'US1', 'Staff001', 'staff.001', @GbPk, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@GsPk02, 'US2', 'Staff002', 'staff.002', @GbPk, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.WorkItem (WKI_PK, WKI_WorkItemNumber, WKI_SystemCreateTimeUtc, WKI_SystemCreateUser, WKI_SystemLastEditTimeUtc, WKI_SystemLastEditUser) VALUES
					(newid(), 'WI00001001', '2016-05-31 01:11:00', 'US1', '2016-05-31 01:11:00', 'US1'),
					(newid(), 'WI00002002', '2016-06-30 00:00:00', 'US1', '2016-06-30 00:00:00', 'US1'),
					(newid(), 'WI00003003', '2015-06-12 03:13:00', 'US2', '2015-06-12 03:13:00', 'US2'),
					(newid(), 'WI00004004', '2016-06-23 14:24:00', 'US2', '2016-06-23 14:24:00', 'US2');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			AssertRow(transactions, 0, "DEM", "DEM", "US1", 1);
			AssertRow(transactions, 1, "DEM", "DEM", "US2", 1);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedCompanyCode, string expectedBranchCode, string expectedUserCode, int expectedCount)
		{
			string assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.GetCompanyCode() == expectedCompanyCode && t.GetBranchCode() == expectedBranchCode && t.ClientStaffCode == expectedUserCode);

			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2016, 6, 1), transaction.ServiceOccuredUTC);
			AssertEquals(assertPrefix + "TransactionGuidReference", "13A03301-0000-0000-0000-000000000000", transaction.Reference5);
			AssertEquals(assertPrefix + "BranchCode", expectedBranchCode, transaction.GetBranchCode());
			AssertEquals(assertPrefix + "ItemCount", expectedCount, transaction.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get { return AusydMonthRange.New(2016, 6); }
		}

		public void TestScriptShouldUseDateTimeParameters()
		{
			var script = new CreatedWorkflows();
			var parameters = ((IStlScript)new RefStlScriptRetriever(script)).GetInputParameters(AusydMonthRange.New(2016, 6));
			AssertEquals(true, parameters.All(x => x.SchemaColumn.SqlDbType == SqlDbType.SmallDateTime));
		}

		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}
	}

	[TestedType(typeof(ProcessAndWorkflowWorkItem))]
	class ProcessAndWorkflowWorkItem_NoExtraNonsenseTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			const string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @PerPk UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbPerson (PER_PK, PER_FullName) values (@PerPk, 'name')				

				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_GB_HomeBranch, GS_IsActive, GS_IsDevice, GS_IsResource, GS_IsSystemAccount, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES
					(@GsPk01, 'US1', 'Staff001', 'staff.001', @GbPk, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.WorkItem (WKI_PK, WKI_WorkItemNumber, WKI_SystemCreateTimeUtc, WKI_SystemCreateUser, WKI_SystemLastEditTimeUtc, WKI_SystemLastEditUser, WKI_WorkItemType, WKI_WorkItemArea) VALUES
					(newid(), 'WI00002002', '2016-06-30 00:00:00', 'US1', '2016-06-30 00:00:00', 'US1', 'ABC', '123'),
					(newid(), 'WI00002003', '2016-06-30 00:00:00', 'US1', '2016-06-30 00:00:00', 'US1', 'ADH', '123'),
					(newid(), 'WI00002004', '2016-06-30 00:00:00', 'US1', '2016-06-30 00:00:00', 'US1', 'ABC', 'WHS'),
					(newid(), 'WI00002005', '2016-06-30 00:00:00', 'US1', '2016-06-30 00:00:00', 'US1', 'ADH', 'WHS');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());

			var transaction = transactions.Single();
			AssertEquals("The results should include all rows, including those with the criteria formerly reserved for WhsAdhocServiceJobs. SAD!", 4, transaction.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get { return AusydMonthRange.New(2016, 6); }
		}
	}
}
