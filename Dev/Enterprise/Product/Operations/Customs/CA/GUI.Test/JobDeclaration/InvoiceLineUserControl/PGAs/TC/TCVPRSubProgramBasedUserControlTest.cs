using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class TCVPRSubProgramBasedUserControlTest : TestCaseWithFactory
	{
		public void TestAutoScroll()
		{
			using (var filterControl = new TCVPRUserControl(false))
			{
				filterControl.Show();
				Assert(filterControl.AutoScroll);
				AssertEquals(400, filterControl.AutoScrollMinSize.Height);
				AssertEquals(1080, filterControl.AutoScrollMinSize.Width);
			}
		}
	}
}
