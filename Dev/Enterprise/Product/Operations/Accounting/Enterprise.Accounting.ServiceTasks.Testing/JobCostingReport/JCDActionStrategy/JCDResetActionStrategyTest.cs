using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(JobCostingDataPopulationServiceTask))]
	public class JCDResetActionStrategyTest : JCDActionStrategyTestBase
	{
		/*
		 * Scenario#	Description
		 *		1		JCD Service task is not initialized. But user wants to Reset it
		 *		2		JCD Service task is just initialized. There are still old AL records to process. But user wants to Reset it
		 *		3		JCD service task is set to RST, but all JCD DB objects are already manually deleted.
		*/

		public void TestResetAction_Scenario1()
		{
			CreateEnvironment_Scenario1();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.ReInitializeJobCostingDataQueueServiceTask);

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

			//Now run the Service Task
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				#region Run Data Population Service Task and Assert 

				var log = InitialiseAndRunTaskSchedule(task);
				var expectedLogMessages = @"
Debug|Re-initializing Job Costing Data Queue service task. This action will remove all existing Job Costing Report related processed data and prepare the service task to reprocess.
Debug|Checking whether action can be performed
Debug|Starting DB Object Synchronization.
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
Debug|Dropping PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord
Debug|Dropping TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable
Debug|Dropping database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL
Debug|Dropped database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL
Debug|Dropping database objects : RptDtJobCostingData, PS_AccountingPeriodCompany, PF_AccountingPeriodCompany
Debug|Dropped database objects : RptDtJobCostingData, PS_AccountingPeriodCompany, PF_AccountingPeriodCompany
Debug|All Job Costing Report related DB Objects are dropped.
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

		void CreateEnvironment_Scenario1()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}
		}

		public void TestResetAction_Scenario2()
		{
			var allPKs = CreateEnvironment_Scenario2();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.ReInitializeJobCostingDataQueueServiceTask);

			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				#region Run Data Population Service Task and Assert 

				//Now run the Service Task
				var log = InitialiseAndRunTaskSchedule(task);
				var expectedLogMessages = @"
Debug|Re-initializing Job Costing Data Queue service task. This action will remove all existing Job Costing Report related processed data and prepare the service task to reprocess.
Debug|Checking whether action can be performed
Debug|Starting DB Object Synchronization.
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
Debug|Dropping PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord
Debug|Dropping TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable
Debug|Dropping database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL
Debug|Dropped database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL
Debug|Dropping database objects : RptDtJobCostingData, PS_AccountingPeriodCompany, PF_AccountingPeriodCompany
Debug|Dropped database objects : RptDtJobCostingData, PS_AccountingPeriodCompany, PF_AccountingPeriodCompany
Debug|All Job Costing Report related DB Objects are dropped.
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
			}
		}

		ZGuid[] CreateEnvironment_Scenario2()
		{
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

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			var tempTableData = Helper.LoadDataFromRptDtUnprocessedAccTransactionLinesTable();
			AssertEquals("Number of Rows", 8, tempTableData.Rows.Count);

			return allPKs;
		}

		public void TestResetAction_Scenario3()
		{
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

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
				using (Env.Instance.TemporaryServiceTaskContext(JobCostingDataPopulationServiceTask.Code, canRunInAnyBranch: true))
				{
					task.RunTask();
				}
			}

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.ReInitializeJobCostingDataQueueServiceTask);

			TestConnection.ExecuteNonQuery(@"
DROP FUNCTION [dbo].[RptDt_Report_GlobalForwardingAndCustomsSummary]

DROP FUNCTION [dbo].[RptDt_Report_GlobalJobProfitSummaryByJob]

DROP FUNCTION [dbo].[RptDt_GlobalJobProfitReportCore]

DROP FUNCTION [dbo].[RptDt_GetPeriodKeys]

DROP PROCEDURE [dbo].[RptDtPopulateJobCostingDataFromQueue]

DROP TRIGGER [dbo].[RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue]

DROP TABLE [dbo].[RptDtJobCostingData]

DROP TABLE [dbo].[RptDtJobCostingDataAmountByJob]

DROP PARTITION SCHEME PS_AccountingPeriodCompany

DROP PARTITION FUNCTION PF_AccountingPeriodCompany");

			AssertEquals(false, new JCDDBObjectsChecker(TestConnection).DoesAnyJCDDBObjectExist());

			//Now run the Service Task when registry status is RST
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				var log = InitialiseAndRunTaskSchedule(task);

				AssertEquals("Registry Status will be STR, as there is no JCD DB object. JCD will need to be initialised, instead of Re-Initialised", JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);
			}

			//Now run the Service Task when registry status is STR
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

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
		}

		#region Starting Period calculation related tests

		public void TestStartingPeriodWhenNumberOfAccountingPeriodsIsLessThanAllowedMaxNumberOfPartition()
		{
			var periodKeys = Helper.CreatePeriods();
			CreateEnvironmentForStartingPeriodTesting();

			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, periodKeys.Count + 1);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.ReInitializeJobCostingDataQueueServiceTask);
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
			CreateEnvironmentForStartingPeriodTesting();

			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, periodKeys.Count);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.ReInitializeJobCostingDataQueueServiceTask);
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
			CreateEnvironmentForStartingPeriodTesting();

			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, periodKeys.Count - 1);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.ReInitializeJobCostingDataQueueServiceTask);
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
			CreateEnvironmentForStartingPeriodTesting();

			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.ReInitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				var expectedErrorMessage = FormattableString.Invariant($"Cannot create partitions. Allowed maximum number of partitions is too small to generate report data for at least one accounting period of all companies. Please set a higher value at {AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.HumanReadableRegistryPath()}.");
				AssertExceptionThrown("An exception should be thrown", typeof(InvalidOperationException), expectedErrorMessage, () => InitialiseAndRunTaskSchedule(task));
			}
		}

		void CreateEnvironmentForStartingPeriodTesting()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}
		}

		#endregion
	}
}
