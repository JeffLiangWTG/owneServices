using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class BulkConsolidateProgressEventArgsTest : TestCase
	{
		public void TestConstructor()
		{
			BulkConsolidateProgressEventArgs eventArgs = new BulkConsolidateProgressEventArgs(10, "Test Message");
			AssertEquals("percentComplete", 10, eventArgs.percentComplete);
			AssertEquals("message", "Test Message", eventArgs.message);

			eventArgs = new BulkConsolidateProgressEventArgs(120, 1000, "Test Message 2");
			AssertEquals("percentComplete", 12, eventArgs.percentComplete);
			AssertEquals("message", "Test Message 2", eventArgs.message);
		}
	}
}
