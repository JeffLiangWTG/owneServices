using CargoWise.Data;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check.Testing
{
	[TestedType(typeof(TraceFlagChecker))]
	sealed class TraceFlagCheckerTest : CheckerTestCaseBase
	{
		public void TestTraceFlags()
		{
			DbHealthWarningList warnings = new DbHealthWarningList();
			IChecker checker = new TraceFlagChecker();
			AssertEquals("description", "Check required trace flags are enabled on the SQL Server", checker.Description);
			checker.Check(Db.Connection, warnings, new TestServiceLogger());
		}

		public void TestEnabledTraceFlagsNotListedinCw1DoNotGenerateNotifications()
		{
			// Skip check for tf12502 as it's tested individually below
			using var revertVersion = Db.Connection.SetSqlServerVersionForTest(SqlServerVersions.Sql2022Cu4);

			var tfc = new TraceFlagCheckerForTesting();
			DbHealthWarningList warnings = new DbHealthWarningList();
			tfc.CheckTraceFlags(Db.Connection, warnings);

			AssertEquals("Number of warnings", 3, warnings.Count);
			AssertEquals("First warning description", "Trace Flag 1448 is not enabled on the SQL Server", warnings[0].Description);
			AssertEquals("First warning action", "Enable this trace flag", warnings[0].Action);
			AssertEquals("Second warning description", "Trace Flag 4199 is not enabled on the SQL Server", warnings[1].Description);
			AssertEquals("Second warning action", "Enable this trace flag", warnings[1].Action);
			AssertEquals("Third warning description", "Trace Flag 15006 is not enabled on the SQL Server", warnings[2].Description);
			AssertEquals("Third warning action", "Enable this trace flag", warnings[2].Action);

			tfc.EnableTF(1448, false);
			tfc.EnableTF(4199, false);
			tfc.EnableTF(15006, false);
			warnings = new DbHealthWarningList();
			tfc.CheckTraceFlags(Db.Connection, warnings);
			AssertEquals("Number of warnings", 3, warnings.Count);
			AssertEquals("First warning description", "Trace Flag 1448 is not enabled on the SQL Server", warnings[0].Description);
			AssertEquals("First warning action", "Enable this trace flag", warnings[0].Action);
			AssertEquals("Second warning description", "Trace Flag 4199 is not enabled on the SQL Server", warnings[1].Description);
			AssertEquals("Second warning action", "Enable this trace flag", warnings[1].Action);
			AssertEquals("Third warning description", "Trace Flag 15006 is not enabled on the SQL Server", warnings[2].Description);
			AssertEquals("Third warning action", "Enable this trace flag", warnings[2].Action);

			tfc.DisableTF(4199);
			tfc.EnableTF(4199, true);
			tfc.DisableTF(1448);
			tfc.EnableTF(1448, true);
			tfc.DisableTF(15006);
			tfc.EnableTF(15006, true);
			warnings = new DbHealthWarningList();
			tfc.CheckTraceFlags(Db.Connection, warnings);
			AssertEquals("No warnings expected", 0, warnings.Count);

			tfc.EnableTF(500, true);
			tfc.EnableTF(600, true);
			tfc.EnableTF(700, false);
			tfc.EnableTF(3266, false);
			warnings = new DbHealthWarningList();
			tfc.CheckTraceFlags(Db.Connection, warnings);
			AssertEquals("Additional TFs should not generate warnings", 0, warnings.Count);
		}

		public void TestNoWarningWhenTraceFlag12502IsNotEnabledOnSqlServerBelow2022Cu5()
		{
			Test(SqlServerVersions.Sql2019Cu26);
			Test(SqlServerVersions.Sql2022Cu3);
			Test(SqlServerVersions.Sql2022Cu4);

			void Test(string version)
			{
				// Arrange
				var tfc = new TraceFlagCheckerForTesting();
				tfc.EnableTF(1448, true);
				tfc.EnableTF(4199, true);
				tfc.EnableTF(15006, true);

				var warnings = new DbHealthWarningList();
				using var revertVersion = Db.Connection.SetSqlServerVersionForTest(version);

				// Act
				tfc.CheckTraceFlags(Db.Connection, warnings);

				// Assert
				AssertEquals("Number of warnings", 0, warnings.Count);
			}
		}

		public void TestNoWarningWhenTraceFlag12502IsEnabledOnSqlServerAbove2022Cu5()
		{
			Test(SqlServerVersions.Sql2022Cu5);
			Test(SqlServerVersions.Sql2022Cu6);

			void Test(string version)
			{
				// Arrange
				var tfc = new TraceFlagCheckerForTesting();
				tfc.EnableTF(1448, true);
				tfc.EnableTF(4199, true);
				tfc.EnableTF(15006, true);
				tfc.EnableTF(12502, true);

				var warnings = new DbHealthWarningList();
				using var revertVersion = Db.Connection.SetSqlServerVersionForTest(version);

				// Act
				tfc.CheckTraceFlags(Db.Connection, warnings);

				// Assert
				AssertEquals("Number of warnings", 0, warnings.Count);
			}
		}

		public void TestWarningWhenTraceFlag12502IsNotEnabledOnSqlServerAbove2022Cu5()
		{
			Test(SqlServerVersions.Sql2022Cu5);
			Test(SqlServerVersions.Sql2022Cu6);

			void Test(string version)
			{
				// Arrange
				var tfc = new TraceFlagCheckerForTesting();
				tfc.EnableTF(1448, true);
				tfc.EnableTF(4199, true);
				tfc.EnableTF(15006, true);

				var warnings = new DbHealthWarningList();
				using var revertVersion = Db.Connection.SetSqlServerVersionForTest(version);

				// Act
				tfc.CheckTraceFlags(Db.Connection, warnings);

				// Assert
				AssertEquals($"Number of warnings, sql server version = {version}", 1, warnings.Count);
				AssertEquals($"Warning description, sql server version = {version}", "Trace Flag 12502 is not enabled on the SQL Server", warnings[0].Description);
				AssertEquals($"Warning action, sql server version = {version}", "Enable this trace flag", warnings[0].Action);
			}
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new TraceFlagChecker();
		}

		// See https://learn.microsoft.com/en-us/troubleshoot/sql/releases/sqlserver-2022/build-versions
		static class SqlServerVersions
		{
			public const string Sql2019Cu26 = "15.0.4365.2";
			public const string Sql2022Cu3 = "16.0.4025.1";
			public const string Sql2022Cu4 = "16.0.4035.4";

			public const string Sql2022Cu5 = "16.0.4045.3";
			public const string Sql2022Cu6 = "16.0.4055.4";
		}
	}
}
