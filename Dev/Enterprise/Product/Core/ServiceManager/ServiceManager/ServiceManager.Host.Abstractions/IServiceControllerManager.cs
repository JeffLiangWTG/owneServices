using System.Collections.Generic;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace ServiceManager.Host.Abstractions;

public interface IServiceControllerManager
{
	IReadOnlyDictionary<ServiceType, IServiceController> GetServiceControllers(IServiceManagerHostOptions hostOptions);
	void Start(IServiceController sc);
	void Stop(IServiceController sc);
}
