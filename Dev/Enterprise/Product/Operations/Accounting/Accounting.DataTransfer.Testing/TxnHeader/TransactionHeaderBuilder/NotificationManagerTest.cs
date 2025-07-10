using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class NotificationManagerTest : TestCase
	{
		public void TestReportNoBizObjsFoundWarningDoesNotReportIfValueForDisplayIsEmpty()
		{
			NotificationManager manager = new NotificationManager(new NotificationBuffer());
			manager.ReportNoBizObjsFoundWarning(null, "Test Business Object Description", "", "Test Context");
			AssertEquals(0, ((NotificationBuffer)(manager.NotificationSubscriber)).Events.Length);

			manager.ReportNoBizObjsFoundWarning(null, "Test Business Object Description", "Some Value", "Test Context");
			AssertEquals(1, ((NotificationBuffer)(manager.NotificationSubscriber)).Events.Length);
			AssertEquals("Warning: Test ContextNo matches were found for the following Test Business Object Description: Some Value", (((NotificationBuffer)(manager.NotificationSubscriber)).Events[0]).Message);
		}
	}
}
