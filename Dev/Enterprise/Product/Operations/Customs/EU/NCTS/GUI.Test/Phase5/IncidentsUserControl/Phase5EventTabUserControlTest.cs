using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5EventTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals("Phase5EventTabUserControl BindingSource DataSourceType", typeof(EnRouteIncidentCollection), userControl.BindingSource.DataSourceType);
		}

		public void TestIncidentsSplitContainer()
		{
			var incidentsSplitContainer = userControl.IncidentsSplitContainer;
			CombineAssertions(() =>
			{
				AssertType<KSplitContainer>("Type", incidentsSplitContainer);
				AssertEquals("Splitter distance", 65, incidentsSplitContainer.SplitterDistance);
				AssertEquals("Splitter width", 1, incidentsSplitContainer.SplitterWidth);
				AssertEquals("Minimum size of panel 1", 40, incidentsSplitContainer.Panel1MinSize);
				AssertEquals("Minimum size of panel 2", 250, incidentsSplitContainer.Panel2MinSize);
			});
		}

		public void TestPhase5IncidentsGridUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var incidentsSplitContainer = userControl.IncidentsSplitContainer;
			var incidentsGridUserControl = incidentsSplitContainer.Panel1.FindSingle<Phase5IncidentsGridUserControl>("Phase5IncidentsGridUserControl");

			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, incidentsGridUserControl.Dock);
				AssertEquals("BindingMember", ".", incidentsGridUserControl.GetBindingMember());
			});
		}

		public void TestIncidentDetailsGroupBox()
		{
			var incidentDetailsGroupBox = userControl.IncidentDetailsGroupBox;
			CombineAssertions(() =>
			{
				AssertType<ZGroupBox>(incidentDetailsGroupBox);
				AssertEquals("Caption", "Incident Details", incidentDetailsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", DockStyle.Left, incidentDetailsGroupBox.Dock);
				AssertEquals("Within Panel2 of IncidentSplitContainer", true, userControl.IncidentsSplitContainer.Panel2.Controls.Contains(incidentDetailsGroupBox));
			});
		}

		public void TestIncidentDetailsDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var incidentDetailsDynamicLayoutPanel = userControl.IncidentDetailsDynamicLayoutPanel;

			CombineAssertions(() =>
			{
				AssertEquals("IncidentDetailsDynamicLayoutPanel is within the group box", true, userControl.IncidentDetailsGroupBox.Controls.Contains(incidentDetailsDynamicLayoutPanel));
				AssertEquals("IncidentDetailsDynamicLayoutPanel Dock", DockStyle.Fill, incidentDetailsDynamicLayoutPanel.Dock);

				DynamicLayoutPanelTest.AssertControlsOrder(incidentDetailsDynamicLayoutPanel,
					nameof(IncidentDetailsControlBag.IncidentCodeDropEdit),
					nameof(IncidentDetailsControlBag.LocationOfGoodsUserControl),
					nameof(IncidentDetailsControlBag.InformationTextBox),
					nameof(IncidentDetailsControlBag.EventCountryCodeDropEdit),
					nameof(IncidentDetailsControlBag.TransportMeansGroupUserControl),
					nameof(IncidentDetailsControlBag.EndorsementDateEdit),
					nameof(IncidentDetailsControlBag.EndorsementAuthorityTextBox),
					nameof(IncidentDetailsControlBag.EndorsementCountryCodeDropEdit),
					nameof(IncidentDetailsControlBag.EndorsementPlaceTextBox));
			});
		}

		public void TestContainersAndSealsGroupBox()
		{
			var containersAndSealsGroupBox = userControl.ContainersAndSealsGroupBox;
			CombineAssertions(() =>
			{
				AssertType<ZGroupBox>(containersAndSealsGroupBox);
				AssertEquals("Caption", "Containers/Equipment and Seals", containersAndSealsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", DockStyle.Fill, containersAndSealsGroupBox.Dock);
			});
		}

		public void TestContainersAndSealsUserControl()
		{
			var containersAndSealsUserControl = userControl.ContainersAndSealsUserControl;
			AssertEquals("ContainersAndSealsUserControl is within the group box", true, userControl.ContainersAndSealsGroupBox.Controls.Contains(containersAndSealsUserControl));
		}

		public void TestIncidentGroupBox()
		{
			var incidentGroupBox = userControl.IncidentGroupBox;
			CombineAssertions(() =>
			{
				AssertType<ZGroupBox>(incidentGroupBox);
				AssertEquals("Dock", DockStyle.Fill, incidentGroupBox.Dock);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5EventTabUserControl();
		}
		Phase5EventTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
