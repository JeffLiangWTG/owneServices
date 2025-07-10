using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.AutoDeploy.Test
{
	[TestedType(typeof(ScheduledUpgradeServiceTask))]
	public class UpgradeScheduleServiceTaskTest : ServiceTaskTestCase<ScheduledUpgradeServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			ErrorReporter.Clear();
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testProcess = new UpgradeScheduleServiceTaskAnyBranchTestHelper(Factory);
				var logger = new TestServiceLogger();
				testProcess.ServiceLogger = logger;

				AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
				using (Env.Instance.UserContextManagerForTesting.SetEmptyThreadUserContextSecurityForTest())
				using (Env.Instance.TemporaryServiceTaskContext(testProcess.GetType().Name, canRunInAnyBranch: true))
				{
					AssertExceptionThrown(typeof(ApplicationException), () => testProcess.RunTask());
				}
				AssertEquals("Exception Report Count Should Be One ", 1, ErrorReporter.TotalErrorCount);
				Assert(ErrorReporter.LastExceptionReported is BranchAccessedWithoutConfiguredEnvironmentException);
				ErrorReporter.Clear();

				var branch = GetBranchForEDIServiceTasksTest();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				using (Env.Instance.TemporaryServiceTaskContext(testProcess.GetType().Name, canRunInAnyBranch: true))
				{
					AssertNoExceptionThrown(() => testProcess.RunTask());
				}
			}
		}

		GlbBranch GetBranchForEDIServiceTasksTest()
		{
			return Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_IsActive, true));
		}

		public void TestExecute()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testBuildPackagePath = resourceRetriever.SaveResourceToFile("ZClientEDI.Test.DummyUpgradePackageCS.zip");
				UpgradesToClient upgrade1 = GetTestUpgrade(testBuildPackagePath);
				UpgradesToClient upgrade2 = GetTestUpgrade(testBuildPackagePath);
				upgrade2.LicDatabase.LD_ServerCode = "SV2";
				ReleaseBuild build = Factory.New<ReleaseBuild>();
				build.HL_ExeVersionDate = ZDateTime.Now.AddDays(-1);
				build.HL_PackagePath = testBuildPackagePath;
				UpgradesToClient upgrade3 = GetTestUpgrade(testBuildPackagePath);
				upgrade3.LicDatabase.LD_ServerCode = "SV3";
				upgrade3.L1_HL = build.PK;
				UpgradesToClient upgrade4 = GetTestUpgrade(testBuildPackagePath);
				upgrade4.LicDatabase.LD_ServerCode = "SV4";
				upgrade4.L1_HL = build.PK;
				ReleaseBuild buildEx = Factory.New<ReleaseBuild>();
				buildEx.HL_ExeVersionDate = ZDateTime.Now.AddDays(-2);
				buildEx.HL_PackagePath = testBuildPackagePath;
				UpgradesToClient upgradeEx = GetTestUpgrade(testBuildPackagePath);
				upgradeEx.LicDatabase.LD_ServerCode = "EXC";
				upgradeEx.L1_HL = buildEx.PK;
				Factory.Save();
				var testProcess = new UpgradeScheduleServiceTaskTestHelper();
				var logger = new TestServiceLogger();
				testProcess.ServiceLogger = logger;
				testProcess.ReturnSenderForUpgrades = new ZGuid[] { upgrade1.PK, upgrade2.PK, upgrade3.PK, upgradeEx.PK };
				testProcess.ThrowExceptionForUpgrades = new ZGuid[] { upgradeEx.PK };
				ErrorReporter.Clear();
				testProcess.RunTask();
				upgrade1.Reload();
				AssertEquals("Processed", UpgradesToClientStatus.Codes.Processed, upgrade1.L1_CurrentStatus);
				upgrade2.Reload();
				AssertEquals("Processed", UpgradesToClientStatus.Codes.Processed, upgrade2.L1_CurrentStatus);
				upgrade3.Reload();
				AssertEquals("Failed", UpgradesToClientStatus.Codes.Failed, upgrade3.L1_CurrentStatus);
				upgrade4.Reload();
				AssertEquals("Failed", UpgradesToClientStatus.Codes.Failed, upgrade4.L1_CurrentStatus);
				upgradeEx.Reload();
				AssertEquals("Failed", UpgradesToClientStatus.Codes.Failed, upgradeEx.L1_CurrentStatus);
				AssertEquals("Logger Lines Count", 9, logger.Count);
				Assert("Log Processing", logger[0].IndexOf("Processing scheduled upgrades ...") > -1);
				Assert("Log Processed", logger[1].IndexOf("was sucessfully processed") > -1);
				Assert("Log Failed with Exception", logger[3].IndexOf("failed with exception") > -1);
				Assert("Log Exception Details", logger[4].IndexOf("Exception Details : System.InvalidOperationException: Exception thrown") > -1);
				Assert("Log Failed because it unable to get Sender", logger[5].IndexOf("failed because it was unable to get an upgrade sender.") > -1);
				Assert("Log Processed", logger[7].IndexOf("Processed 2, failed 3.") > -1);
				Assert("Log Total Processed", logger[8].IndexOf("Total processed 2, failed 3.") > -1);
				AssertEquals(typeof(InvalidOperationException), ErrorReporter.LastExceptionReported.GetType());
				AssertEquals("Processing Upgrade Exception", ErrorReporter.LastKeyReported);
				AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestExecute_PackageSenderRunResult()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testBuildPackagePath = resourceRetriever.SaveResourceToFile("ZClientEDI.Test.DummyUpgradePackageCS.zip");
				UpgradesToClient upgrade1 = GetTestUpgrade(testBuildPackagePath);
				UpgradesToClient upgrade2 = GetTestUpgrade(testBuildPackagePath);
				upgrade2.LicDatabase.LD_ServerCode = "SV2";
				ReleaseBuild build = Factory.New<ReleaseBuild>();
				build.HL_ExeVersionDate = ZDateTime.Now.AddDays(-1);
				build.HL_PackagePath = testBuildPackagePath;
				UpgradesToClient upgrade3 = GetTestUpgrade(testBuildPackagePath);
				upgrade3.LicDatabase.LD_ServerCode = "SV3";
				upgrade3.L1_HL = build.PK;
				UpgradesToClient upgrade4 = GetTestUpgrade(testBuildPackagePath);
				upgrade4.LicDatabase.LD_ServerCode = "SV4";
				upgrade4.L1_HL = build.PK;
				ReleaseBuild buildEx = Factory.New<ReleaseBuild>();
				buildEx.HL_ExeVersionDate = ZDateTime.Now.AddDays(-2);
				buildEx.HL_PackagePath = testBuildPackagePath;
				UpgradesToClient upgradeEx = GetTestUpgrade(testBuildPackagePath);
				upgradeEx.LicDatabase.LD_ServerCode = "EXC";
				upgradeEx.L1_HL = buildEx.PK;
				Factory.Save();
				var testProcess = new UpgradeScheduleServiceTaskWithPackageSenderRunResultForTest();
				testProcess.PackageSenderRunResult = false;
				var logger = new TestServiceLogger();
				testProcess.ServiceLogger = logger;
				ErrorReporter.Clear();
				testProcess.RunTask();
				upgrade1.Reload();
				AssertEquals("Failed", UpgradesToClientStatus.Codes.Failed, upgrade1.L1_CurrentStatus);
				upgrade2.Reload();
				AssertEquals("Failed", UpgradesToClientStatus.Codes.Failed, upgrade2.L1_CurrentStatus);
				upgrade3.Reload();
				AssertEquals("Failed", UpgradesToClientStatus.Codes.Failed, upgrade3.L1_CurrentStatus);
				upgrade4.Reload();
				AssertEquals("Failed", UpgradesToClientStatus.Codes.Failed, upgrade4.L1_CurrentStatus);
				upgradeEx.Reload();
				AssertEquals("Failed", UpgradesToClientStatus.Codes.Failed, upgradeEx.L1_CurrentStatus);

				var logs = logger.ToString();
				AssertContains("Information|Total processed 0, failed 5.", logs);
				AssertContains("Error|Processing upgrade for the client TGBLOG, server SV4 failed using Blocked upgrade method.", logs);
			}
		}

		public void TestDeleteFileCreatedByPackageBuilder()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testBuildPackagePath = resourceRetriever.SaveResourceToFile("ZClientEDI.Test.DummyUpgradePackageCS.zip");
				UpgradesToClient upgrade = GetTestUpgrade(testBuildPackagePath);
				ReleaseBuild build = Factory.New<ReleaseBuild>();
				build.HL_ExeVersionDate = ZDateTime.Now.AddDays(-1);
				build.HL_PackagePath = testBuildPackagePath;
				upgrade.L1_HL = build.PK;
				Factory.Save();
				var testProcess = new UpgradeScheduleServiceTaskTestHelper();
				var logger = new TestServiceLogger();
				testProcess.ServiceLogger = logger;
				testProcess.ReturnSenderForUpgrades = new ZGuid[] { upgrade.PK };
				testProcess.ResultPackagePath = Path.Combine(Env.TempPath, "DummyPackage.edp");
				using (File.Create(testProcess.ResultPackagePath))
				{
				}

				try
				{
					Assert("File Exists", File.Exists(testProcess.ResultPackagePath));
					testProcess.RunTask();
					AssertEquals("Old File does not exists", false, File.Exists(testProcess.ResultPackagePath));
				}
				finally
				{
					if (File.Exists(testProcess.ResultPackagePath))
					{
						File.Delete(testProcess.ResultPackagePath);
					}
				}
			}
		}

		public void TestDatabaseWithMultipleEnterprises()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testBuildPackagePath = resourceRetriever.SaveResourceToFile("ZClientEDI.Test.DummyUpgradePackageCS.zip");
				UpgradesToClient upgrade1 = GetTestUpgrade(testBuildPackagePath);
				LicenceEnterprise enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
				enterprise2.LE_EnterpriseCode = "EN2";
				upgrade1.LicDatabase.LD_LE = enterprise2.PK;
				Factory.Save();
				var testProcess = new UpgradeScheduleServiceTaskTestHelper();
				var logger = new TestServiceLogger();
				testProcess.ServiceLogger = logger;
				testProcess.RunTask();
				upgrade1.Reload();
				AssertEquals("Failed", UpgradesToClientStatus.Codes.Failed, upgrade1.L1_CurrentStatus);
				AssertEquals("Logger Lines Count", 4, logger.Count);
				Assert("Log Processing", logger[0].IndexOf("Processing scheduled upgrades ...") > -1);
				Assert("Log Failed", logger[1].IndexOf("failed because the database has multiple enterprises") > -1);
				Assert("Log Processed", logger[2].IndexOf("Processed 0, failed 1.") > -1);
				Assert("Log Total Processed", logger[3].IndexOf("Total processed 0, failed 1.") > -1);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		#region Implementation
		#region Test Helper Classes
		class PackageSenderTestHelper : HttpPackageSender
		{
			public PackageSenderTestHelper(string licenceEnterpriseCode, string packagePath, params UpgradesToClient[] upgradeRequests) : base(licenceEnterpriseCode, packagePath, upgradeRequests)
			{
			}

			public override bool Run()
			{
				if (ThrowException)
				{
					throw new InvalidOperationException("Exception thrown");
				}

				return RunResult;
			}

			public bool ThrowException;
			public bool RunResult { get; set; } = true;
		}

		class PackageSenderTestAnyBranchHelper : HttpPackageSender
		{
			public string PackagePath;
			public string LicenceEnterpriseCode;

			public PackageSenderTestAnyBranchHelper(string licenceEnterpriseCode, string packagePath, params UpgradesToClient[] upgradeRequests) : base(licenceEnterpriseCode, packagePath, upgradeRequests)
			{
				this.PackagePath = packagePath;
				this.LicenceEnterpriseCode = licenceEnterpriseCode;
			}
		}

		class RuntimePackageBuilderTestHelper : RuntimePackageBuilder
		{
			public RuntimePackageBuilderTestHelper(ReleaseBuild build, string targetPath) : base(build, targetPath)
			{
			}

			protected override void BuildFromMasterPackageBlob(string enterpriseCode, bool isHostedOnWiseCloud)
			{
			}
		}

		protected class UpgradeScheduleServiceTaskTestHelper : ScheduledUpgradeServiceTask
		{
			protected override HttpPackageSender GetSender(string packagePath, UpgradesToClient[] upgradeRequests)
			{
				IList upgrades = ReturnSenderForUpgrades;
				if (upgrades == null)
				{
					return null;
				}
				else
				{
					foreach (UpgradesToClient upgrade in upgradeRequests)
					{
						if (!upgrades.Contains(upgrade.PK))
						{
							return null;
						}
					}

					PackageSenderTestHelper sender = new PackageSenderTestHelper(upgradeRequests[0].ClientSpecificCode, packagePath, upgradeRequests);
					IList upgradesWithException = ThrowExceptionForUpgrades;
					foreach (UpgradesToClient upgrade in upgradeRequests)
					{
						if (upgradesWithException != null && upgradesWithException.Contains(upgrade.PK))
						{
							sender.ThrowException = true;
						}
					}

					return sender;
				}
			}

			protected override RuntimePackageBuilder GetPackageBuilder(ReleaseBuild build, string targetDirectory)
			{
				return new RuntimePackageBuilderTestHelper(build, targetDirectory);
			}

			protected override string GetResultPackagePath(RuntimePackageBuilder builder)
			{
				return ResultPackagePath;
			}

			public ZGuid[] ReturnSenderForUpgrades = Array.Empty<ZGuid>();
			public ZGuid[] ThrowExceptionForUpgrades = Array.Empty<ZGuid>();
			public string ResultPackagePath = "";
		}

		protected class UpgradeScheduleServiceTaskAnyBranchTestHelper : ScheduledUpgradeServiceTask
		{
			public BusinessObjectFactory Factory { get; set; }

			public UpgradeScheduleServiceTaskAnyBranchTestHelper(BusinessObjectFactory businessObjectFactory) : base()
			{
				this.Factory = businessObjectFactory;
			}
			protected override HttpPackageSender GetSender(string packagePath, UpgradesToClient[] upgradeRequests)
			{
				var sender = new PackageSenderTestAnyBranchHelper(upgradeRequests[0].ClientSpecificCode, packagePath, upgradeRequests);
				return sender;
			}

			public override void RunTask(CancellationToken token)
			{
				var xml = @"<UpgradeDownload>
  <ExeVersionDate>24-OCT-05 00:00</ExeVersionDate>
  <MajorVersion>1</MajorVersion>
  <MinorVersion>1</MinorVersion>
  <Release>2123</Release>
  <Patch>15568</Patch>
  <Comment>Deployed via Web</Comment>
  <ForceDownload>Y</ForceDownload>
  <PackageURL>http://www.cargowise.com/ftpmirror/ediEnterprise/Generic/Package20051024_000000_1_1_2123_15568.edp</PackageURL>
</UpgradeDownload>";
				var messageCreator = ObjectFactory.Get<IOutgoingSystemMessage>();
				messageCreator.Create(Factory, xml, "");
			}
		}

		protected class UpgradeScheduleServiceTaskWithPackageSenderRunResultForTest : ScheduledUpgradeServiceTask
		{
			protected override HttpPackageSender GetSender(string packagePath, UpgradesToClient[] upgradeRequests)
			{
				var sender = new PackageSenderTestHelper(upgradeRequests[0].ClientSpecificCode, packagePath, upgradeRequests);
				sender.RunResult = PackageSenderRunResult;
				return sender;
			}

			protected override RuntimePackageBuilder GetPackageBuilder(ReleaseBuild build, string targetDirectory)
			{
				return new RuntimePackageBuilderTestHelper(build, targetDirectory);
			}

			public bool PackageSenderRunResult { get; set; } = true;
		}

		#endregion
		protected UpgradesToClient GetTestUpgrade(string testBuildPackagePath)
		{
			LicenceDatabase database = HeaderForTest.LicCompany.LicDatabases.AddNew();
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_PackagePath = testBuildPackagePath;
			UpgradesToClient testUpgrade = Factory.New<UpgradesToClient>();
			testUpgrade.L1_HL = build.PK;
			testUpgrade.L1_LD = database.PK;
			testUpgrade.L1_OC = HeaderForTest.Contacts[0].PK;
			testUpgrade.L1_RequestedDateTime = ZDateTime.Now;
			testUpgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Queued;
			return testUpgrade;
		}

		EDIOrgHeader HeaderForTest
		{
			get
			{
				if (fHeaderForTest == null)
				{
					fHeaderForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
					fHeaderForTest.OH_RL_NKClosestPort = "AUBNE";
					fHeaderForTest.OH_FullName = "My Organisation";
					fHeaderForTest.OH_Code = "TGBLOG";
					fHeaderForTest.CreateAndLoadLicenceForOrg();
					fHeaderForTest.GenerateNewLicenceCode();
					OrgContact contact = fHeaderForTest.Contacts.AddNew();
				}

				return fHeaderForTest;
			}
		}

		EDIOrgHeader fHeaderForTest;
		#endregion
	}
}
