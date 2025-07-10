using NLog;
using NLog.Config;
using NLog.Targets;

namespace ServiceManager.Logging.CW
{
	class NLogEventLogTargetFactory : INLogTargetFactory
	{
		public Target GetOrCreateTarget()
		{
			LogManager.Configuration ??= new LoggingConfiguration();

			var target = LogManager.Configuration.FindTargetByName<EventLogTarget>(TargetName);
			if (target != null)
			{
				return target;
			}

			target = new EventLogTarget
			{
				Name = TargetName,
				OnOverflow = EventLogTargetOverflowAction.Split,
				Source = CargoWise.BrandManager.BrandingFactory.Instance.ProductName,
				Log = "Application",
				EventId = "${event-properties:EventId:whenEmpty=0}",
				Layout = @"${message}

${exception:format=toString}

PID:${processid}
ApplicationPhysicalPath:${event-properties:applicationPath}
ProcessControllerVersion:${event-properties:processcontroller:objectpath=version}
ServerName:${event-properties:database_host}
DatabaseName:${event-properties:database}
User:${environment-user}",
			};

			return target;
		}

		public const string TargetName = nameof(EventLogTarget);
	}
}
