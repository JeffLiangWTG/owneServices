using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.IO;
using CargoWise.Loader.Client;
using CargoWise.Loader.Common;
using Microsoft.CSharp;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	class ProgramLocationCheckerTest : TestCase
	{
		public void TestNeedsToInstall()
		{
			AssertEquals(true, new ProgramLocationChecker(new Installation(new MockEnterpriseConfiguration())).NeedsToInstall());
			AssertEquals(false, new ProgramLocationChecker(new Installation(new MockEnterpriseConfiguration() { BaseTargetPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) })).NeedsToInstall());
			var config = new MockEnterpriseConfiguration();
			config.Initialize(new string[] { EnterpriseConfiguration.RunFromThisLocationArgument });
			AssertEquals(false, new ProgramLocationChecker(new Installation(config)).NeedsToInstall());
		}

		public void TestRunWithInstalledExe()
		{
			using (var tempDirectory = new TempDirectory())
			{
				CreateExe(Path.Combine(tempDirectory.DirectoryName, "CargoWise.Start.exe"), FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
				var checker = new ProgramLocationCheckerForTest(new Installation(new MockEnterpriseConfiguration() { BaseTargetPath = tempDirectory.DirectoryName, StartupPathOverride = @"\\start\from\here" }));
				checker.Install(new InstallationResultCollection());
				AssertEquals(Path.Combine(tempDirectory.DirectoryName, "CargoWise.Start.exe"), checker.StartedExe);
				AssertEquals(@" -StartupPath:""\\start\from\here""", checker.StartedWithCommandLine);
			}
		}

		public void TestRunWithoutInstalledExe()
		{
			string tempExeFilePath = Path.Combine(Temp.TempPath, "CargoWise.Start.exe");
			string tempExeConfigFilePath = Path.Combine(Temp.TempPath, "CargoWise.Start.exe.config");
			string tempAppLogConfigFilePath = Path.Combine(Temp.TempPath, "applog.json");
			try
			{
				var checker = new ProgramLocationCheckerForTest(new Installation(new MockEnterpriseConfiguration() { BaseTargetPath = @"C:\no\such\path", StartupPathOverride = @"\\start\from\here" }));
				checker.Install(new InstallationResultCollection());
				AssertEquals(tempExeFilePath, checker.StartedExe);
				Assert(File.Exists(tempExeFilePath));
				Assert(File.Exists(tempExeConfigFilePath));
				Assert(File.Exists(tempAppLogConfigFilePath));
				AssertNotEquals(0, new FileInfo(tempExeConfigFilePath).Length);
				AssertEquals(@" -StartupPath:""\\start\from\here"" -RunFromThisLocation", checker.StartedWithCommandLine);
			}
			finally
			{
				File.Delete(tempExeFilePath);
				File.Delete(tempExeConfigFilePath);
				File.Delete(tempAppLogConfigFilePath);
			}
		}

		public void TestRunWithOldInstalledExe()
		{
			string tempExeFilePath = Path.Combine(Temp.TempPath, "CargoWise.Start.exe");
			string tempExeConfigFilePath = Path.Combine(Temp.TempPath, "CargoWise.Start.exe.config");
			string tempAppLogConfigFilePath = Path.Combine(Temp.TempPath, "applog.json");
			try
			{
				using (var tempDirectory = new TempDirectory())
				{
					var version = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
					version = new Version(version.Major, version.Minor, version.Build - 1, version.Revision);
					CreateExe(Path.Combine(tempDirectory.DirectoryName, "CargoWise.Start.exe"), version.ToString());
					var checker = new ProgramLocationCheckerForTest(new Installation(new MockEnterpriseConfiguration() { BaseTargetPath = tempDirectory.DirectoryName, StartupPathOverride = @"\\start\from\here" }));
					checker.Install(new InstallationResultCollection());
					Assert(File.Exists(tempExeFilePath));
					Assert(File.Exists(tempExeConfigFilePath));
					Assert(File.Exists(tempAppLogConfigFilePath));
					AssertNotEquals(0, new FileInfo(tempExeConfigFilePath).Length);
					AssertEquals(@" -StartupPath:""\\start\from\here"" -RunFromThisLocation", checker.StartedWithCommandLine);
				}
			}
			finally
			{
				File.Delete(tempExeFilePath);
				File.Delete(tempExeConfigFilePath);
				File.Delete(tempAppLogConfigFilePath);
			}
		}

		public void TestAddRunFromThisLocationArgumentWhenCommandArgumentIsPresent()
		{
			var tempExeFilePath = Path.Combine(Temp.TempPath, "CargoWise.Start.exe");
			var tempExeConfigFilePath = Path.Combine(Temp.TempPath, "CargoWise.Start.exe.config");
			var tempAppLogConfigFilePath = Path.Combine(Temp.TempPath, "applog.json");
			try
			{
				using (var tempDirectory = new TempDirectory())
				{
					var mockConfig = new MockEnterpriseConfiguration()
					{
						BaseTargetPath = tempDirectory.DirectoryName,
						StartupPathOverride = @"\\start\from\here",
					};
					mockConfig.Initialize(new string[]
					{
						"ServerName",
						"AB -Cmd CD", //DatabaseName
						"-UpdateUsageLog",
						"-RemoveOldVersions",
						"-NoUI",
						"-Automated",
						"-InstallCurrentVersion",
						"-Cmd", "C:\\Program Files\\WiseTech\\SessionBroker\\CargoWise.Blazor.SessionBroker.exe",
						"--urls", "http://127.0.0.1:0/",
						"--CargoWiseOptions:VersionBrokerProcessCorrelationId", "xyz",
					});
					AssertEquals(
						expected: "ServerName \"AB -Cmd CD\" -UpdateUsageLog -RemoveOldVersions -NoUI -Automated -InstallCurrentVersion -Cmd \"C:\\Program Files\\WiseTech\\SessionBroker\\CargoWise.Blazor.SessionBroker.exe\" --urls http://127.0.0.1:0/ --CargoWiseOptions:VersionBrokerProcessCorrelationId xyz",
						actual: mockConfig.AllArguments);
					var checker = new ProgramLocationCheckerForTest(
							new Installation(mockConfig));
					checker.Install(new InstallationResultCollection());
					AssertEquals(
						expected: "ServerName \"AB -Cmd CD\" -UpdateUsageLog -RemoveOldVersions -NoUI -Automated -InstallCurrentVersion -StartupPath:\"\\\\start\\from\\here\" -RunFromThisLocation -Cmd \"C:\\Program Files\\WiseTech\\SessionBroker\\CargoWise.Blazor.SessionBroker.exe\" --urls http://127.0.0.1:0/ --CargoWiseOptions:VersionBrokerProcessCorrelationId xyz",
						actual: checker.StartedWithCommandLine);
				}
			}
			finally
			{
				File.Delete(tempExeFilePath);
				File.Delete(tempExeConfigFilePath);
				File.Delete(tempAppLogConfigFilePath);
			}
		}

		public void TestAdditionalParametersWillPassToSessionBroker()
		{
			var tempExeFilePath = Path.Combine(Temp.TempPath, "CargoWise.Start.exe");
			var tempExeConfigFilePath = Path.Combine(Temp.TempPath, "CargoWise.Start.exe.config");
			var tempAppLogConfigFilePath = Path.Combine(Temp.TempPath, "applog.json");
			try
			{
				using (var tempDirectory = new TempDirectory())
				{
					var mockConfig = new MockEnterpriseConfiguration()
					{
						BaseTargetPath = tempDirectory.DirectoryName,
						StartupPathOverride = @"\\start\from\here",
					};
					mockConfig.Initialize(new string[]
					{
					"ServerName",
					"AB -Cmd CD", //DatabaseName
					"-UpdateUsageLog",
					"-RemoveOldVersions",
					"-NoUI",
					"-Automated",
					"-InstallCurrentVersion",
					"-Cmd", "C:\\Program Files\\WiseTech\\SessionBroker\\CargoWise.Blazor.SessionBroker.exe",
					"--urls", "http://127.0.0.1:0/",
					"--VersionBrokerRegistrationCallback", $"http://localhost:11933/BlazorSessionBrokerLaunched/guid-0031",
					"--CargoWiseOptions:VersionBrokerProcessCorrelationId", "xyz",
					"--CargoWiseOptions:DbServerName", "SYDSP-SSQL-6.sand.wtg.zone\\INSTANCE1",
					"--CargoWiseOptions:DatabaseName", "\"SH0WI00672635 Add Something\"",
					});

					var install = new InstallationProgramFromVersionedFile(new ClientInstallation(mockConfig));
					AssertEquals(
						expected: "--urls http://127.0.0.1:0/ --VersionBrokerRegistrationCallback http://localhost:11933/BlazorSessionBrokerLaunched/guid-0031 --CargoWiseOptions:VersionBrokerProcessCorrelationId xyz --CargoWiseOptions:DbServerName SYDSP-SSQL-6.sand.wtg.zone\\INSTANCE1 --CargoWiseOptions:DatabaseName \"SH0WI00672635 Add Something\"",
						install.Arguments);
				}
			}
			finally
			{
				File.Delete(tempExeFilePath);
				File.Delete(tempExeConfigFilePath);
				File.Delete(tempAppLogConfigFilePath);
			}
		}

		void CreateExe(string path, string version)
		{
			CSharpCodeProvider compiler = new CSharpCodeProvider();
			CompilerParameters options = new CompilerParameters();
			options.ReferencedAssemblies.Add("System.dll");
			options.GenerateExecutable = true;
			options.OutputAssembly = Path.Combine(path);
			CompilerResults result = compiler.CompileAssemblyFromSource(options, string.Format(
@"					[assembly: System.Reflection.AssemblyFileVersion(""{0}"")]
					public static class Program
					{{
						public static void Main(string[] cmd)
						{{
						}}
					}}", version));
			string[] output = new string[result.Output.Count];
			result.Output.CopyTo(output, 0);
			AssertEquals(string.Join("\r\n", output), 0, result.Errors.Count);
		}

		class ProgramLocationCheckerForTest : ProgramLocationChecker
		{
			public ProgramLocationCheckerForTest(Installation installation)
				: base(installation)
			{
			}

			internal override void StartAndExit(string exe, string commandLine)
			{
				this.StartedExe = exe;
				this.StartedWithCommandLine = commandLine;
			}

			public string StartedExe { get; private set; }
			public string StartedWithCommandLine { get; private set; }
		}
	}
}
