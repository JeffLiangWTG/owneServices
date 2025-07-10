using System;

namespace ServiceManager.Shared.Abstractions
{
	public interface IServiceTasksReloader
	{
		IServiceTaskCollectionGovernor Reload(DateTimeOffset lastUpdateTime);
	}
}
