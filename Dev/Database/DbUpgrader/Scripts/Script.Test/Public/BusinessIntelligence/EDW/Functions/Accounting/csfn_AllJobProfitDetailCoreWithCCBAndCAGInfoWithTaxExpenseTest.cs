using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(csfn_AllJobProfitDetailCoreWithCCBAndCAGInfoWithTaxExpense))]
	class csfn_AllJobProfitDetailCoreWithCCBAndCAGInfoWithTaxExpenseTest : BiCreateScriptTest
	{
		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestActiveStatusFiltering()
		{
			var companyPK = TestHelper.InsertCompany("CNY", "DCN");
			InitActiveAndInactiveJobs(companyPK);

			var dtWithAllLines = GetResultSetAll(companyPK, activeStatus: "All");
			var dtWithActiveLines = GetResultSetAll(companyPK, activeStatus: "Active");
			var dtWithInactiveLines = GetResultSetAll(companyPK, activeStatus: "Inactive");

			AssertActiveStatusCount(dtWithAllLines, "All", 2, 1, 1);
			AssertActiveStatusCount(dtWithActiveLines, "Active", 1, 1, 0);
			AssertActiveStatusCount(dtWithInactiveLines, "Inactive", 1, 0, 1);
		}

		void AssertActiveStatusCount(DataTable dt, string activeStatus, int allLinesCount, int activeLinesCount, int inactiveLinesCount)
		{
			AssertEquals($"Should have {allLinesCount} lines when active status filter is '{activeStatus}'", allLinesCount, dt.Rows.Count);
			AssertEquals($"Should have {activeLinesCount} lines of active job when active status filter is '{activeStatus}'", activeLinesCount, dt.Select("JH_IsActive = 1").Length);
			AssertEquals($"Should have {inactiveLinesCount} lines of inactive job when active status filter is '{activeStatus}'", inactiveLinesCount, dt.Select("JH_IsActive = 0").Length);
		}

		void InitActiveAndInactiveJobs(Guid companyPK)
		{
			TestHelper.CreateJobHeader("S0001", companyPK);
			TestHelper.CreateJobHeader("S0002", companyPK, isActive: 0);
			TestHelper.InsertGLTransactionLineExtended("WIP", companyPK, "2023-09-08", "2023-09-08");
			TestHelper.InsertGLTransactionLineExtended("WIP", companyPK, "2023-09-08", "", jobHeaderKey: 2);
		}

		DataTable GetResultSetAll(Guid companyPK, string activeStatus = "")
		{
			var sql = string.Format(@"
SELECT * FROM [{0}].[dbo].[csfn_AllJobProfitDetailCoreWithCCBAndCAGInfoWithTaxExpense](
	'{1}',					-- @CompanyPK
	'1900-01-01 00:00:00',	-- @TransactionFrom
	'2079-01-01 00:00:00',	-- @TransactionTo
	'',						-- @JobType
	'',						-- @OutstandingWIP
	'',						-- @OutstandingACR
	'',						-- @NotIncludeReversedWIPACR
	'',						-- @ChargeCode
	'',						-- @ChargeCodeNOTIN
	'',						-- @ChargeGroup
	NULL,					-- @SalesGroup
	NULL,					-- @ExpenseGroup
	NULL,					-- @AL_GBList
	NULL,					-- @AL_GEList
	'',						-- @PostedOnly
	'',						-- @CFSJobType
	'',						-- @UnPostedOnly
	@MNGListValue,			-- @MNGList
	1,						-- @MNGListIsEmpty
	'{2}'					-- @ActiveStatus
)",
				ScriptDbName,
				companyPK,
				activeStatus
			);
			var command = EDWConnection.Command(sql);
			var mngList = new TVPParamInfo("@MNGListValue", "dbo.TVP_uniqueidentifier", typeof(Guid), Array.Empty<object>());
			mngList.AddTVPParameters(command);
			return DataUtils.GetDataTableFromCommand(command);
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ??= new AccountingFunctionTestingHelper(EDWConnection, ScriptDbName);
		AccountingFunctionTestingHelper fTestHelper;
		DbConnection EDWConnection => fEDWConnection ??= Db.NewAdminConnection(ScriptDbName);
		DbConnection fEDWConnection;
	}
}
