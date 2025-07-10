using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public sealed class InstallationItemContainer : InstallationItem
	{
		public InstallationItemContainer(Installation installation, bool isRemoteExclusive = false)
			: base(installation)
		{
			Argument.NotNull(installation, nameof(installation));
			this.isRemoteExclusive = isRemoteExclusive;
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		protected override bool IsRemoteExclusive
		{
			get
			{
				return isRemoteExclusive;
			}
		}
		readonly bool isRemoteExclusive;
	}
}
