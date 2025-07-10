using System;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	class NudgeInterceptor : IServiceTaskNudger
	{
		public NudgeInterceptor(string code)
		{
			Code = code;
		}
		string Code { get; }
		public int FireCount { get; private set; }

		public void NudgeServiceTask(string serviceTaskCode, TimeSpan? delay = null)
		{
			if (serviceTaskCode == Code)
			{
				FireCount++;
			}
		}
	}
}
