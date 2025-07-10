using System;
using System.ServiceProcess;

namespace Enterprise.RemotePrinting.Client
#if WixCustomAction
.CustomAction // Different namespace for file copy in Setup project (to prevent error 'same symbol name defined in multiple project').
#endif
{
	public class PrintServiceController : IServiceController
	{
		public PrintServiceController(ServiceController controller)
		{
			Controller = controller;
		}

		readonly ServiceController Controller;

		public ServiceControllerStatus Status => Controller.Status;

		public ServiceStartMode StartType => Controller.StartType;

		public void Stop() => Controller.Stop();

		public void Start() => Controller.Start();

		public void WaitForStatus(ServiceControllerStatus desiredStatus, TimeSpan timeout) => Controller.WaitForStatus(desiredStatus, timeout);

		public void Dispose() => Controller.Dispose();
	}
}
