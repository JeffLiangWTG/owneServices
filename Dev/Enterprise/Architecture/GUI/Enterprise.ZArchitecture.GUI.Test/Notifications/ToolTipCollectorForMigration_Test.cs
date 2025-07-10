using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class ToolTipCollectorForMigration_Test : NUnit.Framework.TestCase
	{
		public void TestGetSet()
		{
			var collector = new ToolTipCollectorForMigration();
			var control1 = new Control();
			var control2 = new Control();
			collector.SetToolTip(control1, "teapot");
			AssertEquals("Found correct tip", "teapot", collector.GetToolTip(control1));
			AssertEquals("No tip", "", collector.GetToolTip(control2));
		}
	}
}
