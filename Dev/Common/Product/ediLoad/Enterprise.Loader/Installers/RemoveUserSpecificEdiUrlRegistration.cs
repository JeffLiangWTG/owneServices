using CargoWise.Loader.Common;
using Microsoft.Win32;

namespace Enterprise.Loader
{
	class RemoveUserSpecificEdiUrlRegistration : InstallationItem
	{
		public RemoveUserSpecificEdiUrlRegistration(Installation installation)
			: base(installation)
		{
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			ClearValue(@"Software\Classes\EdiEnterprise.edient\DefaultIcon");
			ClearValue(@"Software\Classes\EdiEnterprise.edient\shell\open\command");
			ClearValue(@"Software\Classes\edient\DefaultIcon");
			ClearValue(@"Software\Classes\edient\shell\open\command");
			return InstallationResult.OK();
		}

		void ClearValue(string keyPath)
		{
			using (var key = Registry.CurrentUser.OpenSubKey(keyPath, true))
			{
				if (key != null && key.GetValue(null) != null)
				{
					key.DeleteValue(null);
				}
			}
		}
	}
}

