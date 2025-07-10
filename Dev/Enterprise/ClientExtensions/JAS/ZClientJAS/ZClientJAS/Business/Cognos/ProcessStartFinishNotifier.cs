using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class ProcessStartFinishNotifier : IDisposable
	{
		public ProcessStartFinishNotifier(ICognosNotificationSubscriber notifications, string processDescription)
			: this(notifications, processDescription, false, false)
		{
		}

		public ProcessStartFinishNotifier(ICognosNotificationSubscriber notifications, string processDescription, bool includeStartFinishTime, bool appendNewLineAfterFinishing)
		{
			this.Notifications = notifications;
			this.ProcessDescription = processDescription;
			this.IncludeStartFinishTime = includeStartFinishTime;
			this.AppendNewLineAfterFinishing = appendNewLineAfterFinishing;

			notifications.Notify(CreateInfoNotification("Start"));
		}

		InfoNotification CreateInfoNotification(string prefix, bool appendNewLine)
		{
			string dateTimeString = (IncludeStartFinishTime) ? ZDateTime.Now.ToString(" - dd/MM/yyyy HH:mm:ss") : "";
			string info = string.Format("{0} {1}{2}{3}", prefix, ProcessDescription, dateTimeString, (appendNewLine) ? "\r\n" : "");
			return new InfoNotification(info);
		}

		InfoNotification CreateInfoNotification(string prefix)
		{
			return CreateInfoNotification(prefix, false);
		}

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			if (!Notifications.HasErrors)
			{
				Notifications.Notify(CreateInfoNotification("Finish", AppendNewLineAfterFinishing));
			}
		}

		#endregion

		readonly ICognosNotificationSubscriber Notifications;
		readonly string ProcessDescription;
		readonly bool IncludeStartFinishTime;
		readonly bool AppendNewLineAfterFinishing;
	}
}
