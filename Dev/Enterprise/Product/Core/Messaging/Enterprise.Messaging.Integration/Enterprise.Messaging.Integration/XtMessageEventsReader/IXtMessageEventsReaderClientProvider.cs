namespace Enterprise.Messaging.Integration
{
	public interface IXtMessageEventsReaderClientProvider
	{
		IXtMessageEventsReaderClient XtMessageEventsReaderClient { get; }

		void TearDown();
	}
}
