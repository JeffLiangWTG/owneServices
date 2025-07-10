using System;
using System.Data;
using CargoWise.Application;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Data.SqlServer
{
	public class CdcLatencyProvider : IBacklogInfoProvider
	{
		public CdcLatencyProvider()
		{
		}

		long? acceptableBacklog;
		public long AcceptableBacklog
		{
			get
			{
				if (!acceptableBacklog.HasValue)
				{
					acceptableBacklog = ObjectFactory.Get<ISystemDataRegistry>().CdcLatencyProviderAcceptableBacklog.Value;
				}

				return acceptableBacklog.Value;
			}
		}

		public TimeSpan[] Timespans => new TimeSpan[]
		{
			TimeSpan.FromSeconds(3),
			TimeSpan.FromSeconds(5),
			TimeSpan.FromSeconds(7),
			TimeSpan.FromSeconds(11),
		};

		public AttemptInGettingBacklog GetCurrentBacklog()
		{
			var backLogResult = new BacklogResult();
			backLogResult.BacklogSize = Duration;
			backLogResult.BacklogDescription = $"Cdc transactions queued:{backLogResult.BacklogSize}";

			var result = new AttemptInGettingBacklog(true, string.Empty, backLogResult);
			return result;
		}

		internal virtual int Duration => GetQueueSize();

		// This is a copy of CdcScanner.GetQueueSize() for development to avoid circular references
			// http://crikey.wtg.zone/TestResults/b56430af-71ba-4a1e-822b-573d654b6702
		// Before we push this, we should find a better solution
		public static int GetQueueSize()
		{
			var sqlText = @"
				DECLARE @BacklogCount int
				DECLARE @LatencyResult TABLE
					(
					[database] sysname,
					[replicated transactions] int,
					[replication rate trans/sec] float,
					[replication latency (sec)] float,
					replbeginlsn binary(10),
					replnextlsn binary(10)
					)
				INSERT INTO @LatencyResult ([database], [replicated transactions], [replication rate trans/sec], [replication latency (sec)], replbeginlsn, replnextlsn) EXEC sp_replcounters;
				SET @BacklogCount = (SELECT TOP 1 [replicated transactions] FROM @LatencyResult WHERE [database] = @CurrentDatabase)

				SELECT 
					CASE WHEN @BacklogCount IS NULL THEN 0 ELSE @BacklogCount 
				END
			";

			try
			{
				using (var connection = Db.NewAdminConnection())
				using (var cmd = connection.Command(sqlText))
				{
					cmd.AddParameter("@CurrentDatabase", SqlDbType.NVarChar, Db.DatabaseName);
					return (int)cmd.ExecuteScalar();
				}
			}
			catch (System.Data.Common.DbException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName)
			{
				return 0;
			}
		}
	}
}
