using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_AllJobProfitDetailTest : JobProfitFilterTest
	{
		protected override DataTable RunScriptWithFilter(string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			return RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, ZDateTime.Empty, ZDateTime.Empty, activeStatus: activeStatus, notIncludeReversedWIPACR: notIncludeReversedWIPACR);
		}

		#region Active Status filter test

		protected override string[] ActiveStatusFilterKeyColumnsForTest =>
			["JH_JobNum", "AL_LineAmount"];

		protected override string[] ActiveStatusFilterHeadersForTest =>
			["JH_JobNum", "AL_LineAmount", "JH_IsActive"];

		protected override object[][] ActiveStatusFilterActiveLinesForTest =>
			[
				["S0001", -10m, true],
				["S0001", 20m, true],
				["S0002", -40m, true],
				["S0002", 60m, true],
			];
		protected override object[][] ActiveStatusFilterInactiveLinesForTest =>
			[
				["S0003", 90m, false],
				["S0003", -90m, false],
				["S0003", 120m, false],
				["S0003", -120m, false],
				["S0003", 160m, false],
				["S0003", -160m, false],
				["S0003", 200m, false],
				["S0003", -200m, false],
			];

		#endregion

		public void TestOutstandingWIPandACRFiltering()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 100m, 100m);

			charge1.JR_OSCostAmt = 0m;
			charge1.JR_OSSellAmt = 0m;

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 10.0m, 0m);

			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			var job3 = TestObjectCreator.CreateJob(shipment3, false);
			TestObjectCreator.CreateCharge(job3, TestObjectCreator.CC1, 0m, 10.2m);

			var shipment4 = TestObjectCreator.CreateShipment("S0004");
			var job4 = TestObjectCreator.CreateJob(shipment4, false);
			TestObjectCreator.CreateCharge(job4, TestObjectCreator.CC1, 89m, 89m);
			TestObjectCreator.CreateCharge(job4, TestObjectCreator.CC2, -89m, -89m);

			var shipment5 = TestObjectCreator.CreateShipment("S0005");
			var job5 = TestObjectCreator.CreateJob(shipment5, false);
			TestObjectCreator.CreateCharge(job5, TestObjectCreator.CC1, 79m, 79m);
			TestObjectCreator.CreateCharge(job5, TestObjectCreator.CC1, -79m, -79m);

			Factory.Save();

			var headers = new[] { "JH_JobNum", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount", "JH_Profit" };
			var keyColumns = new[] { "JH_JobNum", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };
			var result = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, ZDateTime.Empty, ZDateTime.Empty);
			var lines = new object[][]
						{
							new object[] {	"S0002",		-10m,		0m,		0m,		0m,		-10m	},
							new object[] {	"S0003",		0m,			10.2m,	0m,		0m,		10.2m	},
							new object[] {	"S0004",		-89m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0004",		0m,			89m,	0m,		0m,		0m	},
							new object[] {	"S0004",		89m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0004",		0m,			-89m,	0m,		0m,		0m	},
							new object[] {	"S0005",		-79m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			79m,	0m,		0m,		0m	},
							new object[] {	"S0005",		79m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			-79m,	0m,		0m,     0m  },
						};
			AssertDataTableAllRowsByKeyColumns("Filtering Without outstanding WIP, without outstanding ACR", result, headers, lines, keyColumns);

			result = RunScript(ZDateTime.Empty, ZDateTime.Empty, true, false, ZDateTime.Empty, ZDateTime.Empty);
			lines = new object[][]
						{
							new object[] {	"S0003",		0m,			10.2m,	0m,		0m,		10.2m	},
							new object[] {	"S0004",		-89m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0004",		0m,			89m,	0m,		0m,		0m	},
							new object[] {	"S0004",		89m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0004",		0m,			-89m,	0m,		0m,		0m	},
							new object[] {	"S0005",		-79m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			79m,	0m,		0m,		0m	},
							new object[] {	"S0005",		79m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			-79m,	0m,		0m,     0m  },
						};
			AssertDataTableAllRowsByKeyColumns("Filtering With outstanding WIP, without outstanding ACR", result, headers, lines, keyColumns);

			result = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, true, ZDateTime.Empty, ZDateTime.Empty);
			lines = new object[][]
						{
							new object[] {	"S0002",		-10m,		0m,		0m,		0m,     -10m  },
							new object[] {	"S0004",		-89m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0004",		0m,			89m,	0m,		0m,		0m	},
							new object[] {	"S0004",		89m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0004",		0m,			-89m,	0m,		0m,		0m	},
							new object[] {	"S0005",		-79m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			79m,	0m,		0m,		0m	},
							new object[] {	"S0005",		79m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			-79m,	0m,		0m,		0m	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering Without outstanding WIP, with outstanding ACR", result, headers, lines, keyColumns);

			result = RunScript(ZDateTime.Empty, ZDateTime.Empty, true, true, ZDateTime.Empty, ZDateTime.Empty);
			lines = new object[][]
						{
							new object[] {	"S0002",		-10m,		0m,		0m,		0m,     -10m  },
							new object[] {	"S0003",		0m,			10.2m,	0m,		0m,		10.20m	},
							new object[] {	"S0004",		-89m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0004",		0m,			89m,	0m,		0m,		0m	},
							new object[] {	"S0004",		89m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0004",		0m,			-89m,	0m,		0m,		0m	},
							new object[] {	"S0005",		-79m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			79m,	0m,		0m,		0m	},
							new object[] {	"S0005",		79m,		0m,		0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			-79m,	0m,		0m,		0m	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering With outstanding WIP, with outstanding ACR", result, headers, lines, keyColumns);
		}

		public void TestOutstandingWIPandACRFiltering_JobBranchManagementCode()
		{
			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = TestObjectCreator.NonCurrentBranch;

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			job1.JH_GB = branch1.PK;
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 10.0m, 0m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			job2.JH_GB = branch2.PK;
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 0m, 10.2m);

			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRA', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch1.GB_Code, branch1.GB_GC));
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRB', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch2.GB_Code, branch2.GB_GC));

			var headers = new[] { "JH_JobNum", "JH_BranchCode", "JH_Profit" };
			var keyColumns = new[] { "JH_JobNum", "JH_BranchCode" };
			var lines = new object[][]
						{
							new object[] { "S0001", branch1.GB_Code, -10M }
						};
			var result = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, ZDateTime.Empty, ZDateTime.Empty, "BRA");
			AssertDataTableAllRowsByKeyColumns("Should return job S0001 with branch1 and branch management code BRA", result, headers, lines, keyColumns);

			result = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, ZDateTime.Empty, ZDateTime.Empty, "BRB");
			lines = new object[][]
						{
							new object[] { "S0002", branch2.GB_Code, 10.2M }
						};
			AssertDataTableAllRowsByKeyColumns("Should return job S0002 with branch2 and branch management code BRB", result, headers, lines, keyColumns);
		}

		[TestDate(2012, 09, 24)]
		public void TestRevRecognitionFilter()
		{
			SetupDeclarationsShipmentsWithJobsRevRecognitionDates();
			Factory.Save();

			var jh_FromRevenueRecognizedDate = ZDateTime.Today.AddMonths(-1);
			var jh_ToRevenueRecognizedDate = ZDateTime.Today;

			var headers = new string[] { "AL_LineAmount", "WIPAmount", "REVAmount", "CSTAmount", "ACRAmount", "JH_JobNum" };
			var lines = new List<object[]>
				{
					new object[] { -1.00M, 0.00M, 0.00M, 0.00M, -1.00M, "S001001" },
					new object[] { 3.00M, 3.00M, 0.00M, 0.00M,  0.00M, "S001001" },
					new object[] { -2.00M, 0.00M, 0.00M, 0.00M, -2.00M, "S001001" },
					new object[] { 4.00M, 4.00M, 0.00M, 0.00M,  0.00M, "S001001" },
					new object[] { -2000.00M,    0.00M, 0.00M, 0.00M, -2000.00M, "B001002" },
					new object[] { -1000.00M,    0.00M, 0.00M, 0.00M, -1000.00M, "B001002" },
					new object[] { 4000.00M, 4000.00M, 0.00M, 0.00M,     0.00M, "B001002" },
					new object[] { 3000.00M, 3000.00M, 0.00M, 0.00M,     0.00M, "B001002" }
				};
			var dataTable = RunScript(jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate);
			AssertDataTableAllRowsByKeyColumns("Case 1", dataTable, headers, lines.ToArray());

			Action<string, decimal> assertJobProfit = (jobNumber, profit ) =>
			{
				var jobProfits = (from row in dataTable.AsEnumerable()
								 where row.Field<string>("JH_JobNum") == jobNumber && row.Field<decimal?>("JH_Profit") != null
								 select row.Field<decimal>("JH_Profit")).ToArray();
				AssertEquals(jobNumber + ": number of records with caclucated profit", 1, jobProfits.Length);
				AssertEquals(jobNumber + ": profit amount", profit, jobProfits.First());
			};
			assertJobProfit("S001001", 4);
			assertJobProfit("B001002", 4000);

			lines.AddRange(new object[][]
			{
					new object[] { -20.00M,  0.00M, 0.00M, 0.00M, -20.00M, "S001002" },
					new object[] { -10.00M,  0.00M, 0.00M, 0.00M, -10.00M, "S001002" },
					new object[] { 40.00M, 40.00M, 0.00M, 0.00M,   0.00M, "S001002" },
					new object[] { 30.00M, 30.00M, 0.00M, 0.00M,   0.00M, "S001002" },
					new object[] { -200.00M,   0.00M, 0.00M, 0.00M, -200.00M, "B001001" },
					new object[] { -100.00M,   0.00M, 0.00M, 0.00M, -100.00M, "B001001" },
					new object[] { 400.00M, 400.00M, 0.00M, 0.00M,    0.00M, "B001001" },
					new object[] { 300.00M, 300.00M, 0.00M, 0.00M,    0.00M, "B001001" },
			});
			dataTable = RunScript();
			AssertDataTableAllRowsByKeyColumns("Case 2", dataTable, headers, lines.ToArray());
			assertJobProfit("S001002", 40);
			assertJobProfit("B001001", 400);
		}

		public void TestReport_AllJobProfitDetailTestWithLegacyGwConsolAndNewGCN()
		{
			Func<string, string, string> getSql = (selectJobTypeDetails, jobType) =>
$@"EXEC Report_AllJobProfitDetail 
	@SelectJobTypeDetails = '{selectJobTypeDetails}',
	@JobType = '{jobType}',
	@JH_GC = '{GlbCompany.CurrentCompany.PK}',
	@JH_Status = NULL,
	@JH_BranchPKList = '',
	@JH_departmentpkList = '',
	@JH_SalesRepCode = NULL,
	@JH_OperatorCode = NULL,
	@JH_FromCreatedDate = '',
	@JH_ToCreatedDate = '',
	@JH_FromClosedDate = '',
	@JH_ToClosedDate = '',
	@JH_FromRevenueRecognizedDate = '1900-01-01 00:00:00',
	@JH_ToRevenueRecognizedDate = '2079-06-06 23:59:29',
	@JH_LocalClientPKList = '',
	@AC_ChargeGroup = NULL,
	@AL_BranchPKList = '',
	@AL_departmentpkList = '',
	@AL_ChargeCodePKList = '',
	@AL_ChargeCodePKNOTINList = '',
	@AL_CreditorPKList = '',
	@AL_DebtorPKList = '',
	@AL_FromDate = '1900-01-01 00:00:00',
	@AL_ToDate = '2079-06-06 23:59:29',
	@CurrentCountry = 'AU',
	@IsCommissionable = '',
	@AL_OutstandingWIPOnly = 'N',
	@AL_OutstandingACROnly = 'N',
	@AC_AR_SalesGroup = NULL,
	@AC_AR_ExpenseGroup = NULL,
	@ProfitLossReasonCode= NULL,
	@CFS_JobType = '',
	@JobsIncluded = ''";

			var jobs = new List<Job>();
			var legacyGwConsol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var gCNConsol = TestObjectCreator.CreateGatewayConsol("AUSYD", "NZAKL", "C002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "NZAKL", saveIt: true);
			shipment.JS_IsCFSRegistered = true;
			jobs.Add(TestObjectCreator.CreateJobForLegacyGateway(legacyGwConsol));
			jobs.Add(TestObjectCreator.CreateJob(gCNConsol, false));
			jobs.Add(TestObjectCreator.CreateJob(shipment, false));
			jobs.ForEach(x => TestObjectCreator.CreateCharge(x, TestObjectCreator.CC1, osCostAmt: 10M, creditor: TestObjectCreator.AALSHI, osSellAmt: 35M, debtor: TestObjectCreator.ABIGAS));
			Factory.Save();

			AssertEquals(true, legacyGwConsol.IsLegacyGateway);
			AssertEquals(true, gCNConsol.IsGatewayConsol);

			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, getSql("N", "L"));
			AssertEquals("legacyGwConsol and GCNConsol should not be selected", 2, dataTable.Rows.Count);

			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, getSql("N", "A"));
			AssertEquals("legacyGwConsol and GCNConsol should be selected", 6, dataTable.Rows.Count);

			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, getSql("Y", "L"));
			//this line won't fail even without production code changes. Inner join with csfn_ConsolsForTransportCriteria just prevent selection of any consol here.
			//To make it fail consol data setup should be improved to be selected by csfn_ConsolsForTransportCriteria used in SQL.
			AssertEquals("legacyGwConsol and GCNConsol should not be selected", 2, dataTable.Rows.Count);

			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, getSql("Y", "A"));
			AssertEquals("legacyGwConsol and GCNConsol should be selected", 6, dataTable.Rows.Count);
		}

		public void TestReport_AllJobProfitDetailTestJobInactive()
		{
			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = TestObjectCreator.NonCurrentBranch;

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			job1.JH_GB = branch1.PK;
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 10.0m, 0m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			job2.JH_GB = branch2.PK;

			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 0m, 10.2m);

			Factory.Save();

			string deActivateJobSql = $@"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = '{job2.PK}'";
			DataUtils.GetDataTableFromQuery(Db.Connection, deActivateJobSql);

			var headers = new[] { "JH_JobNum", "JH_BranchCode", "JH_Profit" };
			var keyColumns = new[] { "JH_JobNum", "JH_BranchCode" };
			var lines = new object[][]
						{
							new object[] { "S0001", branch1.GB_Code, -10M },
							new object[] { "S0002", branch2.GB_Code, 10.2M }
						};
			var result = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, ZDateTime.Empty, ZDateTime.Empty);
			AssertDataTableAllRowsByKeyColumns("Should return job S0002 with inactive jobheader", result, headers, lines, keyColumns);
		}

		[TestDate(2022, 1, 2, 3, 4, 0)]
		public void TestReversedWIPandACR()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 100m, 100m);

			charge1.JR_OSCostAmt = 10m;
			charge1.JR_OSSellAmt = 10m;

			Factory.Save();
			charge1.Accrual.AL_PostDate = new ZDateTime(2022, 10, 01);
			charge1.WIP.AL_PostDate = new ZDateTime(2022, 10, 02);
			charge1.ReverseAccrual(new ZDateTime(2022, 10, 03));
			charge1.ReverseWIP(new ZDateTime(2022, 10, 04));

			Factory.Save();

			var headers = new[] { "JH_JobNum", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount", "JH_Profit" };
			var keyColumns = new[] { "JH_JobNum", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };
			var result = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, new ZDateTime(2022, 10, 01), new ZDateTime(2022, 10, 05));
			var result1 = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, new ZDateTime(2022, 10, 01), new ZDateTime(2022, 10, 02));
			var result2 = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, new ZDateTime(2022, 10, 02), new ZDateTime(2022, 10, 03));
			var result3 = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, new ZDateTime(2022, 10, 03), new ZDateTime(2022, 10, 04));
			var result4 = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, new ZDateTime(2022, 10, 04), new ZDateTime(2022, 10, 05));

			var lines = new object[][]
						{
							new object[] {  "S0001",        -10m,       0m,     0m,     0m,     0m    },
							new object[] {  "S0001",        10m,       0m,     0m,     0m,     0m    },
							new object[] {  "S0001",        0m,       10m,     0m,     0m,     0m    },
							new object[] {  "S0001",        0m,       -10m,     0m,     0m,     0m    },
						};

			var line1 = new object[][]
						{
							new object[] {  "S0001",        -10m,       0m,     0m,     0m,     -10m    },
						};
			var line2 = new object[][]
						{
							new object[] {  "S0001",        0m,       10m,     0m,     0m,     10m    },
						};
			var line3 = new object[][]
						{
							new object[] {  "S0001",       10m,       0m,     0m,     0m,     10m    },
						};
			var line4 = new object[][]
						{
							new object[] {  "S0001",        0m,     -10m,     0m,     0m,     -10m    },
						};

			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR1", result, headers, lines, keyColumns);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR2", result1, headers, line1, keyColumns);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR3", result2, headers, line2, keyColumns);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR4", result3, headers, line3, keyColumns);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR5", result4, headers, line4, keyColumns);
		}

		DataTable RunScript()
		{
			return RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, ZDateTime.Empty, ZDateTime.Empty);
		}

		DataTable RunScript(ZDateTime jh_FromRevenueRecognizedDate, ZDateTime jh_ToRevenueRecognizedDate)
		{
			return RunScript(jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate, false, false, ZDateTime.Empty, ZDateTime.Empty);
		}

		DataTable RunScript(ZDateTime jh_FromRevenueRecognizedDate, ZDateTime jh_ToRevenueRecognizedDate, bool outstandingWIP, bool outstandingACR, ZDateTime fromPostDate, ZDateTime toPostDate,
			string jobBranchManagementCode = "", string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			fromPostDate = fromPostDate.IsEmpty ? ZDateTime.MinSmallDateTimeValue : fromPostDate;
			toPostDate = toPostDate.IsEmpty ? ZDateTime.MaxSmallDateTimeValue : toPostDate;
			string sql = string.Format(@"
EXEC Report_AllJobProfitDetail 
	@JobType = '',
	@JH_GC = '{0}',
	@JH_Status = NULL,
	@JH_BranchPKList = '',
	@JH_departmentpkList = '',
	@JH_SalesRepCode = NULL,
	@JH_OperatorCode = NULL,
	@JH_FromCreatedDate = '',
	@JH_ToCreatedDate = '',
	@JH_FromClosedDate = '',
	@JH_ToClosedDate = '',
	@JH_FromRevenueRecognizedDate = '{1}',
	@JH_ToRevenueRecognizedDate = '{2}',
	@JH_LocalClientPKList = '',
	@AC_ChargeGroup = NULL,
	@AL_BranchPKList = '',
	@AL_departmentpkList = '',
	@AL_ChargeCodePKList = '',
	@AL_ChargeCodePKNOTINList = '',
	@AL_CreditorPKList = '',
	@AL_DebtorPKList = '',
	@AL_FromDate = '{3}',
	@AL_ToDate = '{4}',
	@CurrentCountry = 'AU',
	@IsCommissionable = '',
	@AL_OutstandingWIPOnly = '{5}',
	@AL_OutstandingACROnly = '{6}',
	@AC_AR_SalesGroup = NULL,
	@AC_AR_ExpenseGroup = NULL,
	@ProfitLossReasonCode= NULL,
	@CFS_JobType = '',
	@BranchManagementCode = '{7}',
	@JH_IsActive = '{8}',
	@NotIncludeReversedWIPACR = '{9}'
",
						GlbCompany.CurrentCompany.PK,							//@JH_GC
						GetMinDateTimeString(jh_FromRevenueRecognizedDate),		//@JH_FromRevenueRecognizedDate
						GetMaxDateTimeString(jh_ToRevenueRecognizedDate),       //@JH_ToRevenueRecognizedDate
						GetMinDateTimeString(fromPostDate),     //@AL_FromDate
						GetMaxDateTimeString(toPostDate),       //@AL_ToDate
						outstandingWIP ? "Y" : "",								//@AL_OutstandingWIPOnly
						outstandingACR ? "Y" : "",								//@AL_OutstandingACROnly
						jobBranchManagementCode,
						activeStatus,                                           //@JH_IsActive
						notIncludeReversedWIPACR								//@NotIncludeReversedWIPACR
						);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}

