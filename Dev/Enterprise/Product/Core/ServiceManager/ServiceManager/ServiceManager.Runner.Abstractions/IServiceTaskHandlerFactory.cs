namespace ServiceManager.Runner.Abstractions
{
	public interface IServiceTaskHandlerFactory
	{
		IServiceTaskHandler CreateServiceTaskHandler(string assemblyName, string typeName);
	}
}
