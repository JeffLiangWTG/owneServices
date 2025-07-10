using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ReportsGridUserControlMenuProviderTest : TestCase
	{
		public void TestAdditionalReportsGridMenuItems()
		{
			using (var gridProvider = new ReportsGridUserControlProviderForTesting())
			{
				IReportsGridUserControlMenuProvider provider = new ReportsGridUserControlMenuProvider(gridProvider);
				var additionalReportsGridMenuItems = provider.AdditionalReportsGridMenuItems;
				CombineAssertions(() =>
				{
					AssertEquals(1, additionalReportsGridMenuItems.Count);
					AssertEquals("&Select/Edit Report Items", additionalReportsGridMenuItems[0].Text);
				});
			}
		}

		public void TestAdditionalMenuItemsForMainForm()
		{
			using (var gridProvider = new ReportsGridUserControlProviderForTesting())
			{
				IReportsGridUserControlMenuProvider provider = new ReportsGridUserControlMenuProvider(gridProvider);
				var additionalMenuItemsForMainForm = provider.AdditionalMenuItemsForMainForm;
				CombineAssertions(() =>
				{
					AssertEquals(1, additionalMenuItemsForMainForm.Count);
					AssertEquals("&Select/Edit Report Items", additionalMenuItemsForMainForm[0].Text);
				});
			}
		}
	}
}
