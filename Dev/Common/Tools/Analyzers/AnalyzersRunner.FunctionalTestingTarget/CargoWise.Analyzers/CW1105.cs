//CW1105:Do Not Use System Windows Forms User Control Or KUser Control Analyzer

using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1105 : System.IDisposable
	{
		readonly UserControl userControlField = new KUserControl();

		public void Confirm()
		{
			var localUserControl = new UserControl();
			localUserControl = userControlField;
		}

		public void Dispose()
		{
			throw new System.NotImplementedException();
		}
	}
}
