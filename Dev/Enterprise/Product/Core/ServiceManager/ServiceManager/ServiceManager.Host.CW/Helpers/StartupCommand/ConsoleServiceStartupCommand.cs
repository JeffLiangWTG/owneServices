using System;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW
{
	class ConsoleServiceStartupCommand : IHostStartupCommand
	{
		public ConsoleServiceStartupCommand(IControllerService controllerService)
		{
			this.controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
		}

		public int Execute()
		{
			controllerService.ConsoleRun(() => Console.ReadLine());

			return 0;
		}

		readonly IControllerService controllerService;
	}
}
