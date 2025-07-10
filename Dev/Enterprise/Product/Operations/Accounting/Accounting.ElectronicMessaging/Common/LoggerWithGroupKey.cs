using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Newtonsoft.Json;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class LoggerWithGroupKey : INotifications
	{
		public LoggerWithGroupKey()
		{
			Notifications = new Dictionary<string, List<string>>();
		}

		public void Add(INotification notification)
		{
			var groupKey = notification is INotificationWithGroupKey notificationWithGroupKey
				? notificationWithGroupKey.GroupKey
				: string.Empty;

			if (!Notifications.ContainsKey(groupKey))
			{
				Notifications[groupKey] = new List<string>();
			}
			Notifications[groupKey].Add(notification.Message);
		}

		public override string ToString()
		{
			return Notifications.Any()
				? JsonConvert.SerializeObject(Notifications)
				: string.Empty;
		}

		Dictionary<string, List<string>> Notifications { get; }

		public static bool TryDeserializeObject(string value, out IDictionary<string, string[]> messagesWithKey)
		{
			try
			{
				messagesWithKey = string.IsNullOrEmpty(value)
					? new Dictionary<string, string[]>()
					: JsonConvert.DeserializeObject<Dictionary<string, string[]>>(value);
			}
			catch
			{
				messagesWithKey = null;
			}

			return messagesWithKey != null;
		}
	}
}
