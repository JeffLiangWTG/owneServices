using CargoWise.Setup.Test.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.Setup.Test;

class InstallDirectorTest
{
	[TestCase]
	public async Task TestInstallComponents()
	{
		// Arrange
		var config = new PartialConfigurationModel
		{
			Components = new[] { "Comp1", "coMp3" },
			InstallType = InstallType.Install
		}.WithDefaults();
		var dummyComponents = GetDummyComponents();
		var director = SetupDirector(dummyComponents.Select(x => x.Object).ToArray());
		var token = CancellationToken.None;

		// Act
		await director.Run(config, token);

		// Assert
		dummyComponents[0].Verify(x => x.Install(config, token), Times.Once());
		dummyComponents[1].Verify(x => x.Install(config, token), Times.Never());
		dummyComponents[2].Verify(x => x.Install(config, token), Times.Once());
	}

	[TestCase]
	public async Task TestInstallNothing()
	{
		// Arrange
		var config = new PartialConfigurationModel
		{
			Components = Array.Empty<string>(),
			InstallType = InstallType.Install
		}.WithDefaults();
		var dummyComponents = GetDummyComponents();
		var director = SetupDirector(dummyComponents.Select(x => x.Object).ToArray());
		var token = CancellationToken.None;

		// Act
		await director.Run(config, token);

		// Assert
		dummyComponents[0].Verify(x => x.Remove(config, token), Times.Never());
		dummyComponents[1].Verify(x => x.Remove(config, token), Times.Never());
		dummyComponents[2].Verify(x => x.Remove(config, token), Times.Never());
	}

	[TestCase]
	public async Task TestRemoveComponents()
	{
		// Arrange
		var config = new PartialConfigurationModel
		{
			Components = new[] { "Comp1", "Comp3" },
			InstallType = InstallType.Uninstall
		}.WithDefaults();
		var dummyComponents = GetDummyComponents();
		var director = SetupDirector(dummyComponents.Select(x => x.Object).ToArray());
		var token = CancellationToken.None;

		// Act
		await director.Run(config, token);

		// Assert
		dummyComponents[0].Verify(x => x.Remove(config, token), Times.Once());
		dummyComponents[1].Verify(x => x.Remove(config, token), Times.Never());
		dummyComponents[2].Verify(x => x.Remove(config, token), Times.Once());
	}

	[TestCase]
	public async Task TestCancellationBetweenTasks()
	{
		// Arrange
		var config = new PartialConfigurationModel
		{
			Components = new[] { "Comp1", "Comp3" },
			InstallType = InstallType.Install
		}.WithDefaults();
		var dummyComponents = GetDummyComponents();
		var loggerMock = new Mock<ILogger<IInstallDirector>>();
		var director = new InstallDirector(dummyComponents.Select(x => x.Object), loggerMock.Object);
		var tokenSource = new CancellationTokenSource();
		var token = tokenSource.Token;

		// Act
		tokenSource.Cancel();
		await director.Run(config, token);

		// Assert
		loggerMock.VerifyLog(LogLevel.Information, "Cancellation requested", Times.Once());
	}

	Mock<IInstallationComponent>[] GetDummyComponents()
	{
		return new[]
		{
			MockComponent("Comp1"),
			MockComponent("Comp2"),
			MockComponent("Comp3"),
		};
	}

	Mock<IInstallationComponent> MockComponent(string name)
	{
		var mock = new Mock<IInstallationComponent>();
		mock.SetupGet(x => x.Name).Returns(name);
		return mock;
	}

	InstallDirector SetupDirector(IReadOnlyCollection<IInstallationComponent> mockComponents)
	{
		var loggerMock = new Mock<ILogger<IInstallDirector>>();
		return new InstallDirector(mockComponents, loggerMock.Object);
	}
}
