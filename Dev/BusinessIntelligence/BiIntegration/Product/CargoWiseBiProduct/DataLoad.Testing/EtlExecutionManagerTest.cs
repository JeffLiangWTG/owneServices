using System;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using NUnit.Framework;

namespace CargoWise.Bi.Product.DataLoad.Testing
{
	class EtlExecutionManagerTest : TestCase
	{
		[UseSnapshotProtection([DatabaseType.Main, DatabaseType.EDW], true)]
		public void TestRunEtlWithUnrestrictedWriter()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (var biConnection = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.EdwDatabaseName))
			{
				if (!CdcDatabase.IsEnabled(adminConnection, Db.DatabaseName))
				{
					CdcDatabase.Enable(adminConnection, Db.DatabaseName);
				}

				var table = new CdcTable("dbo", "orgcontact");
				if (!table.IsCdcEnabled(adminConnection))
				{
					table.EnableCdc(adminConnection);
				}

				_ = adminConnection.ExecuteNonQuery("update orgcontact set OC_IsActive = case when OC_IsActive = 1 then 0 else 1 end where oc_pk = (select top 1 oc_pk from orgcontact)");

				var etlManager = EtlExecutionManagerFactory.NewForEdwRecurringExecution(Db.Connection, biConnection, new LoggerForTest());
				AssertNoExceptionThrown(() => etlManager.ExecuteEdwEtlProcess());
			}
		}

		public void TestEtlLockReleasedOnCompletion()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (var biConnection = Db.NewExtraConnectionToMainDb())
			{
				AssertEtlLock("ETL lock before ETL execution", adminConnection, expectedExistingLockInfo: null);

				IEtlAuditExecution etlManager = new EtlExecutionManagerForEtlLockTest(
					biConnection,
					() =>
						AssertEtlLock(
							"ETL lock during execution",
							adminConnection,
							expectedExistingLockInfo: string.Format(CultureInfo.InvariantCulture,
								"Computer [{0}] - Program [{1}] - DB Login [{2}] - Login Time [{3}]",
								System.Environment.MachineName,
								DbConnectionConstants.ApplicationNames.CargoWiseOne,
								((IDbReconnectionHandling)biConnection).LoginName,
								SqlFormatInfo.ToSqlDateTimeString(biConnection.LoginTime)
							)
						)
				);

				etlManager.ExecuteAuditEtlProcess();

				AssertEtlLock("ETL lock after ETL execution", adminConnection, expectedExistingLockInfo: null);
			}
		}

		/// <summary>
		/// If ETL lock is not currently held by anyone, GetSessionHoldingLock returns null.
		/// </summary>
		void AssertEtlLock(string assertionMsg, AdminConnection connectionToGetLockInfo, string expectedExistingLockInfo)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				SELECT
					'Computer [' + es.[host_name] + ']'
					+ ' - Program [' + es.[program_name] + ']'
					+ ' - DB Login [' + es.[login_name] + ']'
					+ ' - Login Time [' + convert(varchar(25), es.[login_time], 121) + ']'
				FROM
					sys.dm_tran_locks tl
					INNER JOIN sys.dm_exec_sessions es ON es.session_id = tl.request_session_id
					INNER JOIN sys.databases db ON db.database_id = tl.resource_database_id
				WHERE 1=1
					AND tl.resource_type = 'APPLICATION'
					AND tl.resource_description like @LockKeyPattern
					AND db.name = @DbName");
			object objResult;
			try
			{
				using (var cmd = connectionToGetLockInfo.Command(sqlText, cmdTimeoutInSeconds: 60))
				{
					cmd.AddParameter("@LockKeyPattern", SqlDbType.NVarChar, 256, "%" + BiServiceTaskLockHandler.LockKey + "%");
					cmd.AddParameter("@DbName", SqlDbType.NVarChar, 128, Db.DatabaseName);
					objResult = cmd.ExecuteScalar();
				}
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.TimeoutExpired)
			{
				objResult = null;
			}

			var actualLockInfo = (objResult == null) ? null : objResult.ToString();
			AssertEquals(assertionMsg, expectedExistingLockInfo, actualLockInfo);
		}

		class EtlExecutionManagerForEtlLockTest : EtlExecutionManager
		{
			public EtlExecutionManagerForEtlLockTest(DbConnection biConnection, Action assertLockInfoAction)
				: base(null, biConnection, new LoggerForTest())
			{
				this.assertLockInfoAction = assertLockInfoAction;
			}

			readonly Action assertLockInfoAction;

			protected override void ExecuteEtl(Action<TsqlScriptRunner> etlAction, TsqlScriptRunner scriptRunner)
			{
				assertLockInfoAction();
			}

			protected override bool ExecuteAuditEtlProcess()
			{
				return false;
			}

			protected override string MessageWhenBiDatabaseDoesNotExist
			{
				get
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
