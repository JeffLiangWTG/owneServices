using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1041 : Control
	{
		public CW1041()
		{
			//CW1041:Check for improper use of the scaling tools
			_ = ControlDpiScalingHelper.ScaleToCurrentDpiX(Width);
		}
	}
}
