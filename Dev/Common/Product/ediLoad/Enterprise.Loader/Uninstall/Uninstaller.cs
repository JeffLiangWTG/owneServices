using CargoWise.Loader.Common;
using Enterprise.Client.Common;

namespace Enterprise.Loader
{
	class Uninstaller : InstallationItem
	{
		public Uninstaller(Installation installation)
			: base(installation)
		{
			Dependencies.Add(new CurrentVersionFileRemover(installation));
			Dependencies.Add(new OldVersionsRemover(installation, true));
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}
	}
}
