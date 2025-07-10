namespace ServiceManager.Runner.Abstractions
{
	public interface IServiceTaskHandlerInitializer
	{
		IDisposableServiceTaskHandler CreateServiceTaskHandler(string assemblyName, string code, string taskConfigString = "");
	}
}
