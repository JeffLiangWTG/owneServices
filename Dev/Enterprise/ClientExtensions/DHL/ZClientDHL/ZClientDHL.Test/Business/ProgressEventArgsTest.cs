using NUnit.Framework;

namespace Enterprise.Client.DHL.Testing
{
	public class ProgressEventArgsTest : TestCase
	{
		public void TestConstructor()
		{
			DHLProgressEventArgs eventArgs = new DHLProgressEventArgs(10, "Test Message");
			AssertEquals("PercentComplete", 10, eventArgs.PercentComplete);
			AssertEquals("Message", "Test Message", eventArgs.Message);
			eventArgs = new DHLProgressEventArgs(120, 1000, "Test Message 2");
			AssertEquals("PercentComplete", 12, eventArgs.PercentComplete);
			AssertEquals("Message", "Test Message 2", eventArgs.Message);
		}

		public void TestMaxPercentageComplete()
		{
			DHLProgressEventArgs eventArgs = new DHLProgressEventArgs(101, "Test Message");
			AssertEquals("PercentComplete", 100, eventArgs.PercentComplete);
			eventArgs = new DHLProgressEventArgs(101, 100, "Test Message 2");
			AssertEquals("PercentComplete", 100, eventArgs.PercentComplete);
		}
	}
}
