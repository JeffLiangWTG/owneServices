using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting.Testing
{
	[TestedType(typeof(Report_AllJobProfitCharge))]
	class Report_AllJobProfitChargeTest : BiCreateScriptTest
	{
		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }

		public void TestJobInactive()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				edwConnection.BeginTransaction();
				InitActiveAndInactiveJobs(edwConnection);

				var dt = GetResultSetAll(edwConnection);

				AssertEquals("Should have data with inactive job. 2 lines come through", 2, dt.Rows.Count);
				AssertEquals("Should have data with inactive job.", 1, dt.Select($"JobNo = '{InactiveJobNumber}'").Length);
			}
		}

		public void TestActiveStatusFiltering()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				edwConnection.BeginTransaction();
				InitActiveAndInactiveJobs(edwConnection);

				var dtWithAllLines = GetResultSetAll(edwConnection, "All");
				var dtWithActiveLines = GetResultSetAll(edwConnection, "Active");
				var dtWithInactiveLines = GetResultSetAll(edwConnection, "Inactive");

				AssertActiveStatusCount(dtWithAllLines, "All", 2, 1, 1);
				AssertActiveStatusCount(dtWithActiveLines, "Active", 1, 1, 0);
				AssertActiveStatusCount(dtWithInactiveLines, "Inactive", 1, 0, 1);
			}
		}

		void AssertActiveStatusCount(DataTable dt, string activeStatus, int allLinesCount, int activeLinesCount, int inactiveLinesCount)
		{
			AssertEquals($"Should have {allLinesCount} lines when active status filter is '{activeStatus}'", allLinesCount, dt.Rows.Count);
			AssertEquals($"Should have {activeLinesCount} lines of active job when active status filter is '{activeStatus}'", activeLinesCount, dt.Select("IsActive = 1").Length);
			AssertEquals($"Should have {inactiveLinesCount} lines of inactive job when active status filter is '{activeStatus}'", inactiveLinesCount, dt.Select("IsActive = 0").Length);
		}

		void InitActiveAndInactiveJobs(AdminConnection edwConnection)
		{
			var helper = new TestDbHelper(edwConnection);

			helper.CreateEDWCompany(ScriptDbName, edwConnection, "TST", DefaultCompanyPK, DefaultCompanyKey);
			var chargeCode1PK = helper.CreateEDWChargeCode(ScriptDbName, edwConnection, DefaultCompanyKey, "CC1", 11);
			var creditorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZC", "Creditor", 1);
			var debtorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZD", "Debtor", 2);
			var branchPK = helper.CreateEDWBranch(ScriptDbName, edwConnection, "ZZB", DefaultCompanyKey, 21);
			var departmentPK = helper.CreateEDWDepartment(ScriptDbName, edwConnection, "ZZD", 31);
			var shipmentPK = helper.CreateEDWShipment(ScriptDbName, edwConnection, "S99999999", "2012-07-25", 41);

			var activeJob1PK = helper.CreateEDWJob(ScriptDbName, edwConnection, ActiveJobNumber, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", "2012-07-25", 51, 1);
			var inactiveJobPK = helper.CreateEDWJob(ScriptDbName, edwConnection, InactiveJobNumber, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, "ET", shipmentPK, "WRK", "2012-07-25", 51, 0);

			helper.CreateEDWJobChargeAndLines(ScriptDbName, edwConnection, activeJob1PK, chargeCode1PK, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, debtorOrgPK, creditorOrgPK, 100, "2012-07-25");
			helper.CreateEDWJobChargeAndLines(ScriptDbName, edwConnection, inactiveJobPK, chargeCode1PK, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, debtorOrgPK, creditorOrgPK, 111, "2012-07-25");
		}

		#region Show Reverse WIP ACR filter test

		public void TestDoNotShowReverseWIPACRFilter()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				edwConnection.BeginTransaction();

				var helper = new TestDbHelper(edwConnection);

				helper.CreateEDWCompany(ScriptDbName, edwConnection, "TST", DefaultCompanyPK, DefaultCompanyKey);
				var chargeCode1PK = helper.CreateEDWChargeCode(ScriptDbName, edwConnection, DefaultCompanyKey, "CC1", 11);
				var chargeCode2PK = helper.CreateEDWChargeCode(ScriptDbName, edwConnection, DefaultCompanyKey, "CC2", 11);
				var creditorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZC", "Creditor", 1);
				var debtorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZD", "Debtor", 2);
				var branchPK = helper.CreateEDWBranch(ScriptDbName, edwConnection, "ZZB", DefaultCompanyKey, 21);
				var departmentPK = helper.CreateEDWDepartment(ScriptDbName, edwConnection, "ZZD", 31);
				var shipmentPK = helper.CreateEDWShipment(ScriptDbName, edwConnection, "S99999999", "2012-07-25", 41);

				var job1PK = helper.CreateEDWJob(ScriptDbName, edwConnection, "J0001", DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", "2012-07-25", 51, 1);

				helper.CreateEDWJobChargeAndLines(ScriptDbName, edwConnection, job1PK, chargeCode1PK, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, debtorOrgPK, creditorOrgPK, 100, "2012-07-25");
				helper.CreateEDWJobChargeAndLines(ScriptDbName, edwConnection, job1PK, chargeCode2PK, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, debtorOrgPK, creditorOrgPK, 100, "2012-07-25", "2012-07-25");

				var dtWithReverseWIPACR = GetResultSetAll(edwConnection, "All", "");
				var dtWithoutReverseeWIPACR = GetResultSetAll(edwConnection, "All", "Y");

				AssertEquals("Number of result", 2, dtWithReverseWIPACR.Rows.Count);
				AssertEquals("Number of result", 1, dtWithoutReverseeWIPACR.Rows.Count);
			}
		}

		#endregion

		DataTable GetResultSetAll(AdminConnection edwConnection, string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			string sql = string.Format(@"
SELECT * FROM [{0}].[dbo].[Report_AllJobProfitCharge](
'{1}',					--@CompanyPK
'1900-01-01 00:00:00',					--@TransactionFrom
'2079-06-06 23:59:29',					--@TransactionTo
'',						--@JobType
'',					--@OutstandingWIP
'',					--@OutstandingACR
'',						--@ChargeCode
'',						--@ChargeCodeNOTIN
NULL,					--@ChargeGroup
NULL,					--@SalesGroup
NULL,					--@ExpenseGroup
'',						--@TransactionBranch
'',						--@TransactionDepartment
'',						--@TransactionDebtor
'',						--@TransactionCreditor
'',						--@IsCommissionable
NULL,					--@ProfitLossReasonCode
'1900-01-01 00:00:00',	--@RevRecogFrom
'2079-06-06 23:59:29',	--@RevRecogTo
NULL,					--@CFSJobType
'{2}',					--@ActiveStatus
'{3}'					--@NotIncludeReversedWIPACR
)",
				ScriptDbName,
				DefaultCompanyPK,           //@CompanyPK
				activeStatus,                //@ActiveStatus
				notIncludeReversedWIPACR    //@NotIncludeReversedWIPACR
			);
			var command = edwConnection.Command(sql);
			return DataUtils.GetDataTableFromCommand(command);
		}

		readonly Guid DefaultCompanyPK = new Guid("FC4CF1FF-7FFD-4356-A3B3-6E15D8896066");
		const int DefaultCompanyKey = 9999;
		const string ActiveJobNumber = "S99999999";
		const string InactiveJobNumber = "I99999999";
	}
}
