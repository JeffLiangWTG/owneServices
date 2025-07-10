using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting.Testing
{
	[TestedType(typeof(csfn_AllJobProfitDetailCore))]
	class csfn_AllJobProfitDetailCoreTest : BiCreateScriptTest
	{
		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }

		void InitData(AdminConnection edwConnection, bool isReversed = false)
		{
			var helper = new TestDbHelper(edwConnection);
			helper.CreateEDWCompany(ScriptDbName, edwConnection, "TST", DefaultCompanyPK, DefaultCompanyKey);
			var chargeCode1PK = helper.CreateEDWChargeCode(ScriptDbName, edwConnection, DefaultCompanyKey, "CC1", 11);
			var creditorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZC", "Creditor", 1);
			var debtorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZD", "Debtor", 2);
			var branchPK = helper.CreateEDWBranch(ScriptDbName, edwConnection, "ZZB", DefaultCompanyKey, 21);
			var departmentPK = helper.CreateEDWDepartment(ScriptDbName, edwConnection, "ZZD", 31);
			var shipmentPK = helper.CreateEDWShipment(ScriptDbName, edwConnection, "S99999999", "2012-07-25", 41);
			helper.CreateEDWJob(ScriptDbName, edwConnection, "S99999998", DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", "2012-07-25", 51, 1);
			var shipmentJobPK = helper.CreateEDWJob(ScriptDbName, edwConnection, "S99999999", DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", "2012-07-25", 51, 1);
			var shipmentJobPK2 = helper.CreateEDWJob(ScriptDbName, edwConnection, "I99999999", DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, "ET", shipmentPK, "WRK", "2012-07-25", 51, 0);
			helper.CreateEDWJobChargeAndLines(ScriptDbName, edwConnection, shipmentJobPK, chargeCode1PK, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, debtorOrgPK, creditorOrgPK, 100, "2012-07-25");
			helper.CreateEDWJobChargeAndLines(ScriptDbName, edwConnection, shipmentJobPK2, chargeCode1PK, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, debtorOrgPK, creditorOrgPK, 111, "2012-07-25",  isReversed ? "2012-07-26" : null);
		}

		public void Testcsfn_AllJobProfitDetailCore_ActiveStatus()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				edwConnection.BeginTransaction();
				InitData(edwConnection);

				var dt = GetResultSetAll(edwConnection, Array.Empty<Guid>(), activeStatus: "All");

				AssertEquals("Should have data with all jobs. 4 lines come through", 4, dt.Rows.Count);
				AssertEquals("Should have data with all jobs.", 2, dt.Select("JH_JobNum = 'I99999999'").Length);
				AssertEquals("Should not have blank jobs.", 0, dt.Select("JH_JobNum = 'S99999998'").Length);

				dt = GetResultSetAll(edwConnection, Array.Empty<Guid>(), activeStatus: "Active");

				AssertEquals("Should have data with active jobs. 2 lines come through", 2, dt.Rows.Count);
				AssertEquals("Should have data with active jobs.", 2, dt.Select("JH_JobNum = 'S99999999'").Length);
				AssertEquals("Should not have inActive jobs.", 0, dt.Select("JH_JobNum = 'S99999998'").Length);

				dt = GetResultSetAll(edwConnection, Array.Empty<Guid>(), activeStatus: "InActive");

				AssertEquals("Should have data with inActive jobs. 2 lines come through", 2, dt.Rows.Count);
				AssertEquals("Should have data with inActive jobs.", 2, dt.Select("JH_JobNum = 'I99999999'").Length);
				AssertEquals("Should not have active jobs.", 0, dt.Select("JH_JobNum = 'S99999998'").Length);
			}
		}

		#region Show Reversed WIP ACR Filter Test

		public void TestDoNotShowReverseWIPACRFilter()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				edwConnection.BeginTransaction();
				InitData(edwConnection, isReversed: true);

				var resultIncludeReversedWIPACR = GetResultSetAll(edwConnection, Array.Empty<Guid>(), notIncludeReversedWIPACR: "");
				var resultNotIncludeReversedWIPACR = GetResultSetAll(edwConnection, Array.Empty<Guid>(), notIncludeReversedWIPACR: "Y");

				AssertEquals("Number of all result", 6, resultIncludeReversedWIPACR.Rows.Count);
				AssertEquals("Number of result without WIP/ACR", 2, resultNotIncludeReversedWIPACR.Rows.Count);
			}
		}

		#endregion

		DataTable GetResultSetAll(AdminConnection edwConnection, Guid[] values, string notIncludeReversedWIPACR = "", string activeStatus = "")
		{
			var sql = string.Format(
				@"SELECT * FROM [{0}].[dbo].[csfn_AllJobProfitDetailCore]('{1}','1900-01-01 00:00:00','2060-01-1 00:00:00','','','',''
							,NULL,NULL,NULL,NULL,'','','','','','','','{2}','{3}')",
				ScriptDbName, DefaultCompanyPK, notIncludeReversedWIPACR, activeStatus);
			var command = edwConnection.Command(sql);
			return DataUtils.GetDataTableFromCommand(command);
		}

		readonly Guid DefaultCompanyPK = new Guid("FC4CF1FF-7FFD-4356-A3B3-6E15D8896066");
		const int DefaultCompanyKey = 9999;
	}
}
