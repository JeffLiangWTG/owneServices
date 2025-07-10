using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class PreviousDocumentsDetailsLayoutControlTest : TestCaseWithFactory
	{
		public void TestPreviousDocumentsDetailsPanel()
		{
			CombineAssertions(() =>
			{
				using (var form = new PreviousDocumentsDetailsLayoutControl())
				{
					AssertEquals("Dock", DockStyle.None, form.DetailsPanel.Dock);
					AssertEquals("AutoSize", true, form.DetailsPanel.AutoSize);
					AssertEquals("AutoSizeMode", AutoSizeMode.GrowAndShrink, form.DetailsPanel.AutoSizeMode);
				}
			});
		}
	}
}
