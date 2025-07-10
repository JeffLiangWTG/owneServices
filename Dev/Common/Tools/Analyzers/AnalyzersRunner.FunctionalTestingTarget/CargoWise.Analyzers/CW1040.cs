using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1040 : Control
	{
		public CW1040()
		{
			//CW1040:Check for mixed arithmatic between scaled and unscaled components
			Width += 100;
		}
	}
}
