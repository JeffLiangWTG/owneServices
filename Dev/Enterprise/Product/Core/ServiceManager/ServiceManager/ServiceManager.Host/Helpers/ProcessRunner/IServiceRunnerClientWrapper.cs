using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceManagerProto;

namespace Enterprise.ServiceManager.Host
{
	public interface IServiceRunnerClientWrapper : IDisposable
	{
		public void SendRequest(ServiceTaskRunRequest request);
		public Task<bool> NextResponseAsync(CancellationToken cancellationToken);
		public ServiceTaskRunResponse CurrentResponse { get; }
		public void Close();
	}
}
