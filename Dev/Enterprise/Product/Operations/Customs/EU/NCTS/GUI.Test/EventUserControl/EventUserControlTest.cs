using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class EventUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
		}

		public void TestPhase5EventTabUserControl()
		{
			var phase5EventTabUserControl = control.Phase5EventTabUserControl;
			CombineAssertions(() =>
			{
				AssertType<Phase5EventTabUserControl>(phase5EventTabUserControl);
				AssertEquals("BindingMember", "EnRouteIncidents", phase5EventTabUserControl.GetBindingMember());
				AssertEquals("Phase5EventTabUserControl Dock", DockStyle.Fill, phase5EventTabUserControl.Dock);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new EventUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		EventUserControl control;
	}
}
