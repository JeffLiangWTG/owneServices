using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.PrintProcessing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(DocumentJobTask))]
	sealed class DocumentJobTaskTest : ServiceTaskTestCase<DocumentJobTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);

			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "DOD", hostedServiceAttribute.Code);
				AssertEquals("Description", "Documents delivery", hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", true, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			});
		}

		public void TestRunTaskWithJobProcessedByAnotherInstance()
		{
			var printJob = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML));

			Factory.Save();

			var task = new DocumentJobTask
			{
				ActionInAnotherInstance = () =>
				{
					var reloadedPrintJob = new BusinessObjectFactory { RefreshEnabled = false }.Load<StmPrintJob>(printJob.PK);
					reloadedPrintJob.SP_EDocsProcessed = true;
					reloadedPrintJob.Factory.Save();
				}
			};

			InitialiseAndRunTaskSchedule(task);

			AssertEquals(3, task.LoggerForTest.LogEntries.Count());
			AssertEquals("Post lock. Job count: 0, mutex count: 2", task.LoggerForTest.LogEntries.ToArray()[2]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTask_OnlyProcessJobsWhoseEDocsProcessedIsFalse()
		{
			var printJob1 = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.EML));
			printJob1.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			var deliveryGroupGuid = printJob1.SP_SB_DeliveryGroup;
			var parentGuid = printJob1.SP_ParentGuid;

			var printJob2 = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.EML), deliveryGroupGuid);
			printJob2.SP_EDocsProcessed = ZBool.True;

			TestHelper.AddPrintJobToQueue(printJob1);
			TestHelper.AddPrintJobToQueue(printJob2);
			Factory.Save();

			AssertEquals(1, Factory.GetDatabaseCount(typeof(StmDeliveryGroup), new ZQuery(StmDeliveryGroupSchema.PK, deliveryGroupGuid)));

			var loopCount = 0;
			var jobTask = new DocumentJobTask
			{
				ActionInAnotherInstance = () =>
				{
					++loopCount;
					AssertEquals("Can only loop once", 1, loopCount);
				}
			};

			using (jobTask)
			{
				var log = InitialiseAndRunTaskSchedule(jobTask);

				CombineAssertions(() =>
				{
					AssertEquals(6, log.Count);
					AssertEquals("Information|Processing 1 of 1 queued EML jobs", log[0]);
					AssertEquals("Information|Starting EDocProcessor to process \"EML\" print jobs", log[1]);
					AssertEquals("Information|Allocating document \"TestDocument\" of type \"\" with subject \"PrintJobTaskTest\" to eDocs for (SHP, " + parentGuid + ")", log[2]);
					AssertEquals("Warning|Document 'PrintJobTaskTest' of type 'MSC' was allocated, but not registered with business object.", log[3]);
					AssertEquals("Information|Finished allocating document \"TestDocument\" of type \"\"", log[4]);
					AssertEquals("Information|Finished processing queue", log[5]);
				});
			}
		}

		public void TestRunTask_PrintJobsShouldNotBeDeleted_WhenTypeIsNotDDS()
		{
			var printJobOfEml = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.EML));
			printJobOfEml.SP_RelatedBusinessContext = "";
			printJobOfEml.SP_RunDateTime = printJobOfEml.SP_RunDateTime.AddHours(-1);
			printJobOfEml.SP_Sequence = 1;

			var deliveryGroupGuid = printJobOfEml.SP_SB_DeliveryGroup;
			var printJobOfDds = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.DDS), deliveryGroupGuid);
			printJobOfDds.SP_RelatedBusinessContext = "";
			printJobOfDds.SP_Sequence = 2;

			TestHelper.AddPrintJobToQueue(printJobOfEml).SPQ_Sequence = 1;
			TestHelper.AddPrintJobToQueue(printJobOfDds).SPQ_Sequence = 2;

			Factory.Save();

			AssertEquals(2, Factory.GetDatabaseCount(typeof(StmPrintJob), new ZQuery(StmPrintJobSchema.SP_EDocsProcessed, ZBool.False)));
			AssertEquals(2, Factory.GetDatabaseCount(typeof(StmPrintJob), new ZQuery(StmPrintJobSchema.SP_SB_DeliveryGroup, deliveryGroupGuid)));

			using var documentJobTask = new DocumentJobTask();
			var log = InitialiseAndRunTaskSchedule(documentJobTask);

			AssertEquals(3, log.Count);
			AssertEquals("Information|Processing 1 of 2 queued EML jobs", log[0]);
			AssertEquals("Information|Processing 2 of 2 queued DDS jobs", log[1]);
			AssertEquals("Information|Finished processing queue", log[2]);

			AssertEquals(true, Factory.ExistsInDatabase(StmPrintJobSchema.Constants.TableName, new ZQuery(StmPrintJobSchema.PK, printJobOfEml.PK)));
			AssertEquals(false, Factory.ExistsInDatabase(StmPrintJobSchema.Constants.TableName, new ZQuery(StmPrintJobSchema.PK, printJobOfDds.PK)));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(StmDeliveryGroup), new ZQuery(StmDeliveryGroupSchema.SB_IsProcessed, ZBool.True)));
		}

		public void TestDeliveryGroupIsDeleted_WhenNoPrintJobIncluded()
		{
			var printJob1 = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML));
			printJob1.SP_RelatedBusinessContext = "";

			var printJob2 = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.DDS));
			printJob2.SP_RelatedBusinessContext = "";

			var deliveryGroupGuid1 = printJob1.SP_SB_DeliveryGroup;
			var deliveryGroupGuid2 = printJob2.SP_SB_DeliveryGroup;

			Factory.Save();

			AssertEquals(true, Factory.ExistsInDatabase(StmDeliveryGroupSchema.Constants.TableName, new ZQuery(StmDeliveryGroupSchema.PK, deliveryGroupGuid1)));
			AssertEquals(true, Factory.ExistsInDatabase(StmDeliveryGroupSchema.Constants.TableName, new ZQuery(StmDeliveryGroupSchema.PK, deliveryGroupGuid2)));

			using var documentJobTask = new DocumentJobTask();
			InitialiseAndRunTaskSchedule(documentJobTask);

			AssertEquals(true, Factory.ExistsInDatabase(StmDeliveryGroupSchema.Constants.TableName, new ZQuery(StmDeliveryGroupSchema.PK, deliveryGroupGuid1)));
			AssertEquals(false, Factory.ExistsInDatabase(StmDeliveryGroupSchema.Constants.TableName, new ZQuery(StmDeliveryGroupSchema.PK, deliveryGroupGuid2)));
		}

		public void TestTryToLockJobs()
		{
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);
			var deliveryGroup2 = TestAssistant.CreateNewDeliveryGroup(Factory);
			var testParentGuid1 = ZGuid.NewZGuid();
			var testParentGuid2 = ZGuid.NewZGuid();

			var printJob1 = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML), deliveryGroup1.PK, testParentGuid1);
			var printJob2 = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML), deliveryGroup1.PK, testParentGuid2);
			var printJob3 = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML), deliveryGroup2.PK, testParentGuid1);
			var printJob4 = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML), deliveryGroup2.PK, testParentGuid2);

			Factory.Save();

			var printJobs = new[] { printJob1, printJob2, printJob3, printJob4 };

			using var mutexes = new DisposableList(0);
			using var connection1 = Db.NewExtraConnectionToMainDb();
			using var connection2 = Db.NewExtraConnectionToMainDb();

			AssertEquals("Precondition: DeliveryGroup Lock should be acquired", true, PrintJobTaskCore.TryGetLockOnDeliveryGroupGuid(connection1, printJob1, out var deliveryGroupLock));
			AssertEquals("Precondition: ParentGuid Lock should be acquired", true, DocumentJobTask.TryGetLockOnParentGuid(connection1, printJob1, out var parentGuidLock));

			using (deliveryGroupLock)
			using (parentGuidLock)
			using (var task = new DocumentJobTask())
			{
				var jobs = task.TryToLockJobs(printJobs, mutexes, connection2);
				AssertEquals(1, jobs.Length);
				AssertEquals(printJob4.PK, jobs[0].PK);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunWithNoConcurrency()
		{
			var printJobToLock = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.PRN));
			var deliveryGroupGuid = printJobToLock.SP_SB_DeliveryGroup;

			var printJobToBeProcessed = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.PRN), deliveryGroupGuid);
			printJobToBeProcessed.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			var parentGuid = printJobToBeProcessed.SP_ParentGuid;

			TestHelper.AddPrintJobToQueue(printJobToBeProcessed);
			Factory.Save();

			using var connection = Db.NewExtraConnectionToMainDb();

			AssertEquals("Precondition: Lock should be acquired", true, DocumentJobTask.TryGetLockOnParentGuid(connection, printJobToLock, out var lockResult));

			using (lockResult)
			using (var task = new DocumentJobTask())
			{
				var log = InitialiseAndRunTaskSchedule(task);

				AssertEquals(6, log.Count);
				AssertEquals("Information|Processing 1 of 1 queued PRN jobs", log[0]);
				AssertEquals("Information|Starting EDocProcessor to process \"PRN\" print jobs", log[1]);
				AssertEquals("Information|Allocating document \"TestDocument\" of type \"\" with subject \"PrintJobTaskTest\" to eDocs for (SHP, " + parentGuid + ")", log[2]);
				AssertEquals("Warning|Document 'PrintJobTaskTest' of type 'MSC' was allocated, but not registered with business object.", log[3]);
				AssertEquals("Information|Finished allocating document \"TestDocument\" of type \"\"", log[4]);
				AssertEquals("Information|Finished processing queue", log[5]);
			}
		}

		public void TestGetJobsToSaveToEDocsQuery_GetAllPrintJobsFromSameDeliveryGroup()
		{
			// Delivery group 1
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);
			var printJob1 = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.DDS), deliveryGroup1.PK); 
			printJob1.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-5);

			var printJob2 = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.DDS), deliveryGroup1.PK);
			printJob2.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-5);

			TestHelper.AddPrintJobToQueue(printJob1, queueSequence: 1L);
			TestHelper.AddPrintJobToQueue(printJob2, queueSequence: 1L);

			// Delivery group 2
			var deliveryGroup2 = TestAssistant.CreateNewDeliveryGroup(Factory);
			var printJob3 = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.DDS), deliveryGroup2.PK, queueSequence: 2L);

			Factory.Save();

			var printJobPKs = new List<ZGuid>();

			using (var documentJobTask = new DocumentJobTask())
			{
				var result = documentJobTask.TryGetJobsOfGivenTypesQuery([], null, out var query);

				AssertEquals("Query is generated successfully", true, result);

				var cmd = new ZSqlConnectionInfo(Db.Connection, Db.DatabaseName).GetNewDbCommandForStoredProcedure(query.ParameterisedQueryText, query.Parameters);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						printJobPKs.Add((Guid)reader[0]);
					}
				}
			}

			AssertEquals("Should have 2 print jobs loaded", 2, printJobPKs.Count);
			AssertCollectionContains(printJob1.PK, printJobPKs);
			AssertCollectionContains(printJob2.PK, printJobPKs);
			AssertCollectionNotContains(printJob3.PK, printJobPKs);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTask_WhenPrintJobIsLockedByOtherServiceTask()
		{
			var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.PRN));
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			var parentGuid = printJob.SP_ParentGuid;

			TestHelper.AddPrintJobToQueue(printJob);
			Factory.Save();

			TestServiceLogger log;

			var documentJobTask = new DocumentJobTask();

			using (var newConnection = Db.NewExtraConnectionToMainDb())
			{
				AssertEquals("Precondition: DeliveryGroup Lock should be acquired", true, PrintJobTaskCore.TryGetLockOnDeliveryGroupGuid(newConnection, printJob, out var deliveryGroupLock));

				using (deliveryGroupLock)
				{
					log = InitialiseAndRunTaskSchedule(documentJobTask);
					AssertEquals(0, log.Count);
				}
			}

			log = InitialiseAndRunTaskSchedule(documentJobTask);
			AssertEquals(6, log.Count);
			AssertEquals("Information|Allocating document \"TestDocument\" of type \"\" with subject \"PrintJobTaskTest\" to eDocs for (SHP, " + parentGuid + ")", log[2]);
		}

		public void TestJobStillExistsAndAvailable_WhenPrintJobIsGone()
		{
			var printJob = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.PRN));

			Factory.Save();

			var documentJobTask = new DocumentJobTask
			{
				ActionInAnotherInstance = () =>
				{
					Db.Connection.ExecuteNonQuery($"DELETE FROM dbo.{StmPrintJobSchema.Constants.TableName} WHERE {StmPrintJobSchema.Constants.PK} = '{printJob.PK}'");
				}
			};

			using var mutexes = new DisposableList(0);
			var lockedPrintJobs = documentJobTask.TryToLockJobs(new[] { printJob }, mutexes, Db.Connection);

			AssertEquals(0, lockedPrintJobs.Length);
		}

		public void TestJobStillExistsAndAvailable_WhenPrintJobIsNotInTheQueue()
		{
			var printJob = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.PRN));

			Factory.Save();

			AssertEquals(true, Factory.ExistsInDatabase(StmPrintJobQueueSchema.Constants.TableName, new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJob.PK)));

			var documentJobTask = new DocumentJobTask
			{
				ActionInAnotherInstance = () =>
				{
					printJob.SP_EDocsProcessed = true;
					Factory.Save();
				}
			};

			using var mutexes = new DisposableList(0);
			var lockedPrintJobs = documentJobTask.TryToLockJobs(new[] { printJob }, mutexes, Db.Connection);

			AssertEquals("Print Job still should be queued (for printing)", true, Factory.ExistsInDatabase(StmPrintJobQueueSchema.Constants.TableName, new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJob.PK)));
			AssertEquals("Job should not be locked for eDoc processing (already processed)", 0, lockedPrintJobs.Length);
		}

		public void TestGetJobsToSaveToEDocsQuery()
		{
			var documentJobTask = new DocumentJobTask();
			var result = documentJobTask.TryGetJobsOfGivenTypesQuery(Array.Empty<PrintJobType>(), null, out var query);

			AssertEquals(true, result);
			AssertEquals("dbo.GetNextPrintJobForSaveToEDocs", query.ParameterisedQueryText);
		}

		#region Implementations

		PrintJobTaskTestHelper TestHelper => testHelper ??= new PrintJobTaskTestHelper(Factory);

		PrintJobTaskTestHelper testHelper;

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new[]
				{
					new TaskNudgeInformationForTest(
						StmPrintJobQueueSchema.Constants.TableName,
						"Document Delivery - EDocsProcessed = '0'",
						StmPrintJobQueueSchema.Constants.SPQ_EDocsProcessed + "=0")
				};
			}
		}

		#endregion
	}
}
