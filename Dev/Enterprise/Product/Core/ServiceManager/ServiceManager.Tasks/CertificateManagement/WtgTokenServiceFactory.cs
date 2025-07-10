using CargoWise.SystemToSystemTrust;
using Enterprise.Registry.Business;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement;

public class WtgTokenServicesFactory() : TokenServicesFactory(BuildServiceCollection())
{
	static IServiceCollection BuildServiceCollection()
	{
		return new ServiceCollection()
			.AddSingleton(ObjectFactory.Get<IServiceHostsCache>())
			.AddSingleton(ObjectFactory.Get<IDbAccessTokenRetriever>())
			.AddSingleton<ISystemToSystemTrustInfoService>(sp => new SystemToSystemTrustInfoService())
			.AddSingleton<ITokenConfigRetrieverService>(sp => sp.GetRequiredService<ISystemToSystemTrustInfoService>())
			.AddSingleton<ITokenSigningStatusChecker>(sp => new DataProtectorServiceFactory())
			.AddHttpClient();
	}
}
