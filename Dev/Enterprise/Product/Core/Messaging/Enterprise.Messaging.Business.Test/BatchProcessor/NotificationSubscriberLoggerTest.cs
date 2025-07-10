using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.BatchProcessor
{
	sealed class NotificationSubscriberLoggerTest : TestCaseWithFactory
	{
		#region TestLog

		public void TestLog()
		{
			Logger.Log("Random Text");
			AssertEquals("Random Text\r\n", Subscriber.AsString);
		}

		#endregion

		#region Implementation

		LoggingInformation Logger
		{
			get { return logger ?? (logger = new NotificationSubscriberLogger(Subscriber)); }
		}
		LoggingInformation logger;

		NotificationBuffer Subscriber
		{
			get { return subscriber ?? (subscriber = new NotificationBuffer()); }
		}
		NotificationBuffer subscriber;

		#endregion
	}
}
