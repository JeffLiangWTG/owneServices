using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class ZWebBrowser : ZArchitecture.GUI.ZWebBrowser
	{
		public ZWebBrowser()
			: base()
		{
		}

		public override void Navigate(string urlString)
		{
			WebUrlLauncher.Launch(urlString);
		}
	}
}
