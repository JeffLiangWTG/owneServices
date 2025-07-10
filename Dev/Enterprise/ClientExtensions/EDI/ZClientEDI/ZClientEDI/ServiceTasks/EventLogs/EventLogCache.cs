using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Client.EDI.ServiceTasks.EventLogs
{
	class EventLogCache
	{
		public EventLogCache()
		{
			eventLogCachesDictionary = new Dictionary<string, string>();
			var list = LoadListEventLogCache();
			foreach (var eventLogData in list)
			{
				eventLogCachesDictionary[eventLogData.Machine] = eventLogData.EventRecordID;
			}
		}

		/// <summary>
		/// Return HighWaterMark if found
		/// Return null if could not found
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public string FindValue(string key)
		{
			Argument.NotNullOrEmpty(key, "key");
			string value;
			eventLogCachesDictionary.TryGetValue(key, out value);
			return value;
		}

		public void UpdateEventLogCache(string eventRecordID, string hostname)
		{
			Argument.NotNullOrEmpty(eventRecordID, "EventRecordID");
			Argument.NotNullOrEmpty(hostname, "hostname");
			eventLogCachesDictionary[hostname] = eventRecordID;
		}

		public void Sync()
		{
			UpdateCacheDictionaryWithTheLastestInDatabase();
			var result = string.Join(",", eventLogCachesDictionary.Select(m => m.Key + "+" + m.Value).ToArray());
			RegistryMachinesHandler.Sync(result);
		}

		void UpdateCacheDictionaryWithTheLastestInDatabase()
		{
			var list = LoadListEventLogCache();
			foreach (var eventLogData in list)
			{
				if (eventLogCachesDictionary.ContainsKey(eventLogData.Machine))
				{
					try
					{
						if (Int64.Parse(eventLogCachesDictionary[eventLogData.Machine]) < Int64.Parse(eventLogData.EventRecordID))
						{
							eventLogCachesDictionary[eventLogData.Machine] = eventLogData.EventRecordID;
						}
					}
					catch (FormatException ex)
					{
						throw new InvalidOperationException(string.Format("'{0}' and '{1}' should be format-able into number.", eventLogData.Machine, eventLogData.EventRecordID, ex));
					}
				}
				else
				{
					eventLogCachesDictionary[eventLogData.Machine] = eventLogData.EventRecordID;
				}
			}
		}

		readonly Dictionary<string, string> eventLogCachesDictionary;

		IEnumerable<EventLogCacheData> LoadListEventLogCache()
		{
			return RegistryMachinesHandler.GetLatestListEventLogCache();
		}

		internal Dictionary<string, string> EventLogCachesDictionary { get { return eventLogCachesDictionary; } }
	}
}
