using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	public class WebPrintEnvironmentProviderTest : TestCase
	{
		public void TestDbEnvironmentInstance()
		{
			using (var provider = new WebPrintEnvironmentProviderForTest())
			{
				AssertType<WebPrintDbEnvironment>("GetDbEnvironmentInstance should return WebPrintDbEnvironment", provider.DbEnvironmentInstance);
			}
		}

		class WebPrintEnvironmentProviderForTest : WebPrintEnvironmentProvider
		{
			public IDbEnvironment DbEnvironmentInstance => base.GetDbEnvironmentInstance();
		}
	}
}
