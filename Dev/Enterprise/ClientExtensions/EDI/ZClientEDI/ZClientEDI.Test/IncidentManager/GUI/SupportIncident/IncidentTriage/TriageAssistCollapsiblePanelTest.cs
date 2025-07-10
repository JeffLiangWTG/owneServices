using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	public class TriageAssistCollapsiblePanelTest : TestCaseWithFactory
	{
		public void TestIsHorizontal()
		{
			using (var panel = new TriageAssistCollapsiblePanelForTest())
			{
				AssertEquals(false, panel.IsHorizontal_Exposed);
				panel.Dock = System.Windows.Forms.DockStyle.Fill;
				AssertEquals(true, panel.IsHorizontal_Exposed);
			}
		}
	}

	public class TriageAssistCollapsiblePanelForTest : TriageAssistCollapsiblePanel
	{
		public bool IsHorizontal_Exposed => base.IsHorizontal;
	}
}
