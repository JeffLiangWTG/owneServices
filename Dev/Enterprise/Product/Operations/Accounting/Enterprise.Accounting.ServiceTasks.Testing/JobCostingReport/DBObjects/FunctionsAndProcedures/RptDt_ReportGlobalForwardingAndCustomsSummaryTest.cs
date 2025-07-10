using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	class RptDt_ReportGlobalForwardingAndCustomsSummaryTest : ScriptTest
	{
		#region testing basic data

		[TestDate(2012, 09, 24)]
		public void TestBasicUsageWithSingleJob()
		{
			var firstLoadETD = ZDateTime.Today.AddDays(5);
			var firstLoadATD = ZDateTime.Today.AddDays(6);
			var lastDischargeETA = ZDateTime.Today.AddDays(10);
			var lastDischargeATA = ZDateTime.Today.AddDays(11);

			SetUpSingleJob(1, firstLoadETD: firstLoadETD, firstLoadATD: firstLoadATD,
								lastDischargeETA: lastDischargeETA, lastDischargeATA: lastDischargeATA, resetPrepaidCollect: true);
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));
				AssertEquals("Should find single record for job1", 1, dataTable.Rows.Count);

				var row = dataTable.Rows[0];

				string[] fields = { "JH_JobNum", "HouseBillNumber",  "FW_TransportMode", "FW_ContainerMode",
					"NKOrigUNLOCOCode", "OriginETD", "NKDestUNLOCOCode", "DestinationETA",
					"DepartureConsolNum", "DepartureAgentType", "DepartureShippingLineCode", "DepartureCreditorCode",
					"DepartureSendingAgentCode", "DepartureReceivingAgentCode", "DepartureTransportMode",
					"DepartureConsolFirstLoad", "DepartureConsolETD", "DepartureConsolATD",
					"ArrivalConsolLastDischarge", "ArrivalConsolETA", "ArrivalConsolATA",
					"ConsigneeImporterCode", "ConsigneeImporterFullName", "ConsignorShipperSupplierCode", "ConsignorShipperSupplierFullName",
					"ActualWeight", "UnitOfWeight", "ActualVolume", "UnitOfVolume", "ActualChargeable",
					"TotalAttached"
				};

				object[] values =
				{
					"S000001", "HB#1", "TM1", "CM1",
					"AUMEL",  ZDateTime.Today.AddDays(-20), "NZAHU", ZDateTime.Today.AddDays(20),
					"C0011", "A11", "ZSL11", "ZCR11",
					"ZSA11", "ZRA11",  "T11",
					"AUSYD", firstLoadETD, firstLoadATD,
					"NZAKL", lastDischargeETA, lastDischargeATA,
					"ZIMP1", "Importer 1", "ZSHIP1", "Supplier 1",
					Convert.ToDecimal(11.0), "kg", Convert.ToDecimal(21.0), "m3", Convert.ToDecimal(31.0),
					1
				};

				AssertDataRow(row, fields, values);
			}
		}

		#endregion

		#region testing money

		[TestDate(2012, 09, 24)]
		public void TestSingleJobMoneyShownProperly()
		{
			SetUpSingleJob(1, sellAmount: 4M, costAmount: 7M);
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));
				AssertEquals("Should find single record for job1", 1, dataTable.Rows.Count);

				var row = dataTable.Rows[0];

				string[] fields =
				{
					"REVAmountInReportCurrency", "WIPAmountInReportCurrency", "CSTAmountInReportCurrency", "ACRAmountInReportCurrency",
					"ProfitInReportCurrency"
				};

				object[] values =
				{
					0M, 4M, 0M, -7M, -3M
				};

				AssertDataRow(row, fields, values);
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestSingleJobWithMultipleConsolsMoneyShownProperly()
		{
			SetUpSingleJob(1, consols: 2, sellAmount: 4M, costAmount: 7M);
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));
				AssertEquals("Should find single record for job1", 1, dataTable.Rows.Count);

				var row = dataTable.Rows[0];

				string[] fields =
				{
					"REVAmountInReportCurrency", "WIPAmountInReportCurrency", "CSTAmountInReportCurrency", "ACRAmountInReportCurrency",
					"ProfitInReportCurrency"
				};

				object[] values =
				{
					0M, 4M, 0M, -7M, -3M
				};

				AssertDataRow(row, fields, values);
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestDifferentReportCurrencyAndLocalCurrency()
		{
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				var testCompany = TestObjectCreator.CreateNewCompany("BBB", "US");
				testCompany.GC_RX_NKLocalCurrency = "AUD";
				testCompany.GC_IsReciprocal = true;

				var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", testCompany);

				Factory.Save();

				SetUpSingleJob(1, sellAmount: 4M, costAmount: 7M, branch: testBranch, transactionRecognised: ZDateTime.Today.AddDays(2));
				SetUpData();

				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				CreateExchangeRate(GlbCompany.CurrentCompany.PK, "USD", "PER", new ZDateTime(2012, 09, 1), new ZDateTime(2012, 09, 30), 2m);

				Factory.Save();

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), reportCurrency: TestObjectCreator.USD.PK);
				AssertEquals("Should find single record for job1", 1, dataTable.Rows.Count);

				var row = dataTable.Rows[0];

				string[] fields =
				{
					"REVAmountInReportCurrency", "WIPAmountInReportCurrency", "CSTAmountInReportCurrency", "ACRAmountInReportCurrency",
					"ProfitInReportCurrency"
				};

				object[] values =
				{
					0M, 8M, 0M, -14M, -6M,
				};

				AssertDataRow(row, fields, values);
			}
		}
		#endregion

		#region testing summing correctly

		[TestDate(2012, 09, 24)]
		public void TestSingleJobWithMultipleConsolsVolumeAndWeightDisplayedProperly()
		{
			SetUpSingleJob(1, consols: 2);
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));
				AssertEquals("Should find single record for job1", 1, dataTable.Rows.Count);

				var row = dataTable.Rows[0];

				string[] fields =
				{
					"ActualWeight", "ActualVolume", "ActualChargeable",
				};

				object[] values =
				{
					Convert.ToDecimal(11.0), Convert.ToDecimal(21.0), Convert.ToDecimal(31.0)
				};

				AssertDataRow(row, fields, values);
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestSingleJobWithMultipleConsolsShipmentsDisplayedProperly()
		{
			SetUpSingleJob(1, consols: 4, extraShipments: 2);
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));
				AssertEquals("Should find single record for job1", 1, dataTable.Rows.Count);

				var row = dataTable.Rows[0];

				string[] fields =
				{
					"TotalAttached"
				};

				object[] values =
				{
					4
				};

				AssertDataRow(row, fields, values);
			}
		}
		#endregion

		#region testing multiple jobs

		[TestDate(2012, 09, 24)]
		public void TestTwoSeparateJobsStayUncombined()
		{
			SetUpData(2);

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));
				AssertEquals("Should find two records (job1 and job2)", 2, dataTable.Rows.Count);
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestTwoConsolsInTheSameJobAreCombined()
		{
			SetUpSingleJob(1, consols: 2);
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));
				AssertEquals("Should find only one job (both parts should be combined into 1)", 1, dataTable.Rows.Count);
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestTwoConsolsInTheSameJobAreBothFeaturedInConsignRefs()
		{
			SetUpSingleJob(1, consols: 2);
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));
				var row = dataTable.Rows[0];

				var consols = row["consignRefs"].ToString();
				Assert("Job should contain first consol", consols.Contains("C0011"));
				Assert("Job should contain second consol", consols.Contains("C0012"));
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestManyConsolesInTheSameJobAllFeatureInConsignRefs()
		{
			SetUpSingleJob(1, consols: 5);
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));
				var row = dataTable.Rows[0];

				var consols = row["consignRefs"].ToString();

				for (var i = 1; i <= 5; i++)
				{
					Assert("Job should contain consol #" + i, consols.Contains("C001" + i));
				}
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestMultipleChargesInTheSameConsolHasTheConsolOnlyAppearOnce()
		{
			SetUpSingleJob(1);
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));
				var row = dataTable.Rows[0];

				var consols = row["consignRefs"].ToString();
				AssertEquals("Job should have only one consol exactly once", "C0011", consols);
			}
		}

		#endregion

		#region testing departure and arrival consols

		[TestDate(2012, 09, 24)]
		public void TestDepartureAndArrivalConsolDeterminedCorrectly()
		{
			var departureLoadETD = ZDateTime.Today.AddDays(-5);
			var departureDischargeETA = ZDateTime.Today.AddDays(-3);
			var departurePort = "AUMEL";

			var middleLoadETD = ZDateTime.Today;
			var middleDischargeETA = ZDateTime.Today.AddDays(2);
			var middlePort = "AUSYD";

			var arrivalLoadETD = ZDateTime.Today.AddDays(5);
			var arrivalDischargeETA = ZDateTime.Today.AddDays(7);
			var arrivalPort = "NZAKL";

			// Don't create these in chronological order in case that has some effect
			var middleConsol = CreateNewConsol(middlePort, middlePort, 1, "1",
												firstLoadETD: middleLoadETD, firstLoadATD: middleLoadETD,
												lastDischargeETA: middleDischargeETA, lastDischargeATA: middleDischargeETA);
			var departureConsol = CreateNewConsol(departurePort, departurePort, 2, "1",
													firstLoadETD: departureLoadETD, firstLoadATD: departureLoadETD,
													lastDischargeETA: departureDischargeETA, lastDischargeATA: departureDischargeETA);

			var arrivalConsol = CreateNewConsol(arrivalPort, arrivalPort, 3, "1",
													firstLoadETD: arrivalLoadETD, firstLoadATD: arrivalLoadETD,
													lastDischargeETA: arrivalDischargeETA, lastDischargeATA: arrivalDischargeETA);

			SetUpSingleJob(1, consols: 0, existingConsols: new ForwardingConsol[] { middleConsol, departureConsol, arrivalConsol });
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));

				AssertEquals("There should only be one job", 1, dataTable.Rows.Count);

				var row = dataTable.Rows[0];

				string[] fields =
				{
					"DepartureConsolFirstLoad", "DepartureConsolETD", "DepartureConsolATD",
					"ArrivalConsolLastDischarge", "ArrivalConsolETA", "ArrivalConsolATA",
				};

				object[] values =
				{
					departurePort, departureLoadETD, departureLoadETD,
					arrivalPort, arrivalDischargeETA, arrivalDischargeETA
				};

				AssertDataRow(row, fields, values);
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestDepartureAndArrivalConsolDeterminedCorrectlyWithNulls()
		{
			var departureLoadETD = ZDateTime.Today.AddDays(-5);
			var departureDischargeETA = ZDateTime.Today.AddDays(-3);
			string departurePort = null;

			var middleLoadETD = ZDateTime.Today;
			var middleDischargeETA = ZDateTime.Today.AddDays(2);
			var middlePort = "AUSYD";

			var arrivalLoadETD = ZDateTime.Today.AddDays(5);
			var arrivalDischargeETA = ZDateTime.Today.AddDays(7);
			var arrivalPort = "NZAKL";

			// Don't create these in chronological order in case that has some effect
			var middleConsol = CreateNewConsol(middlePort, middlePort, 1, "1",
												firstLoadETD: middleLoadETD, firstLoadATD: middleLoadETD,
												lastDischargeETA: middleDischargeETA, lastDischargeATA: middleDischargeETA);
			var departureConsol = CreateNewConsol(departurePort, departurePort, 2, "1",
													firstLoadETD: departureLoadETD, firstLoadATD: departureLoadETD,
													lastDischargeETA: departureDischargeETA, lastDischargeATA: departureDischargeETA);

			var arrivalConsol = CreateNewConsol(arrivalPort, arrivalPort, 3, "1",
													firstLoadETD: arrivalLoadETD, firstLoadATD: arrivalLoadETD,
													lastDischargeETA: arrivalDischargeETA, lastDischargeATA: arrivalDischargeETA);

			SetUpSingleJob(1, consols: 0, existingConsols: new ForwardingConsol[] { middleConsol, departureConsol, arrivalConsol });
			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50));

				AssertEquals("There should only be one job", 1, dataTable.Rows.Count);

				var row = dataTable.Rows[0];

				string[] fields =
				{
					"DepartureConsolFirstLoad", "DepartureConsolETD", "DepartureConsolATD",
					"ArrivalConsolLastDischarge", "ArrivalConsolETA", "ArrivalConsolATA",
				};

				object[] values =
				{
					"", departureLoadETD, departureLoadETD,
					arrivalPort, arrivalDischargeETA, arrivalDischargeETA
				};

				AssertDataRow(row, fields, values);
			}
		}

		#endregion

		#region testing filters

		[TestDate(2012, 09, 24)]
		public void TestTransactionRecognisedFilter()
		{
			SetUpSingleJob(1, transactionRecognised: ZDateTime.Today.AddDays(-1));
			SetUpSingleJob(2, transactionRecognised: ZDateTime.Today.AddDays(-5));

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(0));

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddDays(-6), ZDateTime.Today.AddDays(-4));

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestDebtorInFilter()
		{
			SetUpSingleJob(1);
			SetUpSingleJob(2);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var firstDebtor = TestObjectCreator.Debtor;
				var secondDebtor = TestObjectCreator.ABIGAS;

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), debtorIN: firstDebtor);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), debtorIN: secondDebtor);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestDebtorNotInFilter()
		{
			SetUpSingleJob(1);
			SetUpSingleJob(2);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var firstDebtor = TestObjectCreator.Debtor;
				var secondDebtor = TestObjectCreator.ABIGAS;

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), debtorNOTIN: secondDebtor);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), debtorNOTIN: firstDebtor);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestCreditorInFilter()
		{
			SetUpSingleJob(1);
			SetUpSingleJob(2);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var firstCreditor = TestObjectCreator.Creditor1;
				var secondCreditor = TestObjectCreator.Creditor2;

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), creditorIN: firstCreditor);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), creditorIN: secondCreditor);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestCreditorNotInFilter()
		{
			SetUpSingleJob(1);
			SetUpSingleJob(2);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var firstCreditor = TestObjectCreator.Creditor1;
				var secondCreditor = TestObjectCreator.Creditor2;

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), creditorNOTIN: secondCreditor);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), creditorNOTIN: firstCreditor);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestSalesRepFilter()
		{
			var firstSalesRep = TestObjectCreator.CreateStaff("SR1");
			var secondSalesRep = TestObjectCreator.CreateStaff("SR2");
			Factory.Save();

			SetUpSingleJob(1, salesRep: firstSalesRep);
			SetUpSingleJob(2, salesRep: secondSalesRep);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), salesRep: firstSalesRep);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), salesRep: secondSalesRep);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestJobOperatorFilter()
		{
			var firstJobOperator = TestObjectCreator.CreateStaff("JO1");
			var secondJobOperator = TestObjectCreator.CreateStaff("JO2");
			Factory.Save();

			SetUpSingleJob(1, jobOperator: firstJobOperator);
			SetUpSingleJob(2, jobOperator: secondJobOperator);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), jobOperator: firstJobOperator);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), jobOperator: secondJobOperator);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestLocalClientFilter()
		{
			var firstLocalClient = TestObjectCreator.CreateOrgHeader("LC1", false, false);
			var secondLocalClient = TestObjectCreator.CreateOrgHeader("LC2", false, false);
			Factory.Save();

			SetUpSingleJob(1, localClient: firstLocalClient);
			SetUpSingleJob(2, localClient: secondLocalClient);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), localClient: firstLocalClient);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), localClient: secondLocalClient);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestOverseasAgentFilter()
		{
			var firstOverseasAgent = TestObjectCreator.CreateOrgHeader("OA1", false, false);
			var secondOverseasAgent = TestObjectCreator.CreateOrgHeader("OA2", false, false);
			Factory.Save();

			SetUpSingleJob(1, overseasAgent: firstOverseasAgent);
			SetUpSingleJob(2, overseasAgent: secondOverseasAgent);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), overseasAgent: firstOverseasAgent);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), overseasAgent: secondOverseasAgent);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestChargeCodeINFilter()
		{
			var firstChargeCode = TestObjectCreator.CC1;
			var secondChargeCode = TestObjectCreator.CC2;

			SetUpSingleJob(1, chargeCode: firstChargeCode);
			SetUpSingleJob(2, chargeCode: secondChargeCode);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), chargeCodeIN: firstChargeCode);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), chargeCodeIN: secondChargeCode);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestChargeCodeNOTINFilter()
		{
			var firstChargeCode = TestObjectCreator.CC1;
			var secondChargeCode = TestObjectCreator.CC2;

			SetUpSingleJob(1, chargeCode: firstChargeCode);
			SetUpSingleJob(2, chargeCode: secondChargeCode);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), chargeCodeNOTIN: secondChargeCode);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), chargeCodeNOTIN: firstChargeCode);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestChargeGroupFilter()
		{
			var firstChargeGroup = "CG1";
			var secondChargeGroup = "CG2";

			TestObjectCreator.CC1.AC_ChargeGroup = firstChargeGroup;
			TestObjectCreator.CC2.AC_ChargeGroup = secondChargeGroup;

			Factory.Save();

			SetUpSingleJob(1, chargeCode: TestObjectCreator.CC1);
			SetUpSingleJob(2, chargeCode: TestObjectCreator.CC2);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), chargeGroup: firstChargeGroup);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), chargeGroup: secondChargeGroup);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestSalesGroupFilter()
		{
			var firstSalesGroup = TestObjectCreator.CreateSalesGroup("SG1");
			var secondSalesGroup = TestObjectCreator.CreateSalesGroup("SG2");

			TestObjectCreator.CC1.AC_AR_SalesGroup = firstSalesGroup.PK;
			TestObjectCreator.CC2.AC_AR_SalesGroup = secondSalesGroup.PK;

			Factory.Save();

			SetUpSingleJob(1, chargeCode: TestObjectCreator.CC1);
			SetUpSingleJob(2, chargeCode: TestObjectCreator.CC2);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), salesGroup: firstSalesGroup);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), salesGroup: secondSalesGroup);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestExpenseGroupFilter()
		{
			var firstExpenseGroup = TestObjectCreator.CreateSalesGroup("EG1");  // There is no "CreateExpenseGroup" function and a sales group
																				// would do as well as an expense group here
			var secondExpenseGroup = TestObjectCreator.CreateSalesGroup("EG2");

			TestObjectCreator.CC1.AC_AR_ExpenseGroup = firstExpenseGroup.PK;
			TestObjectCreator.CC2.AC_AR_ExpenseGroup = secondExpenseGroup.PK;

			Factory.Save();

			SetUpSingleJob(1, chargeCode: TestObjectCreator.CC1);
			SetUpSingleJob(2, chargeCode: TestObjectCreator.CC2);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), expGroup: firstExpenseGroup);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), expGroup: secondExpenseGroup);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestControllingCustomerFilter()
		{
			var firstControllingCustomer = TestObjectCreator.CreateOrgHeader("CC1", false, false);

			var firstControllingCustomerManagementGroup = TestObjectCreator.CreateOrgHeader("CM1", false, false);

			TestObjectCreator.CreateRelatedParty(firstControllingCustomer, firstControllingCustomerManagementGroup, "MNG");

			var secondControllingCustomer = TestObjectCreator.CreateOrgHeader("CC2", false, false);

			var secondControllingCustomerManagementGroup = TestObjectCreator.CreateOrgHeader("CM2", false, false);

			TestObjectCreator.CreateRelatedParty(secondControllingCustomer, secondControllingCustomerManagementGroup, "MNG");

			Factory.Save();

			SetUpSingleJob(1, controllingCustomer: firstControllingCustomer, controllingCustomerManagementGroup: firstControllingCustomerManagementGroup);
			SetUpSingleJob(2, controllingCustomer: secondControllingCustomer, controllingCustomerManagementGroup: secondControllingCustomerManagementGroup);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), controllingCustomerPK: firstControllingCustomer.PK);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), controllingCustomerPK: secondControllingCustomer.PK);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestControllingCustomerManagementGroupFilter()
		{
			var firstControllingCustomer = TestObjectCreator.CreateOrgHeader("CC1", false, false);

			var firstControllingCustomerManagementGroup = TestObjectCreator.CreateOrgHeader("CM1", false, false);

			TestObjectCreator.CreateRelatedParty(firstControllingCustomer, firstControllingCustomerManagementGroup, "MNG");

			var secondControllingCustomer = TestObjectCreator.CreateOrgHeader("CC2", false, false);

			var secondControllingCustomerManagementGroup = TestObjectCreator.CreateOrgHeader("CM2", false, false);

			TestObjectCreator.CreateRelatedParty(secondControllingCustomer, secondControllingCustomerManagementGroup, "MNG");

			Factory.Save();

			SetUpSingleJob(1, controllingCustomer: firstControllingCustomer, controllingCustomerManagementGroup: firstControllingCustomerManagementGroup);
			SetUpSingleJob(2, controllingCustomer: secondControllingCustomer, controllingCustomerManagementGroup: secondControllingCustomerManagementGroup);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), controllingGroupPK: firstControllingCustomerManagementGroup.PK);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), controllingGroupPK: secondControllingCustomerManagementGroup.PK);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestOriginFilter()
		{
			var firstOrigin = "AUMEL";
			var secondOrigin = "USQQU";

			SetUpSingleJob(1, origin: firstOrigin);
			SetUpSingleJob(2, origin: secondOrigin);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), origin: firstOrigin);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), origin: secondOrigin);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestDestinationFilter()
		{
			var firstDestination = "NZAHU";
			var secondDestination = "USABB";

			SetUpSingleJob(1, destination: firstDestination);
			SetUpSingleJob(2, destination: secondDestination);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), destination: firstDestination);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), destination: secondDestination);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestRegistrationDateFilter()
		{
			var firstRegistrationDate = ZDateTime.Today.AddDays(-3);
			var secondRegistrationDate = ZDateTime.Today.AddDays(-6);

			SetUpSingleJob(1, registrationDate: firstRegistrationDate);
			SetUpSingleJob(2, registrationDate: secondRegistrationDate);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), regDateFrom: ZDateTime.Today.AddDays(-4), regDateTo: ZDateTime.Today.AddDays(-2));

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), regDateFrom: ZDateTime.Today.AddDays(-7), regDateTo: ZDateTime.Today.AddDays(-5));

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestOriginETDFilter()
		{
			SetUpSingleJob(1, originETD: ZDateTime.Today.AddDays(-1));
			SetUpSingleJob(2, originETD: ZDateTime.Today.AddDays(-5));

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), originETDFrom: ZDateTime.Today.AddDays(-2), originETDTo: ZDateTime.Today.AddDays(0));

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), originETDFrom: ZDateTime.Today.AddDays(-6), originETDTo: ZDateTime.Today.AddDays(-4));

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestDestETAFilter()
		{
			SetUpSingleJob(1, destETA: ZDateTime.Today.AddDays(15));    // Needs to be above 13, 
																		// since the test consols by default set the arrival date to today + 13
																		// And adding that consol automatically extends the destination date
			SetUpSingleJob(2, destETA: ZDateTime.Today.AddDays(20));

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), destinationETAFrom: ZDateTime.Today.AddDays(14), destinationETATo: ZDateTime.Today.AddDays(16));

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), destinationETAFrom: ZDateTime.Today.AddDays(19), destinationETATo: ZDateTime.Today.AddDays(21));

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestReceivingAgentFilter()
		{
			var firstReceivingAgent = TestObjectCreator.CreateOrgHeader("RA1", false, false);
			var secondReceivingAgent = TestObjectCreator.CreateOrgHeader("RA2", false, false);
			Factory.Save();

			SetUpSingleJob(1, receivingAgent: firstReceivingAgent);
			SetUpSingleJob(2, receivingAgent: secondReceivingAgent);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), receivingAgent: firstReceivingAgent);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), receivingAgent: secondReceivingAgent);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestSendingAgentFilter()
		{
			var firstSendingAgent = TestObjectCreator.CreateOrgHeader("SA1", false, false);
			var secondSendingAgent = TestObjectCreator.CreateOrgHeader("SA2", false, false);
			Factory.Save();

			SetUpSingleJob(1, sendingAgent: firstSendingAgent);
			SetUpSingleJob(2, sendingAgent: secondSendingAgent);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), sendingAgent: firstSendingAgent);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), sendingAgent: secondSendingAgent);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestCarrierFilter()
		{
			var firstCarrier = TestObjectCreator.CreateOrgHeader("CA1", false, false);
			var secondCarrier = TestObjectCreator.CreateOrgHeader("CA2", false, false);
			Factory.Save();

			SetUpSingleJob(1, carrier: firstCarrier);
			SetUpSingleJob(2, carrier: secondCarrier);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), carrier: firstCarrier);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), carrier: secondCarrier);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestCoLoaderFilter()
		{
			var firstColoader = TestObjectCreator.CreateOrgHeader("CO1", false, false);
			var secondColoader = TestObjectCreator.CreateOrgHeader("CO2", false, false);
			Factory.Save();

			SetUpSingleJob(1, coloader: firstColoader, resetPrepaidCollect: true);
			SetUpSingleJob(2, coloader: secondColoader, resetPrepaidCollect: true);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), coloader: firstColoader);

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), coloader: secondColoader);

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestFirstLoadPortFilter()
		{
			SetUpData(2);

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), firstLoadPort: "AUSYD");

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), firstLoadPort: "AFASH");

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestLastDischargePortFilter()
		{
			SetUpData(2);

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), lastDischargePort: "NZAKL");

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), lastDischargePort: "DZAZR");

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestFirstLoadETDFilter()
		{
			var firstLoadETD = ZDateTime.Today.AddDays(3);
			var secondLoadETD = ZDateTime.Today.AddDays(5);
			Factory.Save();

			SetUpSingleJob(1, firstLoadETD: firstLoadETD);
			SetUpSingleJob(2, firstLoadETD: secondLoadETD);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), loadETDFrom: firstLoadETD.AddHours(-3), loadETDTo: firstLoadETD.AddHours(3));

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), loadETDFrom: secondLoadETD.AddHours(-3), loadETDTo: secondLoadETD.AddHours(3));

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}

		[TestDate(2012, 09, 24)]
		public void TestLastDischargeETAFilter()
		{
			var firstLastDischargeETA = ZDateTime.Today.AddDays(3);
			var secondLastDischargeETA = ZDateTime.Today.AddDays(5);
			Factory.Save();

			SetUpSingleJob(1, lastDischargeETA: firstLastDischargeETA);
			SetUpSingleJob(2, lastDischargeETA: secondLastDischargeETA);

			SetUpData();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask(); // Processes AL Records
				dataPopulatingTask.RunTask(); // Populates Report table from the Queue table

				var dataTable1 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), dischargeETAFrom: firstLastDischargeETA.AddHours(-3), dischargeETATo: firstLastDischargeETA.AddHours(3));

				AssertEquals("Filtered result should have exactly one row (which should be job1)", 1, dataTable1.Rows.Count);
				AssertEqualEntry("Filter should have taken job1", "S000001", dataTable1, "JH_JobNum");

				var dataTable2 = RunScript(ZDateTime.Today.AddMonths(-50), ZDateTime.Today.AddMonths(50), dischargeETAFrom: secondLastDischargeETA.AddHours(-3), dischargeETATo: secondLastDischargeETA.AddHours(3));

				AssertEquals("Filtered result should have exactly one row (which should be job2)", 1, dataTable2.Rows.Count);
				AssertEqualEntry("Filter should have taken job2", "S000002", dataTable2, "JH_JobNum");
			}
		}
		#endregion

		#region Implementation

		#region helper functions
		void AssertEqualEntry(string message, string expected, DataTable data, string field)
		{
			var row = data.Rows[0];
			AssertEquals(message, expected, row[field]);
		}

		DataTable RunScript(ZDateTime fromRevenueRecognizedDate, ZDateTime toRevenueRecognizedDate, ZGuid? reportCurrency = null,
								OrgHeader debtorIN = null, OrgHeader debtorNOTIN = null, OrgHeader creditorIN = null, OrgHeader creditorNOTIN = null,

								GlbStaff salesRep = null, GlbStaff jobOperator = null, OrgHeader localClient = null, OrgHeader overseasAgent = null,

								AccChargeCode chargeCodeIN = null, AccChargeCode chargeCodeNOTIN = null, ZString? chargeGroup = null, AccGroups salesGroup = null, AccGroups expGroup = null,

								ZGuid? controllingCustomerPK = null, ZGuid? controllingGroupPK = null,

								ZString? origin = null, ZString? destination = null, ZDateTime? regDateFrom = null, ZDateTime? regDateTo = null,

								ZDateTime? originETDFrom = null, ZDateTime? originETDTo = null, ZDateTime? destinationETAFrom = null, ZDateTime? destinationETATo = null,

								ZString? firstLoadPort = null, ZString? lastDischargePort = null, OrgHeader receivingAgent = null, OrgHeader sendingAgent = null,
								OrgHeader carrier = null, OrgHeader coloader = null,

								ZDateTime? loadETDFrom = null, ZDateTime? loadETDTo = null, ZDateTime? dischargeETAFrom = null, ZDateTime? dischargeETATo = null
								)
		{
			var currency = reportCurrency ?? RefCurrency.LoadFromCurrencyCode(Factory, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency).PK;

			string sqlQuery = string.Format(@"
SELECT * FROM RptDt_Report_GlobalForwardingAndCustomsSummary
( 
	'{0}',					--@TransactionFrom  
	'{1}',					--@TransactionTo	
	'{2}',					--@ReportCurrency
	

	'{3}',					--@TransactionDebtor
	'{4}',					--@TransactionDebtorNOTIN
	'{5}',					--@TransactionCreditor
	'{6}',					--@TransactionCreditorNOTIN

	{7},					--@SalesRepPK
	{8},					--@OperatorPK
	{9},					--@LocalClientPK
	{10},					--@OverseasAgentPK


	'{11}',					--@ChargeCode  
	'{12}',					--@ChargeCodeNOTIN
	'{13}',					--@ChargeGroup
	 {14},					--@SalesGroup
	 {15},					--@ExpenseGroup

	@CCBListValue,			--@ControllingCustomerList
	@CCBListIsEmptyValue,	--@ControllingCustomerIsEmpty
	@MNGListValue,			--@ControllingCustomerGroup
	@MNGListIsEmptyValue,	--@ControllingCustomerGroupIsEmpty

	'{16}',					--@Origin
	'{17}',					--@Destination
	'{18}',					--@RegDateFrom
	'{19}',					--@RegDateTo

	'{20}',					--@OriginETDFrom
	'{21}',					--@OriginETDTo
	'{22}',					--@DestinationETAFrom
	'{23}',					--@DestinationETATo

	'{24}',					--@FirstLoadPort
	'{25}',					--@LastDischargePort
	{26},					--@ReceivingAgent
	{27},					--@SendingAgent
	{28},					--@Carrier
	{29},					--@CoLoader

	'{30}',					--@LoadETDFrom
	'{31}',					--@LoadETDTo
	'{32}',					--@DischargeETAFrom
	'{33}',					--@DischargeETATo
	'{34}'					--@CurrentCountry
)",
						GetMinDateTimeString(fromRevenueRecognizedDate),
						GetMaxDateTimeString(toRevenueRecognizedDate),
						currency,

						debtorIN == null ? "" : debtorIN.PK.ToString(),
						debtorNOTIN == null ? "" : debtorNOTIN.PK.ToString(),
						creditorIN == null ? "" : creditorIN.PK.ToString(),
						creditorNOTIN == null ? "" : creditorNOTIN.PK.ToString(),

						salesRep == null ? "NULL" : string.Format("'{0}'", salesRep.PK.ToString()),
						jobOperator == null ? "NULL" : string.Format("'{0}'", jobOperator.PK.ToString()),
						localClient == null ? "NULL" : string.Format("'{0}'", localClient.PK.ToString()),
						overseasAgent == null ? "NULL" : string.Format("'{0}'", overseasAgent.PK.ToString()),

						chargeCodeIN == null ? ZString.Empty : (ZString)chargeCodeIN.PK.ToString(),
						chargeCodeNOTIN == null ? ZString.Empty : (ZString)chargeCodeNOTIN.PK.ToString(),
						chargeGroup ?? "",
						salesGroup == null ? "NULL" : string.Format("'{0}'", salesGroup.PK.ToString()),
						expGroup == null ? "NULL" : string.Format("'{0}'", expGroup.PK.ToString()),

						origin ?? "",
						destination ?? "",
						GetMinDateTimeString(regDateFrom ?? ZDateTime.MinSmallDateTimeValue),
						GetMaxDateTimeString(regDateTo ?? ZDateTime.MaxSmallDateTimeValue),

						GetMinDateTimeString(originETDFrom ?? ZDateTime.MinSmallDateTimeValue),
						GetMaxDateTimeString(originETDTo ?? ZDateTime.MaxSmallDateTimeValue),
						GetMinDateTimeString(destinationETAFrom ?? ZDateTime.MinSmallDateTimeValue),
						GetMaxDateTimeString(destinationETATo ?? ZDateTime.MaxSmallDateTimeValue),

						firstLoadPort ?? "",
						lastDischargePort ?? "",
						receivingAgent == null ? "NULL" : string.Format("'{0}'", receivingAgent.PK.ToString()),
						sendingAgent == null ? "NULL" : string.Format("'{0}'", sendingAgent.PK.ToString()),
						carrier == null ? "NULL" : string.Format("'{0}'", carrier.PK.ToString()),
						coloader == null ? "NULL" : string.Format("'{0}'", coloader.PK.ToString()),

						GetMinDateTimeString(loadETDFrom ?? ZDateTime.MinSmallDateTimeValue),
						GetMaxDateTimeString(loadETDTo ?? ZDateTime.MaxSmallDateTimeValue),
						GetMinDateTimeString(dischargeETAFrom ?? ZDateTime.MinSmallDateTimeValue),
						GetMaxDateTimeString(dischargeETATo ?? ZDateTime.MaxSmallDateTimeValue),
						Env.CurrentCompany.Country.Code
						);

			var command = TestConnection.Command(sqlQuery);
			AddTVPAndIsEmptyParameters(command, "@MNGListValue", "dbo.TVP_uniqueidentifier", "@MNGListIsEmptyValue", controllingGroupPK.HasValue ? new Guid[] { controllingGroupPK.Value.ToGuid() } : Array.Empty<Guid>());
			AddTVPAndIsEmptyParameters(command, "@CCBListValue", "dbo.TVP_uniqueidentifier", "@CCBListIsEmptyValue", controllingCustomerPK.HasValue ? new Guid[] { controllingCustomerPK.Value.ToGuid() } : Array.Empty<Guid>());
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

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			var resetAction = new JCDResetActionStrategy(TestConnection, new TestServiceLogger());
			resetAction.Process();
			TestObjectCreator.CC1.AC_ChargeGroup = "AC1";
			TestObjectCreator.CC2.AC_ChargeGroup = "AC2";
			Factory.Save();
		}

		#region Test Data Setup

		// Sets up a single job in the database
		// Subsequent calls to SetUpSingleJob should use different values for jobNum
		// jobNum should only be a single digit otherwise they might result in some codes becoming too long
		void SetUpSingleJob(int jobNum, int consols = 1,
							int extraShipments = 0,
							ZDateTime? transactionRecognised = null,
							string origin = "AUMEL",
							string destination = "NZAHU",
							ZDateTime? originETD = null,
							ZDateTime? destETA = null,
							GlbStaff salesRep = null,
							GlbStaff jobOperator = null,
							OrgHeader localClient = null,
							OrgHeader overseasAgent = null,
							AccChargeCode chargeCode = null,
							ZString? chargeGroup = null,
							AccGroups salesGroup = null,
							AccGroups expGroup = null,
							OrgHeader controllingCustomer = null,
							OrgHeader controllingCustomerManagementGroup = null,
							ZDateTime? registrationDate = null,
							OrgHeader carrier = null,
							OrgHeader coloader = null,
							OrgHeader receivingAgent = null,
							OrgHeader sendingAgent = null,
							string firstLoadPort = "AUMEL",
							string lastDischargePort = "NZAHU",
							ZDateTime? firstLoadETD = null,
							ZDateTime? firstLoadATD = null,
							ZDateTime? lastDischargeETA = null,
							ZDateTime? lastDischargeATA = null,
							ForwardingConsol[] existingConsols = null,
							decimal costAmount = 1M,
							decimal sellAmount = 2M,
							GlbBranch branch = null,
							bool resetPrepaidCollect = false)
		{
			string jobNumStr = jobNum.ToString(); // So that we can use this number the codes to differentiate between different jobs

			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00000" + jobNumStr, origin: origin, destination: destination, transportMode: "TM" + jobNumStr);
			Factory.Save();

			shipment.JS_PackingMode = "CM" + jobNumStr;
			shipment.JS_HouseBill = "HB#" + jobNumStr;
			shipment.JS_HouseBillOfLadingType = "BT" + jobNumStr;

			shipment.JS_E_DEP = originETD ?? ZDateTime.Today.AddDays(-20);
			shipment.JS_E_ARV = destETA ?? ZDateTime.Today.AddDays(20);

			shipment.JS_ActualWeight = 10 + jobNum;
			shipment.JS_UnitOfWeight = "kg";
			shipment.JS_ActualVolume = 20 + jobNum;
			shipment.JS_UnitOfVolume = "m3";
			shipment.JS_ActualChargeable = 30 + jobNum;

			var consigneeImporter = TestObjectCreator.CreateOrgHeader("IMP" + jobNumStr, false, false);
			consigneeImporter.OH_FullName = "Importer " + jobNumStr;
			var address = TestObjectCreator.CreateAddress(consigneeImporter, OrgAddressType.Office, true);
			var docAddress = TestObjectCreator.CreateJobDocAddress(shipment.PK, DocAddressTypes.Codes.ConsigneeDocumentaryAddress, address.PK);
			docAddress.E2_ParentTableCode = "JS";

			var consignorShipperSupplier = TestObjectCreator.CreateOrgHeader("SHIP" + jobNumStr, false, false);
			consignorShipperSupplier.OH_FullName = "Supplier " + jobNumStr;
			address = TestObjectCreator.CreateAddress(consignorShipperSupplier, OrgAddressType.Office, true);
			docAddress = TestObjectCreator.CreateJobDocAddress(shipment.PK, DocAddressTypes.Codes.ConsignorDocumentaryAddress, address.PK);
			docAddress.E2_ParentTableCode = "JS";

			var otherShipments = new List<ForwardingShipment>();

			for (var i = 0; i < extraShipments; i++)
			{
				var newShipment = TestObjectCreator.CreateShipment("S00000" + jobNumStr + i.ToString(), origin: origin, destination: destination, transportMode: "TM" + jobNumStr);

				newShipment.JS_JS_ColoadMasterShipment = shipment.PK;

				Factory.Save();

				newShipment.JS_PackingMode = "CM" + jobNumStr;
				newShipment.JS_HouseBill = "HB#" + jobNumStr;
				newShipment.JS_HouseBillOfLadingType = "BT" + jobNumStr;

				newShipment.JS_E_DEP = originETD ?? ZDateTime.Today.AddDays(-20);
				newShipment.JS_E_ARV = destETA ?? ZDateTime.Today.AddDays(20);

				newShipment.JS_ActualWeight = 10 + jobNum;
				newShipment.JS_UnitOfWeight = "kg";
				newShipment.JS_ActualVolume = 20 + jobNum;
				newShipment.JS_UnitOfVolume = "m3";
				newShipment.JS_ActualChargeable = 30 + jobNum;

				otherShipments.Add(newShipment);
			}

			// Check if we already have consols to add to this shipment
			if (existingConsols != null)
			{
				// Add each of these consols onto the shipment
				foreach (var consol in existingConsols)
				{
					consol.Shipments.Add(shipment);

					Factory.Save();
				}
			}

			for (var i = 1; i <= consols; i++)
			{
				// We want at least one instance of different start and end cities, 
				// so we use a different set of start and end cities depending on whether this is the first consol or not
				string startCity;
				string endCity;

				if (i == 1 && jobNum == 1)
				{
					startCity = "AUSYD";
					endCity = "NZAKL";
				}
				else
				{
					startCity = "AFASH";
					endCity = "DZAZR";
				}

				var consol = CreateNewConsol(startCity, endCity, i, jobNumStr,
												carrier: carrier, coLoader: coloader,
												receivingAgent: receivingAgent, sendingAgent: sendingAgent,
												firstLoadETD: firstLoadETD, firstLoadATD: firstLoadATD,
												lastDischargeETA: lastDischargeETA, lastDischargeATA: lastDischargeATA, resetPrepaidCollect);
				consol.Shipments.Add(shipment);

				foreach (var otherShipment in otherShipments)
				{
					consol.Shipments.Add(otherShipment);
				}

				Factory.Save();
			}

			Factory.Save();

			var job = TestObjectCreator.CreateJob(parent: shipment, createWithMutex: false, localClientOrg: localClient, agentOrg: overseasAgent);
			job.JH_GS_NKRepSales = (salesRep ?? TestObjectCreator.CreateStaff("SR" + jobNum)).GS_Code;
			job.JH_GS_NKRepOps = (jobOperator ?? TestObjectCreator.CreateStaff("OP" + jobNum)).GS_Code;

			job.JH_SystemCreateTimeUtc = (registrationDate ?? ZDateTime.Today);

			if (controllingCustomer == null)
			{
				controllingCustomer = TestObjectCreator.CreateOrgHeader("CC" + jobNumStr, false, false);

				controllingCustomerManagementGroup = TestObjectCreator.CreateOrgHeader("CM" + jobNumStr, false, false);

				TestObjectCreator.CreateRelatedParty(controllingCustomer, controllingCustomerManagementGroup, "MNG");
			}

			TestObjectCreator.CreateJobDocAddress(job.JH_ParentID, "SCP", (controllingCustomer.MainAddress.PK));

			Factory.Save();

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			if (branch == null)
			{
				var testCompany = TestObjectCreator.NonCurrentCompany;
				testCompany.GC_RX_NKLocalCurrency = "AUD";

				branch = TestObjectCreator.NonCurrentCompanyBranch;

				Factory.Save();
			}

			Factory.Save();

			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);

			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			using (SetupRevenueRecognition(job))
			{
				job.JH_A_JOP = transactionRecognised ?? ZDateTime.Today.AddDays(-1);

				if (chargeCode == null)
				{
					chargeCode = TestObjectCreator.CC1;
				}

				job.JH_GC = chargeCode.AC_GC;

				if (jobNum == 1)
				{
					TestObjectCreator.CreateCharge(job, chargeCode, string.Empty, TestObjectCreator.AUD, costAmount, TestObjectCreator.Creditor1, TestObjectCreator.AUD, sellAmount, TestObjectCreator.Debtor);
					TestObjectCreator.CreateCharge(job, chargeCode, string.Empty, TestObjectCreator.AUD, 0M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, TestObjectCreator.Debtor);
				}
				else
				{
					TestObjectCreator.CreateCharge(job, chargeCode, string.Empty, TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor2, TestObjectCreator.AUD, 0, TestObjectCreator.ABIGAS);
					TestObjectCreator.CreateCharge(job, chargeCode, string.Empty, TestObjectCreator.AUD, 2M, TestObjectCreator.Creditor2, TestObjectCreator.AUD, 4M, TestObjectCreator.ABIGAS);
				}

				Factory.Save();
			}

			Factory.Save();
		}

		ForwardingConsol CreateNewConsol(string startCity, string endCity, int num, string jobNumStr,
											OrgHeader carrier = null, OrgHeader coLoader = null,
											OrgHeader receivingAgent = null, OrgHeader sendingAgent = null,
											ZDateTime? firstLoadETD = null, ZDateTime? firstLoadATD = null,
											ZDateTime? lastDischargeETA = null, ZDateTime? lastDischargeATA = null, bool resetPrepaidCollect = false)
		{
			var consolNumStr = num.ToString();
			var consol = TestObjectCreator.CreateConsol(startCity, endCity, "C00" + jobNumStr + consolNumStr);

			if (resetPrepaidCollect)
			{
				consol.JK_PrepaidCollect = "";
			}

			// Set all of the departure and arrival times at this consol
			// Setting them on the first transport as that is what TestObjectCreator.CreateConsol does
			if (firstLoadETD != null)
			{
				consol.Transports[0].JW_ETD = firstLoadETD.Value;
			}

			if (firstLoadATD != null)
			{
				consol.Transports[0].JW_ATD = firstLoadATD.Value;
			}

			if (lastDischargeETA != null)
			{
				consol.Transports[0].JW_ETA = lastDischargeETA.Value;
			}

			if (lastDischargeATA != null)
			{
				consol.Transports[0].JW_ATA = lastDischargeATA.Value;
			}

			consol.JK_MasterBillNum = "DMB" + jobNumStr + consolNumStr;
			Factory.Save();

			consol.JK_AgentType = "A" + jobNumStr + consolNumStr;
			consol.JK_TransportMode = "T" + jobNumStr + consolNumStr;

			if (carrier == null)
			{
				carrier = TestObjectCreator.CreateOrgHeader("SL" + jobNumStr + consolNumStr, false, true);  // NOTE: CreateOrgHeader prepends a 'Z' onto the start of the code, 
																											// so the actual code for this would be "ZSL1", and similar for all the other org headers
			}
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.EntityPK;

			if (coLoader == null)
			{
				coLoader = TestObjectCreator.CreateOrgHeader("CR" + jobNumStr + consolNumStr, true, false);
			}
			consol.JK_OA_CreditorAddress = coLoader.MainAddress.EntityPK;

			if (sendingAgent == null)
			{
				sendingAgent = TestObjectCreator.CreateOrgHeader("SA" + jobNumStr + consolNumStr, false, false);
			}

			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.EntityPK;

			if (receivingAgent == null)
			{
				receivingAgent = TestObjectCreator.CreateOrgHeader("RA" + jobNumStr + consolNumStr, false, false);
			}

			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.EntityPK;

			consol.JK_MasterBillNum = "MB" + jobNumStr + consolNumStr;

			Factory.Save();

			return consol;
		}

		void SetUpData(int numJobs = 0)
		{
			// Set up each individual job

			//Start looping from 1 since that makes it easier to read and understand what the test results should be
			for (var i = 1; i <= numJobs; i++)
			{
				SetUpSingleJob(i);
			}

			// Now run the JCD service task
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
