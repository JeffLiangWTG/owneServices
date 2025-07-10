using System;
using System.ServiceProcess;

namespace ServiceManager.Host.Abstractions;

public interface IServiceController : IDisposable
{
	string ServiceName { get; }
	ServiceControllerStatus Status { get; }
	void Start();
	void Stop();
	void WaitForStatus(ServiceControllerStatus status, TimeSpan timeout);
}
