using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.AutoDeploy.Test
{
	[TestedType(typeof(ImportBuildsServiceTask))]
	public class ImportBuildsServiceTaskTest : ServiceTaskTestCase<ImportBuildsServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
		}

		public void TestSuccessfulImport()
		{
			CreateValidPackage("16.4.1.69", DateTime.Now);
			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			AssertReleaseBuildExists(logger.ToString(), expected: true, new Version(16, 4, 1, 69));
		}

		public void TestSuccessfulImport_CargoWiseNext()
		{
			CreateValidPackage("24.11.2.9", DateTime.Now);
			using (EDIDataRegistry.Instance.CargoWiseNextMinimumVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "24.11.1.0"))
			{
				var logger = new TestServiceLogger();
				new ImportBuildsServiceTask()
				{ ServiceLogger = logger }.RunTask();
				AssertReleaseBuildExists(logger.ToString(), expected: true, new Version(24, 11, 2, 9), "CWN");
			}
		}

		public void TestAlreadyImported()
		{
			TestSuccessfulImport();
			EDIDataRegistry.Instance.LastIBPPackageCheck = DateTime.MinValue;
			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			AssertContains("Version already imported", logger.ToString());
		}

		public void TestHighWaterMark()
		{
			CreateValidPackage("16.4.1.69", DateTime.Now);
			Thread.Sleep(TimeSpan.FromSeconds(1));
			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			AssertReleaseBuildExists(logger.ToString(), true, new Version(16, 4, 1, 69));
			logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			AssertNotContains("Version already imported", logger.ToString());
		}

		public void TestHighWaterMarkWhenFileEditedBeforeCreated()
		{
			var filePath = CreateValidPackage("16.4.1.69", new DateTime(2015, 1, 1));
			var fileInfo = new FileInfo(filePath);
			fileInfo.LastWriteTimeUtc = new DateTime(2015, 7, 13);
			fileInfo.CreationTimeUtc = new DateTime(2015, 7, 15);
			EDIDataRegistry.Instance.LastIBPPackageCheck = new DateTime(2015, 7, 14);

			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask { ServiceLogger = logger }.RunTask();
			AssertReleaseBuildExists(logger.ToString(), true, new Version(16, 4, 1, 69));
		}

		public void TestImportFailure()
		{
			File.WriteAllText(Path.Combine(tempDirectory, "whatever.edp"), "not a valid package");
			var logger = new TestServiceLogger();
			ErrorReporter.Clear();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			AssertReleaseBuildExists(logger.ToString(), false, new Version(16, 4, 1, 69));
			AssertContains("Failed to import", logger.ToString());
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestImportFailure_EmptyArchive()
		{
			CreateValidPackage("16.4.1.69", DateTime.Now, addEntry: false);
			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			AssertReleaseBuildExists(logger.ToString(), false, new Version(16, 4, 1, 69));
			AssertContains("Failed to import", logger.ToString());
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestShouldHaveMaxRetry_WhenImportFailure()
		{
			var lastIBPPackageCheck = DateTime.UtcNow.AddDays(-9);
			EDIDataRegistry.Instance.LastIBPPackageCheck = lastIBPPackageCheck;
			File.WriteAllText(Path.Combine(tempDirectory, "whatever.edp"), "not a valid package");
			CreateValidPackage("16.4.1.69", DateTime.Now);
			Task.Delay(100).Wait();

			var logger = new TestServiceLogger();
			ErrorReporter.Clear();

			var ibp = new ImportBuildsServiceTask() { ServiceLogger = logger };
			ibp.RunTask();
			AssertContains("Failed to import", logger.ToString());
			AssertContains("Importing", logger.ToString());
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			Task.Delay(100).Wait();

			logger.ClearLog();
			ibp.RunTask();
			AssertReleaseBuildExists(logger.ToString(), true, new Version(16, 4, 1, 69));
			AssertNotContains("Failed to import", logger.ToString());
			AssertNotContains("Importing", logger.ToString());
			Assert("LastIBPPackageCheck Registry should be updated", EDIDataRegistry.Instance.LastIBPPackageCheck - lastIBPPackageCheck > TimeSpan.FromDays(7));
		}

		public void TestRecentPackageRetained()
		{
			var package = CreateValidPackage("17.10.11.78", DateTime.Now.Subtract(TimeSpan.FromDays(1)));
			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			AssertEquals(true, GetReleaseBuild(new Version(17, 10, 11, 78)).HL_IsActive);
			Assert(File.Exists(package));
		}

		public void TesOldPackageUsedByActiveLicenseRetained()
		{
			var package = CreateValidPackage("17.10.11.78", DateTime.Now.Subtract(TimeSpan.FromDays(60)));
			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			var licence = Factory.NewWithValidTestData<LicenceDatabase>();
			licence.LD_HL_CurrentRunningVersion = GetReleaseBuild(new Version(17, 10, 11, 78)).PK;
			licence.LD_LastHeartbeat = DateTime.Now.Subtract(TimeSpan.FromDays(1));
			Factory.Save();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			AssertEquals(true, GetReleaseBuild(new Version(17, 10, 11, 78)).HL_IsActive);
			Assert(File.Exists(package));
		}

		public void TestOldPackageUsedByInactiveLicenseNotRetained()
		{
			var package = CreateValidPackage("17.10.11.78", DateTime.Now.Subtract(TimeSpan.FromDays(60)));
			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			var licence = Factory.NewWithValidTestData<LicenceDatabase>();
			licence.LD_HL_CurrentRunningVersion = GetReleaseBuild(new Version(17, 10, 11, 78)).PK;
			licence.LD_LastHeartbeat = DateTime.Now.Subtract(TimeSpan.FromDays(60));
			Factory.Save();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			AssertEquals(false, GetReleaseBuild(new Version(17, 10, 11, 78)).HL_IsActive);
			Assert(!File.Exists(package));
		}

		public void TestUnusedOldPackageNotRetained()
		{
			var cw1Version = "17.10.11.78";
			AssertUnusedOldPackageNotRetained(cw1Version);

			var cwNextVersion = "24.12.1.90";
			AssertUnusedOldPackageNotRetained(cwNextVersion);

			void AssertUnusedOldPackageNotRetained(string version)
			{
				var package = CreateValidPackage(version, DateTime.Now.Subtract(TimeSpan.FromDays(60)));
				var logger = new TestServiceLogger();
				new ImportBuildsServiceTask()
				{ ServiceLogger = logger }.RunTask();
				new ImportBuildsServiceTask()
				{ ServiceLogger = logger }.RunTask();
				AssertEquals(false, GetReleaseBuild(new Version(version)).HL_IsActive);
				Assert(!File.Exists(package));
			}
		}

		public void TestLastBuildFromBranchRetained()
		{
			var package1 = CreateValidPackage("17.10.11.77", DateTime.Now.Subtract(TimeSpan.FromDays(60)), "STD");
			var package2 = CreateValidPackage("17.10.11.78", DateTime.Now.Subtract(TimeSpan.FromDays(60)), "STD");
			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			AssertEquals(false, GetReleaseBuild(new Version(17, 10, 11, 77)).HL_IsActive);
			Assert(!File.Exists(package1));
			AssertEquals(true, GetReleaseBuild(new Version(17, 10, 11, 78)).HL_IsActive);
			Assert(File.Exists(package2));
		}

		public void TestUnretainedBuildWithNoPackageFile()
		{
			var rb = Factory.New<ReleaseBuild>();
			rb.HL_MajorVersion = 17;
			rb.HL_MinorVersion = 10;
			rb.HL_Release = 11;
			rb.HL_Patch = 78;
			rb.HL_ExeVersionDate = DateTime.Now.Subtract(TimeSpan.FromDays(60));
			rb.HL_ReleaseStatus = "ALP";
			Factory.Save();
			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			rb.Reload();
			AssertEquals(false, GetReleaseBuild(new Version(17, 10, 11, 78)).HL_IsActive);
		}

		public void TestSapphireBuildRetained()
		{
			var rb1 = Factory.New<ReleaseBuild>();
			rb1.HL_MajorVersion = 0;
			rb1.HL_MinorVersion = 11;
			rb1.HL_Release = 1;
			rb1.HL_Patch = 1;
			rb1.HL_ExeVersionDate = DateTime.Now.Subtract(TimeSpan.FromDays(60));
			rb1.HL_Product = "SPH";
			var rb2 = Factory.New<ReleaseBuild>();
			rb2.HL_MajorVersion = 0;
			rb2.HL_MinorVersion = 11;
			rb2.HL_Release = 1;
			rb2.HL_Patch = 2;
			rb2.HL_ExeVersionDate = DateTime.Now.Subtract(TimeSpan.FromDays(60));
			rb2.HL_Product = "SPH";
			Factory.Save();
			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();
			rb1.Reload();
			AssertEquals(true, GetReleaseBuild(new Version(0, 11, 1, 1)).HL_IsActive);
			AssertEquals(true, GetReleaseBuild(new Version(0, 11, 1, 2)).HL_IsActive);
		}

		public void TestCleanupOldReleaseBuildsShouldUpdateAuditColumns()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AX";
			staff.GS_FullName = "Archie";
			Factory.Save();
			ZString originalLastEditUser;
			ZDateTime originalLastEditDate;
			var zGuid = ZGuid.NewZGuid();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var rb = Factory.NewWithPrimaryKey<ReleaseBuild>(zGuid.ToGuid());
				rb.HL_MajorVersion = 17;
				rb.HL_MinorVersion = 10;
				rb.HL_Release = 11;
				rb.HL_Patch = 78;
				rb.HL_ExeVersionDate = DateTime.Now.Subtract(TimeSpan.FromDays(60));
				rb.HL_ReleaseStatus = "ALP";
				Factory.Save();

				originalLastEditUser = rb.HL_SystemLastEditUser;
				originalLastEditDate = rb.HL_SystemLastEditTimeUtc;
			}

			Env.ClearUserContext();

			var logger = new TestServiceLogger();
			new ImportBuildsServiceTask()
			{ ServiceLogger = logger }.RunTask();

			var factory = new BusinessObjectFactory();
			var build = factory.Load<ReleaseBuild>(zGuid);
			AssertNotEquals(originalLastEditUser, build.HL_SystemLastEditUser);
			AssertNotEquals(originalLastEditDate, build.HL_SystemLastEditTimeUtc);
			AssertEquals(false, build.HL_IsActive);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		string CreateValidPackage(string versionNumber, DateTime exeDate, string ring = "ALP", bool addEntry = true)
		{
			var packageFile = Path.Combine(tempDirectory, versionNumber + ".edp");
			using (var zip = new ZipArchive(File.Create(packageFile), ZipArchiveMode.Create, false))
			using (var releaseInfoFile = TempFile.New())
			{
				if (addEntry)
				{
					ReleaseInfo.CreateNewFileForTesting(releaseInfoFile.Filename, versionNumber, exeDate, ring);
					zip.CreateEntryFromFile(releaseInfoFile.Filename, ReleaseInfo.XmlFileName);
				}

				return packageFile;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		void AssertReleaseBuildExists(string message, bool expected, Version version, string product = "")
		{
			AssertEquals(message, expected, Factory.GetDatabaseCount(typeof(ReleaseBuild), GetReleaseBuildQuery(version, product)) > 0);
		}

		ReleaseBuild GetReleaseBuild(Version version, string product = "")
		{
			return Factory.LoadTop1<ReleaseBuild>(GetReleaseBuildQuery(version, product));
		}

		ZQuery GetReleaseBuildQuery(Version version, string product)
		{
			var query = new ZQuery();
			query.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, version.Major);
			query.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, version.Minor);
			query.AddToFilter(ReleaseBuildSchema.HL_Release, version.Build);
			query.AddToFilter(ReleaseBuildSchema.HL_Patch, version.Revision);
			if (!string.IsNullOrEmpty(product))
			{
				query.AddToFilter(ReleaseBuildSchema.HL_Product, product);
			}
			return query;
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			tempDirectory = new TempDirectory();
			EDIDataRegistry.Instance.MasterPackageArchivePath = tempDirectory.DirectoryName;
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			if (tempDirectory != null)
			{
				tempDirectory.Dispose();
			}
		}

		TempDirectory tempDirectory;
	}
}
