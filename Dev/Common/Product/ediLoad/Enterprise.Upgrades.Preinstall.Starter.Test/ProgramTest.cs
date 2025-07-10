using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using CargoWise.IO;
using Enterprise.Upgrades.UnitTestHelper;
using NUnit.Framework;

namespace Enterprise.Upgrades.Preinstall.Starter.Test
{
	public class ProgramTest
	{
		static IEnumerable<string[]> TestArgsList
		{
			get
			{
				yield return Array.Empty<string>();
				yield return new string[1] { "-NoUI" };
			}
		}

		[TestCaseSource(nameof(TestArgsList))]
		public void TestProgramMainTakesMoreThanOneCommandLineArgs(string[] extraArgs)
		{
			// Arrange
			const string code = @"
using System;

namespace Enterprise.Upgrades.Preinstall.Starter.Test
{
	public class Program
	{
		public static int Main(string[] args)
		{
			return args.Length;
		}
	}
}
";
			// Act
			// Assert
			TestRunNewProgram(
				code,
				extraArgs,
				(process) => Assert.That(process.ExitCode, Is.EqualTo(extraArgs.Length + 1)));
		}

		[TestCaseSource(nameof(TestArgsList))]
		public void TestStartPreinstallProcessInBackground(string[] extraArgs)
		{
			// Arrange
			const string code = @"
using System;

namespace Enterprise.Upgrades.Preinstall.Starter.Test
{
	public class Program
	{
		public static int Main(string[] args)
		{
			return 0;
		}
	}
}
";
			// Act
			// Assert
			TestRunNewProgram(
				code,
				extraArgs,
				(process) => Assert.That(process.StartInfo.CreateNoWindow, Is.True));
		}

		[TestCase]
		public void TestStarterExeUseDefaultDotNetFramework()
		{
			// Arrange
			var starterExe = Assembly.GetAssembly(typeof(Program)).Location;
			var doc = new XmlDocument();
			doc.Load(starterExe + ".config");

			// Act
			var version = doc
				.DocumentElement
				.SelectSingleNode("/configuration/startup/supportedRuntime[@version='v4.0']")
				.Attributes["sku"]
				.Value;

			// Assert
			var dotNetVersion = version.Split('v').Last();
			Assert.That(new Version(dotNetVersion), Is.LessThanOrEqualTo(new Version("4.8.0")),
				"Preinstall4.0.exe should use Windows10 default version of .net framework. This test should only change when all wisecloud machines install new version of .net framework.");
		}

		[TestCase]
		public void TestProgramCheckExecutableDotNetVersion()
		{
			// Arrange
			const string futureVersion = ".NETFramework,Version=v1000.10";
			using var tempDir = new TempDirectory();
			var starterExe = CopyStarterToTargetFolder(tempDir);
			var preinstallExe = CopyPreinstallExeToTargetFolder(tempDir);
			ModifyConfigToFutureVersion(preinstallExe, futureVersion);

			var args = new[] { "InstallationPath", "-NoUI" };
			var processInfo = new ProcessStartInfo()
			{
				FileName = starterExe,
				Arguments = "",
				UseShellExecute = false,
				CreateNoWindow = true,
				WorkingDirectory = Path.GetDirectoryName(tempDir),
				RedirectStandardError = true,
			};
			var errorMessages = new List<string>();

			// Act
			var process = Process.Start(processInfo);
			process.ErrorDataReceived += (s, e) => errorMessages.Add(e.Data);
			process.BeginErrorReadLine();

			// Assert
			Assert.That(process.WaitForExit, Throws.Nothing);
			Assert.That(process.ExitCode, Is.Not.EqualTo(0));
			Assert.That(errorMessages[0], Does.Contain(nameof(NotSupportedException)));

			string CopyStarterToTargetFolder(string targetFolder)
			{
				var requiredAssemblies = new List<string>
				{
					"CargoWise.Definitions.dll",
					"Enterprise.Upgrades.dll",
				};
				var currentStarterExe = Assembly.GetAssembly(typeof(Program)).Location;
				var currentStarterFolder = Path.GetDirectoryName(currentStarterExe);
				var starterExeName = Path.GetFileName(currentStarterExe);
				var targetStarterExe = Path.Combine(targetFolder, starterExeName);

				File.Copy(starterExeName, targetStarterExe);
				File.Copy(starterExeName + ".config", targetStarterExe + ".config");
				requiredAssemblies.ForEach(x => File.Copy(Path.Combine(currentStarterFolder, x), Path.Combine(targetFolder, x)));

				return targetStarterExe;
			}

			string CopyPreinstallExeToTargetFolder(string targetFolder)
			{
				var currentPreintallFolder = Directory.GetCurrentDirectory();
				var currentPreintallExe = Path.Combine(currentPreintallFolder, Program.preInstallAssembly);
				var targetPreintallExe = Path.Combine(targetFolder, Program.preInstallAssembly);

				File.Copy(currentPreintallExe, targetPreintallExe);
				File.Copy(currentPreintallExe + ".config", targetPreintallExe + ".config");
				return targetPreintallExe;
			}

			void ModifyConfigToFutureVersion(string exePath, string futureVersion)
			{
				var doc = new XmlDocument();
				doc.Load(exePath + ".config");

				var versionNode = doc
					.DocumentElement
					.SelectSingleNode("/configuration/startup/supportedRuntime[@version='v4.0']");
				versionNode.Attributes["sku"].Value = futureVersion;
				doc.Save(exePath + ".config");
			}
		}

		void TestRunNewProgram(string codeToRun, string[] extraArgs, Action<Process> assertionAfterProcessStarted)
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var packageApplicationPath = Path.Combine(tempDir, "Distribution", "Application");
				Directory.CreateDirectory(packageApplicationPath);
				var starterExePath = Path.Combine(tempDir, Program.preInstallAssembly);
				var buildResult = CreateAssembly(starterExePath, codeToRun);
				CopyExistedConfigToTargetFolder(Directory.GetCurrentDirectory(), tempDir);
				Assert.That(buildResult, Is.Empty);

				// Act
				var args = new[] { packageApplicationPath }.Concat(extraArgs).ToArray();
				var process = Program.StartPreinstallProcess(tempDir, args);

				// Assert
				Assert.That(process.WaitForExit((int)TimeSpan.FromSeconds(30).TotalMilliseconds), Is.True);
				assertionAfterProcessStarted.Invoke(process);

				void CopyExistedConfigToTargetFolder(string existedConfigFolder, string targetFolder)
				{
					var existedConfig = Path.Combine(existedConfigFolder, Program.preInstallAssembly + ".config");
					var targetConfig = Path.Combine(targetFolder, Program.preInstallAssembly + ".config");
					File.Copy(existedConfig, targetConfig);
				}
			}
		}

		IEnumerable<string> CreateAssembly(string assemblyPath, string codeToRun)
		{
			return RuntimeCompilerHelper.CreateTestAssembly(
				() => new[] { "System.dll", "System.Runtime.dll" },
				() => codeToRun,
				assemblyPath,
				generateExecutable: true);
		}
	}
}
