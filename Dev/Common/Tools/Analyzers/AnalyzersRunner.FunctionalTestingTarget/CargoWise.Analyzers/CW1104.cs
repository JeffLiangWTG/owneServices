// CW1104:Do Not Use System Windows Forms Tab Control Analyzer
// https://devops.wisetechglobal.com/wtg/Content/_wiki/wikis/Content.wiki/6824/CW1104-DoNotUseSystemWindowsFormsTabControlAnalyzer

using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1104 : System.IDisposable
	{
		readonly TabControl tabControlField = new TabControl();

		public void Confirm()
		{
			var localTabControl = new TabControl();
			localTabControl = tabControlField;
		}

		public void Dispose()
		{
			throw new System.NotImplementedException();
		}
	}
}
