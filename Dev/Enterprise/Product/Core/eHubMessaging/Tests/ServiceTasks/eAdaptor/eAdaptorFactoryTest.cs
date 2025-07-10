using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.ServiceTasks;

namespace Enterprise.eHubMessaging.Tests
{
	public class eAdaptorFactoryTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var factory = new eAdaptorFactory();
			AssertNotNull(factory.Create(string.Empty, string.Empty, new NotificationCollection(), "https://localhost/"));
			AssertNull(factory.Create(string.Empty, string.Empty, new NotificationCollection(), null));
			AssertNull(factory.Create(string.Empty, string.Empty, new NotificationCollection(), string.Empty));
		}
	}
}
