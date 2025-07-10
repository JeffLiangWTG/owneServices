using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class DetailsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(CusExitHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestHeaderDetailsDynamicLayoutPanel()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			userControl.SetDataBinding(exitHeader, "");
			var detailsDynamicLayoutPanel = userControl.HeaderOrganisationDetailsDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("In ExitControlTabUserControl", true, userControl.Contains(detailsDynamicLayoutPanel));
				AssertEquals("Dock", DockStyle.Fill, detailsDynamicLayoutPanel.Dock);
				AssertEquals("Padding", new Padding(0, 10, 0, 0), detailsDynamicLayoutPanel.Padding);
				DynamicLayoutPanelTest.AssertControlsOrder(detailsDynamicLayoutPanel,
					nameof(HeaderDetailsControlBag.BranchGuidFindBox),
					nameof(HeaderDetailsControlBag.BrokerCodeFindBox),
					nameof(HeaderDetailsControlBag.ExporterOrgAddressControl),
					nameof(HeaderDetailsControlBag.CarrierAddressWithContactControl));
			});
		}

		public void TestDetailsSplitContainer()
		{
			var splitContainer = userControl.DetailsSplitContainer;

			CombineAssertions(() =>
			{
				AssertEquals("Orientation", Orientation.Vertical, splitContainer.Orientation);
				AssertEquals("SplitterDistance", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(480), splitContainer.SplitterDistance);
				AssertEquals("Panel1MinSize", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(400), splitContainer.Panel1MinSize);
			});
		}

		public void TestGridInSplitContainer()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();

			using (var form = new ExitControlForm(cusExitHeader))
			{
				form.Show();

				var exitControlUserControl = form.ExitControlUserControl;
				var detailsTabPage = exitControlUserControl.DetailsTabPage;
				exitControlUserControl.ExitControlTabControl.SelectedTab = detailsTabPage;

				var detailsTabControl = detailsTabPage.FindSingle<DetailsTabUserControl>();

				var gridControl = detailsTabControl.DetailsSplitContainer.Panel2.FindSingle<HeaderExitReportStatusGridUserControl>(nameof(HeaderExitReportStatusGridUserControl));
				AssertEquals(DockStyle.Fill, gridControl.Dock);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new DetailsTabUserControl();
		}
		DetailsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
