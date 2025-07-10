using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Testing
{
	sealed class BatchProcessorNotificationBufferBridgeTest : TestCase
	{
		public void TestNotifyEventIsBatchNotification()
		{
			BatchNotification batchNotify = new BatchNotification(ErrorType.Error, "Hello World");
			Notify.Notify(batchNotify);
			AssertEquals("Event did not get notified", 1, Notify.Events.Length);
			AssertEquals("Event did not get added to logger", 1, Logger.UserLogStrings.Count);

			Assert("Incorrect log in expected order", ((ZString)Logger.UserLogStrings[0]).Contains("Error: Hello World"));
		}

		public void TestNotifyEventIsErrorNotification()
		{
			ErrorNotification batchNotify = new ErrorNotification(ErrorType.Error, "Hello World");
			Notify.Notify(batchNotify);
			AssertEquals("Event did not get notified", 1, Notify.Events.Length);
			AssertEquals("Event did not get added to logger", 1, Logger.UserLogStrings.Count);

			Assert("Incorrect log in expected order", ((ZString)Logger.UserLogStrings[0]).Contains("Error: Hello World"));
		}

		public void TestNotifyEventIsNotBatchNotification()
		{
			InfoNotification batchNotify = new InfoNotification("Hello World");
			Notify.Notify(batchNotify);
			AssertEquals("Event did not get notified", 1, Notify.Events.Length);
			AssertEquals(0, Logger.UserLogStrings.Count);
		}

		public void TestInnerIsNotNull()
		{
			AssertNotNull(Notify.Inner);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Logger = new LoggingInformation();
			Notify = new BatchProcessorNotificationBufferBridge(Logger);
		}

		BatchProcessorNotificationBufferBridge Notify;
		LoggingInformation Logger;

		#endregion
	}
}
