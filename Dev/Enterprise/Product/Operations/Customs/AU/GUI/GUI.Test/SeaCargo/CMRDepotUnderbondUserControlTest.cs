using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class CMRDepotUnderbondUserControlTest : TestCaseWithFactory
	{
		public void TestCMRDepotUnderbondUserControl()
		{
			using (CMRDepotUnderbondUserControl control = new CMRDepotUnderbondUserControl())
			{
				MenuItem contingencyMenuItem = null;
				foreach (MenuItem item in control.cusUnderbondUserControl1.UnderbondsGrid.ContextMenu.MenuItems)
				{
					if (item.Text == "Create Underbond Contingency Data")
					{
						contingencyMenuItem = item;
						break;
					}
				}

				AssertNotNull("Context menu contains Contingency Data menu found", contingencyMenuItem);
				contingencyMenuItem.PerformClick();
				AssertEquals("Please select a row before attempting to create Contingency Data.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
