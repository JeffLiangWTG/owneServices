using CargoWise.Setup.Test.Helpers;
using NUnit.Framework;

namespace CargoWise.Setup.Test;

class ApplicationArgumentsTest
{
	[TestCase(new string[] { "" }, false)]
	[TestCase(new string[] { "--help" }, true)]
	[TestCase(new string[] { "--HELP" }, true)]
	[TestCase(new string[] { "install", "--components=asdf" }, false)]
	[TestCase(new string[] { "install", "--components=asdf", "--help" }, true)]
	[TestCase(new string[] { "-h" }, true)]
	[TestCase(new string[] { "-j" }, false)]
	public void TestHasHelpArgument(string[] args, bool expected)
	{
		Assert.That(ApplicationArgumentsReader.HasHelpArgument(args), Is.EqualTo(expected));
	}

	[Test]
	public void TestReadConfigWithConfigFile()
	{
		// Arrange
		using (GenerateDummyConfigFile("testConfig.json"))
		{
			var args = new[] { "install", "--configFile=testConfig.json", "--components=value456,thing123" };
			var expected = new PartialConfigurationModel()
			{
				Components = new[] { "value456", "thing123", "Environment" },
				InstallType = InstallType.Install
			}.WithDefaults();

			// Act
			var actual = ApplicationArgumentsReader.ReadConfigFromArgs(args);

			// Assert
			Assert.That(ConfigurationEquals(actual, expected));
		}
	}

	[Test]
	public void TestReadConfigWithConfigFileCaseSensitive()
	{
		// Arrange
		using (GenerateDummyConfigFile("testConfig.json"))
		{
			var args = new[] { "Install", "--ConfigFile=testConfig.json", "--Components=value456,thing123" };
			var expected = new PartialConfigurationModel()
			{
				Components = ["value456", "thing123", "Environment"],
				InstallType = InstallType.Install
			}.WithDefaults();

			// Act
			var actual = ApplicationArgumentsReader.ReadConfigFromArgs(args);

			// Assert
			Assert.That(ConfigurationEquals(actual, expected));
		}
	}

	[Test]
	public void TestReadConfigFailsOnMissingRequired()
	{
		// Arrange
		var args = new[] { "install" };
		var expected = new PartialConfigurationModel()
		{
			Components = new[] { "value456", "thing123" },
			InstallType = InstallType.Install
		}.WithDefaults();

		// Act
		var ex = Assert.Throws<BadUsageException>(() => ApplicationArgumentsReader.ReadConfigFromArgs(args));

		// Act
		Assert.That(ex?.Message, Is.EqualTo("Required argument 'Components' not provided"));
	}

	[Test]
	public void TestReadConfigFromArguments()
	{
		// Arrange
		var args = new[] { "install", "--components=singleValue" };
		var expected = new PartialConfigurationModel()
		{
			Components = new[] { "singleValue", "Environment" },
			InstallType = InstallType.Install
		}.WithDefaults();

		// Act
		var actual = ApplicationArgumentsReader.ReadConfigFromArgs(args);

		// Assert
		Assert.That(ConfigurationEquals(expected, actual));
	}

	[Test]
	public void TestReadConfigWithInvalidConfigFile()
	{
		// Arrange
		var args = new[] { "install", "--configFile=notARealFile.json" };

		// Act
		var ex = Assert.Throws<FileNotFoundException>(() => ApplicationArgumentsReader.ReadConfigFromArgs(args));

		// Assert
		Assert.That(ex?.Message, Does.StartWith("Could not find file ").And.EndsWith("notARealFile.json'."));
	}

	[Test]
	public void TestReadConfigWithInvalidArg()
	{
		// Arrange
		var args = new[] { "install", "--configfile" };

		// Act
		var ex = Assert.Throws<BadUsageException>(() => ApplicationArgumentsReader.ReadConfigFromArgs(args));

		// Assert
		Assert.That(ex?.Message, Is.EqualTo("Invalid argument format '--configfile'"));
	}

	[Test]
	public void TestReadConfigWithUnknown()
	{
		// Arrange
		var args = new[] { "install", "--zxcvzxcv=asdf" };

		// Act
		var ex = Assert.Throws<BadUsageException>(() => ApplicationArgumentsReader.ReadConfigFromArgs(args));

		// Assert
		Assert.That(ex?.Message, Is.EqualTo("Invalid argument name 'zxcvzxcv'"));
	}

	[Test]
	public void TestReadConfigNotUsingConfigFile()
	{
		// Arrange
		var args = new[] { "uninstall", "--components=abc,def" };
		var expected = new PartialConfigurationModel
		{
			InstallType = InstallType.Uninstall,
			Components = ["abc", "def"]
		}.WithDefaults();
		// Act
		var actual = ApplicationArgumentsReader.ReadConfigFromArgs(args);

		// Assert
		Assert.That(ConfigurationEquals(expected, actual));
	}

	[Test]
	public void TestReadConfigWithBadAction()
	{
		// Arrange
		var args = new[] { "wrong" };

		var ex = Assert.Throws<BadUsageException>(() => ApplicationArgumentsReader.ReadConfigFromArgs(args));

		// Assert
		Assert.That(ex?.Message, Is.EqualTo("Must specify install or uninstall"));
	}

	[Test]
	public void TestReadConfigWithoutAction()
	{
		// Arrange
		var args = Array.Empty<string>();

		var ex = Assert.Throws<BadUsageException>(() => ApplicationArgumentsReader.ReadConfigFromArgs(args));

		// Assert
		Assert.That(ex?.Message, Is.EqualTo("Must specify install or uninstall"));
	}

	ApplicationArguments ApplicationArgumentsReader => new ApplicationArguments(new ConfigurationDefaultsProvider());

	bool ConfigurationEquals(ConfigurationModel? x, ConfigurationModel? y) =>
		(x == null && y == null)
		|| (x != null && y != null
			&& x.InstallType == y.InstallType
			&& (x.Components == y.Components || (y.Components != null && (x.Components?.SequenceEqual(y.Components) ?? false)))
		);

	IDisposable GenerateDummyConfigFile(string name)
	{
		File.WriteAllText(name,
@"{
	""DbServer"": ""asdf"",
	""DbName"": ""asdf""
}
");

		return new DisposableAction(() => File.Delete(name));
	}

	public class DisposableAction(Action action) : IDisposable
	{
		public void Dispose() => action();
	}
}
