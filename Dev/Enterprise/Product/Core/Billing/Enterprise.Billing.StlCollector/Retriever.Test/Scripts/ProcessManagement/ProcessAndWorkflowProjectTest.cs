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
	[TestedType(typeof(ProcessAndWorkflowProject))]
	class ProcessAndWorkflowProjectTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @OaPk UNIQUEIDENTIFIER = (SELECT TOP 1 OA_PK FROM dbo.OrgAddress);
				DECLARE @OcPk UNIQUEIDENTIFIER = (SELECT TOP 1 OC_PK FROM dbo.OrgContact);
				DECLARE @GsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @PerPk UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbPerson (PER_PK, PER_FullName) values (@PerPk, 'name')	

				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_GB_HomeBranch, GS_IsActive, GS_IsDevice, GS_IsResource, GS_IsSystemAccount, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES
					(@GsPk01, 'US1', 'Staff001', 'staff.001', @GbPk, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@GsPk02, 'US2', 'Staff002', 'staff.002', @GbPk, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.WorkProject (WKP_PK, WKP_ProjectNumber, WKP_SystemCreateTimeUtc, WKP_SystemCreateUser, WKP_OA_ClientAddress, WKP_OC_Contact, WKP_SystemLastEditTimeUtc, WKP_SystemLastEditUser) VALUES
					(newid(), 'PRJ00001001', '2016-05-31 01:11:00', 'US1', @OaPk, @OcPk, GetUtcDate(), 'E'),
					(newid(), 'PRJ00002002', '2016-06-30 00:00:00', 'US1', @OaPk, @OcPk, GetUtcDate(), 'E'),
					(newid(), 'PRJ00003003', '2015-06-12 03:13:00', 'US2', @OaPk, @OcPk, GetUtcDate(), 'E'),
					(newid(), 'PRJ00004004', '2016-06-23 14:24:00', 'US2', @OaPk, @OcPk, GetUtcDate(), 'E');";
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
}
