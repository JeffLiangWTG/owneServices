using System;
using System.Collections.Generic;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using Enterprise.DbUpgrader.Assemblies;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	[UseSnapshotProtection]
	sealed class OffLineUpgradeObserverTest : TestCase
	{
		public void TestDbOfflineUpgraderLockTimeout()
		{
			var lockTimeoutBeforeOffline = -2;
			var lockTimeoutDuringOffline = -2;
			var lockTimeoutAfterOffline = -2;

			UpgradeManagerTest.SetupDBUpgraderUserInteractionMocks(out var mockServiceProvider, true, true);
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);

			var logger = new List<string>();
			upgrader.UpgradeEvent += (_, message) => logger.Add(message);

			upgrader.UserAction_BeforeOffline_ForTest = () =>
			{
				lockTimeoutBeforeOffline = UpgradeManagerTest.GetLockTimeout(Db.Connection);
			};

			upgrader.UserAction_Offline_ForTest = () =>
			{
				lockTimeoutDuringOffline = UpgradeManagerTest.GetLockTimeout(Db.Connection);
				throw new OperationCanceledException("Upgrade aborted by user");
			};

			upgrader.UserAction_AfterOffline_ForTest = () =>
			{
				lockTimeoutAfterOffline = UpgradeManagerTest.GetLockTimeout(Db.Connection);
			};

			using (UpgradeManagerTest.TemporarySetInitialLockTimeout(Db.Connection, 15_000))
			using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
			{
				AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
					
				var fullLog = string.Join(System.Environment.NewLine, logger);
				AssertContains("Should change LockTimeout to Infinite"
					, expected: "Switching Offline LockTimeout to Infinite"
					, actualContainingExpected: fullLog);
				AssertContains("Should restore LockTimeout"
					, expected: "Restoring Offline LockTimeout"
					, actualContainingExpected: fullLog);

				AssertEquals("LockTimout Before Offline", 15_000, lockTimeoutBeforeOffline);
				AssertEquals("LockTimout During Offline", DbConnection.LockTimeout.Infinite, lockTimeoutDuringOffline);
				AssertEquals("LockTimout Aftere Offline", 15_000, lockTimeoutAfterOffline);
			}
		}

		public void TestUpgradeExceptionWithInternalExceptionIsReported()
		{
			// Arrange
			var upgradeInfo = new UpgradeInfo(new Guid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var versionChangeInfoMock = Mock.Of<IVersionChangeInfo>(info =>
				info.DbReferenceVersion_Schema == SchemaVersion.Application
				&& info.DbReferenceVersion_Clr == SqlClrAssembliesVersion.Application
				&& info.DbReferenceVersion_Script == new VersionLabel(5767, 0)
				&& info.DbReferenceVersion_Transformation == new VersionLabel(0, 0));

			CombineAssertions(() =>
			{
				Test(new Exception());
				Test(new InvalidOperationException());
				Test(new AccessViolationException());
			});

			void Test(Exception exception)
			{
				var upgrader = new UpgradeManagerForTest(versionChangeInfoMock, upgradeInfo, false, false);
				upgrader.UserAction_BeforeOffline_ForTest += () => throw exception;

				// Act
				AssertNoExceptionThrown(() => upgrader.Run());

				// Assert
				AssertType<UpgradeManagerException>(ErrorReporter.LastExceptionReported);
				AssertEquals(exception, ErrorReporter.LastExceptionReported.InnerException);
				ErrorReporter.Clear();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			BiServers.ClearBiServersCache();
		}
	}
}
