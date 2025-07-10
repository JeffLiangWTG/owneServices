using System;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class UIResourcesTest : TestCase
	{
		public void TestGdiObjectsCount()
		{
			Assert("GdiObjectsCount should return a number >0", UIResources.GdiObjectsCount > 0);
		}

		public void TestGdiObjectsCountUnsafe()
		{
			UIResources.triggerException = true;
			Assert("GdiObjectsCount should return 0", UIResources.GdiObjectsCount == 0);
		}

		public void TestUserObjectsCount()
		{
			Assert("UserObjectsCount should return a number >0", UIResources.UserObjectsCount > 0);
		}

		public void TestUserObjectsUnsafe()
		{
			UIResources.triggerException = true;
			Assert("UserObjectsCount should return 0", UIResources.UserObjectsCount == 0);
		}

		public void TestUserWindowHandlesCount()
		{
			Assert("UserWindowHandlesCount should return a number >0", UIResources.UserWindowHandlesCount > 0);
		}

		public void TestMaximumGdiObjectCount()
		{
			AssertEquals(10000, UIResources.MaximumGdiObjectCount);
		}

		public void TestMaximumUserObjectCount()
		{
			AssertEquals(10000, UIResources.MaximumUserObjectCount);
		}

		public void TestFormsOpenLimit()
		{
			AssertEquals(10, UIResources.GetFormsOpenLimit());
		}

		public void TestMaximumGdiObjectCount_WhenRegistryKeyDoesntExist()
		{
			UIResources.SetWindowsRegistryKeyName("splaty");
			AssertEquals(-1, UIResources.MaximumGdiObjectCount);
		}

		public void TestMaximumUserObjectCount_WhenRegistryKeyDoesntExist()
		{
			UIResources.SetWindowsRegistryKeyName("splaty");
			AssertEquals(-1, UIResources.MaximumUserObjectCount);
		}

		public void TestGdiObjectNearlyOverflow()
		{
			UIResources.intervalOverride = TimeSpan.FromMilliseconds(500);
			using (UIResources.MonitorUIResourcesInBackground(UIRessourcesNearlyOverflow))
			{
				UIResources.SetGdiObjectsCount(4000);
				UIResources.MonitoringGDIObjectUsage();
				Assert("Warning message not thrown to user.", !warningMessageThrown);
				AssertEquals("Not fired when under 90% of its limit.", false, UIResources.withinGdiObjectNearlyOverflowCondition);

				UIResources.SetGdiObjectsCount(9901);
				UIResources.MonitoringGDIObjectUsage();
				Assert("Warning message thrown to user.", warningMessageThrown);
				AssertEquals("Fired when all but 1000 handles have been used up", true, UIResources.withinGdiObjectNearlyOverflowCondition);
			}
		}

		public void UIRessourcesNearlyOverflow(object sender, EventArgs e)
		{
			warningMessageThrown = true;
		}

		#region Implementation

		bool warningMessageThrown;
		TestUIResources UIResources
		{
			get { return uiResources ?? (uiResources = new TestUIResources()); }
		}
		TestUIResources uiResources;

		#endregion
	}
}
