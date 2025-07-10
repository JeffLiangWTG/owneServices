using System;
using System.ServiceProcess;

namespace ServiceManager.Host.Abstractions
{
	public interface IControllerService
	{
		TimeSpan RequireSwitchOffTime();
		void ConsoleRun(Action? wait);
		ServiceBase GetService();
	}
}
