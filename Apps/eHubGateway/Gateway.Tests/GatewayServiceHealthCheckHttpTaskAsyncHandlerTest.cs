using CargoWise.eHub.Gateway.HealthCheck;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Gateway.Tests
{
	[TestClass]
	public class GatewayServiceHealthCheckHttpTaskAsyncHandlerTest
	{
		[TestMethod]
		public void TestGatewayServiceHealthCheckHttpTaskAsyncHandler()
		{
			var healthCheckHttpTaskAsyncHandler = new GatewayServiceHealthCheckHttpTaskAsyncHandler();
			Assert.IsTrue(healthCheckHttpTaskAsyncHandler != null);
		}
	}
}
