using CargoWise.BrandManager;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Environment
{
	public class DbUpgradeCaptions : IDbUpgradeCaptions
	{
		public DbUpgradeCaptions()
		{
			using (Res.TemporarilySwitchLanguage(Res.DefaultLanguage))
			{
				UpdateDbUpgradeCaptions();
			}
		}

		public string UpgradeInProgressTitle { get; private set; }
		public string UpgradeInProgressMessage { get; private set; }
		public string PurgeInProgressTitle { get; private set; }
		public string PurgeInProgressMessage { get; private set; }
		public string Exit { get; private set; }

		public void Refresh()
		{
			UpdateDbUpgradeCaptions();
		}

		void UpdateDbUpgradeCaptions()
		{
			UpgradeInProgressTitle = Res.GetString("96b96ccf-cc9c-46d6-ba48-5adeee2bc52c", "{0} Upgrade In Progress", BrandingFactory.Instance.ProductName);
			UpgradeInProgressMessage = Res.GetString("dfd25399-6e0e-4281-a68d-343567a1c036", "Please wait, the database is in the process of being upgraded. {0} will restart automatically after the upgrade is complete.", BrandingFactory.Instance.ProductName);
			PurgeInProgressTitle = Res.GetString("970FDDF7-EDFB-436B-894F-80993071F097", "{0} Purge In Progress", BrandingFactory.Instance.ProductName);
			PurgeInProgressMessage = Res.GetString("12993FA1-ADA7-4938-9E7B-3DD835B95F67", "The database is currently being purged, please wait or contact your system administrator. {0} will restart automatically after the purge is complete.", BrandingFactory.Instance.ProductName);
			Exit = Res.GetString("528e6c1f-cabf-4f9a-8d41-cc72eb69dc36", "Exit");
		}
	}
}
