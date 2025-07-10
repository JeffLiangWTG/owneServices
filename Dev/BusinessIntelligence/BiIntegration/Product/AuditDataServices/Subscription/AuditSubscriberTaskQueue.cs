using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.AuditDataServices.Subscription.Common;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.AuditDataServices.Subscription
{
	// This is the queue for the service tasks inheriting from AuditSubscriberTask, like HVV and GLW
	public abstract class AuditSubscriberTaskQueue : IHostedServiceQueueProvider
	{
		public QueueResult QueueResult => GetQueueResult();

		protected abstract AuditSubscriberTask GetAuditSubscriberServiceTask();

		protected string BiServer => BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);

		IEnumerable<IActualDataChangesAuditSubscriber> GetAuditSubscribers(AuditSubscriberTask auditSubscriberTask)
		{
			var loader = new SubscriberLoader();
			return loader.EnumerateSubscribersOfType(auditSubscriberTask.AssemblyName, auditSubscriberTask.SubscriberNamespace)
				.OfType<IActualDataChangesAuditSubscriber>();
		}

		QueueResult GetQueueResult()
		{
			var serviceTask = GetAuditSubscriberServiceTask();
			if (!ShouldRun(serviceTask))
			{
				return QueueResult.Zero;
			}

			var backlog = 0;
			var maxAgeInSeconds = 0;

			using (var connection = Db.NewExtraUnrestrictedWriterConnection(BiServer, Db.AuditDatabaseName))
			{
				var (queryParameters, minLsn, minLsnPeriod) = GetBacklogQueryParameters(connection, serviceTask);
				if (queryParameters.Count == 0)
				{
					return QueueResult.Zero;
				}

				var tableResult = GetTableLevelBacklogResult(connection, queryParameters.Select(p => p.TableName), minLsnPeriod, minLsn);

				foreach (var parameter in queryParameters)
				{
					var (backlogQueried, maxAgeQueried) = GetBackLog(tableResult, parameter);
					backlog += backlogQueried;
					maxAgeInSeconds = maxAgeQueried > maxAgeInSeconds ? maxAgeQueried : maxAgeInSeconds;
				}
				return new QueueResult(backlog, TimeSpan.FromSeconds(maxAgeInSeconds));
			}
		}

		/// <summary>
		/// Get the backlog for a specific table
		/// </summary>
		/// <param name="tableLevelBackLog"></param>
		/// <param name="parameter"></param>
		/// <returns>(Backlog, MaxAgeInSeconds)</returns>
		(int, int) GetBackLog(Dictionary<string, TableBacklog> tableLevelBackLog, BacklogQueryParameter parameter)
		{
			TableBacklog tableBacklog;
			if (!tableLevelBackLog.TryGetValue(parameter.TableName, out tableBacklog))
			{
				return (0, 0);
			}

			var backlog = 0;
			if (parameter.CountAll)
			{
				backlog += tableBacklog.TotalRecords;
			}
			else
			{
				if (parameter.CountInsert)
				{
					backlog += tableBacklog.TotalInsert;
				}
				if (parameter.CountUpdate)
				{
					backlog += tableBacklog.TotalUpdate;
				}
				if (parameter.CountDelete)
				{
					backlog += tableBacklog.TotalDelete;
				}
			}

			return (backlog, backlog == 0 ? 0 : tableBacklog.MaxAgeInSeconds);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
		Dictionary<string, TableBacklog> GetTableLevelBacklogResult(DbConnection connection, IEnumerable<string> tableNames, int lsnPeriod, Lsn minLsn)
		{
			var result = new Dictionary<string, TableBacklog>();

			var sqlTextGetBackLog = $@"
DECLARE @sql NVARCHAR(MAX) = N'
			SELECT
				[ChangedTableName],
				ISNULL(SUM(NumberOfRows), 0) As TotalRecords,
				ISNULL(SUM(NumberOfRowsInsert), 0) AS TotalInsert,
				ISNULL(SUM(NumberOfRowsUpdate), 0) AS TotalUpdate,
				ISNULL(SUM(NumberOfRowsDelete), 0) AS TotalDelete,
				ISNULL(MAX(DATEDIFF(second, TranEndTimeUTC, GetUtcDate())), 0)
			FROM [{BiConstants.BiAdminSchemaName}].[CdcHistorySummary] chs
			with (index(cci_biadmin_CdcHistorySummary))
			WHERE [ChangedTableName] in (' + @ChangedTableName + N')
			AND [LsnPeriod] >= CAST(' + CONVERT(NVARCHAR(4), @LsnPeriod) + ' AS SMALLINT)
			AND [Lsn] > @Lsn
			GROUP BY [ChangedTableName]'

EXEC sp_executesql @sql,
N'@ChangedTableName NVARCHAR(MAX), @LsnPeriod SMALLINT, @Lsn BINARY(10)',
@ChangedTableName, @LsnPeriod, @Lsn
";
			using (var cmd = connection.Command(sqlTextGetBackLog))
			{
				cmd.AddParameter("@ChangedTableName", System.Data.SqlDbType.NVarChar, string.Join(",", tableNames.Select(x => $"'{x}'")));
				cmd.AddParameter("@LsnPeriod", System.Data.SqlDbType.SmallInt, lsnPeriod);
				cmd.AddParameter("@Lsn", System.Data.SqlDbType.VarBinary, minLsn.Value);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var tableName = reader.GetString(0);
						var totalRecords = reader.GetInt32(1);
						var totalInsert = reader.GetInt32(2);
						var totalUpdate = reader.GetInt32(3);
						var totalDelete = reader.GetInt32(4);
						var maxAgeInSeconds = reader.GetInt32(5);
						result[tableName] = new TableBacklog
						{
							TableName = tableName,
							TotalRecords = totalRecords,
							TotalInsert = totalInsert,
							TotalUpdate = totalUpdate,
							TotalDelete = totalDelete,
							MaxAgeInSeconds = maxAgeInSeconds
						};
					}
				}
				return result;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		(List<BacklogQueryParameter>, Lsn, int) GetBacklogQueryParameters(DbConnection connection, AuditSubscriberTask auditSubscriberTask)
		{
			var result = new List<BacklogQueryParameter>();
			var minLsn = Lsn.MaxValue;
			var minLsnPeriod = 9999;
			var subscriberDictionary = GetAuditSubscribers(auditSubscriberTask)
				.Where(s => s.IsRequired() && TableHasChanges(connection, s.Code, s.Table.TableName))
				.ToDictionary(s => s.Code);

			if (subscriberDictionary.Count == 0)
			{
				return (result, minLsn, minLsnPeriod);
			}

			using (var cmd = connection.Command(GetHighWaterMarksQuery(subscriberDictionary.Keys)))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var subscriberCode = reader[0] as string;

					if (subscriberDictionary.TryGetValue(subscriberCode, out var subscriber))
					{
						var highWaterMark = new Lsn(reader[1] as byte[]);
						if (!int.TryParse(reader[2].ToString(), out var lsnPeriod) || lsnPeriod == 0)
						{
							lsnPeriod = 9999;
						}

						var parameter = new BacklogQueryParameter
						{
							Lsn = highWaterMark,
							LsnPeriod = lsnPeriod,
							SubscriberCode = subscriberCode,
						};

						if (subscriber is IBacklogCountOverridable backlogCountOverridableSubscriber)
						{
							parameter.TableName = backlogCountOverridableSubscriber.EffectiveTableName;
							parameter.CountInsert = backlogCountOverridableSubscriber.CountInsert;
							parameter.CountUpdate = backlogCountOverridableSubscriber.CountUpdate;
							parameter.CountDelete = backlogCountOverridableSubscriber.CountDelete;
						}
						else
						{
							parameter.TableName = subscriber.Table.TableName;
							parameter.CountAll = true;
						}

						result.Add(parameter);
						minLsn = minLsn > highWaterMark ? highWaterMark : minLsn;
						minLsnPeriod = minLsnPeriod > parameter.LsnPeriod ? parameter.LsnPeriod : minLsnPeriod;
					}
				}

				return (result, minLsn, minLsnPeriod);
			}
		}

		bool TableHasChanges(DbConnection auditConnection, string subscriberCode, string tableName)
		{
			var query = @"
FROM biadmin.SubscriberControl sc INNER JOIN biadmin.TableState ts
	ON ts.AetHWMHistorySummaryLsn > sc.LsnHighWaterMark
	OR sc.LsnHighWaterMark IS NULL
WHERE sc.SubscriberCode = @SubscriberCode AND ts.SourceTableName = @ChangedTableName";
			return auditConnection.Exists(query, cmd =>
			{
				cmd.AddParameter("@SubscriberCode", System.Data.SqlDbType.Char, 3, subscriberCode);
				cmd.AddParameter("@ChangedTableName", System.Data.SqlDbType.VarChar, 128, tableName);
			});
		}

		bool ShouldRun(AuditSubscriberTask auditSubscriberTask)
		{
			if (string.IsNullOrEmpty(BiServer))
			{
				return false;
			}
			if (auditSubscriberTask is ClientSpecificAuditSubscriberTask)
			{
				return string.IsNullOrEmpty(ClientSpecificAuditSubscriberTask.IsEdiClient());
			}
			return true;
		}

		string GetHighWaterMarksQuery(IEnumerable<string> subscriberCodes)
		{
			var subscriberCodesFormatted = string.Join(",", subscriberCodes.Select(x => $"'{x}'"));

			return $@"
SELECT [SubscriberCode], [LsnHighWaterMark], [PeriodHighWaterMark]
FROM [{BiConstants.BiAdminSchemaName}].[SubscriberControl] sc
WHERE [SubscriberCode] in ({subscriberCodesFormatted})";
		}

		struct BacklogQueryParameter
		{
			public string TableName { get; set; }
			public string SubscriberCode { get; set; }
			public int LsnPeriod { get; set; }
			public Lsn Lsn { get; set; }
			public bool CountAll { get; set; }
			public bool CountUpdate { get; set; }
			public bool CountDelete { get; set; }
			public bool CountInsert { get; set; }
		}

		struct TableBacklog
		{
			public string TableName { get; set; }
			public int TotalRecords { get; set; }
			public int TotalInsert { get; set; }
			public int TotalUpdate { get; set; }
			public int TotalDelete { get; set; }
#pragma warning disable CW1050 // Use System.TimeSpan Type For A Duration
			public int MaxAgeInSeconds { get; set; }
#pragma warning restore CW1050 // Use System.TimeSpan Type For A Duration
		}
	}
}
