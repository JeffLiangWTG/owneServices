using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.LogShipping.Setup;
using NUnit.Framework;

namespace Enterprise.LogShipping.Testing
{
	sealed class LogShippingConfiguratorTest : LogShippingTestFixture
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1019:NoSqlTransactionRollbackRule")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		[SnailTest]
		public void TestConfigure()
		{
			TempDirectory tempDir = null;
			var testSetupInfo = TestSetupInfo;
			var testDatabaseInfo = testSetupInfo.MainDatabase;
			try
			{
				using (var dbTestHelper = new DbTestHelper())
				{
					testSetupInfo.SetupAction = SetupAction.Setup;
					testDatabaseInfo.BackupFileName = dbTestHelper.GetTestBackupFile(new SqlServerVersionNumber($"{testSetupInfo.PrimaryServer.Version.Major}.{testSetupInfo.PrimaryServer.Version.Minor}"));
					testDatabaseInfo.ShouldInitialise = true;

					tempDir = new TempDirectory();
					testSetupInfo.BackupLocalCopyDirectory = tempDir.DirectoryName;

					try
					{
						var manager = new RestoreManager();
						manager.OnShowMessage += new NotificationDelegate(Manager_OnShowMessage);
						var restoreResult = manager.Restore(testSetupInfo);
						AssertEquals("Restore Database failed. \r\nOutput text:\r\n" + restoreMangagerOutput.ToString(), true, restoreResult);

						var configurator = new LogShippingConfiguratorForTesting();
						configurator.OnShowMessage += new NotificationDelegate(Configurator_OnShowMessage);

						tran = ((IDbConnectionInternals)dbTestHelper.TestConnection).ADOConnection.BeginTransaction();
						try
						{
							ClearLogShippingTables();

							//Configure LS
							configurator.DoConfigure_Exposed(tran, new List<DatabaseInfo> { testSetupInfo.MainDatabase });
							AssertDetailsOfPrimaryServerAndDatabaseConfigured(testSetupInfo);
							AssertDetailsOfSecondaryDatabaseConfigured(testSetupInfo);
							AssertJobsAndJobSchedulesConfigured(testSetupInfo);
							AssertRestoreJobStepsConfigured(configurator, testSetupInfo);

							//Reconfigure LS
							testSetupInfo.BackupLocalCopyDirectory = "TestBackupLocalDirectory";
							testSetupInfo.BackupSourceDirectory = "TestBackupSourceDirectory";
							testSetupInfo.PrimaryServerFromLSMetadata = testSetupInfo.PrimaryServer.FullInstanceName;
							configurator.DoReConfigure_Exposed(testSetupInfo, tran);
							AssertDetailsOfPrimaryServerAndDatabaseConfigured(testSetupInfo);
							AssertJobsAndJobSchedulesConfigured(testSetupInfo);

							//Remove LS
							configurator.DoRemove_Exposed(testSetupInfo, tran);
							AssertLSDataRemoved();
						}
						catch (System.Data.Common.DbException ex)
						{
							Fail("Log Shipping configuration failed. \r\nException:\r\n" + ex.Message + "\r\nOutput text:\r\n" + configuratorOutput.ToString());
						}
						finally
						{
							tran.Rollback();
						}

						configurator.DoDropSecondaryDatabase_Exposed(testSetupInfo, ((IDbConnectionInternals)dbTestHelper.TestConnection).ADOConnection);
						AssertEquals("Secondary database should be dropped", 0, (int)dbTestHelper.TestConnection.ExecuteScalar(string.Format("SELECT Count(*) FROM sys.databases WHERE [name] = '{0}'", testSetupInfo.MainDatabase.SecondaryDatabaseName)));
					}
					finally
					{
						dbTestHelper.DropDatabase(testSetupInfo.MainDatabase.SecondaryDatabaseName);
					}
				}
			}
			finally
			{
				if (tempDir != null)
				{
					var tempDirPath = tempDir.DirectoryName;
					try
					{ tempDir.Dispose(); }
					catch { DirectoryHelper.RetryDeletingTempDirectory(tempDirPath); }
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSetupActions()
		{
			var testSetupInfo = TestSetupInfo;
			var configurator = new LogShippingConfiguratorWithMockMethods();
			testSetupInfo.SetupAction = SetupAction.Setup;
			Assert("Configure Log Shipping", configurator.Configure(testSetupInfo));
			AssertEquals("Methods called when setup", "DoConfigure", configurator.CalledMethodsDuringConfiguration);

			configurator = new LogShippingConfiguratorWithMockMethods();
			testSetupInfo.SetupAction = SetupAction.Change;
			Assert("Reconfigure Log Shipping", configurator.Configure(testSetupInfo));
			AssertEquals("Methods called when change", "DoReConfigure", configurator.CalledMethodsDuringConfiguration);

			configurator = new LogShippingConfiguratorWithMockMethods();
			testSetupInfo.SetupAction = SetupAction.Remove;
			testSetupInfo.ShouldDropDatabaseAfterLSRemoving = false;
			Assert("Remove Log Shipping without dropping secondary db", configurator.Configure(testSetupInfo));
			AssertEquals("Methods called when remove without dropping secondary db", "DoRemove", configurator.CalledMethodsDuringConfiguration);

			configurator = new LogShippingConfiguratorWithMockMethods();
			testSetupInfo.SetupAction = SetupAction.Remove;
			testSetupInfo.ShouldDropDatabaseAfterLSRemoving = true;
			Assert("Remove Log Shipping with dropping secondary db", configurator.Configure(testSetupInfo));
			AssertEquals("Methods called when remove with dropping secondary db", "DoRemoveDoDropSecondaryDatabase", configurator.CalledMethodsDuringConfiguration);
		}

		public void TestConnectionMainProgramName()
		{
			var dummyConfigurator = new LogShippingConfiguratorForTesting();
			var sqlText = "SELECT program_name FROM sys.dm_exec_sessions WHERE session_id = @@spid";

			using (var conn = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.SqlMasterDb))
			{
				var programName = conn.ExecuteScalar(sqlText).ToString();
				AssertCollectionContains(
					"Enterprise DB connection Program Name has changed. Fix the value of LogShippingConfigurator.EnterpriseConnectionProgramName.",
					programName, dummyConfigurator.EnterpriseConnectionProgramNames_Exposed);
			}
		}

		public void TestEnterpriseConnectionProgramNames()
		{
			var dummyConfigurator = new LogShippingConfiguratorForTesting();
			AssertContainsExactElementsInAnyOrder(DbConnectionConstants.ApplicationNamesList, dummyConfigurator.EnterpriseConnectionProgramNames_Exposed);
		}

		public void TestEnterpriseConnectionProgramNamesAsSQLString()
		{
			var dummyConfigurator = new LogShippingConfiguratorForTesting();
			AssertContainsExactElementsInAnyOrder("'CargoWiseOne', 'CargoWiseOneServiceHost', 'CargoWiseOneServiceRunner'", dummyConfigurator.EnterpriseConnectionProgramNamesAsSQLString_Exposed);
		}

		void AssertDetailsOfPrimaryServerAndDatabaseConfigured(LogShippingInfo testSetupInfo)
		{
			var config = GetDataTableFromQuery("SELECT * FROM msdb.dbo.log_shipping_secondary").Rows[0];
			AssertEquals("Primary Server", testSetupInfo.PrimaryServer.FullInstanceName, (string)config["primary_server"]);
			AssertEquals("Primary Database", testSetupInfo.MainDatabase.DatabaseName, (string)config["primary_database"]);
			AssertEquals("Backup Source Directory", testSetupInfo.BackupSourceDirectory, (string)config["backup_source_directory"]);
			AssertEquals("Backup Local Copy Directory", testSetupInfo.BackupLocalCopyDirectory, (string)config["backup_destination_directory"]);
		}

		void AssertDetailsOfSecondaryDatabaseConfigured(LogShippingInfo testSetupInfo)
		{
			var config = GetDataTableFromQuery(@"
				SELECT * FROM msdb.dbo.log_shipping_secondary_databases lssd
				INNER JOIN msdb.dbo.log_shipping_monitor_secondary lsms
				ON lssd.secondary_database = lsms.secondary_database").Rows[0];

			AssertEquals("Secondary Database", testSetupInfo.MainDatabase.SecondaryDatabaseName, (string)config["secondary_database"]);
			AssertEquals("Primary Server", testSetupInfo.PrimaryServer.FullInstanceName, (string)config["primary_server"]);
			AssertEquals("Primary Database", testSetupInfo.MainDatabase.DatabaseName, (string)config["primary_database"]);
			AssertEquals("StandBy restore mode", true, (bool)config["restore_mode"]);
			AssertEquals("Should disconnect users", true, (bool)config["disconnect_users"]);
			AssertEquals("Restore threshold should be 45 minutes", 45, (int)config["restore_threshold"]);
			AssertEquals("Threshold alert should be enabled", true, (bool)config["threshold_alert_enabled"]);
		}

		void AssertJobsAndJobSchedulesConfigured(LogShippingInfo testSetupInfo)
		{
			var sqlText = @"
				SELECT ss.name as schedule_name, sj.enabled as job_enabled, 
					freq_type, freq_interval, freq_subday_type, freq_subday_interval, active_start_time 
				FROM msdb.dbo.sysjobs sj
				INNER JOIN msdb.dbo.sysjobschedules sjs ON sjs.job_id = sj.job_id
				INNER JOIN msdb.dbo.sysschedules ss ON ss.schedule_id = sjs.schedule_id
				WHERE sj.name = '{0}'";

			//Copy job
			var jobName = string.Format("LSCopy_{0}_{1}", testSetupInfo.PrimaryServer.FullInstanceName, testSetupInfo.MainDatabase.DatabaseName);
			var configTable = GetDataTableFromQuery(string.Format(sqlText, jobName));
			Assert("Copy job should exist", configTable.Rows.Count == 1);
			AssertEquals("Copy job should be enabled", (byte)1, (byte)(configTable.Rows[0]["job_enabled"]));
			AssertJobSchedule(configTable.Rows[0], "DefaultCopyJobSchedule", 000500);

			//Restore job
			jobName = string.Format("LSRestore_{0}_{1}", testSetupInfo.PrimaryServer.FullInstanceName, testSetupInfo.MainDatabase.DatabaseName);
			configTable = GetDataTableFromQuery(string.Format(sqlText, jobName));
			Assert("Restore job should exist", configTable.Rows.Count == 1);
			AssertEquals("Restore job should be enabled", (byte)1, (byte)configTable.Rows[0]["job_enabled"]);
			AssertJobSchedule(configTable.Rows[0], "DefaultRestoreJobSchedule", 001000);
		}

		void AssertJobSchedule(DataRow config, string scheduleName, int startTime)
		{
			AssertEquals("Schedule name", scheduleName, (string)config["schedule_name"]);
			AssertEquals(scheduleName + ": Frequency type - Daily(4)", 4, (int)config["freq_type"]);
			AssertEquals(scheduleName + ": Frequency interval - Every day", 1, (int)config["freq_interval"]);
			AssertEquals(scheduleName + ": Frequency subday type - Minutes(4)", 4, (int)config["freq_subday_type"]);
			AssertEquals(scheduleName + ": Frequency subday interval - Every 15 minutes", 15, (int)config["freq_subday_interval"]);
			AssertEquals(scheduleName + ": Active start time", startTime, (int)config["active_start_time"]);
		}

		void AssertRestoreJobStepsConfigured(LogShippingConfiguratorForTesting configurator, LogShippingInfo testSetupInfo)
		{
			var sqlText = @"
				SELECT step_id, step_name, on_success_action, on_fail_action, 
					on_fail_step_id, os_run_priority, subsystem, command
				FROM msdb.dbo.sysjobs sj
				INNER JOIN msdb.dbo.sysjobsteps sjs ON sjs.job_id = sj.job_id
				WHERE sj.name = 'LSRestore_{0}_{1}'";

			var configTable = GetDataTableFromQuery(string.Format(sqlText, testSetupInfo.PrimaryServer.FullInstanceName, testSetupInfo.MainDatabase.DatabaseName));
			AssertEquals("Restore job steps count", 4, configTable.Rows.Count);

			//LockStandbyDatabaseStep
			var restoreLockPropertyName = LogShippingConfigurator.RestoreLockPropertyPrefix + testSetupInfo.MainDatabase.SecondaryDatabaseName;
			var command = string.Format(configurator.LockStandbyDbCommand, restoreLockPropertyName);
			AssertNotLastStep(configTable.Rows[0], 1, "LockStandbyDatabaseStep", command);

			//WaitUntilNoUserStep
			command = string.Format(configurator.WaitUntilNoUserCommand, testSetupInfo.MainDatabase.SecondaryDatabaseName, configurator.EnterpriseConnectionProgramNamesAsSQLString_Exposed);
			AssertNotLastStep(configTable.Rows[1], 2, "WaitUntilNoUserStep", command);

			//RestoreStep
			AssertNotLastStep(configTable.Rows[2], 3);

			//RemoveLockStep
			command = string.Format(configurator.UnlockStandbyDbCommand, restoreLockPropertyName);
			AssertLastStep(configTable.Rows[3], 4, "RemoveLockStep", command);
		}

		void AssertNotLastStep(DataRow step, int stepId, string stepName, string command)
		{
			AssertEquals("Step name", stepName, (string)step["step_name"]);
			AssertEquals(stepName + " : Sub system", "TSQL", (string)step["subsystem"]);
			AssertEquals(stepName + " : Sql Command", command, (string)step["command"]);
			AssertNotLastStep(step, stepId);
		}

		void AssertNotLastStep(DataRow step, int stepId)
		{
			string stepName = (string)step["step_name"];
			AssertEquals(stepName + " : Sequence number", stepId, (int)step["step_id"]);
			AssertEquals(stepName + " : On success action - Go to next step (3)", (byte)3, (byte)step["on_success_action"]);
			AssertEquals(stepName + " : On fail action - Go to step with id = on_fail_step_id (4)", (byte)4, (byte)step["on_fail_action"]);
			AssertEquals(stepName + " : On fail step id - Last step", 4, (int)step["on_fail_step_id"]);
			AssertEquals(stepName + " : Os run priority", 0, (int)step["os_run_priority"]);
		}

		void AssertLastStep(DataRow step, int stepId, string stepName, string command)
		{
			AssertEquals("Step name", stepName, (string)step["step_name"]);
			AssertEquals(stepName + " : Sequence number", stepId, (int)step["step_id"]);
			AssertEquals(stepName + " : On success action - Quit with success (1)", (byte)1, (byte)step["on_success_action"]);
			AssertEquals(stepName + " : On fail action - Quit with failure (2)", (byte)2, (byte)step["on_fail_action"]);
			AssertEquals(stepName + " : Os run priority", 0, (int)step["os_run_priority"]);
			AssertEquals(stepName + " : Sub system", "TSQL", (string)step["subsystem"]);
			AssertEquals(stepName + " : Sql Command", command, (string)step["command"]);
		}

		void AssertLSDataRemoved()
		{
			AssertEquals("msdb.dbo.log_shipping_secondary is clear", 0, GetDataTableFromQuery("SELECT * FROM msdb.dbo.log_shipping_secondary").Rows.Count);
			AssertEquals("msdb.dbo.log_shipping_secondary_databases is clear", 0, GetDataTableFromQuery("SELECT * FROM msdb.dbo.log_shipping_secondary_databases").Rows.Count);
			AssertEquals("msdb.dbo.log_shipping_monitor_secondary is clear", 0, GetDataTableFromQuery("SELECT * FROM msdb.dbo.log_shipping_monitor_secondary").Rows.Count);
			AssertEquals("msdb.dbo.sysjobs is clear", 0, GetDataTableFromQuery("SELECT * FROM msdb.dbo.sysjobs").Rows.Count);
			AssertEquals("msdb.dbo.sysjobschedules is clear", 0, GetDataTableFromQuery("SELECT * FROM msdb.dbo.sysjobschedules").Rows.Count);
			AssertEquals("msdb.dbo.sysschedules is clear", 0, GetDataTableFromQuery("SELECT * FROM msdb.dbo.sysschedules").Rows.Count);
			AssertEquals("msdb.dbo.sysjobsteps is clear", 0, GetDataTableFromQuery("SELECT * FROM msdb.dbo.sysjobsteps").Rows.Count);
		}

		void ClearLogShippingTables()
		{
			string sqlText = @"
				DELETE msdb.dbo.log_shipping_secondary
				DELETE msdb.dbo.log_shipping_secondary_databases
				DELETE msdb.dbo.log_shipping_monitor_secondary
				DELETE msdb.dbo.sysjobschedules
				DELETE msdb.dbo.sysjobs
				DELETE msdb.dbo.sysschedules
				DELETE msdb.dbo.sysjobsteps";
			using (var cmd = tran.Connection.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = sqlText;
				cmd.ExecuteNonQuery();
			}
		}

		LogShippingInfo TestSetupInfo
		{
			get
			{
				if (testSetupInfo == null)
				{
					testSetupInfo = new LogShippingInfo();
					var testDatabaseInfo = new MainDatabaseInfo(testSetupInfo, "TestDatabase" + Guid.NewGuid());
					testSetupInfo.MainDatabase = testDatabaseInfo;
					testSetupInfo.PrimaryServer = new SqlServerInfoForTesting(Db.Connection.ServerNameWithoutInstance, Db.Connection.ServerInstanceName, Db.Connection.ServerVersionNumber.ToString());
					testSetupInfo.SecondaryServer = testSetupInfo.PrimaryServer;
					testSetupInfo.BackupSourceDirectory = Path.Combine(BaseSourcePath, DbTestHelper.TestBackupFilePath);
				}
				return testSetupInfo;
			}
		}
		LogShippingInfo testSetupInfo;

		DataTable GetDataTableFromQuery(string sqlText)
		{
			var result = new DataTable();
			var cmd = tran.Connection.CreateCommand();
			cmd.Transaction = tran;
			cmd.CommandText = sqlText;
			var adapter = CreateDataAdapter(cmd);
			adapter.Fill(result);
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Full name needed to determin SqlClient types")]
		static System.Data.Common.DbDataAdapter CreateDataAdapter(System.Data.Common.DbCommand @this)
		{
			System.Data.Common.DbDataAdapter adapter = null;
#if NET
			if (@this is Microsoft.Data.SqlClient.SqlCommand sqlCommandMS)
			{
				adapter = new Microsoft.Data.SqlClient.SqlDataAdapter(sqlCommandMS);
			}
#endif
			if (adapter == null)
			{
				if (@this is System.Data.SqlClient.SqlCommand sqlCommand)
				{
					adapter = new System.Data.SqlClient.SqlDataAdapter(sqlCommand);
				}
				else
				{
					throw new NotSupportedException($"Unsupported command type: {@this.GetType()}");
				}
			}
			return adapter;
		}

		System.Data.Common.DbTransaction tran;

		void Manager_OnShowMessage(string message)
		{
			restoreMangagerOutput.AppendLine(message);
		}

		void Configurator_OnShowMessage(string message)
		{
			configuratorOutput.AppendLine(message);
		}

		readonly StringBuilder restoreMangagerOutput = new StringBuilder();
		readonly StringBuilder configuratorOutput = new StringBuilder();

		sealed class LogShippingConfiguratorForTesting : LogShippingConfigurator
		{
			internal new string LockStandbyDbCommand => LogShippingConfigurator.LockStandbyDbCommand;

			internal new string UnlockStandbyDbCommand => LogShippingConfigurator.UnlockStandbyDbCommand;

			internal new string WaitUntilNoUserCommand => LogShippingConfigurator.WaitUntilNoUserCommand;

			internal void DoConfigure_Exposed(System.Data.Common.DbTransaction tran, List<DatabaseInfo> databasesToProcess)
				=> DoConfigure(tran, databasesToProcess);

			internal List<string> EnterpriseConnectionProgramNames_Exposed => EnterpriseConnectionProgramNames;

			internal string EnterpriseConnectionProgramNamesAsSQLString_Exposed => EnterpriseConnectionProgramNamesAsSQLString;

			internal void DoReConfigure_Exposed(LogShippingInfo info, System.Data.Common.DbTransaction tran) => DoReConfigure(tran, info);

			internal void DoRemove_Exposed(LogShippingInfo info, System.Data.Common.DbTransaction tran) => DoRemove(tran, info);

			internal void DoDropSecondaryDatabase_Exposed(LogShippingInfo info, System.Data.Common.DbConnection conn) => DoDropSecondaryDatabase(conn, info);
		}

		sealed class LogShippingConfiguratorWithMockMethods : LogShippingConfigurator
		{
			protected override void DoConfigure(System.Data.Common.DbTransaction tran, List<DatabaseInfo> databasesToProcess)
			{
				CalledMethodsDuringConfiguration += "DoConfigure";
			}

			protected override void DoReConfigure(System.Data.Common.DbTransaction tran, LogShippingInfo info)
			{
				CalledMethodsDuringConfiguration += "DoReConfigure";
			}

			protected override void DoRemove(System.Data.Common.DbTransaction tran, LogShippingInfo info)
			{
				CalledMethodsDuringConfiguration += "DoRemove";
			}

			protected override void DoDropSecondaryDatabase(System.Data.Common.DbConnection connection, LogShippingInfo info)
			{
				CalledMethodsDuringConfiguration += "DoDropSecondaryDatabase";
			}

			internal string CalledMethodsDuringConfiguration { get; set; }
		}
	}
}
