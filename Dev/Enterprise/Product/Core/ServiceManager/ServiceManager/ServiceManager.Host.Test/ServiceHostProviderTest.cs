using System.Net;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	public class ServiceHostProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestInitializeController_SetsProxyIfAutodetectDisabled()
		{
			var originalProxy = WebRequest.DefaultWebProxy;
			try
			{
				// Arrange
				var logger = new Mock<IHostLogger>();
				var hostProvider = new ServiceHostProvider(Factory, Mock.Of<IHostRegistrySettings>());
				var host = hostProvider.CreateServiceHost("Host");
				host.SH_ProxyAutoDetect = ZBool.False;
				host.SH_ProxyHost = "Host";
				host.SH_ProxyPort = 123;

				// Act
				hostProvider.InitializeController(host, new Mock<IHostLogger>().Object);

				// Assert
				var expected = new WebProxy("Host", 123);
				var proxy = WebRequest.DefaultWebProxy;
				NUnit.Framework.Assert.That(((WebProxy)proxy).Address, Is.EqualTo(expected.Address));
			}
			finally
			{
				WebRequest.DefaultWebProxy = originalProxy;
			}
		}
	}
}
