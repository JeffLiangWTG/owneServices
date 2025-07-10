using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.StartupCommand
{
	class HostStartupCommandResolverTests : TestCase
	{
		public void TestWrongConstructorParams()
		{
			// Arrange Act Assert
			var exception = AssertExceptionThrown<ArgumentNullException>(() => new HostStartupCommandResolver(serviceHostProvider.Object, null));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("hostOptions"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new HostStartupCommandResolver(null, Mock.Of<IServiceManagerHostOptions>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("serviceProvider"));
		}

		public void TestResolveWithUnregisteredcommandThrowsException()
		{
			// Arrange
			var args = new HostCommandLineArgsParser("ServerName", "DatabaseName");
			var resolver = CreateStartupCommandResolver(args);

			// Act
			AssertExceptionThrown<InvalidOperationException>(() => resolver.Resolve());
		}

		[ExpectNoExceptions]
		public void TestResolveWithEmptyArgsReturnsRunControllerStartupCommand()
		{
			// Arrange
			var expectedStartegy = Mock.Of<IHostStartupCommand>();
			serviceHostProvider
				.Setup(o => o.GetService(typeof(RunControllerStartupCommand)))
				.Returns(expectedStartegy);

			var args = new HostCommandLineArgsParser("ServerName", "DatabaseName");
			var resolver = CreateStartupCommandResolver(args);

			// Act
			var command = resolver.Resolve();

			// Assert
			NUnit.Framework.Assert.That(command, Is.EqualTo(expectedStartegy));
		}

		public void TestResolveWithNullArgsThrowsException()
		{
			// Arrange Act Assert
			AssertExceptionThrown<ArgumentNullException>(() => CreateStartupCommandResolver(null));
		}

		[ExpectNoExceptions]
		public void TestResolveReturnsCorrectType()
		{
			// Arrange
			var optionTypeMapping = new Dictionary<string, Type>
			{
				{ HostCommandLineOptions.OptionConsole, typeof(ConsoleServiceStartupCommand) },
				{ HostCommandLineOptions.OptionInstall, typeof(InstallServiceStartupCommand) },
				{ HostCommandLineOptions.OptionUninstall, typeof(UninstallServiceStartupCommand) },
				{ HostCommandLineOptions.OptionStart, typeof(StartServiceStartupCommand) },
				{ HostCommandLineOptions.OptionStop, typeof(StopServiceStartupCommand) },
			};

			NUnit.Framework.Assert.Multiple(() =>
			{
				foreach (var mapping in optionTypeMapping)
				{
					// Arrange
					var expectedStartegy = Mock.Of<IHostStartupCommand>();
					var serviceHostProvider = new Mock<IServiceProvider>();

					serviceHostProvider.Setup(o => o.GetService(mapping.Value))
						.Returns(expectedStartegy);

					var args = new HostCommandLineArgsParser(new[] { mapping.Key, "ServerName", "DatabaseName" });
					var resolver = new HostStartupCommandResolver(serviceHostProvider.Object, args);

					// Act
					var command = resolver.Resolve();

					// Assert
					NUnit.Framework.Assert.That(command, Is.EqualTo(expectedStartegy));
				}
			});
		}

		protected override void SetUp()
		{
			serviceHostProvider = new Mock<IServiceProvider>();
		}

		HostStartupCommandResolver CreateStartupCommandResolver(IServiceManagerHostOptions hostOptions)
		{
			var resolver = new HostStartupCommandResolver(serviceHostProvider.Object, hostOptions);

			return resolver;
		}

		Mock<IServiceProvider> serviceHostProvider;
	}
}
