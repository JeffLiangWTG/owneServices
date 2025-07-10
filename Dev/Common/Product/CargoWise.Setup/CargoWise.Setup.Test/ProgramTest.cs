using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;

namespace CargoWise.Setup.Test;

public class ProgramTests
{
	[Test]
	public async Task TestHelpMessage()
	{
		// Arrange
		var program = new Program();
		using var consoleMock = new ConsoleMock();

		// Act
		var exitCode = await program.Run(["--help"]);

		// Assert
		Assert.That(consoleMock.Output, Does.Contain(Program.UsageMessage));
		Assert.That(consoleMock.Output, Does.Contain("Available Components:\n\tEnvironment"));
		Assert.That(consoleMock.Error, Is.Empty);
		Assert.That(exitCode, Is.EqualTo((int)Program.ExitCodes.Success));
	}

	[Test]
	public async Task TestHandlesBadUsageException()
	{
		// Arrange
		var program = new Program();
		var mockInstallDirector = new Mock<IInstallDirector>();
		mockInstallDirector.Setup(d => d.Run(It.IsAny<ConfigurationModel>(), It.IsAny<CancellationToken>())).Throws(new BadUsageException("Test message"));
		ReplaceInstallDirector(program, mockInstallDirector);
		using var consoleMock = new ConsoleMock();

		// Act
		var exitCode = await program.Run(minimumArguments);

		// Assert
		var expectedUsageMessage = "Test message\nUsage ./CargoWise.Setup.exe [install|uninstall]  [--configFile=<configFile>] [--<configKey>=<value>]* --components=<component1>,<component2> [--help]";
		Assert.That(consoleMock.Error, Does.Contain(expectedUsageMessage));
		Assert.That(exitCode, Is.EqualTo((int)Program.ExitCodes.BadUsage));
	}

	[Test]
	public async Task TestHandlesOtherException()
	{
		// Arrange
		var program = new Program();
		var mockInstallDirector = new Mock<IInstallDirector>();
		mockInstallDirector.Setup(d => d.Run(It.IsAny<ConfigurationModel>(), It.IsAny<CancellationToken>())).Throws(new InvalidOperationException("Other message", GetPopulatedException()));
		ReplaceInstallDirector(program, mockInstallDirector);
		using var consoleMock = new ConsoleMock();

		// Act
		var exitCode = await program.Run(minimumArguments);

		// Assert
		var expectedErrorMessage = "System.InvalidOperationException: Other message";
		var expectedInnerExceptionMessage = "System.ArgumentException: arggggg im a pirate";
		var errorOutput = consoleMock.Error;
		Assert.That(errorOutput, Does.Contain(expectedErrorMessage));
		Assert.That(errorOutput, Does.Contain(expectedInnerExceptionMessage));
		Assert.That(exitCode, Is.EqualTo((int)Program.ExitCodes.Error));

		Exception GetPopulatedException()
		{
			try
			{
				// must throw to get a stack trace
				throw new ArgumentException("arggggg im a pirate");
			}
			catch (ArgumentException ex)
			{
				return ex;
			}
		}
	}

	readonly string[] minimumArguments = ["install", "--components=abc"];

	void ReplaceInstallDirector(Program program, Mock<IInstallDirector> mockInstallDirector)
	{
		var serviceCollection = new ServiceCollection();
		Program.ConfigureServices(serviceCollection);
		serviceCollection.RemoveAll<IInstallDirector>();
		serviceCollection.AddSingleton(mockInstallDirector.Object);
		program.ServiceProvider = serviceCollection.BuildServiceProvider();
	}

	[Test]
	public void TestAllServicesCanResolve()
	{
		var serviceCollection = new ServiceCollection();
		Program.ConfigureServices(serviceCollection);
		var serviceProvider = serviceCollection.BuildServiceProvider();

		Assert.Multiple(() =>
		{
			foreach (var service in serviceCollection)
			{
				if (!service.ServiceType.ContainsGenericParameters)
				{
					try
					{
						serviceProvider.GetService(service.ServiceType);
					}
					catch (Exception ex)
					{
						Assert.Fail($"Failed to resolve service {service.ServiceType} - '{ex.Message}'");
					}
				}
			}
		});
	}
}
