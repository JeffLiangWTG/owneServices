using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Common.Abstractions;
public interface IServiceTasksReloaderFactory
{
	IServiceTasksReloader CreateServiceTasksReloader();
}
