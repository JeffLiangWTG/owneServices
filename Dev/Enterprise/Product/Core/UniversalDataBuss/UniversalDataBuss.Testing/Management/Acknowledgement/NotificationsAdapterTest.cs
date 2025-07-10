using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	sealed class NotificationsAdapterTest : TestCase
	{
		public void TestAdd()
		{
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceTaskLog);
			var adapter = new NotificationsAdapter(logger);

			var notifications = new NotificationBuffer();
			notifications.AddError("Expected message");

			adapter.Add(notifications.Events[0]);
			AssertEquals("ERROR - Expected message", serviceTaskLog.ToString());
		}
	}
}
