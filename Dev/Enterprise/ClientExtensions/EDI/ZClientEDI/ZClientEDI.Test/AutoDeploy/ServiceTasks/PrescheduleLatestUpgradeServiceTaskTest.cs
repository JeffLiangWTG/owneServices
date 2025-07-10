using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.DevTools.Definitions;

namespace ZClientEDI.Test.AutoDeploy.ServiceTasks
{
	[TestedType(typeof(PreEmptivelyClientSpecificPackageServiceTask))]
	public class PreEmptivelyClientSpecificPackageServiceTaskTest : ServiceTaskTestCase<PreEmptivelyClientSpecificPackageServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => [];

		public void TestLoadDatabase()
		{
			var configString = "<CONFIGSTRING>Y,N,Y,XXXXXXX</CONFIGSTRING>";
			var configStringV2 = "Y,N,Y,XXXXXXX";

			var normalDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			normalDatabase.LD_IsActive = true;
			normalDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			normalDatabase.LD_Product = ProductTypes.Codes.CargoWise;
			normalDatabase.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			normalDatabase.LD_HostedLocation = "SYD";
			normalDatabase.LD_ScheduleStateUPG = configString;
			normalDatabase.LD_NextRunTimeUtcUPG = ZDateTime.UtcNow.AddMinutes(130);

			var normalDatabaseV2 = Factory.NewWithValidTestData<LicenceDatabase>();
			normalDatabaseV2.LD_IsActive = true;
			normalDatabaseV2.LD_LicenceType = DatabaseTypes.Codes.Production;
			normalDatabaseV2.LD_Product = ProductTypes.Codes.CargoWise;
			normalDatabaseV2.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			normalDatabaseV2.LD_HostedLocation = "SYD";
			normalDatabaseV2.LD_ScheduleStateUPG = configStringV2;
			normalDatabaseV2.LD_NextRunTimeUtcUPG = ZDateTime.UtcNow.AddMinutes(130);

			var databaseAfter3Hour = Factory.NewWithValidTestData<LicenceDatabase>();
			databaseAfter3Hour.LD_IsActive = true;
			databaseAfter3Hour.LD_LicenceType = DatabaseTypes.Codes.Production;
			databaseAfter3Hour.LD_Product = ProductTypes.Codes.CargoWise;
			databaseAfter3Hour.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			databaseAfter3Hour.LD_HostedLocation = "SYD";
			databaseAfter3Hour.LD_ScheduleStateUPG = configString;
			databaseAfter3Hour.LD_NextRunTimeUtcUPG = ZDateTime.UtcNow.AddMinutes(180);

			var inActiveDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			inActiveDatabase.LD_IsActive = false;

			var testDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			testDatabase.LD_IsActive = true;
			testDatabase.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase.LD_Product = ProductTypes.Codes.CargoWise;
			testDatabase.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			testDatabase.LD_HostedLocation = "SYD";
			testDatabase.LD_ScheduleStateUPG = configString;
			testDatabase.LD_NextRunTimeUtcUPG = ZDateTime.UtcNow.AddMinutes(130);

			var emptyConfig = Factory.NewWithValidTestData<LicenceDatabase>();
			emptyConfig.LD_IsActive = true;
			emptyConfig.LD_LicenceType = DatabaseTypes.Codes.Test;
			emptyConfig.LD_Product = ProductTypes.Codes.CargoWise;
			emptyConfig.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			emptyConfig.LD_HostedLocation = "SYD";
			emptyConfig.LD_ScheduleStateUPG = string.Empty;
			emptyConfig.LD_NextRunTimeUtcUPG = ZDateTime.UtcNow.AddMinutes(130);

			var ncwDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			ncwDatabase.LD_IsActive = true;
			ncwDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			ncwDatabase.LD_Product = ProductTypes.Codes.CargoWise;
			ncwDatabase.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			ncwDatabase.LD_HostedLocation = "NCW";
			ncwDatabase.LD_ScheduleStateUPG = configString;
			ncwDatabase.LD_NextRunTimeUtcUPG = ZDateTime.UtcNow.AddMinutes(130);

			var nonHtpUpgradeMethodDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			nonHtpUpgradeMethodDatabase.LD_IsActive = true;
			nonHtpUpgradeMethodDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			nonHtpUpgradeMethodDatabase.LD_Product = ProductTypes.Codes.CargoWise;
			nonHtpUpgradeMethodDatabase.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			nonHtpUpgradeMethodDatabase.LD_HostedLocation = "SYD";
			nonHtpUpgradeMethodDatabase.LD_ScheduleStateUPG = configString;
			nonHtpUpgradeMethodDatabase.LD_NextRunTimeUtcUPG = ZDateTime.UtcNow.AddMinutes(130);

			var disableAutoDownloadDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			disableAutoDownloadDatabase.LD_IsActive = true;
			disableAutoDownloadDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			disableAutoDownloadDatabase.LD_Product = ProductTypes.Codes.CargoWise;
			disableAutoDownloadDatabase.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			disableAutoDownloadDatabase.LD_HostedLocation = "SYD";
			disableAutoDownloadDatabase.LD_ScheduleStateUPG = "<CONFIGSTRING>N,N,Y,XXXXXXX</CONFIGSTRING>";
			disableAutoDownloadDatabase.LD_NextRunTimeUtcUPG = ZDateTime.UtcNow.AddMinutes(130);

			var disableAutoDownloadDatabaseV2 = Factory.NewWithValidTestData<LicenceDatabase>();
			disableAutoDownloadDatabaseV2.LD_IsActive = true;
			disableAutoDownloadDatabaseV2.LD_LicenceType = DatabaseTypes.Codes.Production;
			disableAutoDownloadDatabaseV2.LD_Product = ProductTypes.Codes.CargoWise;
			disableAutoDownloadDatabaseV2.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			disableAutoDownloadDatabaseV2.LD_HostedLocation = "SYD";
			disableAutoDownloadDatabaseV2.LD_ScheduleStateUPG = "N,N,Y,XXXXXXX";
			disableAutoDownloadDatabaseV2.LD_NextRunTimeUtcUPG = ZDateTime.UtcNow.AddMinutes(130);

			Factory.Save();

			var task = new PrescheduleLatestUpgradeServiceTaskForTest();

			var database = task.GetDatabasesForTest();
			AssertEquals(2, database.Count());
			AssertNotNull(database.Single(x => x.PK == normalDatabase.PK));
			AssertNotNull(database.Single(x => x.PK == normalDatabaseV2.PK));
		}

		public void TestFindLatestBuildForDatabases()
		{
			var dprLatest = Factory.New<ReleaseBuild>();
			var dprBuild = Factory.New<ReleaseBuild>();
			var dprOld = Factory.New<ReleaseBuild>();

			dprLatest.HL_ExeVersionDate = ZDateTime.Now;
			dprLatest.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			dprLatest.HL_PackagePath = TestBuildPackagePath;
			dprLatest.VersionNumber = new VersionNumber("25.04.27.001");

			dprBuild.HL_ExeVersionDate = ZDateTime.Now.AddMonths(-1);
			dprBuild.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			dprBuild.HL_PackagePath = TestBuildPackagePath;
			dprBuild.VersionNumber = new VersionNumber("25.01.27.001");

			dprOld.HL_ExeVersionDate = ZDateTime.Now.AddMonths(-2);
			dprOld.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			dprOld.HL_PackagePath = TestBuildPackagePath;
			dprOld.VersionNumber = new VersionNumber("20.04.27.001");

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "QWEEE";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ABCCC";

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "QWE";
			enterprise.LE_OH = org1.PK;

			var clientEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			clientEnterprise.LE_EnterpriseCode = "ABC";
			clientEnterprise.LE_OH = org2.PK;

			var normalDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			normalDatabase.LD_IsActive = true;
			normalDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			normalDatabase.LD_Product = ProductTypes.Codes.Enterprise;
			normalDatabase.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			normalDatabase.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			normalDatabase.LD_LE = enterprise.PK;

			var clientDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			clientDatabase.LD_IsActive = true;
			clientDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			clientDatabase.LD_Product = ProductTypes.Codes.Enterprise;
			clientDatabase.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			clientDatabase.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			clientDatabase.LD_LE = clientEnterprise.PK;

			var clientDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			clientDatabase2.LD_IsActive = true;
			clientDatabase2.LD_LicenceType = DatabaseTypes.Codes.Production;
			clientDatabase2.LD_Product = ProductTypes.Codes.Enterprise;
			clientDatabase2.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			clientDatabase2.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			clientDatabase2.LD_LE = clientEnterprise.PK;
			clientDatabase2.LD_HL_CurrentRunningVersion = dprOld.PK;

			Factory.Save();

			var task = new PrescheduleLatestUpgradeServiceTaskForTest();
			var result = task.FindLatestBuildForDatabasesForTest([normalDatabase, clientDatabase, clientDatabase2]);
			AssertEquals(1, result.Count);
			AssertEquals(dprBuild.PK, result.FirstOrDefault().Key.PK);
			AssertEquals(1, result.FirstOrDefault().Value.Count);
			AssertEquals("The result should only contain client specific database", clientDatabase2.PK, result.FirstOrDefault().Value[0].PK);
		}

		public void TestRunTask()
		{
			var dprLatest = Factory.New<ReleaseBuild>();
			var dprBuild = Factory.New<ReleaseBuild>();
			var dprOld = Factory.New<ReleaseBuild>();

			var stdLatest = Factory.New<ReleaseBuild>();
			var stdBuild = Factory.New<ReleaseBuild>();
			var stdOld = Factory.New<ReleaseBuild>();

			#region Release Builds

			dprLatest.HL_ExeVersionDate = ZDateTime.Now;
			dprLatest.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			dprLatest.HL_PackagePath = TestBuildPackagePath;
			dprLatest.VersionNumber = new VersionNumber("25.04.27.001");

			dprBuild.HL_ExeVersionDate = ZDateTime.Now.AddMonths(-1);
			dprBuild.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			dprBuild.HL_PackagePath = TestBuildPackagePath;
			dprBuild.VersionNumber = new VersionNumber("25.01.27.001");

			dprOld.HL_ExeVersionDate = ZDateTime.Now.AddMonths(-2);
			dprOld.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			dprOld.HL_PackagePath = TestBuildPackagePath;
			dprOld.VersionNumber = new VersionNumber("20.04.27.001");

			stdLatest.HL_ExeVersionDate = ZDateTime.Now;
			stdLatest.HL_ReleaseStatus = ReleaseRings.Codes.STD;
			stdLatest.HL_PackagePath = TestBuildPackagePath;
			stdLatest.VersionNumber = new VersionNumber("25.01.01.001");

			stdBuild.HL_ExeVersionDate = ZDateTime.Now.AddMonths(-1);
			stdBuild.HL_ReleaseStatus = ReleaseRings.Codes.STD;
			stdBuild.HL_PackagePath = TestBuildPackagePath;
			stdBuild.VersionNumber = new VersionNumber("24.01.01.001");

			stdOld.HL_ExeVersionDate = ZDateTime.Now.AddMonths(-2);
			stdOld.HL_ReleaseStatus = ReleaseRings.Codes.STD;
			stdOld.HL_PackagePath = TestBuildPackagePath;
			stdOld.VersionNumber = new VersionNumber("20.01.27.001");

			#endregion

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var clientEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();

			#region Enterprises

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "QWEEE";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ABCCC";

			enterprise.LE_EnterpriseCode = "QWE";
			enterprise.LE_OH = org1.PK;

			clientEnterprise.LE_EnterpriseCode = "ABC";
			clientEnterprise.LE_OH = org2.PK;

			#endregion

			var configString = "<CONFIGSTRING>Y,N,Y,XXXXXXX</CONFIGSTRING>";
			var stdDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var stdClientDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var stdDatabaseWithTheUpgrade = Factory.NewWithValidTestData<LicenceDatabase>();
			var stdDisableAutoDownloadDatabase = Factory.NewWithValidTestData<LicenceDatabase>();

			void SetUpDatabase(LicenceDatabase newDatabase)
			{
				newDatabase.LD_HostedLocation = "SYD";
				newDatabase.LD_IsActive = true;
				newDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
				newDatabase.LD_Product = ProductTypes.Codes.Enterprise;
				newDatabase.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
				newDatabase.LD_PublicEmailAddressForUpdate = "123@123.com";
				newDatabase.LD_ScheduleStateUPG = configString;
				newDatabase.LD_NextRunTimeUtcUPG = ZDateTime.UtcNow.AddMinutes(130);
			}

			#region STD Databases

			SetUpDatabase(stdDatabase);
			stdDatabase.LD_LE = enterprise.PK;
			stdDatabase.LD_ReleaseRing = ReleaseRings.Codes.STD;
			stdDatabase.LD_HL_CurrentRunningVersion = stdOld.PK;

			SetUpDatabase(stdClientDatabase);
			stdClientDatabase.LD_LE = clientEnterprise.PK;
			stdClientDatabase.LD_ReleaseRing = ReleaseRings.Codes.STD;
			stdClientDatabase.LD_HL_CurrentRunningVersion = stdOld.PK;

			SetUpDatabase(stdDatabaseWithTheUpgrade);
			stdDatabaseWithTheUpgrade.LD_LE = clientEnterprise.PK;
			stdDatabaseWithTheUpgrade.LD_ReleaseRing = ReleaseRings.Codes.STD;
			stdDatabaseWithTheUpgrade.LD_HL_CurrentRunningVersion = stdOld.PK;

			var request = Factory.NewWithValidTestData<UpgradesToClient>();
			request.L1_LD = stdDatabaseWithTheUpgrade.PK;
			request.L1_HL = stdBuild.PK;

			SetUpDatabase(stdDisableAutoDownloadDatabase);
			stdDisableAutoDownloadDatabase.LD_LE = clientEnterprise.PK;
			stdDisableAutoDownloadDatabase.LD_ReleaseRing = ReleaseRings.Codes.STD;
			stdDisableAutoDownloadDatabase.LD_ScheduleStateUPG = "<CONFIGSTRING>N,N,Y,XXXXXXX</CONFIGSTRING>";
			stdDisableAutoDownloadDatabase.LD_HL_CurrentRunningVersion = stdOld.PK;

			#endregion

			var dprDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var dprClientDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var dprClientDatabaseOutOfDate = Factory.NewWithValidTestData<LicenceDatabase>();

			#region DPR Databases

			SetUpDatabase(dprDatabase);
			dprDatabase.LD_LE = enterprise.PK;
			dprDatabase.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			dprDatabase.LD_HL_CurrentRunningVersion = dprOld.PK;

			SetUpDatabase(dprClientDatabase);
			dprClientDatabase.LD_LE = clientEnterprise.PK;
			dprClientDatabase.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			dprClientDatabase.LD_HL_CurrentRunningVersion = dprOld.PK;

			SetUpDatabase(dprClientDatabase);
			dprClientDatabaseOutOfDate.LD_LE = clientEnterprise.PK;
			dprClientDatabaseOutOfDate.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			dprClientDatabaseOutOfDate.LD_NextRunTimeUtcUPG = ZDateTime.UtcNow.AddMinutes(180);
			dprClientDatabaseOutOfDate.LD_HL_CurrentRunningVersion = dprOld.PK;

			#endregion

			Factory.Save();
			var logger = new TestServiceLogger();
			var task = new PrescheduleLatestUpgradeServiceTaskForTest();
			task.ServiceLogger = logger;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			task.RunTask(CancellationToken.None);

			CombineAssertions(() =>
			{
				var logString = logger.ToString();
				AssertContains(string.Empty, "5 databases were read, and newer available versions existed for 3 databases", logString);
				AssertContains(string.Empty, $"{dprBuild.VersionNumberDisplayText} will be deployed to 1 databases", logString);
				AssertContains(string.Empty, $"{stdBuild.VersionNumberDisplayText} will be deployed to 1 databases", logString);
			});

			var requests = Factory.Load<UpgradesToClient>(new ZQuery());
			AssertEquals(3, requests.Length);
			AssertNoExceptionThrown(() =>
			{
				_ = requests.Single(x => x.L1_LD == stdClientDatabase.PK && x.L1_HL == stdBuild.PK && !x.L1_NotifyUser);
				_ = requests.Single(x => x.L1_LD == dprClientDatabase.PK && x.L1_HL == dprBuild.PK && !x.L1_NotifyUser);
			});
			AssertEquals("No email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		EmbeddedResourceRetriever resourceRetriever;
		string TestBuildPackagePath => testBuildPackagePath;
		string testBuildPackagePath;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			testBuildPackagePath = resourceRetriever.SaveResourceToFile("ZClientEDI.Test.DummyUpgradePackageCS.zip");
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			resourceRetriever.Dispose();
		}

		class PrescheduleLatestUpgradeServiceTaskForTest : PreEmptivelyClientSpecificPackageServiceTask
		{
			public IEnumerable<LicenceDatabase> GetDatabasesForTest() => base.GetDatabases();

			public IDictionary<ReleaseBuild, IList<LicenceDatabase>> FindLatestBuildForDatabasesForTest(IEnumerable<LicenceDatabase> databases) => base.FindLatestBuildForDatabases(databases, CancellationToken.None);
		}
	}
}
