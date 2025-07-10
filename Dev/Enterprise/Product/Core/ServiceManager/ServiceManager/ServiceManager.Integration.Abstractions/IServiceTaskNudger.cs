using System;

namespace ServiceManager.Integration.Abstractions
{
	public interface IServiceTaskNudger
	{
		void NudgeServiceTask(string serviceTaskCode, TimeSpan? delay = null);
	}
}
