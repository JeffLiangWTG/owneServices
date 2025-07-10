// CW1102:Do Not Use System Windows Forms Tab Page Analyzer
// https://devops.wisetechglobal.com/wtg/Content/_wiki/wikis/Content.wiki/6823/CW1102-DoNotUseSystemWindowsFormsTabPageAnalyzer

using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1102 : System.IDisposable
	{
		readonly TabPage TabPageField = new TabPage();

		public void Confirm()
		{
			var localTabPage = new TabPage();
			localTabPage = TabPageField;
		}

		public void Dispose()
		{
			throw new System.NotImplementedException();
		}
	}
}
