using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.RemotePrinting.Server.JobPrinting;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	/// <summary>
	/// Tests class Enterprise.RemotePrinting.Server.RPSCore.PrintServer (RPSCore project)
	/// </summary>
	sealed class PrintServerTest : TestCaseWithFactory
	{
		#region Update StmPrintQueue should update SQ_SystemCreateUser, SQ_SystemCreateTimeUtc, SQ_SystemLastEditUser, SQ_SystemLastEditTimeUtc

		public void TestUpdateExistingPrintQueuesDeleteStatusShouldSetLastEditUserAndTime()
		{
			AssertShouldSetLastEditUserAndTime((printServerName, queue1PK, queue2PK) =>
			{
				var server = new PrintServerForTesting(TestConnection);
				server.UpdateExistingPrintQueuesDeleteStatus_Exposed(printServerName, new List<PrintQueueInfo>() { new PrintQueueInfo { Name = "TestQueue1" } }, TestConnection);

				var factory = new BusinessObjectFactory { RefreshEnabled = false };
				var queue1 = factory.Load<StmPrintQueue>(queue1PK);
				var log1 = queue1.Logs.MostRecentLogByEventTime(Events.EditedARecord);
				AssertEquals("Set it to online with printers TestQueue1", log1.SL_Reference);

				var queue2 = factory.Load<StmPrintQueue>(queue2PK);
				var log2 = queue2.Logs.MostRecentLogByEventTime(Events.EditedARecord);
				AssertEquals("Set it to offline with printers TestQueue1", log2.SL_Reference);
			});
		}

		public void TestUpdateSuspectedSurrogateQueuesAllowPrintingStatusShouldSetLastEditUserAndTime()
		{
			AssertShouldSetLastEditUserAndTime((printServerName, queue1PK, queue2PK) =>
			{
				var printQueues = new List<PrintQueueInfo>();
				printQueues.Add(new PrintQueueInfo { Name = "TestQueue1", IsSuspectedSurrogate = true });
				printQueues.Add(new PrintQueueInfo { Name = "TestQueue2", IsSuspectedSurrogate = true });

				var server = new PrintServerForTesting(TestConnection);
				server.UpdateSuspectedSurrogateQueuesAllowPrintingStatus_Exposed(printServerName, printQueues, TestConnection);
			});
		}

		public void TestUpdatePrintQueuesWebPrintServerAddressShouldSetLastEditUserAndTime()
		{
			AssertShouldSetLastEditUserAndTime((printServerName, queue1PK, queue2PK) =>
			{
				var server = new PrintServerForTesting(TestConnection);
				server.UpdatePrintQueuesWebPrintServerAddress(printServerName, new[] { "TestQueue1", "TestQueue2" }, "JerryTestNewAddress", "JerryTestAddress", TestConnection);
			});
		}

		void AssertShouldSetLastEditUserAndTime(Action<string, Guid, Guid> action)
		{
			var serverName = "JerryTestServerName";
			var queue1PK = Guid.NewGuid();
			var queue2PK = Guid.NewGuid();
			var systemLastEditTimeUtc = new ZDateTime(2024, 3, 18, 0, 0, 0);

			var sqlText = string.Format(@"
				DECLARE @ServerPk UNIQUEIDENTIFIER = NEWID()
				INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (@ServerPk, '{0}')

				INSERT dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_QueueName, SQ_DisplayName, SQ_AllowPrinting, SQ_WebPrintServiceAddress, SQ_QueueDeleted, SQ_SystemCreateUser, SQ_SystemCreateTimeUtc, SQ_SystemLastEditUser, SQ_SystemLastEditTimeUtc)
					VALUES ('{1}', @ServerPk, 'TestQueue1', 'TestQueueDisplay1', 1, 'JerryTestAddress', '2024-03-18 00:00:00', 'TSF', '2024-03-18 00:00:00', 'TSF', '2024-03-18 00:00:00')
				INSERT dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_QueueName, SQ_DisplayName, SQ_AllowPrinting, SQ_WebPrintServiceAddress, SQ_QueueDeleted, SQ_SystemCreateUser, SQ_SystemCreateTimeUtc, SQ_SystemLastEditUser, SQ_SystemLastEditTimeUtc)
					VALUES ('{2}', @ServerPk, 'TestQueue2', 'TestQueueDisplay2', 1, 'JerryTestAddress', null, 'TSF', '2024-03-18 00:00:00', 'TSF', '2024-03-18 00:00:00')",

				serverName, queue1PK.ToString(), queue2PK.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			var queue1 = Factory.Load<StmPrintQueue>(queue1PK);
			var queue2 = Factory.Load<StmPrintQueue>(queue2PK);

			AssertEquals("TSF", queue1.SQ_SystemCreateUser);
			AssertEquals(systemLastEditTimeUtc, queue1.SQ_SystemLastEditTimeUtc);
			AssertEquals("TSF", queue2.SQ_SystemCreateUser);
			AssertEquals(systemLastEditTimeUtc, queue2.SQ_SystemLastEditTimeUtc);

			action.Invoke(serverName, queue1PK, queue2PK);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var reloadQueue1 = newFactory.Load<StmPrintQueue>(queue1PK);
			var reloadQueue2 = newFactory.Load<StmPrintQueue>(queue2PK);

			AssertEquals("ZZ", reloadQueue1.SQ_SystemLastEditUser);
			AssertEquals(ZDateTime.UtcNow.ToShortDateString(), reloadQueue1.SQ_SystemLastEditTimeUtc.ToShortDateString());

			AssertEquals("ZZ", reloadQueue2.SQ_SystemLastEditUser);
			AssertEquals(ZDateTime.UtcNow.ToShortDateString(), reloadQueue2.SQ_SystemLastEditTimeUtc.ToShortDateString());
		}

		public void TestCreateNewPrintQueuesShouldSetLastCreateAndEditUserAndTime()
		{
			var server = new PrintServerForTesting(TestConnection);
			server.CreateNewPrintQueues_Exposed("JerryTestServerName", new List<PrintQueueInfo>() { new () { Name = "JerryTestQueue" } }, TestConnection);

			var queue = Factory.LoadTop1<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.SQ_QueueName, "JerryTestQueue"));
			AssertEquals("ZZ", queue.SQ_SystemCreateUser);
			AssertEquals(ZDateTime.UtcNow.ToShortDateString(), queue.SQ_SystemCreateTimeUtc.ToShortDateString());
			AssertEquals("ZZ", queue.SQ_SystemLastEditUser);
			AssertEquals(ZDateTime.UtcNow.ToShortDateString(), queue.SQ_SystemLastEditTimeUtc.ToShortDateString());
		}

		#endregion

		public void TestJobStatusHasChangedFromQUE()
		{
			var printJob1 = NewTestPrintJobWithValidData(nameof(PrintJobType.PRN));
			var printJob2 = NewTestPrintJobWithValidData(nameof(PrintJobType.PRN));
			printJob2.SP_Status = nameof(PrintJobStatus.WRK);

			Factory.Save();

			var printServer = new PrintServerForTesting(TestConnection);

			AssertEquals("Job Status has not changed.", false, printServer.JobStatusHasChangedFromQUE_Exposed(Db.Connection, printJob1.PK.ToGuid()));
			AssertEquals("Job Status has changed.", true, printServer.JobStatusHasChangedFromQUE_Exposed(Db.Connection, printJob2.PK.ToGuid()));
		}

		public void TestStmPrintJobShouldBeRemovedIfStatusHasChangedDuringGetPrintJobs()
		{
			var serverName = "~JerryTestPrintServer";
			var printQueue = CreatePrintQueueWithDisplayName(serverName, "TestingQueue", "Testing");
			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			var printJob1 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup.PK);
			var printJob2 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup.PK);
			printJob1.SP_SQ = printQueue.PK;
			printJob2.SP_SQ = printQueue.PK;
			AddPrintJobToQueue(printJob1);
			AddPrintJobToQueue(printJob2);

			Factory.Save();

			var printServer = new PrintServerForTesting(TestConnection);
			var jobs = printServer.GetPrintAndFaxJobs_Exposed(serverName);

			AssertEquals("There should be 2 print jobs", 2, jobs.Count);

			printServer.MarkFailedProcessedPrintJobsPK.Add(printJob1.PK.ToGuid());
			printServer.MarkFailedProcessedPrintJobsPK.Add(printJob2.PK.ToGuid());
			jobs = printServer.GetPrintAndFaxJobs_Exposed(serverName);

			AssertEquals("There should be no print job", 0, jobs.Count);
		}

		public void TestSetPrintQueues()
		{
			var serverName1 = "~TestPrintServer1";
			var serverName2 = "~TestPrintServer2";

			var queue1 = CreatePrintQueue(serverName1, "TestQueue1", ZDateTime.Empty);
			var queue2 = CreatePrintQueue(serverName1, "TestQueue2", ZDateTime.Now);
			var queue3 = CreatePrintQueue(serverName1, "QueueWith'Quote1", ZDateTime.Empty);
			var queue4 = CreatePrintQueue(serverName2, "QueueWith'Quote", ZDateTime.Empty);

			Factory.Save();

			var queueListToSet = new List<string>
			{
				queue1.SQ_QueueName, queue2.SQ_QueueName, queue4.SQ_QueueName
			};

			var testPrintServer = new PrintServerForTesting(TestConnection);
			testPrintServer.SetPrintQueuesCore_Exposed(serverName1, queueListToSet);

			var factory = Factory.CreateNewFactory();
			var testQueue = factory.Load<StmPrintQueue>(queue1.PK);
			AssertEquals("queue1 shouldn't be changed because still exists in current list", ZDateTime.Empty, testQueue.SQ_QueueDeleted);

			testQueue = factory.Load<StmPrintQueue>(queue2.PK);
			AssertEquals("queue2 should be marked as not deleted", ZDateTime.Empty, testQueue.SQ_QueueDeleted);

			testQueue = factory.Load<StmPrintQueue>(queue3.PK);
			AssertNotEquals("queue3 should be marked as deleted because doesn't exist anymore", ZDateTime.Empty, testQueue.SQ_QueueDeleted);

			var serverSubQuery = new ZDBOnlySubQuery(typeof(StmPrintServer), StmPrintQueueSchema.SQ_SPS_Server);
			serverSubQuery.AddToFilter(StmPrintServerSchema.SPS_ServerName, serverName1);
			var query = new ZDBOnlyQuery(typeof(StmPrintQueue));
			query.AddSubQuery(serverSubQuery, JoinCondition.And);
			query.AddToFilter(StmPrintQueueSchema.SQ_QueueName, queue4.SQ_QueueName);
			testQueue = factory.LoadTop1<StmPrintQueue>(query);

			AssertNotNull("Server 1 - queue4 should be added as new print queue with ", testQueue);
			AssertEquals("Server 1 - queue4 should be not deleted", ZDateTime.Empty, testQueue.SQ_QueueDeleted);

			testQueue = factory.Load<StmPrintQueue>(queue4.PK);
			AssertEquals("Server2 - queue4 shouldn't be changed", ZDateTime.Empty, testQueue.SQ_QueueDeleted);
		}

		public void TestPrintingFailureWillSendAnEmailToTheUser()
		{
			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue.SQ_DisplayName = "DasPrint";

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "myemail@gmail.com";
			user.GS_Code = "EML";

			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_RetryAttempts = 3;
			printJob.SP_Status = nameof(PrintJobStatus.QUE);
			printJob.SP_GS_NKJobSubmittedBy = user.GS_Code;
			printJob.SP_SQ = printQueue.PK;
			printJob.SP_DocumentName = "PrintMe.bb";

			Factory.Save();

			const string expectedBody = @"Failed to print document 'PrintMe.bb' on printer 'DasPrint' after three attempts:

Error Message: Test printing failure after three attempts

Job Type: [PRN]
Document Name: [PrintMe.bb]
No. of Copies: [1]
Email/Fax Destination: []
Email Subject: []
Email Attachments: [default.XLS]
Parent Table: [Test]";

			var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
			mockEmailSender.Setup(m => m.SendEmailToUser("EML", "Print Job Failed", expectedBody)).Returns(true);

			var printServer = new PrintServerForTesting(TestConnection, mockEmailSender.Object);
			var jobList = new List<PrintJobFailed>
			{
				new (printJob.PK.ToGuid(), "Test printing failure after three attempts")
			};

			printServer.SetPrintJobFailureCore_Exposed(jobList);

			mockEmailSender.VerifyAll();
			Assert("NUnit doesnt recognise VerifyAllExpectations as non-empty test", true);
		}

		public void TestSP_RetryAttemptsAround3_SettingSP_Status()
		{
			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue.SQ_DisplayName = "DasPrint";

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "myemail@gmail.com";
			user.GS_Code = "EML";

			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_RetryAttempts = 2;
			printJob.SP_GS_NKJobSubmittedBy = user.GS_Code;
			printJob.SP_SQ = printQueue.PK;
			printJob.SP_DocumentName = "PrintMe.bb";

			var failingJob1 = Factory.NewWithValidTestData<StmPrintJob>();
			failingJob1.SP_RetryAttempts = 0;
			failingJob1.SP_GS_NKJobSubmittedBy = user.GS_Code;
			failingJob1.SP_SQ = printQueue.PK;
			failingJob1.SP_DocumentName = "PrintMe.bb";

			var failingJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			failingJob2.SP_RetryAttempts = 1;
			failingJob2.SP_GS_NKJobSubmittedBy = user.GS_Code;
			failingJob2.SP_SQ = printQueue.PK;
			failingJob2.SP_DocumentName = "PrintMe.bb";

			var failingJob3 = Factory.NewWithValidTestData<StmPrintJob>();
			failingJob3.SP_RetryAttempts = 3;
			failingJob3.SP_Status = nameof(PrintJobStatus.QUE);
			failingJob3.SP_GS_NKJobSubmittedBy = user.GS_Code;
			failingJob3.SP_SQ = printQueue.PK;
			failingJob3.SP_DocumentName = "PrintMe.bb";

			Factory.Save();

			var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
			mockEmailSender.Setup(m => m.SendEmailToUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			var printServer = new PrintServerForTesting(TestConnection, mockEmailSender.Object);
			var jobList = new List<PrintJobFailed>
			{
				new (failingJob1.PK.ToGuid(), "Test with multiple failed jobs failure after three attempts"),
				new (failingJob2.PK.ToGuid(), "Test with multiple failed jobs failure after three attempts"),
				new (failingJob3.PK.ToGuid(), "Test with multiple failed jobs failure after three attempts"),
			};

			printServer.SetPrintJobFailureCore_Exposed(jobList);

			failingJob1.Reload();
			AssertEquals(1, (int)failingJob1.SP_RetryAttempts);
			AssertEquals(nameof(PrintJobStatus.QUE), failingJob1.SP_Status);

			failingJob2.Reload();
			AssertEquals(1, (int)failingJob2.SP_RetryAttempts);
			AssertEquals(nameof(PrintJobStatus.QUE), failingJob2.SP_Status);

			failingJob3.Reload();
			AssertEquals(3, (int)failingJob3.SP_RetryAttempts);
			AssertEquals(nameof(PrintJobStatus.FAL), failingJob3.SP_Status);

			printJob.Reload();
			AssertEquals(2, (int)printJob.SP_RetryAttempts);
			AssertEquals(nameof(PrintJobStatus.QUE), printJob.SP_Status);
		}

		public void TestPrintingWithNoJobSubmittedBy()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_RetryAttempts = 2;
			printJob.SP_GS_NKJobSubmittedBy = "";

			Factory.Save();

			var mockEmailSender = new Mock<IEmailSender>();
			mockEmailSender.Setup(m => m.SendEmailToUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new InvalidOperationException());

			var printServer = new PrintServerForTesting(TestConnection, mockEmailSender.Object);
			var jobList = new List<PrintJobFailed>
			{
				new (printJob.PK.ToGuid(), "Test with multiple failed jobs failure after three attempts")
			};

			AssertNoExceptionThrown("We should not try to send an email - there is nobody to email", () => printServer.SetPrintJobFailureCore_Exposed(jobList));
		}

		public void TestPostmasterPrintFailEmailsAreCollated_BecauseTheyAreMostLikelyToBeSpammed()
		{
			var jobs = Enumerable.Range(0, 5)
				.Select(i =>
				{
					var job = Factory.NewWithValidTestData<StmPrintJob>();
					job.SP_RetryAttempts = 3;
					job.SP_Status = nameof(PrintJobStatus.QUE);
					job.SP_DocumentName = "Doc " + i + ".xls";
					return job;
				}).ToList();

			Factory.Save();

			string emailBody = null;
			var emailMock = new Mock<IEmailSender>();
			emailMock.Setup(m => m.SendEmailToGroup(PostMastersGroupPK, "Print Job(s) Failed", It.IsAny<string>()))
				.Callback<Guid, string, string>((_, _, arg3) => emailBody = arg3);

			var printServer = new PrintServerForTesting(TestConnection, emailMock.Object);
			var jobList = jobs.Select(job => new PrintJobFailed(job.PK.ToGuid(), "Test with multiple failed jobs failure after three attempts")).ToList();
			printServer.SetPrintJobFailureCore_Exposed(jobList);

			emailMock.Verify(m => m.SendEmailToGroup(PostMastersGroupPK, "Print Job(s) Failed", It.IsAny<string>()), Times.Once());

			foreach (var job in jobs)
			{
				AssertContains("Should have all the job names", job.SP_DocumentName, emailBody);
			}
		}

		public void TestPrintingWithMultipleFailedJobsOnlyEmailsForTheOneWithNoMoreRetries()
		{
			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue.SQ_DisplayName = "My Printer";

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "myemail@gmail.com";
			user.GS_Code = "EML";

			var failingJob = Factory.NewWithValidTestData<StmPrintJob>();
			failingJob.SP_RetryAttempts = 3;
			failingJob.SP_Status = nameof(PrintJobStatus.QUE);
			failingJob.SP_GS_NKJobSubmittedBy = user.GS_Code;
			failingJob.SP_SQ = printQueue.PK;
			failingJob.SP_DocumentName = "Some doc.xls";

			var newJob = Factory.NewWithValidTestData<StmPrintJob>();
			newJob.SP_RetryAttempts = 3;
			newJob.SP_Status = nameof(PrintJobStatus.QUE);
			newJob.SP_GS_NKJobSubmittedBy = user.GS_Code;
			newJob.SP_SQ = printQueue.PK;
			newJob.SP_DocumentName = "Some doc.xls";

			Factory.Save();

			const string expectedBody = @"Failed to print document 'Some doc.xls' on printer 'My Printer' after three attempts:

Error Message: Test with multiple failed jobs failure after three attempts

Job Type: [PRN]
Document Name: [Some doc.xls]
No. of Copies: [1]
Email/Fax Destination: []
Email Subject: []
Email Attachments: [default.XLS]
Parent Table: [Test]";

			var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
			mockEmailSender.Setup(m => m.SendEmailToUser("EML", "Print Job Failed", expectedBody)).Returns(true);

			var printServer = new PrintServerForTesting(TestConnection, mockEmailSender.Object);
			var jobList = new List<PrintJobFailed>
			{
				new (failingJob.PK.ToGuid(), "Test with multiple failed jobs failure after three attempts")
			};

			printServer.SetPrintJobFailureCore_Exposed(jobList);
			mockEmailSender.Verify(m => m.SendEmailToUser("EML", "Print Job Failed", expectedBody), Times.Once());

			failingJob.Reload();
			AssertEquals(3, (int)failingJob.SP_RetryAttempts);
			AssertEquals(nameof(PrintJobStatus.FAL), failingJob.SP_Status);
			
			newJob.Reload();
			AssertEquals(3, (int)newJob.SP_RetryAttempts);
			AssertEquals(nameof(PrintJobStatus.QUE), newJob.SP_Status);
		}

		public void TestSetPrintJobSuccess()
		{
			var job1 = Factory.NewWithValidTestData<StmPrintJob>();
			job1.SP_RelatedBusinessContext = "";
			var job2 = Factory.NewWithValidTestData<StmPrintJob>();
			job2.SP_RelatedBusinessContext = "RBC";
			Factory.Save();

			var jobPkList = new List<Guid>(new [] { job1.PK.ToGuid(), job2.PK.ToGuid() });

			var testPrintServer = new PrintServerForTesting(TestConnection);
			testPrintServer.SetPrintJobSuccessCore_Exposed(jobPkList);

			var factory = Factory.CreateNewFactory();
			var testJob = factory.Load<StmPrintJob>(job1.PK);
			AssertEquals(StmPrintJobSchema.Constants.SP_JobType, "PRS", testJob.SP_JobType);
			AssertEquals(StmPrintJobSchema.Constants.SP_RetryAttempts, (byte)0, testJob.SP_RetryAttempts);

			testJob = factory.Load<StmPrintJob>(job2.PK);
			AssertEquals(StmPrintJobSchema.Constants.SP_JobType, "PRS", testJob.SP_JobType);
			AssertEquals(StmPrintJobSchema.Constants.SP_RetryAttempts, (byte)0, testJob.SP_RetryAttempts);
		}

		public void TestSetPrintJobFailure()
		{
			var jobList = new List<PrintJobFailed>();
			for (var i = 0; i < 3; i++)
			{
				var job = Factory.NewWithValidTestData<StmPrintJob>();
				job.SP_JobType = nameof(PrintType.PRN);

				var failedJob = new PrintJobFailed { JobPk = job.PK.ToGuid(), FailureReason = "Test Failure" };
				jobList.Add(failedJob);
			}

			Factory.Save();

			var printServer = new PrintServerForTesting(TestConnection);

			for (var i = 0; i < 3; i++)
			{
				Factory.ReloadAllSafe<StmPrintJob>();
				foreach (var failedJob in jobList)
				{
					var job = Factory.Load<StmPrintJob>(failedJob.JobPk);

					AssertEquals("The job should not have been set to success (PRS)", nameof(PrintType.PRN), job.SP_JobType);

					var expectedRetries = i <= 1 ? i : 1;

					AssertEquals("The jobs retry count should have been increased", expectedRetries, job.SP_RetryAttempts);
				}

				printServer.SetPrintJobFailureCore_Exposed(jobList);
			}
		}

		public void TestSetPrintJobFailureWithDuplicates()
		{
			var jobList = new List<PrintJobFailed>();
			for (var i = 0; i < 3; i++)
			{
				var job = Factory.NewWithValidTestData<StmPrintJob>();
				job.SP_JobType = nameof(PrintType.PRN);

				var failedJob = new PrintJobFailed { JobPk = job.PK.ToGuid(), FailureReason = "Test Failure" };
				jobList.Add(failedJob);
			}
			jobList.Add(new PrintJobFailed { JobPk = jobList[0].JobPk, FailureReason = "Duplicate job PK" });

			Factory.Save();

			var printServer = new PrintServerForTesting(TestConnection);

			AssertNoExceptionThrown(() => printServer.SetPrintJobFailureCore_Exposed(jobList));
		}

		public void TestGetPrintJobs()
		{
			AssertGetPrintJobs((printServer, serverName) => printServer.GetPrintJobsCore_Exposed(serverName));
		}

		public void TestGetPrintJobsSynchronous()
		{
			AssertGetPrintJobs((printServer, serverName) => printServer.GetPrintJobs<ServerPrintJob>(serverName));
		}

		public void TestGetPrintJobs_CountRetries()
		{
			var queue = CreatePrintQueue("~TestPrintServer1", "TestQueue1", ZDateTime.Empty);
			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			var printJob1 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup.PK);
			printJob1.SP_RetryAttempts = 0;
			printJob1.SP_SQ = queue.PK;
			var printJob2 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup.PK);
			printJob2.SP_RetryAttempts = 1;
			printJob2.SP_SQ = queue.PK;
			var printJob3 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup.PK);
			printJob3.SP_RetryAttempts = 2;
			printJob3.SP_SQ = queue.PK;
			var printJob4 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup.PK);
			printJob4.SP_RetryAttempts = 3;
			printJob4.SP_SQ = queue.PK;

			AddPrintJobToQueue(printJob1);
			AddPrintJobToQueue(printJob2);
			AddPrintJobToQueue(printJob3);

			Factory.Save();

			var testPrintServer = new PrintServerForTesting(TestConnection);
			var jobList = testPrintServer.GetPrintJobsCore_Exposed("~TestPrintServer1");

			AssertEquals("There should be 3 print jobs", 3, jobList.Count);
			AssertEquals(printJob1.PK, jobList[0].JobPk);
			AssertEquals(printJob2.PK, jobList[1].JobPk);
			AssertEquals(printJob3.PK, jobList[2].JobPk);

			var selectSql = $@"
				SELECT SP_PK, SP_RunDateTime, SP_RetryAttempts
				FROM dbo.StmPrintJob
				WHERE SP_SQ = '{queue.PK.ToString()}'
				ORDER BY SP_RunDateTime";

			var reloadedJobs = new List<(Guid pk, byte retries)>();

			using (var command = TestConnection.Command(selectSql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var pk = (Guid)reader["SP_PK"];
					var retries = (byte)reader["SP_RetryAttempts"];
					reloadedJobs.Add((pk, retries));
				}
			}

			AssertEquals(4, reloadedJobs.Count);

			AssertEquals(printJob1.PK, reloadedJobs[0].pk);
			AssertEquals("Should increase retries by 1", (byte)1, reloadedJobs[0].retries);

			AssertEquals(printJob2.PK, reloadedJobs[1].pk);
			AssertEquals("Should increase retries by 1", (byte)2, reloadedJobs[1].retries);

			AssertEquals(printJob3.PK, reloadedJobs[2].pk);
			AssertEquals("Should increase retries by 1", (byte)3, reloadedJobs[2].retries);

			AssertEquals(printJob4.PK, reloadedJobs[3].pk);
			AssertEquals("Should keep maximum retries at 3", (byte)3, reloadedJobs[3].retries);
		}

		public void TestPrintJobHasWatermark()
		{
			var printQueue = CreatePrintQueue("~TestPrintServer", "TestQueue", ZDateTime.Empty);
			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			const string watermarkImage = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAgY0hSTQAAeiYAAICEAAD6AAAAgOgAAHUwAADqYAAAOpgAABdwnLpRPAAAAA1JREFUGFdj+M/A8B8ABQAB/6Zcm10AAAAASUVORK5CYII=";

			var printJob1 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup.PK);
			printJob1.SP_SQ = printQueue.PK;
			var printJob2 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup.PK);
			printJob2.SP_SQ = printQueue.PK;
			printJob2.SP_WatermarkText = "Training / Test  Non Commercial Use only";
			var printJob3 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup.PK);
			printJob3.SP_SQ = printQueue.PK;
			printJob3.SP_WatermarkImage = System.Text.Encoding.Unicode.GetBytes(watermarkImage);

			AddPrintJobToQueue(printJob1);
			AddPrintJobToQueue(printJob2);
			AddPrintJobToQueue(printJob3);
			Factory.Save();

			var testPrintServer = new PrintServerForTesting(TestConnection);
			var jobList = testPrintServer.GetPrintJobsCore_Exposed("~TestPrintServer");

			AssertEquals("There should be 3 print jobs", 3, jobList.Count);

			var expectedPrintJob1 = jobList.Find(j => j.JobPk == printJob1.PK);
			AssertNotNull(expectedPrintJob1);
			AssertEquals(false, expectedPrintJob1.HasWatermark);

			var expectedPrintJob2 = jobList.Find(j => j.JobPk == printJob2.PK);
			AssertNotNull(expectedPrintJob2);
			AssertEquals(true, expectedPrintJob2.HasWatermark);

			var expectedPrintJob3 = jobList.Find(j => j.JobPk == printJob3.PK);
			AssertNotNull(expectedPrintJob3);
			AssertEquals(true, expectedPrintJob3.HasWatermark);
		}

		public void TestNotifyOnJobFailure()
		{
			const string staffWithEmail = "EML", noEmailStaff = "NML";

			var allGoodJob = Factory.NewWithValidTestData<StmPrintJob>();
			allGoodJob.SP_GS_NKJobSubmittedBy = staffWithEmail;
			allGoodJob.SP_DocumentName = "All good.tiff";
			allGoodJob.SP_RetryAttempts = 3;
			allGoodJob.SP_Status = nameof(PrintJobStatus.QUE);

			var noSubmittedByJob = Factory.NewWithValidTestData<StmPrintJob>();
			noSubmittedByJob.SP_GS_NKJobSubmittedBy = string.Empty;
			noSubmittedByJob.SP_DocumentName = "No submitted by.tiff";
			noSubmittedByJob.SP_RetryAttempts = 3;
			noSubmittedByJob.SP_Status = nameof(PrintJobStatus.QUE);

			var noEmailJob = Factory.NewWithValidTestData<StmPrintJob>();
			noEmailJob.SP_GS_NKJobSubmittedBy = noEmailStaff;
			noEmailJob.SP_DocumentName = "No email.tiff";
			noEmailJob.SP_RetryAttempts = 3;
			noEmailJob.SP_Status = nameof(PrintJobStatus.QUE);

			Factory.Save();

			const string expectedBodyAllGood = @"Failed to print document 'All good.tiff' on printer '' after three attempts:

Error Message: Test notify job failure failure after three attempts

Job Type: [PRN]
Document Name: [All good.tiff]
No. of Copies: [1]
Email/Fax Destination: []
Email Subject: []
Email Attachments: [default.XLS]
Parent Table: [Test]";

			const string expectedBodyNoEmail = @"Failed to print document 'No email.tiff' on printer '' after three attempts:

Error Message: Test notify job failure failure after three attempts

Job Type: [PRN]
Document Name: [No email.tiff]
No. of Copies: [1]
Email/Fax Destination: []
Email Subject: []
Email Attachments: [default.XLS]
Parent Table: [Test]";

			const string expectedBodyPostMasterGroup = @"Failed to print the following documents:
	*Failed to print document 'No submitted by.tiff' on printer '' after three attempts:

Error Message: Test notify job failure failure after three attempts

Job Type: [PRN]
Document Name: [No submitted by.tiff]
No. of Copies: [1]
Email/Fax Destination: []
Email Subject: []
Email Attachments: [default.XLS]
Parent Table: [Test]
	*Failed to print document 'No email.tiff' on printer '' after three attempts:

Error Message: Test notify job failure failure after three attempts

Job Type: [PRN]
Document Name: [No email.tiff]
No. of Copies: [1]
Email/Fax Destination: []
Email Subject: []
Email Attachments: [default.XLS]
Parent Table: [Test]";

			var emailSender = new Mock<IEmailSender>(MockBehavior.Strict);
			emailSender.Setup(m => m.SendEmailToUser(staffWithEmail, "Print Job Failed", expectedBodyAllGood)).Returns(true);

			emailSender.Setup(m => m.SendEmailToUser(noEmailStaff, "Print Job Failed", expectedBodyNoEmail)).Returns(false);

			emailSender.Setup(m => m.SendEmailToGroup(PostMastersGroupPK, "Print Job(s) Failed", expectedBodyPostMasterGroup));

			var printServer = new PrintServerForTesting(TestConnection, emailSender.Object);
			var jobList = new List<PrintJobFailed>
			{
				new (allGoodJob.PK.ToGuid(), "Test notify job failure failure after three attempts"),
				new (noEmailJob.PK.ToGuid(), "Test notify job failure failure after three attempts"),
				new (noSubmittedByJob.PK.ToGuid(), "Test notify job failure failure after three attempts")
			};

			SqlEventTracker.Instance.Clear();
			printServer.SetPrintJobFailureCore_Exposed(jobList);
			emailSender.VerifyAll();
			AssertContains("Should call GetFailedPrintJobForRemotePrinting", "EXEC dbo.GetFailedPrintJobForRemotePrinting @FailedJobs", SqlEventTracker.Instance.SqlEventDescription);

			allGoodJob.Reload();
			AssertEquals(3, (int)allGoodJob.SP_RetryAttempts);
			AssertEquals(nameof(PrintJobStatus.FAL), allGoodJob.SP_Status);
			noSubmittedByJob.Reload();
			AssertEquals(3, (int)noSubmittedByJob.SP_RetryAttempts);
			AssertEquals(nameof(PrintJobStatus.FAL), noSubmittedByJob.SP_Status);
			noEmailJob.Reload();
			AssertEquals(3, (int)noEmailJob.SP_RetryAttempts);
			AssertEquals(nameof(PrintJobStatus.FAL), noEmailJob.SP_Status);
		}

		public void TestSetPrintJobFailureWithNullReference()
		{
			var printJob = new PrintServerForTesting(TestConnection);
			AssertNoExceptionThrown(() => printJob.SetPrintJobFailureCore_Exposed(null));
		}

		public void TestGetPrintAndFaxJobs()
		{
			var faxDeviceConfigPK1 = Guid.NewGuid();
			var faxDeviceConfigPK2 = Guid.NewGuid();

			var sqlText = string.Format(@"INSERT dbo.FaxDeviceConfig (FX_PK, FX_ServerName, FX_Name)
VALUES ('{0}', 'TestPrintServer1.some.where', '~TestPrintServer1')
INSERT dbo.FaxDeviceConfig (FX_PK, FX_ServerName, FX_Name)
VALUES ('{1}', 'TestPrintServer3.some.where', '~TestPrintServer3')", faxDeviceConfigPK1, faxDeviceConfigPK2);
			TestConnection.ExecuteNonQuery(sqlText);

			var printQueue = CreatePrintQueue("~TestPrintServer1", "TestQueue1", ZDateTime.Empty);
			var deliveryGroup1 = Factory.New<StmDeliveryGroup>();
			deliveryGroup1.SB_IsProcessed = true;
			var deliveryGroup2 = Factory.New<StmDeliveryGroup>();
			deliveryGroup2.SB_IsProcessed = true;

			var printJob1 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup1.PK);
			printJob1.SP_SQ = printQueue.PK;
			var printJob2 = CreateTestPrintJobOnly(nameof(PrintJobType.FAX), deliveryGroup1.PK);
			printJob2.SP_FX_FaxDevice = faxDeviceConfigPK1;
			printJob2.SP_FaxDestination = "555-555-1";
			var printJob3 = CreateTestPrintJobOnly(nameof(PrintJobType.FAX), deliveryGroup1.PK);
			printJob3.SP_FX_FaxDevice = faxDeviceConfigPK2;
			printJob3.SP_FaxDestination = "555-555-3";
			var printJob4 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup2.PK);
			printJob4.SP_SQ = printQueue.PK;

			AddPrintJobToQueue(printJob1);
			AddPrintJobToQueue(printJob2);
			AddPrintJobToQueue(printJob3);
			AddPrintJobToQueue(printJob4);

			Factory.Save();

			var testPrintServer = new PrintServerForTesting(TestConnection);
			var jobList = testPrintServer.GetPrintAndFaxJobs_Exposed("~TestPrintServer1");

			AssertEquals("There should be 1 print jobs", 1, jobList.Count);
			AssertEquals(printJob1.PK, jobList[0].JobPk);
		}

		public void TestGetPrintJobWithInvalidEmailAttachmentsFileName()
		{
			var printQueue = CreatePrintQueue("~TestPrintServer", "TestQueue", ZDateTime.Empty);
			var printJob = CreateTestPrintJobOnly(nameof(PrintJobType.PRN));
			printJob.SP_EmailAttachments = "a	b:c*d?e<f>g|h.txt";
			printJob.SP_SQ = printQueue.PK;

			AddPrintJobToQueue(printJob);
			Factory.Save();

			var testPrintServer = new PrintServerForTesting(TestConnection);
			var jobList = testPrintServer.GetPrintJobsCore_Exposed("~TestPrintServer");

			AssertEquals("There should be 1 print jobs", 1, jobList.Count);
			AssertEquals("Blob Type", "txt", jobList[0].BlobType);
			AssertEquals("Queue Name", "TestQueue", jobList[0].QueueName);
			AssertEquals("Email Subject Line", "PrintServerTest", jobList[0].EmailSubjectLine);
			AssertEquals("Job PK", printJob.PK, jobList[0].JobPk);
		}

		public void TestGetChangedPrintQueues()
		{
			var printServer1 = "~TestPrintServer1";
			var queueNameList = new List<string> { "Queue1", "QueueWith'Quote", "Queue3" };

			var queue1 = CreatePrintQueueWithDisplayName(printServer1, queueNameList[0], "Server1Queue1");
			queue1.SQ_SupressLetterhead = true;
			queue1.SQ_IsRollPaper = true;
			CreatePrintQueueWithDisplayName("~TestPrintServer2", queueNameList[0], "Server2Queue1");
			var queue3 = CreatePrintQueueWithDisplayName(printServer1, queueNameList[1], "Server1Queue2");
			Factory.Save();

			var testPrintServer = new PrintServerForTesting(TestConnection);
			var queueList = testPrintServer.GetChangedPrintQueuesCore_Exposed(printServer1, queueNameList).OrderBy(queue => queue.Name).ToList();

			AssertEquals("There should be 2 print queues", 2, queueList.Count);
			AssertEquals("Queue0 Name", queue1.SQ_QueueName, queueList[0].Name);
			AssertEquals("Queue0 Display Name", queue1.SQ_DisplayName, queueList[0].DisplayName);
			AssertEquals("Queue0 SQ_SupressLetterhead", queue1.SQ_SupressLetterhead, queueList[0].SuppressLetterhead);
			AssertEquals(true, queueList[0].IsRollPaper);

			AssertEquals("Queue1 Name", queue3.SQ_QueueName, queueList[1].Name);
			AssertEquals("Queue1 Display Name", queue3.SQ_DisplayName, queueList[1].DisplayName);
			AssertEquals("Queue1 SQ_SupressLetterhead", queue3.SQ_SupressLetterhead, queueList[1].SuppressLetterhead);
			AssertEquals(false, queueList[1].IsRollPaper);
		}

		public void TestGetWatermark_XmlText()
		{
			DoesTestGetWatermark(@"<?xml version=""1.0"" encoding=""utf-16""?><Watermark><UseTextWatermark>Y</UseTextWatermark><TextWatermark>TestTextWatermark</TextWatermark><HorizontalAlignment>Left</HorizontalAlignment><VerticalAlignment>Top</VerticalAlignment><Rotation>30</Rotation><FontSize>90</FontSize><Opacity>50</Opacity><HorizontalOffset>10</HorizontalOffset><VerticalOffset>15</VerticalOffset><ImageWatermark /></Watermark>", true, "TestTextWatermark", Array.Empty<byte>());
		}

		public void TestGetWatermark_BlankText()
		{
			DoesTestGetWatermark("", true, "DRAFT", Array.Empty<byte>());
		}

		public void TestGetWatermark_Blob()
		{
			var expectedImage = new byte[] {
				137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,1,0,0,0,1,8,6,0,0,0,31,21,196,137,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,
				65,77,65,0,0,177,143,11,252,97,5,0,0,0,32,99,72,82,77,0,0,122,38,0,0,128,132,0,0,250,0,0,0,128,232,0,0,117,48,0,0,234,96,0,0,58,152,
				0,0,23,112,156,186,81,60,0,0,0,13,73,68,65,84,24,87,99,248,207,192,240,31,0,5,0,1,255,166,92,155,93,0,0,0,0,73,69,78,68,174,66,96,130 };
			DoesTestGetWatermark(@"<?xml version=""1.0"" encoding=""utf-16""?><Watermark><UseTextWatermark>N</UseTextWatermark><TextWatermark>DummyText</TextWatermark><HorizontalAlignment>Left</HorizontalAlignment><VerticalAlignment>Top</VerticalAlignment><Rotation>30</Rotation><FontSize>90</FontSize><Opacity>50</Opacity><HorizontalOffset>10</HorizontalOffset><VerticalOffset>15</VerticalOffset><ImageWatermark>iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAgY0hSTQAAeiYAAICEAAD6AAAAgOgAAHUwAADqYAAAOpgAABdwnLpRPAAAAA1JREFUGFdj+M/A8B8ABQAB/6Zcm10AAAAASUVORK5CYII=</ImageWatermark></Watermark>", false, "DummyText", expectedImage);
		}

		// If the Watermark Registry Item default values change,
		// RPSCore.ServerWatermarkInternal class MUST be adjusted.
		public void TestServerWatermarkDefaultsAreSameAsRegistryWatermarkOnes()
		{
			WatermarkRegistryItem watermarkRegistry = DocumentsDataRegistry.Instance.Watermark;
			AssertEquals("Registry Item Name", watermarkRegistry.Name, PrintServer.WatermarkRegistryItemName);

			var sqlText = $"SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = '{PrintServer.WatermarkRegistryItemName}' AND SD_Owner is null AND SD_DepartmentGuid is null";
			var binaryValue = TestConnection.ExecuteScalar(sqlText);
			AssertNull("[PRE-CONDITION] There should be no Watermark record on StmData", binaryValue);

			var testWatermark = watermarkRegistry.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var testPrintServer = new PrintServerForTesting(TestConnection);
			var testServerWatermark = testPrintServer.GetWatermarkCore_Exposed();

			AssertEquals("UseTextWatermark", testWatermark.UseTextWatermark, testServerWatermark.UseTextWatermark);
			AssertEquals("TextWatermark", testWatermark.TextWatermark, testServerWatermark.TextWatermark);
			AssertEquals("ImageWatermark", testWatermark.ImageWatermarkAsBytes, testServerWatermark.ImageWatermark);
			AssertEquals("HorizontalAlignment", testWatermark.HorizontalAlignment, testServerWatermark.HorizontalAlignment);
			AssertEquals("VerticalAlignment", testWatermark.VerticalAlignment, testServerWatermark.VerticalAlignment);
			AssertEquals("Rotation", testWatermark.Rotation, testServerWatermark.Rotation);
			AssertEquals("FontSize", testWatermark.FontSize, testServerWatermark.FontSize);
			AssertEquals("Opacity", testWatermark.Opacity, testServerWatermark.Opacity);
			AssertEquals("HorizontalOffset", testWatermark.HorizontalOffset, testServerWatermark.HorizontalOffset);
			AssertEquals("VerticalOffset", testWatermark.VerticalOffset, testServerWatermark.VerticalOffset);
		}

		// If the Watermark Registry Item XML schema changes, RPSCore.ServerWatermark class MUST be adjusted.
		public void TestGetWatermarkCompliesWithCurrentRegistryWatermarkXmlSchema()
		{
			var testWatermark = new Watermark
			{
				UseTextWatermark = true, TextWatermark = "SampleWatermarkText", HorizontalAlignment = Watermark.HorizontalAlignmentCodes.Right,
				VerticalAlignment = Watermark.VerticalAlignmentCodes.Bottom,
				Rotation = 20,
				FontSize = 100,
				Opacity = 30,
				HorizontalOffset = 15,
				VerticalOffset = 25
			};

			DocumentsDataRegistry.Instance.Watermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testWatermark);

			var testPrintServer = new PrintServerForTesting(TestConnection);
			var testServerWatermark = testPrintServer.GetWatermark();

			AssertEquals("UseTextWatermark", true, testServerWatermark.UseTextWatermark);
			AssertEquals("TextWatermark", "SampleWatermarkText", testServerWatermark.TextWatermark);
			AssertEquals("ImageWatermark", Array.Empty<byte>(), testServerWatermark.ImageWatermark);
			AssertEquals("HorizontalAlignment", "Right", testWatermark.HorizontalAlignment);
			AssertEquals("VerticalAlignment", "Bottom", testWatermark.VerticalAlignment);
			AssertEquals("Rotation", 20, testWatermark.Rotation);
			AssertEquals("FontSize", 100, testWatermark.FontSize);
			AssertEquals("Opacity", 30, testWatermark.Opacity);
			AssertEquals("HorizontalOffset", 15, testWatermark.HorizontalOffset);
			AssertEquals("VerticalOffset", 25, testWatermark.VerticalOffset);

			Db.Connection.CloseConnection();
		}

		// If the compression mechanism used by the business layer changes,
		// the one used by the RPSCore.PrintServer MUST follow it
		public void TestGetPrintJobsUsesTheRightCompression()
		{
			var originalJobContents = resourceRetriever.Value.GetBytes("Enterprise.RemotePrinting.Server.Testing.TestDocument.xls");
			var printQueue = CreatePrintQueue("~TestPrintServer", "TestQueue", ZDateTime.Empty);
			var printJob = CreateTestPrintJobOnly(nameof(PrintJobType.PRN));
			printJob.SP_SQ = printQueue.PK;
			printJob.SP_CustomProperties = originalJobContents;

			AddPrintJobToQueue(printJob);
			Factory.Save();

			var sqlText = $"SELECT SP_CustomProperties FROM dbo.StmPrintJob WHERE SP_PK = '{printJob.PK.ToString()}'";
			var dbJobContents = (byte[])Db.Connection.ExecuteScalar(sqlText);

			var loadingFactory = Factory.CreateNewFactory();
			var bizLayerJob = loadingFactory.Load<StmPrintJob>(printJob.PK);

			var testPrintServer = new PrintServerForTesting(TestConnection);
			var remotePrintingJobs = testPrintServer.GetPrintJobsCore_Exposed("~TestPrintServer");

			Assert("Job from DB should be compressed and from BizLayer should NOT - Length", bizLayerJob.SP_CustomProperties.Length != dbJobContents.Length);
			AssertEquals("RemotePrinting Job should be the original Job contents", originalJobContents, remotePrintingJobs[0].Contents);
			AssertEquals("RemotePrinting Job should be the same as the BizLayer Job", bizLayerJob.SP_CustomProperties, remotePrintingJobs[0].Contents);

			Db.Connection.CloseConnection();
		}

		public void TestGetPrintAndFaxJobsCompressed()
		{
			var originalJobContents = resourceRetriever.Value.GetBytes("Enterprise.RemotePrinting.Server.Testing.TestDocument.xls");
			var printQueue = CreatePrintQueue("~TestPrintServer", "TestQueue", ZDateTime.Empty);
			var printJob = CreateTestPrintJobOnly(nameof(PrintJobType.PRN));
			printJob.SP_SQ = printQueue.PK;
			printJob.SP_CustomProperties = originalJobContents;

			AddPrintJobToQueue(printJob);
			Factory.Save();

			var testPrintServer = new PrintServerForTesting(TestConnection);
			var remotePrintingJobs = testPrintServer.GetPrintAndFaxJobs_Exposed("~TestPrintServer");

			AssertEquals(true, Compressor.IsCompressed(remotePrintingJobs[0].Contents));

			Db.Connection.CloseConnection();
		}

		public void TestChangingPrintQueueChangesItsPrintQueueStateChanged()
		{
			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_QueueName = "~TestQueue";
			printQueue.SQ_ServerName = "~TestPrintServer";
			printQueue.SQ_TopMargin = 20;
			Factory.Save();

			var previousStateChanged = printQueue.SQ_PrintQueueStateChanged.ToGuid();
			Assert("StateChanged Guid should NOT be empty", previousStateChanged != Guid.Empty);

			printQueue.SQ_TopMargin = 30;
			Factory.Save();

			Assert("StateChanged Guid should have changed", printQueue.SQ_PrintQueueStateChanged.ToGuid() != previousStateChanged);
		}

		public void TestGetMaxBatchSize()
		{
			var printServer = new PrintServerForTesting(TestConnection);

			DocumentsDataRegistry.Instance.WebPrintDocumentPackMaxSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertEquals(0, printServer.GetMaxBatchSizeBaseExposed(TestConnection));

			DocumentsDataRegistry.Instance.WebPrintDocumentPackMaxSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			AssertEquals(1024 * 1024, printServer.GetMaxBatchSizeBaseExposed(TestConnection));

			DocumentsDataRegistry.Instance.WebPrintDocumentPackMaxSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals(10 * 1024 * 1024, printServer.GetMaxBatchSizeBaseExposed(TestConnection));

			DocumentsDataRegistry.Instance.WebPrintDocumentPackMaxSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 23);
			AssertEquals(23 * 1024 * 1024, printServer.GetMaxBatchSizeBaseExposed(TestConnection));
		}

		public void TestGetPrintJobsCoreWithMaxBatchSize()
		{
			var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			var printQueue = CreatePrintQueue("~TestPrintServer", "TestQueue", ZDateTime.Empty);
			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = ZBool.True;

			for (var i = 0; i < 14; i++)
			{
				var printJob = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup.PK);
				printJob.SP_SQ = printQueue.PK;
				printJob.SP_Sequence = i;
				printJob.SP_EmailSubjectLine = "P" + i;
				printJob.SP_CustomProperties = new ZBlob(content);

				AddPrintJobToQueue(printJob);
			}

			Factory.Save();

			var printServer = new PrintServerForTesting(TestConnection) { MaxBatchSizeOverride = 35 };
			var jobs = printServer.GetPrintJobsCore_Exposed(printQueue.SQ_ServerName);

			AssertEquals(3, jobs.Count);
			AssertEquals("P0", jobs[0].EmailSubjectLine);
			AssertEquals("P1", jobs[1].EmailSubjectLine);
			AssertEquals("P2", jobs[2].EmailSubjectLine);

			jobs = printServer.GetPrintJobsCore_Exposed(printQueue.SQ_ServerName);

			AssertEquals(3, jobs.Count);
			AssertEquals("P3", jobs[0].EmailSubjectLine);
			AssertEquals("P4", jobs[1].EmailSubjectLine);
			AssertEquals("P5", jobs[2].EmailSubjectLine);

			printServer.MaxBatchSizeOverride = 5; // bytes
			jobs = printServer.GetPrintJobsCore_Exposed(printQueue.SQ_ServerName);
			AssertEquals("Should have 1 job in batch", 1, jobs.Count);

			printServer.MaxBatchSizeOverride = 15; // bytes
			jobs = printServer.GetPrintJobsCore_Exposed(printQueue.SQ_ServerName);
			AssertEquals(1, jobs.Count);

			printServer.MaxBatchSizeOverride = 20; // bytes
			jobs = printServer.GetPrintJobsCore_Exposed(printQueue.SQ_ServerName);
			AssertEquals(2, jobs.Count);

			printServer.MaxBatchSizeOverride = 0;
			jobs = printServer.GetPrintJobsCore_Exposed(printQueue.SQ_ServerName);
			AssertEquals(4, jobs.Count);
		}

		public void TestUpdatePrintQueuesWebPrintServerAddress()
		{
			var printQueue1 = CreatePrintQueueWithWebServiceAddress("A", "1", "abc");
			var printQueue2 = CreatePrintQueueWithWebServiceAddress("A", "2", "");
			var printQueue3 = CreatePrintQueueWithWebServiceAddress("A", "3", "xyz");
			var printQueue4 = CreatePrintQueueWithWebServiceAddress("A", "4", "abc");
			var printQueue5 = CreatePrintQueueWithWebServiceAddress("A", "5", "abc");
			var printQueue6 = CreatePrintQueueWithWebServiceAddress("B", "6", "abc");
			Factory.Save();

			var printServer = new PrintServerForTesting(TestConnection);
			printServer.UpdatePrintQueuesWebPrintServerAddress("A", new[] { "1", "2", "3", "4", "6" }, "aaa.bbb.ccc", "abc", TestConnection);

			AssertPrintQueueWebServiceAddress("Should update print queue 1", printQueue1, "aaa.bbb.ccc");
			AssertPrintQueueWebServiceAddress("Old address was different", printQueue2, "");
			AssertPrintQueueWebServiceAddress("Old address was different", printQueue3, "xyz");
			AssertPrintQueueWebServiceAddress("Should update print queue 4", printQueue4, "aaa.bbb.ccc");
			AssertPrintQueueWebServiceAddress("Print queue 5 name was not included", printQueue5, "abc");
			AssertPrintQueueWebServiceAddress("Different print server", printQueue6, "abc");

			printServer.UpdatePrintQueuesWebPrintServerAddress("A", new[] { "1", "2", "3", "4", "6" }, "xxx.yyy.zzz", string.Empty, TestConnection);

			AssertPrintQueueWebServiceAddress("Should update print queue 1", printQueue1, "xxx.yyy.zzz");
			AssertPrintQueueWebServiceAddress("Should update print queue 2", printQueue2, "xxx.yyy.zzz");
			AssertPrintQueueWebServiceAddress("Should update print queue 3", printQueue3, "xxx.yyy.zzz");
			AssertPrintQueueWebServiceAddress("Should update print queue 4", printQueue4, "xxx.yyy.zzz");
			AssertPrintQueueWebServiceAddress("Print queue 5 name was not included", printQueue5, "abc");
			AssertPrintQueueWebServiceAddress("Different print server", printQueue6, "abc");
		}

		[UseSnapshotProtection]
		public void TestSendLogsFilesEmail()
		{
			var fileData = new byte[10];
			var mailFilter = new ZQuery(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.EndsWith, "WebPrint Client Logs") { ReLoadExistingRows = true };

			var mailDbItems = Factory.Load<MailItem>(mailFilter);
			AssertEquals("Precondition - no mails in db", 0, mailDbItems.Length);

			var printServer = new PrintServer();
			printServer.SendLogsFilesEmail("aaa@bbb.ccc", "test.zip", fileData, "Test data");

			mailDbItems = Factory.Load<MailItem>(mailFilter);

			AssertEquals(1, mailDbItems.Length);
			AssertEquals(new EnterpriseInformationRetriever().LicenceCode + " - WebPrint Client Logs", mailDbItems[0].MI_Subject);
			AssertEquals("Test data", mailDbItems[0].MI_Body);

			AssertEquals(1, mailDbItems[0].MailAttachments.Count);
			AssertEquals("test.zip", mailDbItems[0].MailAttachments[0].MA_FileName);
			AssertEquals(fileData, (byte[])mailDbItems[0].MailAttachments[0].MA_Data);

			AssertEquals(1, mailDbItems[0].MailRecipients.Count);
			AssertEquals("aaa@bbb.ccc", mailDbItems[0].MailRecipients[0].EmailAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		void AssertGetPrintJobs(Func<PrintServerForTesting, string, List<ServerPrintJob>> getJobsFunc)
		{
			var printQueue1 = CreatePrintQueue("~TestPrintServer1", "TestQueue1", ZDateTime.Empty);
			var printQueue2 = CreatePrintQueue("~TestPrintServer2", "TestQueue2", ZDateTime.Empty);
			var deliveryGroup1 = Factory.New<StmDeliveryGroup>();
			deliveryGroup1.SB_IsProcessed = false;
			var deliveryGroup2 = Factory.New<StmDeliveryGroup>();
			deliveryGroup2.SB_IsProcessed = true;

			var printJobQ1G11 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup1.PK);
			printJobQ1G11.SP_SQ = printQueue1.PK;
			var printJobQ1G21 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup2.PK);
			printJobQ1G21.SP_SQ = printQueue1.PK;
			var printJobQ1G22 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup2.PK);
			printJobQ1G22.SP_SQ = printQueue1.PK;
			var printJobQ2G21 = CreateTestPrintJobOnly(nameof(PrintJobType.PRN), deliveryGroup2.PK);
			printJobQ2G21.SP_SQ = printQueue2.PK;

			AddPrintJobToQueue(printJobQ1G21);
			AddPrintJobToQueue(printJobQ1G22);
			AddPrintJobToQueue(printJobQ2G21);

			Factory.Save();

			SqlEventTracker.Instance.Clear();
			var testPrintServer = new PrintServerForTesting(TestConnection);
			var jobList = getJobsFunc(testPrintServer, "~TestPrintServer1");

			AssertEquals("There should be 2 print jobs", 2, jobList.Count);
			AssertEquals("Job0 Queue", "TestQueue1", jobList[0].QueueName);
			AssertEquals("Job1 Queue", "TestQueue1", jobList[1].QueueName);

			AssertCollectionContains(jobList, j => j.JobPk == printJobQ1G21.PK);
			AssertCollectionContains(jobList, j => j.JobPk == printJobQ1G22.PK);
			AssertContains("Should call GetNextPrintJobForRemotePrinting", "EXEC dbo.GetNextPrintJobForRemotePrinting @ServerName", SqlEventTracker.Instance.SqlEventDescription);
		}

		void DoesTestGetWatermark(string watermarkXml, bool useText, string expectedText, byte[] expectedImage)
		{
			var sqlText = $@"INSERT dbo.StmData (SD_PK, SD_Name , SD_BinaryValue)
VALUES (NEWID(), '{PrintServer.WatermarkRegistryItemName}', convert(varbinary(8000), N'{watermarkXml}'))";

			if (!string.IsNullOrEmpty(watermarkXml))
			{
				TestConnection.ExecuteNonQuery(sqlText);
			}

			var testPrintServer = new PrintServerForTesting(TestConnection);
			var testWatermark = testPrintServer.GetWatermarkCore_Exposed();

			AssertEquals("UseTextWatermark", useText, testWatermark.UseTextWatermark);
			AssertEquals("TextWatermark", expectedText, testWatermark.TextWatermark);
			AssertEquals("ImageWatermark", expectedImage, testWatermark.ImageWatermark);

			if (!string.IsNullOrEmpty(watermarkXml))
			{
				AssertEquals("HorizontalAlignment", "Left", testWatermark.HorizontalAlignment);
				AssertEquals("VerticalAlignment", "Top", testWatermark.VerticalAlignment);
				AssertEquals("Rotation", 30, testWatermark.Rotation);
				AssertEquals("FontSize", 90, testWatermark.FontSize);
				AssertEquals("Opacity", 50, testWatermark.Opacity);
				AssertEquals("HorizontalOffset", 10, testWatermark.HorizontalOffset);
				AssertEquals("VerticalOffset", 15, testWatermark.VerticalOffset);
			}
		}

		void AssertPrintQueueWebServiceAddress(string message, StmPrintQueue printQueue, string expectedWebServiceAddress)
		{
			printQueue.Reload();
			AssertEquals(message, expectedWebServiceAddress, printQueue.SQ_WebPrintServiceAddress);
		}

		StmPrintQueue CreatePrintQueueWithDisplayName(string serverName, string queueName, string displayName)
		{
			var printQueue = CreatePrintQueue(serverName, queueName, ZDateTime.Empty);
			printQueue.SQ_DisplayName = displayName;

			return printQueue;
		}

		StmPrintQueue CreatePrintQueueWithWebServiceAddress(string serverName, string queueName, string webServiceAddress)
		{
			var printQueue = CreatePrintQueue(serverName, queueName, ZDateTime.Empty); 
			printQueue.SQ_WebPrintServiceAddress = webServiceAddress;

			return printQueue;
		}

		StmPrintQueue CreatePrintQueue(string serverName, string queueName, ZDateTime queueDeleted)
		{
			var queue = Factory.New<StmPrintQueue>();
			queue.SQ_QueueName = queueName;
			queue.SQ_ServerName = serverName;
			queue.SQ_QueueDeleted = queueDeleted;

			return queue;
		}

		Guid PostMastersGroupPK => RegistryData.WebPrintNotificationGroup(TestConnection);

		StmPrintJob CreateTestPrintJobOnly(ZString jobType, ZGuid? deliveryGroupGuid = null, ZGuid? parentGuid = null)
		{
			return NewTestPrintJobWithValidData(jobType, deliveryGroupGuid, parentGuid);
		}

		StmPrintJob NewTestPrintJobWithValidData(ZString jobType, ZGuid? deliveryGroupGuid = null, ZGuid? parentGuid = null)
		{
			if (deliveryGroupGuid == null)
			{
				var deliveryGroup = Factory.New<StmDeliveryGroup>();
				deliveryGroup.SB_IsProcessed = true;

				deliveryGroupGuid = deliveryGroup.PK;
			}

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = jobType;
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "TestDocument";
			printJob.SP_CustomProperties = new byte[] { 1, 2, 3, 4, 5 };
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "PrintServerTest";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = parentGuid ?? ZGuid.NewZGuid();
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-1);
			printJob.SP_SB_DeliveryGroup = deliveryGroupGuid.Value;
			printJob.SP_EDocsProcessed = ZBool.False;

			return printJob;
		}

		void AddPrintJobToQueue(StmPrintJob printJob)
		{
			if (printJob == null)
			{
				return;
			}

			var printJobQueue = printJob.Factory.New<StmPrintJobQueue>();
			printJobQueue.SPQ_JobType = printJob.SP_JobType;
			printJobQueue.SPQ_SP_PrintJob = printJob.PK;
			printJobQueue.SPQ_SB_DeliveryGroup = printJob.SP_SB_DeliveryGroup;
			printJobQueue.SPQ_ParentGuidForLock = printJob.SP_ParentGuid.ToString();
			printJobQueue.SPQ_EDocsProcessed = printJob.SP_EDocsProcessed;
			printJobQueue.SPQ_Sequence = 0;
			printJobQueue.SPQ_SQ_PrintQueue = printJob.SP_SQ;

			var printQueue = printJob.PrintQueue;
			if (printQueue != null)
			{
				printJobQueue.SPQ_SPS_PrintServer = printQueue.SQ_SPS_Server;
			}
		}
	}

	sealed class PrintJobAsyncTest : BasePrintJobAsyncTest<ServerPrintJob>
	{
	}

	sealed class PrintJobExAsyncTest : BasePrintJobAsyncTest<ServerPrintJobEx>
	{
	}

	abstract class BasePrintJobAsyncTest<T> : TestCase where T : ServerPrintJob, new()
	{
		[UseSnapshotProtection]
		public void TestSynchronousResult()
		{
			var deliveryGroup = factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			var printJob = factory.New<StmPrintJob>();
			printJob.SP_SQ = printQueue.PK;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob.SP_RunDateTime = ZDateTime.BrettsBirthday;
			printJob.SP_JobType = "PRN";
			AddPrintJobToQueue(printJob);

			factory.Save();

			var printServer = new PrintServer();
			List<T> jobs = null;
			var asyncResult = printServer.BeginGetPrintJobs<T>(printQueue.SQ_ServerName, ar => jobs = printServer.EndGetPrintJobs<T>(ar));
			Assert("asyncResult.IsCompleted", asyncResult.IsCompleted);
			Assert("asyncResult.CompletedSynchronously", asyncResult.CompletedSynchronously);
			AssertNotNull("jobs", jobs);
			AssertEquals(1, jobs.Count);
			AssertEquals(printJob.PK, jobs[0].JobPk);
		}

		[SnailTest]
		[UseSnapshotProtection]
		public void TestAsynchronousResult()
		{
			var printServer = new PrintServer();

			List<T> jobs = null;
			var asyncResult = printServer.BeginGetPrintJobs<T>(printQueue.SQ_ServerName, ar => jobs = printServer.EndGetPrintJobs<T>(ar));
			Assert("!asyncResult.IsCompleted", !asyncResult.IsCompleted);

			var deliveryGroup = factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			var printJob = factory.New<StmPrintJob>();
			printJob.SP_SQ = printQueue.PK;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob.SP_RunDateTime = ZDateTime.BrettsBirthday;
			printJob.SP_JobType = "PRN";
			AddPrintJobToQueue(printJob);

			factory.Save();

			var startTime = DateTime.UtcNow;
			do
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
			}
			while (!asyncResult.IsCompleted && DateTime.UtcNow.Subtract(startTime) < TimeSpan.FromMinutes(2));

			Assert("asyncResult.IsCompleted", asyncResult.IsCompleted);
			Assert("!asyncResult.CompletedSynchronously", !asyncResult.CompletedSynchronously);
			AssertNotNull("jobs", jobs);
			AssertEquals(1, jobs.Count);
			AssertEquals(printJob.PK, jobs[0].JobPk);
		}

		[SnailTest]
		[UseSnapshotProtection]
		public void TestEmptyResult()
		{
			var printServer = new PrintServer();

			List<T> jobs = null;
			var asyncResult = printServer.BeginGetPrintJobs<T>(printQueue.SQ_ServerName, ar => jobs = printServer.EndGetPrintJobs<T>(ar));
			Assert("!asyncResult.IsCompleted", !asyncResult.IsCompleted);

			var startTime = DateTime.UtcNow;
			do
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
			}
			while (!asyncResult.IsCompleted && DateTime.UtcNow.Subtract(startTime) < TimeSpan.FromMinutes(2));

			Assert("asyncResult.IsCompleted", asyncResult.IsCompleted);
			Assert("!asyncResult.CompletedSynchronously", !asyncResult.CompletedSynchronously);
			AssertNotNull("jobs", jobs);
			AssertEquals(0, jobs.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			printQueue = factory.New<StmPrintQueue>();
			printQueue.SQ_QueueName = "TEST";
			printQueue.SQ_ServerName = "TEST";
			printQueue.SQ_DisplayName = "TEST";
		}

		void AddPrintJobToQueue(StmPrintJob printJob)
		{
			var printJobQueue = printJob.Factory.New<StmPrintJobQueue>();
			printJobQueue.SPQ_JobType = printJob.SP_JobType;
			printJobQueue.SPQ_SP_PrintJob = printJob.PK;
			printJobQueue.SPQ_SB_DeliveryGroup = printJob.SP_SB_DeliveryGroup;
			printJobQueue.SPQ_ParentGuidForLock = printJob.SP_ParentGuid.ToString();
			printJobQueue.SPQ_EDocsProcessed = printJob.SP_EDocsProcessed;
			printJobQueue.SPQ_Sequence = 0;
			printJobQueue.SPQ_SQ_PrintQueue = printJob.SP_SQ;
			printJobQueue.SPQ_SPS_PrintServer = printJob.PrintQueue.SQ_SPS_Server;
		}

		readonly BusinessObjectFactory factory = new ();
		StmPrintQueue printQueue;

		#endregion
	}
}
