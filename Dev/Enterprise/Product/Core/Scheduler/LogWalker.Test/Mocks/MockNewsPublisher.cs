using CargoWise.Data;

namespace Enterprise.LogWalker.Test
{
	public class MockNewsPublisher : NewsPublisher
	{
		public int PublishForTest(DbConnection connection, LogSubscriber[] subscribers)
		{
			return CreateLogQueueItemsForEverythingExceptWorkflowTriggerEvents(connection, subscribers)
				+ CreateLogQueueItemsForWorkflowTriggerEvents(connection);
		}
	}
}
