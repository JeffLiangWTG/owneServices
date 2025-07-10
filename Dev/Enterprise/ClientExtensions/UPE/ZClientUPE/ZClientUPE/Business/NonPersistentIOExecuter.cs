using System;
using System.IO;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.Business
{
	public delegate void IOTaskDelegate();
	public delegate void IOFileTaskDelegate(FileInfo file);

	/// <summary>
	/// Exceute a delegated IO task, and give up after several attempts or nominated time out.
	/// </summary>
	public class NonPersistentIOExecuter
	{
		public NonPersistentIOExecuter(INotifications notifications, INotifications emailedNotifications)
		{
			this.notifications = notifications;
			this.emailedNotifications = emailedNotifications;
			TimeOutSeconds = 120;
			MaxRetries = 5;
		}

		public NonPersistentIOExecuter(INotifications notifications)
			: this(notifications, null)
		{
		}

		public bool ExecuteIOTask(IOTaskDelegate ioTask, string fileOrDirectoryName)
		{
			return ExecuteIOTaskCore(ioTask, null, fileOrDirectoryName);
		}

		public bool ExecuteIOFileTask(IOFileTaskDelegate ioTask, FileInfo file)
		{
			return ExecuteIOTaskCore(ioTask, file, file.FullName);
		}

		bool ExecuteIOTaskCore(Delegate ioTask, FileInfo file, string fileOrDirectoryName)
		{
			IOTaskDelegate taskDelegate = ioTask as IOTaskDelegate;
			IOFileTaskDelegate fileTaskDelegate = ioTask as IOFileTaskDelegate;

			if (taskDelegate == null && fileTaskDelegate == null)
			{
				throw new ArgumentNullException(nameof(ioTask));
			}

			bool result = false;
			bool retry;
			int retryCounter = 1;
			ZDateTime endDateTime = ZDateTime.Now.AddSeconds(TimeOutSeconds);
			do
			{
				retry = false;
				try
				{
					if (taskDelegate != null)
					{
						taskDelegate();
					}
					else if (fileTaskDelegate != null)
					{
						fileTaskDelegate(file);
					}
					result = true;
				}
				catch (UnauthorizedAccessException ex)
				{
					retry = (retryCounter++ < MaxRetries && ZDateTime.Now < endDateTime);
					if (retry)
					{
						notifications.Notify(new ErrorNotification(ErrorType.Error, string.Format(accessDeniedRetryingMessageFormat, fileOrDirectoryName)));
						Thread.Sleep(0);
					}
					else
					{
						notifications.Notify(new ErrorNotification(ErrorType.Error, string.Format(accessDeniedMessageFormat, fileOrDirectoryName)));
						if (emailedNotifications != null)
						{
							emailedNotifications.Notify(new ErrorNotification(ErrorType.Error, string.Format(accessDeniedExceptionMessageFormat, fileOrDirectoryName, ex.Message)));
						}
					}
				}
				catch (DirectoryNotFoundException)
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, string.Format(directoryNotFoundMessageFormat, fileOrDirectoryName)));
					if (emailedNotifications != null)
					{
						emailedNotifications.Notify(new ErrorNotification(ErrorType.Error, string.Format(directoryNotFoundMessageFormat, fileOrDirectoryName)));
					}
				}
				catch (IOException ex)
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, string.Format(accessDeniedMessageFormat, fileOrDirectoryName)));
					if (emailedNotifications != null)
					{
						emailedNotifications.Notify(new ErrorNotification(ErrorType.Error, string.Format(accessDeniedExceptionMessageFormat, fileOrDirectoryName, ex.Message)));
					}
				}
			} while (retry);
			return result;
		}
		const string accessDeniedMessageFormat = "Access to file '{0}' has been denied.";
		const string accessDeniedRetryingMessageFormat = accessDeniedMessageFormat + "  Trying again";
		const string accessDeniedExceptionMessageFormat = "Error found with file '{0}': {1}";
		const string directoryNotFoundMessageFormat = "Unable to find directory '{0}'.";

		public int TimeOutSeconds { get; set; }
		public int MaxRetries { get; set; }

		readonly INotifications notifications;
		readonly INotifications emailedNotifications;
	}
}
