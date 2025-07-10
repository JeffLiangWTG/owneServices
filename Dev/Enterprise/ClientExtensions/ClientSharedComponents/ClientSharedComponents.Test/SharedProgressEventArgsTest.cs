using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Testing
{
	public class SharedProgressEventArgsTest : TestCase
	{
		public void TestConstructor()
		{
			SharedProgressEventArgs eventArgs = new SharedProgressEventArgs(10, "Test Message");
			AssertEquals("PercentComplete", 10, eventArgs.PercentComplete);
			AssertEquals("Message", "Test Message", eventArgs.Message);

			eventArgs = new SharedProgressEventArgs(120, 1000, "Test Message 2");
			AssertEquals("PercentComplete", 12, eventArgs.PercentComplete);
			AssertEquals("Message", "Test Message 2", eventArgs.Message);
		}

		public void TestMaxPercentageComplete()
		{
			SharedProgressEventArgs eventArgs = new SharedProgressEventArgs(101, "Test Message");
			AssertEquals("PercentComplete", 100, eventArgs.PercentComplete);

			eventArgs = new SharedProgressEventArgs(101, 100, "Test Message 2");
			AssertEquals("PercentComplete", 100, eventArgs.PercentComplete);
		}

		public void TestConstructor2()
		{
			SharedProgressEventArgs eventArgs = new SharedProgressEventArgs(10, 100, "Test Message");
			AssertEquals("PercentComplete", 10, eventArgs.PercentComplete);
			AssertEquals("Message", "Test Message", eventArgs.Message);

			eventArgs = new SharedProgressEventArgs(120, 1000, "Test Message 2");
			AssertEquals("PercentComplete", 12, eventArgs.PercentComplete);
			AssertEquals("Message", "Test Message 2", eventArgs.Message);
		}

		public void TestMaxPercentageComplete2()
		{
			SharedProgressEventArgs eventArgs = new SharedProgressEventArgs(101, 100, "Test Message");
			AssertEquals("PercentComplete", 100, eventArgs.PercentComplete);

			eventArgs = new SharedProgressEventArgs(101, 100, "Test Message 2");
			AssertEquals("PercentComplete", 100, eventArgs.PercentComplete);
		}
	}
}
