using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	public class GlowClientLauncherTest : TestCaseWithFactory
	{
		public void TestShowsProgressBar()
		{
			var launcher = new GlowClientLauncher(Factory.New<LicenceConnection>());
			AssertEquals(true, launcher.ShowProgressForm);
		}
	}
}