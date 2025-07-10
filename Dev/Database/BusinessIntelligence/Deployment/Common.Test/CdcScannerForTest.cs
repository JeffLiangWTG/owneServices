using System;
using System.Diagnostics;
using CargoWise.Data;
using CargoWise.Data.Testing;

namespace Enterprise.ChangeDataCapture.Common.Testing
{
	public class CdcScannerForTest : CdcScanner
	{
		public CdcScannerForTest(CdcScannerLoggerForTest logger = null, bool useAdminConnection = false)
		{
			CdcScannerLogger = logger ?? new CdcScannerLoggerForTest();
			this.useAdminConnection = useAdminConnection;
		}

		readonly bool useAdminConnection;

		public void SetCdcScanError(DbErrorType errorType)
		{
			((CdcScanProviderForTest)CdcScanProvider).ErrorType = errorType;
		}

		public override CdcScanProvider CdcScanProvider
		{
			get
			{
				if (cdcScanProvider == null)
				{
					cdcScanProvider = new CdcScanProviderForTest(DedicatedNonPooledCdcConnection, CdcScannerLogger);
				}
				return cdcScanProvider;
			}
		}

		protected override DbConnection DedicatedNonPooledCdcConnection
		{
			get
			{
				if (useAdminConnection)
				{
					return Db.NewAdminConnection();
				}
				else
				{
					return base.DedicatedNonPooledCdcConnection;
				}
			}
		}

		readonly DbErrorType dbErrorType;
		public int NumberOfAttempts { get; set; }

		public void HandleInvalidBeginLsnForCdcCommitRecord_Exposed()
		{
			CdcErrorHandler.HandleInvalidBeginLsnForCdcCommitRecord(CdcScanProvider, null);
		}

		public void LogStatus_Exposed(Stopwatch timer, string newCommitLsn, DateTime newEndTime)
		{
			CdcScannerLogger.LogStatus(timer, newCommitLsn, newEndTime);
		}

		public void CallCdcScanProcedure_Exposed(int scanAttempts)
		{
			CallCdcScanProcedure(scanAttempts);
		}

		public TimeSpan GetLogInterval()
		{
			return CdcScannerLogger.LogInterval;
		}

		internal override void CallCdcScanProcedure(int scanAttempts = 0)
		{
			switch (dbErrorType)
			{
				case DbErrorType.DeadlockError:
					var sqlDeadlockException = SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(1205, 1, 1, "", "A deadlock error.", "", 1)));
					throw sqlDeadlockException;
				case DbErrorType.TimeoutExpired:
					var sqltimeoutException = SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(-2, 0, 11, Db.ServerName, "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0)));
					NumberOfAttempts++;
					throw sqltimeoutException;
				case DbErrorType.StatementHasBeenTerminated:
					var statementTerminatedException = SqlExceptionBuilder.CreateSqlError(3621, 1, 1, Db.Connection.ServerName, "The statement has been terminated.", "", 1);
					throw SqlExceptionBuilder.CreateSqlException(statementTerminatedException);
				default:
					break;
			}

			base.CallCdcScanProcedure(scanAttempts);
		}

		public DbConnection DedicatedNonPooledCDCConnectionExposed => DedicatedNonPooledCdcConnection;
	}

	public class CdcScanProviderForTest : CdcScanProvider
	{
		public CdcScanProviderForTest(DbConnection connection, CdcScannerLogger logger = null)
			: base(connection, logger)
		{
		}

		public DbErrorType? ErrorType;

		internal override void CallCdcScanProcedureUnsafe()
		{
			switch (ErrorType)
			{
				case DbErrorType.DeadlockError:
					var sqlDeadlockException = SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(1205, 1, 1, "", "A deadlock error.", "", 1)));
					throw sqlDeadlockException;
				case DbErrorType.TimeoutExpired:
					var sqltimeoutException = SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(-2, 0, 11, Db.ServerName, "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0)));
					throw sqltimeoutException;
				case DbErrorType.StatementHasBeenTerminated:
					var statementTerminatedException = SqlExceptionBuilder.CreateSqlError(3621, 1, 1, Db.Connection.ServerName, "The statement has been terminated.", "", 1);
					throw SqlExceptionBuilder.CreateSqlException(statementTerminatedException);
				default:
					break;
			}

			base.CallCdcScanProcedureUnsafe();
		}
	}
}
