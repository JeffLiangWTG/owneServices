using System;
using Enterprise.Registry.Business;

namespace Enterprise.ClientSharedComponents.ServiceTasks
{
	public static class ServiceTaskOptionsInitialiser
	{
		const string MinimumIntervalAsString = "1";
		public const string MinimumIntervalType = AutomaticProcessRegistryBusinessObject.IntervalTypes.Minutes;
		public const string MinimumIntervalDuration = MinimumIntervalAsString + MinimumIntervalType;
		public static int MinimumInterval => ((IConvertible)MinimumIntervalAsString).ToInt32(null);
	}
}
