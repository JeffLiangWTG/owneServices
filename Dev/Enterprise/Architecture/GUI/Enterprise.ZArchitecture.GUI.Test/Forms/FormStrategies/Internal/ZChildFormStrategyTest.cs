using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.FormStrategies.Internal
{
	public class ZChildFormStrategyTest : TestCaseWithDummy
	{
		public void TestZChildFormAdornments()
		{
			using (var form = new ZChildForm(Dummy))
			{
				AssertNotNull("There should be status bar", form.MainStatusBar);
				AssertNull("There should not be menu", form.Menu);
			}
		}
	}
}
