using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.PrintProcessing.Testing
{
	sealed class PrintJobManagerExceptionHandlerTest : TestCaseWithFactory
	{
		public void TestExternalStorageExceptionIsHandled()
		{
			TestExceptionIsHandled(new ExternalStorageException("There was an error while accessing the document.", "TESTTYPE", new Exception()), "There was an error while accessing the document.");
		}

		public void TestInvalidDataExceptionIsHandled()
		{
			TestExceptionIsHandled(new InvalidDataException("Found invalid data while decoding."), "Found invalid data while decoding");
		}

		public void TestGdiPlusExceptionIsHandled()
		{
			TestExceptionIsHandled(new TypeInitializationException("Gdip", new ExternalException("A generic error occurred in GDI+.")),
				"The type initializer for 'Gdip' threw an exception. --> A generic error occurred in GDI+.");
		}

		void TestExceptionIsHandled(Exception exception, string message)
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var job = Factory.New<StmPrintJob>();
			job.SP_JobType = "EML";
			job.SP_Destination = "test@example.com";
			job.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			var mergedPrintJobs = new StmPrintJobMergedCollection(Factory);
			mergedPrintJobs.Add(job);

			var logger = new LogHolder();
			var printJobManagerExceptionHandler = new PrintJobManagerExceptionHandler(logger.LogAction);
			for (int i = 0; i < 3; i++)
			{
				printJobManagerExceptionHandler.HandlePrintException(mergedPrintJobs, exception);
			}
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains(message, logger.ToString());
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestHandleSqlException21()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var job = Factory.New<StmPrintJob>();
			job.SP_JobType = "EML";
			job.SP_Destination = "test@example.com";
			job.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			var mergedPrintJobs = new StmPrintJobMergedCollection(Factory);
			mergedPrintJobs.Add(job);

			var logger = new LogHolder();
			var printJobManagerExceptionHandler = new PrintJobManagerExceptionHandler(logger.LogAction);
			for (int i = 0; i < 3; i++)
			{
				var sqlException = SqlExceptionBuilder.CreateSqlException(21, "Warning: Fatal error 605 occurred at Sep 10 2020 12:25AM. Note the error and time, and contact your system administrator.");
				printJobManagerExceptionHandler.HandlePrintException(mergedPrintJobs, sqlException);
			}

			AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var expectedLogsMessage = @"Warning - Error processing print job on 1 attempt - 1 affected print jobs will be reprocessed later again

There are some errors on your server. Please contact your system administrator to check your server windows event logs.

Error Message is: Warning: Fatal error 605 occurred at Sep 10 2020 12:25AM. Note the error and time, and contact your system administrator.

--- Print Job 1 of 1 --------------
Job Type: [EML]
Document Name: []
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]
Warning - Error processing print job on 2 attempt - 1 affected print jobs will be reprocessed later again

There are some errors on your server. Please contact your system administrator to check your server windows event logs.

Error Message is: Warning: Fatal error 605 occurred at Sep 10 2020 12:25AM. Note the error and time, and contact your system administrator.

--- Print Job 1 of 1 --------------
Job Type: [EML]
Document Name: []
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]
Warning - Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

There are some errors on your server. Please contact your system administrator to check your server windows event logs.

Error Message is: Warning: Fatal error 605 occurred at Sep 10 2020 12:25AM. Note the error and time, and contact your system administrator.

--- Print Job 1 of 1 --------------
Job Type: [EML]
Document Name: []
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]
";
			AssertMultilineASCIIEquals("logger.ToString()", expectedLogsMessage, logger.ToString());
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFontNotSupportedException()
		{
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			StmPrintQueue testPrintQueue = TestAssistant.CreateTestPrintQueue(Factory);
			ZGuid testParentGuid = ZGuid.NewZGuid();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "^O^";
			staff.GS_EmailAddress = "user@sample.org";

			StmPrintJob printJob = Factory.New<StmPrintJob>();
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
			printJob.SP_GS_NKJobSubmittedBy = staff.GS_Code;
			Factory.Save();

			StmPrintJobMergedCollection mergedPrintJobs = new StmPrintJobMergedCollection(Factory);
			mergedPrintJobs.Add(printJob);

			LogHolder logger = new LogHolder();
			PrintJobManagerExceptionHandler printJobManagerExceptionHandler = new PrintJobManagerExceptionHandler(logger.LogAction);
			for (int i = 0; i < 3; i++)
			{
				AssertEquals("Precondition : Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Precondition : Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				printJobManagerExceptionHandler.HandlePrintException(mergedPrintJobs, ExcelInterfaceException.NewForTesting(ExcelInterfaceExceptionType.ErrorFontNotSupported, "Font crazy (size=10) does not support style regular"));
			}

			AssertEquals("ExceptionReporterTestListener.Instance.Count", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Recipients", "user@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertEquals("Error Processing Print Job(s)", email.Subject);
			AssertMultilineASCIIEquals("email.Body", @"Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

A font was used in a template that is not installed or corrupted on the machine running your Printing Service Tasks.

Please either install this font on the machine running your Printing Service Tasks or remove it from the template.

Error Message is: " + ExcelInterfaceExceptionBase.ErrorFontNotSupportedMessage + @"
Details: [Font crazy (size=10) does not support style regular]

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
		public void TestFontNotSupportedFlexCelCorException()
		{
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			StmPrintQueue testPrintQueue = TestAssistant.CreateTestPrintQueue(Factory);
			ZGuid testParentGuid = ZGuid.NewZGuid();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "^O^";
			staff.GS_EmailAddress = "user@sample.org";

			StmPrintJob printJob = Factory.New<StmPrintJob>();
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
			printJob.SP_GS_NKJobSubmittedBy = staff.GS_Code;
			Factory.Save();

			StmPrintJobMergedCollection mergedPrintJobs = new StmPrintJobMergedCollection(Factory);
			mergedPrintJobs.Add(printJob);

			LogHolder logger = new LogHolder();
			PrintJobManagerExceptionHandler printJobManagerExceptionHandler = new PrintJobManagerExceptionHandler(logger.LogAction);
			for (int i = 0; i < 3; i++)
			{
				AssertEquals("Precondition : Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Precondition : Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				printJobManagerExceptionHandler.HandlePrintException(mergedPrintJobs, new FlexCelCoreException("Font crazy (size=10) does not support style regular", FlxErr.ErrFontNotSupported));
			}

			AssertEquals("ExceptionReporterTestListener.Instance.Count", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Recipients", "user@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertEquals("Error Processing Print Job(s)", email.Subject);
			AssertMultilineASCIIEquals("email.Body", @"Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

A font was used in a template that is not installed or corrupted on the machine running your Printing Service Tasks.

Please either install this font on the machine running your Printing Service Tasks or remove it from the template.

Error Message is: Font crazy (size=10) does not support style regular" + @"

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
		public void TestEmailAddressInvalidException()
		{
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "^O^";
			staff.GS_EmailAddress = "ben@edi.com.au";

			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.EML);
			printJob.SP_EmailAttachmentFormat = "TIF";
			printJob.SP_Destination = "test@abc.com";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Test32bppTifFullPath);
			printJob.SP_EmailAttachments = "test.TIF";
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			printJob.Staff.GS_EmailAddress = "badEmail#aus.com.au ";
			printJob.SP_GS_NKJobSubmittedBy = staff.GS_Code;

			Factory.Save();

			StmPrintJobMergedCollection mergedPrintJobs = new StmPrintJobMergedCollection(Factory);
			mergedPrintJobs.Add(printJob);

			LogHolder logger = new LogHolder();
			PrintJobManagerExceptionHandler printJobManagerExceptionHandler = new PrintJobManagerExceptionHandler(logger.LogAction);

			for (int i = 0; i < 3; i++)
			{
				AssertEquals("Precondition : Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Precondition : Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				printJobManagerExceptionHandler.HandlePrintException(mergedPrintJobs, new EmailNotCompleteException("Email Address 'sdfkjh@sdfkljh!@lkshj' is invalid."));
			}

			AssertEquals("Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Emails to client", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			const string jobsDesctription =
@"A Print Task was created with an invalid Destination Email Address. Please verify the Email Address is valid.

Error Message is: Email Address 'sdfkjh@sdfkljh!@lkshj' is invalid.

--- Print Job 1 of 1 --------------
Job Type: [EML]
Document Name: [Test]
No. of Copies: [1]
Fax Destination: []
Email To: [test@abc.com]
Email Subject: []
Email Attachments: [test.TIF]";

			string expectedLogsMessage = string.Format(
@"Warning - Error processing print job on 1 attempt - 1 affected print jobs will be reprocessed later again

{0}
Warning - Error processing print job on 2 attempt - 1 affected print jobs will be reprocessed later again

{0}
Warning - Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

{0}
Error - Error Sending Print Job Failure Email:- Invalid email address for sender 'badEmail#aus.com.au'",
				jobsDesctription);

			AssertMultilineASCIIEquals("logger.ToString()", expectedLogsMessage, logger.ToString());
		}

		public void TestEmailLocalUserWhenUsersEmailAddressIsValid()
		{
			SendEmailWhenUsersEmailAddressIsInvalid("test@test.com", "test@test.com");
		}

		public void TestEmailPostmasterWhenUsersEmailAddressIsInvalid()
		{
			SendEmailWhenUsersEmailAddressIsInvalid("Bad ### Email.Address!!!!!", "postmaster@sample.org");
		}

		public void TestEmailPostMasterWhenUsersEmailAddressIsMissing()
		{
			SendEmailWhenUsersEmailAddressIsInvalid("", "postmaster@sample.org");
		}

		void SendEmailWhenUsersEmailAddressIsInvalid(string staffEmailAddress, string recipientEmailAddress)
		{
			LogHolder logger = new LogHolder();
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "^O^";
			staff.GS_EmailAddress = staffEmailAddress;

			StmPrintJob job = Factory.New<StmPrintJob>();
			job.SP_JobType = "EML";
			job.SP_Destination = "test@example.com";
			job.SP_SB_DeliveryGroup = deliveryGroup.PK;
			job.SP_GS_NKJobSubmittedBy = staff.GS_Code;

			Factory.Save();

			StmPrintJobMergedCollection mergedPrintJobs = new StmPrintJobMergedCollection(Factory);
			mergedPrintJobs.Add(job);

			PrintJobManagerExceptionHandler printJobManagerExceptionHandler = new PrintJobManagerExceptionHandler(logger.LogAction);
			for (int i = 0; i < 3; i++)
			{
				printJobManagerExceptionHandler.HandlePrintException(mergedPrintJobs, new System.ComponentModel.Win32Exception("some funny errors"));
			}

			AssertNull(ErrorReporter.LastExceptionReported);

			string expectedUserMessage = @"Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

Driver Communication Error sending job to selected Printer. some funny errors

Error Message is: some funny errors

--- Print Job 1 of 1 --------------
Job Type: [EML]
Document Name: []
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]";

			AssertEquals("Emails to client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Error Processing Print Job(s)", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients", recipientEmailAddress, email.Recipients.RecipientsAsDelimitedString());
			AssertMultilineASCIIEquals("email.Body", expectedUserMessage, email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestHandleKnownPrintException()
		{
			LogHolder logger = new LogHolder();
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			StmPrintJob job1 = Factory.New<StmPrintJob>();
			job1.SP_JobType = "EML";
			job1.SP_Destination = "test@example.com";
			job1.SP_SB_DeliveryGroup = deliveryGroup.PK;

			StmPrintJob job2 = Factory.New<StmPrintJob>();
			job2.SP_JobType = "EML";
			job2.SP_Destination = "test@example.com";
			job2.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			StmPrintJobMergedCollection mergedPrintJobs = new StmPrintJobMergedCollection(Factory);
			mergedPrintJobs.Add(job1);
			mergedPrintJobs.Add(job2);

			PrintJobManagerExceptionHandler printJobManagerExceptionHandler = new PrintJobManagerExceptionHandler(logger.LogAction);
			for (int i = 0; i < 3; i++)
			{
				printJobManagerExceptionHandler.HandlePrintException(mergedPrintJobs, new System.ComponentModel.Win32Exception("some funny errors"));
			}
			AssertNull(ErrorReporter.LastExceptionReported);

			const string jobsDescription =
@"Driver Communication Error sending job to selected Printer. some funny errors

Error Message is: some funny errors

--- Print Job 1 of 2 --------------
Job Type: [EML]
Document Name: []
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]

--- Print Job 2 of 2 --------------
Job Type: [EML]
Document Name: []
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]";

			string expectedEmailMessage = string.Format(
@"Error processing print job - giving up after 3 attempts (affecting a total of 2 print job(s))

{0}",
				jobsDescription);

			AssertEquals("Emails to client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Error Processing Print Job(s)", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertMultilineASCIIEquals("email.Body", expectedEmailMessage, email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			string expectedLogsMessage = string.Format(
@"Warning - Error processing print job on 1 attempt - 2 affected print jobs will be reprocessed later again

{0}
Warning - Error processing print job on 2 attempt - 2 affected print jobs will be reprocessed later again

{0}
Warning - Error processing print job - giving up after 3 attempts (affecting a total of 2 print job(s))

{0}",
				jobsDescription);

			AssertMultilineASCIIEquals("Logs Recorded", expectedLogsMessage, logger.ToString());
		}

		public void TestHandleUnknownPrintException()
		{
			LogHolder logger = new LogHolder();
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			StmPrintJob job1 = Factory.New<StmPrintJob>();
			job1.SP_JobType = "EML";
			job1.SP_Destination = "test@example.com";
			job1.SP_SB_DeliveryGroup = deliveryGroup.PK;

			StmPrintJob job2 = Factory.New<StmPrintJob>();
			job2.SP_JobType = "EML";
			job2.SP_Destination = "test@example.com";
			job2.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			StmPrintJobMergedCollection mergedPrintJobs = new StmPrintJobMergedCollection(Factory);
			mergedPrintJobs.Add(job1);
			mergedPrintJobs.Add(job2);

			PrintJobManagerExceptionHandler printJobManagerExceptionHandler = new PrintJobManagerExceptionHandler(logger.LogAction);
			for (int i = 0; i < 3; i++)
			{
				printJobManagerExceptionHandler.HandlePrintException(mergedPrintJobs, new ArgumentException("this is a test"));
			}

			string expectedExceptionMessage = @"Exception Processing Print Jobs: this is a test

--- Print Job 1 of 2 --------------
Job Type: [EML]
Document Name: []
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]
Parent Table: []
Parent PK: [00000000-0000-0000-0000-000000000000]

--- Print Job 2 of 2 --------------
Job Type: [EML]
Document Name: []
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]
Parent Table: []
Parent PK: [00000000-0000-0000-0000-000000000000]
";

			Exception lastExceptionReported = ErrorReporter.LastExceptionReported;
			string lastMessageReported = ErrorReporter.LastMessageReported;
			AssertEquals("ExceptionReporterTestListener.Instance.Count", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
			AssertType("lastExceptionReported", typeof(ArgumentException), lastExceptionReported);
			AssertMultilineASCIIEquals("lastExceptionReported.Message", "this is a test", lastExceptionReported.Message);
			AssertMultilineASCIIEquals("lastMessageReported", expectedExceptionMessage, lastMessageReported);

			const string jobsDescription =
@"An exception report has been sent back to CargoWise for further investigation.

Message: [Exception Processing Print Jobs: this is a test]
Type:    [System.ArgumentException]
HResult: [-2147024809]

--- Print Job 1 of 2 --------------
Job Type: [EML]
Document Name: []
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]

--- Print Job 2 of 2 --------------
Job Type: [EML]
Document Name: []
No. of Copies: [1]
Fax Destination: []
Email To: [test@example.com]
Email Subject: []
Email Attachments: [default.XLS]";

			string expectedEmailMessage = string.Format(
@"Error processing print job - giving up after 3 attempts (affecting a total of 2 print job(s))

{0}",
				jobsDescription);

			AssertEquals("Emails to client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertMultilineASCIIEquals("email.Body", expectedEmailMessage, email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			string expectedLogsMessage = string.Format(
@"Warning - Error processing print job on 1 attempt - 2 affected print jobs will be reprocessed later again

{0}
Warning - Error processing print job on 2 attempt - 2 affected print jobs will be reprocessed later again

{0}
Error - Error processing print job - giving up after 3 attempts (affecting a total of 2 print job(s))

{0}",
				jobsDescription);

			AssertMultilineASCIIEquals("Logs Recorded", expectedLogsMessage, logger.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestZSaveConcurrencyException()
		{
			StmDeliveryGroup deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			StmPrintQueue testPrintQueue = TestAssistant.CreateTestPrintQueue(Factory);
			ZGuid testParentGuid = ZGuid.NewZGuid();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "^O^";
			staff.GS_EmailAddress = "user@sample.org";

			StmPrintJob printJob = Factory.New<StmPrintJob>();
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
			printJob.SP_GS_NKJobSubmittedBy = staff.GS_Code;
			Factory.Save();

			StmPrintJobMergedCollection mergedPrintJobs = new StmPrintJobMergedCollection(Factory);
			mergedPrintJobs.Add(printJob);

			LogHolder logger = new LogHolder();
			PrintJobManagerExceptionHandler printJobManagerExceptionHandler = new PrintJobManagerExceptionHandler(logger.LogAction);
			for (int i = 0; i < 3; i++)
			{
				AssertEquals("Precondition : Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Precondition : Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				printJobManagerExceptionHandler.HandlePrintException(mergedPrintJobs, new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Another user is saving the document(s) to the same job, please try saving it later."), null, null), Factory));
			}

			AssertEquals("ExceptionReporterTestListener.Instance.Count", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Recipients", "user@sample.org", email.Recipients.RecipientsAsDelimitedString());
			AssertEquals("Error Processing Print Job(s)", email.Subject);
			AssertMultilineASCIIEquals("email.Body", $@"Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s))

While your Printing Service Tasks were running, another user had made changes to the job. Current task will abort due to concurrency errors.

Error Message is: 
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = Another user is saving the document(s) to the same job, please try saving it later.

 --> <ROW IS NULL>
InnerException Message = Another user is saving the document(s) to the same job, please try saving it later. --> Another user is saving the document(s) to the same job, please try saving it later.

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

		#region Implementation

		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintQueueSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmDeliveryGroupSchema.Constants.TableName);

			GlbGroup postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff postMaster = postMasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = "postmaster@sample.org";
			postMaster.GS_Code = "_O_";
			postMaster.GS_LoginName = "postmastersample";

			Factory.Save();
		}

		class LogHolder
		{
			public void LogAction(TraceEventType eventType, string statusMessage)
			{
				actions.Append(eventType.ToString() + " - " + statusMessage);
			}

			readonly ZStringBuilder actions = new ZStringBuilder();

			public override string ToString()
			{
				return actions.ToStringWithNewLineBetweenAppends();
			}
		}

		#endregion
	}
}
