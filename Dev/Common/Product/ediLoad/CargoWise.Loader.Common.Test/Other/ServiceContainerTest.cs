using System.Diagnostics;
using CargoWise.Loader.Common.Native;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class ServiceContainerTest : TestCase
	{
		public void TestDirectory()
		{
			AssertEquals("Directory.GetType()", typeof(DirectoryProxy), ServiceContainer.Instance.Directory.GetType());
		}

		public void TestEnvironment()
		{
			AssertEquals("Environment.GetType()", typeof(EnvironmentProxy), ServiceContainer.Instance.Environment.GetType());
		}

		public void TestEventLog()
		{
			AssertEquals("EventLog.GetType()", typeof(EventLogProxy), ServiceContainer.Instance.EventLog.GetType());
		}

		public void TestFile()
		{
			AssertEquals("File.GetType()", typeof(FileProxy), ServiceContainer.Instance.File.GetType());
		}

		public void TestGetGetFileVersionInfo()
		{
			string fileName = Process.GetCurrentProcess().MainModule.FileName;
			IFileVersionInfoProxy versionInfo = ServiceContainer.Instance.GetFileVersionInfo(fileName);
			AssertEquals("GetFileVersionInfo().GetType()", typeof(FileVersionInfoProxy), versionInfo.GetType());
			AssertEquals("GetFileVersionInfo().FileBuildPart", FileVersionInfo.GetVersionInfo(fileName).FileBuildPart, versionInfo.FileBuildPart);
		}

		public void TestMessageBox()
		{
			AssertEquals("MessageBox.GetType()", typeof(MessageBoxProxy), ServiceContainer.Instance.MessageBox.GetType());
		}

		public void TestNativeMethods()
		{
			AssertEquals("NativeMethods.GetType()", typeof(NativeMethods), ServiceContainer.Instance.NativeMethods.GetType());
		}

		public void TestRegistry()
		{
			AssertEquals("Registry.GetType()", typeof(RegistryProxy), ServiceContainer.Instance.Registry.GetType());
		}
	}
}