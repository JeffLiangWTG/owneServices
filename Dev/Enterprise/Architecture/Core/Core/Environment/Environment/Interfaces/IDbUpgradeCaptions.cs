namespace Enterprise.ZArchitecture.Core
{
	public interface IDbUpgradeCaptions
	{
		public string UpgradeInProgressTitle { get; }
		public string UpgradeInProgressMessage { get; }
		public string PurgeInProgressTitle { get; }
		public string PurgeInProgressMessage { get; }
		public string Exit { get; }
		void Refresh();
	}
}
