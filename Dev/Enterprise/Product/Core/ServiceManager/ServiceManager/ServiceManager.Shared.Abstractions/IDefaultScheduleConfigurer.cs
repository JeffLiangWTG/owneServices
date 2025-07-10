using System;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Shared.Abstractions
{
	public interface IDefaultScheduleConfigurer
	{
		void SetDefaultScheduleForTask(IServiceTaskGovernor taskGovernor, IHostedServiceAttribute hostedServiceAttribute);
		void SetNextRunTimeForTask(IServiceTaskGovernor taskGovernor, DateTime nextRunTime);
	}
}
