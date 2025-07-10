using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public abstract class BONotification : NotificationSubscriberNotification
	{
		protected BONotification(NotificationSubscriberType type, string message)
			: base(type, message)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static void AddErrorsFromBusinessObjectValidationIncludingChildren(INotifications notifications, BusinessObject bO)
		{
			foreach (INotification error in bO.NotificationsIncludingChildren.GetErrors())
			{
				string errorWithoutPrefix = error.Message;
				if (error.Message.StartsWith("Error - "))
				{
					errorWithoutPrefix = error.Message.Substring(8);
				}
				notifications.Notify(new BOErrorNotification(errorWithoutPrefix));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static void AddWarningsFromBusinessObjectValidationIncludingChildren(INotifications notifications, BusinessObject bO)
		{
			foreach (INotification warning in bO.NotificationsIncludingChildren.GetWarnings())
			{
				string warningWithoutPrefix = warning.Message;
				if (warning.Message.StartsWith("Warning - "))
				{
					warningWithoutPrefix = warning.Message.Substring(10);
				}
				notifications.Notify(new BOWarningNotification(warningWithoutPrefix));
			}
		}
	}
}
