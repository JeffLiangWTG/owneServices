using CargoWise.Application;
using ServiceManager.Integration.ServiceHostClient;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace ServiceManager.Shared.CW
{
	public class ServiceHostsCacheWithProvider : ServiceHostsCache
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Used in ObjectFactory without dispose function")]
		ServiceHostsCacheWithProvider() : base(ObjectFactory.Get<IServiceHostsProvider>(), new EDIServiceHostClientFactory())
		{
		}
	}
}
