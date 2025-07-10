using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LVSLineDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestRemissionTypeDropEditAndRemissionDropEdit()
		{
			using (var control = new LVSLineDetailsUserControl())
			{
				var remissionTypeDropEdit = control.Controls.Find("RemissionTypeDropEdit", true)[0];
				AssertNotNull(remissionTypeDropEdit);
				var remissionDropEdit = control.Controls.Find("RemissionDropEdit", true)[0];
				AssertNotNull(remissionDropEdit);
			}
		}
	}
}
