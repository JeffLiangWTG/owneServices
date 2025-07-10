//CW1118:Use Keys KeyCode And Keys Modifiers Bitmask Analyzer

using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1118
	{
		public void Method()
		{
			_ = (Keys.A & Keys.Delete) == Keys.Delete;
			_ = (Keys.Alt & Keys.Delete) == Keys.Delete;
			_ = (Keys.Right & Keys.Left) == Keys.Left;
		}
	}
}
