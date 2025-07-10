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
	[TestedType(typeof(CreatedWorkflowTemplates))]
	class CreatedWorkflowTemplatesTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

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

				INSERT dbo.ProcessTaskTemplate
           (P0_PK, P0_IsValid, P0_IsSystem, P0_IsActive, P0_ProcessType, P0_GC, P0_GB, P0_GE, P0_SubType1, P0_SubType2, P0_SubType3, P0_SubType4, P0_SubType5, P0_LoadPortCountry, P0_DischargePortCountry, P0_OrgAssessmentOrder, P0_FormState, P0_EffectiveStartDateUtc, P0_RespondToCascadedEvents, P0_RecalculateScheduledDate, P0_OH_Client, P0_OA_Address, P0_WW, P0_TaskFallbackMethod, P0_MilestoneFallbackMethod, P0_TriggerFallbackMethod, P0_SystemCreateTimeUtc, P0_SystemCreateUser, P0_SystemLastEditTimeUtc, P0_SystemLastEditUser, P0_IsPartialTemplate, P0_IsUniversal, P0_EffectiveEndDateUtc, P0_FS_BufferManagementSystem, P0_Name, P0_Description, P0_CustomFieldFallback, P0_ReleaseGroupFallbackMethod, P0_IsScreenLayoutFallback)     
					VALUES
					(newid(), 1, 0, 1, 'WKI', @GcPk, null, null, '', '', '', '', '', '', '', '', null, null, 1, 1, null, null, null, 'EFB', 'EFB', 'EFB', '2016-05-31 01:11:00', 'US1', '2016-05-31 01:11:00', 'US1', 0, 0, null, null, 'temp1', 'desc1', 'NFB', 'EFB', 0),
					(newid(), 1, 0, 1, 'WKI', @GcPk, null, null, '', '', '', '', '', '', '', '', null, null, 1, 1, null, null, null, 'EFB', 'EFB', 'EFB', '2016-06-30 00:00:00', 'US1', '2016-06-30 00:00:00', 'US1', 0, 0, null, null, 'temp2', 'desc1', 'NFB', 'EFB', 0),
					(newid(), 1, 0, 1, 'WKI', @GcPk, null, null, '', '', '', '', '', '', '', '', null, null, 1, 1, null, null, null, 'EFB', 'EFB', 'EFB', '2016-06-29 00:00:00', 'US1', '2016-06-29 00:00:00', 'US1', 0, 0, null, null, 'temp3', 'desc1', 'NFB', 'EFB', 0),
					(newid(), 1, 0, 1, 'WKI', @GcPk, null, null, '', '', '', '', '', '', '', '', null, null, 1, 1, null, null, null, 'EFB', 'EFB', 'EFB', '2015-06-12 03:13:00', 'US2', '2015-06-12 03:13:00', 'US2', 0, 0, null, null, 'temp4', 'desc1', 'NFB', 'EFB', 0),
					(newid(), 1, 0, 1, 'WKI', @GcPk, null, null, '', '', '', '', '', '', '', '', null, null, 1, 1, null, null, null, 'EFB', 'EFB', 'EFB', '2016-06-23 14:24:00', 'US2', '2016-06-23 14:24:00', 'US2', 0, 0, null, null, 'temp5', 'desc1', 'NFB', 'EFB', 0),
					(newid(), 1, 0, 1, 'WKI', null, null, null, '', '', '', '', '', '', '', '', null, null, 1, 1, null, null, null, 'EFB', 'EFB', 'EFB', '2016-06-23 14:24:00', 'US2', '2016-06-23 14:24:00', 'US2', 0, 0, null, null, 'temp6', 'desc1', 'NFB', 'EFB', 0),
					(newid(), 1, 1, 1, 'WKI', @GcPk, null, null, '', '', '', '', '', '', '', '', null, null, 1, 1, null, null, null, 'EFB', 'EFB', 'EFB', '2016-06-24 14:24:00', 'US2', '2016-06-24 14:24:00', 'US2', 0, 0, null, null, 'temp7-system', 'desc1', 'NFB', 'EFB', 0);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			AssertRow(transactions, 1, "DEM", "DEM", "US2", 1);
			AssertRow(transactions, 2, null, "DEM", "US2", 1);
			AssertRow(transactions, 3, "DEM", "DEM", "US1", 2);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedCompanyCode, string expectedBranchCode, string expectedUserCode, int expectedCount)
		{
			string assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.GetCompanyCode() == expectedCompanyCode && t.GetBranchCode() == expectedBranchCode && t.ClientStaffCode == expectedUserCode);

			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "BranchCode", expectedBranchCode, transaction.GetBranchCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2016, 6, 1), transaction.ServiceOccuredUTC);
			AssertEquals(assertPrefix + "TransactionGuidReference", "13A03301-0000-0000-0000-000000000000", transaction.Reference5);
			AssertEquals(assertPrefix + "ItemCount", expectedCount, transaction.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2016, 6);

		public void TestScriptShouldUseDateTimeParameters()
		{
			var script = new CreatedWorkflows();
			var parameters = ((IStlScript)new RefStlScriptRetriever(script)).GetInputParameters(AusydMonthRange.New(2016, 6));
			AssertEquals(true, parameters.All(x => x.SchemaColumn.SqlDbType == SqlDbType.SmallDateTime));
		}
	}
}
