using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting.Testing
{
	[TestedType(typeof(Report_ForwardingJobProfitAnalysis))]
	class Report_ForwardingJobProfitAnalysisTest : BiCreateScriptTest
	{
		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestActiveStatusFiltering()
		{
			var companyPK = TestHelper.InsertCompany(currency: "CNY", companyCode: "DCN", countryCode: "CN");
			TestHelper.CreateShipment();
			InitActiveAndInactiveJobs(companyPK);

			var dtWithAllLines = GetResultSetAll("CN", companyPK, activeStatus: "All");
			var dtWithActiveLines = GetResultSetAll("CN", companyPK, activeStatus: "Active");
			var dtWithInactiveLines = GetResultSetAll("CN", companyPK, activeStatus: "Inactive");

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

		DataTable GetResultSetAll(string countryCode, Guid companyPK, string activeStatus = "")
		{
			var sql = string.Format(@"
SELECT * FROM [{0}].[dbo].[Report_ForwardingJobProfitAnalysis](
	'{1}',					-- @CurrentCountry
	'{2}',					-- @CompanyPK
	'1900-01-01 00:00:00',	-- @TransactionFrom
	'2079-06-06 23:59:29',	-- @TransactionTo
	'',						-- @JobType
	'',						-- @OutstandingWIP
	'',						-- @OutstandingACR
	'',						-- @NotIncludeReversedWIPACR
	'',						-- @ChargeCode
	'',						-- @ExcludeChargeCode
	'',						-- @ChargeGroup
	NULL,					-- @SalesGroup
	NULL,					-- @ExpenseGroup
	'',						-- @TransactionBranch
	'',						-- @TransactionDepartment
	'',						-- @PostedOnly
	'1900-01-01 00:00:00',	-- @RevRecogFrom
	'2079-06-06 23:59:29',	-- @RevRecogTo
	NULL,					-- @ShipmentTransport
	NULL,					-- @DeclarationTransport
	NULL,					-- @ShipmentCont
	NULL,					-- @DeclarationCont
	NULL,					-- @ISFTransportMode
	NULL,					-- @ISFShipmentType
	@MNGListValue,			-- @MNGList
	1,						-- @MNGListIsEmpty
	'ALL',					-- @Gateway
	'{3}'					-- @ActiveStatus
)",
				ScriptDbName,
				countryCode,
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
