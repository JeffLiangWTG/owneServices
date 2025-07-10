using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using WTG.Data.SqlClient;

namespace Enterprise.LogShipping.Setup
{
	public class LogShippingConfigurator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public bool Configure(LogShippingInfo info)
		{
			Argument.NotNull(info, nameof(info));
			Argument.NotNull(info.SecondaryServer, nameof(info.SecondaryServer));

			bool result = true;

			try
			{
				using var connection = DbManager.OpenNewConnectionWithOdysseyAdminLogin(info.SecondaryServer.FullInstanceName);
				try
				{
					using var tran = connection.BeginTransaction();
					switch (info.SetupAction)
					{
						case SetupAction.Setup:
							DoConfigure(tran, info.GetDatabaseInfoListToProcess());
							break;
						case SetupAction.Change:
							DoReConfigure(tran, info);
							break;
						case SetupAction.Remove:
							DoRemove(tran, info);
							break;
					}
					tran.Commit();
				}
				catch (System.Data.Common.DbException ex)
				{
					result = false;
					ShowMessage("Log Shipping configuration failed.\r\n" + ex.Message);
				}

				if (info.SetupAction == SetupAction.Remove && info.ShouldDropDatabaseAfterLSRemoving)
				{
					try
					{
						DoDropSecondaryDatabase(connection, info);
					}
					catch (System.Data.Common.DbException ex)
					{
						result = false;
						ShowMessage("Cannot drop database.\r\n" + ex.Message);
					}
				}
			}
			catch (System.Data.Common.DbException ex)
			{
				result = false;
				ShowMessage(ex.Message);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual void DoConfigure(System.Data.Common.DbTransaction tran, List<DatabaseInfo> databasesToProcess)
		{
			Argument.NotNull(tran, nameof(tran));
			Argument.NotNull(tran.Connection, nameof(tran.Connection));
			Argument.NotNull(databasesToProcess, nameof(databasesToProcess));

			ShowMessage("Log Shipping configuration started.");

			ShowMessage("Supplying the details of the primary server and database.");
			SupplyDetailsOfPrimaryServerAndDatabases(tran, databasesToProcess);
			ShowMessage("Success.");

			ShowMessage("Adding secondary database.");
			AddSecondaryDatabases(tran, databasesToProcess);
			ShowMessage("Success.");

			ShowMessage("Setting up default schedule for the copy and restore jobs.");

			foreach (DatabaseInfo dbInfo in databasesToProcess)
			{
				if (dbInfo != null)
				{
					//Configure jobs schedule
					JobScheduleSettings scheduleSettings = new JobScheduleSettings();
					scheduleSettings.FrequencyType = FreqType.Daily;
					scheduleSettings.FrequencyInterval = 1;
					scheduleSettings.FrequencySubdayType = FreqSubdayType.Minutes;
					scheduleSettings.FrequencySubdayInterval = 15;
					//Configure Copy job schedule
					scheduleSettings.ScheduleName = "DefaultCopyJobSchedule";
					scheduleSettings.StartTime = new DateTime(2000, 1, 1, 0, 5, 0);
					AddJobSchedule(tran, dbInfo.CopyJobId, scheduleSettings);
					//Configure Restore job schedule
					scheduleSettings.ScheduleName = "DefaultRestoreJobSchedule";
					scheduleSettings.StartTime = new DateTime(2000, 1, 1, 0, 10, 0);
					AddJobSchedule(tran, dbInfo.RestoreJobId, scheduleSettings);
				}
			}

			ShowMessage("Success.");

			ShowMessage("Configuring extra control steps for the restore job.");

			foreach (DatabaseInfo dbInfo in databasesToProcess)
			{
				if (dbInfo != null)
				{
					//Add Lock Standby Database Step
					string restoreLockPropertyName = RestoreLockPropertyPrefix + dbInfo.SecondaryDatabaseName;
					JobStepSettings stepSettings = new JobStepSettings();
					stepSettings.StepName = "LockStandbyDatabaseStep";
					stepSettings.StepID = 1;
					stepSettings.OnSuccessAction = Action.GoToNextStep;
					stepSettings.DbCommand = string.Format(LockStandbyDbCommand, restoreLockPropertyName);
					AddJobStep(tran, dbInfo.RestoreJobId, stepSettings);

					//Add Wait Until No User Step
					stepSettings.StepName = "WaitUntilNoUserStep";
					stepSettings.StepID = 2;
					stepSettings.OnSuccessAction = Action.GoToNextStep;
					stepSettings.DbCommand = string.Format(WaitUntilNoUserCommand, dbInfo.SecondaryDatabaseName, EnterpriseConnectionProgramNamesAsSQLString);
					AddJobStep(tran, dbInfo.RestoreJobId, stepSettings);

					//Add Unlock Standby Database Step
					stepSettings.StepName = "RemoveLockStep";
					stepSettings.StepID = 4;
					stepSettings.OnSuccessAction = Action.QuitWithSuccess;
					stepSettings.DbCommand = string.Format(UnlockStandbyDbCommand, restoreLockPropertyName);
					AddJobStep(tran, dbInfo.RestoreJobId, stepSettings);

					UpdateJobStep(tran, dbInfo.RestoreJobId, 1);
					UpdateJobStep(tran, dbInfo.RestoreJobId, 2);
					UpdateJobStep(tran, dbInfo.RestoreJobId, 3);
				}
			}

			ShowMessage("Success.");

			ShowMessage("Starting the copy and restore jobs.");

			foreach (DatabaseInfo dbInfo in databasesToProcess)
			{
				if (dbInfo != null)
				{
					//Enable jobs
					EnableJob(tran, dbInfo.CopyJobId);
					EnableJob(tran, dbInfo.RestoreJobId);
				}
			}

			ShowMessage("Success.");

			ShowMessage("Log Shipping configuration completed successfully.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual void DoReConfigure(System.Data.Common.DbTransaction tran, LogShippingInfo info)
		{
			Argument.NotNull(tran, nameof(tran));
			Argument.NotNull(tran.Connection, nameof(tran.Connection));
			Argument.NotNull(info, nameof(info));

			List<DatabaseInfo> existingDatabases = new List<DatabaseInfo>();
			List<DatabaseInfo> newDatabases = new List<DatabaseInfo>();

			var listToProcess = info.GetDatabaseInfoListToProcess();

			// LINQ was removed because Code Contract couldn't resolve the logic (CodeContracts: reference use unreached)
			foreach (var item in listToProcess)
			{
				if (item != null)
				{
					if (item.IsNew)
					{
						newDatabases.Add(item);
					}
					else
					{
						existingDatabases.Add(item);
					}
				}
			}

			if (existingDatabases.Count > 0)
			{
				//Disable jobs
				ShowMessage("Disabling the copy and restore jobs.");

				foreach (DatabaseInfo dbInfo in existingDatabases)
				{
					if (dbInfo != null)
					{
						DisableJob(tran, dbInfo.CopyJobId);
						DisableJob(tran, dbInfo.RestoreJobId);
					}
				}

				ShowMessage("Success.\r\n");

				//Change configuration
				ShowMessage("Applying the new Log Shipping settings.");
				ChangeLogShippingDetails(tran, info, existingDatabases);
				ShowMessage("Success.\r\n");

				//Enable jobs
				ShowMessage("Starting the copy and restore jobs.");

				foreach (DatabaseInfo dbInfo in existingDatabases)
				{
					if (dbInfo != null)
					{
						EnableJob(tran, dbInfo.CopyJobId);
						EnableJob(tran, dbInfo.RestoreJobId);
					}
				}

				ShowMessage("Success.\r\n");
				ShowMessage("Log Shipping configuration changed successfully.");
			}

			if (newDatabases.Count > 0)
			{
				DoConfigure(tran, newDatabases);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual void DoRemove(System.Data.Common.DbTransaction tran, LogShippingInfo info)
		{
			Argument.NotNull(tran, nameof(tran));
			Argument.NotNull(tran.Connection, nameof(tran.Connection));
			Argument.NotNull(info, nameof(info));

			ShowMessage("Removing Log Shipping configuration.");
			RemoveLogShipping(tran, info);
			ShowMessage("Log Shipping settings removed successfully.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual void DoDropSecondaryDatabase(System.Data.Common.DbConnection connection, LogShippingInfo info)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(info, nameof(info));

			var listToProcess = info.GetDatabaseInfoListToProcess();

			foreach (DatabaseInfo dbInfo in listToProcess)
			{
				if (dbInfo != null)
				{
					ShowMessage("Getting exclusive access to secondary database to drop it.");
					DbManager.KillConnections(connection, dbInfo.SecondaryDatabaseName);
					ShowMessage("Success.");

					ShowMessage("Dropping secondary database.");
					DbManager.DropDatabase(connection, dbInfo.SecondaryDatabaseName);
					ShowMessage("Success.\r\n");
				}
			}

			ShowMessage("Secondary database/s dropped successfully.");
		}

		#region Log Shipping Configuration

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void SupplyDetailsOfPrimaryServerAndDatabases(System.Data.Common.DbTransaction transaction, List<DatabaseInfo> databasesToProcess)
		{
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction.Connection, nameof(transaction.Connection));
			Argument.NotNull(databasesToProcess, nameof(databasesToProcess));

			foreach (DatabaseInfo dbInfo in databasesToProcess)
			{
				if (dbInfo != null && dbInfo.SetupInfo.PrimaryServer != null)
				{
					using (var cmd = DbManager.NewSqlCommand("master.sys.sp_add_log_shipping_secondary_primary", transaction.Connection))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Transaction = transaction;

						cmd.Parameters.Add(cmd.CreateParameter("@primary_server", dbInfo.SetupInfo.PrimaryServer.FullInstanceName));
						cmd.Parameters.Add(cmd.CreateParameter("@copy_job_name", string.Format("LSCopy_{0}_{1}", dbInfo.SetupInfo.PrimaryServer.FullInstanceName, dbInfo.DatabaseName)));
						cmd.Parameters.Add(cmd.CreateParameter("@restore_job_name", string.Format("LSRestore_{0}_{1}", dbInfo.SetupInfo.PrimaryServer.FullInstanceName, dbInfo.DatabaseName)));
						cmd.Parameters.Add(cmd.CreateParameter("@primary_database", dbInfo.DatabaseName));
						cmd.Parameters.Add(cmd.CreateParameter("@backup_source_directory", dbInfo.SetupInfo.BackupSourceDirectory));
						cmd.Parameters.Add(cmd.CreateParameter("@backup_destination_directory", dbInfo.SetupInfo.BackupLocalCopyDirectory));
						cmd.Parameters.Add(cmd.CreateParameter("@overwrite", SqlBooleanTrue));

						var copyJobId = cmd.CreateParameter("@copy_job_id", SqlDbType.UniqueIdentifier);
						copyJobId.Direction = ParameterDirection.Output;
						cmd.Parameters.Add(copyJobId);

						var restoreJobId = cmd.CreateParameter("@restore_job_id", SqlDbType.UniqueIdentifier);
						restoreJobId.Direction = ParameterDirection.Output;
						cmd.Parameters.Add(restoreJobId);

						cmd.ExecuteNonQuery();

						if (cmd.Parameters["@copy_job_id"] != null && cmd.Parameters["@copy_job_id"].Value != null)
						{
							dbInfo.CopyJobId = (Guid)cmd.Parameters["@copy_job_id"].Value;
						}

						if (cmd.Parameters["@restore_job_id"] != null && cmd.Parameters["@restore_job_id"].Value != null)
						{
							dbInfo.RestoreJobId = (Guid)cmd.Parameters["@restore_job_id"].Value;
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sql query")]
		void AddSecondaryDatabases(System.Data.Common.DbTransaction transaction, List<DatabaseInfo> databasesToProcess)
		{
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction.Connection, nameof(transaction.Connection));
			Argument.NotNull(databasesToProcess, nameof(databasesToProcess));

			foreach (DatabaseInfo dbInfo in databasesToProcess)
			{
				if (dbInfo != null && dbInfo.SetupInfo.PrimaryServer != null)
				{
					using (var cmd = DbManager.NewSqlCommand("master.sys.sp_add_log_shipping_secondary_database", transaction.Connection))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Transaction = transaction;

						cmd.Parameters.Add(cmd.CreateParameter("@secondary_database", dbInfo.SecondaryDatabaseName));
						cmd.Parameters.Add(cmd.CreateParameter("@primary_server", dbInfo.SetupInfo.PrimaryServer.FullInstanceName));
						cmd.Parameters.Add(cmd.CreateParameter("@primary_database", dbInfo.DatabaseName));
						cmd.Parameters.Add(cmd.CreateParameter("@restore_mode", StandByRestoreMode));
						cmd.Parameters.Add(cmd.CreateParameter("@disconnect_users", SqlBooleanTrue));
						cmd.Parameters.Add(cmd.CreateParameter("@restore_threshold", RestoreThresholdInMinutes));
						cmd.Parameters.Add(cmd.CreateParameter("@threshold_alert_enabled", SqlBooleanTrue));
						cmd.Parameters.Add(cmd.CreateParameter("@overwrite", SqlBooleanTrue));

						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		void ChangeLogShippingDetails(System.Data.Common.DbTransaction transaction, LogShippingInfo info, List<DatabaseInfo> databasesToProcess)
		{
			Argument.NotNull(info, nameof(info));
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction.Connection, nameof(transaction.Connection));
			Argument.NotNull(databasesToProcess, nameof(databasesToProcess));

			foreach (DatabaseInfo dbInfo in databasesToProcess)
			{
				if (dbInfo != null)
				{
					using (var cmd = DbManager.NewSqlCommand("master.sys.sp_change_log_shipping_secondary_primary", transaction.Connection))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Transaction = transaction;

						cmd.Parameters.Add(cmd.CreateParameter("@primary_server", info.PrimaryServerFromLSMetadata));
						cmd.Parameters.Add(cmd.CreateParameter("@primary_database", dbInfo.DatabaseName));
						cmd.Parameters.Add(cmd.CreateParameter("@backup_source_directory", info.BackupSourceDirectory));
						cmd.Parameters.Add(cmd.CreateParameter("@backup_destination_directory", info.BackupLocalCopyDirectory));

						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		void RemoveLogShipping(System.Data.Common.DbTransaction transaction, LogShippingInfo info)
		{
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction.Connection, nameof(transaction.Connection));
			Argument.NotNull(info, nameof(info));

			foreach (DatabaseInfo dbInfo in info.GetDatabaseInfoListToProcess())
			{
				if (dbInfo != null)
				{
					using (var cmd = DbManager.NewSqlCommand("master.sys.sp_delete_log_shipping_secondary_database", transaction.Connection))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Transaction = transaction;

						cmd.Parameters.Add(cmd.CreateParameter("@secondary_database", dbInfo.SecondaryDatabaseName));

						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		#region Job

		#region Job Schedule

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void AddJobSchedule(System.Data.Common.DbTransaction transaction, Guid jobUID, JobScheduleSettings settings)
		{
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction.Connection, nameof(transaction.Connection));

			using (var cmd = DbManager.NewSqlCommand("msdb.dbo.sp_add_schedule", transaction.Connection))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction;

				cmd.Parameters.Add(cmd.CreateParameter("@schedule_name", settings.ScheduleName));
				cmd.Parameters.Add(cmd.CreateParameter("@enabled", SqlBooleanTrue));
				cmd.Parameters.Add(cmd.CreateParameter("@freq_type", (int)settings.FrequencyType));
				cmd.Parameters.Add(cmd.CreateParameter("@freq_interval", settings.FrequencyInterval));
				cmd.Parameters.Add(cmd.CreateParameter("@freq_subday_type", (int)settings.FrequencySubdayType));
				cmd.Parameters.Add(cmd.CreateParameter("@freq_subday_interval", settings.FrequencySubdayInterval));
				cmd.Parameters.Add(cmd.CreateParameter("@active_start_time", int.Parse(settings.StartTime.ToString("HHmmss"))));

				var scheduleIdParam = cmd.CreateParameter("@schedule_id", SqlDbType.Int);
				scheduleIdParam.Direction = ParameterDirection.Output;
				cmd.Parameters.Add(scheduleIdParam);

				cmd.ExecuteNonQuery();

				if (cmd.Parameters != null && cmd.Parameters["@schedule_id"] != null && cmd.Parameters["@schedule_id"].Value != null)
				{
					int scheduleID = (int)cmd.Parameters["@schedule_id"].Value;
					AttachJobSchedule(transaction, jobUID, scheduleID);
				}
			}
		}

		void AttachJobSchedule(System.Data.Common.DbTransaction transaction, Guid jobUID, int scheduleID)
		{
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction.Connection, nameof(transaction.Connection));

			using (var cmd = DbManager.NewSqlCommand("msdb.dbo.sp_attach_schedule", transaction.Connection))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction;

				cmd.Parameters.Add(cmd.CreateParameter("@job_id", jobUID));
				cmd.Parameters.Add(cmd.CreateParameter("@schedule_id", scheduleID));

				cmd.ExecuteNonQuery();
			}
		}

		#region Schedule Settings

		struct JobScheduleSettings
		{
			public string ScheduleName { get; set; }
			public FreqType FrequencyType { get; set; }
			public int FrequencyInterval { get; set; }
			public FreqSubdayType FrequencySubdayType { get; set; }
			public int FrequencySubdayInterval { get; set; }
			public DateTime StartTime { get; set; }
		}

		enum FreqType
		{
			Once = 1,
			Daily = 4,
			Weekly = 8,
			Monthly = 16,
			MonthlyRelativeToFreqInterval = 32,
			OnSQLServerAgentStarts = 64,
			OnComputerIsIdle = 128
		}

		enum FreqSubdayType
		{
			Minutes = 0x4,
			Hours = 0x8
		}

		#endregion

		#endregion

		#region Job Step

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void AddJobStep(System.Data.Common.DbTransaction transaction, Guid jobUID, JobStepSettings settings)
		{
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction.Connection, nameof(transaction.Connection));

			using (var cmd = DbManager.NewSqlCommand("msdb.dbo.sp_add_jobstep", transaction.Connection))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction;

				cmd.Parameters.Add(cmd.CreateParameter("@job_id", jobUID));
				cmd.Parameters.Add(cmd.CreateParameter("@step_name", settings.StepName));
				cmd.Parameters.Add(cmd.CreateParameter("@step_id", settings.StepID));
				cmd.Parameters.Add(cmd.CreateParameter("@on_success_action", (int)settings.OnSuccessAction));
				cmd.Parameters.Add(cmd.CreateParameter("@on_fail_action", (int)Action.QuitWithFailure));
				cmd.Parameters.Add(cmd.CreateParameter("@os_run_priority", value: 0));
				cmd.Parameters.Add(cmd.CreateParameter("@subsystem", "TSQL"));
				cmd.Parameters.Add(cmd.CreateParameter("@command", settings.DbCommand));
				cmd.Parameters.Add(cmd.CreateParameter("@database_name", DbManager.MasterDbName));

				cmd.ExecuteNonQuery();
			}
		}

		void UpdateJobStep(System.Data.Common.DbTransaction transaction, Guid jobUID, int stepID)
		{
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction.Connection, nameof(transaction.Connection));

			using (var cmd = DbManager.NewSqlCommand("msdb.dbo.sp_update_jobstep", transaction.Connection))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction;

				cmd.Parameters.Add(cmd.CreateParameter("@job_id", jobUID));
				cmd.Parameters.Add(cmd.CreateParameter("@step_id", stepID));
				cmd.Parameters.Add(cmd.CreateParameter("@on_success_action", (int)Action.GoToNextStep));
				cmd.Parameters.Add(cmd.CreateParameter("@on_fail_action", (int)Action.GoToStepWithId));
				cmd.Parameters.Add(cmd.CreateParameter("@on_fail_step_id", 4));

				cmd.ExecuteNonQuery();
			}
		}

		#region Step Settings

		struct JobStepSettings
		{
			public string StepName { get; set; }
			public int StepID { get; set; }
			public Action OnSuccessAction { get; set; }
			public string DbCommand { get; set; }
		}

		enum Action
		{
			QuitWithSuccess = 1,
			QuitWithFailure = 2,
			GoToNextStep = 3,
			GoToStepWithId = 4
		}

		#endregion

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static void EnableJob(System.Data.Common.DbTransaction transaction, Guid jobUID)
		{
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction.Connection, nameof(transaction.Connection));

			using (var cmd = DbManager.NewSqlCommand("msdb.dbo.sp_update_job", transaction.Connection))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction;

				cmd.Parameters.Add(cmd.CreateParameter("@job_id", jobUID));
				cmd.Parameters.Add(cmd.CreateParameter("@enabled", SqlBooleanTrue));

				cmd.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static void DisableJob(System.Data.Common.DbTransaction transaction, Guid jobUID)
		{
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction.Connection, nameof(transaction.Connection));

			using (var cmd = DbManager.NewSqlCommand("msdb.dbo.sp_update_job", transaction.Connection))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction;

				cmd.Parameters.Add(cmd.CreateParameter("@job_id", jobUID));
				cmd.Parameters.Add(cmd.CreateParameter("@enabled", value: SqlBooleanFalse));

				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region EnterpriseConnectionProgramNames

		protected readonly List<string> EnterpriseConnectionProgramNames = new List<string> { "CargoWiseOne", "CargoWiseOneServiceHost", "CargoWiseOneServiceRunner" };

		protected string EnterpriseConnectionProgramNamesAsSQLString
		{
			get
			{
				return enterpriseConnectionProgramNamesAsSQLString ?? (enterpriseConnectionProgramNamesAsSQLString = "'" + string.Join("', '", EnterpriseConnectionProgramNames) + "'");
			}
		}
		string enterpriseConnectionProgramNamesAsSQLString;

		#endregion

		#region Constants

		const int StandByRestoreMode = 1;
		const int RestoreThresholdInMinutes = 45;
		const int SqlBooleanTrue = 1;
		const int SqlBooleanFalse = 0;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected const string LockStandbyDbCommand = @"
IF NOT EXISTS(SELECT null FROM msdb.sys.extended_properties WHERE name = '{0}')
BEGIN
	EXEC msdb.sys.sp_addextendedproperty 
		@name = '{0}',
		@level0type = 'SCHEMA', 
		@level0name = 'dbo'
END";

		protected const string WaitUntilNoUserCommand = @"
DECLARE @DatabaseId int; SET @DatabaseId = DB_ID('{0}');
DECLARE @LoopCount int; SET @LoopCount = 0;
DECLARE @CheckTime DATETIME; SET @CheckTime = getdate();

WHILE (@LoopCount < 2000) AND EXISTS(
	SELECT * FROM sys.dm_tran_locks l
	INNER JOIN sys.dm_exec_sessions s ON s.session_id = l.request_session_id
	LEFT JOIN sys.dm_exec_requests r ON r.session_id = s.session_id
	WHERE l.resource_database_id = @DatabaseId
	AND s.program_name in ({1})
	AND s.login_time < @CheckTime
	AND
	(
		r.start_time IS NOT NULL
		OR dateadd(minute, 10, s.last_request_end_time) > getdate()
	)
)
BEGIN
	SET @LoopCount = @LoopCount + 1;
	WAITFOR DELAY '00:00:10';
END;
";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected const string UnlockStandbyDbCommand = @"
IF EXISTS(SELECT null FROM msdb.sys.extended_properties WHERE name = '{0}')
BEGIN
	EXEC msdb.sys.sp_dropextendedproperty 
		@name = '{0}',
		@level0type = 'SCHEMA', 
		@level0name = 'dbo'
END
";

		#endregion

		#endregion

		#region ShowMessage

		void ShowMessage(string message)
		{
			if (OnShowMessage != null)
			{
				OnShowMessage(message);
			}
		}

		public event NotificationDelegate OnShowMessage;

		#endregion

		public const string RestoreLockPropertyPrefix = "PtyRestoreLock__";
	}

	public delegate void NotificationDelegate(string message);
	public delegate void ActionResultDelegate();
}
