using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1042 : Control
	{
		[DpiState(DpiState.Unscaled)]
		internal int field;

		public CW1042()
		{
			//CW1042:Check for invalid assignments between scaled and unscaled fields/properties
			field = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		}
	}
}
