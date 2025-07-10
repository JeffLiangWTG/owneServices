using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	public class WebPrintDbEnvironmentTest : TestCase
	{
		public void TestConnectionPoolingMinPoolSize()
		{
			var environment = new WebPrintDbEnvironment();
			AssertEquals("MinPoolSize should be 1", 1, environment.ConnectionPooling.MinPoolSize);
		}
	}
}
