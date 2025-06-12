using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Serilog;

namespace CargoWise.eHub.Products.TWCustoms.Tests
{
	public class PluginManagerTests
	{
		private Mock<PluginManager> pluginManager;
		private Mock<IConfiguration> configurationMock;
		private ManualResetEvent waitEvent;

		private const string ConfigFilePathOne = nameof(ConfigFilePathOne);
		private const string ConfigFilePathTwo = nameof(ConfigFilePathTwo);
		private const string SemaphoreWaitMs = "3000";

		[SetUp]
		public void SetUp()
		{
			var fileManagerMock = new Mock<IFileManager>();
			var loggerMock = new Mock<ILogger>();

			var section = new Mock<IConfigurationSection>();
			section.SetupGet(x => x.Value).Returns(SemaphoreWaitMs);
			configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x.GetSection("SemaphoreWaitMs")).Returns(section.Object);

			waitEvent = new ManualResetEvent(false);
			pluginManager = new Mock<PluginManager>(fileManagerMock.Object, loggerMock.Object) { CallBase = true };
			pluginManager.Setup(x => x.StartProcess(It.IsAny<PluginInfo>()))
				.Returns<PluginInfo>(_ =>
				{
					waitEvent.WaitOne();
					return string.Empty;
				});
		}

		[TearDown]
		public void TearDown()
		{
			if (waitEvent == null) return;
			waitEvent.Set();
			waitEvent.Dispose();
		}

		[Test]
		public async Task TestExecutePluginHandler_SameConfigFile_SecondThreadCanEntryAfterRelease()
		{
			var pluginOne = new PluginInfo { ConfigFilePath = ConfigFilePathOne };

			pluginManager.Setup(x => x.StartProcess(It.IsAny<PluginInfo>()))
				.Returns<PluginInfo>(_ => string.Empty);

			await Task.Run(() => pluginManager.Object.ExecutePluginHandler(pluginOne, configurationMock.Object));
			pluginManager.Verify(x => x.StartProcess(It.IsAny<PluginInfo>()), Times.Once);

			await Task.Run(() => pluginManager.Object.ExecutePluginHandler(pluginOne, configurationMock.Object));
			pluginManager.Verify(x => x.StartProcess(It.IsAny<PluginInfo>()), Times.Exactly(2));
		}

		[Test]
		public async Task TestExecutePluginHandler_SameConfigFile_SecondThreadBlockedAndTimeout()
		{
			var pluginOne = new PluginInfo { ConfigFilePath = ConfigFilePathOne };

			var task = await Task.WhenAny(
					Task.Run(() => pluginManager.Object.ExecutePluginHandler(pluginOne, configurationMock.Object)),
					Task.Run(() => pluginManager.Object.ExecutePluginHandler(pluginOne, configurationMock.Object))
					);

			Assert.That(task.IsCompleted, Is.True);
			Assert.That(task.Result.ErrorCode, Is.EqualTo("E0030"));
			Assert.That(task.Result.ErrorMessge, Is.EqualTo($"Timeout when attempt to obtain semaphore for config file: {ConfigFilePathOne}."));
			pluginManager.Verify(x => x.StartProcess(It.IsAny<PluginInfo>()), Times.Exactly(1));
		}

		[Test]
		public void TestExecutePluginHandler_SameConfigFile_ExceptionShouldReleaseSemaphore()
		{
			var threwException = false;
			var pluginOne = new PluginInfo { ConfigFilePath = ConfigFilePathOne };
			var exception = new Exception("Test exception");

			pluginManager.Setup(x => x.StartProcess(It.IsAny<PluginInfo>()))
				.Returns<PluginInfo>(_ =>
				{
					if (threwException) return string.Empty;

					threwException = true;
					throw exception;
				});


			var ex = Assert.ThrowsAsync<Exception>(
				() => Task.WhenAll(
					Task.Run(() => pluginManager.Object.ExecutePluginHandler(pluginOne, configurationMock.Object)),
					Task.Run(() => pluginManager.Object.ExecutePluginHandler(pluginOne, configurationMock.Object))
					)
				);
			Assert.That(exception, Is.EqualTo(ex));
			pluginManager.Verify(x => x.StartProcess(It.IsAny<PluginInfo>()), Times.Exactly(2));
		}

		[Test]
		public async Task TestExecutePluginHandler_DifferentConfigFile_NotBlocked()
		{
			var pluginOne = new PluginInfo { ConfigFilePath = ConfigFilePathOne };
			var pluginTwo = new PluginInfo { ConfigFilePath = ConfigFilePathTwo };
			var callCount = 0;

			pluginManager.Setup(x => x.StartProcess(It.IsAny<PluginInfo>()))
				.Callback(() => callCount++)
				.Returns<PluginInfo>(_ =>
				{
					waitEvent.WaitOne();
					return string.Empty;
				});

			var tasks = Task.WhenAll
			(
				Task.Run(() => pluginManager.Object.ExecutePluginHandler(pluginOne, configurationMock.Object)),
				Task.Run(() => pluginManager.Object.ExecutePluginHandler(pluginTwo, configurationMock.Object))
			);

			var isComplete = SpinWait.SpinUntil(() =>
			{
				Thread.Sleep(100);
				return callCount == 2;
			}, TimeSpan.FromMilliseconds(int.Parse(SemaphoreWaitMs) + 100));

			Assert.That(isComplete, Is.True, "Timeout when wait both task to complete");
			pluginManager.Verify(x => x.StartProcess(It.IsAny<PluginInfo>()), Times.Exactly(2));

			waitEvent.Set();
			await tasks;
		}
	}
}
