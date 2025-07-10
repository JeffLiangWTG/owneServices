using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.ZArchitecture.GUI
{
	public class GlowLog : INotifications
	{
		readonly HashSet<KeyValuePair<string, string>> log = new HashSet<KeyValuePair<string, string>>();
		public bool HasErrors => log.Any(l => l.Key == LogType.Error);
		public bool HasVerboseErrors => log.Any(l => l.Key == LogType.Verbose);

		[SuppressMessage("Microsoft.Usage", "CA1801", Justification = "To be implemented: Use progress in appended logs")]
		public void AppendLog(string type, string message, decimal progress)
		{
			log.Add(new KeyValuePair<string, string>(type, message));
		}

		public string GetLogType(int index)
		{
			return log.ElementAt(index).Key;
		}

		public string GetLogMessage(int index)
		{
			return log.ElementAt(index).Value;
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "There is no benefit to creating a new type for this.")]
		public IEnumerable<KeyValuePair<string, string>> AsEnumerableWithoutVerbose()
		{
			return log.Where(x => x.Key != LogType.Verbose);
		}

		public string GetLogs()
		{
			return string.Join(System.Environment.NewLine, log.Select(x => x.Value));
		}

		public int CountWithoutVerbose => log.Count(x => x.Key != LogType.Verbose);

		public void Add(INotification notification)
		{
			if (notification.Type == NotificationType.Error || notification is ErrorNotification)
			{
				AppendLog(LogType.Error, notification.Message, 0);
			}
			else if (notification.Type == NotificationType.Warning || notification is WarningNotification)
			{
				AppendLog(LogType.Warning, notification.Message, 0);
			}
			else if (notification.Type == NotificationType.Information || notification is InfoNotification)
			{
				AppendLog(LogType.Info, notification.Message, 0);
			}
			else if (notification is NewlineNotification)
			{
				AppendLog(System.Guid.NewGuid().ToString(), notification.Message, 0);
			}
		}
	}
}
