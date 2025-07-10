using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(FtpJobTask))]
	sealed class FtpJobTaskTest : ServiceTaskTestCase<FtpJobTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);

			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "FDD", hostedServiceAttribute.Code);
				AssertEquals("Description", "FTP documents delivery", hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			});
		}

		public void TestRunTask_WithNotificationSuccessToGroup()
		{
			var group = CreateGroupForNotification();
			CreateFtpPrintJob("TestRunTask_WithNotificationSuccess", CreateReportScheduleTask().PK);

			Factory.Save();

			var ftpJobTask = new FtpJobTask();
			ftpJobTask.ConfigString = new FtpJobConfig("") { NotifyOnSuccess = true, NotificationGroup_PK = group.PK }.ConfigString;

			InitialiseAndRunTaskSchedule(ftpJobTask);

			AssertEquals(1, TestHelper.CountMailDBItems("Scheduled Report FTP delivery successfully"));
			AssertEquals(1, TestHelper.CountMailDBRecipients("Samuel.Wang@cargowise.com"));
		}

		public void TestRunTask_WithNotificationSuccessToPrintUser()
		{
			CreateFtpPrintJob("TestRunTask_WithNotificationSuccess", CreateReportScheduleTask().PK);

			Factory.Save();

			var ftpJobTask = new FtpJobTask();
			ftpJobTask.ConfigString = new FtpJobConfig("") { NotifyOnSuccess = true, NotifyPrintUser = true }.ConfigString;

			InitialiseAndRunTaskSchedule(ftpJobTask);

			AssertEquals(1, TestHelper.CountMailDBItems("Scheduled Report FTP delivery successfully"));
			AssertEquals(1, TestHelper.CountMailDBRecipients("123@123.com"));
		}

		public void TestRunTask_WithNotificationSuccessToPrintUser_NoNullExceptionThrown()
		{
			CreateFtpPrintJob("TestRunTask_WithNotificationSuccess", ZGuid.Empty);

			Factory.Save();

			var ftpJobTask = new FtpJobTask();
			ftpJobTask.ConfigString = new FtpJobConfig("") { NotifyOnSuccess = true, NotifyPrintUser = true }.ConfigString;

			AssertNoExceptionThrown("No Null Exception Thrown", () => InitialiseAndRunTaskSchedule(ftpJobTask));
		}

		public void TestRunTask_WithNotificationFailureToGroup()
		{
			var group = CreateGroupForNotification();
			CreateFtpPrintJob("TestRunTask_WithNotificationFailure", CreateReportScheduleTask().PK);

			Factory.Save();

			var ftpJobTask = new FtpJobTask();
			ftpJobTask.ConfigString = new FtpJobConfig("") { NotifyOnFailure = true, NotificationGroup_PK = group.PK }.ConfigString;

			InitialiseAndRunTaskSchedule(ftpJobTask);

			AssertEquals(1, TestHelper.CountMailDBItems("Scheduled Report FTP delivery failure"));
			AssertEquals(1, TestHelper.CountMailDBRecipients("Samuel.Wang@cargowise.com"));
		}

		public void TestRunTask_WithNotificationFailureToPrintUser()
		{
			CreateFtpPrintJob("TestRunTask_WithNotificationFailure", CreateReportScheduleTask().PK);

			Factory.Save();

			var ftpJobTask = new FtpJobTask();
			ftpJobTask.ConfigString = new FtpJobConfig("") { NotifyOnFailure = true, NotifyPrintUser = true }.ConfigString;

			InitialiseAndRunTaskSchedule(ftpJobTask);

			AssertEquals(1, TestHelper.CountMailDBItems("Scheduled Report FTP delivery failure"));
			AssertEquals(1, TestHelper.CountMailDBRecipients("123@123.com"));
		}

		public void TestRunTask_WithNotificationFailureToPrintUser_NoNullExceptionThrown()
		{
			CreateFtpPrintJob("TestRunTask_WithNotificationFailure", CreateReportScheduleTask().PK);

			Factory.Save();

			var ftpJobTask = new FtpJobTask();
			ftpJobTask.ConfigString = new FtpJobConfig("") { NotifyOnFailure = true, NotifyPrintUser = true }.ConfigString;

			AssertNoExceptionThrown("No Null Exception Thrown", () => InitialiseAndRunTaskSchedule(ftpJobTask));
		}

		public void TestRunTask()
		{
			var ftpPrintJob = CreateFtpPrintJob("127.0.0.1", ZGuid.NewZGuid());

			Factory.Save();

			var deliveryGroupQuery = new ZQuery(StmDeliveryGroupSchema.PK, ftpPrintJob.SP_SB_DeliveryGroup);
			AssertEquals(1, Factory.GetDatabaseCount(typeof(StmDeliveryGroup), deliveryGroupQuery));

			var log = InitialiseAndRunTaskSchedule(new FtpJobTask());

			AssertEquals(3, log.Count);
			AssertEquals("Information|Processing 1 of 1 queued FTP jobs", log[0]);
			AssertEquals("Information|Starting FtpJobProcessor to process \"FTP\" print jobs", log[1]);
			AssertEquals("Information|Finished processing queue", log[2]);

			AssertEquals("No data purge in FDD service task", 1, Factory.GetDatabaseCount(typeof(StmDeliveryGroup), deliveryGroupQuery));
		}

		public void TestLogNotReportRowDeletedError()
		{
			var ftpPrintJob = CreateFtpPrintJob("127.0.0.1", ZGuid.NewZGuid());

			Factory.Save();

			var ftpJobTask = new FtpJobTask();
			InitialiseAndRunTaskSchedule(ftpJobTask);

			ftpPrintJob.Delete();

			ftpJobTask.LogTesting(TraceEventType.Error, "test");

			AssertEquals("Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestLongLogMessage()
		{
			var testParent = CreateReportScheduleTask();
			var ftpPrintJob = CreateFtpPrintJob("127.0.0.1", testParent.PK);

			Factory.Save();

			var deliveryGroupQuery = new ZQuery(StmDeliveryGroupSchema.PK, ftpPrintJob.SP_SB_DeliveryGroup);
			AssertEquals(1, Factory.GetDatabaseCount(typeof(StmDeliveryGroup), deliveryGroupQuery));

			var longLogText = "Long Log : " + new string('L', 1200);
			var task = new FtpJobTaskForLogTest(ftpPrintJob, TraceEventType.Error, longLogText);
			var serviceLog = InitialiseTaskSchedule(task, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
			RunTaskSchedule(task);

			AssertEquals(4, serviceLog.Count);
			AssertEquals("Error|" + longLogText, serviceLog[0]);
			AssertEquals("Information|Processing 1 of 1 queued FTP jobs", serviceLog[1]);
			AssertEquals("Information|Starting FtpJobProcessor to process \"FTP\" print jobs", serviceLog[2]);
			AssertEquals("Information|Finished processing queue", serviceLog[3]);

			Factory.Save();

			testParent.Reload();
			var testParentLog = testParent.Logs.MostRecentLog;
			AssertEquals("Long Log : "                                                                                   //  11
				+ "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL" // 100
				+ "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL" // 100
				+ "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL" // 100
				+ "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL" // 100
				+ "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL" // 100
				+ "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL" // 100
				+ "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL" // 100
				+ "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL" // 100
				+ "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL" // 100
				+ "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL" // 100
				+ "LLLLL|RES=FAI"                                                                                        //  13
					, testParentLog.SL_Reference);
			Assert(testParentLog.SL_Reference.Length <= AutoStmALog.Schema.SL_ReferenceMaxLength);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new (
						StmPrintJobQueueSchema.Constants.TableName,
						"FTP documents delivery",
						StmPrintJobQueueSchema.Constants.SPQ_JobType + "=FTP")
				};
			}
		}

		StmPrintJob CreateFtpPrintJob(string destination, ZGuid parentGuid)
		{
			var printJob = TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintType.FTP), null, parentGuid);
			printJob.SP_EmailFromAddress = "u1\0p1";
			printJob.SP_Destination = destination;

			return printJob;
		}

		PrintJobTaskTestHelper TestHelper => testHelper ??= new PrintJobTaskTestHelper(Factory);

		PrintJobTaskTestHelper testHelper;

		ReportScheduleTask CreateReportScheduleTask()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "123@123.com";
			staff.GS_Code = "JWA";

			var parent = Factory.NewWithValidTestData<ReportScheduleTask>();
			parent.S5_GS_NKPrintUser = staff.GS_Code;

			return parent;
		}

		GlbGroup CreateGroupForNotification()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "ENTPRJTST";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "Samuel.Wang@cargowise.com";
			staff.Groups.Add(group);

			return group;
		}

		sealed class FtpJobTaskForLogTest : FtpJobTask
		{
			public FtpJobTaskForLogTest(StmPrintJob printJob, TraceEventType eventType, string message)
			{
				this.eventType = eventType;
				this.message = message;

				var property = PrintJobManager.GetType().GetProperty("CurrentProcessingJob");
				property.GetSetMethod(true).Invoke(PrintJobManager, new object[] { printJob });
			}

			readonly TraceEventType eventType;
			readonly string message;

			public override void RunTask(CancellationToken token)
			{
				Log(eventType, message);
				base.RunTask(token);
			}
		}
	}
}
