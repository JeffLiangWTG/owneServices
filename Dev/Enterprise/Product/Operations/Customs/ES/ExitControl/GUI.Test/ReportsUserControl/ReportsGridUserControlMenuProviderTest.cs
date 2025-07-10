using Enterprise.Customs.EU.ExitControl.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.GUI.Testing
{
	sealed class ReportsGridUserControlMenuProviderTest : TestCase
	{
		public void TestAdditionalReportsGridMenuItems()
		{
			using (var gridProvider = new EU.ExitControl.GUI.Testing.ReportsGridUserControlProviderForTesting())
			{
				IReportsGridUserControlMenuProvider provider = new ReportsGridUserControlMenuProvider(gridProvider);
				var additionalReportsGridMenuItems = provider.AdditionalReportsGridMenuItems;
				CombineAssertions(() =>
				{
					AssertEquals("There are 4 menu items in the grid", 4, additionalReportsGridMenuItems.Count);
					AssertEquals("First menu item is", "&Select/Edit Report Items", additionalReportsGridMenuItems[0].Text);
					AssertEquals("Second menu item is", "Check for Inbox Notifications", additionalReportsGridMenuItems[1].Text);
					AssertEquals("Third menu item is", "View on Customs Website", additionalReportsGridMenuItems[2].Text);
					AssertEquals("Fourth menu item is", "Set Entry as Failed From Transmission", additionalReportsGridMenuItems[3].Text);
				});
			}
		}
	}
}
