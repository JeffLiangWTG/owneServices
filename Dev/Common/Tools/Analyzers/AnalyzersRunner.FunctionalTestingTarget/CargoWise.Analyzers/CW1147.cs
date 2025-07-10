using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1147
	{
		public void BadCode()
		{
			// CW1147 Do not use WebControls (WebView etc.) in a Winforms app that is accessed over RDP. They are a big security risk. 
			_ = new WebBrowser();
			_ = new MyWebBrowser();
		}
	}

	class MyWebBrowser : WebBrowser
	{
	}
}
