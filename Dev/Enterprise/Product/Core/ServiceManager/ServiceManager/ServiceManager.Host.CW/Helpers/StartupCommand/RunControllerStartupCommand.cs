using System;
using System.ServiceProcess;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW
{
	class RunControllerStartupCommand : IHostStartupCommand
	{
		public RunControllerStartupCommand(IControllerService controllerService)
		{
			this.controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
		}

		public int Execute()
		{
			ServiceBase.Run(controllerService.GetService());

			return 0;
		}

		readonly IControllerService controllerService;
	}
}
