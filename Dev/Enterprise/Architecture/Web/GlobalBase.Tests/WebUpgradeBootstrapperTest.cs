using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWiseOne.WebInfrastructure;
using Enterprise.ZArchitecture.Web.Shared.Test;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.ZArchitecture.Web.GlobalBase.Tests
{
	[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
	class WebUpgradeBootstrapperTest : TestCase
	{
		[ExpectNoExceptions]
		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestWebUpgradeBootstrapperStartedOnApplicationStart()
		{
			// Arrange
			void DoCallback()
			{
				var enterpriseGlobalMock = new Mock<ZEnterpriseGlobal> { CallBase = true };
				enterpriseGlobalMock.Protected().Setup("InitializeServerAndDatabaseNames").Verifiable();
				enterpriseGlobalMock.Protected().Setup("InitializeWebUpgradeBootstrapper").Verifiable();

				// Act
				using (var enterpriseGlobal = enterpriseGlobalMock.Object)
				using (enterpriseGlobal.StartApplicationDisposable())
				{
				}

				// Assert
				AssertNoExceptionThrown(() => enterpriseGlobalMock.Verify());
			}

			using (TestHttpContextHelper.DisposableAppDomain(appDomain => { }, DoCallback))
			{ }
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestWebUpgradeManagerDelayStartedOnDatabaseUpgradeInProgressException()
		{
			TestWebUpgradeManagerOnDatabaseUpgradeException(updateSchemaVersion: false);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestWebUpgradeManagerStartedOnDatabaseUpgradedException()
		{
			TestWebUpgradeManagerOnDatabaseUpgradeException(updateSchemaVersion: true);
		}

		void TestWebUpgradeManagerOnDatabaseUpgradeException(bool updateSchemaVersion)
		{
			// Arrange
			const string expectedEventLogMessage = "ZEnterpriseGlobal Error GetSqlConnection";
			var upgradeInProgressEvent = new ManualResetEventSlim(false);
			var exitEvent = new ManualResetEventSlim(false);
			var timeAppStarted = DateTime.Now;
			var upgradeThread = new Thread(() =>
			{
				using (Db.DisposableUpgrade_ForTest(acquireLockOut: true, killOtherConnections: true, updateSchemaVersion: updateSchemaVersion))
				{
					upgradeInProgressEvent.Set();
					while (!exitEvent.Wait(1)
						&& !GetEventLogs(timeAppStarted).Any(x => x.StartsWith(expectedEventLogMessage)))
					{
						Thread.Sleep(100);
					}
				}
			});

			try
			{
				upgradeThread.Start();
				upgradeInProgressEvent.Wait();

				using (TestHttpContextHelper.DisposableAppDomain(appDomain => { }, () =>
				{
					var enterpriseGlobalMock = new Mock<ZEnterpriseGlobal> { CallBase = true };
					WebUpgradeManager upgradeManager = null;

					// Act
					var webUpgradeManagerStartedEvent = new ManualResetEventSlim(false);
					using (WebUpgradeBootstrapper.DisposableWebUpgradeManagerStartedAction_ForTest((webUpgradeManager) =>
					{
						upgradeManager = webUpgradeManager;
						webUpgradeManagerStartedEvent.Set();
					}))
					using (var enterpriseGlobal = enterpriseGlobalMock.Object)
					using (enterpriseGlobal.StartApplicationDisposable())
					{
						webUpgradeManagerStartedEvent.Wait();
					}
				})) { }
			}
			finally
			{
				exitEvent.Set();
				upgradeThread.Join();
			}

			// Assert
			var eventLogs = GetEventLogs(timeAppStarted);
			AssertEquals(false, eventLogs.Any(x => x.StartsWith($"WebUpgradeBootstrapper CreateAndStartWebUpgradeManagerAsync Error on: {Db.ServerName} {Db.DatabaseName}")));
		}

		static IEnumerable<string> GetEventLogs(DateTime? dateTimeSince, string source = WebInfrastructureConstants.EventSourceName)
		{
			var eventLogs = new List<string>();
			var eventsQuery = new EventLogQuery(
				"Application",
				PathType.LogName,
				$@"
*[
	System/Provider/@Name='{source}'
	and
	System/TimeCreated/@SystemTime >= '{dateTimeSince?.ToUniversalTime():o}'
	and
	System/TimeCreated/@SystemTime <= '{DateTime.Now.ToUniversalTime():o}'
]");
			using (var eventLogReader = new EventLogReader(eventsQuery))
			{
				var eventRecord = eventLogReader.ReadEvent();

				while (eventRecord != null)
				{
					eventLogs.Add(Invariant($"{eventRecord.FormatDescription()}"));
					eventRecord = eventLogReader.ReadEvent();
				}

				return eventLogs;
			}
		}
	}
}
