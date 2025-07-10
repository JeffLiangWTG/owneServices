using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	internal interface IServiceHostTerminator
	{
		void TerminateInactiveServiceTaskHosts(IHostLogger hostLogger);
	}
}
