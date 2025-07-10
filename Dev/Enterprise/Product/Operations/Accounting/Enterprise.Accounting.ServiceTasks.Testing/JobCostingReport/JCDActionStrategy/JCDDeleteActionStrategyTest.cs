using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(JobCostingDataPopulationServiceTask))]
	class JCDDeleteActionStrategyTest : JCDActionStrategyTestBase
	{
		/*
		 * Scenario#	Description
		 *		1		JCD Service task is not initialized. But user wants to delete it
		 *		2		JCD Service task is just initialized. There are still old AL records to process. But user wants to delete it
		 *		3		JCD Service task is set to be removed. But all JCD DB objects are already manually deleted.
		*/

		public void TestDeleteAction_Scenario1()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
				using (Env.Instance.TemporaryServiceTaskContext(JobCostingDataPopulationServiceTask.Code, canRunInAnyBranch: true))
				{
					task.RunTask();
				}
			}

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.RemoveJobCostingDataQueueServiceTask);

			#region Run Data Population Service Task and Assert 

			//Now run the Service Task
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				var log = InitialiseAndRunTaskSchedule(task);
				var expectedLogMessages = @"
Debug|Removing Job Costing Data Queue service task related Database objects
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
Debug|DB Object Synchronization has been completed.
".Trim();
				AssertMultilineASCIIEquals("Log messages", expectedLogMessages, log.ToString());
				AssertDBObjectExist(false);

				AssertEquals("Registry Status", JCDActionList.Codes.NotInitialized, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);
			}
			#endregion
		}

		public void TestDeleteAction_Scenario2()
		{
			var currentValueInRegistry = AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value;
			try
			{
				CreateEnvironemnt_Scenario2();

				AssertDBObjectExist(true);

				AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.RemoveJobCostingDataQueueServiceTask);

				#region Run Data Population Service Task and Assert 

				//Now run the Service Task
				using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
				{
					var log = InitialiseAndRunTaskSchedule(task);
					var expectedLogMessages = @"
Debug|Removing Job Costing Data Queue service task related Database objects
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
Debug|DB Object Synchronization has been completed.
".Trim();
					AssertMultilineASCIIEquals("Log Messages", expectedLogMessages, log.ToString());
					AssertDBObjectExist(false);

					AssertEquals("Registry Status", JCDActionList.Codes.NotInitialized, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

					#endregion
				}
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, currentValueInRegistry);
			}
		}

		public void TestDeleteAction_Scenario3()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
				using (Env.Instance.TemporaryServiceTaskContext(JobCostingDataPopulationServiceTask.Code, canRunInAnyBranch: true))
				{
					task.RunTask();
				}
			}

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.RemoveJobCostingDataQueueServiceTask);

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

			#region Run Data Population Service Task and Assert 

			//Now run the Service Task
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				var log = InitialiseAndRunTaskSchedule(task);

				AssertDBObjectExist(false);

				AssertEquals("Registry Status", JCDActionList.Codes.NotInitialized, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);
			}
			#endregion
		}

		void CreateEnvironemnt_Scenario2()
		{
			var jobs = Helper.CreateJobsWithPeriod(false);

			Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);
			Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: false, reverseACR: false);

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}
		}
	}
}
