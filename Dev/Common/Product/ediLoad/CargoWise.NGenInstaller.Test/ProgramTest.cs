using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.Client.Common.Testing;
using Enterprise.Upgrades;
using NUnit.Framework;
using WTG.ApplicationLogging.Abstractions;

namespace CargoWise.NGenInstaller.Testing
{
	public class ProgramTest : TestCase
	{
		[TestRequiresAdministrativePrivileges("Run NGen.exe")]
		public abstract class LogsTestBase : TestCase
		{
			public abstract class ApplicationLoggingTest : LogsTestBase
			{
				public class ApplicationLogging64Test : ApplicationLoggingTest
				{
					protected override int ExpectedBitVersion => 64;
				}

				public class ApplicationLogging32Test : ApplicationLoggingTest
				{
					protected override int ExpectedBitVersion => 32;
				}

				protected override void SetUp()
				{
					loggingHelper.CleanUpLogs();

					base.SetUp();
				}

				protected override void TearDown()
				{
					base.TearDown();

					loggingHelper.CleanUpLogs();
				}

				protected override IEnumerable<(string level, string message)> GetRealLogInfos()
				{
					return loggingHelper
						.GetLogEntries()
						.Select(o =>
						{
							var logEntry = JsonNode.Parse(o);

							return (logEntry["level"].GetValue<string>(), logEntry["message"].GetValue<string>());
						})
						.ToArray();
				}
			}

			readonly ProgramTest programTest = new ProgramTest();
			readonly LoggingTestHelper loggingHelper = new(Product.CargoWise, "CargoWise.NGenInstaller");
			const string ActionForInstalling = "Install";
			const string ActionForUninstalling = "Uninstall";
			const string ActionForError = "ABC";
			IDisposable disposable;
			static string NGenVersionFor32 = string.Empty;
			static string NGenVersionFor64 = string.Empty;
			
			string CurrentNGenVersion
			{
				get => ExpectedBitVersion == 32 ? NGenVersionFor32 : NGenVersionFor64;
			}

			protected override void MasterSetUp()
			{
				NGenVersionFor32 = GetNGenVersion(true);
				NGenVersionFor64 = GetNGenVersion(false);
				base.MasterSetUp();
			}

			string GetNGenVersion(bool for32BitVersion)
			{
				Regex regex = new Regex(@"Version (?<version>[\d\.]*)");
				var path = Path.Combine(
					Directory.GetParent(Environment.SystemDirectory).FullName,
					string.Format(
						CultureInfo.InvariantCulture,
						@"Microsoft.NET\Framework{0}\v4.0.30319\NGen.exe", (for32BitVersion ? "" : "64")));
				var process = Process.Start(new ProcessStartInfo(path)
				{
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				});
				process.WaitForExit();
				string output = process.StandardOutput.ReadToEnd();
				return regex.Match(output).Groups["version"].Value;
			}

			protected override void SetUp()
			{
				programTest.VersionString = ExpectedBitVersion == 32
					? VersionStringFor32
					: (ExpectedBitVersion == 64
						? VersionStringFor64
						: throw new NotSupportedException($"Not Supported {nameof(ExpectedBitVersion)} : {ExpectedBitVersion}"));

				programTest.SetUp();
				disposable = programTest.CreateFilesThenDeleteOnDispose();
			}

			protected override void TearDown()
			{
				disposable.Dispose();
				programTest?.TearDown();
			}

			public void TestLogsAllSuccess_WhenInstall()
			{
				// Arrange
				var startInfo = GetProcessStartInfo(ActionForInstalling, programTest.dummyAssemblyName);

				// Act
				RunProcess(startInfo);

				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Info", $"Performing {ActionForInstalling} task"),
						("Info", "Using 64 bit version"),
						("Info", @"Waiting for mutex Global\CargoWiseOneNGen..."),
						("Info", @"Mutex Global\CargoWiseOneNGen obtained"),
						("Info", @"Using NGen.exe located: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\NGen.exe"),
						("Info", "Executing command [queue pause]..."),
						("Info", "Command executed."),
						("Warn", $"No valid exe file exists in this folder - {programTest.assemblyPathInTemp}"),
						("Info", $"Executing command [{ActionForInstalling} \"{programTest.dummyAssemblyName}\"]..."),
						("Info", "Command executed."),
						("Info", "Executing command [queue continue]..."),
						("Info", "Command executed."),
						("Info", "Finished CargoWise.NGenInstaller.exe"),
					},
					GetRealLogInfos());
			}

			public void TestLogsAllSuccess_WhenInstallWithExe()
			{
				// Arrange
				var startInfo = GetProcessStartInfo(ActionForInstalling, programTest.dummyAssemblyName);
				var exeFileName = Path.Combine(programTest.assemblyPathInTemp, ExeFileNames.CargoWiseWindowsDesktopExe);
				File.Copy(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ExeFileNames.CargoWiseWindowsDesktopExe), exeFileName);

				// Act
				RunProcess(startInfo);

				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Info", $"Performing {ActionForInstalling} task"),
						("Info", "Using 64 bit version"),
						("Info", @"Waiting for mutex Global\CargoWiseOneNGen..."),
						("Info", @"Mutex Global\CargoWiseOneNGen obtained"),
						("Info", @"Using NGen.exe located: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\NGen.exe"),
						("Info", "Executing command [queue pause]..."),
						("Info", "Command executed."),
						("Info", $"Executing command [{ActionForInstalling} \"{programTest.dummyAssemblyName}\" /ExeConfig:\"{exeFileName}\"]..."),
						("Info", "Command executed."),
						("Info", "Executing command [queue continue]..."),
						("Info", "Command executed."),
						("Info", "Finished CargoWise.NGenInstaller.exe"),
					},
					GetRealLogInfos());
			}

			public void TestLogsAllSuccess_WhenUninstall()
			{
				// Arrange
				var path = $@"Microsoft.NET\Framework64\v4.0.30319\NGen.exe";
				var startInfo = new ProcessStartInfo(Path.Combine(Directory.GetParent(Environment.SystemDirectory).FullName, path))
				{
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					Arguments = "Install " + programTest.dummyAssemblyName,
				};
				RunProcess(startInfo);
				startInfo = GetProcessStartInfo(ActionForUninstalling, programTest.dummyAssemblyName);

				// Act
				RunProcess(startInfo);

				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Info", $"Performing {ActionForUninstalling} task"),
						("Info", $"Using 64 bit version"),
						("Info", @"Waiting for mutex Global\CargoWiseOneNGen..."),
						("Info", @"Mutex Global\CargoWiseOneNGen obtained"),
						("Info", $@"Using NGen.exe located: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\NGen.exe"),
						("Info", "Executing command [queue pause]..."),
						("Info", "Command executed."),
						("Info", $"Executing command [{ActionForUninstalling} \"{programTest.dummyAssemblyName}\"]..."),
						("Info", "Command executed."),
						("Info", "Executing command [queue continue]..."),
						("Info", "Command executed."),
						("Info", "Finished CargoWise.NGenInstaller.exe"),
					},
					GetRealLogInfos());
			}

			public void TestLogsEndWithParsingError()
			{
				// Arrange
				var startInfo = GetProcessStartInfo(ActionForError, programTest.dummyAssemblyName);

				// Act
				RunProcess(startInfo);

				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Error", "Failed to parse command line parameters. Should be: CargoWise.NGenInstaller.exe RootFilePath (Install | Uninstall) [ExecutionTimeoutSeconds]"),
					},
					GetRealLogInfos());
			}

			public void TestLogsEndWithQueuePauseError_WhenInstall()
			{
				// Arrange
				var startInfo = GetProcessStartInfo(ActionForInstalling, programTest.dummyAssemblyName, nGenPath: "C:\\NGen.exe");

				// Act
				RunProcess(startInfo);

				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Info", $"Performing {ActionForInstalling} task"),
						("Info", "Using 64 bit version"),
						("Info", @"Waiting for mutex Global\CargoWiseOneNGen..."),
						("Info", @"Mutex Global\CargoWiseOneNGen obtained"),
						("Info", @"Using NGen.exe located: C:\NGen.exe"),
						("Info", "Executing command [queue pause]..."),
						("Error", "The system cannot find the file specified"),
					},
					GetRealLogInfos());
			}

			public void TestLogsEndWithQueuePauseError_WhenUninstall()
			{
				// Arrange
				var startInfo = GetProcessStartInfo(ActionForUninstalling, programTest.dummyAssemblyName, nGenPath: "C:\\NGen.exe");

				// Act
				RunProcess(startInfo);

				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Info", $"Performing {ActionForUninstalling} task"),
						("Info", $"Using 64 bit version"),
						("Info", @"Waiting for mutex Global\CargoWiseOneNGen..."),
						("Info", @"Mutex Global\CargoWiseOneNGen obtained"),
						("Info", @"Using NGen.exe located: C:\NGen.exe"),
						("Info", "Executing command [queue pause]..."),
						("Error", "The system cannot find the file specified"),
					},
					GetRealLogInfos());
			}

			public void TestLogsEndWithInstallError_WhenInstall()
			{
				// Arrange
				var startInfo = GetProcessStartInfo(ActionForInstalling, programTest.incorrectDllFileName);

				// Act
				RunProcess(startInfo);

				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Info", $"Performing {ActionForInstalling} task"),
						("Info", "Using 64 bit version"),
						("Info", @"Waiting for mutex Global\CargoWiseOneNGen..."),
						("Info", @"Mutex Global\CargoWiseOneNGen obtained"),
						("Info", @"Using NGen.exe located: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\NGen.exe"),
						("Info", "Executing command [queue pause]..."),
						("Info", "Command executed."),
						("Warn", $"No valid exe file exists in this folder - {programTest.assemblyPathInTemp}"),
						("Info", $"Executing command [{ActionForInstalling} \"{programTest.incorrectDllFileName}\"]..."),
						("Info", "Executing command [queue continue]..."),
						("Info", "Command executed."),
						("Error", $"Process finished with error: Process exit code -1 Arguments: \"Install \"{programTest.incorrectDllFileName}\"\" Output: \"Microsoft (R) CLR Native Image Generator - Version {CurrentNGenVersion}\r\nCopyright (c) Microsoft Corporation.  All rights reserved.\r\nUninstalling assembly {programTest.incorrectDllFileName} because of an error during compilation: Failed to load the runtime. (Exception from HRESULT: 0x80131700).\r\nFailed to load the runtime. (Exception from HRESULT: 0x80131700)\r\n\""),
					},
					GetRealLogInfos());
			}

			public void TestLogsEndWithInstallError_WhenInstallWithExe()
			{
				// Arrange
				var startInfo = GetProcessStartInfo(ActionForInstalling, programTest.incorrectDllFileName);
				var exeFileName = Path.Combine(programTest.assemblyPathInTemp, "CargoWiseOne.exe");
				File.Copy(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CargoWiseOne.exe"), exeFileName);

				// Act
				RunProcess(startInfo);

				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Info", $"Performing {ActionForInstalling} task"),
						("Info", "Using 64 bit version"),
						("Info", @"Waiting for mutex Global\CargoWiseOneNGen..."),
						("Info", @"Mutex Global\CargoWiseOneNGen obtained"),
						("Info", @"Using NGen.exe located: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\NGen.exe"),
						("Info", "Executing command [queue pause]..."),
						("Info", "Command executed."),
						("Info", $"Executing command [{ActionForInstalling} \"{programTest.incorrectDllFileName}\" /ExeConfig:\"{exeFileName}\"]..."),
						("Info", "Executing command [queue continue]..."),
						("Info", "Command executed."),
						("Error", $"Process finished with error: Process exit code -1 Arguments: \"Install \"{programTest.incorrectDllFileName}\" /ExeConfig:\"{exeFileName}\"\" Output: \"Microsoft (R) CLR Native Image Generator - Version {CurrentNGenVersion}\r\nCopyright (c) Microsoft Corporation.  All rights reserved.\r\nFailed to compile {programTest.incorrectDllFileName} because of the following error:  is not a valid Win32 application. (Exception from HRESULT: 0x800700C1).\r\nUninstalling assembly {programTest.incorrectDllFileName} because of an error during compilation: Failed to compile {programTest.incorrectDllFileName} because of the following error:  is not a valid Win32 application. (Exception from HRESULT: 0x800700C1).\r\n is not a valid Win32 application. (Exception from HRESULT: 0x800700C1)\r\n\""),
					},
					GetRealLogInfos());
			}

			public void TestLogsEndWithInstallError_WhenUninstall()
			{
				// Arrange
				var startInfo = GetProcessStartInfo(ActionForUninstalling, programTest.dummyAssemblyName);

				// Act
				RunProcess(startInfo);
				var x = GetRealLogInfos();
				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Info", $"Performing {ActionForUninstalling} task"),
						("Info", $"Using 64 bit version"),
						("Info", @"Waiting for mutex Global\CargoWiseOneNGen..."),
						("Info", @"Mutex Global\CargoWiseOneNGen obtained"),
						("Info", $@"Using NGen.exe located: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\NGen.exe"),
						("Info", "Executing command [queue pause]..."),
						("Info", "Command executed."),
						("Info", $"Executing command [{ActionForUninstalling} \"{programTest.dummyAssemblyName}\"]..."),
						("Info", "Executing command [queue continue]..."),
						("Info", "Command executed."),
						("Error", $"Process finished with error: Process exit code -1 Arguments: \"Uninstall \"{programTest.dummyAssemblyName}\"\" Output: \"Microsoft (R) CLR Native Image Generator - Version {CurrentNGenVersion}\r\nCopyright (c) Microsoft Corporation.  All rights reserved.\r\nError: The specified assembly is not installed.\r\n\""),
					},
					GetRealLogInfos());
			}

			public void TestLogsEndWithQueueContinueError_WhenInstall()
			{
				// Arrange
				string batPath = Path.Combine(programTest.tempDir.DirectoryName, "Fake.bat");
				File.WriteAllText(batPath, @"@echo off
echo it is a fake program
if ""%~2"" == ""continue"" ( 
  exit -1
) else (
  exit 0
)");
				var startInfo = GetProcessStartInfo(ActionForInstalling, programTest.dummyAssemblyName, nGenPath: batPath);

				// Act
				RunProcess(startInfo);

				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Info", $"Performing {ActionForInstalling} task"),
						("Info", "Using 64 bit version"),
						("Info", @"Waiting for mutex Global\CargoWiseOneNGen..."),
						("Info", @"Mutex Global\CargoWiseOneNGen obtained"),
						("Info", $@"Using NGen.exe located: {batPath}"),
						("Info", "Executing command [queue pause]..."),
						("Info", "Command executed."),
						("Warn", $"No valid exe file exists in this folder - {programTest.assemblyPathInTemp}"),
						("Info", $"Executing command [{ActionForInstalling} \"{programTest.dummyAssemblyName}\"]..."),
						("Info", "Command executed."),
						("Info", "Executing command [queue continue]..."),
						("Error", "Process finished with error: Process exit code -1 Arguments: \"queue continue\" Output: \"it is a fake program\r\n\""),
					},
					GetRealLogInfos());
			}

			public void TestLogsEndWithQueueContinueError_WhenInstallWithExe()
			{
				// Arrange
				string batPath = Path.Combine(programTest.tempDir.DirectoryName, "Fake.bat");
				File.WriteAllText(batPath, @"@echo off
echo it is a fake program
if ""%~2"" == ""continue"" ( 
  exit -1
) else (
  exit 0
)");
				var startInfo = GetProcessStartInfo(ActionForInstalling, programTest.dummyAssemblyName, nGenPath: batPath);
				var exeFileName = Path.Combine(programTest.assemblyPathInTemp, ExeFileNames.CargoWiseWindowsDesktopExe);
				File.Copy(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ExeFileNames.CargoWiseWindowsDesktopExe), exeFileName);

				// Act
				RunProcess(startInfo);

				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Info", $"Performing {ActionForInstalling} task"),
						("Info", "Using 64 bit version"),
						("Info", @"Waiting for mutex Global\CargoWiseOneNGen..."),
						("Info", @"Mutex Global\CargoWiseOneNGen obtained"),
						("Info", $@"Using NGen.exe located: {batPath}"),
						("Info", "Executing command [queue pause]..."),
						("Info", "Command executed."),
						("Info", $"Executing command [{ActionForInstalling} \"{programTest.dummyAssemblyName}\" /ExeConfig:\"{exeFileName}\"]..."),
						("Info", "Command executed."),
						("Info", "Executing command [queue continue]..."),
						("Error", "Process finished with error: Process exit code -1 Arguments: \"queue continue\" Output: \"it is a fake program\r\n\""),
					},
					GetRealLogInfos());
			}

			public void TestLogsEndWithQueueContinueError_WhenUninstall()
			{
				// Arrange
				string batPath = Path.Combine(programTest.tempDir.DirectoryName, "Fake.bat");
				File.WriteAllText(batPath, @"@echo off
echo it is a fake program
if ""%~2"" == ""continue"" ( 
  exit -1
) else (
  exit 0
)");
				var startInfo = GetProcessStartInfo(ActionForUninstalling, programTest.dummyAssemblyName, nGenPath: batPath);

				// Act
				RunProcess(startInfo);

				// Assertion
				AssertContainsExactElementsInExactOrder(
					new (string level, string message)[]
					{
						("Info", "Starting CargoWise.NGenInstaller.exe"),
						("Info", $"Performing {ActionForUninstalling} task"),
						("Info", $"Using 64 bit version"),
						("Info", @"Waiting for mutex Global\CargoWiseOneNGen..."),
						("Info", @"Mutex Global\CargoWiseOneNGen obtained"),
						("Info", $@"Using NGen.exe located: {batPath}"),
						("Info", "Executing command [queue pause]..."),
						("Info", "Command executed."),
						("Info", $"Executing command [{ActionForUninstalling} \"{programTest.dummyAssemblyName}\"]..."),
						("Info", "Command executed."),
						("Info", "Executing command [queue continue]..."),
						("Error", "Process finished with error: Process exit code -1 Arguments: \"queue continue\" Output: \"it is a fake program\r\n\""),
					},
					GetRealLogInfos());
			}

			protected virtual void RunProcess(ProcessStartInfo processStartInfo)
			{
				var process = Process.Start(processStartInfo);
				process?.WaitForExit();
			}

			protected abstract int ExpectedBitVersion { get; }

			protected abstract IEnumerable<(string level, string message)> GetRealLogInfos();

			static void AssertContainsExactElementsInExactOrder(IEnumerable<(string level, string message)> expected, IEnumerable<(string level, string message)> actual)
			{
				AssertContainsExactElementsInExactOrder(new TupleCaseInsensitiveComparer(), expected, actual);
			}

			class TupleCaseInsensitiveComparer : IEqualityComparer<(string level, string message)>
			{
				public bool Equals((string level, string message) x, (string level, string message) y)
				{
					return StringComparer.OrdinalIgnoreCase.Compare(x.level, y.level) == 0
							&& StringComparer.OrdinalIgnoreCase.Compare(x.message, y.message) == 0;
				}

				public int GetHashCode((string level, string message) obj)
				{
					throw new NotImplementedException();
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			tempDir = new TempDirectory();
			assemblyPathInTemp = Path.Combine(tempDir.DirectoryName, VersionString);
			dummyAssemblyName = Path.Combine(assemblyPathInTemp, "TestDummy.dll");
			incorrectDllFileName = Path.Combine(assemblyPathInTemp, "Fake.dll");
			nonExistingFileName = Path.Combine(tempDir.DirectoryName, TempGuid);
		}

		[TestRequiresAdministrativePrivileges("Create event log source")]
		public void TestSuccessInstallReturnsCodeZero()
		{
			// Arrange
			using (CreateFilesThenDeleteOnDispose())
			{
				var installDummyAssemblyProcessStartInfo = GetProcessStartInfo("Install", dummyAssemblyName);

				// Act
				var process = GetAndWaitForProcess(installDummyAssemblyProcessStartInfo, out var standardOutputAndError);
				
				// Assert
				AssertEquals(standardOutputAndError, 0, process?.ExitCode);
			}
		}

		[TestRequiresAdministrativePrivileges("Create event log source")]
		public void TestSuccessUninstallReturnsCodeZero()
		{
			// Arrange
			using (CreateFilesThenDeleteOnDispose())
			{
				var installDummyAssemblyProcessStartInfo = GetProcessStartInfo("Install", dummyAssemblyName);
				var uninstallDummyAssemblyProcessStartInfo = GetProcessStartInfo("Uninstall", dummyAssemblyName);

				_ = GetAndWaitForProcess(installDummyAssemblyProcessStartInfo, out var standardOutputAndError);

				// Act
				var process = Process.Start(uninstallDummyAssemblyProcessStartInfo);
				process?.WaitForExit();

				// Assert
				AssertEquals(standardOutputAndError, 0, process?.ExitCode);
			}
		}

		[TestRequiresAdministrativePrivileges("Create event log source")]
		public void TestFailedInstallReturnsNonZero()
		{
			// Arrange
			var installNonExistingFileProcessStartInfo = GetProcessStartInfo("Install", nonExistingFileName);

			// Act
			var process = GetAndWaitForProcess(installNonExistingFileProcessStartInfo, out var standardOutputAndError);

			// Assert
			AssertNotEquals(standardOutputAndError, 0, process?.ExitCode);
		}

		[TestRequiresAdministrativePrivileges("Create event log source")]
		public void TestFailedUninstallReturnsNonZero()
		{
			// Arrange
			var uninstallDummyAssemblyProcessStartInfo = GetProcessStartInfo("Uninstall", dummyAssemblyName);

			// Act
			var process = Process.Start(uninstallDummyAssemblyProcessStartInfo);
			process?.WaitForExit();

			// Assert
			AssertNotEquals(0, process?.ExitCode);
		}

		protected override void TearDown()
		{
			tempDir.Dispose();
			base.TearDown();
		}

		static ProcessStartInfo GetProcessStartInfo(string action, string file, int delayTimeInMsForTest = 0, string nGenPath = "")
		{
			return new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "CargoWise.NGenInstaller.exe"))
			{
				WorkingDirectory = Environment.CurrentDirectory,
				Arguments = $"{action} \"{file}\" \"\" {delayTimeInMsForTest} \"{nGenPath}\"",
				UseShellExecute = false,
				CreateNoWindow = true,
			};
		}

		static Process GetAndWaitForProcess(ProcessStartInfo processStartInfo, out string standardOutputAndError)
		{
			processStartInfo.RedirectStandardError = true;
			processStartInfo.RedirectStandardOutput = true;

			var process = Process.Start(processStartInfo);

			var stringBuilder = new StringBuilder();

			DataReceivedEventHandler logToStringBuilder = (sender, args) => stringBuilder.AppendLine(args.Data);

			process.ErrorDataReceived += logToStringBuilder;
			process.OutputDataReceived += logToStringBuilder;

			try
			{
				process.BeginErrorReadLine();
				process.BeginOutputReadLine();

				process?.WaitForExit();
				OldVersionsRemoverTest.WaitForNGenInstaller();

				return process;
			}
			finally
			{
				process.ErrorDataReceived -= logToStringBuilder;
				process.OutputDataReceived -= logToStringBuilder;

				standardOutputAndError = stringBuilder.ToString();

				if (string.IsNullOrWhiteSpace(standardOutputAndError))
				{
					standardOutputAndError = $"[No output from process {processStartInfo.FileName} {processStartInfo.Arguments}]";
				}
			}
		}

		IDisposable CreateFilesThenDeleteOnDispose()
		{
			Directory.CreateDirectory(assemblyPathInTemp);
			MakeDummyAssembly(dummyAssemblyName);

			File.WriteAllText(incorrectDllFileName, "");

			return new DisposableAction(() =>
			{
				var startInfo = GetProcessStartInfo("Uninstall", dummyAssemblyName);
				var process = Process.Start(startInfo);
				process?.WaitForExit();
			});

			void MakeDummyAssembly(string assemblyName)
			{
				CompilerParameters parameters =
					new CompilerParameters { GenerateExecutable = false, OutputAssembly = assemblyName };

				_ = CodeDomProvider
					.CreateProvider("CSharp")
					.CompileAssemblyFromSource(parameters, "public class MyClass {public static int mem=1;}");
			}
		}

		const string TempGuid = "7BC8C8FC-BECE-4CC4-821E-7D8B3D920FD9";
		TempDirectory tempDir;
		string assemblyPathInTemp;
		string dummyAssemblyName;
		string nonExistingFileName;
		string incorrectDllFileName;
		string VersionString = VersionStringFor64;
		const string VersionStringFor64 = "20.0";
		const string VersionStringFor32 = "15.0";
	}
}
