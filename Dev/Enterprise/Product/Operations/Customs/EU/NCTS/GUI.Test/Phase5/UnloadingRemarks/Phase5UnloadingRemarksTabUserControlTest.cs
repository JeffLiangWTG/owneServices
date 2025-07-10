using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5UnloadingRemarksTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals("Phase5UnloadingRemarksTabUserControl BindingSource DataSourceType", typeof(NctsHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestTabBindingMember()
		{
			AssertEquals("Bills", userControl.HouseConsignmentDifferencesTabUserControl.GetBindingMember());
		}

		public void TestTabPagesOrder()
		{
			AssertSequencesEqual("TabPageNames",
			new[]
			{
				userControl.UnloadingDifferencesTabPage.Name, userControl.HouseConsignmentDifferencesTabPage.Name
			}, userControl.UnloadingRemarksTabControl.AllTabPages.Cast<ZTabPage>().Select(x => x.Name));
		}

		public void TestUnloadingDifferencesTabPage()
		{
			var unloadingDifferencesTabPage = userControl.UnloadingDifferencesTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Unloading Differences", unloadingDifferencesTabPage.CaptionResourceString.Caption);
				AssertEquals("Contains UnloadingDifferencesTabUserControl", unloadingDifferencesTabPage.Controls.Contains(userControl.DynamicUnloadingDifferencesTabUserControl), true);
			});
		}

		public void TestHouseConsignmentDifferencesTabPage()
		{
			var houseConsignmentDifferencesTabPage = userControl.HouseConsignmentDifferencesTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "House Consignment Differences", houseConsignmentDifferencesTabPage.CaptionResourceString.Caption);
				AssertEquals("Contains HouseConsignmentDifferencesTabUserControl", houseConsignmentDifferencesTabPage.Controls.Contains(userControl.HouseConsignmentDifferencesTabUserControl), true);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5UnloadingRemarksTabUserControl();
		}
		Phase5UnloadingRemarksTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
