using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5TransportAndPackagingTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals("Phase5TransportAndPackagingTabUserControl BindingSource DataSourceType", typeof(NctsHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestModeOfTransportPanel()
		{
			AssertEquals("Must be at least this size for M.O.T group boxes", 140, userControl.ModeOfTransportPanel.MinimumSize.Height);
		}

		public void TestTransportAndPackingPanel()
		{
			AssertEquals("Must be at least this size for Job Doc Address Control", 100, userControl.TransportAndPackingPanel.MinimumSize.Height);
		}

		public void TestContainersAndSealsPanel()
		{
			AssertEquals(DockStyle.Fill, userControl.ContainersAndSealsPanel.Dock);
		}

		public void TestTransportBorder()
		{
			CombineAssertions(() =>
			{
				AssertCollectionContains("TransportBorderGroupBox on the Panel", userControl.TransportBorderGroupBox, userControl.ModeOfTransportPanel.Controls);
				AssertEquals("TransportBorderGroupBox caption", "Transport Border", userControl.TransportBorderGroupBox.CaptionResourceString.Caption);
			});
		}

		public void TestTransportDeparture()
		{
			CombineAssertions(() =>
			{
				AssertCollectionContains("TransportDepartureGroupBox on the Panel", userControl.TransportDepartureGroupBox, userControl.ModeOfTransportPanel.Controls);
				AssertEquals("TransportDepartureGroupBox caption", "Transport Departure", userControl.TransportDepartureGroupBox.CaptionResourceString.Caption);
			});
		}

		public void TestTransportDetails()
		{
			CombineAssertions(() =>
			{
				AssertCollectionContains("TransportDetailsGroupBox on the Panel", userControl.TransportDetailsGroupBox, userControl.TransportAndPackingPanel.Controls);
				AssertEquals("TransportDetailsGroupBox caption", "Transport Details", userControl.TransportDetailsGroupBox.CaptionResourceString.Caption);
			});
		}
		public void TestContainersAndSealsUserControl()
		{
			var containersAndSealsUserControl = userControl.ContainersAndSealsUserControl;
			CombineAssertions(() =>
			{
				AssertType<Phase5ContainersAndSealsUserControl>("Type", containersAndSealsUserControl);
				AssertEquals("BindingMember", ".", userControl.BindingSource.GetBindingMember(containersAndSealsUserControl));
				AssertEquals("Dock", DockStyle.Fill, containersAndSealsUserControl.Dock);
				AssertCollectionContains("ContainersAndSealsUserControl is within ContainersAndSealsGroupBox", containersAndSealsUserControl, userControl.ContainersAndSealsGroupBox.Controls);
			});
		}

		public void TestContainersAndSeals()
		{
			CombineAssertions(() =>
			{
				AssertCollectionContains("ContainersAndSealsGroupBox on the Panel", userControl.ContainersAndSealsGroupBox, userControl.ContainersAndSealsPanel.Controls);
				AssertEquals("ContainersAndSealsGroupBox caption", "Containers/Equipment and Seals", userControl.ContainersAndSealsGroupBox.CaptionResourceString.Caption);
			});
		}

		public void TestTransportDepartureDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");
			var transportDepartureDynamicLayoutPanel = userControl.TransportDepartureDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, transportDepartureDynamicLayoutPanel.Dock);
				AssertCollectionContains("Within TransportDepartureGroupBox", transportDepartureDynamicLayoutPanel, userControl.TransportDepartureGroupBox.Controls);
				DynamicLayoutPanelTest.AssertControlsOrder(transportDepartureDynamicLayoutPanel, nameof(TransportDepartureUserControl.InlandTransportModeDropEdit));
			});
		}

		public void TestTransportBorderDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");
			var transportBorderDynamicLayoutPanel = userControl.TransportBorderDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, transportBorderDynamicLayoutPanel.Dock);
				AssertCollectionContains("Within TransportBorderGroupBox", transportBorderDynamicLayoutPanel, userControl.TransportBorderGroupBox.Controls);
				DynamicLayoutPanelTest.AssertControlsOrder(transportBorderDynamicLayoutPanel, nameof(TransportBorderUserControl.BorderTransportModeDropEdit));
			});
		}

		public void TestTransportAndPackagingDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");
			var transportAndPackagingDynamicLayoutPanel = userControl.TransportAndPackagingDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("TransportAndPackagingDynamicLayoutPanel Dock", DockStyle.Fill, transportAndPackagingDynamicLayoutPanel.Dock);
				AssertCollectionContains("Within TransportDetailsGroupBox", transportAndPackagingDynamicLayoutPanel, userControl.TransportDetailsGroupBox.Controls);
				DynamicLayoutPanelTest.AssertControlsOrder(transportAndPackagingDynamicLayoutPanel, nameof(TransportAndPackagingControlBag.TransportMethodOfPaymentDropEdit), nameof(TransportAndPackagingControlBag.CarrierDocAddressControl));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5TransportAndPackagingTabUserControl();
		}
		Phase5TransportAndPackagingTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
