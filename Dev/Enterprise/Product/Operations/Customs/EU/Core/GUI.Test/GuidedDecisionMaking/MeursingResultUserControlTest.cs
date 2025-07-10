using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class MeursingResultUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new MeursingResultUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull("MeursingResultDropEdit", control.FindSingle<ZDropEdit>("MeursingResultDropEdit"));
				});
			}
		}
	}
}
