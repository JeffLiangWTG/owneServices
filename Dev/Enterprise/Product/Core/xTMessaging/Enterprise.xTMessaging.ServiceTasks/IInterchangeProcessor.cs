using System.Threading;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.ServiceTasks
{
	public interface IInterchangeProcessor
	{
		void Process(Configuration config, CancellationToken token);
	}
}
