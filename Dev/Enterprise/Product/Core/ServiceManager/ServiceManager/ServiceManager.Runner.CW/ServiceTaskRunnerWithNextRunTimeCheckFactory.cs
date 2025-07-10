using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner;

class ServiceTaskRunnerWithNextRunTimeCheckFactory : IServiceTaskRunnerWithNextRunTimeCheckFactory
{
	public ServiceTaskRunnerWithNextRunTimeCheckFactory(IServiceProvider? serviceProvider)
	{
		this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public IServiceTaskRunnerWithNextRunTimeCheck CreateRunner()
		=> (IServiceTaskRunnerWithNextRunTimeCheck)serviceProvider.GetService(typeof(NativeServiceTaskRunnerWithNextRunTimeCheck))!;

	readonly IServiceProvider serviceProvider;
}

