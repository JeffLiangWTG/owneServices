using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Integration;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.LogWalker.Testing
{
	class OperationsManagerForTesting : OperationsManager
	{
		public OperationsManagerForTesting(LogSubscriber[] subscribers)
		{
			TestSubscriberProvider = new SubscriberProviderForTesting(subscribers);
		}
		internal SubscriberProviderForTesting TestSubscriberProvider { get; }

		public override void QueueAndProcessLogs(ILogger notifier)
		{
			using var mock = ObjectFactory.Substitute(Mock.Of<IServiceTaskNudger>());
			base.QueueAndProcessLogs(notifier);
		}

		protected override IEnumerable<ILogWalkerOperation> GetOrderedOperations()
		{
			yield return GetCleanup();
			yield return GetQueue();
			yield return GetNewBroadcaster();
		}

		public virtual QueueLogs GetQueue() => queueLogs ?? (queueLogs = new QueueLogs());
		QueueLogs queueLogs;

		protected virtual CleanupLogs GetCleanup() => new CleanupLogs();

		protected override SubscriberProvider SubscriberProvider
		{
			get { return TestSubscriberProvider; }
		}

		protected virtual NewsBroadcaster GetNewBroadcaster() => new NewsBroadcaster();
	}
}
