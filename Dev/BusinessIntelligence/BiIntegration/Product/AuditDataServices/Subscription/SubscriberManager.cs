namespace Enterprise.AuditDataServices.Subscription
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using CargoWise.Bi.Common;
	using CargoWise.Common;
	using CargoWise.Data;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.Integration;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;

	public class SubscriberManager
	{
		public SubscriberManager(ILogger serviceLogger, string serviceTaskCode)
		{
			this.serviceLogger = serviceLogger;
			this.auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
			this.serviceTaskCode = serviceTaskCode;
			(subscriberLsnHighWaterMarkCache, subscriberCommandIdHighWaterMarkCache) = GetSubscriberControlCache();
			tableStateLsnHighWaterMarkCache = GetTableStateLsnHighWaterMarkCache();
		}

		readonly protected ILogger serviceLogger;
		readonly protected Dictionary<string, int> subscriberCommandIdHighWaterMarkCache;
		readonly protected Dictionary<string, Lsn> subscriberLsnHighWaterMarkCache;
		readonly protected Dictionary<string, Lsn> tableStateLsnHighWaterMarkCache;
		readonly string auditServer;
		readonly string serviceTaskCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "It's an audit query. WI: SQL CPU Usage alert for Audit - BF4193A9DEFEBB3E06218F636349829E - WI00740754")]
		Dictionary<string, Lsn> GetTableStateLsnHighWaterMarkCache()
		{
			var result = new Dictionary<string, Lsn>();

			var sql = $@"
				SELECT
					SourceSchemaName, SourceTableName, AetHWMHistorySummaryLsn
				FROM
					[{BiConstants.BiAdminSchemaName}].[TableState]
			";

			using (var auditConnection = Db.NewExtraUnrestrictedWriterConnection(auditServer, Db.AuditDatabaseName))
			using (var cmd = auditConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var fullTableName = $"[{(string)reader["SourceSchemaName"]}].[{(string)reader["SourceTableName"]}]";
					var lsn = reader["AetHWMHistorySummaryLsn"] == DBNull.Value ? new Lsn((byte[])null) : new Lsn((byte[])reader["AetHWMHistorySummaryLsn"]);
					result.Add(fullTableName, lsn);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "It's an audit query. WI: SQL CPU Usage alert for Audit - BF4193A9DEFEBB3E06218F636349829E - WI00740754")]
		(Dictionary<string, Lsn>, Dictionary<string, int>) GetSubscriberControlCache()
		{
			var lsnHighWaterMarkCache = new Dictionary<string, Lsn>();
			var commandIdHighWaterMarkCache = new Dictionary<string, int>();

			var sql = $@"
				SELECT
					SubscriberCode, LsnHighWaterMark, CommandIdHighWaterMark
				FROM
					[{BiConstants.BiAdminSchemaName}].[SubscriberControl]
			";

			using (var auditConnection = Db.NewExtraUnrestrictedWriterConnection(auditServer, Db.AuditDatabaseName))
			using (var cmd = auditConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var subscriberCode = (string)reader["SubscriberCode"];
					var lsn = reader["LsnHighWaterMark"] == DBNull.Value ? new Lsn((byte[])null) : new Lsn((byte[])reader["LsnHighWaterMark"]);
					lsnHighWaterMarkCache.Add(subscriberCode, lsn);
					var commandId = reader["CommandIdHighWaterMark"] == DBNull.Value ? 0 : (int)reader["CommandIdHighWaterMark"];
					commandIdHighWaterMarkCache.Add(subscriberCode, commandId);
				}
			}

			return (lsnHighWaterMarkCache, commandIdHighWaterMarkCache);
		}

		internal void Run(CancellationToken token, IEnumerable<IAuditSubscriber> subscribers)
		{
			try
			{
				RunSafe(token, subscribers);
			}
			catch (SqlException ex)
			{
				var type = new DbErrorMatch(ex).ExceptionType;
				switch (type)
				{
					case DbErrorType.LoginDisabled:
					case DbErrorType.PermissionDeniedOnObject:
						var dbUserManager = new DbUserManager();
						dbUserManager.EnsureApplicationLoginForBiDatabase(auditServer, Db.AuditDatabaseName);
						RunSafe(token, subscribers);
						break;
					default:
						throw;
				}
			}
			catch (DatabaseUpgradeInProgressException ex)
			{
				serviceLogger.Log(LogType.Debug, ex.Message);
			}
		}

		void RunSafe(CancellationToken token, IEnumerable<IAuditSubscriber> rawSubscribers)
		{
			using (var auditConnection = Db.NewExtraUnrestrictedWriterConnection(auditServer, Db.AuditDatabaseName))
			{
				var subscribersToRun = GetSubscribersToRun(rawSubscribers, auditConnection);
				if (subscribersToRun.Any())
				{
					SendAuditChangeNotifications(subscribersToRun, token);
				}
			}
		}

		public static void RemoveObsoleteSubscribers(DbConnection auditConnection)
		{
			var existingSubscriberFilter = string.Join("', '", new SubscriberLoader().EnumerateAllSubscriberTypes().Select(s => s.Code));

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS (SELECT NULL FROM [{0}].SubscriberControl WITH (NOLOCK) WHERE SubscriberCode NOT IN ('{1}'))
					DELETE FROM [{0}].SubscriberControl WHERE SubscriberCode NOT IN ('{1}')",
				BiConstants.BiAdminSchemaName,
				existingSubscriberFilter);
			auditConnection.ExecuteNonQuery(sqlText);
		}

		protected IEnumerable<IAuditSubscriberWrapper> GetSubscribersToRun(IEnumerable<IAuditSubscriber> allSubscribers, DbConnection auditConnection)
		{
			var validSubscribers = new List<IAuditSubscriberWrapper>();

			foreach (var rawSubscriber in allSubscribers)
			{
				var subscriber = rawSubscriber.GetWrapper(auditConnection, new SubscriberLogger(serviceLogger, rawSubscriber.Code));

				if (!subscriber.ExistsInSubscriberControlTable)
				{
					subscriber.AddSubscriberToSubscriberControlTable();
				}

				if (HasChangesToProcess(subscriber))
				{
					if (subscriber.ShouldRunSubscriber())
					{
						serviceLogger.Log(LogType.Debug, "Running " + subscriber.Code + " Subscriber");
						validSubscribers.Add(subscriber);
					}
				}
			}

			return validSubscribers;
		}

		internal bool HasChangesToProcess(IAuditSubscriberWrapper subscriber)
		{
			var result = false;
			if (subscriber.IsValid)
			{
				result = subscriber.HasChangesToProcessFromCache(tableStateLsnHighWaterMarkCache, subscriberLsnHighWaterMarkCache, subscriberCommandIdHighWaterMarkCache);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System Notification")]
		protected virtual void SendAuditChangeNotifications(IEnumerable<IAuditSubscriberWrapper> validSubscribers, CancellationToken token)
		{
			IEnumerable<string> subscribersWithChanges;
			serviceLogger.Debug($"Required subscribers: {string.Join(", ", validSubscribers.Select(s => s.Code))}");

			do
			{
				subscribersWithChanges = RunSubscribers(validSubscribers, token);
				validSubscribers = validSubscribers.Where(sub => subscribersWithChanges.Contains(sub.Code));

				serviceLogger.Log(
					LogType.Debug,
					"> Completed processing audit changes" + (
						subscribersWithChanges.Any()
							? " for the following subscriber(s): " + string.Join(", ", subscribersWithChanges) + "."
							: "."
						)
					);
			} while (subscribersWithChanges.Any());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message")]
		public IEnumerable<string> RunSubscribers(IEnumerable<IAuditSubscriberWrapper> subscribers, CancellationToken token)
		{
			var processedSubscribers = new List<string>();
			bool isLockAcquired;
			using (new BiServiceTaskLockHandler().TryGetLock(TimeSpan.FromSeconds(5), BiConstants.AspMaintainanceLockKey + serviceTaskCode, out isLockAcquired))
			{
				if (!isLockAcquired)
				{
					serviceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Subscriber task {0} run skipped due to lock", serviceTaskCode));
					return processedSubscribers;
				}

				foreach (var subscriber in subscribers)
				{
					if (!token.IsCancellationRequested)
					{
						try
						{
							if (subscriber.FetchDataAndProcessChanges())
							{
								processedSubscribers.Add(subscriber.Code);
							}
						}
						catch (SqlException sqlEx) when (new DbErrorMatch(sqlEx).IsInfrastructureDbError)
						{
							serviceLogger.Log(LogType.Warning, $"[{subscriber.Code}] An infrastructure Db error occurred while processing subscriber changes.{System.Environment.NewLine}{sqlEx.Message}");
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
#if DEBUG
							serviceLogger.Log(LogType.Debug, $"[{subscriber.Code}] Exception thrown while processing subscriber changes.");
#else
							serviceLogger.ErrorAndReportException($"[{subscriber.Code}] Exception thrown while processing subscriber changes.", e); // SuppressCodeSmell Reason = Log Message
#endif
						}
					}
					else
					{
						serviceLogger.Debug("Stopping subscriber processing due to token cancellation request.");
						break;
					}
				}

				return processedSubscribers;
			}
		}

		protected virtual IEnumerable<IAuditSubscriber> LoadAllSubscribers()
		{
			return new SubscriberLoader().EnumerateAllSubscriberTypes();
		}
	}
}
