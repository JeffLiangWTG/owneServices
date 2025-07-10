using System;
using System.Threading;
using CargoWise.Loader.Common;
using Enterprise.Upgrades.UnitTestHelper;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Upgrades.Preinstall.Testing
{
	class ProgramTest
	{
		[Test]
		public void TestErrorsAreShownOnInstallationResultsForm()
		{
			// Arrange
			const string windowTitle = "Installation Results";
			var programMock = new Mock<TestProgram>() { CallBase = true };
			var program = programMock.Object;

			programMock.Protected()
				.Setup("AddDependencies")
				.Callback(() =>
				{
					var mockedInstallationItem = new Mock<InstallationItem>(program.TopLevelItem.Installation) { CallBase = true };
					mockedInstallationItem.Protected()
						.Setup<bool>("NeedsToInstallCore")
						.Returns(true);
					mockedInstallationItem.Protected()
						.Setup<InstallationResult>("InstallExcludingDependencies")
						.Returns(InstallationResult.Error(nameof(TestErrorsAreShownOnInstallationResultsForm)));

					program.TopLevelItem.AddDependency(mockedInstallationItem.Object);
				});

			var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
			var thread = new Thread(() => Win32Native.FindInstallationResultsFormAndClose(windowTitle, cancellationTokenSource.Token));
			thread.Start();

			// Act
			// Assert
			var exitCode = int.MaxValue;
			Assert.DoesNotThrow(() =>
			{
				exitCode = program.StartApplication(new[] { @"TempPath\10.8.10.0", "-NoUI" });
			});

			thread.Join();
			Assert.That(exitCode, Is.EqualTo(-1));
		}

		[Test]
		public void TestInitializeParseCommandLineArgs()
		{
			// [TestCaseSource] should not be used here as the calculation should be done at test runtime
			TestInitializeParseCommandLineArgsCore(new[] { "TempPath" }, Environment.UserInteractive ? UILevel.Normal : UILevel.AutomatedWithNoUI);
			TestInitializeParseCommandLineArgsCore(new[] { "TempPath", "-NoUI" }, UILevel.AutomatedWithNoUI);
		}

		void TestInitializeParseCommandLineArgsCore(string[] args, UILevel expectedUILevel)
		{
			// Arrange
			var program = new TestProgram();

			// Act
			program.Initialize(args);

			// Assert
			var config = program.GetConfiguration();
			Assert.That(config.CurrentPackage, Is.EqualTo(args[0]));
			Assert.That(config.UILevel, Is.EqualTo(expectedUILevel), $"Arguments: {string.Join(" ", args)}");
		}

		[Test]
		public void TestInitializeWrongParamsCall()
		{
			// Arrange
			var program = new TestProgram();

			// Act
			var argumentException = Assert.Throws<ArgumentException>(() => { program.Initialize(null); });

			// Assert
			Assert.That(argumentException.ParamName, Is.EqualTo("args"));
			Assert.That(
				argumentException,
				Has.Message.StartsWith("Enterprise.Upgrades.Preinstall command line args must contain at least the package directory."));
		}
	}

	class TestProgram : Program
	{
		internal Configuration GetConfiguration() => Configuration;
	}
}
