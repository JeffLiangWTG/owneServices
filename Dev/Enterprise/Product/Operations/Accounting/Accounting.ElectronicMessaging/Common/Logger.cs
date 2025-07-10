using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class Logger : INotifications
	{
		public Logger()
		{ }

		readonly List<string> notifications = new List<string>();

		void INotifications.Add(INotification notification)
		{
			notifications.Add(notification.Message);
			HasErrors = true;
		}

		public override string ToString()
		{
			return string.Join("\r\n", notifications.ToArray());
		}

		public bool HasErrors { get; private set; }
	}
}
