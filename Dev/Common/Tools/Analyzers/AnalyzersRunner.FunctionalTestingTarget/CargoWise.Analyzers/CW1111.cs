// CW1111:Do Not Use System Windows Forms Tab Draw Mode Owner Draw Fixed Analyzer

using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1111
	{
		public void SetTabControlDrawMode()
		{
			var tabControl = new ZTabControl();
			tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
		}
	}
}
