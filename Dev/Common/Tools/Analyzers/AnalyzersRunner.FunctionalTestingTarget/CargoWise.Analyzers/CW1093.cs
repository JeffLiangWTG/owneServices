using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1093
	{
		public void Method()
		{
			//CW1093:Do Not Use System.Windows.Forms.ToolStrip Controls
			_ = new ToolStripMenuItem();
		}
	}
}
