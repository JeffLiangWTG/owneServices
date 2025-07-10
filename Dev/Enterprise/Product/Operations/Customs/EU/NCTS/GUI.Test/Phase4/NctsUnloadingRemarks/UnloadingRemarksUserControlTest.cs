using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class UnloadingRemarksUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestUnloadingHeaderDifferencesUserControlType()
		{
			using (var form = new ZForm())
			using (var control = new UnloadingRemarksUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var tabPage = (ZTabPage)(control.Controls.Find("HeaderDifferencesTabPage", true)[0]);
				(tabPage.Parent as ZTabControl).SelectedTab = tabPage;
				var userControl = (ZDynamicControlCreationUserControl)control.Controls.Find("UnloadingHeaderDifferencesDynamicUserControl", true)[0];
				AssertEquals(typeof(UnloadingHeaderDifferencesTabUserControl), userControl.UserControlType);
			}
		}
	}
}
