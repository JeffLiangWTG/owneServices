using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class EDI006
	{
		public void Method(Control control)
		{
			//EDI006:RightToLeft Rule
			control.RightToLeft = RightToLeft.Yes;
		}
	}
}
