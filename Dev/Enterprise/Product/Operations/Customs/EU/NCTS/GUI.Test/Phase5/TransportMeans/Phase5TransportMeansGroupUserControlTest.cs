using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5TransportMeansGroupUserControlTest : TestCaseWithFactory
	{
		public void TestTransportMeansGroupBox()
		{
			var transportMeansGroupBox = control.TransportMeansGroupBox;
			CombineAssertions(() =>
			{
				AssertType<ZGroupBox>(transportMeansGroupBox);
				AssertEquals("Caption", "Transport Means", transportMeansGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", DockStyle.Fill, transportMeansGroupBox.Dock);
			});
		}

		public void TestTransportMeansDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			control.SetDataBinding(nctsHeader, "");
			var transportMeansDynamicLayoutPanel = control.TransportMeansDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("TransportMeansDynamicLayoutPanel is within the group box", true, control.TransportMeansGroupBox.Controls.Contains(transportMeansDynamicLayoutPanel));
				AssertEquals("Dock", DockStyle.Fill, transportMeansDynamicLayoutPanel.Dock);

				DynamicLayoutPanelTest.AssertControlsOrder(transportMeansDynamicLayoutPanel,
					nameof(Phase5TransportMeansControlBag.TransportAtDepartureTypeDropEdit),
					nameof(Phase5TransportMeansControlBag.TransportAtDepartureIDTextBox),
					nameof(Phase5TransportMeansControlBag.TransportAtDepartureNationalityCodeFindBox));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new Phase5TransportMeansGroupUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		Phase5TransportMeansGroupUserControl control;
	}
}
