using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.PrintProcessing;
using Enterprise.PrintProcessing.Testing;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(PrintJobTask))]
	sealed class PrintJobTaskTest : ServiceTaskTestCase<PrintJobTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "PRD", hostedServiceAttribute.Code);
				AssertEquals("Description", "Printing documents", hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", true, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("MinimumPeriod", "15seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
			});
		}

		public void TestRunPRNTask_UpdateStmPrintQueueLastUsedDate()
		{
			AssertUpdateStmPrintQueueLastUsedDate(PrintType.PRN);
		} 

		public void TestRunPRSTask_UpdateStmPrintQueueLastUsedDate()
		{
			AssertUpdateStmPrintQueueLastUsedDate(PrintType.PRS);
		}

		public void TestRunTask_WhenJobTypeIsPRN()
		{
			AssertTaskRun(nameof(PrintJobType.PRN));
		}

		public void TestRunTask_WhenJobTypeIsPRS()
		{
			AssertTaskRun(nameof(PrintJobType.PRS));
		}

		public void TestJobStillExistsAndAvailable_WhenJobDeleted_ReturnsFalse()
		{
			var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.PRN));
			var printJobTask = new PrintJobTask
			{
				ActionInAnotherInstance = () =>
				{
					Db.Connection.ExecuteNonQuery($"DELETE FROM dbo.{StmPrintJobSchema.Constants.TableName} WHERE {StmPrintJobSchema.Constants.PK} = '{printJob.PK}'");
				}
			};

			Factory.Save();

			using var mutexes = new DisposableList(0);
			var lockedPrintJobs = printJobTask.TryToLockJobs(new[] { printJob }, mutexes, Db.Connection);

			AssertEquals(0, lockedPrintJobs.Length);
		}

		public void TestJobStillExistsAndAvailable_CheckOnJobType()
		{
			var printJobTask = new PrintJobTask();

			using (var mutexes = new DisposableList(0))
			{
				var printJob = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintJobType.PRN));
				Factory.Save();

				var lockedPrintJobs = printJobTask.TryToLockJobs(new[] { printJob }, mutexes, Db.Connection);

				AssertEquals(1, lockedPrintJobs.Length);
			}

			using (var mutexes = new DisposableList(0))
			{
				var printJob = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintJobType.DDS));
				Factory.Save();

				var lockedPrintJobs = printJobTask.TryToLockJobs(new[] { printJob }, mutexes, Db.Connection);

				AssertEquals(0, lockedPrintJobs.Length);
			}

			using (var mutexes = new DisposableList(0))
			{
				var printJob = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintJobType.PRS));
				Factory.Save();

				var lockedPrintJobs = printJobTask.TryToLockJobs(new[] { printJob }, mutexes, Db.Connection);

				AssertEquals(1, lockedPrintJobs.Length);
			}
		}

		public void TestJobStillExistsAndAvailable_WhenPrintJobIsNotInTheQueue()
		{
			var printJob = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintJobType.PRN));
			var printJobTask = new PrintJobTask
			{
				ActionInAnotherInstance = () =>
				{
					printJob.SP_Status = nameof(PrintJobStatus.WRK);
					Factory.Save();
				}
			};

			Factory.Save();

			AssertEquals(true, Factory.ExistsInDatabase(StmPrintJobQueueSchema.Constants.TableName, new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJob.PK)));

			using var mutexes = new DisposableList(0);
			var lockedPrintJobs = printJobTask.TryToLockJobs(new[] { printJob }, mutexes, Db.Connection);

			AssertEquals(false, Factory.ExistsInDatabase(StmPrintJobQueueSchema.Constants.TableName, new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJob.PK)));
			AssertEquals(0, lockedPrintJobs.Length);
		}

		public void TestRunTask_WhenRunningWithManyTasks()
		{
			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			using (SafeInstalledPrinters.OverridePrintersForTesting(TestAssistant.PrintQueueNameForTesting))
			{
				var maintenanceTask = new PrintJobMaintenanceTask();
				maintenanceTask.RunTask();

				var queue = Factory.LoadTop1<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.SQ_DisplayName, TestAssistant.PrintQueueNameForTesting));
				AssertNotNull(queue);

				var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
				deliveryGroup.SB_IsProcessed = true;

				var jobPks = new ZGuid[20];

				for (var i = 0; i < 20; i++)
				{
					var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup.PK);
					printJob.SP_SQ = queue.PK;
					printJob.SP_RetryAttempts = 2;

					jobPks[i] = printJob.PK;
					TestHelper.AddPrintJobToQueue(printJob);
				}

				Factory.Save();

				var printJobTask = new PrintJobTask();
				InitialiseAndRunTaskSchedule(printJobTask);

				var processedJobs = jobPks.Select(pk => Factory.Load<StmPrintJob>(pk)).ToList();

				AssertEquals("Every job should have been notified of success", 0, processedJobs.Count(job =>
				{
					job.Reload();
					return job.SP_JobType != nameof(PrintJobType.DDS);
				}));
			}
		}

		public void TestRunTask_WhenPrintJobIsLockedByOtherServiceTask()
		{
			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			using (SafeInstalledPrinters.OverridePrintersForTesting(TestAssistant.PrintQueueNameForTesting))
			{
				var maintenanceTask = new PrintJobMaintenanceTask();
				maintenanceTask.RunTask();

				var queue = Factory.LoadTop1<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.SQ_DisplayName, TestAssistant.PrintQueueNameForTesting));
				AssertNotNull(queue);

				var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.PRN));
				printJob.SP_SQ = queue.PK;

				TestHelper.AddPrintJobToQueue(printJob);
				Factory.Save();

				TestServiceLogger log;
				var printJobTask = new PrintJobTask();

				using (var newConnection = Db.NewExtraConnectionToMainDb())
				{
					AssertEquals("Precondition: DeliveryGroup Lock should be acquired", true, PrintJobTaskCore.TryGetLockOnDeliveryGroupGuid(newConnection, printJob, out var deliveryGroupLock));

					using (deliveryGroupLock)
					{
						log = InitialiseAndRunTaskSchedule(printJobTask);
						AssertEquals(0, log.Count);
					}
				}

				log = InitialiseAndRunTaskSchedule(printJobTask);
				AssertEquals(3, log.Count);
				AssertEquals("Information|Starting HardCopyProcessor to process \"PRN\" print jobs", log[1]);
			}
		}

		#region TestExpectedJobTypes

		public void TestExpectedJobTypes()
		{
			var printJobTask = new PrintJobTaskForJobTypesTest();
			printJobTask.RunTask(CancellationToken.None);

			AssertEquals(1, printJobTask.JobTypesWithRequiredPrintServer.Count);
			AssertEquals(PrintJobType.PRN, printJobTask.JobTypesWithRequiredPrintServer[0]);

			AssertEquals(1, printJobTask.JobTypesWithoutRequiredPrintServer.Count);
			AssertEquals(PrintJobType.PRS, printJobTask.JobTypesWithoutRequiredPrintServer[0]);
		}

		class PrintJobTaskForJobTypesTest : PrintJobTask
		{
			public List<PrintJobType> JobTypesWithRequiredPrintServer { get; } = new();
			public List<PrintJobType> JobTypesWithoutRequiredPrintServer { get; } = new();

			protected override bool RunTaskForJobTypes(bool requiresPrintServer, CancellationToken token, params PrintJobType[] jobTypesToProcess)
			{
				foreach (var jobType in jobTypesToProcess)
				{
					if (requiresPrintServer)
					{
						JobTypesWithRequiredPrintServer.Add(jobType);
					}
					else
					{
						JobTypesWithoutRequiredPrintServer.Add(jobType);
					}
				}

				return true;
			}
		}

		#endregion

		#region Implementation

		PrintJobTaskTestHelper TestHelper => testHelper ??= new PrintJobTaskTestHelper(Factory);

		PrintJobTaskTestHelper testHelper;

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new (
						StmPrintJobQueueSchema.Constants.TableName,
						"Printing documents - JobType = 'PRN'",
						StmPrintJobQueueSchema.Constants.SPQ_JobType + "=PRN"),

					new (
						StmPrintJobQueueSchema.Constants.TableName,
						"Printing documents - JobType = 'PRS'",
						StmPrintJobQueueSchema.Constants.SPQ_JobType + "=PRS")
				};
			}
		}

		void AssertUpdateStmPrintQueueLastUsedDate(PrintType printType)
		{
			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			using (SafeInstalledPrinters.OverridePrintersForTesting(TestAssistant.PrintQueueNameForTesting))
			{
				var maintenanceTask = new PrintJobMaintenanceTask();
				maintenanceTask.RunTask();

				var testPrintQueue = Factory.LoadTop1<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.SQ_DisplayName, TestAssistant.PrintQueueNameForTesting));
				AssertNotNull(testPrintQueue);
				AssertEquals("SQ_LastUsedDateTimeUtc should empty", ZDateTime.Empty, testPrintQueue.SQ_LastUsedDateTimeUtc);

				var printJob = TestHelper.CreateTestPrintJobOnly(printType.ToString());
				printJob.SP_SQ = testPrintQueue.PK;
				printJob.SP_EDocsProcessed = ZBool.True;

				TestHelper.AddPrintJobToQueue(printJob);
				Factory.Save();

				var printJobTask = new PrintJobTask();
				InitialiseAndRunTaskSchedule(printJobTask);

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DocumentSentCode);
				query.AddToFilter(StmALogSchema.SL_Parent, printJob.SP_ParentGuid);
				var dsnLogs = Factory.Load<StmALog>(query);

				AssertNotEquals(0, dsnLogs.Length);

				var newFactory = new BusinessObjectFactory();
				var reloadedQueue = newFactory.Load<StmPrintQueue>(testPrintQueue.PK);
				var dateTimeNowLocal = ZDateTime.Now.ToString("yyyyMMddHHmm");
				var dateTimeNowUtc = ZDateTime.UtcNow.ToString("yyyyMMddHHmm");

				CombineAssertions(() =>
				{
					AssertEquals("SL_EventTime ", dateTimeNowLocal, dsnLogs[0].SL_EventTime.ToString("yyyyMMddHHmm"));
					AssertEquals("SQ_LastUsedDateTimeUtc", dateTimeNowUtc, reloadedQueue.SQ_LastUsedDateTimeUtc.ToString("yyyyMMddHHmm"));
				});
			}
		}

		void AssertTaskRun(string printType)
		{
			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			using (SafeInstalledPrinters.OverridePrintersForTesting(TestAssistant.PrintQueueNameForTesting))
			{
				var maintenanceTask = new PrintJobMaintenanceTask();
				maintenanceTask.RunTask();

				var testPrintQueue = Factory.LoadTop1<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.SQ_DisplayName, TestAssistant.PrintQueueNameForTesting));
				AssertNotNull(testPrintQueue);

				var printJob = TestHelper.CreateTestPrintJobOnly(printType);
				printJob.SP_SQ = testPrintQueue.PK;
				printJob.SP_EDocsProcessed = ZBool.True;

				TestHelper.AddPrintJobToQueue(printJob);
				Factory.Save();

				var printJobTask = new PrintJobTask();
				var log = InitialiseAndRunTaskSchedule(printJobTask);

				CombineAssertions(() =>
				{
					AssertEquals(3, log.Count);
					AssertEquals($"Information|Processing 1 of 1 queued {printType} jobs", log[0]);
					AssertEquals($"Information|Starting HardCopyProcessor to process \"{printType}\" print jobs", log[1]);
					AssertEquals("Information|Finished processing queue", log[2]);
				});
			}
		}

		#endregion
	}
}
