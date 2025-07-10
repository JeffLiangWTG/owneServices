using System;

namespace ServiceManager.Integration.ServiceTasks.CW
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class NeedsDataRefreshAttribute : Attribute { }
}
