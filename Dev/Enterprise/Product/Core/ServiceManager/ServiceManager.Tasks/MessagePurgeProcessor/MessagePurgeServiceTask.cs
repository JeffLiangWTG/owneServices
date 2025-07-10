using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Tasks.MessagePurgeProcessor;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	MessagePurgeServiceTask.Code,
	MessagePurgeServiceTask.ServiceTaskDescription,
	"ESV",
	typeof(MessagePurgeServiceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true)]

//There is no need to add HostedServiceBusinessObjectBinding to nudge this service task. This service task runs on a daily basis. We don't want it to run too often.
namespace Enterprise.ServiceManager.Tasks.MessagePurgeProcessor
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
	public class MessagePurgeServiceTask : ServiceProviderImpl
	{
		public const string Code = "EPG";
		public const string ServiceTaskDescription = "Message Purge Service Task";

		public MessagePurgeServiceTask() : this(recordsToPurge: eHubMessagingRegistry.Instance.PurgeSettingsItem.Value.BatchSize)
		{
		}

		internal MessagePurgeServiceTask(int recordsToPurge) : this(recordsToPurge, pauseWaitBackoff: TimeSpan.FromMinutes(1), pauseWaitRetries: 10, pauserFactory: new LowPriorityProcessPauserFactory())
		{
		}

		internal MessagePurgeServiceTask(int recordsToPurge, TimeSpan pauseWaitBackoff, int pauseWaitRetries, ILowPriorityProcessPauserFactory pauserFactory)
		{
			this.recordsToPurge = recordsToPurge;
			this.pauseWaitBackoff = pauseWaitBackoff;
			this.pauseWaitRetries = pauseWaitRetries;
			this.pauserFactory = pauserFactory;
		}

		public override void RunTask(CancellationToken token)
		{
			var waiter = pauserFactory.Create();
			var settings = GetSettings();
			var messageFilters = GetMessageFilters(settings).ToList();

			Log("Starting purge.");
			foreach (var messageFilter in messageFilters)
			{
				MessagePurgeResult messagePurgeResult;
				do
				{
					token.ThrowIfCancellationRequested();
					WaitForBacklogs(waiter, token);
					messagePurgeResult = PurgeMessages(messageFilter);
					UpdateFilterAndSettings(messageFilter, messagePurgeResult);
					Log(string.Format(CultureInfo.InvariantCulture, "{0} EDI Messages and their associated entities have been purged.", messagePurgeResult.MessagesDeleted));
				} while (messagePurgeResult.MessagesDeleted >= recordsToPurge);
			}

			var interchangeFilters = GetInterchangeFilters(settings).ToList();
			foreach (var interchangeFilter in interchangeFilters)
			{
				MessagePurgeResult messagePurgeResult;
				do
				{
					token.ThrowIfCancellationRequested();
					WaitForBacklogs(waiter, token);
					messagePurgeResult = PurgeInterchanges(interchangeFilter);
					UpdateFilterAndSettings(interchangeFilter, messagePurgeResult);
					Log(string.Format(CultureInfo.InvariantCulture, "{0} EDI Interchanges with their associated entities have been purged.", messagePurgeResult.MessagesDeleted));
				} while (messagePurgeResult.MessagesDeleted >= recordsToPurge);
			}

			var messagesDeleted = 0;
			do
			{
				token.ThrowIfCancellationRequested();
				WaitForBacklogs(waiter, token);
				messagesDeleted = PurgeManager.PurgeMessagesMarkedAsDiscarded(recordsToPurge);
				if (messagesDeleted > 0)
				{
					Log(string.Format(CultureInfo.InvariantCulture,
						@$"{messagesDeleted} EDI Messages marked as discarded and their associated entities have been purged based on application codes.
[Application Codes: {string.Join(",", PurgeManager.ApplicationCodes)}]"));
				}
			} while (messagesDeleted >= recordsToPurge);

			do
			{
				token.ThrowIfCancellationRequested();
				WaitForBacklogs(waiter, token);
				messagesDeleted = PurgeManager.PurgeInterchangesMarkedAsDiscarded(recordsToPurge);
				if (messagesDeleted > 0)
				{
					Log(string.Format(CultureInfo.InvariantCulture,
						@$"{messagesDeleted} EDI Interchanges marked as discarded and their associated entities have been purged based on application codes.
[Application Codes: {string.Join(",", PurgeManager.ApplicationCodes)}]"));
				}
			} while (messagesDeleted >= recordsToPurge);

			Log("Finished purge.");
		}

		void WaitForBacklogs(ILowPriorityProcessPauser waiter, CancellationToken token)
		{
			for (int i = 0; i < pauseWaitRetries; ++i)
			{
				try
				{
					waiter.Wait(ServiceLogger);
					waiter.Wait();
					return;
				}
				catch (Exception ex) when (!ex.IsCriticalException() && i < pauseWaitRetries - 1)
				{
					Buffer.Notify(new WarningNotification(ex.ToString()));
					Log(string.Format(CultureInfo.InvariantCulture, "Backing off for {0}.", pauseWaitBackoff));
					token.WaitHandle.WaitOne(pauseWaitBackoff);
					token.ThrowIfCancellationRequested();
				}
			}
		}

		#region Buffer

		void Log(string message)
		{
			Buffer.Notify(new InfoNotification(message));
		}

		internal NotificationBuffer Buffer
		{
			get { return buffer ?? (buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber())); }
		}

		NotificationBuffer buffer;

		#endregion

		#region Implementation

		class MessagePurgeResult
		{
			public int MessagesDeleted { get; set; }
			public DateTime MaxDeletedMessageCreateTimeUtc { get; set; }
		}

		const int CommandTimeout = 1000;

		readonly int recordsToPurge;
		readonly int pauseWaitRetries;
		readonly TimeSpan pauseWaitBackoff;
		readonly ILowPriorityProcessPauserFactory pauserFactory;

		MessagePurgeResult PurgeInterchanges(InterchangeFilter interchangeFilter)
		{
			var query = string.Format(CultureInfo.InvariantCulture, PurgeManager.PurgeInterchangeQuery, recordsToPurge, interchangeFilter.Condition);
			using (var cmd = Db.Connection.Command(query, CommandTimeout))
			{
				cmd.AddParameter(InterchangeFilter.MinMessageCreateTimeParamName, SqlDbType.DateTime, interchangeFilter.MinMessageCreateTimeUtc.ToDateTime());
				cmd.AddParameter(InterchangeFilter.MaxMessageCreateTimeParamName, SqlDbType.DateTime, interchangeFilter.MaxMessageCreateTimeUtc.ToDateTime());
				using (var reader = cmd.ExecuteReader())
				{
					var result = new MessagePurgeResult();
					if (reader.Read())
					{
						result.MessagesDeleted = reader.GetInt32(0);
					}
					if (reader.NextResult() && reader.Read())
					{
						result.MaxDeletedMessageCreateTimeUtc = reader.GetDateTime(0);
					}
					return result;
				}
			}
		}

		MessagePurgeResult PurgeMessages(BaseFilterCreator messageFilter)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, PurgeManager.PurgeMessageQuery, messageFilter.Condition);

			using (var cmd = Db.Connection.Command(sqlText, CommandTimeout))
			{
				cmd.AddParameter(BaseFilterCreator.MinMessageCreateTimeParamName, SqlDbType.DateTime, messageFilter.MinMessageCreateTimeUtc.ToDateTime());
				cmd.AddParameter(BaseFilterCreator.MaxMessageCreateTimeParamName, SqlDbType.DateTime, messageFilter.MaxMessageCreateTimeUtc.ToDateTime());
				cmd.AddParameter("@RecordsToPurge", SqlDbType.Int, recordsToPurge);

				using (var reader = cmd.ExecuteReader())
				{
					var result = new MessagePurgeResult();

					if (reader.Read())
					{
						result.MessagesDeleted = reader.GetInt32(0);
					}

					if (reader.NextResult() && reader.Read())
					{
						result.MaxDeletedMessageCreateTimeUtc = reader.GetDateTime(0);
					}

					return result;
				}
			}
		}

		static IEnumerable<ApplicationCodeObj> GetAllApplicationCodes(PurgeSettings settings)
		{
			return settings.ApplicationCodes.Cast<ApplicationCodeObj>().Concat(settings.HiddenApplicationCodes.Cast<ApplicationCodeObj>());
		}

		void UpdateFilterAndSettings(InterchangeFilter interchangeFilter, MessagePurgeResult messagePurgeResult)
		{
			var latestPurgedMessageTimeUtc = messagePurgeResult.MessagesDeleted == recordsToPurge
				? messagePurgeResult.MaxDeletedMessageCreateTimeUtc
				: interchangeFilter.MaxMessageCreateTimeUtc;
			interchangeFilter.MinMessageCreateTimeUtc = latestPurgedMessageTimeUtc;
			var settings = GetSettings();
			foreach (var applicationCode in interchangeFilter.ApplicationCodes)
			{
				var applicationCodeObj = GetAllApplicationCodes(settings).Single(ac => ac.ApplicationCode == applicationCode);
				var interchangeObjs = applicationCodeObj.Interchanges.Cast<InterchangeObj>();
				foreach (var interchangeObj in interchangeObjs)
				{
					interchangeObj.LatestPurgedMessageTimeUtc = latestPurgedMessageTimeUtc;
				}
			}
			SaveSettings(settings);
		}

		void UpdateFilterAndSettings(BaseFilterCreator filter, MessagePurgeResult messagePurgeResult)
		{
			var latestPurgedMessageTimeUtc = messagePurgeResult.MessagesDeleted == recordsToPurge
				? messagePurgeResult.MaxDeletedMessageCreateTimeUtc
				: filter.MaxMessageCreateTimeUtc;
			filter.MinMessageCreateTimeUtc = latestPurgedMessageTimeUtc;
			var settings = GetSettings();

			if (filter is ApplicationCodeFilterCreator applicationCodeFilter)
			{
				foreach (var applicationCode in applicationCodeFilter.ApplicationCodes)
				{
					var applicationCodeObjs = GetAllApplicationCodes(settings).Where(ac => ac.ApplicationCode == applicationCode).ToList();
					foreach (var applicationCodeObj in applicationCodeObjs)
					{
						applicationCodeObj.LatestPurgedMessageTimeUtc = latestPurgedMessageTimeUtc;
					}
				}
			}
			else if (filter is MessageFilterCreator messageFilterCreator)
			{
				foreach (var kv in messageFilterCreator.MessageTypes)
				{
					var applicationCode = kv.Key;
					var applicationCodeObjs = GetAllApplicationCodes(settings).Where(ac => ac.ApplicationCode == applicationCode).ToList();
					var messageTypes = kv.Value;
					foreach (var typeParam in messageTypes)
					{
						var messageTypeObjs = applicationCodeObjs.SelectMany(ac => ac.MessageTypes.Cast<MessageTypeObj>());
						if (typeParam.PurgeType.Equals(PurgeTypeList.MessageType))
						{
							messageTypeObjs = messageTypeObjs.Where(mt => mt.MessageType == typeParam.MessageType && mt.MessageSubType == "");
						}
						else if (typeParam.PurgeType.Equals(PurgeTypeList.MessageSubType))
						{
							messageTypeObjs = messageTypeObjs.Where(mt => mt.MessageType == "" && mt.MessageSubType == typeParam.MessageSubType);
						}
						else if (typeParam.PurgeType.Equals(PurgeTypeList.MessageTypeAndSubType))
						{
							messageTypeObjs = messageTypeObjs.Where(mt => mt.MessageType == typeParam.MessageType && mt.MessageSubType == typeParam.MessageSubType);
						}

						var messageTypeObj = messageTypeObjs.Single();
						messageTypeObj.LatestPurgedMessageTimeUtc = latestPurgedMessageTimeUtc;
					}
				}
			}

			SaveSettings(settings);
		}

		static PurgeSettings GetSettings()
		{
			return eHubMessagingRegistry.Instance.PurgeSettingsItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		static void SaveSettings(PurgeSettings settings)
		{
			eHubMessagingRegistry.Instance.PurgeSettingsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		}

		static IEnumerable<InterchangeFilter> GetInterchangeFilters(PurgeSettings settings)
		{
			var now = ZDateTime.UtcNow;
			return GetAllApplicationCodes(settings)
				.SelectMany(a => a.Interchanges.OfType<InterchangeObj>().Where(m => m.Selected).Select(m => new
				{
					a.ApplicationCode,
					MinTime = m.LatestPurgedMessageTimeUtc.IsValid ? m.LatestPurgedMessageTimeUtc : now.AddYears(-100),
					MaxTime = now.AddDays(-PurgeIntervalDays(m.PurgeTime, m.PurgeTimeUnit)),
				}))
				.GroupBy(mt => new { mt.MinTime, mt.MaxTime })
				.Select(purgeIntervalGroup => new InterchangeFilter(
					purgeIntervalGroup.Select(type => type.ApplicationCode),
					purgeIntervalGroup.Key.MinTime,
					purgeIntervalGroup.Key.MaxTime));
		}

		static IEnumerable<BaseFilterCreator> GetMessageFilters(PurgeSettings settings)
		{
			var now = ZDateTime.UtcNow;
			var appCodeObjs = GetAllApplicationCodes(settings);
			var messageFilters = appCodeObjs.Where(a => a.Selected && !a.PurgeType.Equals(PurgeTypeList.ApplicationCode))
				.SelectMany(a =>
				{
					return a.MessageTypes.OfType<MessageTypeObj>().Where(m => m.Selected).ToList().Select(m => new
					{
						a.ApplicationCode,
						a.PurgeType,
						m.MessageType,
						m.MessageSubType,
						MinTime = m.LatestPurgedMessageTimeUtc.IsValid ? m.LatestPurgedMessageTimeUtc : now.AddYears(-100),
						MaxTime = now.AddDays(-PurgeIntervalDays(m.PurgeTime, m.PurgeTimeUnit)),
					});
				})
				.GroupBy(mt => new { mt.MinTime, mt.MaxTime })
				.Select(purgeIntervalGroup => new MessageFilterCreator(purgeIntervalGroup.GroupBy(type => type.ApplicationCode).ToDictionary(g => g.Key, g => g.Select(t => new MessageTypeParams(t.PurgeType, t.MessageType, t.MessageSubType)).ToList()),
					purgeIntervalGroup.Key.MinTime,
					purgeIntervalGroup.Key.MaxTime));

			var appCodePurgeTypeFilters = appCodeObjs.Where(a => a.Selected && a.PurgeType.Equals(PurgeTypeList.ApplicationCode))
				.Select(a =>
				{
					return new
					{
						a.ApplicationCode,
						MinTime = a.LatestPurgedMessageTimeUtc.IsValid ? a.LatestPurgedMessageTimeUtc : now.AddYears(-100),
						MaxTime = now.AddDays(-PurgeIntervalDays(a.PurgeTime, a.PurgeTimeUnit))
					};
				})
				.GroupBy(mt => new { mt.MinTime, mt.MaxTime })
				.Select(purgeIntervalGroup => new ApplicationCodeFilterCreator(
					purgeIntervalGroup.Select(g => g.ApplicationCode).ToList(),
					purgeIntervalGroup.Key.MinTime,
					purgeIntervalGroup.Key.MaxTime));

			return messageFilters.Cast<BaseFilterCreator>().Concat(appCodePurgeTypeFilters.Cast<BaseFilterCreator>());
		}

		static int PurgeIntervalDays(ZShort purgeTime, ZGuid purgeTimeUnit)
		{
			if (purgeTimeUnit == TimeUnit.Week)
			{
				return purgeTime * 7;
			}
			if (purgeTimeUnit == TimeUnit.Month)
			{
				return (ZDateTime.UtcNow - ZDateTime.UtcNow.AddMonths(-1 * purgeTime)).Days;
			}
			if (purgeTimeUnit == TimeUnit.Year)
			{
				return (ZDateTime.UtcNow - ZDateTime.UtcNow.AddYears(-1 * purgeTime)).Days;
			}

			return 7;
		}

		#endregion // Implementation
	}
}
