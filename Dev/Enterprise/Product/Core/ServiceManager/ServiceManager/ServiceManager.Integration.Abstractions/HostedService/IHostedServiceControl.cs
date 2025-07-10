namespace ServiceManager.Integration.Abstractions
{
	public interface IHostedServiceControl
	{
		string ConfigControlTypeName { get; }
		string ConfigControlTypeAssemblyName { get; }
	}
}
