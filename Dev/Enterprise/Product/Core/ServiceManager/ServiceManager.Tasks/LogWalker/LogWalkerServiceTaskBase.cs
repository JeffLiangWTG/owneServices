using System.Threading;
using Enterprise.Integration;
using Enterprise.LogWalker;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Tasks.LogWalker
{
	public abstract class LogWalkerServiceTaskBase : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			Runner.Process(new LogWalkerCategoryLogger(ServiceLogger), token);
		}

		protected abstract LogWalkerRunner Runner { get; }

		public void Stop()
		{
			new LogWalkerCategoryLogger(ServiceLogger).Log(LogType.Information, "Host sent stop signal.");
		}
	}
}
