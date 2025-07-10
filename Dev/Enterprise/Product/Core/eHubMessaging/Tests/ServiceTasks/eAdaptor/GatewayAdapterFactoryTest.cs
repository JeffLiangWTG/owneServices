using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.ServiceTasks;

namespace Enterprise.eHubMessaging.Tests
{
	public class GatewayAdaptorFactoryTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var factory = new GatewayAdaptorFactory();
			AssertNotNull(factory.Create(string.Empty, string.Empty, new NotificationCollection(), "www.wisetechglobal.com"));
			AssertNull(factory.Create(string.Empty, string.Empty, new NotificationCollection(), null));
			AssertNull(factory.Create(string.Empty, string.Empty, new NotificationCollection(), string.Empty));
		}
	}
}
