using System.ServiceProcess;

namespace CargoWise.Loader.Common
{
	public interface IServiceControllerProxy
	{
		bool TryGetStatus(string serviceName, out ServiceControllerStatus status);
		bool Start(string serviceName);
		bool Stop(string serviceName);

		string LastError { get; }
	}
}