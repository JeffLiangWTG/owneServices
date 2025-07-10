using System;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	public class NotificationLoggerWomboComboForTest : ILogger, INotifications
	{
		readonly StringBuilder builder = new StringBuilder();
		INotifications notifications;

		public void Add(INotification notification)
		{
			notifications?.Add(notification);
			builder.AppendLine(notification.Message);
		}

		public void Log(LogType type, string message) => this.AddInfo(message);
		public void Log(LogType type, string message, Exception ex) => Log(type, string.Join(System.Environment.NewLine, new[] { message, ex?.Message ?? "", ex?.StackTrace ?? "" }));
		public string GetAllLogs() => builder.ToString();
		public void Clear() => builder.Clear();

		internal IDisposable WithLogger(INotifications notifications)
		{
			this.notifications = notifications;
			return new DisposableAction(() => this.notifications = null);
		}
	}
}
