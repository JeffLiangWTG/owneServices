using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.ServiceTask.Test
{
	[TestedType(typeof(ReleaseBuildTestServiceTask))]
	internal class ReleaseBuildTestServiceTaskTest : ServiceTaskTestCase<ReleaseBuildTestServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var serviceTask = new ReleaseBuildTestServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		[TestDate(2020, 2, 18, 16, 0, 0)]
		public void TestRunTask()
		{
			var build = Factory.New<ReleaseBuild>();
			build.HL_ReleaseStatus = ReleaseRings.Codes.ALP;
			build.HL_IsTestPassed = false;
			build.HL_TestDateUtc = ZDateTime.Empty;
			build.HL_MajorVersion = 21;
			build.HL_MinorVersion = 3;
			build.HL_Release = 5;
			build.HL_Patch = 12;

			var staff1 = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff1.GS_Code = "SO1";
			staff1.GS_FullName = "Staff One";
			staff1.GS_EmailAddress = "newemail1@wisetechglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);
			EDIDataRegistry.Instance.ReleaseBuildTestResultNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Factory.Save();

			var logger = new LoggerForTest();
			var task = new ReleaseBuildTestServiceTaskForTest();
			task.ServiceLogger = logger;
			task.RunTask();

			var expectedLogs =
@"Upgrade configuration - server: sydsp-xrm-1 | databse: MyAccountUat | target host: sydsp-xrm-1
Checked build 21.3.5.12 versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5000.0 | Script Version 4000.0 | Data Version 3000.0 | Transformation Version 2000.0
Starting upgrade
Checked build version number 21.3.5.12
Checked database current version number 21.3.5.11
Starting uploading build to sydsp-xrm-1 MyAccountUat
Build upload completed
Starting remote upgrade command TestCommand.exe -Params
Remote upgrade command completed
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Upgrade completed - database sydsp-xrm-1 MyAccountUat versions match build 21.3.5.12 versions
Starting web app health check on https://sydsp-xrm-1/myaccount-uat/wtg/status
Web app is up and running
";
			AssertEquals(expectedLogs, GetFullLogText(logger));

			build.Reload();
			AssertEquals(true, build.HL_IsTestPassed);
			AssertEquals(new ZDateTime(2020, 2, 18, 16, 0, 0), build.HL_TestDateUtc);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Release Build 21.3.5.12 Test Passed", email.Subject);
			AssertEquals("You are receiving this email because you are a member of the group in registry WiseTech Global Client Extensions/Release Builds & Upgrades/Testing > Release Build Test Result Notification Group",
				email.Body);
		}

		public void TestRunTask_NoRequiredBuilds()
		{
			var build = Factory.New<ReleaseBuild>();
			build.HL_ReleaseStatus = ReleaseRings.Codes.GP1;
			build.HL_IsTestPassed = false;
			build.HL_TestDateUtc = ZDateTime.Empty;
			build.HL_MajorVersion = 21;
			build.HL_MinorVersion = 3;
			build.HL_Release = 5;
			build.HL_Patch = 12;
			Factory.Save();

			var logger = new LoggerForTest();
			var task = new ReleaseBuildTestServiceTaskForTest();
			task.ServiceLogger = logger;
			task.RunTask();
			AssertEquals(string.Empty, GetFullLogText(logger));

			build.Reload();
			AssertEquals(false, build.HL_IsTestPassed);
			AssertEquals(ZDateTime.Empty, build.HL_TestDateUtc);
		}

		[TestDate(2020, 2, 18, 16, 0, 0)]
		public void TestRunTask_BuildHasBeenTested()
		{
			var build = Factory.New<ReleaseBuild>();
			build.HL_ReleaseStatus = ReleaseRings.Codes.ALP;
			build.HL_IsTestPassed = false;
			build.HL_TestDateUtc = new ZDateTime(2020, 1, 1);
			build.HL_MajorVersion = 21;
			build.HL_MinorVersion = 3;
			build.HL_Release = 5;
			build.HL_Patch = 12;
			Factory.Save();

			var logger = new LoggerForTest();
			var task = new ReleaseBuildTestServiceTaskForTest();
			task.ServiceLogger = logger;
			task.RunTask();
			AssertEquals(string.Empty, GetFullLogText(logger));

			build.Reload();
			AssertEquals(false, build.HL_IsTestPassed);
			AssertEquals(new ZDateTime(2020, 1, 1), build.HL_TestDateUtc);
		}

		[TestDate(2020, 2, 18, 16, 0, 0)]
		public void TestRunTask_MultipleBuilds()
		{
			var build1 = Factory.New<ReleaseBuild>();
			build1.HL_ReleaseStatus = ReleaseRings.Codes.ALP;
			build1.HL_IsTestPassed = false;
			build1.HL_TestDateUtc = ZDateTime.Empty;
			build1.HL_MajorVersion = 21;
			build1.HL_MinorVersion = 3;
			build1.HL_Release = 5;
			build1.HL_Patch = 12;

			var build2 = Factory.New<ReleaseBuild>();
			build2.HL_ReleaseStatus = ReleaseRings.Codes.ALP;
			build2.HL_IsTestPassed = false;
			build2.HL_TestDateUtc = ZDateTime.Empty;
			build2.HL_MajorVersion = 21;
			build2.HL_MinorVersion = 3;
			build2.HL_Release = 1;
			build2.HL_Patch = 29;

			var build3 = Factory.New<ReleaseBuild>();
			build3.HL_ReleaseStatus = ReleaseRings.Codes.ALP;
			build3.HL_IsTestPassed = false;
			build3.HL_TestDateUtc = ZDateTime.Empty;
			build3.HL_Superceded = true;
			build3.HL_MajorVersion = 21;
			build3.HL_MinorVersion = 3;
			build3.HL_Release = 6;
			build3.HL_Patch = 8;

			Factory.Save();

			var logger = new LoggerForTest();
			var task = new ReleaseBuildTestServiceTaskForTest();
			task.ServiceLogger = logger;
			task.RunTask();

			var expectedLogs =
@"Upgrade configuration - server: sydsp-xrm-1 | databse: MyAccountUat | target host: sydsp-xrm-1
Checked build 21.3.1.29 versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5000.0 | Script Version 4000.0 | Data Version 3000.0 | Transformation Version 2000.0
Starting upgrade
Checked build version number 21.3.1.29
Checked database current version number 21.3.1.28
Starting uploading build to sydsp-xrm-1 MyAccountUat
Build upload completed
Starting remote upgrade command TestCommand.exe -Params
Remote upgrade command completed
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Upgrade completed - database sydsp-xrm-1 MyAccountUat versions match build 21.3.1.29 versions
Starting web app health check on https://sydsp-xrm-1/myaccount-uat/wtg/status
Web app is up and running
";
			AssertEquals(expectedLogs, GetFullLogText(logger));

			build1.Reload();
			AssertEquals(false, build1.HL_IsTestPassed);

			build2.Reload();
			AssertEquals(true, build2.HL_IsTestPassed);
			AssertEquals(new ZDateTime(2020, 2, 18, 16, 0, 0), build2.HL_TestDateUtc);

			logger.ClearLog();
			task.RunTask();
			expectedLogs =
@"Upgrade configuration - server: sydsp-xrm-1 | databse: MyAccountUat | target host: sydsp-xrm-1
Checked build 21.3.5.12 versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5000.0 | Script Version 4000.0 | Data Version 3000.0 | Transformation Version 2000.0
Starting upgrade
Checked build version number 21.3.5.12
Checked database current version number 21.3.5.11
Starting uploading build to sydsp-xrm-1 MyAccountUat
Build upload completed
Starting remote upgrade command TestCommand.exe -Params
Remote upgrade command completed
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Upgrade completed - database sydsp-xrm-1 MyAccountUat versions match build 21.3.5.12 versions
Starting web app health check on https://sydsp-xrm-1/myaccount-uat/wtg/status
Web app is up and running
";
			AssertEquals(expectedLogs, GetFullLogText(logger));

			build1.Reload();
			AssertEquals(true, build1.HL_IsTestPassed);
			AssertEquals(new ZDateTime(2020, 2, 18, 16, 0, 0), build1.HL_TestDateUtc);

			build3.Reload();
			AssertEquals(false, build3.HL_IsTestPassed);
			AssertEquals(ZDateTime.Empty, build3.HL_TestDateUtc);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		static string GetFullLogText(LoggerForTest logger)
		{
			var builder = new ZStringBuilder();
			foreach (var entry in logger.LogEntries)
			{
				builder.AppendLine(entry);
			}
			return builder.ToString();
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			EDIDataRegistry.Instance.ReleaseBuildTestDbServerName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");
			EDIDataRegistry.Instance.ReleaseBuildTestDbName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MyAccountUat");
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppHost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://sydsp-xrm-1/myaccount-uat/wtg/status");
		}

		class ReleaseBuildTestServiceTaskForTest : ReleaseBuildTestServiceTask
		{
			protected override ReleaseBuildTester GetTester(ReleaseBuild build, ILogger logger, CancellationToken cancellationToken)
			{
				var tester = new ReleaseBuildTesterForTest(build, logger);

				tester.BuildSchemaVersionOverride = "5100.0";
				tester.BuildScriptVersionOverride = "4100.0";
				tester.BuildDataVersionOverride = "3100.0";
				tester.BuildTransformationVersionOverride = "2100.0";

				tester.DbSchemaVersionOverride = "5000.0";
				tester.DbScriptVersionOverride = "4000.0";
				tester.DbDataVersionOverride = "3000.0";
				tester.DbTransformationVersionOverride = "2000.0";

				tester.DbVersionMajorOverride = build.HL_MajorVersion;
				tester.DbVersionMinorOverride = build.HL_MinorVersion;
				tester.DbVersionReleaseOverride = build.HL_Release;
				tester.DbVersionPatchOverride = build.HL_Patch - 1;

				return tester;
			}
		}
	}
}
