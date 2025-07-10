using System;
using System.Collections;
using Moq;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Common
{
	class TcpIpRegistrySettingsInstallerTest
	{
		[Test]
		public void TestEnforceSetIsCalledDuringInstall()
		{
			//Arrange
			var tcpIpRegistryAdjuster = new Mock<ITcpIpRegistryAdjuster>();
			using var installer = new TcpIpRegistrySettingsInstaller(tcpIpRegistryAdjuster.Object);
			var stateSaver = new Mock<IDictionary>();

			//Act
			installer.Install(stateSaver.Object);

			//Assert
			tcpIpRegistryAdjuster.Verify(t => t.Adjust(), Times.Once);
			tcpIpRegistryAdjuster.VerifyNoOtherCalls();
		}

		[Test]
		public void TestWrongParameters()
		{
			var ex = Assert.Throws<ArgumentNullException>(() => new TcpIpRegistrySettingsInstaller(null));
			Assert.That(ex?.ParamName, Is.EqualTo("tcpIpRegistryAdjuster"));
			Assert.DoesNotThrow(() => new TcpIpRegistrySettingsInstaller());
		}
	}
}
