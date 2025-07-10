using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	class ReportsGridFieldsLayoutUserControlTest : TestCaseWithFactory
	{
		public void TestGetLayout()
		{
			using (var form = new ZForm())
			using (var control = new ReportsGridFieldsLayoutUserControlForTest())
			{
				AssertType<ReportsGridFieldsLayout>("ReportsGridFieldsLayoutUserControl.Layout", control.GetLayout());
			}
		}
	}

	class ReportsGridFieldsLayoutUserControlForTest : ReportsGridFieldsLayoutUserControl
	{
		public new IPanelLayoutProvider GetLayout() => base.GetLayout();
	}
}
