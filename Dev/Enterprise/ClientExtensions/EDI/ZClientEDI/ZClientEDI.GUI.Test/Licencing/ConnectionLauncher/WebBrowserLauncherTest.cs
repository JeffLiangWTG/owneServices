using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	public class WebBrowserLauncherTest : TestCaseWithFactory
	{
		public void TestShowsProgressBar()
		{
			var launcher = new WebBrowserLauncher(Factory.New<LicenceConnection>());
			AssertEquals(false, launcher.ShowProgressForm);
		}
	}
}