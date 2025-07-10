using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	class RptDt_Report_GlobalJobProfitSummaryByJobTest : ScriptTest
	{
		[TestDate(2012, 09, 24)]
		public void TestRevRecognitionFilter()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
				var testUser = Factory.NewWithValidTestData<GlbStaff>();
				testUser.GS_IsController = false;

				var testCompany = TestObjectCreator.CreateNewCompany("AAA", "US");
				testCompany.GC_RX_NKLocalCurrency = "USD";
				var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", testCompany);
				Factory.Save();

				var declaration = TestObjectCreator.CreateDeclaration("B001002");
				var job1 = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);
				job1.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				TestObjectCreator.CreateJobChargeRevRecognition(job1, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, AccountingConstants.RevenueRecognitionDateConstants.Immediate);
				TestObjectCreator.CreateJobChargeRevRecognition(job1, RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, ZDateTime.Today.AddDays(-1));
				using (SetupRevenueRecognition(job1))
				{
					job1.JH_A_JOP = ZDateTime.Today.AddDays(-1);
					TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 1000M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 3000M, TestObjectCreator.ABIGAS);
					TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 2000M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 4000M, TestObjectCreator.ABIGAS);
					Factory.Save();
				}

				var shipment1 = TestObjectCreator.CreateShipment("S001001");
				var job2 = TestObjectCreator.CreateJob(shipment1, false);
				job2.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				TestObjectCreator.CreateJobChargeRevRecognition(job2, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, ZDateTime.Today.AddMonths(-1));
				TestObjectCreator.CreateJobChargeRevRecognition(job2, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today.AddMonths(-2));
				using (SetupRevenueRecognition(job2))
				{
					job2.JH_A_JOP = ZDateTime.Today.AddMonths(-2);
					TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 3M, TestObjectCreator.ABIGAS);
					TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 2M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 4M, TestObjectCreator.ABIGAS);
					Factory.Save();
				}

				using (Env.SetTemporaryUserContext(testUser.GS_LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
					var shipment2 = TestObjectCreator.CreateShipment("S001002");
					var job3 = TestObjectCreator.CreateJob(shipment2, false);
					job3.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
					using (SetupRevenueRecognition(job3))
					{
						job3.JH_A_JOP = ZDateTime.Today.AddDays(-2);
						TestObjectCreator.CreateCharge(job3, TestObjectCreator.CC1, string.Empty, TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.USD, 3M, TestObjectCreator.ABIGAS);
						TestObjectCreator.CreateCharge(job3, TestObjectCreator.CC1, string.Empty, TestObjectCreator.USD, 2M, TestObjectCreator.AALSHI, TestObjectCreator.USD, 4M, TestObjectCreator.ABIGAS);
						Factory.Save();
					}
				}
				Factory.Save();

				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var headers = new string[] { "WIPAmountInLocalCurrency", "REVAmountInLocalCurrency", "REVAndWIPAmountInLocalCurrency", "ACRAmountInLocalCurrency", "CSTAmountInLocalCurrency", "CSTAndACRAmountInLocalCurrency", "ProfitInLocalCurrency",
				 "WIPAmountInReportCurrency", "REVAmountInReportCurrency", "REVAndWIPAmountInReportCurrency", "ACRAmountInReportCurrency", "CSTAmountInReportCurrency", "CSTAndACRAmountInReportCurrency", "ProfitInReportCurrency", "JH_JobNum", "GC_Code", "RecognitionDateList", "MissingExchangeRateDates" };
				var expectedDataForJob1 = new object[] { 7000.00M, 0.00M, 7000.00M, -3000.00M, 0.00M, -3000.00M, 4000.00M, 7000.00M, 0.00M, 7000.00M, -3000.00M, 0.00M, -3000.00M, 4000.00M, "B001002", GlbCompany.CurrentCompany.GC_Code, "CUS 23-Sep-12, IMM, JOP 23-Sep-12", "" };
				var expectedDataForJob2 = new object[] { 7.00M, 0.00M, 7.00M, -3.00M, 0.00M, -3.00M, 4.00M, 7.00M, 0.00M, 7.00M, -3.00M, 0.00M, -3.00M, 4.00M, "S001001", GlbCompany.CurrentCompany.GC_Code, "ARV 24-Aug-12, DEP 24-Jul-12, JOP 24-Jul-12", "" };
				var expectedDataForJob3_WithouthExRate = new object[] { 7.00M, 0.00M, 7.00M, -3.00M, 0.00M, -3.00M, 4.00M, 0.00M, 0.00M, 0.00M, 0.00M, 0.00M, 0.00M, 0.00M, "S001002", "AAA", "JOP 22-Sep-12", "(USD>AUD 2012-09-30)" };
				var dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today);
				AssertDataTableAllRowsByKeyColumns("Record for job B001002 and S001002", dataTable, headers, new[] { expectedDataForJob1, expectedDataForJob3_WithouthExRate });

				CreateExchangeRate(testCompany.PK, "AUD", "PER", new ZDateTime(2012, 01, 01), new ZDateTime(2012, 12, 31), 0.5m);
				Factory.Save();

				var expectedDataForJob3_WithExchangeRate = new object[] { 7.00M, 0.00M, 7.00M, -3.00M, 0.00M, -3.00M, 4.00M, 3.50M, 0.00M, 3.50M, -1.50M, 0.00M, -1.50M, 2.00M, "S001002", "AAA", "JOP 22-Sep-12", "" };
				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today);
				AssertDataTableAllRowsByKeyColumns("Record for job B001002 and S001002", dataTable, headers, new[] { expectedDataForJob1, expectedDataForJob3_WithExchangeRate });

				dataTable = RunScript(ZDateTime.Today.AddMonths(-2), ZDateTime.Today.AddMonths(-1));
				AssertDataTableAllRowsByKeyColumns("Record for job S001001", dataTable, headers, new[] { expectedDataForJob2 });

				dataTable = RunScript(ZDateTime.Today.AddMonths(-2), ZDateTime.Today);
				AssertDataTableAllRowsByKeyColumns("Record for job B001002 and S001001 and S001002", dataTable, headers, new[] { expectedDataForJob1, expectedDataForJob2, expectedDataForJob3_WithExchangeRate });
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestRevRecognitionFilterForComapnyWithDifferentPeriodSetup()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask();

				TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
				var testUser = Factory.NewWithValidTestData<GlbStaff>();
				testUser.GS_IsController = false;

				var testCompany = TestObjectCreator.CreateNewCompany("AAA", "US");
				testCompany.GC_RX_NKLocalCurrency = "USD";
				var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", testCompany);
				Factory.Save();

				var shipment1 = TestObjectCreator.CreateShipment("S001001");
				var job1 = TestObjectCreator.CreateJob(shipment1, false);
				job1.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				TestObjectCreator.CreateJobChargeRevRecognition(job1, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, ZDateTime.Today.AddMonths(-1));
				TestObjectCreator.CreateJobChargeRevRecognition(job1, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today.AddMonths(-2));
				using (SetupRevenueRecognition(job1))
				{
					job1.JH_A_JOP = ZDateTime.Today.AddDays(-20);
					TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 0, null, TestObjectCreator.AUD, 7692.31m, TestObjectCreator.ABIGAS);
					Factory.Save();
				}

				using (Env.SetTemporaryUserContext(testUser.GS_LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					TestObjectCreator.CreateTestPeriods(new ZDateTime(2012, 01, 01), Core.Constants.ACPeriodFormat.Weeks);
					var shipment2 = TestObjectCreator.CreateShipment("S001002");
					var job2 = TestObjectCreator.CreateJob(shipment2, false);
					job2.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
					var debtor = Factory.NewWithValidTestData<OrgHeader>();
					debtor.OH_IsDebtor = true;
					using (SetupRevenueRecognition(job2))
					{
						job2.JH_A_JOP = ZDateTime.Today.AddDays(-2);
						TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, string.Empty, testCompany.LocalCurrency, 0, null, testCompany.LocalCurrency, 8800m, debtor);
						Factory.Save();
					}
				}
				Factory.Save();

				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var headers = new string[] { "WIPAmountInLocalCurrency", "REVAmountInLocalCurrency", "ACRAmountInLocalCurrency", "CSTAmountInLocalCurrency", "ProfitInLocalCurrency",
				"WIPAmountInReportCurrency", "REVAmountInReportCurrency", "ACRAmountInReportCurrency", "CSTAmountInReportCurrency","ProfitInReportCurrency", "JH_JobNum", "GC_Code", "RecognitionDateList", "MissingExchangeRateDates" };

				var expectedDataForJob1 = new object[] { 7692.31M, 0.00M, 0.00M, 0.00M, 7692.31M, 7692.31M, 0.00M, 0.00M, 0.00M, 7692.31M, "S001001", GlbCompany.CurrentCompany.GC_Code, "ARV 24-Aug-12, DEP 24-Jul-12, JOP 04-Sep-12", "" };
				var expectedDataForJob2 = new object[] { 8800M, 0.00M, 0.00M, 0.00M, 8800M, 0.00M, 0.00M, 0.00M, 0.00M, 0.00M, "S001002", "AAA", "JOP 22-Sep-12", "(USD>AUD 2012-09-28)" };

				var dataTable = RunScript("AUD", ZDateTime.Today.AddDays(-30), ZDateTime.Today);
				AssertDataTableAllRowsByKeyColumns("Record for S001001 and S001002", dataTable, headers, new[] { expectedDataForJob1, expectedDataForJob2 });

				dataTable = RunScript("AUD", ZDateTime.Today.AddDays(-25), ZDateTime.Today.AddDays(-4));
				AssertDataTableAllRowsByKeyColumns("Record for S001001", dataTable, headers, new[] { expectedDataForJob1 });

				dataTable = RunScript("AUD", ZDateTime.Today.AddDays(-5), ZDateTime.Today);
				AssertDataTableAllRowsByKeyColumns("Record for S001002", dataTable, headers, new[] { expectedDataForJob2 });
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestExchangeRateForCurrencyFilter()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
				var testUser = Factory.NewWithValidTestData<GlbStaff>();
				testUser.GS_IsController = false;

				var testCompany = TestObjectCreator.CreateNewCompany("AAA", "SG");
				testCompany.GC_RX_NKLocalCurrency = "SGD";
				testCompany.GC_IsReciprocal = true;
				var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", testCompany);
				Factory.Save();

				var shipment1 = TestObjectCreator.CreateShipment("S001001");
				var job1 = TestObjectCreator.CreateJob(shipment1, false);
				job1.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				TestObjectCreator.CreateJobChargeRevRecognition(job1, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, ZDateTime.Today.AddMonths(-1));
				TestObjectCreator.CreateJobChargeRevRecognition(job1, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today.AddMonths(-2));
				using (SetupRevenueRecognition(job1))
				{
					job1.JH_A_JOP = ZDateTime.Today.AddMonths(-2);
					TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 0, null, TestObjectCreator.AUD, 7692.31m, TestObjectCreator.ABIGAS);
					Factory.Save();
				}

				using (Env.SetTemporaryUserContext(testUser.GS_LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
					var shipment2 = TestObjectCreator.CreateShipment("S001002");
					var job2 = TestObjectCreator.CreateJob(shipment2, false);
					job2.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
					var debtor = Factory.NewWithValidTestData<OrgHeader>();
					debtor.OH_IsDebtor = true;
					using (SetupRevenueRecognition(job2))
					{
						job2.JH_A_JOP = ZDateTime.Today.AddDays(-2);
						TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, string.Empty, testCompany.LocalCurrency, 0, null, testCompany.LocalCurrency, 8800m, debtor);
						Factory.Save();
					}
				}
				Factory.Save();

				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				CreateExchangeRate(GlbCompany.CurrentCompany.PK, "USD", "CUS", new ZDateTime(2012, 07, 01), new ZDateTime(2012, 09, 30), 1.39m);
				CreateExchangeRate(GlbCompany.CurrentCompany.PK, "USD", "PER", new ZDateTime(2012, 01, 31), new ZDateTime(2012, 03, 30), 0.72m);
				CreateExchangeRate(GlbCompany.CurrentCompany.PK, "USD", "PER", new ZDateTime(2012, 06, 30), new ZDateTime(2012, 09, 30), 0.69m);

				CreateExchangeRate(testCompany.PK, "USD", "PER", new ZDateTime(2012, 09, 01), new ZDateTime(2012, 10, 01), 1.44m);
				Factory.Save();

				var headers = new string[] { "WIPAmountInLocalCurrency", "REVAmountInLocalCurrency", "ACRAmountInLocalCurrency", "CSTAmountInLocalCurrency", "ProfitInLocalCurrency",
				"WIPAmountInReportCurrency", "REVAmountInReportCurrency", "ACRAmountInReportCurrency", "CSTAmountInReportCurrency","ProfitInReportCurrency", "JH_JobNum", "GC_Code", "RecognitionDateList", "MissingExchangeRateDates" };

				var expectedAmountInUSDForJob1 = new ZDecimal(7692.31M * 0.69M).Round(2);
				var expectedAmountInUSDForJob2 = new ZDecimal(8800M / 1.44M).Round(2);

				var expectedDataForJob1 = new object[] { 7692.31M, 0.00M, 0.00M, 0.00M, 7692.31M, expectedAmountInUSDForJob1, 0.00M, 0.00M, 0.00M, expectedAmountInUSDForJob1, "S001001", GlbCompany.CurrentCompany.GC_Code, "ARV 24-Aug-12, DEP 24-Jul-12, JOP 24-Jul-12", "" };
				var expectedDataForJob2 = new object[] { 8800M, 0.00M, 0.00M, 0.00M, 8800M, expectedAmountInUSDForJob2, 0.00M, 0.00M, 0.00M, expectedAmountInUSDForJob2, "S001002", "AAA", "JOP 22-Sep-12", "" };

				var dataTable = RunScript("USD", ZDateTime.Today.AddMonths(-2), ZDateTime.Today);
				AssertDataTableAllRowsByKeyColumns("Record for S001001 and S001002", dataTable, headers, new[] { expectedDataForJob1, expectedDataForJob2 });

				dataTable = RunScript("USD", ZDateTime.Today.AddMonths(-2), ZDateTime.Today.AddMonths(-1));
				AssertDataTableAllRowsByKeyColumns("Record for S001001", dataTable, headers, new[] { expectedDataForJob1 });

				dataTable = RunScript("USD", ZDateTime.Today.AddMonths(-1), ZDateTime.Today);
				AssertDataTableAllRowsByKeyColumns("Record for S001002", dataTable, headers, new[] { expectedDataForJob2 });

				expectedDataForJob1 = new object[] { 7692.31M, 0.00M, 0.00M, 0.00M, 7692.31M, 7692.31M, 0.00M, 0.00M, 0.00M, 7692.31M, "S001001", GlbCompany.CurrentCompany.GC_Code, "ARV 24-Aug-12, DEP 24-Jul-12, JOP 24-Jul-12", "" };
				expectedDataForJob2 = new object[] { 8800M, 0.00M, 0.00M, 0.00M, 8800M, 0.00M, 0.00M, 0.00M, 0.00M, 0.00M, "S001002", "AAA", "JOP 22-Sep-12", "(SGD>AUD 2012-09-30)" };
				dataTable = RunScript("AUD", ZDateTime.Today.AddMonths(-2), ZDateTime.Today);
				AssertDataTableAllRowsByKeyColumns("Record for S001001 and S001002", dataTable, headers, new[] { expectedDataForJob1, expectedDataForJob2 });

				expectedDataForJob1 = new object[] { 7692.31M, 0.00M, 0.00M, 0.00M, 7692.31M, 0.00M, 0.00M, 0.00M, 0.00M, 0.00M, "S001001", GlbCompany.CurrentCompany.GC_Code, "ARV 24-Aug-12, DEP 24-Jul-12, JOP 24-Jul-12", "(AUD>SGD 2012-07-31)" };
				expectedDataForJob2 = new object[] { 8800M, 0.00M, 0.00M, 0.00M, 8800M, 8800M, 0.00M, 0.00M, 0.00M, 8800M, "S001002", "AAA", "JOP 22-Sep-12", "" };
				dataTable = RunScript("SGD", ZDateTime.Today.AddMonths(-2), ZDateTime.Today);
				AssertDataTableAllRowsByKeyColumns("Record for S001001 and S001002", dataTable, headers, new[] { expectedDataForJob1, expectedDataForJob2 });
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestChargeCodeFilter()
		{
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today);
				AssertEquals("Should found both record for job1 and job2", 2, dataTable.Rows.Count);

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", TestObjectCreator.CC1, null);
				AssertEquals("Should found 1 record for job1 as charge code is CC1", 1, dataTable.Rows.Count);
				Assert("Should found 1 record for job1 as charge code is CC1", dataTable.Rows.Cast<DataRow>().All(x => x["JH_JobNum"].ToString() == "S001001"));

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", TestObjectCreator.CC2, null);
				AssertEquals("Should found 1 record for job2 as charge code is CC2", 1, dataTable.Rows.Count);
				Assert("Should found 1 record for job2 as charge code is CC2", dataTable.Rows.Cast<DataRow>().All(x => x["JH_JobNum"].ToString() == "S001002"));

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", null, TestObjectCreator.CC1);
				AssertEquals("Should found 1 record for job2 as charge code is CC2", 1, dataTable.Rows.Count);
				Assert("Should found 1 record for job2 as charge code is CC2", dataTable.Rows.Cast<DataRow>().All(x => x["JH_JobNum"].ToString() == "S001002"));

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", null, TestObjectCreator.CC2);
				AssertEquals("Should found 1 record for job1 as charge code is CC1", 1, dataTable.Rows.Count);
				Assert("Should found 1 record for job1 as charge code is CC1", dataTable.Rows.Cast<DataRow>().All(x => x["JH_JobNum"].ToString() == "S001001"));

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", TestObjectCreator.CC1, TestObjectCreator.CC1);
				AssertEquals("Should not found  any record", 0, dataTable.Rows.Count);

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "FRT");
				AssertEquals("Should found 1 record for job1 as charge code is group is 'FRT'", 1, dataTable.Rows.Count);
				Assert("Should found 1 record for job1 as charge code is group is 'FRT'", dataTable.Rows.Cast<DataRow>().All(x => x["JH_JobNum"].ToString() == "S001001"));

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "BRK");
				AssertEquals("Should found 1 record for job2 as charge code is group is 'BRK'", 1, dataTable.Rows.Count);
				Assert("Should found 1 record for job2 as charge code is group is 'BRK'", dataTable.Rows.Cast<DataRow>().All(x => x["JH_JobNum"].ToString() == "S001002"));
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestDebtorCreditorFilter()
		{
			SetUpData();
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today);
				AssertEquals("Should found both record for job1 and job2", 2, dataTable.Rows.Count);

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", null, null, TestObjectCreator.ABIGAS);
				AssertEquals("Should found 1 record for job1 as debtor is ABIGAS", 1, dataTable.Rows.Count);
				Assert("Should found 1 record for job1 as debtor is ABIGAS", dataTable.Rows.Cast<DataRow>().All(x => x["JH_JobNum"].ToString() == "S001001"));

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", null, null, null, TestObjectCreator.AALSHI);
				AssertEquals("Should found 1 record for job1 as creditor is AALSHI", 1, dataTable.Rows.Count);
				Assert("Should found 1 record for job1 as creditor is AALSHI", dataTable.Rows.Cast<DataRow>().All(x => x["JH_JobNum"].ToString() == "S001001"));

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", null, null, TestObjectCreator.ABIGAS, TestObjectCreator.AALSHI);
				AssertEquals("Should not found any record", 0, dataTable.Rows.Count);
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestSalesAndExpenseGroupFilter()
		{
			SetUpData();
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var expGroup = Factory.NewWithValidTestData<AccGroups>();
				expGroup.AR_Code = "EG1";
				var salesGroup = Factory.NewWithValidTestData<AccGroups>();
				salesGroup.AR_Code = "SG1";

				TestObjectCreator.CC1.AC_AR_SalesGroup = salesGroup.PK;
				TestObjectCreator.CC2.AC_AR_ExpenseGroup = expGroup.PK;
				Factory.Save();

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today);
				AssertEquals("Should found both record for job1 and job2", 2, dataTable.Rows.Count);

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", null, null, null, null, salesGroup);
				AssertEquals("Should found 1 record for job2 as sales group is SG1", 1, dataTable.Rows.Count);
				Assert("Should found 1 record for job2 as sales group is SG1", dataTable.Rows.Cast<DataRow>().All(x => x["JH_JobNum"].ToString() == "S001001"));

				dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", null, null, null, null, null, expGroup);
				AssertEquals("Should found 1 record for job2  as sales group is EG1", 1, dataTable.Rows.Count);
				Assert("Should found 1 record for job2 as sales group is EG1", dataTable.Rows.Cast<DataRow>().All(x => x["JH_JobNum"].ToString() == "S001002"));
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestGetJobCostingDataQueueStatus()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask();
			}

			var shipment1 = TestObjectCreator.CreateShipment("S001001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 3M, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 2M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 4M, TestObjectCreator.ABIGAS);

			var shipment2 = TestObjectCreator.CreateShipment("S001002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC2, string.Empty, TestObjectCreator.AUD, 1M, TestObjectCreator.ZECTRA, TestObjectCreator.AUD, 3M, TestObjectCreator.XLINDU);
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC2, string.Empty, TestObjectCreator.AUD, 2M, TestObjectCreator.ZECTRA, TestObjectCreator.AUD, 4M, TestObjectCreator.XLINDU);

			Factory.Save();

			var results = RunGetJCDQueueStatus();
			AssertEquals("GetJobCostingDataQueueStatus should only ever return 1 row", 1, results.Rows.Count);

			var row = results.Rows[0];
			AssertEquals("Things have been added to the queue so HasRecordInQueue should be 1", 1, row["HasRecordInQueue"]);

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				results = RunGetJCDQueueStatus();
				AssertEquals("GetJobCostingDataQueueStatus should only ever return 1 row", 1, results.Rows.Count);

				row = results.Rows[0];
				AssertEquals("The queue should have been empty so HasRecordInQueue should be 0", 0, row["HasRecordInQueue"]);
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestControllingCustomerFilter()
		{
			AssertControllingCustomerFilter(false);
		}

		[TestDate(2012, 09, 24)]
		public void TestControllingCustomerGroupFilter()
		{
			AssertControllingCustomerFilter(true);
		}

		void AssertControllingCustomerFilter(bool controllingCustomerGroup)
		{
			SetUpData();
			using (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = TestObjectCreator.CreateOrgHeader("TST", true, true);
				orgHeader.OH_IsControllingCustomer = true;
				var orgAddress = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Office, true);

				var relatedParty = orgHeader.AllRelatedParties.AddNew();
				relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				if (!controllingCustomerGroup)
				{
					relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
					relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
					relatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				}
				else
				{
					relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
					relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
				}

				var shipment = TestObjectCreator.CreateShipment("S001003");
				var job = TestObjectCreator.CreateJob(shipment);
				var jobDocAddress = TestObjectCreator.CreateJobDocAddress(shipment.PK, DocAddressTypes.Codes.ControllingCustomer, orgAddress.PK);

				TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 100m, 200m);
				TestObjectCreator.PostJobAsBillingTab(job, JobInvoicingPostingOption.All);
				Factory.Save();

				using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
				{
					dataPopulatingTask.ServiceLogger = new TestServiceLogger();
					dataPopulatingTask.RunTask(); // Processes AL Records
					dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

					DataTable dataTable;
					if (controllingCustomerGroup)
					{
						dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", null, null, null, null, null, null, null, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
					}
					else
					{
						dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today, "", null, null, null, null, null, null, orgHeader.PK);
					}

					AssertEquals("Should only return the record which controlling customer is orgHeader.", 1, dataTable.Rows.Count);
					AssertEquals("Should only return the record which controlling customer is orgHeader.", "S001003", dataTable.Rows[0]["JH_JobNum"]);
				}
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestJobInactive()
		{
			SetUpData();
			var job2 = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_JobNum, "S001002"))[0];
			job2.MarkAsInactive();
			Factory.Save();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-1), ZDateTime.Today);
				AssertEquals("Should found 1 record", 1, dataTable.Rows.Count);
				Assert("Should found 1 record for job1 as jobheader active", dataTable.Rows.Cast<DataRow>().All(x => x["JH_JobNum"].ToString() == "S001001"));
			}
		}

		#region Implementation

		DataTable RunGetJCDQueueStatus()
		{
			string sqlQuery = "SELECT * FROM dbo.GetJobCostingDataQueueStatus()";
			var command = TestConnection.Command(sqlQuery);
			return DataUtils.GetDataTableFromCommand(command);
		}

		DataTable RunScript(ZDateTime fromRevenueRecognizedDate, ZDateTime toRevenueRecognizedDate)
		{
			return RunScript(fromRevenueRecognizedDate, toRevenueRecognizedDate, "");
		}

		DataTable RunScript(ZString reportCurrency, ZDateTime fromRevenueRecognizedDate, ZDateTime toRevenueRecognizedDate)
		{
			var currency = RefCurrency.LoadFromCurrencyCode(Factory, reportCurrency);
			return RunScript(currency.PK, fromRevenueRecognizedDate, toRevenueRecognizedDate, "");
		}

		DataTable RunScript(ZDateTime fromRevenueRecognizedDate, ZDateTime toRevenueRecognizedDate, ZString chargeGroup, AccChargeCode chargeCodeIN = null, AccChargeCode chargeCodeNOTIN = null,
								OrgHeader debtor = null, OrgHeader creditor = null, AccGroups salesGroup = null, AccGroups expGroup = null, ZGuid? cCBOrgPK = null, ZGuid? mNGOrgPK = null)
		{
			var currency = RefCurrency.LoadFromCurrencyCode(Factory, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			return RunScript(currency.PK, fromRevenueRecognizedDate, toRevenueRecognizedDate, chargeGroup, chargeCodeIN, chargeCodeNOTIN,
								 debtor, creditor, salesGroup, expGroup, cCBOrgPK, mNGOrgPK);
		}

		DataTable RunScript(ZGuid reportCurrency, ZDateTime fromRevenueRecognizedDate, ZDateTime toRevenueRecognizedDate, ZString chargeGroup, AccChargeCode chargeCodeIN = null, AccChargeCode chargeCodeNOTIN = null,
								OrgHeader debtor = null, OrgHeader creditor = null, AccGroups salesGroup = null, AccGroups expGroup = null, ZGuid? cCBOrgPK = null, ZGuid? mNGOrgPK = null)
		{
			string sqlQuery = string.Format(@"
SELECT * FROM RptDt_Report_GlobalJobProfitSummaryByJob
( 
	'{0}',					--@ReportCurrency
	'{1}',					--@TransactionFrom  
	'{2}',					--@TransactionTo	
	'{3}' ,					--@ChargeCode  
	'{4}' ,					--@ChargeCodeNOTIN
	'{7}' ,					--@ChargeGroup
	 {8},					--@SalesGroup
	 {9},					--@ExpenseGroup
	'{5}',					--@TransactionDebtor
	'{6}',					--@TransactionCreditor
	@CCBListValue,			--@CCBListValue 
	@CCBListIsEmptyValue,	--@CCBListIsEmptyValue
	@MNGListValue,			--@MNGListValue
	@MNGListIsEmptyValue	--@MNGListIsEmptyValue
) Order By JH_JobNum",
						reportCurrency,
						GetMinDateTimeString(fromRevenueRecognizedDate),
						GetMaxDateTimeString(toRevenueRecognizedDate),
						chargeCodeIN == null ? ZString.Empty : (ZString)chargeCodeIN.PK.ToString(),
						chargeCodeNOTIN == null ? ZString.Empty : (ZString)chargeCodeNOTIN.PK.ToString(),
						debtor == null ? "" : debtor.PK.ToString(),
						creditor == null ? "" : creditor.PK.ToString(),
						chargeGroup,
						salesGroup == null ? "NULL" : string.Format("'{0}'", salesGroup.PK.ToString()),
						expGroup == null ? "NULL" : string.Format("'{0}'", expGroup.PK.ToString())
						);
			var command = TestConnection.Command(sqlQuery);
			AddTVPAndIsEmptyParameters(command, "@MNGListValue", "dbo.TVP_uniqueidentifier", "@MNGListIsEmptyValue", mNGOrgPK.HasValue ? new Guid[] { mNGOrgPK.Value.ToGuid() } : Array.Empty<Guid>());
			AddTVPAndIsEmptyParameters(command, "@CCBListValue", "dbo.TVP_uniqueidentifier", "@CCBListIsEmptyValue", cCBOrgPK.HasValue ? new Guid[] { cCBOrgPK.Value.ToGuid() } : Array.Empty<Guid>());
			return DataUtils.GetDataTableFromCommand(command);
		}

		void CreateExchangeRate(ZGuid companyPK, ZString currencyCode, ZString type, ZDateTime startDate, ZDateTime expiryDate, ZDecimal sellRate)
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_RX_NKExCurrency = currencyCode;
			exchangeRate.RE_ExRateType = type;
			exchangeRate.RE_StartDate = startDate;
			exchangeRate.RE_ExpiryDate = expiryDate;
			exchangeRate.RE_SellRate = sellRate;
			exchangeRate.RE_GC = companyPK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var resetAction = new JCDResetActionStrategy(TestConnection, new TestServiceLogger());
			resetAction.Process();
		}

		#region Test Data Setup
		void SetUpData()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
			TestObjectCreator.CC1.AC_ChargeGroup = "FRT";
			TestObjectCreator.CC2.AC_ChargeGroup = "BRK";
			Factory.Save();

			var shipment1 = TestObjectCreator.CreateShipment("S001001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			job1.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			using (SetupRevenueRecognition(job1))
			{
				job1.JH_A_JOP = ZDateTime.Today.AddDays(-1);
				TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 3M, TestObjectCreator.ABIGAS);
				TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 2M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 4M, TestObjectCreator.ABIGAS);
				Factory.Save();
			}

			var shipment2 = TestObjectCreator.CreateShipment("S001002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			job2.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			using (SetupRevenueRecognition(job2))
			{
				job2.JH_A_JOP = ZDateTime.Today.AddDays(-1);
				TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC2, string.Empty, TestObjectCreator.AUD, 1M, TestObjectCreator.ZECTRA, TestObjectCreator.AUD, 3M, TestObjectCreator.XLINDU);
				TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC2, string.Empty, TestObjectCreator.AUD, 2M, TestObjectCreator.ZECTRA, TestObjectCreator.AUD, 4M, TestObjectCreator.XLINDU);
				Factory.Save();
			}
			Factory.Save();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask();
			}
		}
		IDisposable SetupRevenueRecognition(Job job)
		{
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			var revenueRecognitionByChargeGroupCollection = new RevenueRecognitionCollection();
			var revenueRecognition = revenueRecognitionByChargeGroupCollection.AddNew();
			revenueRecognition.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			revenueRecognition.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			revenueRecognition.Offset = 0;

			return AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetTemporaryValue(Guid.Empty,
				Guid.Empty, Guid.Empty, revenueRecognitionByChargeGroupCollection);
		}

		#endregion

		#endregion

	}
}
