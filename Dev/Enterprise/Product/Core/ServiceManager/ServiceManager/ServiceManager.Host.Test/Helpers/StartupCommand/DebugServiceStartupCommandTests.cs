using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.Common;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.StartupCommand
{
	public class DebugServiceStartupCommandTests
	{
		[Test]
		public void TestExecuteWithNullArgumentThrowsException()
		{
			// Arrange Act Assert
			var result = Assert.Throws<ArgumentNullException>(() => new ConsoleServiceStartupCommand(null));
			Assert.That(result.ParamName, Is.EqualTo("controllerService"));
		}

		[Test]
		public void TestExecuteWithSuccessfulRunReturnsZero()
		{
			//Arrange
			var serviceNameProvider = new Mock<IServiceNameProvider>();
			serviceNameProvider
				.Setup(x => x.GetServiceName())
				.Returns("ServiceName");

			var previousIn = Console.In;
			using var controllerService = new ControllerService(new Lazy<IServiceManagerApplication>(() => Mock.Of<IServiceManagerApplication>()),
				Mock.Of<IHostLogger>(),
				Mock.Of<IApplicationEmergencyExit>(),
				serviceNameProvider.Object,
				Mock.Of<ICancellationRequester>(),
				Mock.Of<ICancellationTokenProvider>());

			using (new DisposableAction(() => Console.SetIn(previousIn)))
			using (var memoryStream = new MemoryStream())
			using (var streamWriter = new StreamWriter(memoryStream))
			using (var streamReader = new StreamReader(memoryStream))
			{
				Console.SetIn(streamReader);

				var command = new ConsoleServiceStartupCommand(controllerService);

				// Act
				var task = Task.Run(() => command.Execute());

				task.Wait();

				// Assert
				Assert.That(task.Result, Is.EqualTo(0));
			}
		}
	}
}
