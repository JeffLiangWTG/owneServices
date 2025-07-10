using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Integration;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.ServiceTask.Test
{
	internal class ReleaseBuildTesterTest : TestCaseWithFactory
	{
		public void TestRunDeployment()
		{
			var build = GetBuildForTest();

			EDIDataRegistry.Instance.ReleaseBuildTestDbServerName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");
			EDIDataRegistry.Instance.ReleaseBuildTestDbName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MyAccountUat");
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppHost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");

			var logger = new LoggerForTest();
			var tester = new ReleaseBuildTesterForTest(build, logger);

			tester.BuildSchemaVersionOverride = "5100.0";
			tester.BuildScriptVersionOverride = "4100.0";
			tester.BuildDataVersionOverride = "3100.0";
			tester.BuildTransformationVersionOverride = "2100.0";

			tester.DbSchemaVersionOverride = "5000.0";
			tester.DbScriptVersionOverride = "4000.0";
			tester.DbDataVersionOverride = "3000.0";
			tester.DbTransformationVersionOverride = "2000.0";

			var result = tester.RunDeployment();

			var expectedLogs =
@"Upgrade configuration - server: sydsp-xrm-1 | databse: MyAccountUat | target host: sydsp-xrm-1
Checked build 21.3.5.12 versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5000.0 | Script Version 4000.0 | Data Version 3000.0 | Transformation Version 2000.0
Starting upgrade
Checked build version number 21.3.5.12
Checked database current version number 0.0.0.0
Starting uploading build to sydsp-xrm-1 MyAccountUat
Build upload completed
Starting remote upgrade command TestCommand.exe -Params
Remote upgrade command completed
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Upgrade completed - database sydsp-xrm-1 MyAccountUat versions match build 21.3.5.12 versions
";
			CombineAssertions(() =>
			{
				AssertEquals(true, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		public void TestRunDeployment_TestDatabaseNotConfigured()
		{
			var build = GetBuildForTest();
			var logger = new LoggerForTest();
			var tester = new ReleaseBuildTesterForTest(build, logger);
			var result = tester.RunDeployment();

			var expectedLogs =
@"Upgrade configuration - server:  | databse:  | target host: 
Invalid upgrade configuration
";
			CombineAssertions(() =>
			{
				AssertEquals(false, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		public void TestRunDeployment_UpgradeNotNeeded()
		{
			var build = GetBuildForTest();

			EDIDataRegistry.Instance.ReleaseBuildTestDbServerName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");
			EDIDataRegistry.Instance.ReleaseBuildTestDbName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MyAccountUat");
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppHost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");

			var logger = new LoggerForTest();
			var tester = new ReleaseBuildTesterForTest(build, logger);

			tester.BuildSchemaVersionOverride = "5100.0";
			tester.BuildScriptVersionOverride = "4100.0";
			tester.BuildDataVersionOverride = "3100.0";
			tester.BuildTransformationVersionOverride = "2100.0";

			tester.DbSchemaVersionOverride = "5100.0";
			tester.DbScriptVersionOverride = "4100.0";
			tester.DbDataVersionOverride = "3100.0";
			tester.DbTransformationVersionOverride = "2100.0";

			var result = tester.RunDeployment();

			var expectedLogs =
@"Upgrade configuration - server: sydsp-xrm-1 | databse: MyAccountUat | target host: sydsp-xrm-1
Checked build 21.3.5.12 versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Upgrade is not required - database sydsp-xrm-1 MyAccountUat versions match build 21.3.5.12 versions
";
			CombineAssertions(() =>
			{
				AssertEquals(true, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		public void TestRunDeployment_FailToUploadBuild()
		{
			var build = GetBuildForTest();

			EDIDataRegistry.Instance.ReleaseBuildTestDbServerName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");
			EDIDataRegistry.Instance.ReleaseBuildTestDbName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MyAccountUat");
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppHost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");

			var logger = new LoggerForTest();
			var tester = new ReleaseBuildTesterForTest(build, logger);

			tester.BuildSchemaVersionOverride = "5100.0";
			tester.BuildScriptVersionOverride = "4100.0";
			tester.BuildDataVersionOverride = "3100.0";
			tester.BuildTransformationVersionOverride = "2100.0";

			tester.DbSchemaVersionOverride = "5000.0";
			tester.DbScriptVersionOverride = "4000.0";
			tester.DbDataVersionOverride = "3000.0";
			tester.DbTransformationVersionOverride = "2000.0";

			tester.TriggerBuildUploadFailure = true;

			var result = tester.RunDeployment();

			var expectedLogs =
@"Upgrade configuration - server: sydsp-xrm-1 | databse: MyAccountUat | target host: sydsp-xrm-1
Checked build 21.3.5.12 versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5000.0 | Script Version 4000.0 | Data Version 3000.0 | Transformation Version 2000.0
Starting upgrade
Checked build version number 21.3.5.12
Checked database current version number 0.0.0.0
Starting uploading build to sydsp-xrm-1 MyAccountUat
Build upload failed
";
			CombineAssertions(() =>
			{
				AssertEquals(false, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		public void TestRunDeployment_BuildAlreadyUploaded()
		{
			var build = GetBuildForTest();

			EDIDataRegistry.Instance.ReleaseBuildTestDbServerName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");
			EDIDataRegistry.Instance.ReleaseBuildTestDbName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MyAccountUat");
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppHost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");

			var logger = new LoggerForTest();
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
			tester.DbVersionPatchOverride = build.HL_Patch;

			var result = tester.RunDeployment();

			var expectedLogs =
@"Upgrade configuration - server: sydsp-xrm-1 | databse: MyAccountUat | target host: sydsp-xrm-1
Checked build 21.3.5.12 versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5000.0 | Script Version 4000.0 | Data Version 3000.0 | Transformation Version 2000.0
Starting upgrade
Checked build version number 21.3.5.12
Checked database current version number 21.3.5.12
Database sydsp-xrm-1 MyAccountUat has current build uploaded
Starting remote upgrade command TestCommand.exe -Params
Remote upgrade command completed
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Upgrade completed - database sydsp-xrm-1 MyAccountUat versions match build 21.3.5.12 versions
";
			CombineAssertions(() =>
			{
				AssertEquals(true, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		public void TestRunDeployment_HasMoreRecentBuild()
		{
			var build = GetBuildForTest();

			EDIDataRegistry.Instance.ReleaseBuildTestDbServerName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");
			EDIDataRegistry.Instance.ReleaseBuildTestDbName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MyAccountUat");
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppHost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");

			var logger = new LoggerForTest();
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
			tester.DbVersionPatchOverride = build.HL_Patch + 1;

			var result = tester.RunDeployment();

			var expectedLogs =
@"Upgrade configuration - server: sydsp-xrm-1 | databse: MyAccountUat | target host: sydsp-xrm-1
Checked build 21.3.5.12 versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5000.0 | Script Version 4000.0 | Data Version 3000.0 | Transformation Version 2000.0
Starting upgrade
Checked build version number 21.3.5.12
Checked database current version number 21.3.5.13
Database sydsp-xrm-1 MyAccountUat has more recent build uploaded
";
			CombineAssertions(() =>
			{
				AssertEquals(false, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		public void TestRunDeployment_FailToRunUpgradeCommand()
		{
			var build = GetBuildForTest();

			EDIDataRegistry.Instance.ReleaseBuildTestDbServerName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");
			EDIDataRegistry.Instance.ReleaseBuildTestDbName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MyAccountUat");
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppHost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");

			var logger = new LoggerForTest();
			var tester = new ReleaseBuildTesterForTest(build, logger);

			tester.BuildSchemaVersionOverride = "5100.0";
			tester.BuildScriptVersionOverride = "4100.0";
			tester.BuildDataVersionOverride = "3100.0";
			tester.BuildTransformationVersionOverride = "2100.0";

			tester.DbSchemaVersionOverride = "5000.0";
			tester.DbScriptVersionOverride = "4000.0";
			tester.DbDataVersionOverride = "3000.0";
			tester.DbTransformationVersionOverride = "2000.0";

			tester.TriggerUpgradeCommandFailure = true;

			var result = tester.RunDeployment();

			var expectedLogs =
@"Upgrade configuration - server: sydsp-xrm-1 | databse: MyAccountUat | target host: sydsp-xrm-1
Checked build 21.3.5.12 versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5000.0 | Script Version 4000.0 | Data Version 3000.0 | Transformation Version 2000.0
Starting upgrade
Checked build version number 21.3.5.12
Checked database current version number 0.0.0.0
Starting uploading build to sydsp-xrm-1 MyAccountUat
Build upload completed
Starting remote upgrade command TestCommand.exe -Params
Waiting for another run
Starting remote upgrade command TestCommand.exe -Params
Remote upgrade command failed
";
			CombineAssertions(() =>
			{
				AssertEquals(false, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		public void TestRunDeployment_FailToUpgrade()
		{
			var build = GetBuildForTest();

			EDIDataRegistry.Instance.ReleaseBuildTestDbServerName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");
			EDIDataRegistry.Instance.ReleaseBuildTestDbName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MyAccountUat");
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppHost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sydsp-xrm-1");

			var logger = new LoggerForTest();
			var tester = new ReleaseBuildTesterForTest(build, logger);

			tester.BuildSchemaVersionOverride = "5100.0";
			tester.BuildScriptVersionOverride = "4100.0";
			tester.BuildDataVersionOverride = "3100.0";
			tester.BuildTransformationVersionOverride = "2100.0";

			tester.DbSchemaVersionOverride = "5000.0";
			tester.DbScriptVersionOverride = "4000.0";
			tester.DbDataVersionOverride = "3000.0";
			tester.DbTransformationVersionOverride = "2000.0";

			tester.TriggerRemoteUpgradeFailure = true;

			var result = tester.RunDeployment();

			var expectedLogs =
@"Upgrade configuration - server: sydsp-xrm-1 | databse: MyAccountUat | target host: sydsp-xrm-1
Checked build 21.3.5.12 versions: Schema Version 5100.0 | Script Version 4100.0 | Data Version 3100.0 | Transformation Version 2100.0
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5000.0 | Script Version 4000.0 | Data Version 3000.0 | Transformation Version 2000.0
Starting upgrade
Checked build version number 21.3.5.12
Checked database current version number 0.0.0.0
Starting uploading build to sydsp-xrm-1 MyAccountUat
Build upload completed
Starting remote upgrade command TestCommand.exe -Params
Remote upgrade command completed
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5000.0 | Script Version 4000.0 | Data Version 3000.0 | Transformation Version 2000.0
Upgrade not complete yet - database sydsp-xrm-1 MyAccountUat versions don't match build 21.3.5.12
Waiting for next check in 1 seconds
Checked database sydsp-xrm-1 MyAccountUat versions: Schema Version 5000.0 | Script Version 4000.0 | Data Version 3000.0 | Transformation Version 2000.0
Upgrade not complete yet - database sydsp-xrm-1 MyAccountUat versions don't match build 21.3.5.12
Waiting for next check in 1 seconds
Upgrade not complete - database sydsp-xrm-1 MyAccountUat versions don't match build 21.3.5.12
";
			CombineAssertions(() =>
			{
				AssertEquals(false, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		public void TestRunTest()
		{
			var build = GetBuildForTest();
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://sydsp-xrm-1/myaccount-uat/wtg/status");

			var logger = new LoggerForTest();
			var tester = new ReleaseBuildTesterForTest(build, logger);
			var result = tester.RunTest();

			var expectedLogs =
@"Starting web app health check on https://sydsp-xrm-1/myaccount-uat/wtg/status
Web app is up and running
";
			CombineAssertions(() =>
			{
				AssertEquals(true, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		public void TestRunTest_WebAppUrlNotConfigured()
		{
			var build = GetBuildForTest();

			var logger = new LoggerForTest();
			var tester = new ReleaseBuildTesterForTest(build, logger);
			var result = tester.RunTest();
			var expectedLogs =
@"Web app health check URL is blank
Web app health check failed
";

			CombineAssertions(() =>
			{
				AssertEquals(false, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		public void TestRunTest_UnexpectedStatusCode()
		{
			var build = GetBuildForTest();
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://sydsp-xrm-1/myaccount-uat/wtg/status");

			var logger = new LoggerForTest();
			var tester = new ReleaseBuildTesterForTest(build, logger) { WebAppHealthCheckStatusCodeOverride = System.Net.HttpStatusCode.RequestTimeout };
			var result = tester.RunTest();

			var expectedLogs =
@"Starting web app health check on https://sydsp-xrm-1/myaccount-uat/wtg/status
Web app is down with status RequestTimeout
Waiting for next check in 1 seconds
Web app is down with status RequestTimeout
Web app health check failed
";

			CombineAssertions(() =>
			{
				AssertEquals(false, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		public void TestRunTest_UnexpectedContent()
		{
			var build = GetBuildForTest();
			EDIDataRegistry.Instance.ReleaseBuildTestWebAppUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://sydsp-xrm-1/myaccount-uat/wtg/status");

			var logger = new LoggerForTest();
			var tester = new ReleaseBuildTesterForTest(build, logger) { WebAppHealthCheckContentOverride = EDIConstants.DatabaseConnectionStatus.VersionMismatch };
			var result = tester.RunTest();
			var expectedLogs =
@"Starting web app health check on https://sydsp-xrm-1/myaccount-uat/wtg/status
Web app is up with issue - ERROR(Database): Database version mismatch
Waiting for next check in 1 seconds
Web app is up with issue - ERROR(Database): Database version mismatch
Web app health check failed
";

			CombineAssertions(() =>
			{
				AssertEquals(false, result);
				AssertEquals(expectedLogs, GetFullLogText(logger));
			});
		}

		static string GetFullLogText(LoggerForTest logger)
		{
			var builder = new ZStringBuilder();
			foreach (var entry in logger.LogEntries)
			{
				builder.AppendLine(entry);
			}
			return builder.ToString();
		}

		ReleaseBuild GetBuildForTest()
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

			return build;
		}
	}
}
