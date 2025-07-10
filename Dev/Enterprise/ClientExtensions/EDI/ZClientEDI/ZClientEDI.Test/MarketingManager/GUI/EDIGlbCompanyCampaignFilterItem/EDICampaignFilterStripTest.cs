using System.Linq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.GUI.Testing
{
	public class EDICampaignFilterStripTest : TestCase
	{
		public void TestCustomFilterControlBuilder()
		{
			using (var filterStrip = new EDICampaignFilterStrip())
			{
				Assert(filterStrip.CustomFilterControlsBuilders.Any(x => x is LicenceUsageFilterControlBuilder));
			}
		}
	}
}
