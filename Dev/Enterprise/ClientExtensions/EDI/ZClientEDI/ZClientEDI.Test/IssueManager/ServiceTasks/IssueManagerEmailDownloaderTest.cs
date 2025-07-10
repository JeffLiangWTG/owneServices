using CargoWise.Types;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.ServiceTask.Testing
{
	class IssueManagerEmailDownloaderTest : TransactionedTestCase
	{
		public void TestMailboxBlank()
		{
			ZString userName = EDIDataRegistry.Instance.IssueReportMailBox.UserName;
			AssertEquals("", userName);
			TestServiceLogger logger = new TestServiceLogger();
			var task = new IssueManagerEmailDownloader(logger, "");
			task.RunTask();
			AssertEquals(1, logger.Count);
			AssertEquals("Error|Issue Report Mailbox not set in Registry", logger[0]);
		}
	}
}
