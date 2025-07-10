using System.Threading;

namespace Enterprise.eHubMessaging.Business
{
	public interface IeHubServiceTaskJob
	{
		void Execute(CancellationToken token);

		bool NextExecuteIterationIsScheduled { get; }
	}
}
