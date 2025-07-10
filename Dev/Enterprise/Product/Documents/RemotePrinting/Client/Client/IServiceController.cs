using System;
using System.ServiceProcess;

namespace Enterprise.RemotePrinting.Client
#if WixCustomAction
.CustomAction // Different namespace for file copy in Setup project (to prevent error 'same symbol name defined in multiple project').
#endif
{
	public interface IServiceController : IDisposable
	{
		ServiceControllerStatus Status { get; }

		ServiceStartMode StartType { get; }

		void Stop();

		void Start();

		void WaitForStatus(ServiceControllerStatus desiredStatus, TimeSpan timeout);
	}
}
