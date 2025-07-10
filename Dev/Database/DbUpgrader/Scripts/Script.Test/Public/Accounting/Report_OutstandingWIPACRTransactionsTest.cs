using System;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_OutstandingWIPACRTransactions))]
	class Report_OutstandingWIPACRTransactionsTest : EdwHashTest
	{
		TestDbHelper helper;
		protected override void SetUp()
		{
			base.SetUp();
			helper = new TestDbHelper(TestConnection);
		}

		public class Report_OutstandingWIPACRTransactionsParams
		{
			public DateTime PeriodToDate { get; set; }
			public Guid CompanyPK { get; set; }
			public string Type { get; set; }
			public string JobStatus { get; set; }
			public string JobType { get; set; }
			public string ChargeCode { get; set; }
			public string ChargeGroup { get; set; }
			public string TransactionBranch { get; set; }
			public string TransactionDepartment { get; set; }
			public string TransactionDebtor { get; set; }
			public string TransactionCreditor { get; set; }
			public DateTime RevRecogFrom { get; set; }
			public DateTime RevRecogTo { get; set; }
			public TVPParamInfo MNGList { get; set; }
			public bool MNGListIsEmpty { get; set; }
		}

		public void TestJobTypeFilter()
		{
			var chargeCode1PK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			var creditorOrgPK = helper.InsertOrgHeader("ZZC", "Creditor");
			var debtorOrgPK = helper.InsertOrgHeader("ZZD", "Debtor");
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var shipmentPK = helper.InsertShipment("S99999999", helper.ToDate("2012-07-25"));
			var storagePK = helper.InsertJobStorage("I99999999", helper.ToDate("2012-07-25"), debtorOrgPK);
			var shipmentJobPK = helper.InsertJob("S99999999", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", helper.ToDate("2012-07-25"));
			var storageJobPK = helper.InsertJob("I99999999", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "ET", storagePK, "WRK", helper.ToDate("2012-07-25"));
			helper.InsertJobChargeAndLines("001", shipmentJobPK, chargeCode1PK, DbHelper.GLAccountPK1, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, debtorOrgPK, creditorOrgPK,
				100, 80, true, helper.ToDate("2012-07-25"), null);
			helper.InsertJobChargeAndLines("002", storageJobPK, chargeCode1PK, DbHelper.GLAccountPK1, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, debtorOrgPK, creditorOrgPK,
				111, 87, true, helper.ToDate("2012-07-25"), null);

			var reportFunctionParams = new Report_OutstandingWIPACRTransactionsParams
			{
				PeriodToDate = helper.ToDate("2012-07-31"),
				CompanyPK = TestDbHelper.DefaultCompanyPK,
				Type = "ALL",
				JobStatus = "",
				JobType = "",
				ChargeCode = null,
				ChargeGroup = null,
				TransactionBranch = null,
				TransactionDepartment = null,
				TransactionDebtor = null,
				TransactionCreditor = null,
				RevRecogFrom = helper.ToDate("1900-01-01 00:00:00"),
				RevRecogTo = helper.ToDate("2079-06-06 23:59:29"),
				MNGList = new TVPParamInfo("@MNGList", "dbo.TVP_uniqueidentifier", typeof(Guid), Array.Empty<object>()),
				MNGListIsEmpty = true
			};

			var reader = helper.RunTableValuedFunction("Report_OutstandingWIPACRTransactions", reportFunctionParams);
			var dt = new DataTable();
			dt.Load(reader);
			AssertEquals("No job type filter. All 4 lines come through", 4, dt.Rows.Count);

			reportFunctionParams.JobType = "S";
			reader = helper.RunTableValuedFunction("Report_OutstandingWIPACRTransactions", reportFunctionParams);
			dt = new DataTable();
			dt.Load(reader);
			AssertEquals("Shipment type filter. Only 2 lines come through", 2, dt.Rows.Count);
			AssertEquals("Shipment type filter. Both lines are shipments.", "S99999999", dt.Rows[0]["JobNum"]);
			AssertEquals("Shipment type filter. Both lines are shipments.", "S99999999", dt.Rows[1]["JobNum"]);

			reportFunctionParams.JobType = "W";
			reader = helper.RunTableValuedFunction("Report_OutstandingWIPACRTransactions", reportFunctionParams);
			dt = new DataTable();
			dt.Load(reader);
			AssertEquals("Warehouse type filter. Only 2 lines come through", 2, dt.Rows.Count);
			AssertEquals("Warehouse type filter. Both lines are storage.", "I99999999", dt.Rows[0]["JobNum"]);
			AssertEquals("Warehouse type filter. Both lines are storage.", "I99999999", dt.Rows[1]["JobNum"]);
		}

		public void TestOSCurrencyOSAmount()
		{
			var chargeCode1PK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			var creditorOrgPK = helper.InsertOrgHeader("ZZC", "Creditor");
			var debtorOrgPK = helper.InsertOrgHeader("ZZD", "Debtor");
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var shipmentPK = helper.InsertShipment("S99999999", helper.ToDate("2012-07-25"));
			var shipmentJobPK = helper.InsertJob("S99999999", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", helper.ToDate("2012-07-25"));
			helper.InsertJobChargeAndLines("001", shipmentJobPK, chargeCode1PK, DbHelper.GLAccountPK1, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, debtorOrgPK, creditorOrgPK,
				100, 80, true, helper.ToDate("2012-07-25"), null);

			helper.RunSQL(null, @"
UPDATE dbo.Jobcharge
SET
	JR_RX_NKSellCurrency = 'USD',
	JR_RX_NKCostCurrency = 'GBP',
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'");
			helper.RunSQL(null, @"
UPDATE dbo.Jobcharge
SET
	JR_OSSellAmt = 100,
	JR_OSCostAmt = -80,
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'");

			var reportFunctionParams = new Report_OutstandingWIPACRTransactionsParams
			{
				PeriodToDate = helper.ToDate("2012-07-31"),
				CompanyPK = TestDbHelper.DefaultCompanyPK,
				Type = "ALL",
				JobStatus = "",
				JobType = "",
				ChargeCode = null,
				ChargeGroup = null,
				TransactionBranch = null,
				TransactionDepartment = null,
				TransactionDebtor = null,
				TransactionCreditor = null,
				RevRecogFrom = helper.ToDate("1900-01-01 00:00:00"),
				RevRecogTo = helper.ToDate("2079-06-06 23:59:29"),
				MNGList = new TVPParamInfo("@MNGList", "dbo.TVP_uniqueidentifier", typeof(Guid), Array.Empty<object>()),
				MNGListIsEmpty = true
			};

			reportFunctionParams.JobType = "S";
			var reader = helper.RunTableValuedFunction("Report_OutstandingWIPACRTransactions", reportFunctionParams);
			var dt = new DataTable();
			dt.Load(reader);
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

		public void TestJobInactive()
		{
			var chargeCode1PK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			var creditorOrgPK = helper.InsertOrgHeader("ZZC", "Creditor");
			var debtorOrgPK = helper.InsertOrgHeader("ZZD", "Debtor");
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var shipmentPK = helper.InsertShipment("S99999999", helper.ToDate("2012-07-25"));
			var storagePK = helper.InsertJobStorage("I99999999", helper.ToDate("2012-07-25"), debtorOrgPK);
			var shipmentJobPK = helper.InsertJob("S99999999", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", helper.ToDate("2012-07-25"));
			var storageJobPK = helper.InsertJob("I99999999", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "ET", storagePK, "WRK", helper.ToDate("2012-07-25"));
			helper.InsertJobChargeAndLines("001", shipmentJobPK, chargeCode1PK, DbHelper.GLAccountPK1, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, debtorOrgPK, creditorOrgPK,
				100, 80, true, helper.ToDate("2012-07-25"), null);
			helper.InsertJobChargeAndLines("002", storageJobPK, chargeCode1PK, DbHelper.GLAccountPK1, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, debtorOrgPK, creditorOrgPK,
				111, 87, true, helper.ToDate("2012-07-25"), null);

			helper.RunSQL(new { JH_PK = storageJobPK }, @"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = @JH_PK");

			var reportFunctionParams = new Report_OutstandingWIPACRTransactionsParams
			{
				PeriodToDate = helper.ToDate("2012-07-31"),
				CompanyPK = TestDbHelper.DefaultCompanyPK,
				Type = "ALL",
				JobStatus = "",
				JobType = "",
				ChargeCode = null,
				ChargeGroup = null,
				TransactionBranch = null,
				TransactionDepartment = null,
				TransactionDebtor = null,
				TransactionCreditor = null,
				RevRecogFrom = helper.ToDate("1900-01-01 00:00:00"),
				RevRecogTo = helper.ToDate("2079-06-06 23:59:29"),
				MNGList = new TVPParamInfo("@MNGList", "dbo.TVP_uniqueidentifier", typeof(Guid), Array.Empty<object>()),
				MNGListIsEmpty = true
			};

			var reader = helper.RunTableValuedFunction("Report_OutstandingWIPACRTransactions", reportFunctionParams);
			var dt = new DataTable();
			dt.Load(reader);
			AssertEquals("Should have data with inactive job. 4 lines come through", 4, dt.Rows.Count);
			AssertEquals("Should have data with inactive job. Both lines are shipments.", 2, dt.Select("JobNum = 'I99999999'").Length);
		}
	
		protected override string expectedMainDbFunctionHash => "49B7F6E592547CA818F82160751627545565E28B0DA351DAA30A3443B5DC13E5";
		protected override string expectedEdwDbFunctionHash => "732FEA1DB86AF709B5F7A3DB773CF288070252F495FCAB69BACC4FDAE189A8D6";

		protected override string edwScriptPath => "ReportFunctions/Accounting/Report_OutstandingWIPACRTransactions.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_OutstandingWIPACRTransactions();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.Report_OutstandingWIPACRTransactions();
		}
	}
}

