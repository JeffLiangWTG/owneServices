using CargoWise.ComponentModel;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture
{
	public static class NotificationToILoggerConversion
	{
		public static LogType ToLogType(this INotificationType type)
		{
			if (type.IsFatal || type == CargoWise.EntityFramework.NotificationType.MessageError)
			{
				return LogType.Error;
			}
			else if (type == NotificationType.Warning)
			{
				return LogType.Warning;
			}
			else if (type.Equals(NotificationSubscriberType.VerboseInfo))
			{
				return LogType.Debug;
			}
			else
			{
				return LogType.Information;
			}
		}
	}
}
