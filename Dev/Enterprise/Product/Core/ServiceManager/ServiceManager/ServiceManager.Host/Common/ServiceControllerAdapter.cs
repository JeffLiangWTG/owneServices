using System;
using System.ServiceProcess;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host;

sealed class ServiceControllerAdapter : IServiceController
{
	readonly ServiceController serviceController;

	public ServiceControllerAdapter(ServiceController serviceController)
	{
		this.serviceController = serviceController;
	}

	public string ServiceName => serviceController.ServiceName;

	public ServiceControllerStatus Status => serviceController.Status;

	public void Start() => serviceController.Start();

	public void Stop() => serviceController.Stop();

	public void WaitForStatus(ServiceControllerStatus status, TimeSpan timeout)
		=> serviceController.WaitForStatus(status, timeout);

	public void Dispose() => serviceController.Dispose();
}
