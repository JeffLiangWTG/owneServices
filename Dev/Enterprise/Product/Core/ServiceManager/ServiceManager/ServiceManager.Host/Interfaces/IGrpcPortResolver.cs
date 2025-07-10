namespace Enterprise.ServiceManager.Host
{
	public interface IGrpcPortResolver
	{
		bool PortOpened { get; }
	}
}
