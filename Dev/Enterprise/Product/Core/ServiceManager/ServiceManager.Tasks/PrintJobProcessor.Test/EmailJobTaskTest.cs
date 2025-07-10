using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.PrintProcessing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(EmailJobTask))]
	sealed class EmailJobTaskTest : ServiceTaskTestCase<EmailJobTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "EDD", hostedServiceAttribute.Code);
				AssertEquals("Description", "Email and Fax documents delivery", hostedServiceAttribute.Description);
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

			var emailJobTask = new EmailJobTask
			{
				ActionInAnotherInstance = () =>
				{
					Db.Connection.ExecuteNonQuery($"UPDATE dbo.{StmPrintJobSchema.Constants.TableName} SET {StmPrintJobSchema.Constants.SP_JobType} = 'DDS' WHERE {StmPrintJobSchema.Constants.PK} = '{printJob.PK}'");
				}
			};

			InitialiseAndRunTaskSchedule(emailJobTask);

			AssertEquals(3, emailJobTask.LoggerForTest.LogEntries.Count());
			AssertEquals("Post lock. Job count: 0, mutex count: 1", emailJobTask.LoggerForTest.LogEntries.ToArray()[2]);
		}

		public void TestRunTask()
		{
			var printJob1 = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML));
			var deliveryGroupGuid = printJob1.SP_SB_DeliveryGroup;
			var parentGuid = printJob1.SP_ParentGuid;

			var printJob2 = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintJobType.EML), deliveryGroupGuid, parentGuid);
			printJob2.SP_Destination = "example2@example.com";

			Factory.Save();

			AssertEquals(1, Factory.GetDatabaseCount(typeof(StmDeliveryGroup), new ZQuery(StmDeliveryGroupSchema.PK, deliveryGroupGuid)));

			var emailJobTask = new EmailJobTask();
			var log = InitialiseAndRunTaskSchedule(emailJobTask);

			AssertEquals(2, TestHelper.CountMailDBItems("PrintJobTaskTest"));

			AssertEquals(7, log.Count);
			AssertEquals("Information|Processing 1 of 2 queued EML jobs", log[0]);
			AssertEquals("Information|Starting EmailProcessor to process \"EML\" print jobs", log[1]);
			AssertEquals("Information|Processing document \"TestDocument\" with subject \"PrintJobTaskTest\".\r\nDocument type [].\r\nRelated Business Context: [SHP].\r\nEmail Attachments: [test.XLS].\r\nNumber of Copies: [1].\r\nEmail To: [example@example.com].\r\nFax Destination:[].", log[2]);
			AssertEquals("Information|Processing 2 of 2 queued EML jobs", log[3]);
			AssertEquals("Information|Starting EmailProcessor to process \"EML\" print jobs", log[4]);
			AssertEquals("Information|Processing document \"TestDocument\" with subject \"PrintJobTaskTest\".\r\nDocument type [].\r\nRelated Business Context: [SHP].\r\nEmail Attachments: [test.XLS].\r\nNumber of Copies: [1].\r\nEmail To: [example2@example.com].\r\nFax Destination:[].", log[5]);
			AssertEquals("Information|Finished processing queue", log[6]);

			AssertEquals("No data purge in EDD service task", 1, Factory.GetDatabaseCount(typeof(StmDeliveryGroup), new ZQuery(StmDeliveryGroupSchema.PK, deliveryGroupGuid)));
		}

		public void TestDeliverDocumentPackWithMoreThan50DocumentsWouldCreateOnlyOneEmail()
		{
			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);

			var deliveryGroupGuid = TestAssistant.CreateNewDeliveryGroup(Factory).PK;
			var parentGuid = ZGuid.NewZGuid();

			//Up to MaxBatchSize => 1 single email
			Enumerable.Range(0, PrintJobTaskCore.BaseMaxBatchSize).ToList().ForEach(_ => TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML), deliveryGroupGuid, parentGuid));
			Factory.Save();

			AssertEquals(0, TestHelper.CountMailDBItems("PrintJobTaskTest"));

			InitialiseAndRunTaskSchedule(new EmailJobTask());

			AssertEquals(1, TestHelper.CountMailDBItems("PrintJobTaskTest"));

			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);

			//More than MaxBatchSize => 1 single emails
			Enumerable.Range(0, PrintJobTaskCore.BaseMaxBatchSize + 1).ToList().ForEach(_ => TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML), deliveryGroupGuid, parentGuid));
			Factory.Save();

			AssertEquals(0, TestHelper.CountMailDBItems("PrintJobTaskTest"));

			InitialiseAndRunTaskSchedule(new EmailJobTask());

			AssertEquals(1, TestHelper.CountMailDBItems("PrintJobTaskTest"));
		}

		public void TestRunTask_JobIsUpdatedForDocumentJobTask_WhenRelatedBusinessContextIsInvalid()
		{
			var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.EML));
			var deliveryGroupGuid = printJob.SP_SB_DeliveryGroup;
			printJob.SP_RetryAttempts = 2;
			TestHelper.AddPrintJobToQueue(printJob);

			Factory.Save();

			InitialiseAndRunTaskSchedule(new EmailJobTask());

			AssertEquals("EDD should not have cleaned up the StmDeliveryGroup record", 1, Factory.GetDatabaseCount(typeof(StmDeliveryGroup), new ZQuery(StmDeliveryGroupSchema.PK, deliveryGroupGuid)));

			var printJobReloaded = new BusinessObjectFactory().Load<StmPrintJob>(printJob.PK);

			AssertNotNull("Print job should not be deleted", printJobReloaded);
			AssertEquals("DDS", printJobReloaded.SP_JobType);
			AssertEquals((byte)0, printJobReloaded.SP_RetryAttempts);
			AssertNotEquals(deliveryGroupGuid, printJobReloaded.SP_SB_DeliveryGroup);
		}

		public void TestRunTask_JobIsDeleted_WhenRelatedBusinessContextIsInvalid()
		{
			var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.EML));
			var deliveryGroupGuid = printJob.SP_SB_DeliveryGroup;
			printJob.SP_RetryAttempts = 2;
			printJob.SP_RelatedBusinessContext = ZString.Empty;

			TestHelper.AddPrintJobToQueue(printJob);

			Factory.Save();

			InitialiseAndRunTaskSchedule(new EmailJobTask());

			AssertEquals("StmPrintJob should have been cleaned up", 0, Factory.GetDatabaseCount(typeof(StmPrintJob), new ZQuery(StmPrintJobSchema.PK, printJob.PK)));
			AssertEquals("EDD should not have cleaned up the StmDeliveryGroup record", 1, Factory.GetDatabaseCount(typeof(StmDeliveryGroup), new ZQuery(StmDeliveryGroupSchema.PK, deliveryGroupGuid)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStmPrintJobShouldBeDeletedAfterEDD()
		{
			var deliveryGroupGuid = TestAssistant.CreateNewDeliveryGroup(Factory).PK;
			var parentGuid = ZGuid.NewZGuid();
			var glowPortalsUri = "https://localhost/Glow/";

			using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var printJob = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML), deliveryGroupGuid, parentGuid);
				printJob.SP_RelatedBusinessContext = ZString.Empty;
				Factory.Save();

				InitialiseAndRunTaskSchedule(new EmailJobTask());
				AssertEquals("StmPrintJob should have been cleaned up as it's only processed by EmailProcessor", 0, Factory.GetDatabaseCount(typeof(StmPrintJob), new ZQuery(StmPrintJobSchema.PK, printJob.PK)));
			}

			using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUri))
			{
				var printJob = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML), deliveryGroupGuid, parentGuid);
				printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
				printJob.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.ReportStatistic;
				printJob.SP_ParentTableName = StmReportRunSchema.Constants.TableName;
				printJob.SP_DocumentType = Core.Constants.RefDocTypes.ScheduledReport;

				Factory.Save();

				var emailAttachmentSizeLimitInMB = 0;
				using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emailAttachmentSizeLimitInMB))
				{
					InitialiseAndRunTaskSchedule(new EmailJobTask());
					AssertEquals("StmPrintJob should have been cleaned up as it's processed by EmailProcessor & EDocProcessor", 0, Factory.GetDatabaseCount(typeof(StmPrintJob), new ZQuery(StmPrintJobSchema.PK, printJob.PK)));
				}
			}

			using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUri))
			{
				var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.EML), deliveryGroupGuid, parentGuid);
				printJob.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.ReportStatistic;
				printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
				printJob.SP_ParentTableName = StmReportRunSchema.Constants.TableName;
				printJob.SP_DocumentType = Core.Constants.RefDocTypes.ScheduledReport; 
				printJob.SP_EDocsProcessed = ZBool.True;

				TestHelper.AddPrintJobToQueue(printJob);

				Factory.Save();

				var emailAttachmentSizeLimitInMB = 0;
				using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emailAttachmentSizeLimitInMB))
				{
					InitialiseAndRunTaskSchedule(new EmailJobTask());
					AssertEquals("StmPrintJob should have been cleaned up as it's processed by EmailProcessor & EDocProcessor", 0, Factory.GetDatabaseCount(typeof(StmPrintJob), new ZQuery(StmPrintJobSchema.PK, printJob.PK)));
				}
			}

			using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUri))
			{
				var printJob = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.EML), deliveryGroupGuid, parentGuid);
				printJob.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.ReportStatistic;
				printJob.SP_ParentTableName = StmReportRunSchema.Constants.TableName;
				printJob.SP_DocumentType = Core.Constants.RefDocTypes.ScheduledReport;

				Factory.Save();

				var emailAttachmentSizeLimitInMB = printJob.SP_CustomProperties.Length;
				using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emailAttachmentSizeLimitInMB))
				{
					InitialiseAndRunTaskSchedule(new EmailJobTask());
					AssertEquals("StmPrintJob should have been cleaned up as it's only processed by EmailProcessor and doesn't require further process by EDocProcessor", 0, Factory.GetDatabaseCount(typeof(StmPrintJob), new ZQuery(StmPrintJobSchema.PK, printJob.PK)));
				}
			}

			using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUri))
			{
				var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintType.EML), deliveryGroupGuid, parentGuid);
				printJob.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.ReportStatistic;
				printJob.SP_ParentTableName = StmReportRunSchema.Constants.TableName;
				printJob.SP_DocumentType = Core.Constants.RefDocTypes.ScheduledReport;
				printJob.SP_EDocsProcessed = ZBool.True;

				TestHelper.AddPrintJobToQueue(printJob);

				Factory.Save();

				var emailAttachmentSizeLimitInMB = printJob.SP_CustomProperties.Length;
				using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emailAttachmentSizeLimitInMB))
				{
					InitialiseAndRunTaskSchedule(new EmailJobTask());
					AssertEquals("StmPrintJob should have been cleaned up as it's only processed by EmailProcessor and doesn't require further process by EDocProcessor", 0, Factory.GetDatabaseCount(typeof(StmPrintJob), new ZQuery(StmPrintJobSchema.PK, printJob.PK)));
				}
			}
		}

		public void TestGetJobsToSaveToEDocsQuery()
		{
			var documentJobTask = new EmailJobTask();
			var result = documentJobTask.TryGetJobsOfGivenTypesQuery(new[] { PrintJobType.EML, PrintJobType.FAX }, null, out var query);

			AssertEquals(true, result);
			AssertEquals("dbo.GetNextPrintJob", query.ParameterisedQueryText);
			AssertEquals(1, query.Parameters.Length);
			AssertEquals("SP_JobType = 'EML', 'FAX'", query.Parameters[0].LiteralTextADO);
			AssertEquals("@JobTypes", query.Parameters[0].ParameterName);
		}

		#region Implementation

		PrintJobTaskTestHelper TestHelper => testHelper ??= new PrintJobTaskTestHelper(Factory);

		PrintJobTaskTestHelper testHelper;

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new (StmPrintJobQueueSchema.Constants.TableName, "Email delivery", StmPrintJobQueueSchema.Constants.SPQ_JobType + "=EML"),
					new (StmPrintJobQueueSchema.Constants.TableName, "Fax delivery", StmPrintJobQueueSchema.Constants.SPQ_JobType + "=FAX")
				};
			}
		}

		#endregion
	}
}
