using CargoWise.Data;

namespace Enterprise.LogWalker.Testing
{
	static class NewsPublisherExtensionsForTesting
	{
		public static void CreateNewLogQueueItems(this NewsPublisher newsPublisher, LogSubscriber[] subscribers)
		{
			while
			(
				newsPublisher.CreateLogQueueItemsForWorkflowTriggerEvents(Db.Connection) > 0 ||
				newsPublisher.CreateLogQueueItemsForEverythingExceptWorkflowTriggerEvents(Db.Connection, subscribers) > 0
			)
			{
			}
		}
	}
}
