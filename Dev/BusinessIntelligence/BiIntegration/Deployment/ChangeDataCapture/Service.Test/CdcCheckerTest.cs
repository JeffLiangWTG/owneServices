using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ChangeDataCapture.Service
{
	class CdcCheckerTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestCheckCdcJobStatus()
		{
			using (var connection = Db.NewAdminConnection())
			{
				if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Enable(connection, Db.DatabaseName);
				}
				CreateCdcCleanupJob(connection);

				try
				{
					CombineAssertions("Before checking", () =>
					{
						AssertEquals("CDC clean up enabled?", true, IsCdcCleanupJobEnabled(connection));
						Int64 defaultRetention = 4320; //3 days
						AssertEquals("CDC clean up retention", defaultRetention, GetCdcCleanupRetentionValue(connection));
					});

					CdcChecker.CheckCdcCleanupJobRunningStatusAndEnforceRetentionPeriod(new LoggerForTest());
					CombineAssertions("After checking", () =>
					{
						AssertEquals("CDC clean up enabled?", false, IsCdcCleanupJobEnabled(connection));
						AssertEquals("CDC clean up retention", 43200, GetCdcCleanupRetentionValue(connection));
					});
				}
				finally
				{
					RemoveCdcCleanupJob(connection);
				}
			}
		}

		void CreateCdcCleanupJob(DbConnection connection)
		{
			try
			{
				var sqlText = String.Format(
	@"
IF EXISTS (SELECT NULL FROM msdb.sys.objects WHERE name = 'cdc_jobs')
BEGIN
	DECLARE @SqlText nvarchar(max) =
	N'IF EXISTS (SELECT NULL FROM msdb.dbo.cdc_jobs WHERE database_id = DB_ID(''{0}''))
		BEGIN
			EXEC [{0}].sys.sp_cdc_drop_job @job_type = N''cleanup'';
		END'
	EXEC (@SqlText)
END

EXEC [{0}].sys.sp_cdc_add_job @job_type = N'cleanup';
EXEC [{0}].sys.sp_cdc_start_job @job_type = N'cleanup';", connection.CurrentDatabase);

				connection.ExecuteNonQuery(sqlText);
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.RequestToStopCdcJobRefused)
			{
			}
		}

		void RemoveCdcCleanupJob(DbConnection connection)
		{
			var sqlText = String.Format(
@"EXEC [{0}].sys.sp_cdc_drop_job @job_type = N'cleanup';", connection.CurrentDatabase);

			connection.ExecuteNonQuery(sqlText);
		}

		bool IsCdcCleanupJobEnabled(DbConnection connection)
		{
			var sqlText = String.Format(
@"SELECT J.enabled
FROM msdb.dbo.cdc_jobs cj with (nolock)
	INNER JOIN msdb.dbo.sysjobs j with (nolock) ON j.job_id = cj.job_id
WHERE cj.job_type = 'cleanup'
	AND cj.database_id = DB_ID('{0}')", connection.CurrentDatabase);

			return Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}

		Int64 GetCdcCleanupRetentionValue(DbConnection connection)
		{
			var sqlText = String.Format(
@"SELECT retention
FROM msdb.dbo.cdc_jobs with (nolock)
WHERE job_type = 'cleanup'
	AND database_id = DB_ID('{0}')", connection.CurrentDatabase);

			return Convert.ToInt64(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}
	}

	class NUnitAssertionTest : TestCase
	{
		[Test]
		public void AllowNUnitModernAssertionsInNUnitCore()
		{
			Assert(true);
		}
	}
}
