using System;
using CargoWise.NGenInstallerProgram;
using CargoWise.NGenInstallerProgram.Exceptions;
using NUnit.Framework;

namespace CargoWise.NGenInstaller.Testing
{
	public class CommandLineParametersTest : TestCase
	{
		public void TestParse_Successfully()
		{
			const string ActionInstall = "Install";
			const string ActionUninstall = "Uninstall";
			const string RootPath = "C:\\1.dll";
			const string ExecutionTimeout = "5";
			const string TestNGenPath = "C:\\NGen.exe";
			const string TestWaitTime = "100";

			CombineAssertions(() =>
			{
				Test(new[] { ActionInstall, RootPath }, NGenAction.Install, RootPath, null, 0, string.Empty);
				Test(new[] { ActionUninstall, RootPath }, NGenAction.Uninstall, RootPath, null, 0, string.Empty);
				Test(new[] { ActionUninstall, RootPath, string.Empty }, NGenAction.Uninstall, RootPath, null, 0, string.Empty);
				Test(new[] { ActionUninstall, RootPath, ExecutionTimeout }, NGenAction.Uninstall, RootPath, TimeSpan.FromSeconds(5), 0, string.Empty);
				Test(new[] { ActionInstall, RootPath, string.Empty, TestWaitTime }, NGenAction.Install, RootPath, null, 100, string.Empty);
				Test(new[] { ActionInstall, RootPath, string.Empty, TestWaitTime, TestNGenPath }, NGenAction.Install, RootPath, null, 100, TestNGenPath);
				Test(new[] { ActionInstall, RootPath, ExecutionTimeout, TestWaitTime, TestNGenPath }, NGenAction.Install, RootPath, TimeSpan.FromSeconds(5), 100, TestNGenPath);
			});

			void Test(string[] command, NGenAction expectedAction, string expectedRootPath, TimeSpan? expectedTimeout, int expectedDelay, string expectedNGenPath)
			{
				// Arrange
				// Action
				var parameters = CommandLineParameters.Parse(command);

				// Assert
				AssertEquals(expectedAction, parameters.Action);
				AssertEquals(expectedRootPath, parameters.RootFilePath);
				AssertEquals(TimeSpan.FromMilliseconds(expectedDelay), parameters.DelayTimeForTest);
				AssertEquals(expectedNGenPath, parameters.NGenPathForTest);
				AssertEquals(expectedTimeout, parameters.ExecutionTimeout);
			}
		}

		public void TestParse_Failed()
		{
			const string ActionInstall = "Install";
			const string ActionError = "Action";
			const string RootPath = "C:\\1.dll";

			CombineAssertions(() =>
			{
				Test(new string[] { ActionError, RootPath });
				Test(new string[] { ActionInstall, RootPath, "OneSecond" });
				Test(new string[] { ActionInstall, RootPath, "1.2" });
				Test(new string[] { ActionInstall, RootPath, "1.0" });
			});

			void Test(string[] command)
			{
				// Arrange
				// Action
				// Assert
				AssertExceptionThrown<ParameterParseException>(() => _ = CommandLineParameters.Parse(command));
			}
		}
	}
}
