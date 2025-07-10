using System;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Business
{
	public static class LogsCollectorExtensions
	{
		public static void AddInfo(this INotifications logsCollector, string message)
		{
			logsCollector?.Add(new InfoNotification(message));
		}

		public static void AddVerboseInfo(this INotifications logsCollector, Func<string> genMessage)
		{
			logsCollector?.Add(new VerboseInfoNotification(genMessage()));
		}
	}
}
