using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Runner;

class RunnerServiceTaskLoaderFactory : IServiceTaskLoaderFactory
{
	public RunnerServiceTaskLoaderFactory(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public IServiceTaskLoader CreateServiceTaskLoader()
	{
		return (IServiceTaskLoader)serviceProvider.GetRequiredService(typeof(NativeServiceTaskLoader));
	}

	readonly IServiceProvider serviceProvider;
}

