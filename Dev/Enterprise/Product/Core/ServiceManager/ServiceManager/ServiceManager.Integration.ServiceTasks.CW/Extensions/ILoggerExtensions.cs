using CargoWise.ComponentModel;
using Enterprise.Integration;

namespace ServiceManager.Integration.ServiceTasks.CW
{
	public static class ILoggerExtensions
	{
		public static INotifications GetTaskNotificationSubscriber(this ILogger logger)
		{
			 return new TaskNotificationSubscriber(logger);
		}
	}
}
