
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_JobProfitTest : ScriptTest
	{
		public void TestOutstandingWIPandACRFiltering()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			var shipment4 = TestObjectCreator.CreateShipment("S0004");
			var shipment5 = TestObjectCreator.CreateShipment("S0005");

			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			var job3 = TestObjectCreator.CreateJob(shipment3, false);
			var job4 = TestObjectCreator.CreateJob(shipment4, false);
			var job5 = TestObjectCreator.CreateJob(shipment5, false);

			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 100m, 100m);
			charge1.JR_OSCostAmt = 0m;
			charge1.JR_OSSellAmt = 0m;

			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 10.0m, 0m);
			TestObjectCreator.CreateCharge(job3, TestObjectCreator.CC1, 0m, 10.2m);
			TestObjectCreator.CreateCharge(job4, TestObjectCreator.CC1, 89m, 89m);
			TestObjectCreator.CreateCharge(job4, TestObjectCreator.CC2, -89m, -89m);
			TestObjectCreator.CreateCharge(job5, TestObjectCreator.CC1, 79m, 79m);
			TestObjectCreator.CreateCharge(job5, TestObjectCreator.CC1, -79m, -79m);

			Factory.Save();

			var headers = new[] { "JH_JobNum", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };
			var result1 = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false);
			var lines1 = new object[][]
						{
							new object[] {	"S0002",		-10m,		0m,		0m,		0m	},
							new object[] {	"S0003",		0m,			10.2m,	0m,		0m	},
							new object[] {	"S0004",		0m,			0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			0m,		0m,		0m	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering Without outstanding WIP, without outstanding ACR", result1, headers, lines1, headers);

			var result2 = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, true, false);
			var lines2 = new object[][]
						{
							new object[] {	"S0003",		0m,			10.2m,	0m,		0m	},
							new object[] {	"S0004",		0m,			0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			0m,		0m,		0m	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering With outstanding WIP, without outstanding ACR", result2, headers, lines2, headers);

			var result3 = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, true);
			var lines3 = new object[][]
						{
							new object[] {	"S0002",		-10m,		0m,		0m,		0m	},
							new object[] {	"S0004",		0m,			0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			0m,		0m,		0m	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering Without outstanding WIP, with outstanding ACR", result3, headers, lines3, headers);

			var result4 = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, true, true);
			var lines4 = new object[][]
						{
							new object[] {	"S0002",		-10m,		0m,		0m,		0m	},
							new object[] {	"S0003",		0m,			10.2m,	0m,		0m	},
							new object[] {	"S0004",		0m,			0m,		0m,		0m	},
							new object[] {	"S0005",		0m,			0m,		0m,		0m	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering With outstanding WIP, with outstanding ACR", result4, headers, lines4, headers);
		}

		public void TestCorrectWIPPostingDates()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var wip1 = TestObjectCreator.CreateWIP(job, c => PopulateJobCharge(c));
			var wip2 = TestObjectCreator.CreateWIP(job, c => PopulateJobCharge(c));
			var wip3 = TestObjectCreator.CreateWIP(job, c => PopulateJobCharge(c));

			Factory.Save();

			wip1.Reverse(false);
			wip2.Reverse(false);
			wip3.Reverse(false);

			wip1.AL_PostDate = new ZDateTime(2000, 1, 1);
			wip2.AL_PostDate = new ZDateTime(2000, 1, 2);
			wip3.AL_PostDate = new ZDateTime(2000, 1, 3);

			wip1.AL_ReverseDate = new ZDateTime(2010, 1, 1);
			wip2.AL_ReverseDate = new ZDateTime(2010, 1, 2);
			wip3.AL_ReverseDate = new ZDateTime(2010, 1, 3);

			Factory.Save();

			var al_FromDate = new ZDateTime(1999, 1, 1);
			var al_ToDate = new ZDateTime(2011, 1, 1);

			var headers = new[] { "AL_PostDate", "AL_LineAmount" };
			var result = RunScript(true, false, false, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, al_FromDate, al_ToDate, "All");
			var lines = new object[][]
						{
							new object[] { new DateTime(2000, 1, 1), new System.Decimal(350.0000) },
							new object[] { new DateTime(2000, 1, 2), new System.Decimal(350.0000) },
							new object[] { new DateTime(2000, 1, 3), new System.Decimal(350.0000) },
							new object[] { new DateTime(2010, 1, 1), new System.Decimal(-350.0000) },
							new object[] { new DateTime(2010, 1, 2), new System.Decimal(-350.0000) },
							new object[] { new DateTime(2010, 1, 3), new System.Decimal(-350.0000) }
						};
			AssertDataTableSelectedRows("Incorrect AL_PostDates for WIPs", result, headers, lines, false);
		}

		public void TestCorrectAccrualPostingDates()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var accrual1 = TestObjectCreator.CreateAccrual(job, c => PopulateJobCharge(c));
			var accrual2 = TestObjectCreator.CreateAccrual(job, c => PopulateJobCharge(c));
			var accrual3 = TestObjectCreator.CreateAccrual(job, c => PopulateJobCharge(c));

			Factory.Save();

			accrual1.Reverse(false);
			accrual2.Reverse(false);
			accrual3.Reverse(false);

			accrual1.AL_PostDate = new ZDateTime(2000, 1, 1);
			accrual2.AL_PostDate = new ZDateTime(2000, 1, 2);
			accrual3.AL_PostDate = new ZDateTime(2000, 1, 3);

			accrual1.AL_ReverseDate = new ZDateTime(2010, 1, 1);
			accrual2.AL_ReverseDate = new ZDateTime(2010, 1, 2);
			accrual3.AL_ReverseDate = new ZDateTime(2010, 1, 3);

			Factory.Save();

			var al_FromDate = new ZDateTime(1999, 1, 1);
			var al_ToDate = new ZDateTime(2011, 1, 1);

			var headers = new[] { "AL_PostDate" };
			var result = RunScript(true, false, false, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, al_FromDate, al_ToDate, "All");
			var lines = new object[][]
						{
							new object[] { new DateTime(2000, 1, 1) },
							new object[] { new DateTime(2000, 1, 2) },
							new object[] { new DateTime(2000, 1, 3) },
							new object[] { new DateTime(2010, 1, 1) },
							new object[] { new DateTime(2010, 1, 2) },
							new object[] { new DateTime(2010, 1, 3) }
						};
			AssertDataTableSelectedRows("Incorrect AL_PostDates for ACRs", result, headers, lines, false);
		}

		public void TestCorrectWIPPostingDatesWithTransactionRecognizedDateFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var shipment2 = TestObjectCreator.CreateShipment("S0002");

			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);

			var wip1 = TestObjectCreator.CreateWIP(job1, c => PopulateJobCharge(c));
			var wip2 = TestObjectCreator.CreateWIP(job2, c => PopulateJobCharge(c));

			var revRecog1 = TestObjectCreator.CreateJobChargeRevRecognition(job1, RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, new ZDateTime(2000, 1, 1));
			var revRecog2 = TestObjectCreator.CreateJobChargeRevRecognition(job2, RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, new ZDateTime(2010, 1, 1));

			Factory.Save();

			wip1.Reverse(false);
			wip1.AL_PostDate = new ZDateTime(2000, 1, 1);
			wip1.AL_ReverseDate = new ZDateTime(2000, 1, 2);

			wip2.Reverse(false);
			wip2.AL_PostDate = new ZDateTime(2010, 1, 1);
			wip2.AL_ReverseDate = new ZDateTime(2010, 1, 2);

			Factory.Save();

			var headers = new[] { "AL_PostDate", "AL_LineAmount" };

			var al_FromDate = new ZDateTime(1999, 1, 1);
			var al_ToDate = new ZDateTime(2011, 1, 1);

			var jh_FromRevenueRecognizedDate1 = new ZDateTime(1999, 1, 1);
			var jh_ToRevenueRecognizedDate1 = new ZDateTime(2001, 1, 1);

			var result1 = RunScript(true, false, false, string.Empty, ZDateTime.Empty, jh_FromRevenueRecognizedDate1, jh_ToRevenueRecognizedDate1, false, false, al_FromDate, al_ToDate, "All");
			var lines1 = new object[][]
						{
							new object[] { new DateTime(2000, 1, 1), new System.Decimal(350.0000) },
							new object[] { new DateTime(2000, 1, 2), new System.Decimal(-350.0000) },
						};
			AssertDataTableSelectedRows("Incorrect AL_PostDates for WIPs", result1, headers, lines1, false);

			var jh_FromRevenueRecognizedDate2 = new ZDateTime(2009, 1, 1);
			var jh_ToRevenueRecognizedDate2 = new ZDateTime(2011, 1, 1);

			var result2 = RunScript(true, false, false, string.Empty, ZDateTime.Empty, jh_FromRevenueRecognizedDate2, jh_ToRevenueRecognizedDate2, false, false, al_FromDate, al_ToDate, "All");
			var lines2 = new object[][]
						{
							new object[] { new DateTime(2010, 1, 1), new System.Decimal(350.0000) },
							new object[] { new DateTime(2010, 1, 2), new System.Decimal(-350.0000) },
						};
			AssertDataTableSelectedRows("Incorrect AL_PostDates for WIPs", result2, headers, lines2, false);
		}

		public void TestCorrectAccrualPostingDatesWithTransactionRecognizedDateFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var shipment2 = TestObjectCreator.CreateShipment("S0002");

			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);

			var accrual1 = TestObjectCreator.CreateWIP(job1, c => PopulateJobCharge(c));
			var accrual2 = TestObjectCreator.CreateWIP(job2, c => PopulateJobCharge(c));

			var revRecog1 = TestObjectCreator.CreateJobChargeRevRecognition(job1, RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, new ZDateTime(2000, 1, 1));
			var revRecog2 = TestObjectCreator.CreateJobChargeRevRecognition(job2, RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, new ZDateTime(2010, 1, 1));

			Factory.Save();

			accrual1.Reverse(false);
			accrual1.AL_PostDate = new ZDateTime(2000, 1, 1);
			accrual1.AL_ReverseDate = new ZDateTime(2000, 1, 2);

			accrual2.Reverse(false);
			accrual2.AL_PostDate = new ZDateTime(2010, 1, 1);
			accrual2.AL_ReverseDate = new ZDateTime(2010, 1, 2);

			Factory.Save();

			var headers = new[] { "AL_PostDate" };

			var al_FromDate = new ZDateTime(1999, 1, 1);
			var al_ToDate = new ZDateTime(2011, 1, 1);

			var jh_FromRevenueRecognizedDate1 = new ZDateTime(1999, 1, 1);
			var jh_ToRevenueRecognizedDate1 = new ZDateTime(2001, 1, 1);

			var result1 = RunScript(true, false, false, string.Empty, ZDateTime.Empty, jh_FromRevenueRecognizedDate1, jh_ToRevenueRecognizedDate1, false, false, al_FromDate, al_ToDate, "All");
			var lines1 = new object[][]
						{
							new object[] { new DateTime(2000, 1, 1) },
							new object[] { new DateTime(2000, 1, 2) },
						};
			AssertDataTableSelectedRows("Incorrect AL_PostDates for ACRs", result1, headers, lines1, false);

			var jh_FromRevenueRecognizedDate2 = new ZDateTime(2009, 1, 1);
			var jh_ToRevenueRecognizedDate2 = new ZDateTime(2011, 1, 1);

			var result2 = RunScript(true, false, false, string.Empty, ZDateTime.Empty, jh_FromRevenueRecognizedDate2, jh_ToRevenueRecognizedDate2, false, false, al_FromDate, al_ToDate, "All");
			var lines2 = new object[][]
						{
							new object[] { new DateTime(2010, 1, 1) },
							new object[] { new DateTime(2010, 1, 2) },
						};
			AssertDataTableSelectedRows("Incorrect AL_PostDates for ACRs", result2, headers, lines2, false);
		}

		[ExpectNoExceptions]
		public void TestAllLogicBranchesGeneratesExecutableSQL()
		{
			ZDateTime jk_JX_FromETD = ZDateTime.Today.AddMonths(-1);

			RunScript(true, false, true, "", jk_JX_FromETD);
			RunScript(true, false, true, "S");
			RunScript(true, false, true, "B");
			RunScript(true, false, true, "");
			RunScript(true, true, true, "");
			RunScript(false, true, true, "");
			RunScript(false, false, false, "", jk_JX_FromETD);
			RunScript(true, false, false, "", jk_JX_FromETD);
			RunScript(false, false, false, "S");
			RunScript(true, false, false, "S");
			RunScript(false, false, false, "B");
			RunScript(true, false, false, "B");
			RunScript(false, false, false, "");
			RunScript(true, false, false, "");
			RunScript(true, true, false, "");
		}

		[TestDate(2012, 09, 24)]
		public void TestRevRecognitionFilter()
		{
			SetupDeclarationsShipmentsWithJobsRevRecognitionDates();
			Factory.Save();

			ZDateTime jk_JX_FromETD = ZDateTime.Today.AddMonths(-1);
			ZDateTime jh_FromRevenueRecognizedDate = ZDateTime.Today.AddMonths(-1);
			ZDateTime jh_ToRevenueRecognizedDate = ZDateTime.Today;

			AssertRevRecognitionFilterTestResults("Case 01", () => RunScript(true, false, true, "", jk_JX_FromETD), true, true, false, false);
			AssertRevRecognitionFilterTestResults("Case 02", () => RunScript(true, false, true, "", jk_JX_FromETD, jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, false);
			AssertRevRecognitionFilterTestResults("Case 03", () => RunScript(true, false, true, "S"), true, true, false, false);
			AssertRevRecognitionFilterTestResults("Case 04", () => RunScript(true, false, true, "S", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, false);
			AssertRevRecognitionFilterTestResults("Case 05", () => RunScript(true, false, true, "B"), false, false, true, true);
			AssertRevRecognitionFilterTestResults("Case 06", () => RunScript(true, false, true, "B", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), false, false, false, true);
			AssertRevRecognitionFilterTestResults("Case 07", () => RunScript(true, false, true, ""), true, true, true, true);
			AssertRevRecognitionFilterTestResults("Case 08", () => RunScript(true, false, true, "", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, true);
			AssertRevRecognitionFilterTestResults("Case 09", () => RunScript(true, true, true, ""), true, true, false, false, true);
			AssertRevRecognitionFilterTestResults("Case 10", () => RunScript(true, true, true, "", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, false, true);
			AssertRevRecognitionFilterTestResults("Case 11", () => RunScript(false, true, true, ""), true, true, false, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 12", () => RunScript(false, true, true, "", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 13", () => RunScript(false, false, false, "", jk_JX_FromETD), true, true, false, false, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 14", () => RunScript(false, false, false, "", jk_JX_FromETD, jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, false, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 15", () => RunScript(true, false, false, "", jk_JX_FromETD), true, true, false, false, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 16", () => RunScript(true, false, false, "", jk_JX_FromETD, jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, false, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 17", () => RunScript(false, false, false, "S"), true, true, false, false, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 18", () => RunScript(false, false, false, "S", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, false, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 19", () => RunScript(true, false, false, "S"), true, true, false, false, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 20", () => RunScript(true, false, false, "S", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, false, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 21", () => RunScript(false, false, false, "B"), false, false, true, true, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 22", () => RunScript(false, false, false, "B", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), false, false, false, true, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 23", () => RunScript(true, false, false, "B"), false, false, true, true, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 24", () => RunScript(true, false, false, "B", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), false, false, false, true, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 25", () => RunScript(false, false, false, ""), true, true, true, true, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 26", () => RunScript(false, false, false, "", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, true, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 27", () => RunScript(true, false, false, ""), true, true, true, true, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 28", () => RunScript(true, false, false, "", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, true, false, true, false);
			AssertRevRecognitionFilterTestResults("Case 29", () => RunScript(true, true, false, ""), true, true, false, false, true, true, false);
			AssertRevRecognitionFilterTestResults("Case 30", () => RunScript(true, true, false, "", jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate), true, false, false, false, true, true, false);
		}

		[TestDate(2012, 09, 24)]
		public void TestJobInactive()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 0m, 10.2m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 10.0m, 0m);

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

			var headers = new[] { "JH_JobNum", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };
			var result = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false);
			var lines =
				new object[][]
				{
					new object[] {  "S0001",        0m,         10.2m,  0m,     0m  },
					new object[] {  "S0002",        -10.0m,         0m,  0m,     0m  },
				};

			AssertDataTableAllRowsByKeyColumns("Should have data with inactive job", result, headers, lines, headers);
		}

		[TestDate(2012, 09, 24)]
		public void TestGetActiveInactiveAndAllJobReport()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 0m, 10.2m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 10.0m, 0m);
			Factory.Save();

			string deActivateJobSql = $@" UPDATE dbo.JobHeader SET JH_IsActive = 0, JH_SystemLastEditTimeUtc = GETUTCDATE(), JH_SystemLastEditUser = '~BP' WHERE JH_PK = '{job2.PK}'";
			DataUtils.GetDataTableFromQuery(Db.Connection, deActivateJobSql);

			var headers = new[] { "JH_JobNum", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };

			// Active Job
			var result = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, "Active");
			var lines = new object[][] { new object[] {  "S0001",        0m,         10.2m,  0m,     0m  }, };

			AssertDataTableAllRowsByKeyColumns("Should have data with active job", result, headers, lines, headers);

			// Inactive Job
			result = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, "Inactive");
			lines = new object[][] { new object[] {  "S0002",        -10.0m,         0m,  0m,     0m  }, };

			AssertDataTableAllRowsByKeyColumns("Should have data with inactive job", result, headers, lines, headers);

			// All Job
			result = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, "All");
			lines = new object[][] { new object[] {  "S0001",        0m,         10.2m,  0m,     0m  }, new object[] {  "S0002",        -10.0m,         0m,  0m,     0m  }, };

			AssertDataTableAllRowsByKeyColumns("Should have data with all job", result, headers, lines, headers);
		}

		[TestDate(2012, 09, 24)]
		public void TestNoPassingParamExcludeReversedWIPACR()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 10m, 20m);
			Factory.Save();

			charge1.ReverseAccrual(ZDateTime.Today);
			charge1.ReverseWIP(ZDateTime.Today);
			Factory.Save();

			var headers = new[] { "JH_JobNum", "ACRAmount", "WIPAmount" };
			var keyColumns = new[] { "JH_JobNum", "ACRAmount", "WIPAmount" };
			object[][] lines =
			[
				["S0001", -10m, 0m],
				["S0001", 10m, 0m],
				["S0001", -10m, 0m],
				["S0001", 0m, 20m],
				["S0001", 0m, -20m],
				["S0001", 0m, 20m],
			];

			var result = RunScript(true, false, false, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, ZDateTime.Today, ZDateTime.Today.AddDays(1), ZString.Empty, null);
			AssertDataTableAllRowsByKeyColumns("Should contain reversed lines when not passing the parameter AL_ExcludeReversedWIPACR", result, headers, lines, keyColumns);
		}

		[TestDate(2022, 1, 2, 3, 4, 0)]
		public void TestExcludeReversedWIPAndACRFiltering()
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

			var headers = new[] { "JH_JobNum", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };
			var keyColumns = new[] { "JH_JobNum", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };

			var result =  RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, new ZDateTime(2022, 10, 01), new ZDateTime(2022, 10, 05), "All", "Y");
			var result1 = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, new ZDateTime(2022, 10, 01), new ZDateTime(2022, 10, 02), "All", "Y");
			var result2 = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, new ZDateTime(2022, 10, 02), new ZDateTime(2022, 10, 03), "All", "Y");
			var result3 = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, new ZDateTime(2022, 10, 03), new ZDateTime(2022, 10, 04), "All", "Y");
			var result4 = RunScript(true, false, true, string.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, new ZDateTime(2022, 10, 04), new ZDateTime(2022, 10, 05), "All", "Y");

			var line1 = new object[][]
						{
							new object[] {  "S0001",        -10m,       0m,     0m,     0m },
						};
			var line2 = new object[][]
						{
							new object[] {  "S0001",        0m,       10m,     0m,     0m },
						};
			var line3 = new object[][]
						{
							new object[] {  "S0001",       10m,       0m,     0m,     0m  },
						};
			var line4 = new object[][]
						{
							new object[] {  "S0001",        0m,       -10m,     0m,     0m },
						};

			AssertEquals("Filtering reversed WIP and ACR1", 0, result.Rows.Count);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR2", result1, headers, line1, keyColumns);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR3", result2, headers, line2, keyColumns);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR4", result3, headers, line3, keyColumns);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR5", result4, headers, line4, keyColumns);
		}

		[TestDate(2012, 09, 24)]
		public void TestProfitWithReversedWIPAndACR()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 10m, 0m);
			var charge2 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC3, 40m, 60m);
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC4, 90m, 120m);

			Factory.Save();

			charge1.ReverseAccrual(ZDateTime.Today);

			charge2.ReverseWIP(ZDateTime.Today);
			charge2.ReverseAccrual(ZDateTime.Today);

			Factory.Save();

			var headers = new[] { "JH_JobNum", "AC_Code", "JH_Profit" };
			object[][] lines =
			[
				["S0001", TestObjectCreator.CC1.AC_Code, DBNull.Value],
				["S0001", TestObjectCreator.CC1.AC_Code, DBNull.Value],
				["S0001", TestObjectCreator.CC1.AC_Code, -10m],
				["S0001", TestObjectCreator.CC3.AC_Code, DBNull.Value],
				["S0001", TestObjectCreator.CC3.AC_Code, DBNull.Value],
				["S0001", TestObjectCreator.CC3.AC_Code, DBNull.Value],
				["S0001", TestObjectCreator.CC3.AC_Code, DBNull.Value],
				["S0001", TestObjectCreator.CC3.AC_Code, DBNull.Value],
				["S0001", TestObjectCreator.CC3.AC_Code, 20m],
				["S0001", TestObjectCreator.CC4.AC_Code, DBNull.Value],
				["S0001", TestObjectCreator.CC4.AC_Code, 30m],
			];

			var result = RunScript(true, false, false, ZString.Empty);

			AssertDataTableAllRows("The row has JH_Profit value should be in the last of the group with same JH_JobNum and AC_Code", result, headers, lines);
		}

		#region Helper Methods

		void AssertRevRecognitionFilterTestResults(string message, Func<DataTable> runScript, bool job1Included, bool job2Included, bool job3Included, bool job4Included, bool includeConsolDetailes = false, bool includeJobDetailes = true, bool groupByJob = true)
		{
			var headers = new List<object>();
			string columnJH_JobNum = "JH_JobNum";
			string columnJK_UniqueConsignRef = "JK_UniqueConsignRef";
			string columnAL_LineAmount = "AL_LineAmount";
			var jobFields = new object[] { columnJH_JobNum, "FW_Consols", "RecognitionDateList" };
			var amountRows = new object[][] { new object[] { columnAL_LineAmount, "WIPAmount", "REVAmount", "CSTAmount", "ACRAmount" } };
			var keyColumns = new List<string> { columnAL_LineAmount };
			if (includeJobDetailes)
			{
				keyColumns.Add(columnJH_JobNum);
			}
			if (includeConsolDetailes)
			{
				keyColumns.Add(columnJK_UniqueConsignRef);
			}
			Action<string, List<object[]>> generateRows = (consolData, rows) =>
			{
				foreach (var amountFields in amountRows)
				{
					var fieldList = new List<object>();
					if (includeConsolDetailes)
					{
						fieldList.Add(consolData);
					}
					if (includeJobDetailes)
					{
						fieldList.AddRange(jobFields);
					}
					fieldList.AddRange(amountFields);
					rows.Add(fieldList.ToArray());
				}
			};
			var headerRows = new List<object[]>();
			generateRows(columnJK_UniqueConsignRef, headerRows);
			headers = new List<object>(headerRows[0]);

			var detailedAmountFields = Array.Empty<object[]>();
			var lines = new List<object[]>();

			Action<string[]> generateLines = consolNumbers =>
			{
				var amountGroupBy = from row in detailedAmountFields
														group row by 1 into g
														select new object[]
									{
										g.Sum(x => (decimal)x[0]),
										g.Sum(x => (decimal)x[1]),
										g.Sum(x => (decimal)x[2]),
										g.Sum(x => (decimal)x[3]),
										g.Sum(x => (decimal)x[4])
									};
				amountRows = groupByJob ? amountGroupBy.ToArray() : detailedAmountFields;
				foreach (var consolNumber in consolNumbers)
				{
					generateRows(consolNumber, lines);
				}
			};

			if (job1Included)
			{
				jobFields = new object[] { "S001001", "C001001, C001002", "ARV 24-Aug-12, DEP 24-Jul-12, IMM" };
				detailedAmountFields = new object[][]
					{
						new object[] { -1.00M, 0.00M, 0.00M, 0.00M, -1.00M },
						new object[] { 3.00M, 3.00M, 0.00M, 0.00M,  0.00M },
						new object[] { -2.00M, 0.00M, 0.00M, 0.00M, -2.00M },
						new object[] { 4.00M, 4.00M, 0.00M, 0.00M,  0.00M }
					};

				generateLines(includeConsolDetailes ? new[] { "C001001", "C001002" } : new[] { "" });
			}
			if (job2Included)
			{
				jobFields = new object[] { "S001002", "C001003", "DEL 24-Apr-12, IMM, PIC 24-Mar-12" };
				detailedAmountFields = new object[][]
					{
						new object[] { -20.00M,  0.00M, 0.00M, 0.00M, -20.00M },
						new object[] { -10.00M,  0.00M, 0.00M, 0.00M, -10.00M },
						new object[] { 40.00M, 40.00M, 0.00M, 0.00M,   0.00M },
						new object[] { 30.00M, 30.00M, 0.00M, 0.00M,   0.00M }
					};

				generateLines(new[] { "C001003" });
			}
			if (job3Included)
			{
				jobFields = new object[] { "B001001", null, "CUS 24-Sep-12, IMM, JOP 24-Jun-12" };
				detailedAmountFields = new object[][]
					{
						new object[] { -200.00M,   0.00M, 0.00M, 0.00M, -200.00M },
						new object[] { -100.00M,   0.00M, 0.00M, 0.00M, -100.00M },
						new object[] { 400.00M, 400.00M, 0.00M, 0.00M,    0.00M },
						new object[] { 300.00M, 300.00M, 0.00M, 0.00M,    0.00M }
					};

				generateLines(new[] { "" });
			}
			if (job4Included)
			{
				jobFields = new object[] { "B001002", null, "CUS 23-Sep-12, IMM" };
				detailedAmountFields = new object[][]
					{
						new object[] { -2000.00M,    0.00M, 0.00M, 0.00M, -2000.00M },
						new object[] { -1000.00M,    0.00M, 0.00M, 0.00M, -1000.00M },
						new object[] { 4000.00M, 4000.00M, 0.00M, 0.00M,     0.00M },
						new object[] { 3000.00M, 3000.00M, 0.00M, 0.00M,     0.00M }
					};

				generateLines(new[] { "" });
			}
			var dataTable = runScript();
			AssertDataTableAllRowsByKeyColumns(message, dataTable, headers.OfType<string>().ToArray(), lines.ToArray(), keyColumns.ToArray());
		}

		void PopulateJobCharge(BaseCharge charge)
		{
			charge.JR_OSSellAmt = 350m;
		}

		#endregion

		#region SQL Queries

		DataTable RunScript(ZBool includeJobDetails, ZBool includeConsolDetails, ZBool groupByJob, ZString jobType)
		{
			return RunScript(includeJobDetails, includeConsolDetails, groupByJob, jobType, ZDateTime.Empty);
		}

		DataTable RunScript(ZBool includeJobDetails, ZBool includeConsolDetails, ZBool groupByJob, ZString jobType, ZDateTime jk_JX_FromETD)
		{
			return RunScript(includeJobDetails, includeConsolDetails, groupByJob, jobType, jk_JX_FromETD, ZDateTime.Empty, ZDateTime.Empty);
		}

		DataTable RunScript(ZBool includeJobDetails, ZBool includeConsolDetails,
			ZBool groupByJob, ZString jobType, ZDateTime jh_FromRevenueRecognizedDate, ZDateTime jh_ToRevenueRecognizedDate)
		{
			return RunScript(includeJobDetails, includeConsolDetails, groupByJob, jobType, ZDateTime.Empty, jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate);
		}

		DataTable RunScript(ZBool includeJobDetails, ZBool includeConsolDetails,
			ZBool groupByJob, ZString jobType, ZDateTime jk_JX_FromETD, ZDateTime jh_FromRevenueRecognizedDate, ZDateTime jh_ToRevenueRecognizedDate)
		{
			return RunScript(includeJobDetails, includeConsolDetails, groupByJob, jobType, jk_JX_FromETD, jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate, false, false);
		}

		DataTable RunScript(ZBool includeJobDetails, ZBool includeConsolDetails,
			ZBool groupByJob, ZString jobType, ZDateTime jk_JX_FromETD, ZDateTime jh_FromRevenueRecognizedDate, ZDateTime jh_ToRevenueRecognizedDate,
			bool outstandingWIP, bool outstandingACR)
		{
			return RunScript(includeJobDetails, includeConsolDetails, groupByJob, jobType, jk_JX_FromETD,
				jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate, outstandingWIP, outstandingACR, ZDateTime.Today, ZDateTime.Today.AddDays(1), ZString.Empty);
		}

		DataTable RunScript(ZBool includeJobDetails, ZBool includeConsolDetails,
			ZBool groupByJob, ZString jobType, ZDateTime jk_JX_FromETD, ZDateTime jh_FromRevenueRecognizedDate, ZDateTime jh_ToRevenueRecognizedDate,
			bool outstandingWIP, bool outstandingACR, ZString jobActiveStatus)
		{
			return RunScript(includeJobDetails, includeConsolDetails, groupByJob, jobType, jk_JX_FromETD,
				jh_FromRevenueRecognizedDate, jh_ToRevenueRecognizedDate, outstandingWIP, outstandingACR, ZDateTime.Today, ZDateTime.Today.AddDays(1), jobActiveStatus);
		}

		DataTable RunScript(ZBool includeJobDetails, ZBool includeConsolDetails,
			ZBool groupByJob, ZString jobType, ZDateTime jk_JX_FromETD, ZDateTime jh_FromRevenueRecognizedDate, ZDateTime jh_ToRevenueRecognizedDate,
			bool outstandingWIP, bool outstandingACR, ZDateTime al_FromDate, ZDateTime al_ToDate, ZString jobActiveStatus, string excludeReversedWIPACR = "")
		{
			var paramExcludeReversedWIPACR = excludeReversedWIPACR == null ? string.Empty : $"@AL_ExcludeReversedWIPACR = '{excludeReversedWIPACR}',";
			var sql = string.Format(@"
						EXEC Report_JobProfit
						@IncludeJobDetails = '{9}',
						@IncludeConsolDetails = '{10}',
						@GroupByJob = '{11}',
						@JobType = '{12}',
						@JH_GC = '{0}',
						@JH_Status = NULL,
						@JH_IsActive = '{16}',
						@JH_BranchPKList = '''{2}'',''{3}''',
						@JH_departmentpkList = '',
						@JH_SalesRepPK = NULL,
						@JH_OperatorPK = NULL,
						@JH_FromCreatedDate = '1900-01-01 00:00:00',
						@JH_ToCreatedDate = '2079-06-06 23:59:29',
						@JH_FromClosedDate = '',
						@JH_ToClosedDate = '',
						@JH_FromRevenueRecognizedDate = '{7}' ,
						@JH_ToRevenueRecognizedDate = '{8}',
						@JH_LocalClientPKList = '',
						@JH_OverseasAgentPKList = '',
						@AC_ChargeGroup = NULL ,
						@AL_BranchPKList = '',
						@AL_departmentpkList = '''{1}''',
						@AL_ChargeCodePKList = '',
						@AL_ExcludedChargeCodePKList = '',
						@AL_CreditorPKList = '',
						@AL_DebtorPKList = '',
						@AL_FromDate = '{5}',
						@AL_ToDate = '{6}',
						{17}
						@JobShipmentList = NULL,
						@JobDeclarationList = NULL,
						@JS_TransportMode =NULL ,
						@JE_TransportMode =NULL,
						@JS_ContainerMode = NULL,
						@JE_ContainerMode = NULL,
						@FW_OriginPK =NULL ,
						@FW_DestinationPK = NULL,
						@FW_FromETA = '1900-01-01 00:00:00',
						@FW_ToETA =   '2079-06-06 23:59:29',
						@FW_FromETD = '1900-01-01 00:00:00',
						@FW_ToETD =   '2079-06-06 23:59:29',
						@AL_OutstandingWIPOnly = '{14}',
						@AL_OutstandingACROnly = '{15}',
						@JobConsolList = NULL,
						@JK_ContainerMode ='' ,
						@JK_TransportMode ='' ,
						@JK_AgentType ='' ,
						@JK_SendingAgentPK =NULL ,
						@JK_ReceivingAgentPK =NULL ,
						@JK_ColoadAgentPK =NULL ,
						@JK_CarrierPK =NULL ,
						@JK_JX_LoadPort =NULL ,
						@JK_JX_DischargePort =NULL ,
						@JK_JX_FromETD ='{13}' ,
						@JK_JX_ToETD =	'2079-06-06 23:59:29' ,
						@JK_JX_FromETA ='1900-01-01 00:00:00' ,
						@JK_JX_ToETA =	'2079-06-06 23:59:29' ,
						@CurrentCountry = '{4}',
						@FW_FromRegistration = '1900-01-01 00:00:00',
						@FW_ToRegistration =   '2079-06-06 23:59:29',
						@AC_AR_SalesGroup = NULL,
						@AC_AR_ExpenseGroup = NULL,
						@Gateway = 'ALL'
						",
						GlbCompany.CurrentCompany.PK,							//@JH_GC
						GlbDepartment.CurrentDepartment.PK,						//@AL_departmentpkList
						GlbBranch.CurrentBranch.PK,								//@JH_BranchPKList
						TestObjectCreator.NonCurrentBranch.PK,					//@JH_BranchPKList
						GlbCompany.CurrentCompany.GC_RN_NKCountryCode,          //@CurrentCountry
						al_FromDate,											//@AL_FromDate
						al_ToDate,												//@AL_ToDate
						GetMinDateTimeString(jh_FromRevenueRecognizedDate),		//@JH_FromRevenueRecognizedDate
						GetMaxDateTimeString(jh_ToRevenueRecognizedDate),		//@JH_ToRevenueRecognizedDate
						includeJobDetails, 										//@IncludeJobDetails
						includeConsolDetails, 									//@IncludeConsolDetails
						groupByJob, 											//@GroupByJob
						jobType,												//@JobType
						GetMinDateTimeString(jk_JX_FromETD),					//@JK_JX_FromETD
						outstandingWIP ? "Y" : "",								//@AL_OutstandingWIPOnly
						outstandingACR ? "Y" : "",								//@AL_OutstandingACROnly
						jobActiveStatus,										//@JH_IsActive
						paramExcludeReversedWIPACR								//@AL_ExcludeReversedWIPACR
			);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		#endregion
	}
}
