using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1046 : Control
	{
		public void Method()
		{
			//CW1046:Do Not Specify Tooltips Manually Rule
			ToolTipService.SetToolTip(this, "caption");
		}
	}
}
