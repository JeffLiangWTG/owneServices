using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.NGenInstallerProgram;
using CargoWise.NGenInstallerProgram.Exceptions;
using Enterprise.Upgrades;
using Microsoft.CSharp;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.NGenInstaller.Testing
{
	class NGenCallerTest
	{
		[TestCase]
		[Property("DAT:RequiresAdminPrivileges", 1)]
		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Using the same fixed path for CW1 installers")]
		public void InstallAndUninstall()
		{
			// Arrange
			var testDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "WiseTech Global", "CargoWise One", nameof(NGenCallerTest), $"{Guid.NewGuid():N}");
			using (var tempDirectory = new TempDirectory(testDir))
			{
				var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				new[]
				{
					ExeFileNames.CargoWiseWindowsDesktopExe,
					$"{ExeFileNames.CargoWiseWindowsDesktopExe}.config",
				}.ForEach(x => File.Copy(Path.Combine(binPath, x), Path.Combine(tempDirectory, x)));
				var exeConfigPath = Path.Combine(tempDirectory, ExeFileNames.CargoWiseWindowsDesktopExe);

				var zombieAssemblyFilePath = Path.Combine(tempDirectory, "zombie.dll");
				CreateTestAssembly(zombieAssemblyFilePath);

				var installerLogs = new List<string>();
				var loggerMock = new Mock<ILogger>();
				loggerMock.Setup(x => x
					.Log(LogLevel.Information,
						It.IsAny<EventId>(),
						It.IsAny<It.IsAnyType>(),
						It.IsAny<Exception>(),
						It.IsAny<Func<It.IsAnyType, Exception, string>>()))
					.Callback(new InvocationAction(invocation =>
					{
						var state = invocation.Arguments[2];
						var exception = (Exception)invocation.Arguments[3];
						var formatter = invocation.Arguments[4];

						var invokeMethod = formatter.GetType().GetMethod("Invoke");
						var logMessage = (string)invokeMethod?.Invoke(formatter, new[] { state, exception });

						installerLogs.Add(logMessage);
					}));

				var ngenCaller = new NGenCaller(loggerMock.Object, ngenExePath);

				// Act - Install
				ngenCaller.InstallOrUninstall(NGenAction.Install, zombieAssemblyFilePath);

				// Assert
				var output = QueryNGen(zombieAssemblyFilePath);
				Assert.That(output, Does.Contain(zombieAssemblyFilePath));
				Assert.That(output, Does.Not.Contain("Error: The specified assembly is not installed."));
				Assert.That(installerLogs, Does.Contain($"Using NGen.exe located: {ngenExePath}"));
				Assert.That(installerLogs, Does.Contain($@"Executing command [Install ""{zombieAssemblyFilePath}"" /ExeConfig:""{exeConfigPath}""]..."));
				Assert.That(installerLogs, Does.Contain("Command executed."));
				installerLogs.Clear();

				// Act - Uninstall
				ngenCaller.InstallOrUninstall(NGenAction.Uninstall, zombieAssemblyFilePath);

				// Assert
				output = QueryNGen(zombieAssemblyFilePath);
				Assert.That(output, Does.Contain("Error: The specified assembly is not installed."));
				Assert.That(output, Does.Not.Contain(zombieAssemblyFilePath));
				Assert.That(installerLogs, Does.Contain($@"Executing command [Uninstall ""{zombieAssemblyFilePath}""]..."));
				Assert.That(installerLogs, Does.Contain("Command executed."));
			}
		}

		[TestCase]
		[Property("DAT:RequiresAdminPrivileges", 1)]
		public void NGenCallException()
		{
			// Arrange
			using (var tempDirectory = new TempDirectory())
			{
				var exeConfigPath = Path.Combine(tempDirectory, ExeFileNames.CargoWiseWindowsDesktopExe);
				var zombieAssemblyFilePath = Path.Combine(tempDirectory, "zombie.dll");
				var ngenCaller = new NGenCaller(Mock.Of<ILogger>(), ngenExePath);

				// Act - Install
				var exception = Assert.Throws<NGenCallException>(() => ngenCaller.InstallOrUninstall(NGenAction.Install, zombieAssemblyFilePath));

				// Assert
				Assert.That(
					exception.Message,
					Does.Contain("Process finished with error: Process exit code -1")
					.And.Contain($@"Install ""{zombieAssemblyFilePath}"""));

				// Act - Uninstall
				exception = Assert.Throws<NGenCallException>(() => ngenCaller.InstallOrUninstall(NGenAction.Uninstall, zombieAssemblyFilePath));

				// Assert
				// ToDo: assert no exception thrown after the #if DEBUG code is removed
				// see https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FCommon%2FProduct%2FediLoad%2FCargoWise.NGenInstaller%2FNGenCaller.cs&version=GBmaster&line=67&lineEnd=70&lineStartColumn=1&lineEndColumn=7&lineStyle=plain&_a=contents
				// from NGenCaller.cs InstallOrUninstall method
				// where there is a requirement to do so "Always raise the error in debug builds, as it is required for OldVersionsRemoverTest.TestDeleteOrphanedVersionManyTimes"
				Assert.That(
					exception.Message,
					Does.Contain("Process finished with error: Process exit code -1")
					.And.Contain($@"Uninstall ""{zombieAssemblyFilePath}""")
					.And.Contain("Error: The specified assembly is not installed."));
			}
		}

		[TestCase]
		public void ProcessIsKilledAfterTimeout()
		{
			// Arrange
			using (var nGen = MockProcess.WithExecutionTime(TimeSpan.FromSeconds(30)))
			{
				var nGenCaller = new NGenCaller(Mock.Of<ILogger>(), nGen.FilePath, timeout: TimeSpan.FromSeconds(5));

				// Act
				Assert.That(() => nGenCaller.InstallOrUninstall(NGenAction.Install, @"c:\some\path"), Throws.InstanceOf<NGenTimeoutException>());

				// Assert
				Assert.That(nGen.GetExecutionCount(), Is.EqualTo(1));
			}
		}

		[TestCase]
		public void TestDefaultNGenConfig()
		{
			var callerWithDefaultConfig = new NGenCaller(Mock.Of<ILogger>());

			Assert.That(callerWithDefaultConfig.ExecutionTimeout, Is.EqualTo(TimeSpan.FromMinutes(25)));
		}

		static readonly string ngenExePath =
			Path.Combine(Directory.GetParent(Environment.SystemDirectory).FullName, $@"Microsoft.NET\Framework64\v4.0.30319\NGen.exe");

		static string QueryNGen(string assemblyFilePath)
		{
			var startInfo = new ProcessStartInfo(ngenExePath)
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = false,
				Arguments = $@"display ""{assemblyFilePath}""",
			};

			using (var process = Process.Start(startInfo))
			{
				var output = process.StandardOutput.ReadToEnd();
				process.WaitForExit();

				return output;
			}
		}

		static IEnumerable<string> CreateTestAssembly(string assemblyPath)
		{
			using (var compiler = new CSharpCodeProvider())
			{
				var options = new CompilerParameters();

				options.ReferencedAssemblies.Add("System.dll");
				options.GenerateInMemory = false;
				options.IncludeDebugInformation = true;
				options.GenerateExecutable = false;
				options.OutputAssembly = assemblyPath;

				var code = $@"
using System;

namespace CargoWise.NGenInstaller.Testing
{{
	public class Zombie
	{{
		public void Greetings()
		{{
			Console.WriteLine(""Hello, world!"");
		}}
	}}
}}
";
				var result = compiler.CompileAssemblyFromSource(options, code);
				var output = new string[result.Output.Count];
				result.Output.CopyTo(output, 0);

				return output;
			}
		}

		class MockProcess : IDisposable
		{
			MockProcess(int delay)
			{
				this.delay = delay;
				temp = new TempDirectory();
				batchFilePath = Path.Combine(temp, "proc.bat");
				counterFilePath = Path.Combine(temp, "count.txt");
				File.WriteAllText(counterFilePath, "0");
			}

			public string FilePath
			{
				get
				{
					CreateBatchFile();
					return batchFilePath;
				}
			}

			public int GetExecutionCount()
			{
				return int.Parse(File.ReadAllText(counterFilePath));
			}

			void CreateBatchFile()
			{
				if (!File.Exists(batchFilePath))
				{
					File.WriteAllText(batchFilePath, $@"
set /p counter=<""{counterFilePath}""
set /a counter += 1
echo %counter% > ""{counterFilePath}""

if %counter% gtr {noMoreDelayThreshold ?? 99} ( 
 ping localhost -n 1
) else ( 
 ping localhost -n {delay + 1} 
)
");
				}
			}

			public void Dispose()
			{
				temp?.Dispose();
			}

			public MockProcess ForTheFirstThreeTimes()
			{
				noMoreDelayThreshold = 3;
				return this;
			}

			public static MockProcess WithExecutionTime(TimeSpan time)
			{
				return new MockProcess((int)time.TotalSeconds);
			}

			readonly TempDirectory temp;
			readonly string batchFilePath;
			readonly string counterFilePath;
			readonly int delay;
			int? noMoreDelayThreshold;
		}
	}
}
