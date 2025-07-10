using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.PrintProcessing.Test.Processors;
using Enterprise.Registry.Business;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FlexCel.XlsAdapter;
using Moq;
using NUnit.Framework;

namespace Enterprise.PrintProcessing.Testing
{
	sealed class PrintJobManagerTest : TestCaseWithFactory
	{
		public void TestReportErrorWhenProcess()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			var stmPrintJob = Factory.New<StmPrintJob>();
			stmPrintJob.SP_JobType = "EML";
			stmPrintJob.SP_Destination = "test@example.com";
			stmPrintJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			var exceptionToThrow = new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((IBusinessObjectInternals)stmPrintJob).Row, Db.Connection), Factory)
			{
				Source = "Any"
			};
			var manager = new PrintJobManagerForTesting()
			{
				ExceptionToThrowWhenProcessingPrintJobs = exceptionToThrow
			};

			var mockGuidPlugin = new Mock<IDbConnectionGuiPlugin>();
			var mockDbEnv = new Mock<BaseDbEnvironment>();
			mockDbEnv.Setup(m => m.ConnectionGuiPlugin)
				.Returns(mockGuidPlugin.Object);

			using (DbEnv.SetTemporaryDbEnvironment(mockDbEnv.Object))
			{
				AssertNoExceptionThrown(() => manager.ProcessPrintJobs([stmPrintJob]));
				AssertEquals(nameof(ZSaveConcurrencyException), ExceptionReporterTestListener.Instance[0].GetType().Name);
				AssertNotEquals("Put Factory.Save() in a try/catch. This exception was caught by the top level exception reporter.", ExceptionReporterTestListener.Instance[0].Message);
				AssertEquals("ProcessPrintJobs_ZSaveConcurrencyException", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
		}

		#region TestFtpProcessorNotification

		public void TestFtpProcessorNotification()
		{
			PrintJobManager manager = new PrintJobManager();
			manager.OnNotification += (eventType, message) => { };
			var printJobMergedCollection = new StmPrintJobMergedCollection(Factory);
			var printJob = printJobMergedCollection.AddNew();
			printJob.SP_JobType = nameof(PrintType.FTP);
			printJob.SP_RelatedBusinessContext = "SHP";
			var processor = PrintJobManager.ProcessorFactory.GetProcessor(printJobMergedCollection, null, manager, manager.OnNotification);
			AssertEquals(typeof(FtpJobProcessor), processor.GetType());
			AssertNotNull((processor as FtpJobProcessor).runNotification);
		}

		#endregion

		#region SMS Processing

		#region TestNoSmsMessageProcessedWhenSmsSenderNotRegistered

		public void TestNoSmsMessageProcessedWhenSmsSenderNotRegistered()
		{
			Sms sms = new Sms("123", "sms message");
			SmsSender sender = SmsSender.New();
			sender.Send(sms);

			DummySmsSender.UnregisterThisSubTypeOverride();
			AssertNull("Precondition - SmsSender should be unregistered.", SmsSender.New());

			PrintJobManager manager = new PrintJobManager();
			SmsProcessor.IsProcessStartedForTesting = false;
			manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertEquals("No SmsSender is registered, the SMS processor should not have been started.", false, SmsProcessor.IsProcessStartedForTesting);
		}

		#endregion

		#region TestSmsProcessorProcessesJob

		public void TestSmsProcessorProcessesJob()
		{
			SmsSender sender = SmsSender.New();
			PrintJobManager manager = new PrintJobManager();

			Sms sms = new Sms("123", "sms message");
			sender.Send(sms);

			ZQuery smsJobsQuery = new ZQuery(StmPrintJobSchema.SP_JobType, "SMS");
			AssertEquals("Precondition - SMS jobs in DB", 1, Factory.GetDatabaseCount(typeof(StmPrintJob), smsJobsQuery));

			manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertEquals("SMS jobs in DB", 0, Factory.GetDatabaseCount(typeof(StmPrintJob), smsJobsQuery));
		}

		#endregion

		#region TestSmsProcessorIncreasesRetrySendFailureAndRunDate

		[TestDate(2007, 1, 9, 0, 0, 0)]
		public void TestSmsProcessorIncreasesRetrySendFailureAndRunDate()
		{
			SmsSender sender = SmsSender.New();
			PrintJobManager manager = new PrintJobManager();

			Sms sms = new Sms("123", "sms message");
			sender.Send(sms);

			ZQuery smsJobsQuery = new ZQuery(StmPrintJobSchema.SP_JobType, "SMS");
			StmPrintJob job = Factory.LoadTop1<StmPrintJob>(smsJobsQuery);
			AssertEquals("Precondition", (byte)0, job.SP_RetryAttempts);

			DummySmsSender.SimulateSuccessOnNextSend = false;
			manager.ProcessPrintJobs(GetPrintJobsToDeliver());

			StmPrintJob jobLoadedFromDb = Factory.Load<StmPrintJob>(job.PK);
			AssertEquals((byte)1, jobLoadedFromDb.SP_RetryAttempts);
			AssertEquals(ZDateTime.UtcNow.AddSeconds(SmsProcessor.RetryTimeoutInSeconds), jobLoadedFromDb.SP_RunDateTime);
		}

		#endregion

		#region TestSmsProcessorLogsSuccessMessage

		public void TestSmsProcessorLogsSuccessMessage()
		{
			SmsSender sender = SmsSender.New();
			PrintJobManager manager = new PrintJobManager();

			bool successLoggedOk = false;
			manager.OnProgress += (eventType, message) =>
				successLoggedOk |= (message == "SMS sent. SMS Number: 123. SMS Message: sms message.");

			sender.Send(new Sms("123", "sms message"));
			DummySmsSender.SimulateSuccessOnNextSend = true;
			manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertEquals("Success log should have been created for successful SMS send.", true, successLoggedOk);
			successLoggedOk = false;

			sender.Send(new Sms("456", "sms message"));
			DummySmsSender.SimulateSuccessOnNextSend = false;
			manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertEquals("Success log should *not* have been created for failed SMS send.", false, successLoggedOk);
		}

		#endregion

		#region TestSmsProcessorLogsFailureMessage

		public void TestSmsProcessorLogsFailureMessage()
		{
			SmsSender sender = SmsSender.New();
			PrintJobManager manager = new PrintJobManager();

			bool errorLoggedOk = false;
			manager.OnProgress += (eventType, message) =>
				errorLoggedOk |= (message == "SMS send failed, attempt 1. SMS Number: 123. SMS Message: sms message. Error Message: message from SmsSender");

			sender.Send(new Sms("123", "sms message"));
			DummySmsSender.SimulateSuccessOnNextSend = false;
			manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertEquals("Error log should have been created for failed SMS send.", true, errorLoggedOk);
			errorLoggedOk = false;

			sender.Send(new Sms("456", "sms message"));
			DummySmsSender.SimulateSuccessOnNextSend = true;
			manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertEquals("Error log should *not* have been created for successful SMS send.", false, errorLoggedOk);
		}

		#endregion

		#region class DummySmsSender

		public class DummySmsSender : SmsSender
		{
			public static void RegisterThisSubTypeOverride()
			{
				NewSmsSenderDelegate = GetNewDummySmsSender;
			}

			public static void UnregisterThisSubTypeOverride()
			{
				NewSmsSenderDelegate = null;
			}

			static DummySmsSender GetNewDummySmsSender()
			{
				return new DummySmsSender();
			}

			protected override SmsSendResult SendCore(Sms sms)
			{
				SendCount++;
				LastSentSmsPhoneNumber = sms.PhoneNumbers[0];
				LastSentSmsMessage = sms.Message;

				return new SmsSendResult(SimulateSuccessOnNextSend, "message from SmsSender");
			}

			public static void ResetStaticTestData()
			{
				SendCount = 0;
				LastSentSmsPhoneNumber = "";
				LastSentSmsMessage = "";
				SimulateSuccessOnNextSend = true;
			}

			public static int SendCount;
			public static ZString LastSentSmsPhoneNumber;
			public static ZString LastSentSmsMessage;
			public static bool SimulateSuccessOnNextSend = true;
		}

		#endregion

		#endregion

		#region TestGetProcessor

		public void TestGetProcessorForDDS()
		{
			var printJobMergedCollection = new StmPrintJobMergedCollection(Factory);
			printJobMergedCollection.AddNew().SP_JobType = nameof(PrintType.DDS);
			AssertEquals(null, PrintJobManager.ProcessorFactory.GetProcessor(printJobMergedCollection, null));
		}

		public void TestGetProcessorForFAX()
		{
			var printJobMergedCollection = new StmPrintJobMergedCollection(Factory);
			printJobMergedCollection.AddNew().SP_JobType = nameof(PrintType.FAX);
			AssertEquals(typeof(FaxProcessor), PrintJobManager.ProcessorFactory.GetProcessor(printJobMergedCollection, null).GetType());
		}

		public void TestGetProcessorForEML()
		{
			var printJobMergedCollection = new StmPrintJobMergedCollection(Factory);
			printJobMergedCollection.AddNew().SP_JobType = nameof(PrintType.EML);
			AssertEquals(typeof(EmailProcessor), PrintJobManager.ProcessorFactory.GetProcessor(printJobMergedCollection, null).GetType());
		}

		public void TestGetProcessorForFtp()
		{
			var printJobMergedCollection = new StmPrintJobMergedCollection(Factory);
			printJobMergedCollection.AddNew().SP_JobType = nameof(PrintType.FTP);
			AssertEquals(typeof(FtpJobProcessor), PrintJobManager.ProcessorFactory.GetProcessor(printJobMergedCollection, null).GetType());
		}

		public void TestGetProcessorForPRN()
		{
			var printJobMergedCollection = new StmPrintJobMergedCollection(Factory);
			printJobMergedCollection.AddNew().SP_JobType = nameof(PrintType.PRN);
			AssertEquals(typeof(HardCopyProcessor), PrintJobManager.ProcessorFactory.GetProcessor(printJobMergedCollection, null).GetType());
		}

		#endregion

		public void TestBumpUpRetryAttempt_WhenNonConcurrencyException()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");
			var printJob1 = Factory.New<StmPrintJob>();
			printJob1.SP_JobType = nameof(PrintType.EML);
			printJob1.SP_EmailAttachmentFormat = "PDF";
			printJob1.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-1);
			printJob1.SP_DocumentName = "Test";
			printJob1.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob1.SP_EmailAttachments = PrintProcessingConstants.TestGeneratedReport;

			var printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = nameof(PrintType.EML);
			printJob2.SP_EmailAttachmentFormat = "PDF";
			printJob2.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-1);
			printJob2.SP_DocumentName = "Test";
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob2.SP_EmailAttachments = PrintProcessingConstants.TestGeneratedReport;

			Factory.Save();

			var onProgressResults = new ZStringBuilder();
			var manager = new PrintJobManager();
			manager.OnProgress += (_, message) => onProgressResults.AppendIfNotEmpty(message);

			var listStmPrintJobs = GetPrintJobsToDeliver();
			listStmPrintJobs.Select(c => c).First().Factory.Saving += _ =>
			{
				// It's on purpose for testing all print jobs will be loaded when bumping up SP_RetryAttempts/
				var sql = string.Format($"UPDATE dbo.StmPrintJob SET SP_JobType = 'DDS' WHERE SP_PK = '{printJob1.PK}'");
				using var command = Db.Connection.Command(sql);
				command.ExecuteNonQuery();

				throw new OutOfMemoryException("testing");
			};

			AssertExceptionThrown<OutOfMemoryException>(() => { manager.ProcessPrintJobs(listStmPrintJobs); });
			AssertEquals("RetryAttempts has been increased, should equal", (short)1, printJob1.SP_RetryAttempts);
			AssertEquals("RetryAttempts has been increased, should equal", (short)1, printJob2.SP_RetryAttempts);
			ErrorReporter.Clear();
		}

		public void TestBumpUpRetryAttempt_IgnoreProcessedRecords_WhenConcurrencyException()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");
			var printJob1 = Factory.New<StmPrintJob>();
			printJob1.SP_JobType = nameof(PrintType.EML);
			printJob1.SP_EmailAttachmentFormat = "PDF";
			printJob1.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-1);
			printJob1.SP_DocumentName = "Test";
			printJob1.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob1.SP_EmailAttachments = PrintProcessingConstants.TestGeneratedReport;

			var printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = nameof(PrintType.EML);
			printJob2.SP_EmailAttachmentFormat = "PDF";
			printJob2.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-1);
			printJob2.SP_DocumentName = "Test";
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob2.SP_EmailAttachments = PrintProcessingConstants.TestGeneratedReport;

			Factory.Save();

			var onProgressResults = new ZStringBuilder();
			var manager = new PrintJobManager();
			manager.OnProgress += (_, message) => onProgressResults.AppendIfNotEmpty(message);
			var listStmPrintJobs = GetPrintJobsToDeliver();
			listStmPrintJobs.Select(c => c).First().Factory.Saving += f =>
			{
				var row = ((IBusinessObjectInternals)Factory.Load<StmPrintJob>(new ZQuery()).First(x => x.PK == printJob1.PK)).Row;

				var sql = string.Format($"UPDATE dbo.StmPrintJob SET SP_JobType = 'DDS' WHERE SP_PK = '{printJob1.PK}'");
				using var command = Db.Connection.Command(sql);
				command.ExecuteNonQuery();

				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(""), row, ((IDbConnected)f).Connection), f);
			};

			AssertNoExceptionThrown(() => { manager.ProcessPrintJobs(listStmPrintJobs); });
			AssertEquals("For print job 1, SP_RetryAttempts is still 0", (short)0, printJob1.SP_RetryAttempts);
			AssertEquals("For print job 2, SP_RetryAttempts has been increased, should equal", (short)1, printJob2.SP_RetryAttempts);
			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		public void TestBumpUpRetryAttemptWhenCheckFailureException()
		{
			var sql = string.Format(@"INSERT dbo.StmPrintJob ( SP_PK, SP_JobType, SP_EmailAttachmentFormat,	SP_RunDateTime,	SP_DocumentName, SP_EmailAttachments)
VALUES  (@PK, @SJobType, @EmailAttachmentFormat , @RunDateTime,	@DocumentName, @EmailAttachments) ");
			using (var command = Db.Connection.Command(sql))
			{
				var printJobPK = ZGuid.NewZGuid();

				command.AddParameterBasedOnDbColumn("@PK", printJobPK.ToGuid(), StmPrintJobSchema.PK);
				command.AddParameterBasedOnDbColumn("@SJobType", nameof(PrintType.EML), StmPrintJobSchema.SP_JobType);
				command.AddParameterBasedOnDbColumn("@EmailAttachmentFormat", "PDF", StmPrintJobSchema.SP_EmailAttachmentFormat);
				command.AddParameterBasedOnDbColumn("@RunDateTime", ZDateTime.UtcNow.ToDateTime(), StmPrintJobSchema.SP_RunDateTime);
				command.AddParameterBasedOnDbColumn("@DocumentName", PrintProcessingConstants.TestGeneratedReport, StmPrintJobSchema.SP_DocumentName);
				command.AddParameterBasedOnDbColumn("@EmailAttachments", PrintProcessingConstants.TestGeneratedReport, StmPrintJobSchema.SP_EmailAttachments);

				command.ExecuteNonQuery();
				BusinessObjectFactory.SavingEventHandler action = f =>
				{
					throw new ZConcurrencyCheckFailureException("Another user has converted the booking into a shipment.", "Booking concurrency error", true);
				};

				var onProgressResults = new ZStringBuilder();
				var manager = new PrintJobManager();
				manager.OnProgress += (eventType, message) => onProgressResults.AppendIfNotEmpty(message);
				var listStmPrintJobs = GetPrintJobsToDeliver();
				listStmPrintJobs.Select(c => c).First().Factory.Saving += action;

				var printJob = Factory.Load<StmPrintJob>(new ZQuery()).First(x => x.PK == printJobPK);
				AssertNoExceptionThrown(() => { manager.ProcessPrintJobs(listStmPrintJobs); });
				AssertEquals("RetryAttempts has been increased, should equal", (short)1, printJob.SP_RetryAttempts);

				ErrorReporter.Clear();
			}
		}

		public void TestBumpUpRetryAttemptWhenThereIsCriticalException()
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "PDF";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "Test";
			printJob.SP_EmailAttachments = PrintProcessingConstants.TestGeneratedReport;
			Factory.Save();

			var manager = new PrintJobManagerForTesting();
			manager.ExceptionToThrowBeforeSwitchingBlobType = new OutOfMemoryException();

			var listStmPrintJobs = GetPrintJobsToDeliver();
			AssertExceptionThrown<OutOfMemoryException>(() => { manager.ProcessPrintJobs(listStmPrintJobs); });
			AssertEquals("RetryAttempts has been increased, should equal", (short)1, printJob.SP_RetryAttempts);

			AssertExceptionThrown<OutOfMemoryException>(() => { manager.ProcessPrintJobs(listStmPrintJobs); });
			AssertEquals("RetryAttempts has been increased, should equal", (short)2, printJob.SP_RetryAttempts);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImageFormatException()
		{
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");

			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "TIF";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.CorruptFileFullPath);
			printJob.SP_EmailAttachments = "test.TIF";
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			PrintJobManager printJobManager = new PrintJobManager();
			for (int i = 0; i < 3; i++)
			{
				AssertEquals("Precondition : Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Precondition : Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				printJobManager.ProcessPrintJobs(GetPrintJobsToDeliver());
			}

			AssertEquals("ExceptionReporterTestListener.Instance.Count", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Emails to client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Error Processing Print Job(s)", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertMultilineASCIIEquals("email.Body", @"Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

There is an error with image format.

Error Message is: The image data stream supplied does not contain a valid image format.

--- Print Job 1 of 1 --------------
Job Type: [EML]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [test.TIF]", email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFlexCelXlsAdapterException()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			var testPrintQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var testParentGuid = ZGuid.NewZGuid();

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob.SP_SQ = testPrintQueue.PK;
			printJob.SP_GS_NKJobSubmittedBy = User.GS_Code;
			Factory.Save();

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var mockManager = new MockPrintJobManagerWithException(new FlexCelXlsAdapterException("Oops, unknown file format", XlsErr.ErrFileIsNotSupported));
				for (int i = 0; i < 3; i++)
				{
					AssertEquals("Precondition : Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
					AssertEquals("Precondition : Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
					mockManager.ProcessPrintJobs(GetPrintJobsToDeliver());
				}
			}

			AssertEquals("ExceptionReporterTestListener.Instance.Count", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Recipients", "user@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertEquals("Error Processing Print Job(s)", email.Subject);
			AssertMultilineASCIIEquals("email.Body", @"Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

The file is not on any on the formats supported by FlexCel.

Error Message is: Oops, unknown file format

--- Print Job 1 of 1 --------------
Job Type: [PRN]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: []
Email Subject: []
Email Attachments: [test.XLS]
Parent Table: [JobShipment]", email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenericExceptionIsReportedToTheUserANDNotReportedToCargoWiseWhenAPrintJobFailsSoThatTheyKnowTheJobHasNotMadeIt()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			var testPrintQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var testParentGuid = ZGuid.NewZGuid();

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob.SP_SQ = testPrintQueue.PK;
			printJob.SP_GS_NKJobSubmittedBy = User.GS_Code;
			Factory.Save();

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var bitmapCreationExceptionManager = new MockPrintJobManagerWithException(new IOException("Harry Eats Hats", -2147024784));
				for (int i = 0; i < 3; i++)
				{
					AssertEquals("Precondition : Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
					AssertEquals("Precondition : Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
					bitmapCreationExceptionManager.ProcessPrintJobs(GetPrintJobsToDeliver());
				}
			}

			AssertEquals("ExceptionReporterTestListener.Instance.Count", 0, ExceptionReporterTestListener.Instance.Count);

			AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Recipients", "user@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertEquals("Error Processing Print Job(s)", email.Subject);
			AssertMultilineASCIIEquals("email.Body", @"Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

An IO error occurred. Please check user permissions, security settings, that the folder exists and that there are no network errors/mis-configurations.

Error Message is: Harry Eats Hats

--- Print Job 1 of 1 --------------
Job Type: [PRN]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: []
Email Subject: []
Email Attachments: [test.XLS]
Parent Table: [JobShipment]
", email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEDocsOfflineException()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			var testPrintQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var testParentGuid = ZGuid.NewZGuid();

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob.SP_SQ = testPrintQueue.PK;
			printJob.SP_GS_NKJobSubmittedBy = User.GS_Code;
			Factory.Save();

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var mockManager = new MockPrintJobManagerWithException(new EDocsOffLineException("A Beautiful Day"));
				for (int i = 0; i < 3; i++)
				{
					AssertEquals("Precondition : Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
					AssertEquals("Precondition : Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
					mockManager.ProcessPrintJobs(GetPrintJobsToDeliver());
				}
			}

			AssertEquals("ExceptionReporterTestListener.Instance.Count", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Recipients", "user@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertEquals("Error Processing Print Job(s)", email.Subject);
			AssertMultilineASCIIEquals("email.Body", @"Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

There is an error with the Database Server. Please inform your System Administrator of this problem.

Error Message is: A Beautiful Day

--- Print Job 1 of 1 --------------
Job Type: [PRN]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: []
Email Subject: []
Email Attachments: [test.XLS]
Parent Table: [JobShipment]", email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBitmapCreationExceptionDimensionInsideRangeAreNotReported()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			var testPrintQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var testParentGuid = ZGuid.NewZGuid();

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob.SP_SQ = testPrintQueue.PK;
			printJob.SP_GS_NKJobSubmittedBy = User.GS_Code;
			Factory.Save();

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var bitmapCreationExceptionManager = new MockPrintJobManagerWithException(BitmapCreationException.NewForTesting(new ArgumentException(), 5000, 5000, 300, System.Drawing.Imaging.PixelFormat.Format24bppRgb));
				for (int i = 0; i < 3; i++)
				{
					AssertEquals("Precondition : Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
					AssertEquals("Precondition : Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
					bitmapCreationExceptionManager.ProcessPrintJobs(GetPrintJobsToDeliver());
				}
			}

			AssertEquals("Errors to Cargowise should be 0 when width/height < 9999", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients", "user@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertEquals("Error Processing Print Job(s)", email.Subject);
			AssertMultilineASCIIEquals("email.Body", @$"Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

There was an error rendering an image - Could not allocate resources to render a Bitmap for Document Production.

Error Message is: Bitmap creation failed. 
Your system might be low on memory or system resources, please close all other tasks, exit {Core.Constants.ProductName}, come back in and try again.
Parameters are: 
  Width = 5000
  Height = 5000
  Resolution = 300
  Pixel Format = Format24bppRgb --> Value does not fall within the expected range.

--- Print Job 1 of 1 --------------
Job Type: [PRN]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: []
Email Subject: []
Email Attachments: [test.XLS]
Parent Table: [JobShipment]", email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBitmapCreationExceptionDimensionOutsideRangeAreReported()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			var testPrintQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var testParentGuid = ZGuid.NewZGuid();

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob.SP_SQ = testPrintQueue.PK;
			Factory.Save();

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var bitmapCreationExceptionManager = new MockPrintJobManagerWithException(BitmapCreationException.NewForTesting(new ArgumentException(), 10000, 10000, 300, System.Drawing.Imaging.PixelFormat.Format24bppRgb));
				for (int i = 0; i < 3; i++)
				{
					AssertEquals("Precondition : Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
					AssertEquals("Precondition : Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
					bitmapCreationExceptionManager.ProcessPrintJobs(GetPrintJobsToDeliver());
				}
			}

			AssertEquals("Errors to Cargowise should be 1 when width/height > 9999", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertEquals("Error Processing Print Job(s)", email.Subject);
			AssertMultilineASCIIEquals("email.Body", @$"Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

An exception report has been sent back to CargoWise for further investigation.

Message: [Exception Processing Print Jobs: Bitmap creation failed. 
Your system might be low on memory or system resources, please close all other tasks, exit {Core.Constants.ProductName}, come back in and try again.
Parameters are: 
  Width = 10000
  Height = 10000
  Resolution = 300
  Pixel Format = Format24bppRgb --> Value does not fall within the expected range.]
Type:    [Enterprise.DocumentEngine.Exceptions.BitmapCreationException]
HResult: [-2146233088]

--- Print Job 1 of 1 --------------
Job Type: [PRN]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: []
Email Subject: []
Email Attachments: [test.XLS]
Parent Table: [JobShipment]", email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConcurrencyExceptionOnSaveStopsSaveSoThePrintJobCanRunNextTime()
		{
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "PDF";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			StmPrintJob job1 = factory1.Load<StmPrintJob>(printJob.PK);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			StmPrintJob job2 = factory2.Load<StmPrintJob>(printJob.PK);

			PrintJobManager manager1 = new PrintJobManager();
			AssertNoExceptionThrown(delegate
			{
				manager1.ProcessPrintJobs(new StmPrintJob[] { job1 });
			});

			PrintJobManager manager2 = new PrintJobManager();
			AssertNoExceptionThrown(delegate
			{
				manager2.ProcessPrintJobs(new StmPrintJob[] { job2 });
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHandlesDocumentConverterExceptions()
		{
			PrintJobManagerForTesting manager = new PrintJobManagerForTesting();
			manager.ExceptionToThrowBeforeSwitchingBlobType = new Exception("DocumentEngineException", new Exception("Some inner exception"));
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "PDF";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			ZStringBuilder onProgressResults = new ZStringBuilder();
			manager.OnProgress += (eventType, message) => onProgressResults.AppendIfNotEmpty(message);

			for (int i = 0; i < 3; i++)
			{
				manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			}
			AssertEquals("Exception should have been handled and used our delegate", true, onProgressResults.ToStringWithNewLineBetweenAppends().Contains("DocumentEngineException"));

			ErrorReporter.Clear();
		}

		public void TestLogErrorsWhenSavingAttachmentToFilesystem()
		{
			var manager = new PrintJobManagerForTesting();
			var onProgressResults = new ZStringBuilder();
			manager.OnProgress += (eventType, message) => onProgressResults.AppendIfNotEmpty(message);

			var job = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Subject");
			job.ErrorsWhenConverting.Add(("Test Error Message", new Exception("Exception Message")));

			manager.ProcessPrintJobs(new[] { job });
			AssertContains("Test Error Message", onProgressResults.ToString());
			AssertEquals(0, job.ErrorsWhenConverting.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeleteJobs()
		{
			StmPrintQueue queue = TestAssistant.CreateTestPrintQueue(Factory);

			StmPrintJob job1 = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Subject");
			StmPrintJob job2 = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Subject");

			SetJobAttributes(job1, queue, ZDateTime.UtcNow);
			SetJobAttributes(job2, queue, ZDateTime.UtcNow);

			Factory.Save();
			AssertEquals("2 print jobs exists", 2, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			PrintJobManager manager = new PrintJobManager();
			manager.Delete(new StmPrintJob[] { job1 });

			AssertEquals("Given job was deleted", 1, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			AssertNotNull(Factory.Load(typeof(StmPrintJob), job2.PK));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeleteHandleConcurrencyExceptionOnly()
		{
			StmPrintQueue queue = TestAssistant.CreateTestPrintQueue(Factory);
			StmPrintJob job = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Subject");

			SetJobAttributes(job, queue, ZDateTime.UtcNow);

			Factory.Save();
			AssertEquals("1 print jobs exists", 1, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			MockPrintJobManager manager = new MockPrintJobManager();
			manager.DeleteShouldCallBaseMethod = true;
			manager.SaveShouldThrowAnException = true;

			manager.SaveExceptionToThrow = new Exception();
			AssertExceptionThrown(typeof(Exception), delegate
			{
				manager.Delete(new StmPrintJob[] { job });
			});

			manager.SaveExceptionToThrow = FakeConcurrencyException();
			AssertNoExceptionThrown(delegate
			{
				manager.Delete(new StmPrintJob[] { job });
			});
		}

		ZSaveConcurrencyException FakeConcurrencyException()
		{
			return new ZSaveConcurrencyException(new ZDataConcurrencyException(null, null, null), Factory);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHandleNoConcreteTypeException()
		{
			var queue = TestAssistant.CreateTestPrintQueue(Factory);
			var job = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Subject");
			SetJobAttributes(job, queue, ZDateTime.UtcNow);
			Factory.Save();

			AssertEquals(nameof(PrintJobStatus.QUE), job.SP_Status);

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var mockManager = new MockPrintJobManagerWithException(new NoConcreteTypeException("I am a NoConcreteTypeException"))
				{
					RegisterFactorySaving = true
				};
				mockManager.ProcessPrintJobs(new[] { job });
			}

			AssertEquals("ErrorFactorySavingWhenProcessingPrintJobs", ErrorReporter.LastKeyReported);
			AssertEquals(nameof(PrintJobStatus.FAL), new BusinessObjectFactory().Load<StmPrintJob>(job.PK).SP_Status);
			ErrorReporter.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPurgeJobsOlderThanTwoWeeks()
		{
			StmPrintQueue queue = TestAssistant.CreateTestPrintQueue(Factory);

			StmPrintJob oldJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Old Subject");
			StmPrintJob freshJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "New Subject");

			DateTime twoWeeksAgo = ZDateTime.Now.ToDateTime().Subtract(TimeSpan.FromDays(14));

			SetJobAttributes(oldJob, queue, twoWeeksAgo);
			SetJobAttributes(freshJob, queue, twoWeeksAgo.Add(TimeSpan.FromSeconds(5)));

			Factory.Save();
			AssertEquals("2 print jobs exists", 2, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			MockPrintJobManager manager = new MockPrintJobManager();
			manager.PurgeOld();

			Assert(manager.DeleteWasCalled);
			Assert(manager.JobsPassedToDelete.Length == 1);
			Assert(manager.JobsPassedToDelete[0].PK == oldJob.PK);
		}

		static void SetJobAttributes(StmPrintJob job, StmPrintQueue queue, ZDateTime runDate)
		{
			job.SP_JobType = nameof(PrintJobType.PRN);
			job.SP_RunDateTime = runDate;
			job.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			job.SP_EmailSubjectLine = PrintProcessingConstants.TestGeneratedReport;
			job.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
			job.SP_SQ = queue.PK;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHardCopyForProcessedExcel()
		{
			var printQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Email Subject");
			printJob.SP_JobType = nameof(PrintJobType.PRN);
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob.SP_EmailSubjectLine = PrintProcessingConstants.TestGeneratedReport;
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
			printJob.SP_SQ = printQueue.PK;
			Factory.Save();

			AssertEquals("Print jobs exist", 1, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var manager = new PrintJobManager();
				manager.ProcessPrintJobs(GetPrintJobsToDeliver());
				AssertEquals("Print jobs exist", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmailPDFForProcessedExcel()
		{
			string recipient = "test@example.com";

			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Email Subject");
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "PDF";
			printJob.SP_Destination = recipient;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
			printJob.SP_EmailAttachments = PrintProcessingConstants.TestGeneratedReport;
			Factory.Save();

			PrintJobManager doPrintJob = new PrintJobManager();
			doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertNull("Print job for pk should not exist", Factory.Load(typeof(StmPrintJob), printJob.PK));

			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email recipient ", recipient, email.Recipients[0]);
			AssertEquals("Email attachment ", Path.GetFileNameWithoutExtension(PrintProcessingConstants.TestGeneratedReport) + ".pdf", email.Attachments[0].DisplayName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmailXLSForProcessedExcel()
		{
			string recipient = "test@example.com";

			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Email Subject");
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = recipient;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
			printJob.SP_EmailAttachments = PrintProcessingConstants.TestGeneratedReport;
			Factory.Save();

			PrintJobManager doPrintJob = new PrintJobManager();
			doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertNull("Print job should not exist", Factory.Load(typeof(StmPrintJob), printJob.PK));

			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email recipient ", recipient, email.Recipients[0]);
			AssertEquals("Email attachment ", PrintProcessingConstants.TestGeneratedReport, email.Attachments[0].DisplayName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmailForProcessedExcel()
		{
			string recipient = "test@example.com";

			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Email Subject");
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = recipient;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
			printJob.SP_EmailAttachments = PrintProcessingConstants.TestGeneratedReport;
			Factory.Save();

			PrintJobManager doPrintJob = new PrintJobManager();
			doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertNull("Print job should not exist", Factory.Load(typeof(StmPrintJob), printJob.PK));

			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email recipient ", recipient, email.Recipients[0]);
			AssertEquals("Email attachment ", PrintProcessingConstants.TestGeneratedReport.ToUpper(), email.Attachments[0].DisplayName.ToUpper());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPrepareAttachmentsForDelivery_HTML()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");
			var recipient = "test@example.com";

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "HTML";
			printJob.SP_Destination = recipient;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
			printJob.SP_EmailAttachments = PrintProcessingConstants.TestGeneratedReport;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			var printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = nameof(PrintType.EML);
			printJob2.SP_EmailAttachmentFormat = "HTML";
			printJob2.SP_Destination = recipient;
			printJob2.SP_RunDateTime = ZDateTime.UtcNow;
			printJob2.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob2.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReportWith2Sheets);
			printJob2.SP_EmailAttachments = PrintProcessingConstants.TestGeneratedReport;
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			AssertExcelSheetCount(printJob.SP_CustomProperties, 1);
			AssertExcelSheetCount(printJob2.SP_CustomProperties, 2);

			try
			{
				var doPrintJob = new PrintJobManager();
				var mergedPrintJobs = StmPrintJobGroupCollection.MergePrintJobsByRecipient(new StmPrintJob[] { printJob, printJob2 })[0];
				doPrintJob.PrepareAttachmentsForDelivery(mergedPrintJobs);

				AssertExcelSheetCount(printJob.SP_CustomProperties, 3);
				Assert(printJob.StoredAttachmentFilename.EndsWith("HTML", StringComparison.OrdinalIgnoreCase));
				AssertEquals("No Attachment", "", printJob2.StoredAttachmentFilename);
			}
			finally
			{
				printJob.DeleteStoredAttachment();
			}
		}

		void AssertExcelSheetCount(byte[] data, int sheetCount)
		{
			var file = new XlsFile();
			var memoryStream = new MemoryStream(data);
			file.Open(memoryStream);

			AssertEquals("Sheet count", sheetCount, file.SheetCount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFaxForProcessedExcel()
		{
			Mailer.Fax.FaxDef.SetTypeToCreateForTesting(typeof(DummyFax));
			DummyFax.CreatedDummyFaxDefs.Clear();
			try
			{
				GlbCompany dummyCompany = Factory.New(typeof(GlbCompany)) as GlbCompany;
				GlbBranch dummyBranch = Factory.New(typeof(GlbBranch)) as GlbBranch;
				dummyBranch.GB_GC = dummyCompany.PK;

				Guid parentGuid = Env.CurrentBranchPK;
				string faxnumber = "+61290251198";

				StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Email Subject");
				printJob.SP_JobType = nameof(PrintType.FAX);
				printJob.SP_EmailAttachmentFormat = "XLS";
				printJob.SP_FaxDestination = faxnumber;
				printJob.SP_RunDateTime = ZDateTime.UtcNow;
				printJob.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
				printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
				printJob.SP_ParentTableName = "GlbBranch";
				printJob.SP_ParentGuid = parentGuid;
				printJob.SP_GB = dummyBranch.PK;
				Factory.Save();

				PrintJobManager doPrintJob = new PrintJobManager();
				doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver());
				AssertEquals("FAA", printJob.SP_JobType);
				printJob.Delete();
				AssertEquals("Fax defs created count", 1, DummyFax.CreatedDummyFaxDefs.Count);
				var sentFax = DummyFax.CreatedDummyFaxDefs[0];
				AssertEquals("Fax number", faxnumber, sentFax.FaxNumber);
				AssertEquals("Fax SysId (used by fax gateway to identify Enterprise faxes)", "ediEnterprise", sentFax.SysId);
				AssertEquals("Sending company", dummyCompany.GC_Code, sentFax.SendingCompanyCode);

				ZQuery logFilter = new ZQuery();
				logFilter.AddToFilter(StmALogSchema.SL_Parent, parentGuid);
				logFilter.AddToFilter(StmALogSchema.SL_Table, "GlbBranch");
				logFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DocumentSent.Code);
				StmALog[] logs = (StmALog[])Factory.Load(typeof(StmALog), logFilter);
				AssertEquals("Log count", 1, logs.Length);

				AssertEquals("Fax SysJobId (used for ACK/NACK)", printJob.PK, new ZGuid(sentFax.SysFaxJobId));
				Assert("FaxDef Sent", sentFax.Sent);
			}
			finally
			{
				Mailer.Fax.FaxDef.ResetTypeToCreateForTesting();
				DummyFax.CreatedDummyFaxDefs.Clear();
			}
		}

		void AssertFaxAwaitingAcknowledgmentExists(StmPrintJob printJob)
		{
			AssertEquals("FAA", printJob.SP_JobType);
		}

		class DummyFax : Mailer.Fax.FaxDef
		{
			public static readonly List<DummyFax> CreatedDummyFaxDefs = new List<DummyFax>();
			bool sent;

			public DummyFax()
			{
				CreatedDummyFaxDefs.Add(this);
			}

			public bool Sent
			{
				get { return sent; }
			}

			public override void Send()
			{
				sent = true;
				Assertion.Assert("Attachment file exists.", File.Exists(FaxFileName));
				Assertion.Assert("Attachment file not empty.", new FileInfo(FaxFileName).Length > 0);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleFaxesForProcessedExcel()
		{
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");

			StmPrintJob printJob1 = Factory.New<StmPrintJob>();
			printJob1.SP_JobType = nameof(PrintType.FAX);
			printJob1.SP_FaxDestination = "+61290251198";
			printJob1.SP_RunDateTime = ZDateTime.UtcNow;
			printJob1.SP_EmailSubjectLine = "test";
			printJob1.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob1.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob1.SP_SB_DeliveryGroup = deliveryGroup.PK;

			StmPrintJob printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = nameof(PrintType.FAX);
			printJob2.SP_FaxDestination = "+61290251198";
			printJob2.SP_RunDateTime = ZDateTime.UtcNow;
			printJob2.SP_EmailSubjectLine = "test";
			printJob2.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob2.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;

			StmPrintJob printJob3 = Factory.New<StmPrintJob>();
			printJob3.SP_JobType = nameof(PrintType.FAX);
			printJob3.SP_FaxDestination = "+61290251198";
			printJob3.SP_RunDateTime = ZDateTime.UtcNow;
			printJob3.SP_EmailSubjectLine = "test";
			printJob3.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob3.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob3.SP_SB_DeliveryGroup = deliveryGroup.PK;

			StmPrintJob printJob4 = Factory.New<StmPrintJob>();
			printJob4.SP_JobType = nameof(PrintType.EML);
			printJob4.SP_EmailAttachmentFormat = "PDF";
			printJob4.SP_Destination = "test@example.com";
			printJob4.SP_RunDateTime = ZDateTime.UtcNow;
			printJob4.SP_EmailSubjectLine = "test";
			printJob4.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob4.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob4.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			AssertEquals("Print jobs exist", 4, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			PrintJobManager doPrintJob = new PrintJobManager();
			doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver());
			printJob1.Delete();
			printJob2.Delete();
			printJob3.Delete();
			Factory.Save();
			AssertEquals("Print jobs exist", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			int recipientCount = 0;
			int attachmentCount = 0;
			int emailCount = 0;

			foreach (EmailDef email in Env.OutgoingMailManager.EmailsCreated)
			{
				emailCount++;
				attachmentCount += email.Attachments.Count;
				recipientCount += email.Recipients.Count;
			}

			AssertEquals("Outbound mail exists", 4, emailCount);
			AssertEquals("Outbound mail with recipients", 4, recipientCount);
			AssertEquals("Outbound mail with attachment", /* faxes */ (3 * 2) + /* emails */ 1, attachmentCount);
		}

		public void TestSchemaWorksAsExpected()
		{
			Assert("StmALog.Schema.SL_Reference.Length >= 128", StmALog.Schema.SL_ReferenceMaxLength >= 128);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessSingleDeliveryTypes()
		{
			var printQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");

			var printJob1 = Factory.New<StmPrintJob>();
			printJob1.SP_JobType = nameof(PrintType.PRN);
			printJob1.SP_RunDateTime = ZDateTime.UtcNow;
			printJob1.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob1.SP_EmailSubjectLine = "test";
			printJob1.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob1.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob1.SP_SQ = printQueue.PK;

			var printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = nameof(PrintType.PRN);
			printJob2.SP_RunDateTime = ZDateTime.UtcNow;
			printJob2.SP_EmailSubjectLine = "test";
			printJob2.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob2.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob2.SP_SQ = printQueue.PK;

			var printJob3 = Factory.New<StmPrintJob>();
			printJob3.SP_JobType = nameof(PrintType.PRN);
			printJob3.SP_RunDateTime = ZDateTime.UtcNow;
			printJob3.SP_EmailSubjectLine = "test";
			printJob3.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob3.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob3.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob3.SP_SQ = printQueue.PK;

			var printJob4 = Factory.New<StmPrintJob>();
			printJob4.SP_JobType = nameof(PrintType.FAX);
			printJob4.SP_FaxDestination = "+61290251198";
			printJob4.SP_RunDateTime = ZDateTime.UtcNow;
			printJob4.SP_EmailSubjectLine = "test";
			printJob4.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob4.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob4.SP_SB_DeliveryGroup = deliveryGroup.PK;

			var printJob5 = Factory.New<StmPrintJob>();
			printJob5.SP_JobType = nameof(PrintType.EML);
			printJob5.SP_EmailAttachmentFormat = "PDF";
			printJob5.SP_Destination = "test@example.com";
			printJob5.SP_RunDateTime = ZDateTime.UtcNow;
			printJob5.SP_EmailSubjectLine = "test";
			printJob5.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob5.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob5.SP_SB_DeliveryGroup = deliveryGroup.PK;

			var printJob6 = Factory.New<StmPrintJob>();
			printJob6.SP_JobType = nameof(PrintJobType.DDS);
			printJob6.SP_RunDateTime = ZDateTime.UtcNow;
			printJob6.SP_EmailSubjectLine = "test";
			printJob6.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob6.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob6.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob6.SP_SQ = printQueue.PK;

			Factory.Save();

			AssertEquals("Print jobs exist", 6, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			var doPrintJob = new PrintJobManager();
			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver(PrintJobType.PRN));
				doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver(PrintJobType.PRN)); // try a few times in case of intermittent failures
				AssertEquals("Print jobs exist", 3, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			}

			doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver(PrintJobType.DDS));
			doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver(PrintJobType.DDS)); // try a few times in case of intermittent failures
			AssertEquals("Print jobs exist", 2, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver(PrintJobType.EML));
			doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver(PrintJobType.EML)); // try a few times in case of intermittent failures
			AssertEquals("Print jobs exist", 1, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver(PrintJobType.FAX));
			doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver(PrintJobType.FAX)); // try a few times in case of intermittent failures
			printJob4.Delete();
			Factory.Save();
			AssertEquals("Should be no more print jobs", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestCultureForEmailJobs()
		{
			CultureInfo oldCulture = Enterprise.ZArchitecture.Core.Culture.Current;
			using (Env.CurrentCompany.Country.SetCultureForTest(new CultureInfo("de-DE")))
			{
				try
				{
					StmPrintQueue printQueue = TestAssistant.CreateTestPrintQueue(Factory);
					StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");

					StmPrintJob printJob = Factory.New<StmPrintJob>();
					printJob.SP_JobType = nameof(PrintType.EML);
					printJob.SP_EmailAttachmentFormat = "PDF";
					printJob.SP_Destination = "test@example.com";
					printJob.SP_RunDateTime = ZDateTime.UtcNow;
					printJob.SP_EmailSubjectLine = "test";
					printJob.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
					printJob.SP_CustomProperties = File.ReadAllBytes(ReportPath);
					printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

					Factory.Save();

					PrintJobManager doPrintJob = new PrintJobManager();
					doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver());

					AssertEquals("Should have set process culture back to default after printing", oldCulture, Enterprise.ZArchitecture.Core.Culture.Current);
				}
				finally
				{
					Enterprise.ZArchitecture.Core.Culture.Set(oldCulture);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessSingleDeliveryTypesOverload()
		{
			var printQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");

			var printJob1 = Factory.New<StmPrintJob>();
			printJob1.SP_JobType = nameof(PrintType.PRN);
			printJob1.SP_RunDateTime = ZDateTime.UtcNow;
			printJob1.SP_EmailSubjectLine = "test";
			printJob1.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob1.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob1.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob1.SP_SQ = printQueue.PK;

			var printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = nameof(PrintType.FAX);
			printJob2.SP_FaxDestination = "+61290251198";
			printJob2.SP_RunDateTime = ZDateTime.UtcNow;
			printJob2.SP_EmailSubjectLine = "test";
			printJob2.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob2.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;

			var printJob3 = Factory.New<StmPrintJob>();
			printJob3.SP_JobType = nameof(PrintType.EML);
			printJob3.SP_EmailAttachmentFormat = "PDF";
			printJob3.SP_Destination = "test@example.com";
			printJob3.SP_RunDateTime = ZDateTime.UtcNow;
			printJob3.SP_EmailSubjectLine = "test";
			printJob3.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob3.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob3.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			AssertEquals("Print jobs exist", 3, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var doPrintJob = new PrintJobManager();
				doPrintJob.ProcessPrintJobs(new StmPrintJob[] { printJob1 });
				AssertEquals("Print jobs exist", 2, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLogToStmALogWithoutParentViaEmail()
		{
			const string SubjectLine = "A test subject line";
			const string ExpectedSubjectLine = "example@example.com - " + SubjectLine;
			StmALog log = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, SubjectLine));
			AssertNull("StmALog should not exist", log);

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "OIC";
			staff.GS_LoginName = "kthxbye";
			Factory.Save();

			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, SubjectLine);
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "TestDocument";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailSubjectLine = SubjectLine;
			printJob.SP_ParentTableName = "";
			printJob.SP_ParentGuid = ZGuid.Empty;
			printJob.SP_GS_NKJobSubmittedBy = staff.GS_Code;
			Factory.Save();

			UpdateUserForPrintJobLogs(printJob, staff.GS_Code);

			PrintJobManager manager = new PrintJobManager();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertEquals("Print job should have gone through successfully and been deleted", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			AssertEquals("One email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			log = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, ExpectedSubjectLine));
			AssertNull("No log should be created if print job has no parent", log);
		}

		void UpdateUserForPrintJobLogs(StmPrintJob printJob, ZString userInitials)
		{
			BusinessObject[] logs = Factory.Load(typeof(StmALog), new ZQuery(StmALogSchema.SL_Parent, printJob.PK));
			foreach (StmALog log in logs)
			{
				log.SL_GS_NKUser = userInitials;
			}
			Factory.Save();
		}

		[TestDate(1994, 3, 3)]
		public void TestFaxLogUnderstandsFormattedNumbers()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			StmALog log = dummy.Logs.AddNew(Events.DocumentSent, "+61 (2) 9025-1199 - Blah - Blah - Blah", new ZDateTimeOffset(1994, 3, 3));
			Factory.Save();

			string faxNumberParsedFromQuery = Db.Connection.ExecuteScalar("SELECT FaxNumber FROM FaxLog('1994-03-03', '1994-03-04')").ToString(); // no z equivalent for calling a function
			AssertEquals("Parsed fax number", "+61 (2) 9025-1199", faxNumberParsedFromQuery);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLogToStmALogWithoutParentViaPrint()
		{
			const string SubjectLine = "A test subject line";
			var expectedSubjectLine = TestAssistant.PrintQueueNameForTesting + " - " + SubjectLine;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "OIC";
			staff.GS_LoginName = "kthxbye";
			Factory.Save();
			var log1 = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, SubjectLine));
			AssertNull("StmALog should not exist", log1);

			var printQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, SubjectLine);
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "TestDocument";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailSubjectLine = SubjectLine;
			printJob.SP_ParentTableName = "";
			printJob.SP_ParentGuid = ZGuid.Empty;
			printJob.SP_SQ = printQueue.PK;
			printJob.SP_GS_NKJobSubmittedBy = "OIC";
			Factory.Save();

			UpdateUserForPrintJobLogs(printJob, staff.GS_Code);

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var manager = new PrintJobManager();
				manager.ProcessPrintJobs(GetPrintJobsToDeliver());
				AssertEquals("Print job should have gone through successfully and been deleted", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			}

			var log2 = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, expectedSubjectLine));
			AssertNull("No log should be created if print job has no parent", log2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLogToStmALogWithoutParentViaFax()
		{
			const string SubjectLine = "A test subject line";
			const string ExpectedSubjectLine = "0290251199 - " + SubjectLine;
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "OIC";
			staff.GS_LoginName = "kthxbye";
			Factory.Save();
			StmALog log = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, SubjectLine));
			AssertNull("StmALog should not exist", log);

			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, SubjectLine);
			printJob.SP_JobType = nameof(PrintType.FAX);
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "TestDocument";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_FaxDestination = "0290251199";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailSubjectLine = SubjectLine;
			printJob.SP_ParentTableName = "";
			printJob.SP_ParentGuid = ZGuid.Empty;
			printJob.SP_GS_NKJobSubmittedBy = staff.GS_Code;
			Factory.Save();

			UpdateUserForPrintJobLogs(printJob, staff.GS_Code);

			PrintJobManager manager = new PrintJobManager();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			printJob.Delete();
			Factory.Save();
			AssertEquals("Print job should have gone through successfully and been deleted", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			log = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, ExpectedSubjectLine));
			AssertNull("No log should be created if print job has no parent", log);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLogToStmALogWithParentViaEmail()
		{
			const string SubjectLine = "A test subject line";
			const string ExpectedSubjectLine = "example@example.com - " + SubjectLine;
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "OIC";
			staff.GS_LoginName = "kthxbye";
			Factory.Save();
			StmALog log = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, SubjectLine));
			AssertNull("StmALog should not exist", log);
			Guid testSP_ParentGuid = Env.CurrentBranchPK;

			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Email Subject");
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "TestDocument";
			printJob.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = SubjectLine;
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testSP_ParentGuid;
			printJob.SP_GS_NKJobSubmittedBy = staff.GS_Code;
			Factory.Save();

			UpdateUserForPrintJobLogs(printJob, staff.GS_Code);

			PrintJobManager manager = new PrintJobManager();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertEquals("Print job should have gone through successfully and been deleted", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			log = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, ExpectedSubjectLine));
			AssertNotNull("Log should have been created", log);
			AssertEquals("Log staff user should be saved from the Print Job, not the current user", "OIC", log.SL_GS_NKUser);
			AssertEquals("Reference should be email subject line", ExpectedSubjectLine, log.SL_Reference);
			AssertEquals("Event type should be document sent", "DSN", log.Event.SE_Code);
			AssertEquals("Parent Guid should be empty for print job that didn't have a parent", testSP_ParentGuid, log.SL_Parent);
			AssertEquals("Parent Table should be empty for print job that didn't have a parent", "GlbBranch", log.SL_Table);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLogToStmALogWithParentViaPrint()
		{
			const string SubjectLine = "A test subject line";
			var expectedSubjectLine = TestAssistant.PrintQueueNameForTesting + " - " + SubjectLine;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "OIC";
			staff.GS_LoginName = "kthxbye";

			var log1 = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, SubjectLine));
			AssertNull("StmALog should not exist", log1);

			var testSP_ParentGuid = Env.CurrentBranchPK;
			var printQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Email Subject");
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "TestDocument";
			printJob.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob.SP_EmailSubjectLine = SubjectLine;
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testSP_ParentGuid;
			printJob.SP_GS_NKJobSubmittedBy = staff.GS_Code;
			printJob.SP_SQ = printQueue.PK;
			Factory.Save();

			UpdateUserForPrintJobLogs(printJob, staff.GS_Code);

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var manager = new PrintJobManager();
				for (int i = 0; i < 2; i++)
				{
					manager.ProcessPrintJobs(GetPrintJobsToDeliver());
				}
			}

			AssertEquals("Print job should have gone through successfully and been deleted", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			var log2 = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, expectedSubjectLine));
			AssertNotNull("Log should have been created", log2);
			AssertEquals("Log staff user should be saved from the Print Job, not the current user", "OIC", log2.SL_GS_NKUser);
			AssertEquals("Reference should be email subject line", expectedSubjectLine, log2.SL_Reference);
			AssertEquals("Event type should be document sent", "DSN", log2.Event.SE_Code);
			AssertEquals("Parent Guid should be empty for print job that didn't have a parent", testSP_ParentGuid, log2.SL_Parent);
			AssertEquals("Parent Table should be empty for print job that didn't have a parent", "GlbBranch", log2.SL_Table);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLogToStmALogWithParentViaFax()
		{
			const string SubjectLine = "A test subject line";
			const string ExpectedSubjectLine = "0290251199 - " + SubjectLine;
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "OIC";
			staff.GS_LoginName = "kthxbye";
			Factory.Save();
			StmALog log = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, SubjectLine));
			AssertNull("StmALog should not exist", log);
			Guid testSP_ParentGuid = Env.CurrentBranchPK;

			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Email Subject");
			printJob.SP_JobType = nameof(PrintType.FAX);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_FaxDestination = "0290251199";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "TestDocument";
			printJob.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = SubjectLine;
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testSP_ParentGuid;
			printJob.SP_GS_NKJobSubmittedBy = staff.GS_Code;
			Factory.Save();

			UpdateUserForPrintJobLogs(printJob, staff.GS_Code);

			PrintJobManager manager = new PrintJobManager();
			ZQuery pKFilter = new ZQuery(StmPrintJobSchema.PK, printJob.PK);
			manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			log = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, ExpectedSubjectLine));
			printJob.Delete();
			Factory.Save();
			AssertEquals("Print job should have gone through successfully and been deleted", 0, Factory.GetDatabaseCount(typeof(StmPrintJob), pKFilter));

			AssertNotNull("Log should have been created", log);
			AssertEquals("Log staff user should be saved from the Print Job, not the current user", "OIC", log.SL_GS_NKUser);
			AssertEquals("Reference should be email subject line", ExpectedSubjectLine, log.SL_Reference);
			AssertEquals("Event type should be document sent", "DSN", log.Event.SE_Code);
			AssertEquals("Parent Guid should be empty for print job that didn't have a parent", testSP_ParentGuid, log.SL_Parent);
			AssertEquals("Parent Table should be empty for print job that didn't have a parent", "GlbBranch", log.SL_Table);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPrintExceptionOutcomeForCargowiseFailure()
		{
			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Email Subject");
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "TIF";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.CorruptFileFullPath);

			GlbGroup postMaster = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff staff = postMaster.Staff.AddNew();
			staff.GS_EmailAddress = "admin@example.com";
			staff.GS_Code = "ZAC";

			Factory.Save();

			PrintJobManager manager = new PrintJobManager();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals("Zero retries", (short)0, printJob.SP_RetryAttempts);
			for (int i = 0; i < 3; i++)
			{
				manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			}

			string lastMessageReported = ErrorReporter.LastMessageReported;
			Exception lastExceptionReported = ErrorReporter.LastExceptionReported;
			ErrorReporter.Clear();

			AssertMultilineASCIIEquals("ErrorReporter.LastMessageReported", @"Exception Processing Print Jobs: " + ExcelInterfaceExceptionBase.FileCorruptedMessage + @" --> Error reading Excel records. File invalid

--- Print Job 1 of 1 --------------
Job Type: [EML]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]
Parent Table: []
Parent PK: [00000000-0000-0000-0000-000000000000]", lastMessageReported);

			AssertNotNull("lastExceptionReported", lastExceptionReported);
			AssertEquals("lastExceptionReported.Message", ExcelInterfaceExceptionBase.FileCorruptedMessage, lastExceptionReported.Message);
			AssertEquals("lastExceptionReported.Message", typeof(ExcelInterfaceException), lastExceptionReported.GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFailedDocumentIsRetriedThreeTimes()
		{
			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory, "Email Subject");
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "TIF";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.CorruptFileFullPath);
			Factory.Save();

			PrintJobManager manager = new PrintJobManager();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals("Zero retries", (short)0, printJob.SP_RetryAttempts);
			for (int i = 0; i < 3; i++)
			{
				manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			}
			AssertEquals("Should not have sent the job - the file is corrupt", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals("Should have notified user of exception", UnitTestUserNotification.Instance.LastMessage.ToString(), "Error " + ExcelInterfaceExceptionBase.FileCorruptedMessage);
			ExceptionReporterTestListener.Instance.Clear();
			printJob.Reload();
			AssertEquals("Retry attempts", (short)3, printJob.SP_RetryAttempts);
			manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertEquals("Should not have sent the job - the file is corrupt", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			string lastMessageReported = ErrorReporter.LastMessageReported;
			ErrorReporter.Clear();
			AssertMultilineASCIIEquals("ErrorReporter.LastMessageReported", @"Exception Processing Print Jobs: " + ExcelInterfaceExceptionBase.FileCorruptedMessage + @" --> Error reading Excel records. File invalid

--- Print Job 1 of 1 --------------
Job Type: [EML]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]
Parent Table: []
Parent PK: [00000000-0000-0000-0000-000000000000]", lastMessageReported);

			printJob.Reload();
			AssertEquals("Retry attempts - should have given up and stopped processing", (short)4, printJob.SP_RetryAttempts);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFailedDocumentWillStopOtherDocumentsInDeliveryGroupFromBeingProcessed()
		{
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");

			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "TIF";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.TIF";
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			StmPrintJob printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = nameof(PrintType.EML);
			printJob2.SP_EmailAttachmentFormat = "TIF";
			printJob2.SP_Destination = "test@example.com";
			printJob2.SP_RunDateTime = ZDateTime.UtcNow;
			printJob2.SP_DocumentName = "Test";
			printJob2.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.CorruptFileFullPath);
			printJob2.SP_EmailAttachments = "test.TIF";
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;

			StmPrintJob printJob3 = Factory.New<StmPrintJob>();
			printJob3.SP_JobType = nameof(PrintType.EML);
			printJob3.SP_EmailAttachmentFormat = "TIF";
			printJob3.SP_Destination = "test@example.com";
			printJob3.SP_RunDateTime = ZDateTime.UtcNow;
			printJob3.SP_DocumentName = "Test";
			printJob3.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob3.SP_EmailAttachments = "test.TIF";
			printJob3.SP_SB_DeliveryGroup = deliveryGroup.PK;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals("Should have added 3 jobs", 3, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			new PrintJobManager().ProcessPrintJobs(GetPrintJobsToDeliver());

			AssertEquals("Should NOT have emailed the two jobs that didn't have errors", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("All three jobs should still be in the DB, can't be deleted yet because they haven't been delivered", 3, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			// Retry to force a failure report
			new PrintJobManager().ProcessPrintJobs(GetPrintJobsToDeliver());
			new PrintJobManager().ProcessPrintJobs(GetPrintJobsToDeliver());

			AssertEquals("Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Emails to client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Error Processing Print Job(s)", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertMultilineASCIIEquals("email.Body", @"Error processing print job - giving up after 3 attempts (affecting a total of 3 print job(s))

There is an error with image format.

Error Message is: The image data stream supplied does not contain a valid image format.

--- Print Job 1 of 3 --------------
Job Type: [EML]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [test.TIF]

--- Print Job 2 of 3 --------------
Job Type: [EML]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [test.TIF]

--- Print Job 3 of 3 --------------
Job Type: [EML]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [test.TIF]", email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestEmailTIFfile()
		{
			ZString tifFileName = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.Test8bppTifFileName;
			ZGuid testParentGuid = Env.CurrentBranchPK;
			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory);
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "TIF";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(tifFileName);
			printJob.SP_EmailAttachments = "test.TIF";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_DocumentType = "MSC";
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			Factory.Save();

			new PrintJobManager().ProcessPrintJobs(GetPrintJobsToDeliver());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestFaxTIFfile()
		{
			ZString tifFileName = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.Test8bppTifFileName;
			ZGuid testParentGuid = Env.CurrentBranchPK;
			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory);
			printJob.SP_JobType = "FAX";
			printJob.SP_EmailAttachmentFormat = "TIF";
			printJob.SP_FaxDestination = "0290251199";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(tifFileName);
			printJob.SP_EmailAttachments = "test.TIF";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_DocumentType = "MSC";
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			Factory.Save();

			new PrintJobManager().ProcessPrintJobs(GetPrintJobsToDeliver());
			printJob.Delete();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestPrintTIFfile()
		{
			var printQueue = TestAssistant.CreateTestPrintQueue(Factory);

			var tifFileName = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.Test8bppTifFileName;
			var testParentGuid = Env.CurrentBranchPK;
			var printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory);
			printJob.SP_JobType = "PRN";
			printJob.SP_EmailAttachmentFormat = "TIF";
			printJob.SP_FaxDestination = "";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(tifFileName);
			printJob.SP_EmailAttachments = "test.TIF";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_DocumentType = "MSC";
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = printQueue.PK;
			Factory.Save();

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				new PrintJobManager().ProcessPrintJobs(GetPrintJobsToDeliver());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDuplicateFilenamesInOneMergedGroup()
		{
			PrintJobManager manager = new PrintJobManager();
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			StmPrintJob printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = "EML";
			printJob2.SP_EmailAttachmentFormat = "XLS";
			printJob2.SP_Destination = "example@example.com";
			printJob2.SP_DocumentName = "Test";
			printJob2.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob2.SP_EmailAttachments = "test.XLS";
			printJob2.SP_EmailSubjectLine = "Subject";
			printJob2.SP_RunDateTime = ZDateTime.UtcNow;
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;
			Factory.Save();

			// mimic behaviour of ProcessPrintJobs() which would call this method once for each print job
			printJob.SaveAttachmentToFilesystem(0);
			printJob2.SaveAttachmentToFilesystem(0);

			Assert("Two print jobs in the same delivery group with exactly the same filename info should produce two different StoredAttachmentFilenames", printJob.StoredAttachmentFilename.ToUpper() != printJob2.StoredAttachmentFilename.ToUpper());

			File.Delete(printJob.StoredAttachmentFilename);
			File.Delete(printJob2.StoredAttachmentFilename);
		}

		public void TestHandlePrintException()
		{
			PrintJobManager manager = new MockPrintJobManagerWithException(new Exception());

			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			StmPrintJob failedPrintJob = Factory.New<StmPrintJob>();
			failedPrintJob.SP_JobType = "EML";
			failedPrintJob.SP_Destination = "test@example.com";
			failedPrintJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			StmPrintJob oKPrintJob1 = Factory.New<StmPrintJob>();
			oKPrintJob1.SP_JobType = "EML";
			oKPrintJob1.SP_Destination = "test@example.com";
			oKPrintJob1.SP_SB_DeliveryGroup = deliveryGroup.PK;

			StmPrintJob oKPrintJob2 = Factory.New<StmPrintJob>();
			oKPrintJob2.SP_JobType = "EML";
			oKPrintJob2.SP_Destination = "test@example.com";
			oKPrintJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			manager.ProcessPrintJobs(new StmPrintJob[] { oKPrintJob1, failedPrintJob, oKPrintJob2 });

			AssertEquals("Print Job should have RetryAttempts incremented", 1, (int)failedPrintJob.SP_RetryAttempts);
			AssertEquals("Print Job should have RetryAttempts incremented", 1, (int)oKPrintJob1.SP_RetryAttempts);
			AssertEquals("Print Job should have RetryAttempts incremented", 1, (int)oKPrintJob2.SP_RetryAttempts);

			ErrorReporter.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocManagerLoggingHappensSeparatelyToDeliveryPRN()
		{
			var queue = TestAssistant.CreateTestPrintQueue(Factory);
			var testParentGuid = Env.CurrentBranchPK;
			var printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory);
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_SQ = queue.PK;
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_EDocsProcessed = true;

			Factory.Save();

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var printJobManager = new PrintJobManager();
				int printCount = 0;
				printJobManager.OnProgress += (eventType, message) => { if (message.StartsWith("Processing ")) { ++printCount; } };
				for (int i = 0; i < 2; i++)
				{
					printJobManager.ProcessPrintJobs(GetPrintJobsToDeliver());
				}
				AssertEquals("Should have processed one job for PRN", 1, printCount);
				AssertEquals("No print jobs should exist", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
				AssertEquals("print job should be deleted", true, printJob.IsDeleted);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocManagerLoggingHappensSeparatelyToDeliveryEML()
		{
			ZGuid testParentGuid = Env.CurrentBranchPK;
			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory);
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_EDocsProcessed = true; // say the eDocs has been processed previously in another task

			Factory.Save();
			PrintJobManager printJobManager = new PrintJobManager();
			int printCount = 0;
			printJobManager.OnProgress += (eventType, message) => { if (message.StartsWith("Processing ") && !message.Contains("Processing document")) { ++printCount; } };
			for (int i = 0; i < 2; i++)
			{
				printJobManager.ProcessPrintJobs(GetPrintJobsToDeliver());
			}
			AssertEquals("Should have processed one job for email", 1, printCount);
			AssertEquals("print job should be deleted", true, printJob.IsDeleted);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocManagerLoggingHappensSeparatelyToDeliveryFAX()
		{
			ZGuid testParentGuid = Env.CurrentBranchPK;
			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory);
			printJob.SP_JobType = nameof(PrintType.FAX);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_FaxDestination = "0290251199";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;

			Factory.Save();
			PrintJobManager printJobManager = new PrintJobManager();
			int printCount = 0;
			printJobManager.OnProgress += (eventType, message) => { if (message.StartsWith("Processing ") && !message.Contains("Processing document")) { ++printCount; } };
			printJobManager.ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertEquals("Should have processed one FAX", 1, printCount);
			AssertEquals("print job should be FAA", "FAA", printJob.SP_JobType);
			printJob.Delete();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocManagerLoggingForJobsWithoutValidRelatedBusinessContext()
		{
			ZGuid testParentGuid = Env.CurrentBranchPK;
			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory);
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = ZString.Empty;// empty business context
			printJob.SP_RunDateTime = ZDateTime.UtcNow;

			Factory.Save();
			new PrintJobManager().ProcessPrintJobs(GetPrintJobsToDeliver());

			AssertEquals("Should be no print jobs left - job not resubmitted as doc manager job", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocManagerLoggingForJobsWithValidRelatedBusinessContext()
		{
			ZGuid testParentGuid = Env.CurrentBranchPK;
			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory);
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "SHP"; // valid business context
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_EDocsProcessed = true;

			Factory.Save();
			PrintJobManager printJobManager = new PrintJobManager();
			int printCount = 0;
			printJobManager.OnProgress += (eventType, message) => { if (message.StartsWith("Processing ") && !message.Contains("Processing document")) { ++printCount; } };
			for (int i = 0; i < 2; i++)
			{
				printJobManager.ProcessPrintJobs(GetPrintJobsToDeliver());
			}
			AssertEquals("Should have processed one jobs - one EML", 1, printCount);
			AssertEquals("Job should be deleted", true, printJob.IsDeleted);
			AssertEquals("Should be no print jobs left now", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocManagerLoggingForJobsWithInvalidRelatedBusinessContext()
		{
			ZGuid testParentGuid = Env.CurrentBranchPK;
			StmPrintJob printJob = TestAssistant.CreateNewPrintJobWithDeliveryGroup(Factory);
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "ABC"; // not valid business context
			printJob.SP_RunDateTime = ZDateTime.UtcNow;

			Factory.Save();
			new PrintJobManager().ProcessPrintJobs(GetPrintJobsToDeliver());
			AssertEquals("Job is deleted", true, printJob.IsDeleted);
			AssertEquals("Should be no print jobs left", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocManagerLoggingForMultipleJobs()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var testParentGuid = Env.CurrentBranchPK;
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = ZString.Empty; // empty business context
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			var printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = nameof(PrintType.EML);
			printJob2.SP_EmailAttachmentFormat = "XLS";
			printJob2.SP_Destination = "example@example.com";
			printJob2.SP_DocumentName = "Test";
			printJob2.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob2.SP_EmailAttachments = "test.XLS";
			printJob2.SP_EmailSubjectLine = "Subject";
			printJob2.SP_ParentTableName = "GlbBranch";
			printJob2.SP_ParentGuid = testParentGuid;
			printJob2.SP_RelatedBusinessContext = "SHP"; // a valid business context
			printJob2.SP_RunDateTime = ZDateTime.UtcNow;
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob2.SP_EDocsProcessed = true;

			Factory.Save();
			var printJobManager = new PrintJobManager();
			var printCount = 0;
			printJobManager.OnProgress += (eventType, message) => { if (message.StartsWith("Processing ") && !message.Contains("Processing document")) { ++printCount; } };
			printJobManager.ProcessPrintJobs(GetPrintJobsToDeliver());
			CombineAssertions(() =>
			{
				AssertEquals("Should have attempted to run 1 jobs - merged EML(2) and DDS", 1, printCount);
				AssertEquals("Print job is now deleted", true, printJob.IsDeleted);
				AssertEquals("Print job is now deleted", true, printJob2.IsDeleted);
				AssertEquals("Should be no more print jobs", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliveryGroupNotDeletedIfDeliveryFails()
		{
			ZGuid testParentGuid = ZGuid.NewZGuid();
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.DDS);
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.CorruptFileFullPath);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "SHP"; // empty business context
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			for (int i = 0; i < 3; i++)
			{
				new EDocPrintJobManager().ProcessPrintJobs(GetPrintJobsToDeliver());
			}
			AssertEquals("Should have notified user of exception", UnitTestUserNotification.Instance.LastMessage.ToString(), "Error " + ExcelInterfaceExceptionBase.FileCorruptedMessage);
			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Print job is not deleted - failed delivery", false, printJob.IsDeleted);
			AssertEquals("Delivery group should still be in db - failed delivery", 1, Factory.GetDatabaseCount(typeof(StmDeliveryGroup)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDatabaseConnectionClosedExceptionsAreRethrownAndNotReported()
		{
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			ZGuid testParentGuid = ZGuid.NewZGuid();

			StmPrintJob printJobEML = Factory.New<StmPrintJob>();
			printJobEML.SP_JobType = nameof(PrintType.EML);
			printJobEML.SP_EmailAttachmentFormat = "XLS";
			printJobEML.SP_Destination = "example@example.com";
			printJobEML.SP_DocumentName = "Test";
			printJobEML.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJobEML.SP_EmailAttachments = "test.XLS";
			printJobEML.SP_EmailSubjectLine = "Subject";
			printJobEML.SP_ParentTableName = "JobShipment";
			printJobEML.SP_ParentGuid = testParentGuid;
			printJobEML.SP_RelatedBusinessContext = "SHP";
			printJobEML.SP_RunDateTime = ZDateTime.UtcNow;
			printJobEML.SP_RetryAttempts = 2;
			printJobEML.SP_SB_DeliveryGroup = deliveryGroup.PK;
			Factory.Save();

			var manager = new MockPrintJobManagerWithException(new DatabaseConnectionClosedException());
			bool exceptionCaught = false;
			try
			{
				ExceptionReporterTestListener.Instance.Clear();
				manager.ProcessPrintJobs(GetPrintJobsToDeliver());
			}
			catch (DatabaseConnectionClosedException)
			{
				exceptionCaught = true;
			}
			Assert("DatabaseConnectionClosedException should have been thrown further up the callstack", exceptionCaught);
			AssertEquals("No silent notification should have been sent", 0, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Still one print job in the database", 1, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			printJobEML.SP_RetryAttempts = 0;
			Factory.Save();

			var managerWithIOException = new MockPrintJobManagerWithException(new Exception());
			ExceptionReporterTestListener.Instance.Clear();
			for (int i = 0; i < 3; i++)
			{
				managerWithIOException.ProcessPrintJobs(GetPrintJobsToDeliver());
			}
			AssertEquals("A silent notification should have been sent", 1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Still one print job in the database", 1, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			ExceptionReporterTestListener.Instance.Clear();
		}

		#region TestLoginToJobBranch

		public void TestLoginToJobBranch()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			StmPrintJob job = Factory.NewWithValidTestData<StmPrintJob>();
			job.SP_GB = branch.PK;

			AssertNotEquals("We should be in original branch yet.", branch.PK, GlbBranch.CurrentBranch.PK);

			PrintJobManagerForLoginTest jobManager = new PrintJobManagerForLoginTest();
			jobManager.ProcessingJobs += (sender, args) => AssertEquals("We should be in temporary branch now.", branch.PK, GlbBranch.CurrentBranch.PK);
			jobManager.ProcessPrintJobs(new[] { job });

			AssertNotEquals("We should be in original branch again.", branch.PK, GlbBranch.CurrentBranch.PK);
		}

		class PrintJobManagerForLoginTest : PrintJobManager
		{
			internal override void ProcessPrintJobsCore(IEnumerable<StmPrintJob> jobs)
			{
				if (ProcessingJobs != null)
				{
					ProcessingJobs(this, EventArgs.Empty);
				}
			}

			public event EventHandler ProcessingJobs;
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestZCannotSaveException()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			var testPrintQueue = TestAssistant.CreateTestPrintQueue(Factory);

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_ParentTableName = "GlbBranch";
			printJob.SP_ParentGuid = Env.CurrentBranchPK;
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob.SP_SQ = testPrintQueue.PK;
			printJob.SP_GS_NKJobSubmittedBy = User.GS_Code;

			var shipment = (IDocsAndCartageParent)Factory.New<Integration.Forwarding.IForwardingShipment>();
			var jobRequiredDocumentEPR1 = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentEPR1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentEPR1.EQ_DocType = Core.Constants.RefDocTypes.EntryPrint;
			jobRequiredDocumentEPR1.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;

			Factory.Save();

			var jobRequiredDocumentEPR2 = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentEPR2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentEPR2.EQ_DocType = Core.Constants.RefDocTypes.EntryPrint;
			jobRequiredDocumentEPR2.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;

			using (ObjectFactory.Substitute<IPrinterFactory>(new MockPrinterFactory()))
			{
				var manager = new PrintJobManager();
				AssertEquals("Precondition : Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Precondition : Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				manager.ProcessPrintJobs(new[] { printJob });
			}

			AssertEquals("ExceptionReporterTestListener.Instance.Count", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Shouldn't send emails to client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCanRunInAnyBranch()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "PDF";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_EmailSubjectLine = "test";
			printJob.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			printJob.SP_CustomProperties = File.ReadAllBytes(ReportPath);
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			var doPrintJob = new PrintJobManager();

			using (EnvProxy.Instance.TemporaryServiceTaskContext("FDD", true))
			{
				doPrintJob.ProcessPrintJobs(GetPrintJobsToDeliver());
			}
			AssertEquals("No Errors", false, ErrorReporter.LastMessageReported.Contains("Service Task: FDD accesses environment current branch without setting the environment first.\r\nDirect access to Env.CurrentBranch is not allowed from service tasks, please use Enterprise.Environment.DisposableEnvironment to set your service task's running environment before the task execution.\r\n"));

			ErrorReporter.Clear();
		}

		#region Test Objects

		#region MockPrintJobManagerWithException

		class MockPrintJobManagerWithException : PrintJobManager
		{
			public MockPrintJobManagerWithException(Exception ex)
			{
				this.ex = ex;
			}

			protected override void DeliverPrintJobs(StmPrintJobMergedCollection mergedPrintGroup)
			{
				if (RegisterFactorySaving)
				{
					mergedPrintGroup.Factory.Saving += factory => throw ex;
				}
				throw ex;
			}

			readonly Exception ex;
			public bool RegisterFactorySaving;
		}

		#endregion

		#region MockPrintJobManager

		class MockPrintJobManager : PrintJobManager
		{
			public bool DeleteWasCalled;
			public StmPrintJob[] JobsPassedToDelete;
			public bool DeleteShouldCallBaseMethod;

			public bool SaveShouldThrowAnException;
			public Exception SaveExceptionToThrow;

			public override void Delete(StmPrintJob[] jobs)
			{
				DeleteWasCalled = true;
				JobsPassedToDelete = jobs;

				if (DeleteShouldCallBaseMethod)
				{
					base.Delete(jobs);
				}
			}

			internal override void Save(BusinessObjectFactory factory)
			{
				if (SaveShouldThrowAnException)
				{
					throw SaveExceptionToThrow;
				}
			}
		}

		#endregion

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		StmPrintJob[] GetPrintJobsToDeliver(params PrintJobType[] jobTypes)
		{
			ZQuery query = new ZQuery();
			foreach (PrintJobType jobType in jobTypes)
			{
				query.AddToFilter(JoinCondition.Or, StmPrintJobSchema.SP_JobType, jobType.ToString());
			}

			return new BusinessObjectFactory().Load<StmPrintJob>(query);
		}

		protected override void SetUp()
		{
			DummySmsSender.RegisterThisSubTypeOverride();

			var config = new ServerUsernamePasswordConfiguration();
			config.UserName = "Geoff";
			config.Password = "pass";
			config.ConfirmPassword = "pass";
			PhysicalServerDataRegistry.Instance.SmsConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintQueueSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmDeliveryGroupSchema.Constants.TableName);

			InstalledPrintersListForTesting = SafeInstalledPrinters.OverridePrintersForTesting(TestAssistant.PrintQueueNameForTesting);

			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var postMaster = postMasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = "postmaster@sample.org";
			postMaster.GS_Code = "_O_";
			postMaster.GS_LoginName = "postmastersample";

			User = Factory.NewWithValidTestData<GlbStaff>();
			User.GS_EmailAddress = "user@sample.org";
			User.GS_Code = "W_W";
			User.GS_LoginName = "usersample";

			Factory.Save();
		}

		GlbStaff User;
		protected override void TearDown()
		{
			DummySmsSender.UnregisterThisSubTypeOverride();
			DummySmsSender.ResetStaticTestData();

			InstalledPrintersListForTesting.Dispose();

			base.TearDown();
		}

		readonly string ReportPath = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport;
		IDisposable InstalledPrintersListForTesting;

		#endregion
	}
}
