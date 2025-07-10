using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Enterprise.Client.EDI.ServiceTasks.EventLogs
{
	public static class RegistryMachinesHandler
	{
		public static IEnumerable<string> GetLatestAccessibleHostNameList()
		{
			return GetLatestListFromRegistry().AsParallel().Where(x =>
				{
					try
					{
						using (Ping pinger = new Ping())
						{
							PingReply reply = pinger.Send(x);
							return reply.Status == IPStatus.Success;
						}
					}
					catch (PingException)
					{
						return false;
					}
				});
		}

		public static IEnumerable<EventLogCacheData> GetLatestListEventLogCache()
		{
			var line = EDIDataRegistry.Instance.EventLogHighWaterMarkList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return string.IsNullOrEmpty(line) ? Enumerable.Empty<EventLogCacheData>() : line.Split(new char[] { ',' }).Select(p =>
			{
				var element = p.Split('+');
				return new EventLogCacheData(element);
			});
		}

		public static void Sync(string result)
		{
			EDIDataRegistry.Instance.EventLogHighWaterMarkList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, result);
		}

		static IEnumerable<string> GetLatestListFromRegistry()
		{
			var line = EDIDataRegistry.Instance.MachineHostNameList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());
		}
	}

	public struct EventLogCacheData
	{
		readonly string machine;
		readonly string eventRecordID;
		public EventLogCacheData(string[] data)
		{
			machine = data[0];
			eventRecordID = data[1];
		}
		public string Machine { get { return machine; } }
		public string EventRecordID { get { return eventRecordID; } }
	}
}
