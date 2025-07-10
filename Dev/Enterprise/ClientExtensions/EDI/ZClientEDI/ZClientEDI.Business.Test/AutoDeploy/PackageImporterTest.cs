using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.AutoDeploy.Test
{
	class PackageImporterTest : TestCaseWithFactory
	{
		public void TestImportPackage()
		{
			int originalReleaseBuildCount = Factory.GetDatabaseCount(typeof(ReleaseBuild));

			ZQuery filter = new ZQuery(ReleaseBuildSchema.HL_MajorVersion, 1);
			filter.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, 2);
			filter.AddToFilter(ReleaseBuildSchema.HL_Release, 3);
			filter.AddToFilter(ReleaseBuildSchema.HL_Patch, 4);

			ReleaseBuildCollection collection = new ReleaseBuildCollection(Factory, filter);
			collection.Load();

			AssertEquals("Precondition: There should not be any ReleaseBuilds with the version 1.2.3.4.", 0, collection.Count);

			using (var zipArchiveFile = TempFile.New())
			using (var releaseInfoFile = TempFile.New())
			{
				ZDateTime exeDate = new ZDateTime(2006, 7, 2, 12, 31, 0);
				ReleaseInfo.CreateNewFileForTesting(releaseInfoFile.Filename, "1.2.3.4", exeDate.ToDateTime(), "DPR");
				using (var zip = new ZipArchive(File.Create(zipArchiveFile.Filename), ZipArchiveMode.Create, false))
				{
					zip.CreateEntryFromFile(releaseInfoFile.Filename, ReleaseInfo.XmlFileName);
				}

				using (var importer = new PackageImporter(Factory, zipArchiveFile.Filename))
				{
					importer.Import();
				}

				int originalStmUpgradeCount = Factory.GetDatabaseCount(typeof(StmUpgrade));

				ReleaseInfo releaseInfo = ReleaseInfo.CreateNewInstanceForTesting("1.2.3.4", exeDate.ToDateTime(), ReleaseRings.Codes.DPR);

				AssertEquals("Only one new ReleaseBuild should be created.", originalReleaseBuildCount + 1, Factory.GetDatabaseCount(typeof(ReleaseBuild)));
				AssertEquals("No StmUpgrade should be created.", originalStmUpgradeCount, Factory.GetDatabaseCount(typeof(StmUpgrade)));

				filter.ReLoadExistingRows = true;
				collection.Load(filter);
				AssertEquals("A ReleaseBuild should have been imported.", 1, collection.Count);

				ReleaseBuild build = collection[0];
				AssertEquals("ExeVersion", "1.2.3.4", build.ExeVersion);
				AssertEquals("HL_ExeVersionDate", exeDate, build.HL_ExeVersionDate);
				AssertEquals("HL_ReleaseStatus", ReleaseRings.Codes.DPR, build.HL_ReleaseStatus);
				AssertEquals("HL_Superceded", false, build.HL_Superceded);
				AssertEquals("HL_PackagePath", build.HL_PackagePath, zipArchiveFile.Filename);
				AssertEquals("HL_Product", "ENT", build.HL_Product);
			}
		}

		public void TestImportPackage_CargoWiseNext()
		{
			using (EDIDataRegistry.Instance.CargoWiseNextMinimumVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "24.11.1.0"))
			using (var zipArchiveFile = TempFile.New())
			using (var releaseInfoFile = TempFile.New())
			{
				var exeDate = new ZDateTime(2024, 11, 4, 0, 0, 0);
				ReleaseInfo.CreateNewFileForTesting(releaseInfoFile.Filename, "24.11.4.9", exeDate.ToDateTime(), "DPR");
				using (var zip = new ZipArchive(File.Create(zipArchiveFile.Filename), ZipArchiveMode.Create, false))
				{
					zip.CreateEntryFromFile(releaseInfoFile.Filename, ReleaseInfo.XmlFileName);
				}
				using (var importer = new PackageImporter(Factory, zipArchiveFile.Filename))
				{
					importer.Import();
				}

				var filter = new ZQuery(ReleaseBuildSchema.HL_MajorVersion, 24);
				filter.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, 11);
				filter.AddToFilter(ReleaseBuildSchema.HL_Release, 4);
				filter.AddToFilter(ReleaseBuildSchema.HL_Patch, 9);

				var build = Factory.LoadTop1<ReleaseBuild>(filter);
				AssertEquals("ExeVersion", "24.11.4.9", build.ExeVersion);
				AssertEquals("HL_ExeVersionDate", exeDate, build.HL_ExeVersionDate);
				AssertEquals("HL_ReleaseStatus", ReleaseRings.Codes.DPR, build.HL_ReleaseStatus);
				AssertEquals("HL_Superceded", false, build.HL_Superceded);
				AssertEquals("HL_PackagePath", build.HL_PackagePath, zipArchiveFile.Filename);
				AssertEquals("HL_Product", "CWN", build.HL_Product);
			}
		}

		public void TestImportPackage_CargoWise()
		{
			using (EDIDataRegistry.Instance.CargoWiseMinimumVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "25.3.19.0"))
			using (var zipArchiveFile = TempFile.New())
			using (var releaseInfoFile = TempFile.New())
			{
				var exeDate = new ZDateTime(2024, 10, 4, 0, 0, 0);
				ReleaseInfo.CreateNewFileForTesting(releaseInfoFile.Filename, "25.3.19.9", exeDate.ToDateTime(), "DPR");
				using (var zip = new ZipArchive(File.Create(zipArchiveFile.Filename), ZipArchiveMode.Create, false))
				{
					zip.CreateEntryFromFile(releaseInfoFile.Filename, ReleaseInfo.XmlFileName);
				}
				using (var importer = new PackageImporter(Factory, zipArchiveFile.Filename))
				{
					importer.Import();
				}

				var filter = new ZQuery(ReleaseBuildSchema.HL_MajorVersion, 25);
				filter.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, 3);
				filter.AddToFilter(ReleaseBuildSchema.HL_Release, 19);
				filter.AddToFilter(ReleaseBuildSchema.HL_Patch, 9);

				var build = Factory.LoadTop1<ReleaseBuild>(filter);
				AssertEquals("ExeVersion", "25.3.19.9", build.ExeVersion);
				AssertEquals("HL_ExeVersionDate", exeDate, build.HL_ExeVersionDate);
				AssertEquals("HL_ReleaseStatus", ReleaseRings.Codes.DPR, build.HL_ReleaseStatus);
				AssertEquals("HL_Superceded", false, build.HL_Superceded);
				AssertEquals("HL_PackagePath", build.HL_PackagePath, zipArchiveFile.Filename);
				AssertEquals("HL_Product", "CGW", build.HL_Product);
			}
		}

		public void TestImportPackage_OlderThanCargoWiseNewerThanCargoWiseNext()
		{
			using (EDIDataRegistry.Instance.CargoWiseMinimumVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "25.3.19.0"))
			using (EDIDataRegistry.Instance.CargoWiseNextMinimumVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "25.1.1.0"))
			using (var zipArchiveFile = TempFile.New())
			using (var releaseInfoFile = TempFile.New())
			{
				var exeDate = new ZDateTime(2024, 10, 4, 0, 0, 0);
				ReleaseInfo.CreateNewFileForTesting(releaseInfoFile.Filename, "25.2.2.9", exeDate.ToDateTime(), "DPR");
				using (var zip = new ZipArchive(File.Create(zipArchiveFile.Filename), ZipArchiveMode.Create, false))
				{
					zip.CreateEntryFromFile(releaseInfoFile.Filename, ReleaseInfo.XmlFileName);
				}
				using (var importer = new PackageImporter(Factory, zipArchiveFile.Filename))
				{
					importer.Import();
				}

				var filter = new ZQuery(ReleaseBuildSchema.HL_MajorVersion, 25);
				filter.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, 2);
				filter.AddToFilter(ReleaseBuildSchema.HL_Release, 2);
				filter.AddToFilter(ReleaseBuildSchema.HL_Patch, 9);

				var build = Factory.LoadTop1<ReleaseBuild>(filter);
				AssertEquals("ExeVersion", "25.2.2.9", build.ExeVersion);
				AssertEquals("HL_ExeVersionDate", exeDate, build.HL_ExeVersionDate);
				AssertEquals("HL_ReleaseStatus", ReleaseRings.Codes.DPR, build.HL_ReleaseStatus);
				AssertEquals("HL_Superceded", false, build.HL_Superceded);
				AssertEquals("HL_PackagePath", build.HL_PackagePath, zipArchiveFile.Filename);
				AssertEquals("HL_Product", "CWN", build.HL_Product);
			}
		}

		public void TestImportPackage_OlderThanCargoWiseAndCargoWiseNext()
		{
			using (EDIDataRegistry.Instance.CargoWiseMinimumVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "25.3.19.0"))
			using (EDIDataRegistry.Instance.CargoWiseNextMinimumVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "25.1.1.0"))
			using (var zipArchiveFile = TempFile.New())
			using (var releaseInfoFile = TempFile.New())
			{
				var exeDate = new ZDateTime(2024, 10, 4, 0, 0, 0);
				ReleaseInfo.CreateNewFileForTesting(releaseInfoFile.Filename, "24.10.30.9", exeDate.ToDateTime(), "DPR");
				using (var zip = new ZipArchive(File.Create(zipArchiveFile.Filename), ZipArchiveMode.Create, false))
				{
					zip.CreateEntryFromFile(releaseInfoFile.Filename, ReleaseInfo.XmlFileName);
				}
				using (var importer = new PackageImporter(Factory, zipArchiveFile.Filename))
				{
					importer.Import();
				}

				var filter = new ZQuery(ReleaseBuildSchema.HL_MajorVersion, 24);
				filter.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, 10);
				filter.AddToFilter(ReleaseBuildSchema.HL_Release, 30);
				filter.AddToFilter(ReleaseBuildSchema.HL_Patch, 9);

				var build = Factory.LoadTop1<ReleaseBuild>(filter);
				AssertEquals("ExeVersion", "24.10.30.9", build.ExeVersion);
				AssertEquals("HL_ExeVersionDate", exeDate, build.HL_ExeVersionDate);
				AssertEquals("HL_ReleaseStatus", ReleaseRings.Codes.DPR, build.HL_ReleaseStatus);
				AssertEquals("HL_Superceded", false, build.HL_Superceded);
				AssertEquals("HL_PackagePath", build.HL_PackagePath, zipArchiveFile.Filename);
				AssertEquals("HL_Product", "ENT", build.HL_Product);
			}
		}

		public void TestImportPackageWhenLastBuildIsSupersededButCutOffBuildIsFine()
		{
			DateTime lastCutOffDateTime;
			var existingBuilds = PrepareBuildsForImportPackageTest(true, false, true, true, false, ReleaseRings.Codes.DPR, out lastCutOffDateTime);

			var build1001 = existingBuilds[0];
			var build1002 = existingBuilds[1];

			AssertNotNull(build1001);
			AssertNotNull(build1002);

			Assert(!build1001.HL_Superceded);
			Assert(build1002.HL_Superceded);

			var newBuild = ImportPackageForTest(DateTime.Now);

			Assert("New build should not be active if the last one is superceded", newBuild.HL_Superceded);
		}

		public void TestImportPackageWhenOnlyOneLastBuildIsActive()
		{
			DateTime lastCutOffDateTime;
			var existingBuilds = PrepareBuildsForImportPackageTest(true, true, true, false, false, ReleaseRings.Codes.DPR, out lastCutOffDateTime);

			var build1001 = existingBuilds[0];
			var build1002 = existingBuilds[1];

			AssertNotNull(build1001);
			AssertNotNull(build1002);

			Assert(build1001.HL_Superceded);
			Assert(!build1002.HL_Superceded);

			Assert(build1001.HL_ExeVersionDate < lastCutOffDateTime);
			Assert(build1002.HL_ExeVersionDate > lastCutOffDateTime);

			var newBuild = ImportPackageForTest(DateTime.Now);

			build1001.Reload();
			build1002.Reload();

			Assert("New build is supposed to be active", !newBuild.HL_Superceded);
			Assert("The second last build should be considered as a weekly cut-off build and supposed to be active", !build1002.HL_Superceded);
			Assert("The weekly build has to have a proper comment", build1002.HL_Comment == "Latest weekly DPR");
		}

		public void TestImportPackageWhenNoActiveBuildsAreBeforeCutOffDateAndTwoBuildsAreAfter()
		{
			DateTime lastCutOffDateTime;
			var existingBuilds = PrepareBuildsForImportPackageTest(true, true, true, true, true, ReleaseRings.Codes.DPR, out lastCutOffDateTime);

			var build1001 = existingBuilds[0];
			var build1002 = existingBuilds[1];

			AssertNotNull(build1001);
			AssertNotNull(build1002);

			Assert(!build1001.HL_Superceded);
			Assert(!build1002.HL_Superceded);

			Assert(build1001.HL_ExeVersionDate > lastCutOffDateTime);
			Assert(build1002.HL_ExeVersionDate > lastCutOffDateTime);

			var newBuild = ImportPackageForTest(DateTime.Now);

			build1001.Reload();
			build1002.Reload();

			Assert("New build is supposed to be active", !newBuild.HL_Superceded);
			Assert("The build before the new one is supposed to be superceded", build1002.HL_Superceded);
			Assert("The first build after cut-off date should be considered as a weekly cut-off build and is supposed to be active", !build1001.HL_Superceded);
			Assert("The weekly build has to have a proper comment", build1001.HL_Comment == "Latest weekly DPR");
		}

		public void TestImportPackageWithOneActiveBuildBeforeCutOffDateAndLastBuildIsActive()
		{
			DateTime lastCutOffDateTime;
			var existingBuilds = PrepareBuildsForImportPackageTest(true, true, true, true, false, ReleaseRings.Codes.DPR, out lastCutOffDateTime);

			var build1001 = existingBuilds[0];
			var build1002 = existingBuilds[1];

			AssertNotNull(build1001);
			AssertNotNull(build1002);

			Assert(!build1001.HL_Superceded);
			Assert(!build1002.HL_Superceded);

			Assert(build1001.HL_ExeVersionDate < lastCutOffDateTime);
			Assert(build1002.HL_ExeVersionDate > lastCutOffDateTime);

			var newBuild = ImportPackageForTest(DateTime.Now);

			build1001.Reload();
			build1002.Reload();

			Assert("New build is supposed to be active", !newBuild.HL_Superceded);
			Assert("The build before the new one is supposed to be superceded", build1002.HL_Superceded);
			Assert("The last build before cut-off date should be considered as a weekly cut-off build and is supposed to be active", !build1001.HL_Superceded);
			Assert("The weekly build has to have a proper comment", build1001.HL_Comment == "Latest weekly DPR");
		}

		public void TestImportPackageWithTwoActiveBuildsBeforeCutOffDate()
		{
			DateTime lastCutOffDateTime;
			var existingBuilds = PrepareBuildsForImportPackageTest(true, true, false, true, false, ReleaseRings.Codes.DPR, out lastCutOffDateTime);

			var build1001 = existingBuilds[0];
			var build1002 = existingBuilds[1];

			AssertNotNull(build1001);
			AssertNotNull(build1002);

			Assert(!build1001.HL_Superceded);
			Assert(!build1002.HL_Superceded);

			Assert(build1001.HL_ExeVersionDate < lastCutOffDateTime);
			Assert(build1002.HL_ExeVersionDate < lastCutOffDateTime);

			var newBuild = ImportPackageForTest(DateTime.Now);

			build1001.Reload();
			build1002.Reload();

			Assert("New build is supposed to be active", !newBuild.HL_Superceded);
			Assert("The last build before cut-off date should be considered as a weekly cut-off build and is supposed to be active", !build1002.HL_Superceded);
			Assert("The former cut-off build is supposed to be superseded", build1001.HL_Superceded);
			Assert("The weekly build has to have a proper comment", build1002.HL_Comment == "Latest weekly DPR");
		}

		public void TestImportPackageLatestWeeklyWithLogs()
		{
			DateTime lastCutOffDateTime;
			var existingBuilds = PrepareBuildsForImportPackageTest(true, true, false, true, false, ReleaseRings.Codes.DPR, out lastCutOffDateTime);

			var build1001 = existingBuilds[0];
			var build1002 = existingBuilds[1];

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			build1002.Logs.AddNew(AutoEvents.EditedARecord, "Marked as weekly build by IBP Service Task");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			build1001.Factory.Save();

			AssertNotEquals("Latest weekly DPR", build1002.HL_Comment);
			AssertNotNull(build1001);
			AssertNotNull(build1002);

			Assert(!build1001.HL_Superceded);
			Assert(!build1002.HL_Superceded);

			Assert(build1001.HL_ExeVersionDate < lastCutOffDateTime);
			Assert(build1002.HL_ExeVersionDate < lastCutOffDateTime);

			Factory.SetContext(EDIConstants.BusinessContext.ImportBuildsServiceTask);
			var newBuild = ImportPackageForTest(DateTime.Now);

			build1001.Reload();
			build1002.Reload();

			Assert("New build is supposed to be active", !newBuild.HL_Superceded);
			Assert("The last build before cut-off date should be considered as a weekly cut-off build and is supposed to be active", !build1002.HL_Superceded);
			Assert("The former cut-off build is supposed to be superseded", build1001.HL_Superceded);
			Assert("The weekly build has to have a proper comment", build1002.HL_Comment == "Latest weekly DPR");

			var expectedReference = "Marked as weekly build by IBP Service Task";
			var expectedLog = build1002.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, expectedReference));
			AssertEquals("Log still needs to be added because comments was changed", 2, expectedLog.Length);

			Assert(!newBuild.HL_Superceded);
			ImportPackageWithVersionForTest(DateTime.Now.AddMinutes(1), "1.2.3.5");

			build1001.Reload();
			build1002.Reload();
			newBuild.Reload();

			Assert(newBuild.HL_Superceded);
			expectedLog = build1002.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, expectedReference));
			AssertEquals("Log should not be added again because comments has no changes", 2, expectedLog.Length);
		}

		public void TestImportPackageWithWeeklyCutOffBuildsDisabled()
		{
			DateTime lastCutOffDateTime;
			var existingBuilds = PrepareBuildsForImportPackageTest(false, true, true, true, false, ReleaseRings.Codes.DPR, out lastCutOffDateTime);

			var build1001 = existingBuilds[0];
			var build1002 = existingBuilds[1];

			AssertNotNull(build1001);
			AssertNotNull(build1002);

			Assert(!build1001.HL_Superceded);
			Assert(!build1002.HL_Superceded);

			Assert(build1001.HL_ExeVersionDate < lastCutOffDateTime);
			Assert(build1002.HL_ExeVersionDate > lastCutOffDateTime);

			var newBuild = ImportPackageForTest(DateTime.Now);

			build1001.Reload();
			build1002.Reload();

			Assert("New build is supposed to be active", !newBuild.HL_Superceded);
			Assert("Only one build is supposed to be active as weekly build cut-off is disabled", build1002.HL_Superceded && build1001.HL_Superceded);
		}

		public void TestLastBuildCutOffDateTimeIsCalculatedCorrectly()
		{
			EDIDataRegistry.Instance.EnableWeeklyBuildCutOff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Action<DayOfWeek, string> setDayOfWeekAndTimeForWeeklyBuild = (d, t) =>
			{
				EDIDataRegistry.Instance.WeeklyBuildCutOffDay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, d.ToString());
				EDIDataRegistry.Instance.WeeklyBuildCutOffTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, t);
			};

			var newbuildExeDate = new DateTime(2017, 5, 22, 16, 45, 20); //Monday

			DateTime lastWeeklyBuildCutOffDateTime = DateTime.MinValue;

			Action assertEverythingIsFine = () =>
			{
				var delta = newbuildExeDate - lastWeeklyBuildCutOffDateTime;
				Assert(delta > TimeSpan.Zero);
				Assert(delta < TimeSpan.FromDays(7));

				var expectedDayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), EDIDataRegistry.Instance.WeeklyBuildCutOffDay.Value);
				DateTime tempDate;
				Assert(DateTime.TryParse(EDIDataRegistry.Instance.WeeklyBuildCutOffTime.Value, out tempDate));
				var expectedTime = tempDate.TimeOfDay;
				Assert(lastWeeklyBuildCutOffDateTime.DayOfWeek == expectedDayOfWeek);
				Assert(lastWeeklyBuildCutOffDateTime.TimeOfDay == expectedTime);
			};

			var weeklyBuildCutOffSettings = new WeeklyCutOffBuildSettings();

			//scenario 1: the same day of week as the new build, the time is earlier (same day expected)
			setDayOfWeekAndTimeForWeeklyBuild(DayOfWeek.Monday, "13:40");
			lastWeeklyBuildCutOffDateTime = weeklyBuildCutOffSettings.GetLatestCutOffDateTime(newbuildExeDate);
			assertEverythingIsFine();

			//scenario 2: the same day of week as the new build, the time is greater (a week ago day expected)
			setDayOfWeekAndTimeForWeeklyBuild(DayOfWeek.Monday, "16:50:27");
			lastWeeklyBuildCutOffDateTime = weeklyBuildCutOffSettings.GetLatestCutOffDateTime(newbuildExeDate);
			assertEverythingIsFine();

			//scenario 3: another day of week than the day of the new build (the day in the a week ago expected)
			setDayOfWeekAndTimeForWeeklyBuild(DayOfWeek.Wednesday, "16:50:27");
			lastWeeklyBuildCutOffDateTime = weeklyBuildCutOffSettings.GetLatestCutOffDateTime(newbuildExeDate);
			assertEverythingIsFine();

			//scenario 4: the date of the new build coincides with the last cut-off date
			setDayOfWeekAndTimeForWeeklyBuild(DayOfWeek.Monday, "16:45:20");
			lastWeeklyBuildCutOffDateTime = weeklyBuildCutOffSettings.GetLatestCutOffDateTime(newbuildExeDate);
			Assert(newbuildExeDate == lastWeeklyBuildCutOffDateTime);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestCorrectSupersessionIrrespectiveOfImportOrder()
		{
			var newTime = DateTime.Now;

			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.ReleaseBuild");
			var countOfBuilds = Factory.GetDatabaseCount(typeof(ReleaseBuild));
			Assert(countOfBuilds == 0);

			//base build is manually inserted and not imported.
			var baseBuild = Factory.New<ReleaseBuild>();
			baseBuild.HL_MajorVersion = 1;
			baseBuild.HL_MinorVersion = 0;
			baseBuild.HL_Release = 0;
			baseBuild.HL_Patch = 1;
			baseBuild.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			baseBuild.HL_Superceded = false;
			baseBuild.HL_ExeVersionDate = newTime.AddDays(-1);

			//subsequent builds are added via import process.
			var b001 = ImportPackageWithVersionForTest(newTime, "1.0.0.20");
			var b002 = ImportPackageWithVersionForTest(newTime.AddMinutes(-1), "1.0.0.15");
			var b003 = ImportPackageWithVersionForTest(newTime.AddMinutes(-2), "1.0.0.11");
			var b004 = ImportPackageWithVersionForTest(newTime.AddMinutes(-3), "1.0.0.9");
			var b005 = ImportPackageWithVersionForTest(newTime.AddMinutes(-4), "1.0.0.7");
			var b006 = ImportPackageWithVersionForTest(newTime.AddMinutes(-5), "1.0.0.5");

			Assert("Base build should be superseded", baseBuild.HL_Superceded);
			Assert("Newest build should not be superseded", !b001.HL_Superceded);
			Assert("Second newest build should be superseded", b002.HL_Superceded);
			Assert("Third newest build should be superseded", b003.HL_Superceded);
			Assert("Fourth newest build should be superseded", b004.HL_Superceded);
			Assert("Fifth newest build should be superseded", b005.HL_Superceded);
			Assert("Sixth newest build should be superseded", b006.HL_Superceded);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestCorrectSupersessionUsesVersionWhenSameDate()
		{
			var newTime = new DateTime(2017, 9, 9, 9, 0, 0);

			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.ReleaseBuild");
			var countOfBuilds = Factory.GetDatabaseCount(typeof(ReleaseBuild));
			Assert(countOfBuilds == 0);

			//base build is manually inserted and not imported.
			var baseBuild = Factory.New<ReleaseBuild>();
			baseBuild.HL_MajorVersion = 1;
			baseBuild.HL_MinorVersion = 0;
			baseBuild.HL_Release = 0;
			baseBuild.HL_Patch = 1;
			baseBuild.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			baseBuild.HL_Superceded = false;
			baseBuild.HL_ExeVersionDate = newTime.AddDays(-1);

			Assert("Base build should not be superseded yet ", !baseBuild.HL_Superceded);
			//subsequent builds are added via import process.
			var build10 = ImportPackageWithVersionForTest(newTime, "1.0.0.10");
			Assert("Base build should be superseded", baseBuild.HL_Superceded);
			Assert("Newest build should not be superseded", !build10.HL_Superceded);

			var build11 = ImportPackageWithVersionForTest(newTime, "1.0.0.11");
			Assert("old build should be superseded", build10.HL_Superceded);
			Assert("Newest build should not be superseded", !build11.HL_Superceded);

			var build20 = ImportPackageWithVersionForTest(newTime, "1.0.0.20");
			Assert("old build should be superseded", build11.HL_Superceded);
			Assert("Newest build should not be superseded", !build20.HL_Superceded);

			var build3 = ImportPackageWithVersionForTest(newTime, "1.0.0.3");
			Assert("this build should be superseded", build3.HL_Superceded);
			Assert("Newest build should not be superseded", !build20.HL_Superceded);
		}

		public void TestImportPackageWhenFirstInRing()
		{
			var newBuild = ImportPackageForTest(DateTime.Now);
			Assert("First imported package in a ring should not be superseded", !newBuild.HL_Superceded);
		}

		ReleaseBuild ImportPackageForTest(DateTime exeDate)
		{
			return ImportPackageWithVersionForTest(exeDate, "1.2.3.4");
		}

		ReleaseBuild ImportPackageWithVersionForTest(DateTime exeDate, string versionNumber)
		{
			using (var zipArchiveFile = TempFile.New())
			using (var releaseInfoFile = TempFile.New())
			{
				ReleaseInfo.CreateNewFileForTesting(releaseInfoFile.Filename, versionNumber, exeDate, ReleaseRings.Codes.DPR);
				using (var zip = new ZipArchive(File.Create(zipArchiveFile.Filename), ZipArchiveMode.Create, false))
				{
					zip.CreateEntryFromFile(releaseInfoFile.Filename, ReleaseInfo.XmlFileName);
				}

				using (var importer = new PackageImporter(Factory, zipArchiveFile.Filename))
				{
					return importer.Import();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		internal static List<ReleaseBuild> PrepareBuildsForImportPackageTest(bool enableWeeklyBuildCutOff,
			bool isTheLastBuildActive,
			bool isTheLastBuildAfterCutOff,
			bool isTheFirstBuildActive,
			bool isTheFirstBuildAfterCutOff,
			string releaseRing,
			out DateTime lastCutOffDate)
		{
			lastCutOffDate = DateTime.Now.AddDays(-1).Date;

			if (enableWeeklyBuildCutOff)
			{
				EDIDataRegistry.Instance.EnableWeeklyBuildCutOff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				EDIDataRegistry.Instance.WeeklyBuildCutOffDay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lastCutOffDate.DayOfWeek.ToString());
				EDIDataRegistry.Instance.WeeklyBuildCutOffTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lastCutOffDate.ToShortTimeString());
			}

			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.ReleaseBuild");

			var countOfBuilds = factory.GetDatabaseCount(typeof(ReleaseBuild));

			Assert(countOfBuilds == 0);

			Func<int, int, int, int, ReleaseBuild> getBuild = (major, minor, release, patch) =>
				 {
					 var newBuild = factory.New<ReleaseBuild>();
					 newBuild.HL_MajorVersion = major;
					 newBuild.HL_MinorVersion = minor;
					 newBuild.HL_Release = release;
					 newBuild.HL_Patch = patch;
					 newBuild.HL_ReleaseStatus = releaseRing;
					 return newBuild;
				 };

			var build1001 = getBuild(1, 0, 0, 1);
			var build1002 = getBuild(1, 0, 0, 2);

			build1001.HL_Superceded = !isTheFirstBuildActive;
			build1002.HL_Superceded = !isTheLastBuildActive;

			build1001.HL_ExeVersionDate = !isTheFirstBuildAfterCutOff
											? lastCutOffDate.AddMinutes(-2)
											: lastCutOffDate.AddMinutes(1);

			build1002.HL_ExeVersionDate = !isTheLastBuildAfterCutOff
											? lastCutOffDate.AddMinutes(-1)
											: lastCutOffDate.AddMinutes(2);
			factory.Save();

			return new List<ReleaseBuild>(2)
			{
				build1001, build1002
			};
		}
	}
}
