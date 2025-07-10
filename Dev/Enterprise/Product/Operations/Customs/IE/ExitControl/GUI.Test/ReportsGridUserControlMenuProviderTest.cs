using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.EU.ExitControl.GUI.Testing;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	sealed class ReportsGridUserControlMenuProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalReportsGridMenuItems()
		{
			using (var gridProvider = new ReportsGridUserControlProviderForTesting())
			{
				IReportsGridUserControlMenuProvider provider = new ReportsGridUserControlMenuProvider(gridProvider);
				var additionalReportsGridMenuItems = provider.AdditionalReportsGridMenuItems;
				CombineAssertions(() =>
				{
					AssertEquals(2, additionalReportsGridMenuItems.Count);
					AssertEquals("&Select/Edit Report Items", additionalReportsGridMenuItems[0].Text);
					AssertEquals("Upload Supporting Documents", additionalReportsGridMenuItems[1].Text);
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
					AssertEquals(2, additionalMenuItemsForMainForm.Count);
					AssertEquals("&Select/Edit Report Items", additionalMenuItemsForMainForm[0].Text);
					AssertEquals("Upload Supporting Documents", additionalMenuItemsForMainForm[1].Text);
				});
			}
		}
	}
}
