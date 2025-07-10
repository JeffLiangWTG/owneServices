using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.EU.GUI.Testing;

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
