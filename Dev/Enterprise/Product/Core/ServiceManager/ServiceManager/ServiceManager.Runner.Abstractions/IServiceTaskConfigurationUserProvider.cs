using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Runner.Abstractions
{
	public interface IServiceTaskConfigurationUserProvider
	{
		IServiceTaskConfigurationUser GetServiceTaskConfigurationUser();
	}
}
