using System.Threading;

namespace Enterprise.ServiceManager.Host
{
	public interface IServiceManagerApplication
	{
		void Run(CancellationToken cancellationToken);
	}
}