using System;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore
{
	sealed class CommandLineSetTest : TestCase
	{
		public void TestCommandLineParse()
		{
			var cmd = new CommandLineSet();
			var args = new[] { "-server", "testServer", "-d", "testDB", "-bak", @"""temp\Databases for CW1\BM_20141220.bak""", "-ro", "restorewithrecovery", "-dfp", "D:\\A DataPath\\Here\\", "-r", "-lfp", "L:\\", "-excludeauditdb" };

			CombineAssertions(() =>
			{
				AssertEquals("Valid arguments", true, cmd.Parse(args));

				AssertNotNull(Defaults.Instance.ServerName);
				AssertEquals("testServer", Defaults.Instance.ServerName);
				AssertNotNull(Defaults.Instance.DatabaseName);
				AssertEquals("testDB", Defaults.Instance.DatabaseName);
				AssertNotNull(Defaults.Instance.BackupFileName);
				AssertEquals(@"temp\Databases for CW1\BM_20141220.bak", Defaults.Instance.BackupFileName);
				AssertNotNull(Defaults.Instance.RestoreOption);
				AssertEquals(DbRestoreOption.RestoreWithRecovery, Defaults.Instance.RestoreOption);
				AssertEquals(true, Defaults.Instance.RunDbRestore);
				AssertEquals(true, Defaults.Instance.IsAuditDBExcludedFromRestore);
				AssertNull(Defaults.Instance.SalesDbRestore);
				AssertEquals(false, Defaults.Instance.RunSalesDbRestore);

				AssertEquals("Data File Path", @"D:\A DataPath\Here\", Defaults.Instance.DataFilePath);
				AssertEquals("Log File Path", @"L:\", Defaults.Instance.LogFilePath);
			});
		}

		public void TestCommandLineParse_WithConfusingParameters()
		{
			var cmd = new CommandLineSet();
			var args = new[] { "-source", "testDB", "-bak", @"temp\BM_20141220.bak", "-ro", "restorewithrecovery", "-r" };

			CombineAssertions(() =>
			{
				AssertEquals("Invalid arguments", false, cmd.Parse(args));

				AssertNull(Defaults.Instance.ServerName);
				AssertNull(Defaults.Instance.DatabaseName);
				AssertNull(Defaults.Instance.BackupFileName);
			});
		}

		public void TestCommandLineParse_AuditDatabaseArguments()
		{
			var cmd = new CommandLineSet();
			var args = new[] { "-as", "auditServer", "-ab", @"temp\Audit.bak", "-adfp", @"temp\Data", "-alfp", @"temp\Logs" };

			CombineAssertions(() =>
			{
				AssertEquals("Valid arguments", true, cmd.Parse(args));

				AssertEquals("auditServer", Defaults.Instance.AuditServerName);
				AssertEquals(@"temp\Audit.bak", Defaults.Instance.AuditBackupFileName);
				AssertEquals(@"temp\Data", Defaults.Instance.AuditDataFilePath);
				AssertEquals(@"temp\Logs", Defaults.Instance.AuditLogFilePath);
			});
		}

		public void TestCommandLineParse_EdwDatabaseArguments()
		{
			var cmd = new CommandLineSet();
			var args = new[] { "-dws", "dwServer", "-eb", @"temp\EDW.bak", "-edfp", @"temp\Data", "-elfp", @"temp\Logs" };

			CombineAssertions(() =>
			{
				AssertEquals("Valid arguments", true, cmd.Parse(args));

				AssertEquals("dwServer", Defaults.Instance.DataWarehouseServerName);
				AssertEquals(@"temp\EDW.bak", Defaults.Instance.EdwBackupFileName);
				AssertEquals(@"temp\Data", Defaults.Instance.EdwDataFilePath);
				AssertEquals(@"temp\Logs", Defaults.Instance.EdwLogFilePath);
			});
		}

		public void TestCommandLineParse_AddDbToAvailabilityGroup()
		{
			var cmd = new CommandLineSet();
			var args = new[] { "-adag", "Yes", "-ag", "testgroup" };

			CombineAssertions(() =>
			{
				AssertEquals("Valid arguments", true, cmd.Parse(args));

				AssertEquals(true, Defaults.Instance.AddDbToAvailabilityGroup);
				AssertEquals("testgroup", Defaults.Instance.AvailabilityGroup);
			});
		}

		public void TestCommandLineParse_InvalidRestoreOption()
		{
			var cmd = new CommandLineSet();
			string[] args = {
				"-ro",
				"RestoreWithWhateverOption"
			};
			AssertEquals("Invalid restore option", false, cmd.Parse(args));
		}

		public void TestAddMissingArgumentsAndRemoveEscapeSequence()
		{
			var cmd = new CommandLineSet();
			string[] args = {
				"ServerName",
				"DbName",
				"-R",
				"-rO",
				"RestoreWithRecovery",
				"-dFp",
				"\"D:\\DataFilePath\"",
				"-LFP",
				"L:\"",
				"-bAk",
				"D:\\BakPathHere\\File.bak",
			};

			CombineAssertions(() =>
			{
				AssertEquals("Valid arguments", true, cmd.Parse(args));

				AssertNotNull(Defaults.Instance.ServerName);
				AssertEquals("ServerName", Defaults.Instance.ServerName);
				AssertNotNull(Defaults.Instance.DatabaseName);
				AssertEquals("DbName", Defaults.Instance.DatabaseName);
				AssertNotNull(Defaults.Instance.BackupFileName);
				AssertEquals("D:\\BakPathHere\\File.bak", Defaults.Instance.BackupFileName);
				AssertNotNull(Defaults.Instance.RestoreOption);
				AssertEquals(DbRestoreOption.RestoreWithRecovery, Defaults.Instance.RestoreOption);
				AssertEquals(true, Defaults.Instance.RunDbRestore);

				AssertNull(Defaults.Instance.SalesDbRestore);
				AssertEquals(false, Defaults.Instance.RunSalesDbRestore);

				AssertEquals("Data File Path", @"D:\DataFilePath", Defaults.Instance.DataFilePath);
				AssertEquals("Log File Path", @"L:", Defaults.Instance.LogFilePath);
			});
		}

		public void TestAddMissingArguments()
		{
			var cmd = new CommandLineSet();

			Defaults.ResetDefaultInstance();
			var args = new[] { "ServerName", "DbName", "-R" };
			cmd.Parse(args);
			AssertServerAndDatabaseName("Missing -d and -s arguments", "ServerName", "DbName");

			Defaults.ResetDefaultInstance();
			args = new[] { "ServerName", "-database", "DbName", "-R" };
			cmd.Parse(args);
			AssertServerAndDatabaseName("Missing -s arguments", "ServerName", "DbName");

			Defaults.ResetDefaultInstance();
			args = new[] { "-server", "ServerName", "DbName", "-R" };
			cmd.Parse(args);
			AssertServerAndDatabaseName("Missing -d arguments", "ServerName", "DbName");

			Defaults.ResetDefaultInstance();
			args = new[] { "whateverArg", "-s", "ServerName", "AnotherArg", "-d", "DbName" };
			cmd.Parse(args);
			AssertServerAndDatabaseName("-d and -s are present but not the first two arguments", "ServerName", "DbName");

			Defaults.ResetDefaultInstance();
			args = Array.Empty<string>();
			cmd.Parse(args);
			AssertServerAndDatabaseName("No arguments", null, null);

			Defaults.ResetDefaultInstance();
			args = new[] { "whateverArg" };
			cmd.Parse(args);
			AssertServerAndDatabaseName("One argument only", "whateverArg", null);

			Defaults.ResetDefaultInstance();
			args = new[] { "-d", "DbName58" };
			cmd.Parse(args);
			AssertServerAndDatabaseName("-s, -d and DbName arguments #1", null, "DbName58");

			Defaults.ResetDefaultInstance();
			args = new[] { "srvNameHere", "whateverArg", "-d", "DbName58", "anotherArg" };
			cmd.Parse(args);
			AssertServerAndDatabaseName("-s, -d and DbName arguments #2", "srvNameHere", "DbName58");

			Defaults.ResetDefaultInstance();
			args = new[] { "ttt", "-s", "srvNameHere", "-ro", "RestoreWithRecovery" };
			cmd.Parse(args);
			AssertServerAndDatabaseName("-d is missing #1", "srvNameHere", null);

			Defaults.ResetDefaultInstance();
			args = new[] { "-s", "srvNameHere", "-ro", "RestoreWithRecovery" };
			cmd.Parse(args);
			AssertServerAndDatabaseName("-d is missing #2", "srvNameHere", null);

			Defaults.ResetDefaultInstance();
			args = new[] { "-ro", "RestoreWithRecovery" };
			cmd.Parse(args);
			AssertServerAndDatabaseName("Missing -d and -s arguments", null, null);
		}

		public void TestCommandLineParse_DisplayHelpText()
		{
			var cmd = new CommandLineSet();

			CombineAssertions(() =>
			{
				AssertEquals("Valid argument", true, cmd.Parse(new[] { "" }));
				AssertEquals(false, Defaults.Instance.DisplayHelpText);
			});

			Defaults.ResetDefaultInstance();
			CombineAssertions(() =>
			{
				AssertEquals("Valid argument", true, cmd.Parse(new[] { "-h" }));
				AssertEquals(true, Defaults.Instance.DisplayHelpText);
			});
		}

		void AssertServerAndDatabaseName(string message, string serverName, string dbName)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals(serverName, Defaults.Instance.ServerName);
				AssertEquals(dbName, Defaults.Instance.DatabaseName);
			});
		}
	}
}
