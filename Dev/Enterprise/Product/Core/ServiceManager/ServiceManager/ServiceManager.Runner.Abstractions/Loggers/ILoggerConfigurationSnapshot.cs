using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Runner.Abstractions
{
	public interface ILoggerConfigurationSnapshot
	{
		void VerifyCurrentConfiguration(IHostedServiceAttribute hostedServiceAttribute);
	}
}
