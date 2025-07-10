using NUnit.Framework;

namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor.Testing
{
	class CurrentVersionReportMessageActionTest : TestCase
	{
		public void TestCreateProcessor()
		{
			var action = new CurrentVersionReportMessageAction(null);
			var processor = (CurrentVersionReportProcessor)action.CreateProcessor("", null);
			AssertEquals("ReceivedViaEhub", true, processor.ReceivedViaEhub);
		}
	}
}
