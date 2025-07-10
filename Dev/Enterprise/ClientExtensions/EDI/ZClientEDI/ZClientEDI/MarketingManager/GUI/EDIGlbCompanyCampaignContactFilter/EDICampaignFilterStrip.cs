using Enterprise.MarketingManager.GUI;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public class EDICampaignFilterStrip : CampaignFilterStrip
	{
		public EDICampaignFilterStrip()
			: base()
		{
			AddCustomFilterControlsBuilder(new LicenceUsageFilterControlBuilder());
		}
	}
}
