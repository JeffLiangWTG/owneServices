using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Data;

namespace Enterprise.ChangeDataCapture.Common
{
	public static class CdcErrorHandler
	{
		public static void HandleCdcScanProcedureException(SqlException ex, int scanAttempts, CdcScanProvider cdcScanProvider)
		{
			var exceptionType = new DbErrorMatch(ex).ExceptionType;
			switch (exceptionType)
			{
				case DbErrorType.InvalidBeginLsnForCdcCommitRecord:
					HandleInvalidBeginLsnForCdcCommitRecord(cdcScanProvider, ex);
					break;

				case DbErrorType.DeadlockError:
					HandleDeadLockError(cdcScanProvider, scanAttempts, ex);
					break;

				case DbErrorType.TimeoutExpired:
					HandleTimeoutError(ex, cdcScanProvider);
					break;

				case DbErrorType.ExpectedParameterNotSupplied:
				case DbErrorType.IncorrectParameterForProcedure:
				case DbErrorType.IncorrectNumberOfParametersForProcedure:
				case DbErrorType.CouldNotFindStoredProcedure:
					cdcScanProvider.CallOldCdcScanProcedureUnsafe();
					break;

				case DbErrorType.LockTimeoutExpired:
				case DbErrorType.CannotRetrievePeerToPeerDbInfo:
					cdcScanProvider.Logger.Warning($"{ex.Message}\r\n{ex.StackTrace}");
					break;

				case DbErrorType.StatementHasBeenTerminated:
					throw new CdcException($"Session ID: {cdcScanProvider.Connection.SPID}\r\n{ex.Message}\r\n{GetCdcErrors(cdcScanProvider.Connection)}");

				default:
					if (ShouldCheckCdcErrors(ex))
					{
						if (IsSqlInternalCdcStructureError(cdcScanProvider.Connection))
						{
							throw new CdcInternalStructureChangedException(ex.Message);
						}
						else if (IsEndLsnNotNullable(cdcScanProvider.Connection))
						{
							SetEndLsnNullableForCdcTables(cdcScanProvider.Connection);
							cdcScanProvider.CallCdcScanProcedureUnsafe();
							break;
						}
						else
						{
							throw new CdcException(GetCdcErrors(cdcScanProvider.Connection));
						}
					}
					else
					{
						throw ex;
					}
			}
		}

		static void HandleTimeoutError(SqlException ex, CdcScanProvider cdcScanProvider)
		{
			cdcScanProvider.CommandTimeout += TimeSpan.FromHours(1);
			if (cdcScanProvider.CommandTimeout <= cdcScanProvider.MaxCommandTimeout)
			{
				cdcScanProvider.CallCdcScanProcedureUnsafe();
			}
			else
			{
				throw ex;
			}
		}

		internal static void HandleDeadLockError(CdcScanProvider scanProcedure, int scanAttempts, Exception ex)
		{
			if (scanAttempts >= 5)
			{
				throw ex;
			}

			var sleepInterval = TimeSpan.FromSeconds(10);
			Thread.Sleep(sleepInterval);

			scanProcedure.ExecuteProcedure(scanAttempts + 1);
		}

		internal static void HandleInvalidBeginLsnForCdcCommitRecord(CdcScanProvider scanProvider, SqlException ex)
		{
			byte[] xactid;
			byte[] xact_seqno;

			using (var adminConnection = Db.NewAdminConnection())
			{
				GetReplDoneParameters(adminConnection, ex, out xactid, out xact_seqno);
				FixBeginLsnForCdcScan(adminConnection, xactid, xact_seqno);
			}

			scanProvider.CallCdcScanProcedureUnsafe();
			HandleDataLoss(scanProvider.Connection, xactid);
		}

		static void GetReplDoneParameters(DbConnection conn, SqlException ex, out byte[] xactid, out byte[] xact_seqno)
		{
			var sqlText = @"
begin try
	exec sp_replcmds
end try
begin catch
end catch";

			using (var reader = conn.Command(sqlText).ExecuteReader())
			{
				if (reader.Read())
				{
					xactid = (byte[])reader["xactid"];
					xact_seqno = (byte[])reader["xact_seqno"];
				}
				else
				{
					throw new CdcException("sp_replcmds returned no rows. Failed to fix begin LSN for CDC scan.", ex);
				}
			}
		}

		static void FixBeginLsnForCdcScan(DbConnection conn, byte[] xactid, byte[] xact_seqno)
		{
			using (var cmd = conn.Command("sp_repldone"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@xactid", SqlDbType.Binary, xactid);
				cmd.AddParameter("@xact_seqno", SqlDbType.Binary, xact_seqno);
				cmd.ExecuteNonQuery();
			}
			ExecReplFlush(conn);
		}

		static void ExecReplFlush(DbConnection conn)
		{
			using (var cmd = conn.Command("sp_replflush"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.ExecuteNonQuery();
			}
		}

		static void HandleDataLoss(DbConnection conn, byte[] xactid)
		{
			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(conn);
			if (!string.IsNullOrEmpty(auditServer))
			{
				using (var auditConnection =
					auditServer.Equals(Db.ServerName, StringComparison.OrdinalIgnoreCase) ?
						conn :
						Db.NewExtraConnectionWithMainDbCredentials(auditServer, Db.SqlMasterDb))
				{
					HandleAuditDataLoss(auditConnection, xactid);
				}
			}

			var dataWarehouseServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(conn);
			if (!string.IsNullOrEmpty(dataWarehouseServer))
			{
				using (var dwConnection =
				dataWarehouseServer.Equals(Db.ServerName, StringComparison.OrdinalIgnoreCase) ?
					conn :
					Db.NewExtraConnectionWithMainDbCredentials(dataWarehouseServer, Db.SqlMasterDb))
				{
					HandleEdwDataLoss(dwConnection);
				}
			}
		}

		static void HandleAuditDataLoss(DbConnection auditConnection, byte[] xactid)
		{
			if (auditConnection.DatabaseExists(Db.AuditDatabaseName))
			{
				using (((ICurrentDbControl)auditConnection).UseDatabase(Db.AuditDatabaseName))
				{
					var sqlText = string.Format(CultureInfo.InvariantCulture, @"
INSERT INTO [{0}].[DataLossLog] (SourceSchemaName, SourceTableName, StartLsn, EndLsn, MinTableLsn, LossType, CreateDateTimeUTC)
VALUES('', '', @xactid, 0x0, 0x0, 'Corrupted', GETUTCDATE())
", BiConstants.BiAdminSchemaName);
					using (var cmd = auditConnection.Command(sqlText))
					{
						cmd.AddParameter("@xactid", SqlDbType.Binary, xactid);
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		static void HandleEdwDataLoss(DbConnection dwConnection)
		{
			if (dwConnection.DatabaseExists(Db.EdwDatabaseName))
			{
				using (((ICurrentDbControl)dwConnection).UseDatabase(Db.EdwDatabaseName))
				{
					var sqlText = string.Format(CultureInfo.InvariantCulture, @"
DELETE FROM [{0}].[MasterState]
UPDATE [{0}].[StagingTableState]
SET CurrentMaxLsn = 0x0,
	CurrentState = 'New',
	InitialLoadRequired = 1
", BiConstants.BiAdminSchemaName);
					dwConnection.ExecuteNonQuery(sqlText);
				}
			}
		}

		internal static bool ShouldCheckCdcErrors(SqlException ex)
		{
			const string logScanErrorRegexPattern = @"(?:Log(?:-|\s*)Scan|Change\s+Data\s+Capture).+Refer\s+to\s+previous\s+errors\s+in\s+the\s+current\s+session"; // string of the exception message
			return Regex.IsMatch(ex.Message, logScanErrorRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

		internal static bool IsSqlInternalCdcStructureError(DbConnection conn)
		{
			string sqlText = @"
					IF exists(
						SELECT 1
						FROM sys.dm_cdc_errors
						WHERE error_message like '%Error converting data type%'
						OR error_message like '%has too many arguments specified%'
						OR error_message like '%Operand type clash%'
					) SELECT 1
					ELSE SELECT 0";
			return Convert.ToBoolean(conn.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}

		internal static bool IsEndLsnNotNullable(DbConnection conn)
		{
			return conn.Exists("FROM sys.dm_cdc_errors WHERE error_message like '%Cannot insert the value NULL into column ''[_][_]$end_lsn''%'");
		}

		internal static string GetCdcErrors(DbConnection conn)
		{
			string sqlText = @"SELECT TOP 5 CONVERT(varchar, entry_time, 22) + ': (' + CONVERT(varchar, error_number) + ') ' + error_message FROM sys.dm_cdc_errors ORDER BY session_id DESC, entry_time DESC";
			var errorMessages = DataUtils.GetListOfValuesFromQuery(conn, sqlText);

			return $"CDC capture failed.\r\n{string.Join("\r\n", errorMessages)}";
		}

		static void SetEndLsnNullableForCdcTables(DbConnection conn)
		{
			var captureInstances = DataUtils.GetListOfValuesFromQuery(conn, @"select object_name(object_id) from sys.columns where name = '__$end_lsn' and is_nullable = 0 and object_schema_name(object_id) = 'cdc'");

			foreach (var captureInstance in captureInstances)
			{
				conn.ExecuteNonQuery($@"ALTER TABLE cdc.[{captureInstance}] ALTER COLUMN __$end_lsn binary(10) NULL");
			}
		}
	}
}
