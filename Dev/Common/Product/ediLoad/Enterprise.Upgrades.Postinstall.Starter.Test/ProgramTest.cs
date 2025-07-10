using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CargoWise.IO;
using Enterprise.Upgrades.UnitTestHelper;
using NUnit.Framework;

namespace Enterprise.Upgrades.Postinstall.Starter.Test
{
	public class ProgramTest
	{
		static IEnumerable<string[]> TestArgsList
		{
			get
			{
				yield return Array.Empty<string>();
				yield return new[] { "InstallationPath" };
				yield return new[] { "InstallationPath", "-NoUI" };
			}
		}

		[TestCaseSource(nameof(TestArgsList))]
		public void TestProgramMainTakesMoreThanOneCommandLineArgs(string[] args)
		{
			// Arrange
			const string code = @"
using System;

namespace Enterprise.Upgrades.Postinstall.Starter.Test
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
				args,
				(process) => Assert.That(process.ExitCode, Is.EqualTo(args.Length)));
		}

		[TestCaseSource(nameof(TestArgsList))]
		public void TestStartPreinstallProcessInBackground(string[] args)
		{
			// Arrange
			const string code = @"
using System;

namespace Enterprise.Upgrades.Postinstall.Starter.Test
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
				args,
				(process) => Assert.That(process.StartInfo.CreateNoWindow, Is.True));
		}

		void TestRunNewProgram(string codeToRun, string[] extraArgs, Action<Process> assertionAfterProcessStarted)
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var starterExePath = Path.Combine(tempDir, Program.postInstallAssembly);

				var buildResult = RuntimeCompilerHelper.CreateTestAssembly(
					() => new[] { "System.dll", "System.Runtime.dll" },
					() => codeToRun,
					starterExePath,
					generateExecutable: true);
				Assert.That(buildResult, Is.Empty);

				// Act
				var process = Program.StartPostinstallProcess(starterExePath, extraArgs);

				// Assert
				Assert.That(process.WaitForExit((int)TimeSpan.FromSeconds(30).TotalMilliseconds), Is.True);
				assertionAfterProcessStarted.Invoke(process);
			}
		}
	}
}
