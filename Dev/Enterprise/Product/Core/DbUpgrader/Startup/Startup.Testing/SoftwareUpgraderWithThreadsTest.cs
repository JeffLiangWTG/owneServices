using System;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class SoftwareUpgraderWithThreadsTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestReportIssueWhenFailingToUpgradeDatabase()
		{
			var mockInstance = new Mock<SoftwareUpgrader>();
			mockInstance.Setup(m => m.RunCurrentVersionWriter(It.IsAny<UpgradeInfo>(), It.IsAny<IUpgradeManager>()))
						.Callback(() => throw new InvalidOperationException("Invalid operation"));
			SoftwareUpgrader.OverridableInstance.Value = mockInstance.Object;
			ErrorReporter.Clear();

			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgradeManager = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, false, true);
			var log = new StringBuilder();
			upgradeManager.UpgradeEvent += (type, message) =>
			{
				log.Append(type).Append(" ").AppendLine(message);
			};

			try
			{
				upgradeManager.Run();
			}
			catch (DeveloperNotificationException)
			{ }

			var messageReported = ErrorReporter.LastMessageReported;
			var exceptionReported = ErrorReporter.LastExceptionReported;
			ErrorReporter.Clear();
			CombineAssertions(() =>
			{
				Assert($"An issue should be reported when failing to upgrade the database. LastMessageReported is '{messageReported}'{System.Environment.NewLine}Logs: {log}", messageReported.Contains("Failed to upgrade database"));
				AssertType<UpgradeManagerException>(exceptionReported);
				AssertType<InvalidOperationException>(exceptionReported.InnerException);
				mockInstance.VerifyAll();
			});
		}

		[UseSnapshotProtection]
		public void TestDontReportSqlLockLostExceptionFromUpgradeDatabase()
		{
			var mockInstance = new Mock<SoftwareUpgrader>();
			mockInstance.Setup(m => m.RunCurrentVersionWriter(It.IsAny<UpgradeInfo>(), It.IsAny<IUpgradeManager>()))
				.Callback(() => throw new Exception("Upgrading Failed", new SqlLockLostException(@"A db reconnect was attempted while undisposed SqlLocks existed. Details below:
SqlApplicationLock{Key: 'Online DB Upgrade', IsDisposed: False, WasAcquiredOnLastCheck: True}", Array.Empty<string>())));
			SoftwareUpgrader.OverridableInstance.Value = mockInstance.Object;

			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgradeManager = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, false, true);
			try
			{
				upgradeManager.Run();
			}
			catch (DeveloperNotificationException)
			{ }

			AssertNull("ErrorReporter.LastExceptionReported", ErrorReporter.LastExceptionReported);
			mockInstance.VerifyAll();
		}

		[UseSnapshotProtection]
		public void TestDoNotReportOnceServerRequirmentsNotMetAbortsUpgrade()
		{
			var mockInstance = new Mock<SoftwareUpgrader>();
			mockInstance.Setup(m => m.RunCurrentVersionWriter(It.IsAny<UpgradeInfo>(), It.IsAny<IUpgradeManager>()))
						.Callback(() => throw new ServerRequirementsNotMetException("Server requirements not met:"));
			SoftwareUpgrader.OverridableInstance.Value = mockInstance.Object;
			ErrorReporter.Clear();

			DoNotReportIssueWhenUserAbortsUpgrade();
		}

		void DoNotReportIssueWhenUserAbortsUpgrade()
		{
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgradeManager = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, false, true);
			try
			{
				upgradeManager.Run();
			}
			catch (DeveloperNotificationException)
			{ }

			AssertNull("No exception was reported", ErrorReporter.LastExceptionReported);
		}
	}
}
