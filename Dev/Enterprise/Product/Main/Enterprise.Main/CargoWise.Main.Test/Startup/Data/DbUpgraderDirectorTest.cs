using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class DbUpgraderDirectorTest : AbstractApplicationStartupTaskTest<DbUpgraderDirector>
	{
		public void TestGetVersionChangeText_ContainsRelevantSections()
		{
			var director = new DbUpgraderDirector();
			var changeText = director.GetVersionChangeText();
			AssertContains("Release: ", changeText);
			AssertContains("Schema Version: ", changeText);
			AssertContains("Script Version: ", changeText);
			AssertContains("Data Version: ", changeText);
			AssertContains("Transformation Version: ", changeText);
			AssertContains("(no update required)", changeText);
			AssertNotContains("Numbers should not be formatted with comma separators", ",", changeText);
		}

		public void TestIsDocManagerUpgradeIncluded()
		{
			Env.Registry.DatabaseMajorSchemaVersion = SchemaVersion.DocManager.Major - 1;
			Env.Registry.DatabaseMinorSchemaVersion = SchemaVersion.DocManager.Minor - 1;
			var director = new DbUpgraderDirector();
			director.CheckAndUpgradeDb(); // should never actually upgrade in testing, the method needs to be called to set internal fields
			AssertEquals("Should return true for IsDocManagerUpgrade included - the previous version was one before the DocManager upgrade", true, director.IsDocManagerSchemaUpgradeIncluded);

			Env.Registry.DatabaseMajorSchemaVersion = SchemaVersion.DocManager.Major;
			Env.Registry.DatabaseMinorSchemaVersion = SchemaVersion.DocManager.Minor;
			director = new DbUpgraderDirector();
			director.CheckAndUpgradeDb(); // should never actually upgrade in testing, the method needs to be called to set internal fields
			AssertEquals("Should return false for IsDocManagerUpgrade included - already at that version", false, director.IsDocManagerSchemaUpgradeIncluded);

			Env.Registry.DatabaseMajorSchemaVersion = SchemaVersion.Application.Major;
			Env.Registry.DatabaseMinorSchemaVersion = SchemaVersion.Application.Minor;
			director = new DbUpgraderDirector();
			director.CheckAndUpgradeDb(); // should never actually upgrade in testing, the method needs to be called to set internal fields
			AssertEquals("Should return false for IsDocManagerUpgrade included - already past that version", false, director.IsDocManagerSchemaUpgradeIncluded);
		}

		[TestDate(2020, 3, 31, 13, 52, 37)]
		public void TestRunningSimultaneousUpgradesDoNotUseSameLogFiles()
		{
			DeleteTestData();
			var director1 = new DbUpgraderDirector();
			var director2 = new DbUpgraderDirector();
			var director3 = new DbUpgraderDirector();

			try
			{
				var logFileName = director1.UpgradeLogFilePath;
				AssertEquals($"Filename should be {logFileName.Substring(0, logFileName.Length - 4) + " (1).LOG"} for second log", logFileName.Substring(0, logFileName.Length - 4) + " (1).LOG", director2.UpgradeLogFilePath);
				AssertEquals($"Filename should be {logFileName.Substring(0, logFileName.Length - 4) + " (2).LOG"} for third log", logFileName.Substring(0, logFileName.Length - 4) + " (2).LOG", director3.UpgradeLogFilePath);
			}
			finally
			{
				DeleteIfExists(director1.UpgradeLogFilePath);
				DeleteIfExists(director2.UpgradeLogFilePath);
				DeleteIfExists(director3.UpgradeLogFilePath);
			}
		}

		public void DeleteTestData()
		{
			var director = new TestDbUpgraderDirector();
			var rootFolderPath = director.UpgradeLogDirectory;
			var filesToDelete = @"*20200331_135237*.LOG";
			string[] fileList = Directory.GetFiles(rootFolderPath, filesToDelete);
			foreach (string file in fileList)
			{
				File.Delete(file);
			}
			fileList = Directory.GetFiles(rootFolderPath, filesToDelete);
			AssertEquals("There should be no test files present", fileList.Length, 0);
		}

		public void TestCheckAndUpgradeDbFailsWhenDeletedPendingTasksExist()
		{
			var provider = new Mock<IOnlineTransformationProvider>(MockBehavior.Strict);
			provider.Setup(m => m.GetRunningTasks()).Returns(Enumerable.Empty<IOnlineTransformation>());
			provider.Setup(m => m.DeletedPendingTasks).Returns(new[] { "Abc", "123" });

			var director = new Mock<TestDbUpgraderDirector>(new ValidationResponse() { Successful = true }, true) { CallBase = true };
			director.Setup(m => m.ShowDenyUpgradeMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Callback(() =>
				{
					director.Setup(m => m.ShowDenyUpgradeMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).CallBase();
				});

			Env.Registry.DatabaseMinorSchemaVersion = Env.Registry.DatabaseMinorSchemaVersion - 1;

			using (ObjectFactory.Substitute(provider.Object))
			{
				AssertEquals(false, director.Object.CheckAndUpgradeDb().Successful);
				director.Verify(d => d.ShowDenyUpgradeMessage("The application will be terminated.\r\nThe Online Data Transformation Service (ODT) Service Task must run to completion before the upgrade can be completed.\r\nThe following online transformations have yet to complete:\r\n\tAbc\r\n\t123", "Pending deleted transformations exist:\r\n\r\n\tAbc\r\n\t123", null));
			}
		}

		public void TestCheckAndUpgradeDbSucceedsWhenNoDeletedPendingTasksExist()
		{
			var provider = new Mock<IOnlineTransformationProvider>(MockBehavior.Strict);
			provider.Setup(m => m.GetRunningTasks()).Returns(Enumerable.Empty<IOnlineTransformation>());
			provider.Setup(m => m.DeletedPendingTasks).Returns(Array.Empty<string>());

			var director = new TestDbUpgraderDirector(new ValidationResponse() { Successful = true }, true);

			Env.Registry.DatabaseMinorSchemaVersion = Env.Registry.DatabaseMinorSchemaVersion - 1;

			using (ObjectFactory.Substitute(provider.Object))
			{
				Assert(director.CheckAndUpgradeDb().Successful);
			}
		}

		public void TestCheckForUpgrade()
		{
			var result = new ValidationResponse();
			result.Successful = true;
			var director = new TestDbUpgraderDirector(result, true);
			AssertEquals("PreCondition: Should not indicate upgrade required", false, director.IsDbUpgradeRequired());
			Assert("PreCondition: Should not need to perform upgrade", director.CheckAndUpgradeDb().Successful);
			Env.Registry.DatabaseMinorSchemaVersion = Env.Registry.DatabaseMinorSchemaVersion - 1;

			var upgradingDirector = new TestDbUpgraderDirector(result, true);
			AssertEquals("Should indicate that an upgrade is required", true, upgradingDirector.IsDbUpgradeRequired());
			Assert("Should indicate upgrade is okay", upgradingDirector.CheckAndUpgradeDb().Successful);
		}

		public void TestUpgradeWithClientDocuments()
		{
			var softwareUpgrade = new UpgradeInfo(new Guid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				var result = new ValidationResponse();
				result.Successful = true;

				var director = new TestDbUpgraderDirector(result, true);
				director.SetSoftwareUpgrade(softwareUpgrade);
				AssertEquals("Should not indicate upgrade required", false, director.IsDbUpgradeRequired());

				DataRegistry.Instance.ClientDocumentName = "ABC";

				director = new TestDbUpgraderDirector(result, true);
				AssertEquals("Should not indicate upgrade required", false, director.IsDbUpgradeRequired());

				director = new TestDbUpgraderDirector(result, true);
				director.SetSoftwareUpgrade(softwareUpgrade);
				AssertEquals("Should indicate upgrade required", true, director.IsDbUpgradeRequired());
			}
		}

		public void TestSendVersionReport()
		{
			var mockSender = new Mock<Integration.IVersionUpgradeReportSender>();
			ObjectFactory.Substitute(mockSender.Object);
			mockSender.SetupSequence(m => m.Send())
				.Pass()
				.Throws(new InvalidOperationException("simulate send exception"));

			using (Env.SetTemporaryUserContext(null))
			{
				var result = new ValidationResponse();
				result.Successful = true;

				AssertNull(Env.CurrentBranch);

				var director = new TestDbUpgraderDirector(result, true);
				director.SendVersionReport();

				director.SendVersionReport();
				AssertEquals("SendVersionReport", ErrorReporter.LastMessageReported);
			}

			mockSender.VerifyAll();
			ErrorReporter.Clear();
		}

		public override int DefaultErrorExitCode => ExitCodes.DbUpgraderDirectorFailure;

		public class NewTest : TestCase
		{
			public void TestWorksOnNotInitializedEnvironmentForDbUpgraderDirector()
			{
				// Arrange
				using (Db.ClearServerDetailsTemporarily())
				{
					var commandLineArguments = new CommandLineArguments(
						new[]
						{
							"dbServer",
							"dbName",
							"ModuleName",
						},
						new Hashtable(new Dictionary<string, object>
						{
							[ApplicationArguments.OptionTestAdapter] = false,
							[ApplicationArguments.OptionConsoleUpgrader] = false,
							[ApplicationArguments.OptionScheduledDbUpgrader] = false,
						}));

					// Act
					var result = DbUpgraderDirector.New(commandLineArguments);

					// Assert
					AssertType<DbUpgraderDirector>(result);
				}
			}

			public void TestWorksOnNotInitializedEnvironmentForScheduledUpgraderDirector()
			{
				// Arrange
				using (Db.ClearServerDetailsTemporarily())
				{
					var commandLineArguments = new CommandLineArguments(
						new[]
						{
							"dbServer",
							"dbName",
							"ModuleName",
						},
						new Hashtable(new Dictionary<string, object>
						{
							[ApplicationArguments.OptionTestAdapter] = false,
							[ApplicationArguments.OptionConsoleUpgrader] = false,
							[ApplicationArguments.OptionNotifyOnSuccessfulUpgrade] = false,
							[ApplicationArguments.NotificationGroupPK] = string.Empty,
							[ApplicationArguments.OptionScheduledDbUpgrader] = true,
						}));

					// Act
					var result = DbUpgraderDirector.New(commandLineArguments);

					// Assert
					AssertType<ScheduledUpgraderDirector>(result);
				}
			}
		}
	}
}
