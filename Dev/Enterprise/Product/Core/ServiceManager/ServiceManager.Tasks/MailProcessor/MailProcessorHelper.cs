using System;
using CargoWise.Application;
using CargoWise.Types;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Tasks.MailProcessor;

public static class MailProcessorHelper
{
	public static void RescheduleMailTask(string code, ZDateTime nextRunTimeUtc)
	{
		if (ObjectFactory.Get<IServiceManagerQuerier>().CheckStateOfNamedServiceTask(code) > ServiceTaskStatus.ServiceTaskIsInactive)
		{
			ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskNextRuntime(code, ZDateTime.Truncate(nextRunTimeUtc, TimeSpan.TicksPerSecond).UtcToDateTimeOffset().ToDateTimeOffsetSafe());
		}
	}
}
