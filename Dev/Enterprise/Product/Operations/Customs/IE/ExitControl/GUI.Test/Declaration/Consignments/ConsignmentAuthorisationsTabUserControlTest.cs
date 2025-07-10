using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	sealed class ConsignmentAuthorisationsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ICusAuthorizationUsageCollection<Business.CusAuthorizationUsage, CusExitConsignment>), userControl.BindingSource.DataSourceType);
		}

		[RequiresSTA]
		public void TestControls_AuthorisationsGrid()
		{
			var exitConsignment = Factory.New<CusExitConsignment>();
			using (var form = new ZForm(exitConsignment))
			using (var control = new ConsignmentAuthorisationsTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var authorisationsGrid = control.FindSingleOrDefault<ZGrid>("AuthorisationsGrid");

					AssertEquals("Columns", 3, authorisationsGrid.ColumnStyles.Count);
					AssertEquals("AGC_Code", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40), authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_Code).Width);
					AssertEquals("AGC_Number", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130), authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_Number).Width);
					AssertEquals("AGC_OH_Owner", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_OH_Owner).Width);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ConsignmentAuthorisationsTabUserControl();
		}
		ConsignmentAuthorisationsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
