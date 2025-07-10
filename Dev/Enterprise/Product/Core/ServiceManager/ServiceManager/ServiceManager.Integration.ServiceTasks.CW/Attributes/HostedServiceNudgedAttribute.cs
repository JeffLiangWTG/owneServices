using System;

namespace ServiceManager.Integration.ServiceTasks.CW
{
	[AttributeUsage(AttributeTargets.Method)]
	[Serializable]
	public sealed class HostedServiceNudgedAttribute : Attribute
	{
	}
}
