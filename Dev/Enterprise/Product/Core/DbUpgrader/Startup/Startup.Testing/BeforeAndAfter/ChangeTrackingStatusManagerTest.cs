using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class ChangeTrackingStatusManagerTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestEnableChangeTrackingDoesNotThrowExceptionAndDoesNotAffectUpgrade() => ChangeTrackingExceptionHandling(
			isEnable: true,
			testSetupAction: null,
			expectedMessages: new[] { "--- Enable Change Tracking - START ---", "--- Enable Change Tracking - END   ---" },
			unexpectedMessages: new[] { "Skipping [Enable Change Tracking] as it is turned off in the registry.", "EnableChangeTracking.SQL Resource Stream is null" });

		[UseSnapshotProtection]
		public void TestDisableChangeTrackingDoesNotThrowExceptionAndDoesNotAffectUpgrade() => ChangeTrackingExceptionHandling(
			isEnable: false,
			testSetupAction: null,
			expectedMessages: new[] { "--- Disable Change Tracking - START ---", "--- Disable Change Tracking - END   ---" },
			unexpectedMessages: new[] { "DisableChangeTracking.SQL Resource Stream is null" });

		[UseSnapshotProtection]
		public void TestEnableChangeTrackingThrowsInvalidOperationExceptionAndDoesNotAffectUpgrade() => ChangeTrackingExceptionHandling(
			isEnable: true,
			testSetupAction: () => throw new InvalidOperationException("EnableChangeTracking.SQL Resource Stream is null"),
			expectedMessages: new[] { "--- Enable Change Tracking - START ---", "Enabling Change Tracking encountered error : EnableChangeTracking.SQL Resource Stream is null" },
			unexpectedMessages: new[] { "--- Enable Change Tracking - END   ---" },
			expectedErrorMessage: "Enabling Change Tracking encountered error : EnableChangeTracking.SQL Resource Stream is null");

		[UseSnapshotProtection]
		public void TestDisableChangeTrackingThrowsInvalidOperationExceptionAndDoesNotAffectUpgrade() => ChangeTrackingExceptionHandling(
			isEnable: false,
			testSetupAction: () => throw new InvalidOperationException("DisableChangeTracking.SQL Resource Stream is null"),
			expectedMessages: new[] { "--- Disable Change Tracking - START ---", "Disabling Change Tracking encountered error : DisableChangeTracking.SQL Resource Stream is null" },
			unexpectedMessages: new[] { "--- Disable Change Tracking - END   ---" },
			expectedErrorMessage: "Disabling Change Tracking encountered error : DisableChangeTracking.SQL Resource Stream is null");

		[UseSnapshotProtection]
		public void TestEnableChangeTrackingThrowsSqlExceptionAndDoesNotAffectUpgrade() => ChangeTrackingExceptionHandling(
			isEnable: true,
			testSetupAction: () =>
			{
				var error = SqlExceptionBuilder.CreateSqlError(1718, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Change tracking must be enabled on database 'DBNAME' before it can be enabled on table 'TABLENAME'.", "", 0);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var sqlException = SqlExceptionBuilder.CreateSqlException(errors);
				throw sqlException;
			},
			expectedMessages: new[] { "--- Enable Change Tracking - START ---", "Enabling Change Tracking encountered error : Change tracking must be enabled on database 'DBNAME' before it can be enabled on table 'TABLENAME'." },
			unexpectedMessages: new[] { "--- Enable Change Tracking - END   ---" },
			expectedErrorMessage: "Enabling Change Tracking encountered error : Change tracking must be enabled on database 'DBNAME' before it can be enabled on table 'TABLENAME'.");

		[UseSnapshotProtection]
		public void TestDisableChangeTrackingThrowsSqlExceptionAndDoesNotAffectUpgrade() => ChangeTrackingExceptionHandling(
			isEnable: false,
			testSetupAction: () =>
			{
				var error = SqlExceptionBuilder.CreateSqlError(1718, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Change tracking must be enabled on database 'DBNAME' before it can be disabled on table 'TABLENAME'.", "", 0);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var sqlException = SqlExceptionBuilder.CreateSqlException(errors);
				throw sqlException;
			},
			expectedMessages: new[] { "--- Disable Change Tracking - START ---", "Disabling Change Tracking encountered error : Change tracking must be enabled on database 'DBNAME' before it can be disabled on table 'TABLENAME'." },
			unexpectedMessages: new[] { "--- Disable Change Tracking - END   ---" },
			expectedErrorMessage: "Disabling Change Tracking encountered error : Change tracking must be enabled on database 'DBNAME' before it can be disabled on table 'TABLENAME'.");

		void ChangeTrackingExceptionHandling(bool isEnable, Action testSetupAction, string[] expectedMessages, string[] unexpectedMessages, string expectedErrorMessage = "")
		{
			try
			{
				using (var connection = Db.NewAdminConnection())
				{
					string exceptionMessage = "";
					var logger = new DummyLoggerForTest();

					var changeTrackingStatusManager = new ChangeTrackingStatusManagerForTesting(testSetupAction);
					changeTrackingStatusManager.SetChangeTrackingStatus_Exposed(connection, logger, isEnable, ref exceptionMessage);

					AssertMessages(logger.Logs, expectedMessages, unexpectedMessages);
					AssertEquals("Error Message", expectedErrorMessage, exceptionMessage);
				}
			}
			finally
			{
				ObjectFactory.DisposeSubstitutions();
			}
		}

		[RequiresLargeLogFile]
		public void TestReEnableChangeTrackToEnsureDataCleanup_GlowEnabled_CdcEnabled_NotAllowCTWhenCDCEnabled() => ReEnableChangeTrackToEnsureDataCleanup_ExceptionHandling(
			isGlowEnabled: true,
			isCdcEnabled: true,
			allowCTWhenCDCEnabled: false,
			expectedMessages: new[] { "--- Disable Change Tracking - START ---", "--- Disable Change Tracking - END   ---", "Skipping [Enable Change Tracking] as it is not allowed by the key [AllowCTWhenCDCEnabled]." },
			unexpectedMessages: new[] { "--- Enable Change Tracking - START ---", "--- Enable Change Tracking - END   ---", "Skipping [Enable Change Tracking] as Glow Service Uri is not set." });

		[RequiresLargeLogFile]
		public void TestReEnableChangeTrackToEnsureDataCleanup_GlowEnabled_CdcEnabled_AllowCTWhenCDCEnabled() => ReEnableChangeTrackToEnsureDataCleanup_ExceptionHandling(
			isGlowEnabled: true,
			isCdcEnabled: true,
			allowCTWhenCDCEnabled: true,
			expectedMessages: new[] { "--- Enable Change Tracking - START ---", "--- Enable Change Tracking - END   ---" },
			unexpectedMessages: new[] { "Skipping [Enable Change Tracking] as it is not allowed by the key [AllowCTWhenCDCEnabled].", "Skipping [Enable Change Tracking] as Glow Service Uri is not set." });

		[RequiresLargeLogFile]
		public void TestReEnableChangeTrackToEnsureDataCleanup_GlowEnabled_NoCdcEnabled_AllowCTWhenCDCEnabled() => ReEnableChangeTrackToEnsureDataCleanup_ExceptionHandling(
			isGlowEnabled: true,
			isCdcEnabled: false,
			allowCTWhenCDCEnabled: true,
			expectedMessages: new[] { "--- Enable Change Tracking - START ---", "--- Enable Change Tracking - END   ---" },
			unexpectedMessages: new[] { "Skipping [Enable Change Tracking] as it is not allowed by the key [AllowCTWhenCDCEnabled].", "Skipping [Enable Change Tracking] as Glow Service Uri is not set." });

		[RequiresLargeLogFile]
		public void TestReEnableChangeTrackToEnsureDataCleanup_GlowEnabled_NoCdcEnabled_NotAllowCTWhenCDCEnabled() => ReEnableChangeTrackToEnsureDataCleanup_ExceptionHandling(
			isGlowEnabled: true,
			isCdcEnabled: false,
			allowCTWhenCDCEnabled: false,
			expectedMessages: new[] { "--- Enable Change Tracking - START ---", "--- Enable Change Tracking - END   ---" },
			unexpectedMessages: new[] { "Skipping [Enable Change Tracking] as it is not allowed by the key [AllowCTWhenCDCEnabled].", "Skipping [Enable Change Tracking] as Glow Service Uri is not set." });

		[RequiresLargeLogFile]
		public void TestReEnableChangeTrackToEnsureDataCleanup_NoGlowEnabled_CdcEnabled_NotAllowCTWhenCDCEnabled() => ReEnableChangeTrackToEnsureDataCleanup_ExceptionHandling(
			isGlowEnabled: false,
			isCdcEnabled: true,
			allowCTWhenCDCEnabled: false,
			expectedMessages: new[] { "--- Disable Change Tracking - START ---", "--- Disable Change Tracking - END   ---", "Skipping [Enable Change Tracking] as Glow Service Uri is not set." },
			unexpectedMessages: new[] { "--- Enable Change Tracking - START ---", "--- Enable Change Tracking - END   ---", "Skipping [Enable Change Tracking] as it is not allowed by the key [AllowCTWhenCDCEnabled]." });

		[RequiresLargeLogFile]
		public void TestReEnableChangeTrackToEnsureDataCleanup_NoGlowEnabled_CdcEnabled_AllowCTWhenCDCEnabled() => ReEnableChangeTrackToEnsureDataCleanup_ExceptionHandling(
			isGlowEnabled: false,
			isCdcEnabled: true,
			allowCTWhenCDCEnabled: true,
			expectedMessages: new[] { "--- Disable Change Tracking - START ---", "--- Disable Change Tracking - END   ---", "Skipping [Enable Change Tracking] as Glow Service Uri is not set." },
			unexpectedMessages: new[] { "--- Enable Change Tracking - START ---", "--- Enable Change Tracking - END   ---", "Skipping [Enable Change Tracking] as it is not allowed by the key [AllowCTWhenCDCEnabled]." });

		[RequiresLargeLogFile]
		public void TestReEnableChangeTrackToEnsureDataCleanup_NoGlowEnabled_NoCdcEnabled_AllowCTWhenCDCEnabled() => ReEnableChangeTrackToEnsureDataCleanup_ExceptionHandling(
			isGlowEnabled: false,
			isCdcEnabled: false,
			allowCTWhenCDCEnabled: true,
			expectedMessages: new[] { "--- Disable Change Tracking - START ---", "--- Disable Change Tracking - END   ---", "Skipping [Enable Change Tracking] as Glow Service Uri is not set." },
			unexpectedMessages: new[] { "--- Enable Change Tracking - START ---", "--- Enable Change Tracking - END   ---", "Skipping [Enable Change Tracking] as it is not allowed by the key [AllowCTWhenCDCEnabled]." });

		[RequiresLargeLogFile]
		public void TestReEnableChangeTrackToEnsureDataCleanup_NoGlowEnabled_NoCdcEnabled_NotAllowCTWhenCDCEnabled() => ReEnableChangeTrackToEnsureDataCleanup_ExceptionHandling(
			isGlowEnabled: false,
			isCdcEnabled: false,
			allowCTWhenCDCEnabled: false,
			expectedMessages: new[] { "--- Disable Change Tracking - START ---", "--- Disable Change Tracking - END   ---", "Skipping [Enable Change Tracking] as Glow Service Uri is not set." },
			unexpectedMessages: new[] { "--- Enable Change Tracking - START ---", "--- Enable Change Tracking - END   ---", "Skipping [Enable Change Tracking] as it is not allowed by the key [AllowCTWhenCDCEnabled]." });

		void ReEnableChangeTrackToEnsureDataCleanup_ExceptionHandling(bool isGlowEnabled, bool isCdcEnabled, bool allowCTWhenCDCEnabled, string[] expectedMessages, string[] unexpectedMessages)
		{
			try
			{
				var testSystemDataRegistry = SystemDataRegistryForTest.Get();
				testSystemDataRegistry.AllowCTWhenCDCEnabled = allowCTWhenCDCEnabled;
				var testGlowRegistry = GlowRegistryForTest.Get();
				testGlowRegistry.GlowServiceUri = isGlowEnabled ? "https://TEST/Glow/" : "/";

				using (var connection = Db.NewAdminConnection())
				{
					CdcDatabase.Disable(connection, Db.DatabaseName);
					if (isCdcEnabled)
					{
						CdcDatabase.Enable(connection, Db.DatabaseName);
						var cdcSupport = new CdcSupport(new DummyUpgradeManager(), Db.DatabaseName, connection);
						cdcSupport.SynchroniseCdcSchema();
					}

					string exceptionMessage = "";
					var dummyUpgradeManager = new UpgradeManagerForTestWithOutputBuffer();

					var changeTrackingStatusManager = new ChangeTrackingStatusManager();
					changeTrackingStatusManager.ReEnableChangeTrackToEnsureDataCleanup(connection, dummyUpgradeManager, ref exceptionMessage);

					AssertMessages(dummyUpgradeManager.OutputTextCollection.Cast<string>(), expectedMessages, unexpectedMessages);
				}
			}
			finally
			{
				ObjectFactory.DisposeSubstitutions();
			}
		}

		void AssertMessages(IEnumerable<string> actualLogs, string[] expectedMessages, string[] unexpectedMessages)
		{
			foreach (var expectedMessage in expectedMessages)
			{
				AssertEquals($"[{expectedMessage}] logged? {string.Join(System.Environment.NewLine, actualLogs)}", true, actualLogs.Any(m => m.Contains(expectedMessage)));
			}

			foreach (var unexpectedMessage in unexpectedMessages)
			{
				AssertEquals($"[{unexpectedMessage}] logged? {string.Join(System.Environment.NewLine, actualLogs)}", false, actualLogs.Any(m => m.Contains(unexpectedMessage)));
			}
		}

		const string AssemblyName = "CargoWise.Glow.Model.CW.Resources";
		const string ResourceName = "CargoWise.Glow.Model.CW1.Resources.EnableChangeTracking.sql";

		string GetResourceAssemblyDir
		{
			get
			{
				var location = GetType().Assembly.Location;
				var dir = Path.GetDirectoryName(location);
				return dir;
			}
		}

		string GetResourceAssmblyFilename => $"{AssemblyName}.dll";

		string GetResourceAssemblyPath
		{
			get
			{
				var basePath = GetResourceAssemblyDir;
				var filename = GetResourceAssmblyFilename;
				var primaryPath = Path.Combine(basePath, filename);
				var fallbackPath = Path.Combine(Directory.GetParent(basePath).FullName, filename);

				var path = File.Exists(primaryPath) ? primaryPath :
						   File.Exists(fallbackPath) ? fallbackPath : null;

				AssertNotNull(path, $"Assembly not found at {primaryPath} or {fallbackPath}");
				return path;
			}
		}

		Assembly ResourceAssembly => Assembly.LoadFrom(GetResourceAssemblyPath);

		public void TestAssemblyExists()
		{
			AssertEquals(true, File.Exists(GetResourceAssemblyPath));
		}

		public void TestResourceExists()
		{
			var matchingResource = ResourceAssembly.GetManifestResourceNames().Where(s => s.Equals(ResourceName)).ToArray();
			AssertEquals(1, matchingResource.Length);
		}

		sealed class ChangeTrackingStatusManagerForTesting : ChangeTrackingStatusManager
		{
			public ChangeTrackingStatusManagerForTesting(Action setupActionForTesting = null) : base()
			{
				this.setupActionForTesting = setupActionForTesting;
			}

			public void SetChangeTrackingStatus_Exposed(AdminConnection connection, IUpgradeTaskWorkflowLogger logger, bool isEnable, ref string exceptionMessage)
			{
				SetChangeTrackingStatus(connection, logger, isEnable, ref exceptionMessage);
			}

			internal override void ExecuteGlowSetChangeTrackingStatusScript(bool isEnable, AdminConnection connection)
			{
				if (setupActionForTesting == null)
				{
					base.ExecuteGlowSetChangeTrackingStatusScript(isEnable, connection);
				}
				else
				{
					setupActionForTesting();
				}
			}

			readonly Action setupActionForTesting;
		}
	}
}
