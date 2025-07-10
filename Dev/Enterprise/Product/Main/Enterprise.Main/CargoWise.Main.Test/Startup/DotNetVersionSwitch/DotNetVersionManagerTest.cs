using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Main.Startup.DotNetVersionSwitch;
using Enterprise.Startup;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static CargoWise.Main.Test.ApplicationArgumentsTestHelper;

namespace CargoWise.Main.Test.Startup.DotNetVersionSwitch
{
	public class DotNetVersionManagerTest : TestCase
	{
#if WINZOR
		public void TestIsVersionCurrentRunning()
		{
			var originalEnvVariable = Environment.GetEnvironmentVariable("WINZOR_USE_NETCORE_LIBS");
			var net8Instance = new DotNetCore8VersionManager();
			var netFrameworkInstance = new DotNetFrameworkVersionManager();
			try
			{
				Environment.SetEnvironmentVariable("WINZOR_USE_NETCORE_LIBS", "True");
				AssertEquals("Net8 should be current running", expected: true, net8Instance.IsVersionCurrentRunning());
				AssertEquals("NetFramework should not be current running", expected: false, netFrameworkInstance.IsVersionCurrentRunning());

				Environment.SetEnvironmentVariable("WINZOR_USE_NETCORE_LIBS", "False");
				AssertEquals("Net8 should not be current running", expected: false, net8Instance.IsVersionCurrentRunning());
				AssertEquals("NetFramework should be current running", expected: true, netFrameworkInstance.IsVersionCurrentRunning());

				Environment.SetEnvironmentVariable("WINZOR_USE_NETCORE_LIBS", string.Empty);
				AssertEquals("Net8 should not be current running", expected: false, net8Instance.IsVersionCurrentRunning());
				AssertEquals("NetFramework should be current running", expected: true, netFrameworkInstance.IsVersionCurrentRunning());

				Environment.SetEnvironmentVariable("WINZOR_USE_NETCORE_LIBS", null);
				AssertEquals("Net8 should not be current running", expected: false, net8Instance.IsVersionCurrentRunning());
				AssertEquals("NetFramework should be current running", expected: true, netFrameworkInstance.IsVersionCurrentRunning());
			}
			finally
			{
				Environment.SetEnvironmentVariable("WINZOR_USE_NETCORE_LIBS", originalEnvVariable);
			}
		}

		public void TestNet8VersionManagerQueryString()
		{
			var net8Instance = new DotNetCore8VersionManager();
			var query = net8Instance.BuildQueryString();
			AssertContains("Query should contain NetCore parameter", "netcore", query.ToString());
		}

		public void TestNetFrameworkVersionManagerQueryString()
		{
			var net8Instance = new DotNetFrameworkVersionManager();
			var query = net8Instance.BuildQueryString();
			AssertNotContains("Query should not contain NetCore parameter", "netcore", query.ToString());
		}
#else
		public void TestLaunchCurrentVersion()
		{
			using (TemporaryApplicationArguments([]))
			{
				var mockProgramRestarter = new Mock<IProgramRestarter>();
				var manager = new DotNetVersionManagerForTest(mockProgramRestarter.Object)
				{
					ExeFilePath = Path.Combine(AssemblyLoader.GetBinPath(), ExeFileNames.CargoWiseWindowsDesktopExe),
				};

				AssertNoExceptionThrown(manager.LaunchCurrentVersion);
				mockProgramRestarter.Verify(m => m.Restart(manager.GetExePath(), It.Is<CommandLineArguments>(args => (bool)args[ApplicationArguments.OptionSkipDotNetVersionSwitch])), Times.Once);
			}
		}

		public void TestLaunchCurrentVersionThrowApplicationException()
		{
			using (TemporaryApplicationArguments([]))
			{
				var originalArgs = CommandLineArguments.UsedToLaunchApplication.ToString();
				var mockProgramRestarter = new Mock<IProgramRestarter>();
				var manager = new DotNetVersionManagerForTest(mockProgramRestarter.Object)
				{
					ExeFilePath = "not-exists.exe",
				};

				var ex = AssertExceptionThrown<ApplicationException>(manager.LaunchCurrentVersion);
				AssertEquals("Exception message should match.", $"The executable for {manager.MenuCaption} does not exist.", ex.Message);
				AssertEquals("CommandLineArguments should not change.", originalArgs, CommandLineArguments.UsedToLaunchApplication.ToString());
				mockProgramRestarter.Verify(m => m.Restart(It.IsAny<string>(), It.IsAny<CommandLineArguments>()), Times.Never);
			}
		}

		public void TestIsVersionCurrentRunning()
		{
			var isNet8 = false;
			var isNetFramework = false;

#if NET8_0
			isNet8 = true;
#elif NETFRAMEWORK
			isNetFramework = true;
#endif

			var net8Instance = new DotNetCore8VersionManager() as IDotNetVersionManager;
			AssertEquals($"isNet8: {isNet8}", isNet8, net8Instance.IsVersionCurrentRunning());

			var netFrameworkInstance = new DotNetFrameworkVersionManager() as IDotNetVersionManager;
			AssertEquals($"isNetFramework: {isNetFramework}", isNetFramework, netFrameworkInstance.IsVersionCurrentRunning());
		}

		public void TestNet8VersionManagerPath()
		{
			var net8Instance = new DotNetCore8VersionManager();
			var path = net8Instance.GetExePath();
			Assert($"Path should be valid, but: {path}", path.Contains("net8.0"));
		}

		public void TestNetFrameworkVersionManagerPath()
		{
			var netFrameworkInstance = new DotNetFrameworkVersionManager();
			var path = netFrameworkInstance.GetExePath();
			Assert($"Path should be valid, but: {path}", !path.Contains("net8.0"));
		}

		class DotNetVersionManagerForTest(IProgramRestarter programRestarter) : DotNetVersionManager(programRestarter)
		{
			public string ExeFilePath;
			public override MultilingualString MenuCaption { get; } = (NoResString)"Test .Net Version Manager";
			public override bool IsVersionCurrentRunning() => false;
			public override string GetExePath() => ExeFilePath;
		}
#endif
	}
}
