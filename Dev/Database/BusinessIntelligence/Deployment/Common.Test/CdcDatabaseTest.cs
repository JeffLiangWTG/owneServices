using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ChangeDataCapture.Common.Testing
{
	class CdcDatabaseTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestDisableCaptureInstances_CdcDisabled()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				CdcDatabase.Enable(testConnection, Db.DatabaseName);

				var cdcTable = new CdcTableForTesting("dbo", "GlbStaff");
				Assert("CDC should not be enabled on test table on creation", !cdcTable.IsCdcEnabled(testConnection));
				var isChangeTrackingEnabledOriginal = IsChangeTrackingEnabled(testConnection);

				CdcDatabase.DisableCaptureInstancesAndEnableChangeTrackingIfRequired(testConnection, shouldEnableChangeTracking: false);
				Assert("CDC should not be enabled on test table after DisableCaptureInstances", !cdcTable.IsCdcEnabled(testConnection));
				AssertEquals("Change Tracking should not be changed", isChangeTrackingEnabledOriginal, IsChangeTrackingEnabled(testConnection));

				cdcTable.EnableCdc(testConnection);
				CdcDatabase.DisableCaptureInstancesAndEnableChangeTrackingIfRequired(testConnection, shouldEnableChangeTracking: false);
				Assert("If CDC is enabled on the table during another function, it should be disabled on next DisableCaptureInstances", !cdcTable.IsCdcEnabled(testConnection));
			}
		}

		[UseSnapshotProtection]
		public void TestDisableCaptureInstancesWithChangeTracking()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				CdcDatabase.Enable(testConnection, Db.DatabaseName);

				var cdcTable = new CdcTableForTesting("dbo", "GlbStaff");
				cdcTable.EnableCdc(testConnection);

				CdcDatabase.DisableCaptureInstancesAndEnableChangeTrackingIfRequired(testConnection, shouldEnableChangeTracking: true);

				Assert("CDC should not be enabled on test table after DisableCaptureInstances", !cdcTable.IsCdcEnabled(testConnection));
				Assert("Change Tracking should be enabled", IsChangeTrackingEnabled(testConnection));
			}
		}

		bool IsChangeTrackingEnabled(AdminConnection testConnection)
		{
			var sqlText = @"
				IF EXISTS (
					select * from sys.change_tracking_tables where object_id = OBJECT_ID(N'dbo.GlbStaff')
				)
				SELECT 1 ELSE SELECT 0
			";
			var changeTrackingIsEnabled = Convert.ToBoolean(testConnection.ExecuteScalar<int>(sqlText));
			return changeTrackingIsEnabled;
		}

		[UseSnapshotProtection]
		public void TestDisableCaptureInstances()
		{
			string dbName = Db.DatabaseName;

			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					EnableCDCForDb(dbName, testConnection);
					testConnection.BeginTransaction();
					var processHeaderCDCTable = new CdcTable(ProcessHeaderSchema.Constants.SqlSchemaName, ProcessHeaderSchema.Constants.TableName);
					var glbStaffCDCTable = new CdcTable(GlbStaffSchema.Constants.SqlSchemaName, GlbStaffSchema.Constants.TableName);
					processHeaderCDCTable.EnableCdc(testConnection);

					string sqlText = $@"Select count([name]) from sys.tables where is_tracked_by_cdc = 1 and [name] in ('{ProcessHeaderSchema.Constants.TableName}','{GlbStaffSchema.Constants.TableName}')";
					var qty = testConnection.ExecuteScalar(sqlText);

					AssertEquals($"Is CDC enabled [{dbName}]?", true, CdcDatabase.IsEnabled(testConnection, dbName));
					AssertEquals($"{ProcessHeaderSchema.Constants.TableName} and {GlbStaffSchema.Constants.TableName} are enabled for CDC", 1, qty);

					CdcDatabase.DisableCaptureInstances(testConnection);

					qty = testConnection.ExecuteScalar(sqlText);

					AssertEquals($"{ProcessHeaderSchema.Constants.TableName} and {GlbStaffSchema.Constants.TableName} are not enabled for CDC", 0, qty);
				}
				finally
				{
					testConnection.RollbackTransaction();
					CdcDatabase.Disable(testConnection, dbName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnableCdc()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				EnableCDCForDb(Db.DatabaseName, testConnection);

				AssertEquals("Is CDC enabled?", true, CdcDatabase.IsEnabled(testConnection, Db.DatabaseName));
				AssertCdcTriggerEnabled(testConnection, Db.DatabaseName, false);
				AssertCdcMemberOfDbOwnerRole(testConnection, Db.DatabaseName);
			}
		}

		[UseSnapshotProtection]
		public void TestDisableCdcJobs()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				EnableCDCForDb(Db.DatabaseName, testConnection);

				CleanupOldCdcJobs(testConnection);
				CreateCdcJobs(testConnection, Db.DatabaseName);
				AssertCdcJobsEnabled(testConnection, Db.DatabaseName, true);

				CdcDatabase.DisableAndStopJobsAndTrigger(testConnection, Db.DatabaseName);
				AssertCdcJobsEnabled(testConnection, Db.DatabaseName, false);
				AssertCdcTriggerEnabled(testConnection, Db.DatabaseName, false);
			}
		}

		[UseSnapshotProtection]
		public void TestDisableCdcJobsWhenNoCdcJobsExist()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				EnableCDCForDb(Db.DatabaseName, testConnection);

				CleanupOldCdcJobs(testConnection);
				CreateCdcJobs(testConnection, Db.DatabaseName);
				AssertCdcJobsTableExists(testConnection, true);

				DropCdcJobs(testConnection, Db.DatabaseName);
				AssertCdcJobsTableExists(testConnection, false);

				CdcDatabase.DisableAndStopJobsAndTrigger(testConnection, Db.DatabaseName);
				AssertCdcJobsEnabled(testConnection, Db.DatabaseName, false);
				AssertCdcTriggerEnabled(testConnection, Db.DatabaseName, false);
			}
		}

		[ExpectNoExceptions]
		public void TestIsCdcJobEnabled()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.IsCdcJobEnabled(testConnection, Db.DatabaseName, "capture");
			}
		}

		[ExpectNoExceptions]
		public void TestIsCdcJobRunning()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.IsCdcJobRunning(testConnection, Db.DatabaseName, "capture");
			}
		}

		[UseSnapshotProtection]
		public void TestNullableEndLsnExists()
		{
			using (var connection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Disable(connection, Db.DatabaseName);
				}

				AssertEquals("CDC is disabled on the database", false, CdcDatabase.NullableEndLsnExists(connection, Db.DatabaseName));

				CdcDatabase.Enable(connection, Db.DatabaseName);

				AssertEquals("No CDC tables exist", false, CdcDatabase.NullableEndLsnExists(connection, Db.DatabaseName));

				connection.ExecuteNonQuery("EXEC sys.sp_cdc_enable_table @source_schema = N'dbo', @source_name = N'GlbStaff', @role_name = NULL, @capture_instance = N'dbo_GlbStaff'");

				AssertEquals("CDC tables exist", true, CdcDatabase.NullableEndLsnExists(connection, Db.DatabaseName));
			}
		}

		#region Implementation

		static void EnableCDCForDb(string testRefDb, AdminConnection testConnection)
		{
			if (CdcDatabase.IsEnabled(testConnection, testRefDb))
			{
				CdcDatabase.Disable(testConnection, testRefDb);
			}
			CdcDatabase.Enable(testConnection, testRefDb);
		}

		void CleanupOldCdcJobs(DbConnection conn)
		{
			var sqlText =
@"declare @SqlText nvarchar(max) = '';
SELECT @SqlText = @SqlText + 'EXEC msdb.dbo.sp_delete_job @job_name = N''' + name + ''';'
FROM msdb.dbo.sysjobs
IF (@SqlText is not null) EXEC(@SqlText);

IF EXISTS (SELECT null FROM msdb.sys.objects WHERE name = 'cdc_jobs')
BEGIN
	DELETE FROM msdb.dbo.cdc_jobs
END";

			conn.ExecuteNonQuery(sqlText);
		}

		void CreateCdcJobs(DbConnection conn, string dbName)
		{
			var sqlText = String.Format(
@"EXEC [{0}].sys.sp_cdc_add_job @job_type = N'capture';
EXEC [{0}].sys.sp_cdc_add_job @job_type = N'cleanup';

EXEC [{0}].sys.sp_cdc_start_job @job_type = N'capture';
EXEC [{0}].sys.sp_cdc_start_job @job_type = N'cleanup';", dbName);

			conn.ExecuteNonQuery(sqlText);
		}

		void DropCdcJobs(DbConnection conn, string dbName)
		{
			var sqlText = String.Format(
@"EXEC [{0}].sys.sp_cdc_drop_job @job_type = N'capture';
EXEC [{0}].sys.sp_cdc_drop_job @job_type = N'cleanup';", dbName);

			conn.ExecuteNonQuery(sqlText);
		}

		void AssertCdcJobsTableExists(DbConnection conn, bool exists)
		{
			var sqlText = @"IF EXISTS (SELECT null FROM msdb.sys.objects WHERE name = 'cdc_jobs') SELECT 1 ELSE SELECT 0;";

			AssertEquals("msdb.dbo.cdc_jobs exists?", exists, Convert.ToBoolean(conn.ExecuteScalar(sqlText)));
		}

		void AssertCdcMemberOfDbOwnerRole(DbConnection conn, string dbName)
		{
			string sqlText = String.Format(@"
				IF EXISTS (
					SELECT null
					FROM [{0}].sys.database_role_members drm
					INNER JOIN [{0}].sys.database_principals r ON r.principal_id = drm.role_principal_id
					INNER JOIN [{0}].sys.database_principals m ON m.principal_id = drm.member_principal_id
					WHERE r.name = 'db_owner'
					AND m.name = 'cdc'
				) SELECT 1 ELSE SELECT 0", dbName);
			var isMember = Convert.ToBoolean(conn.ExecuteScalar(sqlText));
			AssertEquals("Is cdc user a member of the db_owner database role?", true, isMember);
		}

		void AssertCdcJobsEnabled(DbConnection conn, string dbName, bool enabled)
		{
			CombineAssertions(() =>
			{
				string sqlText = String.Format(@"
IF EXISTS (SELECT null FROM msdb.sys.objects WHERE name = 'cdc_jobs')
BEGIN
	select job.name
	from msdb.dbo.sysjobs job
	inner join msdb.dbo.cdc_jobs cdcjob
	on cdcjob.job_id = job.job_id
	inner join sys.databases db
	on db.database_id = cdcjob.database_id
	where
		db.name = '{0}' and job.enabled = 1;
END", dbName);
				var enabledJobs = DataUtils.GetListOfValuesFromQuery(conn, sqlText);
				AssertEquals(String.Format("CDC jobs enabled?\r\n{0}", String.Join(", ", enabledJobs)), enabled, enabledJobs.Any());

				if (!enabled)
				{
					sqlText = String.Format(@"
IF EXISTS (SELECT null FROM msdb.sys.objects WHERE name = 'cdc_jobs')
BEGIN
	select job.name
	from msdb.dbo.sysjobs job
	inner join msdb.dbo.sysjobactivity activity
	on job.job_id = activity.job_id 
	inner join msdb.dbo.syssessions sess
	on sess.session_id = activity.session_id
	inner join msdb.dbo.cdc_jobs cdcjob
	on cdcjob.job_id = job.job_id
	inner join sys.databases db
	on db.database_id = cdcjob.database_id
	where
		sess.agent_start_date = (SELECT MAX(agent_start_date) FROM msdb.dbo.syssessions s WHERE s.session_id = sess.session_id) and
		activity.run_requested_date is not null and
		activity.stop_execution_date is null and
		db.name = '{0}'
END;", dbName);
					var runningJobs = DataUtils.GetListOfValuesFromQuery(conn, sqlText);
					AssertEquals(String.Format("CDC jobs started?\r\n{0}", String.Join(", ", runningJobs)), enabled, runningJobs.Any());
				}
			});
		}

		void AssertCdcTriggerEnabled(AdminConnection conn, string testRefDb, bool enabled)
		{
		var sqlText = $@"
SELECT is_disabled
FROM [{testRefDb}].sys.triggers
WHERE name = 'tr_MScdc_ddl_event'";
			var isTriggerDisabled = Convert.ToBoolean(conn.ExecuteScalar(sqlText));
			AssertEquals($"CDC trigger is enabled", enabled, !isTriggerDisabled);
		}

		#endregion
	}
}
