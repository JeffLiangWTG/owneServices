using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class ReportsUcc6GridFieldsLayoutUserControlTest : TestCaseWithFactory
{
	public void TestGetLayout()
	{
		using (var form = new ZForm())
		using (var control = new ReportsGridFieldsLayoutUserControlForTest())
		{
			AssertType<ReportsUcc6GridFieldsLayout>("ReportsGridFieldsLayoutUserControl.Layout", control.GetLayout());
		}
	}

	class ReportsGridFieldsLayoutUserControlForTest : ReportsUcc6GridFieldsLayoutUserControl
	{
		public new IPanelLayoutProvider GetLayout() => base.GetLayout();
	}
}
