using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(JobCostingDataPopulationServiceTask))]
	public class JCDDefaultActionStrategyTest : JCDActionStrategyTestBase
	{
		#region ReportData Population related Tests

		//CASE #	|	Rev. Date	|	Rev. Period	|	Post Date	|	Post Period	|	Valid For Processing
		//------------------------------------------------------------------------------------------------
		//	1		|		Y		|		Y		|		Y		|		Y		|			Y
		//	2		|		Y		|		N		|		Y		|		Y		|			N
		//	3		|		Y		|		Y		|		Y		|		N		|			N
		//	4		|		Y		|		N		|		Y		|		N		|			N
		//	5		|		Y		|		Y		|		N		|		-		|			Y
		//	6		|		Y		|		N		|		N		|		-		|			N
		//	7		|		N		|		-		|		Y		|		Y		|			Y
		//	8		|		N		|		-		|		Y		|		N		|			N

		[TestDate(2017, 6, 8, 0, 0, 0)]
		public void TestCorrectRowIsPickedFromTheQueue_Case1To4()
		{
			AccountingConfigurationRegistry.Instance.JobCostingQueueProcessBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			#region creating Past and Future Periods

			var futureDate = new ZDateTime(2050, 02, 01);
			var pastDate = new ZDateTime(2000, 02, 01);
			var helper = new AccountingPeriodTestHelper(Factory);
			var futurePeriod = helper.SetupSinglePeriod(022050, futureDate, futureDate.AddDays(28));
			var pastPeriod = helper.SetupSinglePeriod(022000, pastDate, pastDate.AddDays(29));

			#endregion

			#region Create Old AL Records to match with CASE # 1 to 4

			//Insert data in AccTransactionLines table for 2 Jobs
			//Amounts should be
			//Job 1: WIP -> 110, ACR -> -111, REV -> 112, CST ->113
			//Job 2: WIP -> 120, ACR -> -121, REV -> 122, CST ->123()
			var jobs = Helper.CreateJobsWithPeriod();

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);
			Helper.ReverseWipAcr(wipacr1.Item1); //CASE #1
			using (Helper.SetTemporaryCurrentDate(futureDate))
			{
				Helper.ReverseWipAcr(wipacr1.Item2); //CASE #2
			}

			Tuple<Business.WIPAccrual.WIP, Business.WIPAccrual.Accrual> wipacr2 = null;
			using (Helper.SetTemporaryCurrentDate(pastDate))
			{
				wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: false, reverseACR: false);
				Helper.ReverseWipAcr(wipacr2.Item2); //CASE #4
			}
			Helper.ReverseWipAcr(wipacr2.Item1); //CASE #3

			#endregion

			//Populating RptDtUnprocessedAccTransactionLines table by Start action
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
				using (Env.Instance.TemporaryServiceTaskContext(JobCostingDataPopulationServiceTask.Code, canRunInAnyBranch: true))
				{
					task.RunTask();
				}
			}

			#region Deleting periods to simulate CASE # 2, 3 and 4

			futurePeriod.Delete();
			pastPeriod.Delete();
			Factory.Save();

			#endregion

			#region Run the Data Population Service Task

			//Now run the Service Task. This time it will run as Queue Population is done
			using (ServiceTaskNudger_ForTest.GetTestInstance().AllowNudging())
			{
				ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Enqueue(new JobCostingDataPopulationServiceTask_ForTest(TestConnection));
				while (ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Any())
				{
					using (var task = ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Dequeue())
					{
						InitialiseAndRunTaskSchedule(task);
					}
				}
			}

			//Assert JobCostingData Table Data
			var table = Helper.LoadDataFromReportTable();
			if (table != null)
			{
				var expectedPeriodKey = $"{ZDateTime.Today.Year}{ZDateTime.Today.Month.ToString("00")}{GlbCompany.CurrentCompany.PK}";

				AssertEquals("Transaction Line Count", 2, table.Rows.Count);
				AssertJCReportDataLine(table, wipacr1.Item1.PK.ToGuid(), "WIP", 110, expectedPeriodKey, ZDateTime.Today);
				AssertJCReportDataLine(table, wipacr1.Item1.PK.ToGuid(), "WIP", -110, expectedPeriodKey, ZDateTime.Today);
			}

			var aggregateData = Helper.LoadDataFromReportAmountByJobTable();
			CombineAssertions(() =>
			{
				AssertEquals(1, aggregateData.Rows.Count);
				AssertReportAggregateUniqueIds(aggregateData, new[] { "201706|EDI|ZDebtor|J00000|AUD" });
				AssertReportAggregateRow(aggregateData, "201706|EDI|ZDebtor|J00000|AUD", 0m, 0m, 2);
			});

			#endregion

		}

		[TestDate(2017, 6, 8, 0, 0, 0)]
		public void TestCorrectRowIsPickedFromTheQueue_Case5To8()
		{
			using (AccountingConfigurationRegistry.Instance.JobCostingQueueProcessBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
			{
				AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

				#region creating Past and Future Periods

				var futureDate = new ZDateTime(2050, 02, 01);
				var helper = new AccountingPeriodTestHelper(Factory);
				var futurePeriod = helper.SetupSinglePeriod(022050, futureDate, futureDate.AddDays(28));

				#endregion

				#region Create Old AL Records to match with CASE # 5 to 8

				//Insert data in AccTransactionLines table for 2 Jobs
				//Amounts should be
				//Job 1: WIP -> 110, ACR -> -111, REV -> 112, CST ->113
				//Job 2: WIP -> 120, ACR -> -121, REV -> 122, CST ->123
				var jobs = Helper.CreateJobsWithPeriod();

				var arLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true); //CASE #5
				var arLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true, reverseDate: futureDate);//CASE #6
				var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);//CASE #7
				using (Helper.SetTemporaryCurrentDate(futureDate))
				{
					Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: false, reverseACR: false);//CASE #8
				}

				#endregion

				#region Deleting periods to simulate CASE # 6 and 8

				futurePeriod.Delete();
				Factory.Save();

				#endregion

				#region Run the Data Population Service Task

				//Populating RptDtUnprocessedAccTransactionLines table by Start action
				using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
				{
					InitialiseAndRunTaskSchedule(task);
				}

				//Now run the Service Task. This time it will run as Queue Population is done
				using (ServiceTaskNudger_ForTest.GetTestInstance().AllowNudging())
				{
					ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Enqueue(new JobCostingDataPopulationServiceTask_ForTest(TestConnection));
					while (ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Any())
					{
						using (var task = ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Dequeue())
						{
							InitialiseAndRunTaskSchedule(task);
						}
					}
				}

				//Assert JobCostingData Table Data
				var table = Helper.LoadDataFromReportTable();
				if (table != null)
				{
					var expectedPeriodKey = $"{ZDateTime.Today.Year}{ZDateTime.Today.Month.ToString("00")}{GlbCompany.CurrentCompany.PK}";
					var expectedPeriodKey2 = $"{ZDateTime.Today.AddDays(2).Year}{ZDateTime.Today.AddDays(2).Month.ToString("00")}{GlbCompany.CurrentCompany.PK}";

					AssertEquals("Transaction Line Count", 3, table.Rows.Count);

					AssertJCReportDataLine(table, wipacr1.Item1.PK.ToGuid(), "WIP", 110, expectedPeriodKey, ZDateTime.Today);
					AssertJCReportDataLine(table, wipacr1.Item2.PK.ToGuid(), "ACR", -111, expectedPeriodKey, ZDateTime.Today);
					AssertJCReportDataLine(table, arLine1.PK.ToGuid(), "REV", 112, expectedPeriodKey2, ZDateTime.Today.AddDays(2));
				}

				var aggregateData = Helper.LoadDataFromReportAmountByJobTable();
				CombineAssertions(() =>
				{
					AssertEquals(2, aggregateData.Rows.Count);
					AssertReportAggregateUniqueIds(aggregateData, new[]
					{
						"201706|EDI|ZCreditor1|J00000|AUD",
						"201706|EDI|ZDebtor|J00000|AUD",
					});

					AssertReportAggregateRow(aggregateData, "201706|EDI|ZCreditor1|J00000|AUD", 0m, -111m, 1);
					AssertReportAggregateRow(aggregateData, "201706|EDI|ZDebtor|J00000|AUD", 222m, 0m, 2);
				});

				#endregion
			}
		}

		[TestDate(2017, 6, 8, 0, 0, 0)]
		public void TestALRecordsWithInvalidALPeriodGetProcessedOncePeriodIsCreated()
		{
			AccountingConfigurationRegistry.Instance.JobCostingQueueProcessBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			#region Create Old AL Records without any Accounting Period being created

			//Insert data in AccTransactionLines table for 2 Jobs
			//Amounts should be
			//Job 1: WIP -> 110, ACR -> -111, REV -> 112, CST ->113
			//Job 2: WIP -> 120, ACR -> -121, REV -> 122, CST ->123()
			var jobs = Helper.CreateJobs();

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: true, reverseACR: false);

			#endregion

			//Populating RptDtUnprocessedAccTransactionLines table by start action
			var task1 = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				task1.Process();
			}

			var log = new TestServiceLogger();
			var task2 = new JCDDefaultActionStrategy(TestConnection, log);
			task2.Process();

			AssertEquals("Debug|Checking whether action can be performed", log[0]);
			AssertEquals("Debug|Starting DB Object Synchronization.", log[1]);
			AssertEquals("Debug|Temporary DB Objects are up-to-date.", log[2]);
			AssertEquals("Debug|Functions, Stored Procedures and Dependent Tables are up-to-date.", log[3]);
			AssertEquals("Debug|DB Object Synchronization has been completed.", log[4]);
			AssertEquals("Debug|Partition Keys are up-to-date.", log[5]);
			AssertEquals("Debug|Populating Job Costing Report Data Queue table for existing transactions.", log[6]);
			AssertEquals("Debug|Transformed record # 1 to 1000. There are no more records to be transformed.", log[7]);
			AssertEquals("Debug|Dropping PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord", log[8]);
			AssertEquals("Debug|Dropping TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable", log[9]);
			AssertEquals("Debug|Dropping database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL", log[10]);
			AssertEquals("Debug|Dropped database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL", log[11]);
			AssertEquals("Debug|Service Task has been nudged successfully", log[12]);

			AssertEquals("JCD Service Controller Registry Status", JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			log = new TestServiceLogger();
			var task3 = new JCDDefaultActionStrategy(TestConnection, log);
			task3.Process();

			AssertEquals("Debug|Checking whether action can be performed", log[0]);
			AssertEquals("Debug|Starting DB Object Synchronization.", log[1]);
			AssertEquals("Debug|Functions, Stored Procedures and Dependent Tables are up-to-date.", log[2]);
			AssertEquals("Debug|DB Object Synchronization has been completed.", log[3]);
			AssertEquals("Debug|Partition Keys are up-to-date.", log[4]);
			AssertEquals("Debug|Populating JobCostingData Table", log[5]);
			AssertEquals("Information|There are records in the Queue. But none of them are eligible to be processed as accounting Periods are missing for \n EDI --> 2017.", log[6]);

			CombineAssertions("Empty report table, as there is no accounting Period", () =>
			{
				var mainTable = Helper.LoadDataFromReportTable();
				AssertEquals("Report table", 0, mainTable.Rows.Count);
				var aggregateTable = Helper.LoadDataFromReportAmountByJobTable();
				AssertEquals("Aggregate table of Amount by Job", 0, aggregateTable.Rows.Count);
			});

			#region Create Accounting Period

			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			Helper.CreatePeriods();

			#endregion

			ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Enqueue(new JobCostingDataPopulationServiceTask_ForTest(TestConnection));
			using (ServiceTaskNudger_ForTest.GetTestInstance().AllowNudging())
			{
				int iterationIdx = 1;
				while (ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Any())
				{
					using (var task = ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Dequeue())
					{
						#region Run the Data Population Service Task Again after creating Accounting Period

						log = InitialiseAndRunTaskSchedule(task);

						AssertEquals("Debug|Starting Job Costing Report related data processing", log[0]);
						AssertEquals("Debug|Checking whether action can be performed", log[1]);
						AssertEquals("Debug|Starting DB Object Synchronization.", log[2]);
						AssertEquals("Debug|Functions, Stored Procedures and Dependent Tables are up-to-date.", log[3]);

						if (iterationIdx == 1)
						{
							for (int i = 5; i <= 147; i = i + 2)
							{
								AssertStartsWith("Partition Key Name", "Debug|Creating Partition ", log[i]);
								AssertEquals("Debug|Partition is created successfully", log[i + 1]);
							}

							AssertEquals("Information|Partition keys are in-sync with accounting periods.", log[149]);
							AssertEquals("Debug|Populating JobCostingData Table", log[150]);
							AssertEquals("Information|Completed processing 4 records. Trying to Nudge this service task as there are more records to process.", log[151]);
							AssertEquals("Debug|Service Task has been nudged successfully", log[152]);
						}
						else
						{
							AssertEquals("Debug|Partition Keys are up-to-date.", log[5]);
							AssertEquals("Debug|Populating JobCostingData Table", log[6]);
							AssertEquals("Information|Completed processing all records.", log[7]);
						}

						#endregion
					}

					iterationIdx++;
				}
			}

			//Assert JobCostingData Table Data
			var table = Helper.LoadDataFromReportTable();
			if (table != null)
			{
				var expectedPeriodKey = $"{ZDateTime.Today.Year}{ZDateTime.Today.Month.ToString("00")}{GlbCompany.CurrentCompany.PK}";

				AssertEquals("Transaction Line Count", 5, table.Rows.Count);

				AssertJCReportDataLine(table, wipacr1.Item1.PK.ToGuid(), "WIP", 110, expectedPeriodKey, ZDateTime.Today);
				AssertJCReportDataLine(table, wipacr2.Item1.PK.ToGuid(), "WIP", 120, expectedPeriodKey, ZDateTime.Today);
				AssertJCReportDataLine(table, wipacr2.Item1.PK.ToGuid(), "WIP", -120, expectedPeriodKey, ZDateTime.Today);

				AssertJCReportDataLine(table, wipacr1.Item2.PK.ToGuid(), "ACR", -111, expectedPeriodKey, ZDateTime.Today);
				AssertJCReportDataLine(table, wipacr2.Item2.PK.ToGuid(), "ACR", -121, expectedPeriodKey, ZDateTime.Today);
			}

			var aggregateData = Helper.LoadDataFromReportAmountByJobTable();
			CombineAssertions(() =>
			{
				AssertEquals(4, aggregateData.Rows.Count);
				AssertReportAggregateUniqueIds(aggregateData, new[]
				{
					"201706|EDI|ZDebtor|J00000|AUD",
					"201706|EDI|ZCreditor1|J00000|AUD",
					"201706|EDI|ZDebtor|J00001|AUD",
					"201706|EDI|ZCreditor1|J00001|AUD",
				});

				AssertReportAggregateRow(aggregateData, "201706|EDI|ZDebtor|J00000|AUD", 110m, 0m, 1);
				AssertReportAggregateRow(aggregateData, "201706|EDI|ZCreditor1|J00000|AUD", 0m, -111m, 1);
				AssertReportAggregateRow(aggregateData, "201706|EDI|ZDebtor|J00001|AUD", 0m, 0m, 2);
				AssertReportAggregateRow(aggregateData, "201706|EDI|ZCreditor1|J00001|AUD", 0m, -121m, 1);
			});
		}

		void AssertPartitionKeys(List<string> expectedPartitionKeys)
		{
			var sql = @"	SELECT	CAST(value as char(42)) as PeriodKey
							FROM	sys.partition_functions AS pf
									INNER JOIN sys.partition_range_values AS prv ON prv.function_id = pf.function_id
							WHERE	pf.name = 'PF_AccountingPeriodCompany'";

			var actualPartitionKeys = new List<string>();
			var dt = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			if (dt != null)
			{
				foreach (DataRow row in dt.Rows)
				{
					actualPartitionKeys.Add(Convert.ToString(row[0]).ToLower());
				}
			}

			AssertContainsExactElementsInAnyOrder(expectedPartitionKeys, actualPartitionKeys);
		}

		void AssertJCReportDataLine(DataTable jCCostingLines, Guid linePK, string lineType, decimal amount, string periodKey, ZDateTime postDate)
		{
			var qryString = string.Format("JCD_AL='{0}' AND JCD_LineType='{1}' AND JCD_LineAmount={2} AND JCD_PeriodCompanyKey='{3}' AND JCD_PostDate >= #{4}# AND  JCD_PostDate < #{5}#",
				linePK, lineType, amount, periodKey, postDate.Date.ToString("dd MMM yyyy"), postDate.Date.AddDays(1).ToString("dd MMM yyyy"));
			var rows = jCCostingLines.Select(qryString);
			AssertEquals("Row Should Exist", 1, rows.Length);
		}

		void AssertReportAggregateUniqueIds(DataTable data, IReadOnlyCollection<string> expectedUniqueIds)
		{
			var actualIds = data.Rows.Cast<DataRow>().Select(x => x["UniqueId"].ToString());
			AssertContainsExactElementsInAnyOrder("UniqueIds", expectedUniqueIds, actualIds);
		}

		void AssertReportAggregateRow(DataTable data, string expectedUniqueId, decimal expectedRevenue, decimal expectedCost, long expectedRowCount)
		{
			var row = data.Rows.Cast<DataRow>().Single(x => x["UniqueId"].ToString() == expectedUniqueId);
			AssertEquals($"Revenue[{expectedUniqueId}]", expectedRevenue, row["JCA_Revenue"]);
			AssertEquals($"Cost[{expectedUniqueId}]", expectedCost, row["JCA_Cost"]);
			AssertEquals($"RowCount[{expectedUniqueId}]", expectedRowCount, row["JCA_RowCount"]);
		}

		#endregion

		#region Overall JCD Service task Tests

		[TestDate(2023, 10, 30)]
		public void TestJCDDefaultActionStrategyOverall()
		{
			AccountingConfigurationRegistry.Instance.JobCostingQueueProcessBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			#region Create Old AL Records

			//Insert data in AccTransactionLines table for 2 Jobs
			//Amounts should be
			//Job 1: WIP -> 110, ACR -> -111, REV -> 112, CST ->113
			//Job 2: WIP -> 120, ACR -> -121, REV -> 122, CST ->123()
			var jobs = Helper.CreateJobsWithPeriod();

			var apLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var apLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var arLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: true, reverseACR: false);

			#endregion

			//Populating RptDtUnprocessedAccTransactionLines table by start action
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538
			using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 4, explanation: JCDDBObjectCheckerExplanationForSTR))
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			//Populating JobCostingDataQueue table
			using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 2, explanation: JCDDBObjectCheckerExplanationForPOR))
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				var log = InitialiseAndRunTaskSchedule(task);

				#region Assert Log Message

				AssertEquals("Debug|Starting Job Costing Report related data processing", log[0]);
				AssertEquals("Debug|Checking whether action can be performed", log[1]);
				AssertEquals("Debug|Starting DB Object Synchronization.", log[2]);
				AssertEquals("Debug|Temporary DB Objects are up-to-date.", log[3]);
				AssertEquals("Debug|Functions, Stored Procedures and Dependent Tables are up-to-date.", log[4]);
				AssertEquals("Debug|DB Object Synchronization has been completed.", log[5]);
				AssertEquals("Debug|Partition Keys are up-to-date.", log[6]);
				AssertEquals("Debug|Populating Job Costing Report Data Queue table for existing transactions.", log[7]);
				AssertEquals("Debug|Transformed record # 1 to 1000. There are no more records to be transformed.", log[8]);
				AssertEquals("Debug|Dropping PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord", log[9]);
				AssertEquals("Debug|Dropping TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable", log[10]);
				AssertEquals("Debug|Dropping database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL", log[11]);
				AssertEquals("Debug|Dropped database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL", log[12]);

				#endregion
			}

			int actualNumberOfNudgingOccurred = 0;
			bool isNudged = true;

			//Now run the Service Task. This time it will populate report table as jobcostingdataqueue table is populated by transforming old AL record.
			using (ServiceTaskNudger_ForTest.GetTestInstance().AllowNudging())
			{
				ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Enqueue(new JobCostingDataPopulationServiceTask_ForTest(TestConnection));
				while (isNudged)
				{
					using (TrackJCDBObjectCheckerQueryCall(expectedNumberOfCall: 0, explanation: JCDDBObjectCheckerExplanationForCUP))
					using (var task = ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Dequeue())
					{
						var log = InitialiseAndRunTaskSchedule(task);

						var logIndex = 0;
						AssertEquals("Debug|Starting Job Costing Report related data processing", log[logIndex]);
						AssertEquals("Debug|Checking whether action can be performed", log[++logIndex]);
						AssertEquals("Debug|Starting DB Object Synchronization.", log[++logIndex]);
						AssertEquals("Debug|Functions, Stored Procedures and Dependent Tables are up-to-date.", log[++logIndex]);
						AssertEquals("Debug|DB Object Synchronization has been completed.", log[++logIndex]);
						AssertEquals("Debug|Partition Keys are up-to-date.", log[++logIndex]);
						AssertEquals("Debug|Populating JobCostingData Table", log[++logIndex]);

						isNudged = ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Any();
						if (isNudged)
						{
							AssertEquals("Information|Completed processing 1 records. Trying to Nudge this service task as there are more records to process.", log[++logIndex]);
							AssertEquals("Debug|Service Task has been nudged successfully", log[++logIndex]);
						}
						else
						{
							AssertEquals("Information|Completed processing all records.", log[++logIndex]);
						}
					}

					actualNumberOfNudgingOccurred++;
				}
			}

			AssertEquals("Nudging Occurred", 9, actualNumberOfNudgingOccurred);

			//Assert JobCostingData Table Data
			var table = Helper.LoadDataFromReportTable();
			if (table != null)
			{
				var expectedPeriodKey = $"{ZDateTime.Today.Year}{ZDateTime.Today.Month.ToString("00")}{GlbCompany.CurrentCompany.PK}";
				var expectedPeriodKey2 = $"{ZDateTime.Today.AddDays(2).Year}{ZDateTime.Today.AddDays(2).Month.ToString("00")}{GlbCompany.CurrentCompany.PK}";

				AssertEquals("Transaction Line Count", 9, table.Rows.Count);

				AssertJCReportDataLine(table, wipacr1.Item1.PK.ToGuid(), "WIP", 110, expectedPeriodKey, ZDateTime.Today);
				AssertJCReportDataLine(table, wipacr2.Item1.PK.ToGuid(), "WIP", 120, expectedPeriodKey, ZDateTime.Today);
				AssertJCReportDataLine(table, wipacr2.Item1.PK.ToGuid(), "WIP", -120, expectedPeriodKey, ZDateTime.Today);

				AssertJCReportDataLine(table, wipacr1.Item2.PK.ToGuid(), "ACR", -111, expectedPeriodKey, ZDateTime.Today);
				AssertJCReportDataLine(table, wipacr2.Item2.PK.ToGuid(), "ACR", -121, expectedPeriodKey, ZDateTime.Today);

				AssertJCReportDataLine(table, arLine1.PK.ToGuid(), "REV", 112, expectedPeriodKey2, ZDateTime.Today.AddDays(2));
				AssertJCReportDataLine(table, arLine2.PK.ToGuid(), "REV", 122, expectedPeriodKey2, ZDateTime.Today.AddDays(2));

				AssertJCReportDataLine(table, apLine1.PK.ToGuid(), "CST", -113, expectedPeriodKey2, ZDateTime.Today.AddDays(2));
				AssertJCReportDataLine(table, apLine2.PK.ToGuid(), "CST", -123, expectedPeriodKey2, ZDateTime.Today.AddDays(2));
			}

			var aggregateData = Helper.LoadDataFromReportAmountByJobTable();
			CombineAssertions(() =>
			{
				AssertEquals(8, aggregateData.Rows.Count);
				AssertReportAggregateUniqueIds(aggregateData, new[]
				{
					"202310|EDI|ZCreditor1|J00000|AUD",
					"202310|EDI|ZDebtor|J00000|AUD",
					"202310|EDI|ZCreditor1|J00001|AUD",
					"202310|EDI|ZDebtor|J00001|AUD",
					"202311|EDI|ZCreditor1|J00000|AUD",
					"202311|EDI|ZDebtor|J00000|AUD",
					"202311|EDI|ZCreditor1|J00001|AUD",
					"202311|EDI|ZDebtor|J00001|AUD",
				});

				AssertReportAggregateRow(aggregateData, "202310|EDI|ZCreditor1|J00000|AUD", 0m, -111m, 1);
				AssertReportAggregateRow(aggregateData, "202310|EDI|ZDebtor|J00000|AUD", 110m, 0m, 1);
				AssertReportAggregateRow(aggregateData, "202310|EDI|ZCreditor1|J00001|AUD", 0m, -121m, 1);
				AssertReportAggregateRow(aggregateData, "202310|EDI|ZDebtor|J00001|AUD", 0m, 0m, 2);
				AssertReportAggregateRow(aggregateData, "202311|EDI|ZCreditor1|J00000|AUD", 0m, -113m, 1);
				AssertReportAggregateRow(aggregateData, "202311|EDI|ZDebtor|J00000|AUD", 112m, 0m, 1);
				AssertReportAggregateRow(aggregateData, "202311|EDI|ZCreditor1|J00001|AUD", 0m, -123m, 1);
				AssertReportAggregateRow(aggregateData, "202311|EDI|ZDebtor|J00001|AUD", 122m, 0m, 1);
			});
		}

		public void TestProperErrorMessageIsLoggedWhenNudgingFails()
		{
			var previousErrorReporterInstance = ErrorReporter.Instance;
			try
			{
				AccountingConfigurationRegistry.Instance.JobCostingQueueProcessBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
				AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

				ErrorReporter.Instance = new DummyErrorReporter();

				#region Creating AL records

				//Insert data in AccTransactionLines table for 2 Jobs
				//Amounts should be
				//Job 1: WIP -> 110, ACR -> -111, REV -> 112, CST ->113
				//Job 2: WIP -> 120, ACR -> -121, REV -> 122, CST ->123
				var jobs = Helper.CreateJobsWithPeriod();

				var apLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
				var apLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

				var arLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
				var arLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

				var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);
				var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: true, reverseACR: false);

				#endregion

				//Populating RptDtUnprocessedAccTransactionLines table by start action
				using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
				{
					InitialiseAndRunTaskSchedule(task);
				}

				using (ServiceTaskNudger_ForTest.GetTestInstance().RaiseExceptionWhileNudging())
				using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
				{
					#region Run Data Population Service Task

					var log = InitialiseAndRunTaskSchedule(task);
					AssertEquals("Debug|Starting Job Costing Report related data processing", log[0]);
					AssertEquals("Debug|Checking whether action can be performed", log[1]);
					AssertEquals("Debug|Starting DB Object Synchronization.", log[2]);
					AssertEquals("Debug|Temporary DB Objects are up-to-date.", log[3]);
					AssertEquals("Debug|Functions, Stored Procedures and Dependent Tables are up-to-date.", log[4]);
					AssertEquals("Debug|DB Object Synchronization has been completed.", log[5]);
					AssertEquals("Debug|Partition Keys are up-to-date.", log[6]);
					AssertEquals("Debug|Populating Job Costing Report Data Queue table for existing transactions.", log[7]);
					AssertEquals("Debug|Transformed record # 1 to 1000. There are no more records to be transformed.", log[8]);
					AssertEquals("Debug|Dropping PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord", log[9]);
					AssertEquals("Debug|Dropping TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable", log[10]);
					AssertEquals("Debug|Dropping database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL", log[11]);
					AssertEquals("Debug|Dropped database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL", log[12]);
					AssertEquals("Error|Failed to Nudge. Error: Forced to fail Nudging", log[13]);

					#endregion
				}
			}
			finally
			{
				ErrorReporter.Instance = previousErrorReporterInstance;
			}
		}

		[TestDate(2023, 10, 30)]
		public void TestJCDDBObjectsAreCreatedCorrectly()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			#region Creating AL records

			//Insert data in AccTransactionLines table for 2 Jobs
			//Amounts should be
			//Job 1: WIP -> 110, ACR -> -111, REV -> 112, CST ->113
			//Job 2: WIP -> 120, ACR -> -121, REV -> 122, CST ->123
			var periodKeys = Helper.CreatePeriods();
			var jobs = Helper.CreateJobs();

			var apLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var arLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);

			#endregion

			//Populating RptDtUnprocessedAccTransactionLines table by Start action
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			// Starting processing Data for Job Costing Report
			using (ServiceTaskNudger_ForTest.GetTestInstance().AllowNudging())
			{
				ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Enqueue(new JobCostingDataPopulationServiceTask_ForTest(TestConnection));
				while (ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Any())
				{
					using (var task = ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Dequeue())
					{
						InitialiseAndRunTaskSchedule(task);
					}
				}
			}

			#region Assertion

			//Assert PeriodKeys
			periodKeys.Add("99999900000000-0000-0000-0000-000000000000");
			AssertPartitionKeys(periodKeys);

			//Assert Partition Scheme is created
			var psExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.partition_schemes ps where NAME = 'PS_AccountingPeriodCompany'") > 0;
			AssertEquals("PS_AccountingPeriodCompany scheme should Exist", true, psExists);

			//Assert Check Constraints are Created and Trusted
			var dt = DataUtils.GetDataTableFromQuery(Helper.Connection, @"SELECT	[name] as ConstraintName,
																							is_disabled,
																							is_not_trusted
																					FROM	sys.check_constraints
																					WHERE	[name] in ('Constraint_JCD_Ledger', 'Constraint_JCD_LineType', 'Constraint_JCD_ParentTableCode', 'Constraint_JCD_PeriodCompanyKey', 'Constraint_JCD_PostPeriod', 'Constraint_JCD_TransactionType')
																					ORDER BY ConstraintName");

			AssertNotNull("Constraint Should Exist", dt);

			var expectedConstraintsInfo = new List<string>() {
						"Constraint_JCD_Ledger-False-False",
						"Constraint_JCD_LineType-False-False",
						"Constraint_JCD_ParentTableCode-False-False",
						"Constraint_JCD_PostPeriod-False-False",
						"Constraint_JCD_TransactionType-False-False"
					};
			var constraintInfo = dt.Select().Select(r => string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", Convert.ToString(r["ConstraintName"]), Convert.ToString(r["is_disabled"]), Convert.ToString(r["is_not_trusted"])));
			AssertContainsExactElementsInAnyOrder("All Constraints Should be Enabled and Trusted", expectedConstraintsInfo, constraintInfo);

			//Assert CI_JCD_PostDate Index is created
			var indexExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.indexes WHERE name='CI_JCD_PostDate' AND object_id = OBJECT_ID('RptDtJobCostingData')") > 0;
			AssertEquals("CI_JCD_PostDate should Exist", true, indexExists);

			//Assert Index is created
			indexExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.indexes WHERE name='NCI_JCD_OH' AND object_id = OBJECT_ID('RptDtJobCostingData')") > 0;
			AssertEquals("NCI_JCD_OH should Exist", true, indexExists);

			//Assert Store Procedure(s) are created
			var spExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('RptDtPopulateJobCostingDataFromQueue'), -1)") > 0;
			AssertEquals("RptDtPopulateJobCostingDataFromQueue", true, spExists);

			//Assert Function(s) are created
			var functionExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('RptDt_Report_GlobalJobProfitSummaryByJob'), -1)") > 0;
			AssertEquals("RptDt_Report_GlobalJobProfitSummaryByJob should Exist", true, functionExists);

			//Assert JobCostingData Table Data
			var table = Helper.LoadDataFromReportTable();
			if (table != null)
			{
				var expectedPeriodKey = $"{ZDateTime.Today.Year}{ZDateTime.Today.Month.ToString("00")}{GlbCompany.CurrentCompany.PK}";
				var expectedPeriodKey2 = $"{ZDateTime.Today.AddDays(2).Year}{ZDateTime.Today.AddDays(2).Month.ToString("00")}{GlbCompany.CurrentCompany.PK}";

				AssertEquals("Transaction Line Count", 4, table.Rows.Count);

				AssertJCReportDataLine(table, wipacr1.Item1.PK.ToGuid(), "WIP", 110, expectedPeriodKey, ZDateTime.Today);
				AssertJCReportDataLine(table, wipacr1.Item2.PK.ToGuid(), "ACR", -111, expectedPeriodKey, ZDateTime.Today);
				AssertJCReportDataLine(table, arLine1.PK.ToGuid(), "REV", 112, expectedPeriodKey2, ZDateTime.Today.AddDays(2));
				AssertJCReportDataLine(table, apLine1.PK.ToGuid(), "CST", -113, expectedPeriodKey2, ZDateTime.Today.AddDays(2));
			}

			var aggregateData = Helper.LoadDataFromReportAmountByJobTable();
			CombineAssertions(() =>
			{
				AssertEquals(4, aggregateData.Rows.Count);
				AssertReportAggregateUniqueIds(aggregateData, new[]
				{
					"202310|EDI|ZDebtor|J00000|AUD",
					"202310|EDI|ZCreditor1|J00000|AUD",
					"202311|EDI|ZDebtor|J00000|AUD",
					"202311|EDI|ZCreditor1|J00000|AUD",
				});

				AssertReportAggregateRow(aggregateData, "202310|EDI|ZDebtor|J00000|AUD", 110m, 0m, 1);
				AssertReportAggregateRow(aggregateData, "202310|EDI|ZCreditor1|J00000|AUD", 0m, -111m, 1);
				AssertReportAggregateRow(aggregateData, "202311|EDI|ZDebtor|J00000|AUD", 112m, 0m, 1);
				AssertReportAggregateRow(aggregateData, "202311|EDI|ZCreditor1|J00000|AUD", 0m, -113m, 1);
			});

			#endregion
		}

		[TestDate(2023, 10, 30)]
		public void TestJCDAmountTableIsAddedAndPopulatedWhenNotExists()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			#region Creating AL records

			//Insert data in AccTransactionLines table for 2 Jobs
			//Amounts should be
			//Job 1: WIP -> 110, ACR -> -111, REV -> 112, CST ->113
			//Job 2: WIP -> 120, ACR -> -121, REV -> 122, CST ->123
			var periodKeys = Helper.CreatePeriods();
			var jobs = Helper.CreateJobs();

			var apLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var arLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);

			#endregion

			//Populating RptDtUnprocessedAccTransactionLines table by Start action
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			// Starting processing Data for Job Costing Report
			using (ServiceTaskNudger_ForTest.GetTestInstance().AllowNudging())
			{
				ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Enqueue(new JobCostingDataPopulationServiceTask_ForTest(TestConnection));
				while (ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Any())
				{
					using (var task = ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Dequeue())
					{
						InitialiseAndRunTaskSchedule(task);
					}
				}
			}
			var mainDataBefore = Helper.LoadDataFromReportTable();
			AssertGreaterThan("Precondition: data exists in main report table", mainDataBefore.Rows.Count, 0);

			// Drop dependent tables & related indexes
			var objectNames = JCDDBObjectInfoList.Instance.Where(x => x.DependencySequence >= 30 && x.DependencySequence <= 39).Select(x => x.Name).ToHashSet();
			var dependentObjectsToDrop = JCDDependentObjectList.GetVersionManager().DBObjects
					.Where(x => x.DbObjectInfo.DependencySequence >= 30 && x.DbObjectInfo.DependencySequence <= 39)
					.Reverse();
			foreach (var dbObject in dependentObjectsToDrop)
			{
				TestConnection.ExecuteNonQuery(dbObject.CheckAndDropSQLText);
			}
			var exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('RptDtJobCostingDataAmountByJob'), -1)") > 0;
			AssertEquals("RptDtJobCostingDataAmountByJob should NOT exist", false, exists);

			AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			// Running service task should re-add tables, and populate with data based on the main table.
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			var mainDataAfter = Helper.LoadDataFromReportTable();
			AssertEquals("No data should be deleted from main report table", mainDataBefore.Rows.Count, mainDataAfter.Rows.Count);

			var dependentData = Helper.LoadDataFromReportAmountByJobTable();
			CombineAssertions("Data in dependent aggregated table should be populated based on main report table", () =>
			{
				AssertEquals(4, dependentData.Rows.Count);
				AssertReportAggregateUniqueIds(dependentData, new[]
				{
					"202310|EDI|ZDebtor|J00000|AUD",
					"202310|EDI|ZCreditor1|J00000|AUD",
					"202311|EDI|ZDebtor|J00000|AUD",
					"202311|EDI|ZCreditor1|J00000|AUD",
				});

				AssertReportAggregateRow(dependentData, "202310|EDI|ZDebtor|J00000|AUD", 110m, 0m, 1);
				AssertReportAggregateRow(dependentData, "202310|EDI|ZCreditor1|J00000|AUD", 0m, -111m, 1);
				AssertReportAggregateRow(dependentData, "202311|EDI|ZDebtor|J00000|AUD", 112m, 0m, 1);
				AssertReportAggregateRow(dependentData, "202311|EDI|ZCreditor1|J00000|AUD", 0m, -113m, 1);
			});
		}

		[TestDate(2023, 10, 31)]
		public void TestJCDDependentTablesWithVeryLargeMoneyValues()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var registryValue = new MaximumAllowedTransactionAmount()
			{
				MaximumAllowedHeaderAmount = 1200000000000000M,
				MaximumAllowedLineAmount = 1200000000000000M
			};
			AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);

			#region Creating AL records

			//Insert data in AccTransactionLines table for 2 Jobs
			//Amounts should exceed SQL money type when added
			var periodKeys = Helper.CreatePeriods();
			var jobs = Helper.CreateJobs();
			Helper.CreateACRWIPLine(jobs[0], 1, 900000000000179m, 0, reverseWIP: false, reverseACR: false);
			Helper.CreateACRWIPLine(jobs[0], 1, 900000000000181m, 0, reverseWIP: false, reverseACR: false);
			Helper.CreateACRWIPLine(jobs[0], 1, 900000000000317m, 0, reverseWIP: false, reverseACR: false);

			Helper.CreateACRWIPLine(jobs[1], 1, 0, 900000000000109m, reverseWIP: false, reverseACR: false);
			Helper.CreateACRWIPLine(jobs[1], 1, 0, 900000000000131m, reverseWIP: false, reverseACR: false);
			Helper.CreateACRWIPLine(jobs[1], 1, 0, 900000000000167m, reverseWIP: false, reverseACR: false);

			#endregion

			//Populating RptDtUnprocessedAccTransactionLines table by Start action
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			// Starting processing Data for Job Costing Report
			using (ServiceTaskNudger_ForTest.GetTestInstance().AllowNudging())
			{
				ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Enqueue(new JobCostingDataPopulationServiceTask_ForTest(TestConnection));
				while (ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Any())
				{
					using (var task = ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Dequeue())
					{
						InitialiseAndRunTaskSchedule(task);
					}
				}
			}

			var mainData = Helper.LoadDataFromReportTable();
			AssertEquals("Data should be inserted in main report table", 6, mainData.Rows.Count);

			var aggregateData = Helper.LoadDataFromReportAmountByJobTable();
			CombineAssertions(() =>
			{
				AssertEquals(2, aggregateData.Rows.Count);
				AssertReportAggregateUniqueIds(aggregateData, new[]
				{
					"202310|EDI|ZCreditor1|J00000|AUD",
					"202310|EDI|ZDebtor|J00001|AUD",
				});

				AssertReportAggregateRow(aggregateData, "202310|EDI|ZCreditor1|J00000|AUD", 0m, -2700000000000677m, 3);
				AssertReportAggregateRow(aggregateData, "202310|EDI|ZDebtor|J00001|AUD", 2700000000000407m, 0m, 3);
			});
		}

		[TestDate(2023, 10, 31)]
		public void TestJCDCanPopulateReportTableifCompanyLocalCurrencyIsChanged()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			//Insert data in AccTransactionLines table for 2 Jobs
			Helper.CreatePeriods();

			//Populating RptDtUnprocessedAccTransactionLines table by Start action
			using (var task = new JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			var jobs = Helper.CreateJobs();

			var currCompany = GlbCompany.CurrentCompany;

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (currCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.EuropeanUnion))
			{
				AssertWithCurrentLocalCurrency(Core.Constants.CurrencyCodes.EuropeanUnion, expectNumberOfJCDRows: 6, expectedNumberOfJCARow: 2);
			}

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (currCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.Australia))
			{
				AssertWithCurrentLocalCurrency(Core.Constants.CurrencyCodes.Australia, expectNumberOfJCDRows: 12, expectedNumberOfJCARow: 4);
			}

			void AssertWithCurrentLocalCurrency(string localCurrency, int expectNumberOfJCDRows, int expectedNumberOfJCARow)
			{
				AssertEquals("Pre-condition", localCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

				#region Creating AL records

				Helper.CreateACRWIPLine(jobs[0], 1, 200, 0, reverseWIP: false, reverseACR: false);
				Helper.CreateACRWIPLine(jobs[0], 1, 300, 0, reverseWIP: false, reverseACR: false);
				Helper.CreateACRWIPLine(jobs[0], 1, 400, 0, reverseWIP: false, reverseACR: false);

				Helper.CreateACRWIPLine(jobs[1], 1, 0, 500, reverseWIP: false, reverseACR: false);
				Helper.CreateACRWIPLine(jobs[1], 1, 0, 600, reverseWIP: false, reverseACR: false);
				Helper.CreateACRWIPLine(jobs[1], 1, 0, 700, reverseWIP: false, reverseACR: false);

				#endregion

				// Starting processing Data for Job Costing Report
				using (ServiceTaskNudger_ForTest.GetTestInstance().AllowNudging())
				{
					ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Enqueue(new JobCostingDataPopulationServiceTask_ForTest(TestConnection));
					while (ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Any())
					{
						using (var task = ServiceTaskNudger_ForTest.GetTestInstance().QueuedServiceTask.Dequeue())
						{
							InitialiseAndRunTaskSchedule(task);
						}
					}
				}

				var mainData = Helper.LoadDataFromReportTable();
				AssertEquals("Data should be inserted in main report table", expectNumberOfJCDRows, mainData.Rows.Count);

				var aggregateData = Helper.LoadDataFromReportAmountByJobTable();
				CombineAssertions(() =>
				{
					AssertEquals(expectedNumberOfJCARow, aggregateData.Rows.Count);
					var actualIds = aggregateData.Rows.Cast<DataRow>().Select(x => x["UniqueId"].ToString());
					var expectedUniqueIds = new[]
					{
						$"202310|EDI|ZCreditor1|{jobs[0].JH_JobNum}|{localCurrency}",
						$"202310|EDI|ZDebtor|{jobs[1].JH_JobNum}|{localCurrency}",
					};
					expectedUniqueIds.ForEach(uid => AssertCollectionContains($"UniqueIds-{uid}", uid, actualIds));

					AssertReportAggregateRow(aggregateData, $"202310|EDI|ZCreditor1|{jobs[0].JH_JobNum}|{localCurrency}", 0m, -900m, 3);
					AssertReportAggregateRow(aggregateData, $"202310|EDI|ZDebtor|{jobs[1].JH_JobNum}|{localCurrency}", 1800m, 0m, 3);
				});
			}
		}

		#endregion
	}
}
