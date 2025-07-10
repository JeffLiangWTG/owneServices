using System.Collections.Generic;

namespace Enterprise.LogWalker.Testing
{
	sealed class SubscriberProviderForTesting : SubscriberProvider
	{
		public SubscriberProviderForTesting(LogSubscriber[] subscribers)
		{
			this.subscribers = subscribers;
		}
		readonly LogSubscriber[] subscribers;

		protected internal override IEnumerable<LogSubscriber> AllLogSubscribers
		{
			get { return subscribers; }
		}
	}
}
