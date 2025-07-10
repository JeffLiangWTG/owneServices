using System;

namespace ServiceManager.Integration.Abstractions
{
	public interface IServiceManagerGovernor
	{
		void SetServiceTaskNextRuntime(string serviceTaskCode, DateTimeOffset? nextRunTime);
		void SetServiceTaskIsActive(string serviceTaskCode, bool isActive);
	}
}
