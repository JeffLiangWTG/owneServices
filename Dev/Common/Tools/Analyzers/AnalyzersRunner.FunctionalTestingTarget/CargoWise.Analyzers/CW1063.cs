using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1063 : Control
	{
		public void Method()
		{
			//CW1063:Do Not Use System.Windows.Forms.Screen Class
			_ = Screen.FromControl(this);
		}
	}
}
