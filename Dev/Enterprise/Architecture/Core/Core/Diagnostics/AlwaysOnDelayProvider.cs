using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Data.SqlServer
{
	public class AlwaysOnDelayProvider : IBacklogInfoProvider
	{
		public AlwaysOnDelayProvider(string databaseName = "")
		{
			this.databaseName = string.IsNullOrWhiteSpace(databaseName) ? Db.DatabaseName : databaseName;
		}

		readonly string databaseName;

		public virtual AttemptInGettingBacklog GetCurrentBacklog()
		{
			var currentBacklog = new BacklogResult();

			var attemptToGetReplicas = GetSecondaryAsyncReplicas();

			if (attemptToGetReplicas.Item1 && attemptToGetReplicas.Item3.Count > 0 && AcceptableBacklog > 0)
			{
				currentBacklog = GetMaximumBacklog(attemptToGetReplicas.Item3);
			}

			return new AttemptInGettingBacklog(attemptToGetReplicas.Item1, attemptToGetReplicas.Item2, currentBacklog);
		}

		long? acceptableBacklog;
		public long AcceptableBacklog
		{
			get
			{
				if (!acceptableBacklog.HasValue)
				{
					acceptableBacklog = ObjectFactory.Get<ISystemDataRegistry>().ReportingDbServerThreshold.Ticks;
				}

				return acceptableBacklog.Value;
			}
		}

		public TimeSpan[] Timespans
		{
			get
			{
				return new[]
				{
					TimeSpan.FromSeconds(10),
					TimeSpan.FromSeconds(20),
					TimeSpan.FromSeconds(30),
					TimeSpan.FromSeconds(60),
					TimeSpan.FromSeconds(120),
				};
			}
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is a log information")]
		internal protected virtual Tuple<bool, string, List<string>> GetSecondaryAsyncReplicas()
		{
			var failureReason = string.Empty;
			var replicas = new List<string>();

			using (var connection = Db.NewAdminConnection())
			using (connection.UseMasterDb())
			{
				var sql = @"
SELECT
	rep.replica_server_name
	, is_healthy = CASE WHEN sts.synchronization_health <> 0 THEN 1 ELSE 0 END
	, is_async   = 1 - rep.availability_mode
FROM
	sys.dm_hadr_database_replica_states AS sts
	JOIN sys.availability_replicas      AS rep ON rep.replica_id = sts.replica_id
WHERE 1=1
	AND sts.database_id = DB_ID(@DbName)
	AND sts.is_local = 0

";

				using (var cmd = connection.Command(sql))
				{
					cmd.AddParameter("@DbName", SqlDbType.NVarChar, 128, databaseName);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var server_name = DataUtils.GetDbSeverFullDomainNameIncludingSqlPort((string)reader["replica_server_name"]);
							var is_healthy = Convert.ToBoolean(reader["is_healthy"], CultureInfo.InvariantCulture);
							var is_async = Convert.ToBoolean(reader["is_async"], CultureInfo.InvariantCulture);

							if (!is_healthy)
							{
								failureReason = string.Format(CultureInfo.InvariantCulture, "AlwaysOn synchronization delay (The current {0} replica is not healthy: {1})", is_async ? "ASYNC" : "SYNC", server_name);
								return new Tuple<bool, string, List<string>>(false, failureReason, replicas);
							}

							if (is_async)
							{
								replicas.Add(server_name);
							}
						}
					}
				}
			}

			return new Tuple<bool, string, List<string>>(true, failureReason, replicas);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is a log information")]
		BacklogResult GetMaximumBacklog(List<string> replicas)
		{
			var backlogResult = new BacklogResult();
			foreach (var serverName in replicas)
			{
				var sql = "SELECT MAX(SL_PostedTimeUtc) FROM dbo.StmALog;";
				var primaryServerLastPost = new ZDateTime(Db.Connection.ExecuteScalar(sql));

				try
				{
					using (var connection = Db.NewAdminConnection(serverName, Db.DatabaseName))
					{
						var secondaryServerLastPost = new ZDateTime(connection.ExecuteScalar(sql));
						var currentDelay = (primaryServerLastPost - secondaryServerLastPost).Ticks;
						if (backlogResult.BacklogSize < currentDelay)
						{
							backlogResult.BacklogSize = currentDelay;
						}
					}
				}
				catch (System.Data.Common.DbException sqlException)
				{
					throw new AlwaysOnDelayProviderException(
						$@"{nameof(AlwaysOnDelayProvider)} has encountered an error to {nameof(GetMaximumBacklog)} on replica: {serverName}, database: {Db.DatabaseName}.
{sqlException.Message}.
",
						sqlException);
				}
			}

			backlogResult.BacklogDescription = string.Format(CultureInfo.InvariantCulture,
				"AlwaysOn synchronization delay (current: {0} min, acceptable: {1} min)"
				, TimeSpan.FromTicks(backlogResult.BacklogSize).TotalMinutes // 0
				, TimeSpan.FromTicks(AcceptableBacklog).TotalMinutes         // 1
				);

			return backlogResult;
		}

		#endregion // Implementation
	}
}
