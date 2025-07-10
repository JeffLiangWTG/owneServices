using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Common
{
	class TcpIpRegistryAdjusterTest
	{
		class AdjustTest
		{
			[Test]
			public void TestSettingsAreAdjustedCorrectly()
			{
				//Act
				tcpIpRegistryAdjuster!.Adjust();

				//Assert
				registryAdapterMock!.Verify(c => c.SetLocalMachineDwordRegistryValue(TcpIpRegistryAdjuster.TcpipRegistrySubKey, TcpIpRegistryAdjuster.TcpipRegistryParameter_MaxUserPort, 65534), Times.Once);
				registryAdapterMock.Verify(c => c.SetLocalMachineDwordRegistryValue(TcpIpRegistryAdjuster.TcpipRegistrySubKey, TcpIpRegistryAdjuster.TcpipRegistryParameter_TcpTimedWaitDelay, 30), Times.Once);
				registryAdapterMock.VerifyNoOtherCalls();
			}

			[Test]
			public void TestDoesNotCatchExceptions()
			{
				//Arrange
				var error = new InvalidOperationException("registry write error");
				registryAdapterMock!.Setup(rw => rw.SetLocalMachineDwordRegistryValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Throws(error);

				//Act
				void act()
				{
					tcpIpRegistryAdjuster!.Adjust();
				}

				//Assert
				var thrown = Assert.Throws<InvalidOperationException>(act);
				Assert.That(thrown, Is.EqualTo(error));
			}

			[Test]
			public void TestDoesNotRead()
			{
				//Act
				tcpIpRegistryAdjuster!.Adjust();

				//Assert
				registryAdapterMock!.Verify(r => r.ReadLocalMachineDwordRegistryValue(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			}

			[SetUp]
			public void SetUp()
			{
				registryAdapterMock = new Mock<IWindowsRegistryAdapter>();
				tcpIpRegistryAdjuster = new TcpIpRegistryAdjuster(registryAdapterMock.Object);
			}

			Mock<IWindowsRegistryAdapter>? registryAdapterMock;
			TcpIpRegistryAdjuster? tcpIpRegistryAdjuster;
		}

		class TryAdjustIfRequiredTest
		{
			[Test]
			public void TestSettingsAreAdjustedCorrectly()
			{
				//Act
				tcpIpRegistryAdjuster!.TryAdjustIfRequired(loggerMock!.Object);

				//Assert
				Assert.DoesNotThrow(() =>
				{
					registryAdapterMock!.Verify(c => c.ReadLocalMachineDwordRegistryValue(TcpIpRegistryAdjuster.TcpipRegistrySubKey, TcpIpRegistryAdjuster.TcpipRegistryParameter_MaxUserPort), Times.Once);
					registryAdapterMock.Verify(c => c.SetLocalMachineDwordRegistryValue(TcpIpRegistryAdjuster.TcpipRegistrySubKey, TcpIpRegistryAdjuster.TcpipRegistryParameter_MaxUserPort, 65534), Times.Once);
				}, "Correct value of MaxUserPort was not read and/or set");

				Assert.DoesNotThrow(() =>
				{
					registryAdapterMock!.Verify(c => c.ReadLocalMachineDwordRegistryValue(TcpIpRegistryAdjuster.TcpipRegistrySubKey, TcpIpRegistryAdjuster.TcpipRegistryParameter_TcpTimedWaitDelay), Times.Once);
					registryAdapterMock.Verify(c => c.SetLocalMachineDwordRegistryValue(TcpIpRegistryAdjuster.TcpipRegistrySubKey, TcpIpRegistryAdjuster.TcpipRegistryParameter_TcpTimedWaitDelay, 30), Times.Once);
				}, "Correct value of TcpTimedWaitDelay was not read and/or set");

				registryAdapterMock!.VerifyNoOtherCalls();
			}

			[Test]
			public void TestWritesToRegistryOnlyWhenValueIsDifferent()
			{
				//Arrange
				registryAdapterMock
					!.Setup(rw => rw.ReadLocalMachineDwordRegistryValue(TcpIpRegistryAdjuster.TcpipRegistrySubKey, TcpIpRegistryAdjuster.TcpipRegistryParameter_MaxUserPort))
					.Returns(65534);

				registryAdapterMock
					.Setup(rw => rw.ReadLocalMachineDwordRegistryValue(TcpIpRegistryAdjuster.TcpipRegistrySubKey, TcpIpRegistryAdjuster.TcpipRegistryParameter_TcpTimedWaitDelay))
					.Returns(30);

				//Act
				tcpIpRegistryAdjuster!.TryAdjustIfRequired(loggerMock!.Object);

				//Assert
				registryAdapterMock.Verify(r => r.SetLocalMachineDwordRegistryValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
			}

			[Test]
			public void TestLogsWhenMaxUserPortIsAdjusted()
			{
				//Act
				tcpIpRegistryAdjuster!.TryAdjustIfRequired(loggerMock!.Object);

				//Assert
				loggerMock.Verify(l => l.Log(LogLevel.Information, "Tcp/Ip Registry Settings 'MaxUserPort' adjusted from [] to [65534]"), Times.Once);
			}

			[Test]
			public void TestLogsWhenTcpTimedWaitDelayIsAdjusted()
			{
				//Act
				tcpIpRegistryAdjuster!.TryAdjustIfRequired(loggerMock!.Object);

				//Assert
				loggerMock.Verify(l => l.Log(LogLevel.Information, "Tcp/Ip Registry Settings 'TcpTimedWaitDelay' adjusted from [] to [30]"), Times.Once);
			}

			[Test]
			public void TestLogsWhenMaxUserPortFailedToModify()
			{
				//Arrange
				var ex = new NotSupportedException("not lucky today");
				registryAdapterMock!.Setup(r => r.SetLocalMachineDwordRegistryValue(It.IsAny<string>(), TcpIpRegistryAdjuster.TcpipRegistryParameter_MaxUserPort, It.IsAny<int>())).Throws(ex);

				//Act
				tcpIpRegistryAdjuster!.TryAdjustIfRequired(loggerMock!.Object);

				//Assert
				loggerMock.Verify(l => l.Log(LogLevel.Warning, "Tcp/Ip Registry Settings 'MaxUserPort' is recommended to have the value [65534], but it cannot be adjusted: not lucky today"), Times.Once);
			}

			[Test]
			public void TestLogsWhenTcpTimedWaitDelayFailedToModify()
			{
				//Arrange
				var ex = new NotImplementedException("come back sunday");
				registryAdapterMock!.Setup(r => r.SetLocalMachineDwordRegistryValue(It.IsAny<string>(), TcpIpRegistryAdjuster.TcpipRegistryParameter_TcpTimedWaitDelay, It.IsAny<int>())).Throws(ex);

				//Act
				tcpIpRegistryAdjuster!.TryAdjustIfRequired(loggerMock!.Object);

				//Assert
				loggerMock.Verify(l => l.Log(LogLevel.Warning, "Tcp/Ip Registry Settings 'TcpTimedWaitDelay' is recommended to have the value [30], but it cannot be adjusted: come back sunday"), Times.Once);
			}

			[Test]
			public void TestMaxUserPortIsAdjustedEvenWhenTimedWaitDelayFails()
			{
				//Arrange
				registryAdapterMock
					!.Setup(r => r.SetLocalMachineDwordRegistryValue(It.IsAny<string>(), TcpIpRegistryAdjuster.TcpipRegistryParameter_TcpTimedWaitDelay, It.IsAny<int>()))
					.Throws(new Exception());

				//Act
				tcpIpRegistryAdjuster!.TryAdjustIfRequired(loggerMock!.Object);

				//Assert
				registryAdapterMock.Verify(r => r.SetLocalMachineDwordRegistryValue(It.IsAny<string>(), TcpIpRegistryAdjuster.TcpipRegistryParameter_TcpTimedWaitDelay, 30), Times.Once);
				registryAdapterMock.Verify(r => r.SetLocalMachineDwordRegistryValue(It.IsAny<string>(), TcpIpRegistryAdjuster.TcpipRegistryParameter_MaxUserPort, 65534), Times.Once);
			}

			[Test]
			public void TestTimedWaitDelayIsAdjustedEvenWhenMaxUserPortFails()
			{
				//Arrange
				registryAdapterMock
					!.Setup(r => r.SetLocalMachineDwordRegistryValue(It.IsAny<string>(), TcpIpRegistryAdjuster.TcpipRegistryParameter_MaxUserPort, It.IsAny<int>()))
					.Throws(new Exception());

				//Act
				tcpIpRegistryAdjuster!.TryAdjustIfRequired(loggerMock!.Object);

				//Assert
				registryAdapterMock.Verify(r => r.SetLocalMachineDwordRegistryValue(It.IsAny<string>(), TcpIpRegistryAdjuster.TcpipRegistryParameter_TcpTimedWaitDelay, 30), Times.Once);
				registryAdapterMock.Verify(r => r.SetLocalMachineDwordRegistryValue(It.IsAny<string>(), TcpIpRegistryAdjuster.TcpipRegistryParameter_MaxUserPort, 65534), Times.Once);
			}

			[Test]
			public void TestWrongParams()
			{
				var thrown = Assert.Throws<ArgumentNullException>(() => tcpIpRegistryAdjuster!.TryAdjustIfRequired(null));
				Assert.That(thrown?.ParamName, Is.EqualTo("logger"));
			}

			[SetUp]
			public void SetUp()
			{
				registryAdapterMock = new Mock<IWindowsRegistryAdapter>();
				tcpIpRegistryAdjuster = new TcpIpRegistryAdjuster(registryAdapterMock.Object);
				loggerMock = new Mock<IHostLogger>();
			}

			Mock<IWindowsRegistryAdapter>? registryAdapterMock;
			TcpIpRegistryAdjuster? tcpIpRegistryAdjuster;
			Mock<IHostLogger>? loggerMock;
		}

		[Test]
		public void TestWrongConstructorParams()
		{
			var thrown = Assert.Throws<ArgumentNullException>(() => new TcpIpRegistryAdjuster(null));
			Assert.That(thrown?.ParamName, Is.EqualTo("registryAdapter"));
		}
	}
}
