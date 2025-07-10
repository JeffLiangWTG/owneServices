using System.Threading;

namespace Enterprise.LogWalker
{
	interface ILogWalkerOperation
	{
		void Execute(SubscriberParameters subscriberParameters, LogSubscriber[] subscribers, CancellationToken token);
	}
}
