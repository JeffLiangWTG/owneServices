using System;
using System.Data;
using CargoWise.Data;

namespace Enterprise.ChangeDataCapture.Common
{
	public class CdcScanProvider
	{
		public CdcScanProvider(DbConnection mainDbConnection, CdcScannerLogger logger)
		{
			if (logger == null)
			{
				throw new Exception("CdcScannerLogger cannot be null");
			}
			this.Logger = logger;
			this.Connection = mainDbConnection;
			this.CommandTimeout = DefaultCommandTimeout;
		}

		public TimeSpan DefaultCommandTimeout = TimeSpan.FromMinutes(30);
		public TimeSpan MaxCommandTimeout = TimeSpan.FromHours(2.5);
		public TimeSpan CommandTimeout;

		internal readonly DbConnection Connection;
		public readonly CdcScannerLogger Logger;

		int? maxTransactions;
		internal int MaxTransactions
		{
			get
			{
				return (maxTransactions = maxTransactions ?? DbRegistry.BiCdcMaxTransactions.LoadValue(Connection)).Value;
			}
			set
			{
				maxTransactions = value;
			}
		}

		int? maxScans;
		int MaxScans
		{
			get
			{
				return (maxScans = maxScans ?? DbRegistry.BiCdcMaxScans.LoadValue(Connection)).Value;
			}
		}

		public void ExecuteProcedure(int scanAttempts = 0)
		{
			try
			{
				CallCdcScanProcedureUnsafe();
			}
			catch (SqlException ex)
			{
				CdcErrorHandler.HandleCdcScanProcedureException(ex, scanAttempts, this);
			}
		}

		internal virtual void CallCdcScanProcedureUnsafe()
		{
			Logger.Debug($"Executing internal CDC Scan procedure");
			using (var cmd = Connection.Command("dbo.CdcScan"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@maxtrans", SqlDbType.Int, MaxTransactions);
				cmd.AddParameter("@maxscans", SqlDbType.Int, MaxScans);
				cmd.AddParameter("@continuous", SqlDbType.TinyInt, 0);
				cmd.CommandTimeout = CommandTimeout.Seconds;
				cmd.ExecuteNonQuery();
			}
		}

		internal void CallOldCdcScanProcedureUnsafe()
		{
			Logger.Debug($"Executing Microsoft CDC Scan procedure");
			using (var cmd = Connection.Command("sys.sp_cdc_scan"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@maxtrans", SqlDbType.Int, MaxTransactions);
				cmd.AddParameter("@maxscans", SqlDbType.Int, MaxScans);
				cmd.AddParameter("@continuous", SqlDbType.TinyInt, 0);
				cmd.CommandTimeout = CommandTimeout.Seconds;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
