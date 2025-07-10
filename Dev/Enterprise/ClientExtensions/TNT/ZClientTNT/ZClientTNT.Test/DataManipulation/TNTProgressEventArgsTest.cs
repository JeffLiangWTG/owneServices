using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	public class TNTProgressEventArgsTest : TestCase
	{
		public void TestConstructor()
		{
			TNTProgressEventArgs eventArgs = new TNTProgressEventArgs(10, "Test Message");
			AssertEquals("PercentComplete", 10, eventArgs.PercentComplete);
			AssertEquals("Message", "Test Message", eventArgs.Message);
			eventArgs = new TNTProgressEventArgs(120, 1000, "Test Message 2");
			AssertEquals("PercentComplete", 12, eventArgs.PercentComplete);
			AssertEquals("Message", "Test Message 2", eventArgs.Message);
		}
	}
}
