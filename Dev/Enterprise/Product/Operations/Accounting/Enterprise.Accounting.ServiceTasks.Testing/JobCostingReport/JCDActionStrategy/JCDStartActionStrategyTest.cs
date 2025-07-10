using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(JobCostingDataPopulationServiceTask))]
	public class JCDStartActionStrategyTest : JCDActionStrategyTestBase
	{
		/*
		 * Scenario#	Description
		 *		1		No JCD related DB object exist in Database
		 *		2		One or more JCD related DB Object exist in Database but JCD service task controller value is 'STR'
		 *		3		JCD service task controller value is 'CDB'
		 *		4		JCD service task controller value is 'CDB' and Temp tables have incomplete data possibly due to a DB level exception.
		*/
		public void TestStartAction_Scenario1()
		{
			//Actual test begins here
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			#region creating AL Records

			var jobs = Helper.CreateJobsWithPeriod(false);

			var apLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var apLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var arLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: false, reverseACR: false);

			var allPKs = new[]
			{
					apLine1.PK, apLine2.PK, arLine1.PK, arLine2.PK, wipacr1.Item1.PK, wipacr1.Item2.PK, wipacr2.Item1.PK,
					wipacr2.Item2.PK
				};

			#endregion

			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				#region Run Data Population Service Task and Assert 

				//Now run the Service Task

				var log = InitialiseAndRunTaskSchedule(task);
				var expectedLogMessages = @"
Debug|Intializing Job Costing Data Queue service task
Debug|Checking whether action can be performed
Debug|Starting DB Object Synchronization.
Debug|Creating Job Costing Data Queue service task related tables and partition
Debug|Job Costing Data Queue service task related tables and partition have been created successfully
Debug|Dropping PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord
Debug|Dropping TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable
Debug|Creating TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable
Debug|Creating PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord
Information|Temporary DB Objects are up-to-date.
Debug|Dropping FUNCTION RptDt_Report_GlobalForwardingAndCustomsSummary
Debug|Dropping FUNCTION RptDt_Report_GlobalJobProfitSummaryByJob
Debug|Dropping PROCEDURE RptDtPopulateJobCostingDataFromQueue
Debug|Dropping TRIGGER RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue
Debug|Dropping FUNCTION RptDt_GlobalJobProfitReportCore
Debug|Dropping FUNCTION RptDt_GetPeriodKeys
Debug|Dropping INDEX RptDt_NX_JCA_OH_JCA_GC_JCA_JH
Debug|Dropping INDEX RptDt_NX_JCA_JH_JCA_GC_JCA_OH
Debug|Dropping INSERT INTO RptDtJobCostingDataAmountByJob
Debug|Dropping UNIQUE CLUSTERED INDEX RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH
Debug|Dropping UNIQUE CLUSTERED INDEX RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH_JCA_RX_NKLocalCurrency
Debug|Dropping TABLE RptDtJobCostingDataAmountByJob
Debug|Dropping INDEX RptDt_NI_JCD_OH_JCD_GC_JCD_JH
Debug|Dropping INDEX RptDt_NI_JCD_JH_JCD_GC_JCD_OH
Debug|Dropping UNIQUE CLUSTERED INDEX RptDt_CI_JCD_PostPeriod_JCD_GC_JCD_OH_JCD_JH
Debug|Dropping VIEW RptDt_ViewJobCostingDataAmountByJob
Debug|Creating TABLE RptDtJobCostingDataAmountByJob
Debug|Creating UNIQUE CLUSTERED INDEX RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH_JCA_RX_NKLocalCurrency
Debug|Creating INSERT INTO RptDtJobCostingDataAmountByJob
Debug|Creating INDEX RptDt_NX_JCA_JH_JCA_GC_JCA_OH
Debug|Creating INDEX RptDt_NX_JCA_OH_JCA_GC_JCA_JH
Debug|Creating FUNCTION RptDt_GetPeriodKeys
Debug|Creating FUNCTION RptDt_GlobalJobProfitReportCore
Debug|Creating TRIGGER RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue
Debug|Creating PROCEDURE RptDtPopulateJobCostingDataFromQueue
Debug|Creating FUNCTION RptDt_Report_GlobalJobProfitSummaryByJob
Debug|Creating FUNCTION RptDt_Report_GlobalForwardingAndCustomsSummary
Information|Functions, Stored Procedures and Dependent Tables are up-to-date.
Debug|DB Object Synchronization has been completed.
Debug|Getting AccTransactionLines that need to be transformed for JobCosting Reports.
Debug|Temporary tables have been populated successfully
Debug|Service Task has been nudged successfully
".Trim();
				AssertMultilineASCIIEquals("Log Messages", expectedLogMessages, log.ToString());
				AssertDBObjectExist(true);

				var tempTableData = Helper.LoadDataFromRptDtUnprocessedAccTransactionLinesTable();

				AssertEquals("Number of Rows", 8, tempTableData.Rows.Count);

				AssertEquals("Rows 1: UL_RowNumber", 1, Convert.ToInt32(tempTableData.Rows[0]["UL_RowNumber"]));
				AssertEquals("Rows 1: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[0]["UL_ALPK"])));

				AssertEquals("Rows 2: UL_RowNumber", 2, Convert.ToInt32(tempTableData.Rows[1]["UL_RowNumber"]));
				AssertEquals("Rows 2: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[1]["UL_ALPK"])));

				AssertEquals("Rows 3: UL_RowNumber", 3, Convert.ToInt32(tempTableData.Rows[2]["UL_RowNumber"]));
				AssertEquals("Rows 3: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[2]["UL_ALPK"])));

				AssertEquals("Rows 4: UL_RowNumber", 4, Convert.ToInt32(tempTableData.Rows[3]["UL_RowNumber"]));
				AssertEquals("Rows 4: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[3]["UL_ALPK"])));

				AssertEquals("Rows 5: UL_RowNumber", 5, Convert.ToInt32(tempTableData.Rows[4]["UL_RowNumber"]));
				AssertEquals("Rows 5: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[4]["UL_ALPK"])));

				AssertEquals("Rows 6: UL_RowNumber", 6, Convert.ToInt32(tempTableData.Rows[5]["UL_RowNumber"]));
				AssertEquals("Rows 6: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[5]["UL_ALPK"])));

				AssertEquals("Rows 7: UL_RowNumber", 7, Convert.ToInt32(tempTableData.Rows[6]["UL_RowNumber"]));
				AssertEquals("Rows 7: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[6]["UL_ALPK"])));

				AssertEquals("Rows 8: UL_RowNumber", 8, Convert.ToInt32(tempTableData.Rows[7]["UL_RowNumber"]));
				AssertEquals("Rows 8: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[7]["UL_ALPK"])));

				AssertEquals("Registry Status", JCDActionList.Codes.ProcessingOldTransactionLines, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

				#endregion

				using (Env.Instance.TemporaryServiceTaskContext(JobCostingDataPopulationServiceTask.Code, canRunInAnyBranch: true))
				{
					task.RunTask();
				}
			}
		}

		public void TestStartAction_Scenario2()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.NotInitialized);

			CreateEnvironment_Scenario2();

			#region Creating AL Records

			var jobs = Helper.CreateJobsWithPeriod(false);

			var apLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var apLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var arLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: false, reverseACR: false);

			#endregion

			var log = new TestServiceLogger();
			var strategy = new JCDStartActionStrategy(TestConnection, log);
			strategy.Process();

			AssertEquals("Log Message", "Debug|Checking whether action can be performed", log[0]);
			AssertMultilineASCIIEquals("Log Message", @"Error|Job Costing Data Queue service task is already initialized. So cannot be initialized again. If you want to start from fresh, please choose 'RST' action instead.
 Job Costing Data Queue Service Task Controller Registry Value: NON
 Missing DB Object Names: NR_RC__Clustered_UL_RowNumber, RptDtUnprocessedReversedAL, FK_UC__Clustered_URL_ALPK, RptDt_TG_AccTransactionLines_InsertToReversedLinesTable, RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue, RptDtTransformAccTransactionLineToJobCostingQueueRecord, PF_AccountingPeriodCompany, PS_AccountingPeriodCompany, RptDtJobCostingData, CI_JCD_PostDate, NCI_JCD_OH, RptDtJobCostingDataAmountByJob, RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH_JCA_RX_NKLocalCurrency, RptDt_NX_JCA_JH_JCA_GC_JCA_OH, RptDt_NX_JCA_OH_JCA_GC_JCA_JH, RptDtPopulateJobCostingDataFromQueue, RptDt_GetPeriodKeys, RptDt_GlobalJobProfitReportCore, RptDt_Report_GlobalJobProfitSummaryByJob, RptDt_Report_GlobalForwardingAndCustomsSummary", log[1]);

			var tempTableData = Helper.LoadDataFromRptDtUnprocessedAccTransactionLinesTable();
			AssertEquals("Number of Rows", 0, tempTableData.Rows.Count);

			AssertEquals("Registry Status", JCDActionList.Codes.NotInitialized, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);
		}

		void CreateEnvironment_Scenario2()
		{
			//Creating an Environment where few JCD related DB objects exist in DB.
			Helper.Connection.ExecuteNonQuery(@"IF OBJECT_ID(N'RptDtUnprocessedAccTransactionLines') IS NULL
												BEGIN
													CREATE TABLE RptDtUnprocessedAccTransactionLines (UL_RowNumber bigint IDENTITY(1,1) NOT NULL, UL_ALPK uniqueidentifier NOT NULL)
												END");
		}

		[TestDate(2023, 10, 1)]
		public void TestStartAction_Scenario3()
		{
			CreateEnvrionemnt_scenario3();

			#region Creating AL Records

			var jobs = Helper.CreateJobsWithPeriod(false);

			var apLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var apLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var arLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: false, reverseACR: false);

			var allPKs = new[]
			{
					apLine1.PK, apLine2.PK, arLine1.PK, arLine2.PK, wipacr1.Item1.PK, wipacr1.Item2.PK, wipacr2.Item1.PK,
					wipacr2.Item2.PK
			};

			TestConnection.ExecuteNonQuery("DELETE FROM dbo.JobCostingDataQueue");
			#endregion

			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				#region Run Data Population Service Task and Assert

				//Run the Service Task
				var log = InitialiseAndRunTaskSchedule(task);
				var expectedLogMessages = @"
Debug|Intializing Job Costing Data Queue service task
Debug|Checking whether action can be performed
Debug|Starting DB Object Synchronization.
Debug|Temporary DB Objects are up-to-date.
Debug|Functions, Stored Procedures and Dependent Tables are up-to-date.
Debug|DB Object Synchronization has been completed.
Debug|Creating Partition 20220122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220322C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220422C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220522C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220622C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220722C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220822C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220922C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20221022C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20221122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20221222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230322C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230422C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230522C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230622C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230722C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230822C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230922C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20231022C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20231122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20231222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240322C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240422C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240522C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240622C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240722C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240822C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240922C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20241022C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20241122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20241222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 202201878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202202878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202203878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202204878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202205878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202206878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202207878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202208878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202209878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202210878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202211878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202212878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202301878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202302878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202303878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202304878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202305878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202306878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202307878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202308878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202309878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202310878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202311878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202312878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202401878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202402878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202403878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202404878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202405878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202406878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202407878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202408878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202409878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202410878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202411878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202412878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Information|Partition keys are in-sync with accounting periods.
Debug|Getting AccTransactionLines that need to be transformed for JobCosting Reports.
Debug|Temporary tables have been populated successfully
Debug|Service Task has been nudged successfully
".Trim();
				AssertMultilineASCIIEquals("Log Messages", expectedLogMessages, log.ToString());
				AssertDBObjectExist(true);

				var tempTableData = Helper.LoadDataFromRptDtUnprocessedAccTransactionLinesTable();

				AssertEquals("Number of Rows", 8, tempTableData.Rows.Count);

				AssertEquals("Rows 1: UL_RowNumber", 1, Convert.ToInt32(tempTableData.Rows[0]["UL_RowNumber"]));
				AssertEquals("Rows 1: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[0]["UL_ALPK"])));

				AssertEquals("Rows 2: UL_RowNumber", 2, Convert.ToInt32(tempTableData.Rows[1]["UL_RowNumber"]));
				AssertEquals("Rows 2: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[1]["UL_ALPK"])));

				AssertEquals("Rows 3: UL_RowNumber", 3, Convert.ToInt32(tempTableData.Rows[2]["UL_RowNumber"]));
				AssertEquals("Rows 3: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[2]["UL_ALPK"])));

				AssertEquals("Rows 4: UL_RowNumber", 4, Convert.ToInt32(tempTableData.Rows[3]["UL_RowNumber"]));
				AssertEquals("Rows 4: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[3]["UL_ALPK"])));

				AssertEquals("Rows 5: UL_RowNumber", 5, Convert.ToInt32(tempTableData.Rows[4]["UL_RowNumber"]));
				AssertEquals("Rows 5: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[4]["UL_ALPK"])));

				AssertEquals("Rows 6: UL_RowNumber", 6, Convert.ToInt32(tempTableData.Rows[5]["UL_RowNumber"]));
				AssertEquals("Rows 6: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[5]["UL_ALPK"])));

				AssertEquals("Rows 7: UL_RowNumber", 7, Convert.ToInt32(tempTableData.Rows[6]["UL_RowNumber"]));
				AssertEquals("Rows 7: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[6]["UL_ALPK"])));

				AssertEquals("Rows 8: UL_RowNumber", 8, Convert.ToInt32(tempTableData.Rows[7]["UL_RowNumber"]));
				AssertEquals("Rows 8: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[7]["UL_ALPK"])));

				AssertEquals("Registry Status", JCDActionList.Codes.ProcessingOldTransactionLines, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

				#endregion
			}
		}

		void CreateEnvrionemnt_scenario3()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.CompletedAllDatabaseObjectsForJobCostingDataQueueHaveBeenCreated);
		}

		public void TestIsReportTableIsTrue_RptDtJobCostingDataHasData_CDB()
		{
			var dBObjectChecker = new JCDDBObjectsChecker(TestConnection);
			CreateEnvrionemnt_scenario3();

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.RptDtJobCostingData
	( JCD_AL, JCD_AH, JCD_JH, JCD_AC, JCD_AG, JCD_OH, JCD_GE, JCD_GB, JCD_GC, JCD_LineType, JCD_Desc, JCD_PostDate, JCD_PostPeriod, JCD_RevRecognitionType, JCD_LineAmount, JCD_GSTVAT, JCD_RX_NKLocalCurrency, JCD_OSAmount, JCD_RX_NKCurrency,JCD_TransactionNum, JCD_TransactionType, JCD_Ledger, JCD_ParentID, JCD_ParentTableCode, JCD_PeriodCompanyKey )
	VALUES
	( NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), 'CST', 'TST Line', '12 DEC 2020', 202001, 'IMM', 250, 25, 'AUD', 250, 'AUD', 'I102589', 'INV', 'AR', NEWID(), 'SH', 'NOKEY')");

			AssertEquals("After insert data into RptDtJobCostingData", false, dBObjectChecker.IsReportTableEmpty());

			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			AssertEquals("After STR", true, dBObjectChecker.IsReportTableEmpty());
			AssertEquals("After STR registry value", JCDActionList.Codes.ProcessingOldTransactionLines, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		[TestDate(2023, 10, 1)]
		public void TestStartAction_Scenario4()
		{
			var allPKs = CreateEnvrionemnt_scenario4();

			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				#region Run Data Population Service Task and Assert

				//Run the Service Task
				var log = InitialiseAndRunTaskSchedule(task);
				var expectedLogMessages = @"
Debug|Intializing Job Costing Data Queue service task
Debug|Checking whether action can be performed
Debug|Starting DB Object Synchronization.
Debug|Temporary DB Objects are up-to-date.
Debug|Functions, Stored Procedures and Dependent Tables are up-to-date.
Debug|DB Object Synchronization has been completed.
Debug|Creating Partition 20220122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220322C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220422C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220522C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220622C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220722C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220822C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20220922C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20221022C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20221122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20221222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230322C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230422C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230522C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230622C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230722C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230822C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20230922C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20231022C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20231122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20231222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240322C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240422C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240522C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240622C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240722C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240822C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20240922C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20241022C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20241122C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 20241222C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061
Debug|Partition is created successfully
Debug|Creating Partition 202201878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202202878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202203878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202204878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202205878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202206878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202207878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202208878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202209878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202210878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202211878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202212878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202301878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202302878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202303878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202304878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202305878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202306878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202307878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202308878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202309878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202310878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202311878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202312878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202401878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202402878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202403878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202404878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202405878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202406878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202407878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202408878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202409878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202410878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202411878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Debug|Creating Partition 202412878D7ACA-FFC3-49FC-9710-969CA0C0F2AC
Debug|Partition is created successfully
Information|Partition keys are in-sync with accounting periods.
Debug|Getting AccTransactionLines that need to be transformed for JobCosting Reports.
Debug|Temporary tables have been populated successfully
Debug|Service Task has been nudged successfully
".Trim();
				AssertMultilineASCIIEquals("Log Messages", expectedLogMessages, log.ToString());
				AssertDBObjectExist(true);

				var tempTableData = Helper.LoadDataFromRptDtUnprocessedAccTransactionLinesTable();

				AssertEquals("Number of Rows", 8, tempTableData.Rows.Count);

				AssertEquals("Rows 1: UL_RowNumber", 1, Convert.ToInt32(tempTableData.Rows[0]["UL_RowNumber"]));
				AssertEquals("Rows 1: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[0]["UL_ALPK"])));

				AssertEquals("Rows 2: UL_RowNumber", 2, Convert.ToInt32(tempTableData.Rows[1]["UL_RowNumber"]));
				AssertEquals("Rows 2: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[1]["UL_ALPK"])));

				AssertEquals("Rows 3: UL_RowNumber", 3, Convert.ToInt32(tempTableData.Rows[2]["UL_RowNumber"]));
				AssertEquals("Rows 3: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[2]["UL_ALPK"])));

				AssertEquals("Rows 4: UL_RowNumber", 4, Convert.ToInt32(tempTableData.Rows[3]["UL_RowNumber"]));
				AssertEquals("Rows 4: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[3]["UL_ALPK"])));

				AssertEquals("Rows 5: UL_RowNumber", 5, Convert.ToInt32(tempTableData.Rows[4]["UL_RowNumber"]));
				AssertEquals("Rows 5: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[4]["UL_ALPK"])));

				AssertEquals("Rows 6: UL_RowNumber", 6, Convert.ToInt32(tempTableData.Rows[5]["UL_RowNumber"]));
				AssertEquals("Rows 6: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[5]["UL_ALPK"])));

				AssertEquals("Rows 7: UL_RowNumber", 7, Convert.ToInt32(tempTableData.Rows[6]["UL_RowNumber"]));
				AssertEquals("Rows 7: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[6]["UL_ALPK"])));

				AssertEquals("Rows 8: UL_RowNumber", 8, Convert.ToInt32(tempTableData.Rows[7]["UL_RowNumber"]));
				AssertEquals("Rows 8: UL_ALPK", true, allPKs.Contains(new ZGuid(tempTableData.Rows[7]["UL_ALPK"])));

				AssertEquals("Registry Status", JCDActionList.Codes.ProcessingOldTransactionLines, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

				#endregion
			}
		}

		ZGuid[] CreateEnvrionemnt_scenario4()
		{
			var deleteAction = new JCDDeleteActionStrategy(Helper.Connection, new TestServiceLogger());
			deleteAction.Process();
			AssertEquals("Registry Status", JCDActionList.Codes.NotInitialized, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.CompletedAllDatabaseObjectsForJobCostingDataQueueHaveBeenCreated);

			#region Creating AL Records

			var jobs = Helper.CreateJobsWithPeriod(false);

			var apLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var apLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var arLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: false, reverseACR: false);

			var allPKs = new[]
			{
				apLine1.PK, apLine2.PK, arLine1.PK, arLine2.PK, wipacr1.Item1.PK, wipacr1.Item2.PK, wipacr2.Item1.PK, wipacr2.Item2.PK
			};

			Helper.Connection.ExecuteNonQuery("DELETE FROM dbo.JobCostingDataQueue");

			#endregion

			return allPKs;
		}

		#region Queue Population related Tests

		//	
		//	TTP = RptDtUnprocessedAccTransactionLines Population process
		//	UToQ = Populating JobCostingDataQueue table from RptDtUnprocessedAccTransactionLines table 
		//
		//	Scenario #	|	Post Date is filled in		|	Reverse Date is filled in
		//-------------------------------------------------------------------------------
		//		1		|		Before TTP and UToQ		|		Before TTP and UToQ
		//		2		|		Before TTP and UToQ		|		After TTP and Before UToQ
		//		3		|		Before TTP and UToQ		|		After TTP and UToQ
		//		4		|		After TTP and UToQ		|		After TTP and UToQ
		//

		public void TestJobCostingQueueTablePopulation_Scenario1()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			#region Data Setup for scenario #1

			//Creating AL records
			var jobs = Helper.CreateJobsWithPeriod();

			var aplLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var aplLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arlLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var arlLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: true, reverseACR: true);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: true, reverseACR: true);

			//Populating RptDtUnprocessedAccTransactionLines table by Start action
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 4, explanation: JCDDBObjectCheckerExplanationForSTR))
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			AssertDBObjectExist(true);
			AssertEquals("Registry Status", JCDActionList.Codes.ProcessingOldTransactionLines, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			var expectedALs = new ZGuid[]
			{
				aplLine1.PK, aplLine2.PK, arlLine1.PK, arlLine2.PK, wipacr1.Item1.PK, wipacr1.Item2.PK, wipacr2.Item1.PK, wipacr2.Item2.PK, Helper.narline.PK
			};
			AssertTempALPKTableData(expectedALs, Array.Empty<ZGuid>());
			AssertQueueTableData(new List<Tuple<Guid, DateTime, DateTime, Guid>>());

			#endregion

			//Populating Queue table (UToQ)
			using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 2, explanation: JCDDBObjectCheckerExplanationForPOR))
			{
				var populateQueue = new JCDDefaultActionStrategy_ProcessOldRecordOnly_ForTest(Helper.Connection, new TestServiceLogger());
				populateQueue.Process();
			}

			AssertEquals("Registry Status", JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);
			var expectedQueuedValues = new List<Tuple<Guid, DateTime, DateTime, Guid>>()
			{
				Helper.CreateTuppleForAssertion(aplLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(aplLine2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr1.Item1, setPostdateToNull: false, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr1.Item2, setPostdateToNull: false, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item1, setPostdateToNull: false, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item2, setPostdateToNull: false, setReverseDateToNull: false)
			};
			AssertTempDBObjectExist(false);
			AssertQueueTableData(expectedQueuedValues);
		}

		public void TestJobCostingQueueTablePopulation_Scenario2()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			#region Data Setup for scenario #2

			//Creating AL records
			var jobs = Helper.CreateJobsWithPeriod();

			var aplLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: false);
			var aplLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arlLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: false);
			var arlLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseACR: false, reverseWIP: false);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseACR: true, reverseWIP: true);

			//Populating RptDtUnprocessedAccTransactionLines table by Start action
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 4, explanation: JCDDBObjectCheckerExplanationForSTR))
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			AssertDBObjectExist(true);
			AssertEquals("Registry Status", JCDActionList.Codes.ProcessingOldTransactionLines, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			//Reversing Lines after TTP but before UToQ
			Helper.Recognize(jobs[0]);

			var expectedALs = new ZGuid[] { aplLine1.PK, aplLine2.PK, arlLine1.PK, arlLine2.PK, wipacr1.Item1.PK, wipacr1.Item2.PK, wipacr2.Item1.PK, wipacr2.Item2.PK, Helper.narline.PK };
			var expectedReversedALs = new ZGuid[] { aplLine1.PK, arlLine1.PK, wipacr1.Item1.PK, wipacr1.Item2.PK };
			var expectedQueuedValues = new List<Tuple<Guid, DateTime, DateTime, Guid>>()
			{
				Helper.CreateTuppleForAssertion(aplLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr1.Item1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr1.Item2, setPostdateToNull: true, setReverseDateToNull: false)
			};
			AssertTempALPKTableData(expectedALs, expectedReversedALs);
			AssertQueueTableData(expectedQueuedValues);

			#endregion

			//Populating Queue table (UToQ)
			using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 2, explanation: JCDDBObjectCheckerExplanationForPOR))
			{
				var populateQueue = new JCDDefaultActionStrategy_ProcessOldRecordOnly_ForTest(Helper.Connection, new TestServiceLogger());
				populateQueue.Process();
			}
			AssertEquals("Registry Status", JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			expectedQueuedValues = new List<Tuple<Guid, DateTime, DateTime, Guid>>()
			{
				Helper.CreateTuppleForAssertion(aplLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr1.Item1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr1.Item2, setPostdateToNull: true, setReverseDateToNull: false),

				Helper.CreateTuppleForAssertion(wipacr1.Item1, setPostdateToNull: false, setReverseDateToNull: true),
				Helper.CreateTuppleForAssertion(wipacr1.Item2, setPostdateToNull: false, setReverseDateToNull: true),

				Helper.CreateTuppleForAssertion(aplLine2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item1, setPostdateToNull: false, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item2, setPostdateToNull: false, setReverseDateToNull: false)
			};

			AssertTempDBObjectExist(false);
			AssertQueueTableData(expectedQueuedValues);
		}

		public void TestJobCostingQueueTablePopulation_Scenario3()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			//Creating AL records
			var jobs = Helper.CreateJobsWithPeriod();

			var aplLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: false);
			var aplLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arlLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: false);
			var arlLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseACR: false, reverseWIP: false);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseACR: true, reverseWIP: true);

			//Populating RptDtUnprocessedAccTransactionLines table by Start action
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 4, explanation: JCDDBObjectCheckerExplanationForSTR))
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			AssertDBObjectExist(true);
			AssertEquals("Registry Status", JCDActionList.Codes.ProcessingOldTransactionLines, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			var expectedALs = new ZGuid[] { aplLine1.PK, aplLine2.PK, arlLine1.PK, arlLine2.PK, wipacr1.Item1.PK, wipacr1.Item2.PK, wipacr2.Item1.PK, wipacr2.Item2.PK, Helper.narline.PK };
			AssertTempALPKTableData(expectedALs, Array.Empty<ZGuid>());
			AssertQueueTableData(new List<Tuple<Guid, DateTime, DateTime, Guid>>());

			//Populating Queue table (UToQ)
			using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 2, explanation: JCDDBObjectCheckerExplanationForPOR))
			{
				var populateQueue = new JCDDefaultActionStrategy_ProcessOldRecordOnly_ForTest(Helper.Connection, new TestServiceLogger());
				populateQueue.Process();
			}

			AssertEquals("Registry Status", JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			var expectedQueuedValues = new List<Tuple<Guid, DateTime, DateTime, Guid>>()
			{
				Helper.CreateTuppleForAssertion(wipacr1.Item1, setPostdateToNull: false, setReverseDateToNull: true),
				Helper.CreateTuppleForAssertion(wipacr1.Item2, setPostdateToNull: false, setReverseDateToNull: true),

				Helper.CreateTuppleForAssertion(aplLine2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item1, setPostdateToNull: false, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item2, setPostdateToNull: false, setReverseDateToNull: false),
			};
			AssertTempDBObjectExist(false);
			AssertQueueTableData(expectedQueuedValues);

			//Now Reversing Line after TTP and after UToQ
			Helper.Recognize(jobs[0]);

			expectedQueuedValues = new List<Tuple<Guid, DateTime, DateTime, Guid>>()
			{
				Helper.CreateTuppleForAssertion(wipacr1.Item1, setPostdateToNull: false, setReverseDateToNull: true),
				Helper.CreateTuppleForAssertion(wipacr1.Item2, setPostdateToNull: false, setReverseDateToNull: true),

				Helper.CreateTuppleForAssertion(aplLine2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item1, setPostdateToNull: false, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item2, setPostdateToNull: false, setReverseDateToNull: false),

				Helper.CreateTuppleForAssertion(aplLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr1.Item1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr1.Item2, setPostdateToNull: true, setReverseDateToNull: false)
			};
			AssertQueueTableData(expectedQueuedValues);
		}

		public void TestJobCostingQueueTablePopulation_Scenario4()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			//Populating RptDtUnprocessedAccTransactionLines table by Start action
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 4, explanation: JCDDBObjectCheckerExplanationForSTR))
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			AssertDBObjectExist(true);
			AssertEquals("Registry Status", JCDActionList.Codes.ProcessingOldTransactionLines, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);
			AssertTempALPKTableData(Array.Empty<ZGuid>(), Array.Empty<ZGuid>());
			AssertQueueTableData(new List<Tuple<Guid, DateTime, DateTime, Guid>>());

			using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 2, explanation: JCDDBObjectCheckerExplanationForPOR))
			{
				//Populating Queue table (UToQ)
				var populateQueue = new JCDDefaultActionStrategy_ProcessOldRecordOnly_ForTest(Helper.Connection, new TestServiceLogger());
				populateQueue.Process();
			}

			AssertEquals("Registry Status", JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			AssertTempDBObjectExist(false);
			AssertQueueTableData(new List<Tuple<Guid, DateTime, DateTime, Guid>>());

			//Creating Transactions after TTP and UToQ
			var jobs = Helper.CreateJobsWithPeriod();

			var aplLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var aplLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arlLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var arlLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseACR: true, reverseWIP: true);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseACR: true, reverseWIP: true);

			var expectedQueuedValues = new List<Tuple<Guid, DateTime, DateTime, Guid>>()
			{
				Helper.CreateTuppleForAssertion(aplLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(aplLine2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine2, setPostdateToNull: true, setReverseDateToNull: false),

				Helper.CreateTuppleForAssertion(wipacr1.Item1, setPostdateToNull: false, setReverseDateToNull: true),
				Helper.CreateTuppleForAssertion(wipacr1.Item2, setPostdateToNull: false, setReverseDateToNull: true),
				Helper.CreateTuppleForAssertion(wipacr2.Item1, setPostdateToNull: false, setReverseDateToNull: true),
				Helper.CreateTuppleForAssertion(wipacr2.Item2, setPostdateToNull: false, setReverseDateToNull: true),

				Helper.CreateTuppleForAssertion(wipacr1.Item1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr1.Item2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item2, setPostdateToNull: true, setReverseDateToNull: false)
			};
			AssertQueueTableData(expectedQueuedValues);
		}

		public void TestLogMessage()
		{
			using (AccountingConfigurationRegistry.Instance.TransactionLineToJobCostingRecordTransformationBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

				var jobs = Helper.CreateJobsWithPeriod();

				Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
				Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

				Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
				Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

				Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: true, reverseACR: true);
				Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: true, reverseACR: true);

				//Populating RptDtUnprocessedAccTransactionLines table by Start action
				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 4, explanation: JCDDBObjectCheckerExplanationForSTR))
				using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
				{
					InitialiseAndRunTaskSchedule(task);
				}

				using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 2, explanation: JCDDBObjectCheckerExplanationForPOR))
				{
					//Populating Queue table (UToQ)
					var logger = new TestServiceLogger();
					var populateQueue = new JCDDefaultActionStrategy_ProcessOldRecordOnly_ForTest(Helper.Connection, logger);
					populateQueue.Process();

					AssertEquals("Debug|Checking whether action can be performed", logger[0]);
					AssertEquals("Debug|Starting DB Object Synchronization.", logger[1]);
					AssertEquals("Debug|Temporary DB Objects are up-to-date.", logger[2]);
					AssertEquals("Debug|Functions, Stored Procedures and Dependent Tables are up-to-date.", logger[3]);
					AssertEquals("Debug|DB Object Synchronization has been completed.", logger[4]);
					AssertEquals("Debug|Partition Keys are up-to-date.", logger[5]);
					AssertEquals("Debug|Populating Job Costing Report Data Queue table for existing transactions.", logger[6]);
					AssertEquals("Debug|Transformed record # 1 to 2. There are 7 more record(s) to be transformed.", logger[7]);
					AssertEquals("Debug|Transformed record # 3 to 4. There are 5 more record(s) to be transformed.", logger[8]);
					AssertEquals("Debug|Transformed record # 5 to 6. There are 3 more record(s) to be transformed.", logger[9]);
					AssertEquals("Debug|Transformed record # 7 to 8. There are 1 more record(s) to be transformed.", logger[10]);
					AssertEquals("Debug|Transformed record # 9 to 9. There are no more records to be transformed.", logger[11]);
					AssertEquals("Debug|Dropping PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord", logger[12]);
					AssertEquals("Debug|Dropping TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable", logger[13]);
					AssertEquals("Debug|Dropping database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL", logger[14]);
					AssertEquals("Debug|Dropped database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL", logger[15]);
				}

				AssertTempDBObjectExist(false);
			}
		}

		public void TestHighWaterMarkIsTakenFromRegistryAfterErrorIsThrown()
		{
			AccountingConfigurationRegistry.Instance.TransactionLineToJobCostingRecordTransformationBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var jobs = Helper.CreateJobsWithPeriod();

			var aplLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var aplLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arlLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var arlLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: true, reverseACR: true);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: true, reverseACR: true);

			//Populating RptDtUnprocessedAccTransactionLines table by Start action			
			var startTask = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				startTask.Process();
			}

			//Running ODT Service Task
			TestServiceLogger log = null;
			JCDDefaultActionStrategy_ProcessOldRecordOnly_ForTest queueTask;
			try
			{
				log = new TestServiceLogger();
				queueTask = new JCDDefaultActionStrategy_ProcessOldRecordOnly_ForTest(TestConnection, log, 5);
				queueTask.Process();
			}
			catch
			{
				AssertEquals("Debug|Checking whether action can be performed", log[0]);
				AssertEquals("Debug|Starting DB Object Synchronization.", log[1]);
				AssertEquals("Debug|Temporary DB Objects are up-to-date.", log[2]);
				AssertEquals("Debug|Functions, Stored Procedures and Dependent Tables are up-to-date.", log[3]);
				AssertEquals("Debug|DB Object Synchronization has been completed.", log[4]);
				AssertEquals("Debug|Partition Keys are up-to-date.", log[5]);
				AssertEquals("Debug|Populating Job Costing Report Data Queue table for existing transactions.", log[6]);
				AssertEquals("Debug|Transformed record # 1 to 2. There are 7 more record(s) to be transformed.", log[7]);
				AssertEquals("Debug|Transformed record # 3 to 4. There are 5 more record(s) to be transformed.", log[8]);
				AssertContains(@"Error|Failed to load data into the JobCostingDataQueue table. 
Exception: System.OutOfMemoryException 
Message: Forced OutOfMemoeryException 
StackTrace:", log[9]);

				AssertTempDBObjectExist(true);
			}

			log = new TestServiceLogger();
			queueTask = new JCDDefaultActionStrategy_ProcessOldRecordOnly_ForTest(Helper.Connection, log, 12);
			queueTask.Process();

			AssertEquals("Debug|Checking whether action can be performed", log[0]);
			AssertEquals("Debug|Starting DB Object Synchronization.", log[1]);
			AssertEquals("Debug|Temporary DB Objects are up-to-date.", log[2]);
			AssertEquals("Debug|Functions, Stored Procedures and Dependent Tables are up-to-date.", log[3]);
			AssertEquals("Debug|DB Object Synchronization has been completed.", log[4]);
			AssertEquals("Debug|Partition Keys are up-to-date.", log[5]);
			AssertEquals("Debug|Populating Job Costing Report Data Queue table for existing transactions.", log[6]);
			AssertEquals("Debug|Transformed record # 5 to 6. There are 3 more record(s) to be transformed.", log[7]);
			AssertEquals("Debug|Transformed record # 7 to 8. There are 1 more record(s) to be transformed.", log[8]);
			AssertEquals("Debug|Transformed record # 9 to 9. There are no more records to be transformed.", log[9]);
		}

		public void TestHighWaterMarkValue()
		{
			AccountingConfigurationRegistry.Instance.TransactionLineToJobCostingRecordTransformationBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var jobs = Helper.CreateJobsWithPeriod();

			var aplLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var aplLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arlLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var arlLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: true, reverseACR: true);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: true, reverseACR: true);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				startAction.Process();
			}

			var defaultAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
			defaultAction.Process();

			var expectedQueuedValues = new List<Tuple<Guid, DateTime, DateTime, Guid>>()
			{
				Helper.CreateTuppleForAssertion(aplLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(aplLine2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine1, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(arlLine2, setPostdateToNull: true, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr1.Item1, setPostdateToNull: false, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr1.Item2, setPostdateToNull: false, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item1, setPostdateToNull: false, setReverseDateToNull: false),
				Helper.CreateTuppleForAssertion(wipacr2.Item2, setPostdateToNull: false, setReverseDateToNull: false)
			};
			AssertTempDBObjectExist(false);
			AssertQueueTableData(expectedQueuedValues);
			AssertEquals(1, AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark.Value);
		}

		#endregion

		#region Starting Period calculation related tests

		public void TestStartingPeriodWhenNumberOfAccountingPeriodsIsLessThanAllowedMaxNumberOfPartition()
		{
			var periodKeys = Helper.CreatePeriods();
			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, periodKeys.Count + 1);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
				var expectedStartingPeriod = TestConnection.ExecuteScalar<int>("SELECT MIN(AM_Period) FROM dbo.AccPeriodManagement");
				AssertEquals("JCD report data collection should start from the earliest accounting period available in AccPeriodManagement table", expectedStartingPeriod, AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);
			}
		}

		public void TestStartingPeriodWhenNumberOfAccountingPeriodsIsEqualToAllowedMaxNumberOfPartition()
		{
			var periodKeys = Helper.CreatePeriods();
			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, periodKeys.Count);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
				var expectedStartingPeriod = TestConnection.ExecuteScalar<int>("SELECT MIN(AM_Period) FROM dbo.AccPeriodManagement");
				AssertEquals("JCD report data collection should start from the earliest accounting period available in AccPeriodManagement table", expectedStartingPeriod, AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);
				AssertEquals("JCD report data collection should start from the earliest accounting period available in AccPeriodManagement table", expectedStartingPeriod, Helper.GetStartingPeriodFromPartitionKey());
			}
		}

		public void TestStartingPeriodWhenNumberOfAccountingPeriodsIsMoreThanAllowedMaxNumberOfPartition()
		{
			var periodKeys = Helper.CreatePeriods();
			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, periodKeys.Count - 1);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
				var expectedStartingPeriod = TestConnection.ExecuteScalar<int>("SELECT MIN(AM_Period) FROM dbo.AccPeriodManagement") + 1;
				AssertEquals("JCD report data collection should start from the second most earliest accounting period available in AccPeriodManagement table", expectedStartingPeriod, AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);
				AssertEquals("JCD report data collection should start from the second most earliest accounting period available in AccPeriodManagement table", expectedStartingPeriod, Helper.GetStartingPeriodFromPartitionKey());
			}
		}

		public void TestTestStartingPeriodWhenMaxPartitionNumberIsTooSmall()
		{
			var periodKeys = Helper.CreatePeriods();
			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				var expectedErrorMessage = FormattableString.Invariant($"Cannot create partitions. Allowed maximum number of partitions is too small to generate report data for at least one accounting period of all companies. Please set a higher value at {AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.HumanReadableRegistryPath()}.");
				AssertExceptionThrown("An exception should be thrown", typeof(InvalidOperationException), expectedErrorMessage, () => InitialiseAndRunTaskSchedule(task));
			}
		}

		#endregion
	}
}
