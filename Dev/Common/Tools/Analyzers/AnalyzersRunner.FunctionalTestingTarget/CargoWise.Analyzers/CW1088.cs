using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1088
	{
		public void Method()
		{
			//CW1088:Do Not Use System.Windows.Forms.Clipboard
			Clipboard.Clear();
		}
	}
}
