using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	sealed class ArrivalNotificationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType() => AssertEquals("BindingSource DataSourceType should be NctsHeader.", typeof(NctsHeader), control.BindingSource.DataSourceType);

		public void TestExpectedNextCustomsProcedureDropEditType() => AssertType<ZDropEdit>("Type of ExpectedNextCustomsProcedureDropEdit should be ZDropEdit.", control.ExpectedNextCustomsProcedureDropEdit);

		public void TestExpectedNextCustomsProcedureTabIndex()
		{
			AssertEquals("TabIndex of ExpectedNextCustomsProcedureDropEdit is expected to be 23.", 23, control.ExpectedNextCustomsProcedureDropEdit.TabIndex);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ArrivalNotificationDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ArrivalNotificationDetailsUserControl control;
	}
}
