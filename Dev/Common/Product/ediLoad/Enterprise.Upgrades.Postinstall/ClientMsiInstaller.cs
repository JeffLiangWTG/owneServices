using System.IO;
using CargoWise.ApplicationManager.Common;
using CargoWise.Loader.Common;
using Enterprise.Client.Common;
using Microsoft.Win32;

namespace Enterprise.Upgrades.Postinstall
{
	class ClientMsiInstaller : WindowsInstallerAppManagerInvoker
	{
		public ClientMsiInstaller()
			: this(null)
		{
		}

		public ClientMsiInstaller(Installation installation)
			: base(installation)
		{
			IgnoreMutexErrors = true;
		}

		protected override string Arguments
		{
			get
			{
				return "/qn";
			}
		}

		public override string InstallerAbsolutePath
		{
			get { return Path.Combine(Installation.Configuration.GetApplicationPathInCurrentPackage(), "CargoWiseOneSetup.msi"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected override string NameOfComponentBeingInstalled
		{
			get { return "CargoWise One Client"; }
		}

		protected override bool NeedsToInstallCore()
		{
			using (var regKey = Registry.LocalMachine.OpenSubKey(SetupHelper.KeyName, false))
			{
				return regKey == null || regKey.GetValue("ClientInstalled") == null;
			}
		}

		protected override MutexRequest CreateMutexRequest()
		{
			return MutexRequest.Create(NameOfComponentBeingInstalled, 1);
		}

		protected override void SetAppManagerState(object state)
		{
			fullPathOfProgramToRun = (string)state;
		}

		protected virtual ClientSetupHelper GetNewSetupHelper()
		{
			return new ClientSetupHelper();
		}

		ClientSetupHelper SetupHelper
		{
			get { return setupHelper ?? (setupHelper = GetNewSetupHelper()); }
		}

		protected override Configuration GetNewConfiguration()
		{
			return new Configuration();
		}

		ClientSetupHelper setupHelper;
	}
}

