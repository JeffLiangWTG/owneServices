using System;
using System.Collections.Generic;
using AppDomainWrappers.Net;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Semaphores.Common.Testing
{
	class UpgradeCheckerWithDummyUpgrade : UpgradeChecker
	{
		protected override DateTime ExecuteUpgradeCheck(bool isForegroundThread) { return DateTime.UtcNow; }
	}

	class UpgradeCheckerThatAlwaysChecks : UpgradeChecker
	{
		protected override bool IsItTimeToCheckForUpgrade(TimeSpan upgradeCheckDuration) { return true; }
	}

	public class TestUpgradeChecker : TestCase
	{
		class TransactionedTest : TransactionedTestCase
		{
			public void TestCheckForUpgrade()
			{
				var upgradeChecker = new UpgradeCheckerThatAlwaysChecks();
				var hearbeatDuration = TimeSpan.FromSeconds(1000);

				// DateTime Forthcoming Upgrade
				DataUtils.DropDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty);
				var upgradeDateTimeUtc = upgradeChecker.CheckForUpgrade(true, hearbeatDuration);
				Assert(upgradeDateTimeUtc == DateTime.MinValue);

				var upgradeRegValue = DateTime.UtcNow.AddMinutes(10);
				var upgradeRefreshedRegValue = DateTime.UtcNow;
				var regValue = string.Format("{0}|{1}", SqlFormatInfo.ToSqlDateTimeString(upgradeRegValue), SqlFormatInfo.ToSqlDateTimeString(upgradeRefreshedRegValue));
				DataUtils.DropDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty);
				DataUtils.SaveDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty, regValue);
				upgradeDateTimeUtc = upgradeChecker.CheckForUpgrade(true, hearbeatDuration);
				Assert(Math.Abs((upgradeDateTimeUtc - upgradeRegValue).TotalSeconds) < 1);

				//expired #1
				upgradeRegValue = DateTime.UtcNow.AddMinutes(-2);
				upgradeRefreshedRegValue = DateTime.UtcNow;

				regValue = string.Format("{0}|{1}", SqlFormatInfo.ToSqlDateTimeString(upgradeRegValue), SqlFormatInfo.ToSqlDateTimeString(upgradeRefreshedRegValue));
				DataUtils.DropDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty);
				DataUtils.SaveDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty, regValue);
				upgradeDateTimeUtc = upgradeChecker.CheckForUpgrade(true, hearbeatDuration);
				Assert(upgradeDateTimeUtc == DateTime.MinValue);

				//expired #2
				upgradeRegValue = DateTime.UtcNow.AddMinutes(10);
				upgradeRefreshedRegValue = DateTime.UtcNow.AddMinutes(-5);

				regValue = string.Format("{0}|{1}", SqlFormatInfo.ToSqlDateTimeString(upgradeRegValue), SqlFormatInfo.ToSqlDateTimeString(upgradeRefreshedRegValue));
				DataUtils.DropDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty);
				DataUtils.SaveDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty, regValue);
				upgradeDateTimeUtc = upgradeChecker.CheckForUpgrade(true, hearbeatDuration);
				Assert(upgradeDateTimeUtc == DateTime.MinValue);

				//invalid format
				DataUtils.DropDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty);
				DataUtils.SaveDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty, "ERR_FORMAT");
				upgradeDateTimeUtc = upgradeChecker.CheckForUpgrade(true, hearbeatDuration);
				Assert(upgradeDateTimeUtc == DateTime.MinValue);

				DataUtils.DropDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty);
			}

			public void TestCheckForUpgrade_OnSeparateConnection()
			{
				var upgradeChecker = new UpgradeCheckerThatAlwaysChecks();
				var hearbeatDuration = TimeSpan.FromSeconds(1000);

				var now = DateTime.UtcNow;
				var upgradeTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second)
					.AddMinutes(10);
				var upgradeRefreshedTime = now;
				var regValue = string.Format("{0}|{1}", SqlFormatInfo.ToSqlDateTimeString(upgradeTime), SqlFormatInfo.ToSqlDateTimeString(upgradeRefreshedTime));
				DataUtils.DropDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty);
				DataUtils.SaveDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty, regValue);
				// "false" as 1st parameter means CheckForUpgrade will use a new connection...
				var upgradeDateTimeUtc = upgradeChecker.CheckForUpgrade(false, hearbeatDuration);
				AssertDateTimeWithinOneSecond("CheckForUpgrade", upgradeTime, upgradeDateTimeUtc);
			}
		}

		[UseSnapshotProtection]
		class SnapshotTest : TestCase
		{
			public void TestCheckForUpgradeThrowsDatabaseUpgradedExceptionForBackground()
			{
				// Arrange
				var upgradeChecker = new UpgradeChecker();
				Db.Connection.ExecuteNonQuery(@"
UPDATE
	dbo.StmData
SET
	SD_BinaryValue = CONVERT(varbinary(max), CONVERT(nvarchar(max), CONVERT(int, CONVERT(nvarchar(max), SD_BinaryValue)) + 1))
WHERE
	SD_Name = @name;
",
					command => command.AddParameterBasedOnDbColumn("@name", "DATABASE_SCHEMA_VERSION", StmDataSchema.SD_Name));

				// Act
				// Assert
				AssertExceptionThrown<DatabaseUpgradedException>(
					() => _ = upgradeChecker.CheckForUpgrade(false, TimeSpan.Zero));
			}

			[ExpectNoExceptions]
			public void TestCheckForUpgradeThrowsDatabaseUpgradedExceptionForForeground()
			{
				// Arrange
				var domainData = new Dictionary<string, object>
				{
					{ "ServerName", Db.ServerName },
					{ "DatabaseName", Db.DatabaseName },
					{ "DatabaseMajorSchemaVersion", DbRegistry.DatabaseMajorSchemaVersion.LoadValue(Db.Connection) },
					{ "DatabaseMinorSchemaVersion", DbRegistry.DatabaseMinorSchemaVersion.LoadValue(Db.Connection) }
				};

				var appDomainWrapper = new AppDomainWrapper();
				_ = appDomainWrapper.RunActionInAppDomain(() =>
				{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
					var serverName = (string)AppDomain.CurrentDomain.GetData("ServerName");
					var databaseName = (string)AppDomain.CurrentDomain.GetData("DatabaseName");
					var majorVersion = (int)AppDomain.CurrentDomain.GetData("DatabaseMajorSchemaVersion");
					var minorVersion = (int)AppDomain.CurrentDomain.GetData("DatabaseMinorSchemaVersion");
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

						Db.InitializeDatabaseDetails(serverName, databaseName);

					var dbVersionsMock = new Mock<IDatabaseAspectVersions>();
					var serviceProviderMock = new Mock<IServiceProvider>();

					dbVersionsMock
						.Setup(x => x.SchemaVersion)
						.Returns(new VersionLabel(majorVersion, minorVersion + 1));
					serviceProviderMock
						.Setup(x => x.GetService(typeof(IDatabaseAspectVersions)))
						.Returns(dbVersionsMock.Object);

					using (GlobalServiceProvider.Configure(serviceProviderMock.Object))
					{
						var upgradeChecker = new UpgradeChecker();

						// Act
						// Assert
						AssertExceptionThrown<DatabaseUpgradedException>(
							() => _ = upgradeChecker.CheckForUpgrade(true, TimeSpan.Zero));
					}
				}, domainData);
			}

			public void TestCheckForUpgradeUsesNewAdminConnectionIfOriginalConnectionIsAdmin()
			{
				var loginName = ((IDbReconnectionHandling)Db.Connection).LoginName.QuoteName();
				using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
				{
					var connection = Db.Connection;
					try
					{
						connection.ExecuteNonQuery($"ALTER LOGIN {loginName} DISABLE");

						AssertNoExceptionThrown("Since connection is now admin yet, we should no longer be failing.", () => new UpgradeCheckerThatAlwaysChecks().CheckForUpgrade(false, TimeSpan.FromSeconds(1000)));
					}
					finally
					{
						connection.ExecuteNonQuery($"ALTER LOGIN {loginName} ENABLE");
					}
				}
			}
		}
	}
}
