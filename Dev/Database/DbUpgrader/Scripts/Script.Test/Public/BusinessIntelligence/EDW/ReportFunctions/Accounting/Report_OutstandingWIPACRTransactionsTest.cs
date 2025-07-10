using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting.Testing
{
	[TestedType(typeof(Report_OutstandingWIPACRTransactions))]
	class Report_OutstandingWIPACRTransactionsTest : BiCreateScriptTest
	{
		TestDbHelper helper;

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestJobTypeFilter()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				edwConnection.BeginTransaction();
				helper = new TestDbHelper(edwConnection);

				helper.CreateEDWCompany(ScriptDbName, edwConnection, "TST", DefaultCompanyPK, DefaultCompanyKey);
				var chargeCode1PK = helper.CreateEDWChargeCode(ScriptDbName, edwConnection, DefaultCompanyKey, "CC1", 11);
				var creditorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZC", "Creditor", 1);
				var debtorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZD", "Debtor", 2);
				var branchPK = helper.CreateEDWBranch(ScriptDbName, edwConnection, "ZZB", DefaultCompanyKey, 21);
				var departmentPK = helper.CreateEDWDepartment(ScriptDbName, edwConnection, "ZZD", 31);
				var shipmentPK = helper.CreateEDWShipment(ScriptDbName, edwConnection, "S99999999", "2012-07-25", 41);
				var shipmentJobPK = helper.CreateEDWJob(ScriptDbName, edwConnection, "S99999999", DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", "2012-07-25", 51, 1);
				var shipmentJobPK2 = helper.CreateEDWJob(ScriptDbName, edwConnection, "I99999999", DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, "ET", shipmentPK, "WRK", "2012-07-25", 51, 1);
				helper.CreateEDWJobChargeAndLines(ScriptDbName, edwConnection, shipmentJobPK, chargeCode1PK, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, debtorOrgPK, creditorOrgPK, 100, "2012-07-25");
				helper.CreateEDWJobChargeAndLines(ScriptDbName, edwConnection, shipmentJobPK2, chargeCode1PK, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, debtorOrgPK, creditorOrgPK, 111, "2012-07-25");

				var dt = GetResultSetAll(edwConnection, Array.Empty<Guid>());

				AssertEquals("No job type filter. All 4 lines come through", 4, dt.Rows.Count);

				dt = GetResultSetS(edwConnection, Array.Empty<Guid>());
				AssertEquals("Shipment type filter. Only 2 lines come through", 2, dt.Rows.Count);
				AssertEquals("Shipment type filter. Both lines are shipments.", "S99999999", dt.Rows[0]["JobNum"]);
				AssertEquals("Shipment type filter. Both lines are shipments.", "S99999999", dt.Rows[1]["JobNum"]);

				dt = GetResultSetW(edwConnection, Array.Empty<Guid>());
				AssertEquals("Warehouse type filter. Only 2 lines come through", 2, dt.Rows.Count);
				AssertEquals("Warehouse type filter. Both lines are storage.", "I99999999", dt.Rows[0]["JobNum"]);
				AssertEquals("Warehouse type filter. Both lines are storage.", "I99999999", dt.Rows[1]["JobNum"]);
			}
		}

		public void TestOSCurrencyOSAmount()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				edwConnection.BeginTransaction();
				helper = new TestDbHelper(edwConnection);

				helper.CreateEDWCompany(ScriptDbName, edwConnection, "TST", DefaultCompanyPK, DefaultCompanyKey);
				var chargeCode1PK = helper.CreateEDWChargeCode(ScriptDbName, edwConnection, DefaultCompanyKey, "CC1", 11);
				var creditorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZC", "Creditor", 1);
				var debtorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZD", "Debtor", 2);
				var branchPK = helper.CreateEDWBranch(ScriptDbName, edwConnection, "ZZB", DefaultCompanyKey, 21);
				var departmentPK = helper.CreateEDWDepartment(ScriptDbName, edwConnection, "ZZD", 31);
				var shipmentPK = helper.CreateEDWShipment(ScriptDbName, edwConnection, "S99999999", "2012-07-25", 41);
				var shipmentJobPK = helper.CreateEDWJob(ScriptDbName, edwConnection, "S99999999", DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", "2012-07-25", 51, 1);
				helper.CreateEDWJobChargeAndLines(ScriptDbName, edwConnection, shipmentJobPK, chargeCode1PK, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, debtorOrgPK, creditorOrgPK, 100, "2012-07-25");

				var dt = GetResultSetS(edwConnection, Array.Empty<Guid>());

				AssertEquals("Shipment type filter. Only 2 lines come through", 2, dt.Rows.Count);
				AssertEquals("Shipment type filter. Both lines are shipments.", "S99999999", dt.Rows[0]["JobNum"]);
				AssertEquals("Shipment type filter. Both lines are shipments.", "S99999999", dt.Rows[1]["JobNum"]);

				var revLine = dt.Rows.Cast<DataRow>().FirstOrDefault(x => x["Type"].ToString() == "WIP");
				AssertEquals("USD", revLine["OSCurrency"]);
				AssertEquals(100m, revLine["OSAmount"]);

				var cstLine = dt.Rows.Cast<DataRow>().FirstOrDefault(x => x["Type"].ToString() == "ACR");
				AssertEquals("GBP", cstLine["OSCurrency"]);
				AssertEquals(80m, cstLine["OSAmount"]);
			}
		}

		public void TestJobInactive()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				edwConnection.BeginTransaction();
				helper = new TestDbHelper(edwConnection);

				helper.CreateEDWCompany(ScriptDbName, edwConnection, "TST", DefaultCompanyPK, DefaultCompanyKey);
				var chargeCode1PK = helper.CreateEDWChargeCode(ScriptDbName, edwConnection, DefaultCompanyKey, "CC1", 11);
				var creditorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZC", "Creditor", 1);
				var debtorOrgPK = helper.CreateEDWOrganisation(ScriptDbName, edwConnection, "ZZD", "Debtor", 2);
				var branchPK = helper.CreateEDWBranch(ScriptDbName, edwConnection, "ZZB", DefaultCompanyKey, 21);
				var departmentPK = helper.CreateEDWDepartment(ScriptDbName, edwConnection, "ZZD", 31);
				var shipmentPK = helper.CreateEDWShipment(ScriptDbName, edwConnection, "S99999999", "2012-07-25", 41);
				var shipmentJobPK = helper.CreateEDWJob(ScriptDbName, edwConnection, "S99999999", DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", "2012-07-25", 51, 1);
				var shipmentJobPK2 = helper.CreateEDWJob(ScriptDbName, edwConnection, "I99999999", DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, "ET", shipmentPK, "WRK", "2012-07-25", 51, 0);
				helper.CreateEDWJobChargeAndLines(ScriptDbName, edwConnection, shipmentJobPK, chargeCode1PK, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, debtorOrgPK, creditorOrgPK, 100, "2012-07-25");
				helper.CreateEDWJobChargeAndLines(ScriptDbName, edwConnection, shipmentJobPK2, chargeCode1PK, DefaultCompanyKey, DefaultCompanyPK, branchPK, departmentPK, debtorOrgPK, creditorOrgPK, 111, "2012-07-25");

				var dt = GetResultSetAll(edwConnection, Array.Empty<Guid>());

				AssertEquals("Should have data with inactive job. 4 lines come through", 4, dt.Rows.Count);
				AssertEquals("Should have data with inactive job.", 2, dt.Select("JobNum = 'I99999999'").Length);
			}
		}

		DataTable GetResultSetAll(AdminConnection edwConnection, Guid[] values)
		{
			string sql = string.Format(
				@"SELECT * FROM [{0}].[dbo].[Report_OutstandingWIPACRTransactions]('2023-07-31','FC4CF1FF-7FFD-4356-A3B3-6E15D8896066','ALL','','',null,null,null,null,null,null,'1900-01-01 00:00:00','2079-06-06 23:59:29',@ultimateMNGPKs, @MNGListIsEmpty)",
				ScriptDbName);
			var command = edwConnection.Command(sql);
			helper.AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@ultimateMNGPKs", "@MNGListIsEmpty", values);
			return DataUtils.GetDataTableFromCommand(command);
		}

		DataTable GetResultSetS(AdminConnection edwConnection, Guid[] values)
		{
			string sql = string.Format(
				@"SELECT * FROM [{0}].[dbo].[Report_OutstandingWIPACRTransactions]('2023-07-31','FC4CF1FF-7FFD-4356-A3B3-6E15D8896066','ALL','','S',null,null,null,null,null,null,'1900-01-01 00:00:00','2079-06-06 23:59:29',@ultimateMNGPKs, @MNGListIsEmpty)",
				ScriptDbName);
			var command = edwConnection.Command(sql);
			helper.AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@ultimateMNGPKs", "@MNGListIsEmpty", values);
			return DataUtils.GetDataTableFromCommand(command);
		}

		DataTable GetResultSetW(AdminConnection edwConnection, Guid[] values)
		{
			string sql = string.Format(
				@"SELECT * FROM [{0}].[dbo].[Report_OutstandingWIPACRTransactions]('2023-07-31','FC4CF1FF-7FFD-4356-A3B3-6E15D8896066','ALL','','W',null,null,null,null,null,null,'1900-01-01 00:00:00','2079-06-06 23:59:29',@ultimateMNGPKs, @MNGListIsEmpty)",
				ScriptDbName);
			var command = edwConnection.Command(sql);
			helper.AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@ultimateMNGPKs", "@MNGListIsEmpty", values);
			return DataUtils.GetDataTableFromCommand(command);
		}

		readonly Guid DefaultCompanyPK = new Guid("FC4CF1FF-7FFD-4356-A3B3-6E15D8896066");
		const int DefaultCompanyKey = 9999;

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
