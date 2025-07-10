using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Client.UPE.ServiceTask
{
	[NeedsDataRefresh]
	public abstract class UPEServiceTask : ServiceProviderImpl
	{
		ILogger logger;
		INotifications notifications;

		protected UPEServiceTask()
		{
		}

		protected UPEServiceTask(ILogger logger)
		{
			ServiceLogger = logger;
		}

		protected void Notify(ZString message)
		{
			Notifications.Notify(new InfoNotification(message));
		}

		protected void TryMoveOrDeleteFile(ZString sourceFile, ZString archiveDirectory)
		{
			try
			{
				if (!archiveDirectory.IsEmpty)
				{
					string destFileName = ZGuid.NewZGuid().ToString() + ".txt";
					string destFile = Path.Combine(archiveDirectory, destFileName);
					File.Move(sourceFile, destFile);
				}
				else
				{
					File.Delete(sourceFile);
				}
			}
			catch (IOException)
			{
				WarningNotification warning = new WarningNotification("Cannot archive or delete file \"" + sourceFile + "\"");
				Notifications.Notify(warning);
			}
		}

		protected void TryDeleteFile(ZString sourceFile)
		{
			TryMoveOrDeleteFile(sourceFile, "");
		}

		public INotifications Notifications
		{
			get
			{
				if (notifications == null)
				{
					notifications = ServiceLogger.GetTaskNotificationSubscriber();
				}
				return notifications;
			}
		}

		public ILogger Logger
		{
			get
			{
				if (logger == null)
				{
					logger = ServiceLogger;
				}
				return logger;
			}
		}
	}
}
