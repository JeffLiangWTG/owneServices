using System.Collections.Generic;

namespace ServiceManager.Host.Abstractions;

public interface IServiceControllerFactory
{
	IEnumerable<IServiceController> GetServices(string host);
}
