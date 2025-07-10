using System.Threading;
using Enterprise.Integration;

namespace ServiceManager.Integration.ServiceTasks.CW
{
	public abstract class ServiceProviderImpl
	{
		public abstract void RunTask(CancellationToken youMustReactToThisToken);

		public string ServiceCode { get; set; }
		public ILogger ServiceLogger { get; set; }
	}
}
