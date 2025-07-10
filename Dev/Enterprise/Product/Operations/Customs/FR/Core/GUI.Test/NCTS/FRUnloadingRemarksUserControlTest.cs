using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	sealed class FRUnloadingRemarksUserControlTest : TestCaseWithFactory
	{
		public void TestUnloadingItemDifferencesTabUserControlType()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			using (var form = new ZForm(header))
			using (var control = new FRUnloadingRemarksUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tabPage = (ZTabPage)(control.Controls.Find("GoodsItemDifferencesTabPage", true)[0]);
				(tabPage.Parent as ZTabControl).SelectedTab = tabPage;
				var userControl = (ZDynamicControlCreationUserControl)control.Controls.Find("UnloadingItemDifferencesDynamicUserControl", true)[0];
				AssertEquals(typeof(FRUnloadingItemDifferencesTabUserControl), userControl.UserControlType);
			}
		}
	}
}
