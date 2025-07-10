using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class UnloadingItemDifferencesTabUserControlTest : TestCaseWithFactory
	{
		public void TestUnloadedItemNewUserControl()
		{
			using (var control = new UnloadingItemDifferencesTabUserControl())
			{
				var unloadingItemDifferencesSplitContainer = control.FindSingle<CargoWise.Windows.UI.KSplitContainer>("UnloadingItemDifferencesSplitContainer");
				AssertType<UnloadedItemNewUserControl>(unloadingItemDifferencesSplitContainer.FindSingle<EU.NCTS.GUI.UnloadedItemNewUserControl>("UnloadedItemNewUserControl"));
			}
		}

		public void TestUnloadedItemDetailsUserControl()
		{
			using (var control = new UnloadingItemDifferencesTabUserControl())
			{
				var unloadingItemDifferencesSplitContainer = control.FindSingle<CargoWise.Windows.UI.KSplitContainer>("UnloadingItemDifferencesSplitContainer");
				AssertType<UnloadedItemDetailsUserControl>(unloadingItemDifferencesSplitContainer.FindSingle<EU.NCTS.GUI.UnloadedItemDetailsUserControl>("UnloadedItemDetailsUserControl"));
			}
		}
	}
}
