using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Loader.Common.Installers
{
	public abstract class AssemblyResourceInstaller : WindowsInstallerAppManagerInvoker
	{
		protected AssemblyResourceInstaller(Installation installation) : base(installation)
		{
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			using (CleanupSetupFile())
			{
				ExtractInstaller();
				return DoInstallation();
			}
		}

		InstallationResult DoInstallation()
		{
			InstallationResult result;
			if (AssemblyResolver.IsApplicationManagerInstalled())
			{
				result = base.InstallExcludingDependencies();
				if (result.IsError)
				{
					if (Installation.Configuration.Services.MessageBox.Show(
						$"Automatic upgrade of {NameOfComponentBeingInstalled} services failed:\r\n{result.Message}\r\n\r\nThe upgrade will now run in the foreground.",
						NameOfComponentBeingInstalled,
						MessageBoxButtons.OKCancel,
						MessageBoxIcon.Error) == DialogResult.OK)
					{
						result = InstallManually();
					}
				}
			}
			else
			{
				result = InstallManually();
			}
			if (result.IsOK)
			{
				if (!WaitForInstallerToComplete)
				{
					return result;
				}

				Installation.Configuration.Services.MessageBox.Show(
					$"{NameOfComponentBeingInstalled} upgrade completed.",
					NameOfComponentBeingInstalled,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			return result;
		}

		InstallationResult InstallManually()
		{
			installingManually = true;
			return InvokeInstaller(InstallerAbsolutePath).FirstOrDefault(item => item.IsError) ?? InstallationResult.OK();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Path")]
		void ExtractInstaller()
		{
			installerAbsolutePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WiseTech Global", Process.GetCurrentProcess().Id.ToString(), InstallerName);
			if (!File.Exists(installerAbsolutePath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(installerAbsolutePath));
				using (var resourceStream = ResourceStream())
				using (var fileStream = File.Create(installerAbsolutePath))
				{
					resourceStream.CopyTo(fileStream);
				}
			}
		}

		IDisposable CleanupSetupFile()
		{
			return new DisposableAction(() =>
			{
				if (!WaitForInstallerToComplete)
				{
					return;
				}
				Cleanup();
			});
		}

		public void CleanUpSetupFiles()
		{
			if (!WaitForInstallerToComplete)
			{
				// After waiting for SetUp.exe process to exit, we will delete the setup file.
				// Note that we allow process still being running even up to 9 seconds, which will result in exception during cleanup below.
				for(int i = 0; i < 3; i++)
				{
					try
					{
						Thread.Sleep(3000);
						Cleanup();
						break;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						// Ignore any exception during cleanup because the setup file left behind will be overwritten by the next run.
					}
				}
			}
		}

		void Cleanup()
		{
			if (!File.Exists(installerAbsolutePath))
			{
				return;
			}
			File.Delete(installerAbsolutePath);
		}

		protected override string Arguments
		{
			get { return installingManually ? "" : "/qn"; }
		}

		protected override bool IsRemoteExclusive
		{
			get { return true; }
		}

		protected override Configuration GetNewConfiguration()
		{
			return new Configuration();
		}

		public override string InstallerAbsolutePath
		{
			get { return installerAbsolutePath; }
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		protected abstract string InstallerName { get; }

		protected abstract Stream ResourceStream();

		string installerAbsolutePath;

		bool installingManually;
	}
}
