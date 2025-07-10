using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ABIChildUserControlTestCase : TestCaseWithFactory
	{
		public void TestAutoScroll()
		{
			using (var filterControl = new ABIProgramUserControl(false))
			{
				filterControl.Show();
				Assert(filterControl.AutoScroll);
			}
		}
	}
}
