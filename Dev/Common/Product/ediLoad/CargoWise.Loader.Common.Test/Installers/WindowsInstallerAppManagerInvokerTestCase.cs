using System.IO;
using CargoWise.ApplicationManager.Common;
using NUnit.Framework;

namespace CargoWise.Loader.Common
{
	public abstract class WindowsInstallerAppManagerInvokerTestCase<T> : TestCase where T : WindowsInstallerAppManagerInvoker
	{
		T installer;

		protected T Installer
		{
			get { return installer ?? (installer = GetInstaller()); }
		}

		protected abstract T GetInstaller();

		public virtual void TestRequiredFilesExistInServerInstall()
		{
			string expectedPath = Installer.InstallerAbsolutePath;
			Assert("File should exist: " + expectedPath, File.Exists(expectedPath));
		}
	}

	sealed class WindowsInstallerAppManagerInvokerTest : TestCase
	{
		public void TestGetResult()
		{
			InstallationResultCollection results = new InstallationResultCollection();
			results.Add(InstallationResult.Warning("Warning!"));
			results.Add(InstallationResult.Error("Error!"));
			AppManagerResult result = WindowsInstallerAppManagerInvoker.GetResult(results);
			AssertEquals("Status", AppManagerResultStatus.Error, result.Status);
			AssertEquals("Message", "Warning!\r\n\r\nError!", result.Message);

			results = new InstallationResultCollection();
			results.Add(InstallationResult.Warning("Warning!"));
			result = WindowsInstallerAppManagerInvoker.GetResult(results);
			AssertEquals("Status", AppManagerResultStatus.Success, result.Status);
			AssertEquals("Message", "Warning!", result.Message);

			results = new InstallationResultCollection();
			results.Add(InstallationResult.OK());
			result = WindowsInstallerAppManagerInvoker.GetResult(results);
			AssertEquals("Status", AppManagerResultStatus.Success, result.Status);
			AssertEquals("Message", string.Empty, result.Message);
		}

		public void TestIsValidState()
		{
			AssertEquals(true, WindowsInstallerAppManagerInvoker.IsStateValid("Test"));
			AssertEquals(false, WindowsInstallerAppManagerInvoker.IsStateValid(""));
			AssertEquals(false, WindowsInstallerAppManagerInvoker.IsStateValid(null));
			AssertEquals(false, WindowsInstallerAppManagerInvoker.IsStateValid(10));

			AssertEquals(true, WindowsInstallerAppManagerInvoker.IsStateValid(new[] { "Value 1", "Value 2" }));
			AssertEquals(false, WindowsInstallerAppManagerInvoker.IsStateValid(new[] { "", "Value 2" }));
			AssertEquals(false, WindowsInstallerAppManagerInvoker.IsStateValid(new[] { "Value 1", "" }));
			AssertEquals(false, WindowsInstallerAppManagerInvoker.IsStateValid(new[] { "Value 1", null }));
			AssertEquals(false, WindowsInstallerAppManagerInvoker.IsStateValid(new[] { 72, 34 }));
		}
	}
}