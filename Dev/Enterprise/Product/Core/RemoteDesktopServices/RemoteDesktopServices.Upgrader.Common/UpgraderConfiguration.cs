using CargoWise.BrandManager;
using CargoWise.Loader.Common;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common
{
	public sealed class UpgraderConfiguration : Configuration
	{
		protected override void InitializeCore(string[] args)
		{
			base.InitializeCore(args);
			BrandingType = BrandingFactory.BrandingType.CargoWiseNext;
		}
	}
}
