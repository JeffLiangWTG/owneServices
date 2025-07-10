using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class AWBLineToSplitUserControlTest : TestCaseWithFactory
	{
		public void TestDisableSplitterTabStop()
		{
			using (var form = new ZForm())
			using (var control = new AWBLineToSplitUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var eoriBranchControl = (ZDropEdit)control.Controls.Find("BranchDropEdit", true).First();
				Assert(!eoriBranchControl.ShowDescriptionBox);
				Assert(eoriBranchControl.ShowDescriptionInDropDown);
				Assert(eoriBranchControl.ShowCodeInDropDown);
			}
		}
	}
}
