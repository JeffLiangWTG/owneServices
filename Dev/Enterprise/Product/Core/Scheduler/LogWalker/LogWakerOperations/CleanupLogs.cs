using System.Threading;
using Enterprise.Integration;

namespace Enterprise.LogWalker
{
	class CleanupLogs : ILogWalkerOperation
	{
		public void Execute(SubscriberParameters subscriberParameters, LogSubscriber[] subscribers, CancellationToken token)
		{
			var logger = subscriberParameters.Logger;
			logger.Log(LogType.Information, "Cleaning up old logs.");
			CleanupOldLogs(logger, subscribers, token);
		}

		protected virtual void CleanupOldLogs(ILogger notifier, LogSubscriber[] subscribers, CancellationToken token)
		{
			var archive = new NewsArchive(subscribers);
			archive.CleanupOldLogs(token, notifier);
		}
	}
}
