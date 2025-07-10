namespace ServiceManager.Integration.Abstractions
{
	public interface IHostedServiceType
	{
		string TypeName { get; }
		string TypeAssemblyName { get; }
	}
}
