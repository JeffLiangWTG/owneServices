using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Enterprise.RemotePrinting.Engine
{
	public class SendNotificationEmailHelper
	{
		SendNotificationEmailHelper()
		{
		}

		public static SendNotificationEmailHelper Instance => instance ?? (instance = new SendNotificationEmailHelper());

		[SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Static fields in this class do not need to be thread-static")]
		static SendNotificationEmailHelper instance;

		[SuppressMessage("CargoWiseOne", "CW1061:Do not use System.DateTime.UtcNow Rule", Justification = "Z types are not accessible in this assembly")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static void DailySendNotificationEmailOnce(string subject, string body, string key = null)
		{
			if (string.IsNullOrEmpty(subject))
			{
				Instance.ReportError?.Invoke(Instance, new ReportErrorEventArgs("Empty subject for Send Notification Email", $"Key: {key}\r\nEmail Body: {body}", null));
				return;
			}

			key = key ?? subject;
			var now = DateTime.UtcNow;
			var lastSentTimeUtc = Instance.GetLastSentTimeUtc(key);
			if ((now - lastSentTimeUtc).TotalDays >= 1.0)
			{
				Instance.LastSendNotificationTimes[key] = now;
				Instance.SendNotificationEmail?.Invoke(Instance, new SendNotificationEmailEventArgs(subject, body));
				Instance.CleanOldSendNotificationTimes(now);
			}
		}

		DateTime GetLastSentTimeUtc(string key)
		{
			if (LastSendNotificationTimes.TryGetValue(key, out var lastSentTimeUtc))
			{
				return lastSentTimeUtc;
			}

			return DateTime.MinValue;
		}

		[SuppressMessage("Concurrency", "CW1024:Bad Concurrent Collection Access", Justification = "It uses snapshot array and safe TryRemove and TryAdd")]
		void CleanOldSendNotificationTimes(DateTime now)
		{
			lock (cleanOldSendNotificationTimesLock)
			{
				var oldDate = now.AddDays(-1);
				var items = LastSendNotificationTimes.ToArray().Where(item => item.Value <= oldDate).Select(item => item.Key).ToArray();
				foreach(var key in items)
				{
					if (LastSendNotificationTimes.TryRemove(key, out var currentDate) && currentDate > oldDate)
					{
						LastSendNotificationTimes.TryAdd(key, currentDate);
					}
				}
			}
		}
		readonly object cleanOldSendNotificationTimesLock = new object();

		public event EventHandler<SendNotificationEmailEventArgs> SendNotificationEmail;
		public event EventHandler<ReportErrorEventArgs> ReportError;

		ConcurrentDictionary<string, DateTime> lastSendNotificationTimes;
		ConcurrentDictionary<string, DateTime> LastSendNotificationTimes => lastSendNotificationTimes ?? (lastSendNotificationTimes = new ConcurrentDictionary<string, DateTime>());

#if DEBUG
		public void SetLastSendNotificationTimeForTest(string subject, DateTime lastedSendNotificationTime)
		{
			LastSendNotificationTimes[subject] = lastedSendNotificationTime;
		}

		public ConcurrentDictionary<string, DateTime> LastSendNotificationTimes_ExposedForTest => LastSendNotificationTimes;
#endif
	}
}
