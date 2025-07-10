using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	interface ITcpIpRegistryAdjuster
	{
		void Adjust();
		void TryAdjustIfRequired(IHostLogger logger);
	}
}
