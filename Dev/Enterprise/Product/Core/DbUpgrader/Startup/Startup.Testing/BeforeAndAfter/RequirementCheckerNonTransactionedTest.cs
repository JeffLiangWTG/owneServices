using System.Globalization;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class RequirementCheckerNonTransactionedTest : TestCase
	{
		public void TestCheckServerVersion()
		{
			var max = SqlServerVersionNumber.SupportedVersions.Max();

			CombineAssertions(() =>
			{
				foreach (var version in SqlServerVersionNumber.SupportedVersions)
				{
					Test(version.ToString(), version != max);
				}
			});

			void Test(string version, bool expectingWarning)
			{
				// Arrange
				using (Db.Connection.SetSqlServerVersionForTest(version))
				{
					var upgradeManager = new Mock<IUpgradeManager>(MockBehavior.Strict);
					upgradeManager.Setup(x => x.ShowInfoMessage(It.IsAny<string>()));

					var requirementChecker = new Mock<RequirementChecker>(new[] { Db.DatabaseName }, new[] { Db.DatabaseName }) { CallBase = true };

					// Act
					requirementChecker.Object.CheckServerVersion(Db.Connection, upgradeManager.Object);

					// Assert
					if (expectingWarning)
					{
						upgradeManager.Verify(
							x => x.ShowInfoMessage(
								$"It is recommended to update your version of SQL Server to {max.GetLongDescription()} or higher.\r\n" +
								"WiseTech Global intends to increase the minimum required version of SQL Server, 12 months after the RTM.\r\n" +
								"To plan for this, upgrade your SQL Server version as soon as is practical."), Times.Once());
					}

					upgradeManager.VerifyNoOtherCalls();
					Assert(true);
				}
			}
		}

		public void TestCheckSqlServerGeneration()
		{
			var checker = new MainServerRequirementCheckerForTest();

			AssertNoExceptionThrown(
				"No exception should be thrown because the running SQL Server version is supported.",
				() => checker.CheckSqlServerGeneration_Exposed(Db.Connection));

			using (Db.Connection.SetSqlServerVersionForTest("0.00.0000.0"))
			{
				AssertExceptionThrown(
					"An exception should be thrown because the SQL Server version is not supported.",
					typeof(ServerRequirementsNotMetException),
					() => checker.CheckSqlServerGeneration_Exposed(Db.Connection));
			}
		}

		public void TestCheckSqlServerGenerationForNotYetSupportedSqlServer()
		{
			// Arrange
			var checker = new MainServerRequirementCheckerForTest();

			using (Db.Connection.SetSqlServerVersionForTest("99.00.0000.0"))
			{
				// Act
				// Assert
				AssertNoExceptionThrown(
					"An exception should be thrown because the SQL Server version is not supported.",
					() => checker.CheckSqlServerGeneration_Exposed(Db.Connection));
			}
		}

		public void TestCheckSqlMinimumRequiredAndPreferredVersions()
		{
			var checker = new MainServerRequirementCheckerForTest();
			var logger = new DummyLoggerForTest();

			var latestSupportedVersion = SqlServerVersionNumber.SupportedVersions.Max();
			foreach (var version in SqlServerVersionNumber.SupportedVersions)
			{
				logger.Logs.Clear();

				var justBelowMinVersion = $"{version.Major}.{version.Minor}.{version.Build - 1}.99";

				using (Db.Connection.SetSqlServerVersionForTest(justBelowMinVersion))
				{
					AssertExceptionThrown(
						"An exception should be thrown because the SQL Server version is not supported.",
						typeof(ServerRequirementsNotMetException),
						() => checker.CheckServicePackVersions_Exposed(Db.Connection, logger));
				}

				AssertEquals(0, logger.Logs.Count);
				using (Db.Connection.SetSqlServerVersionForTest(version.ToString()))
				{
					AssertNoExceptionThrown(
						"No exception should be thrown because the running SQL Server version is supported.",
						() => checker.CheckServicePackVersions_Exposed(Db.Connection, logger));

					if (version != latestSupportedVersion)
					{
						AssertEquals(1, logger.Logs.Count);
						AssertContains($"It is recommended to update your version of SQL Server to {latestSupportedVersion.GetLongDescription()} or higher.", logger.Logs[0]);
					}
					else
					{
						AssertEquals(0, logger.Logs.Count);
					}

					logger.Logs.Clear();
				}
			}
		}

		public void TestValidateDbName()
		{
			var testChecker = new MainServerRequirementCheckerForTest();

			// Current main database. No exception expected.
			testChecker.ValidateDbName_Exposed(Db.DatabaseName);

			var longDbName = "ADatabaseNameMoreThan35CharactersLong";

			AssertExceptionThrown(
				typeof(OdysseyDataException),
				string.Format(CultureInfo.InvariantCulture,
					"Main database must not be longer than 35 characters.\r\n\r\nDatabase: {0}",
					longDbName),
				() => testChecker.ValidateDbName_Exposed(longDbName));

			// Database name does not start with a letter. Exception expected.
			AssertValidateDbNameThrowsException(testChecker, "_ADbNameNotStartingWithALetter");

			// Database with letters and underscore. No exception expected.
			AssertValidateDbNameThrowsException(testChecker, "Odyssey_Whatever");

			// Database name with non-alphanumeric characters. Exception expected.
			AssertValidateDbNameThrowsException(testChecker, "A_Db!Name@With#Lots$Of%Crap^In&It*");

			// Database with letters and numbers. No exception expected.
			AssertNoExceptionThrown(() => testChecker.ValidateDbName_Exposed("ADbNameWithOnlyLettersAndNumbers123"));
		}

		void AssertValidateDbNameThrowsException(MainServerRequirementCheckerForTest testChecker, string dbName)
		{
			AssertExceptionThrown(
				typeof(OdysseyDataException),
				string.Format(CultureInfo.InvariantCulture,
					"Main database name must only contain alphanumeric characters, starting with a letter.\r\n\r\nDatabase: {0}",
					dbName),
				() => testChecker.ValidateDbName_Exposed(dbName));
		}

		public void TestGetLowDiskSpaceList()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var testChecker = new MainServerRequirementCheckerForTest();
				var lowDiskSpaceDrives = testChecker.GetLowDiskSpaceList(adminConnection);
				AssertEquals("Is list empty?", false, lowDiskSpaceDrives.Length == 0);
			}
		}
	}
}
