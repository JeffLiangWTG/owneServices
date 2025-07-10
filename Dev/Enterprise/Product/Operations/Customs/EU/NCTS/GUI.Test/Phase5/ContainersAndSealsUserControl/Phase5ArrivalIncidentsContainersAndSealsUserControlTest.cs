using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5ArrivalIncidentsContainersAndSealsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType() => AssertEquals(typeof(EnRouteIncident), control.BindingSource.DataSourceType);

		public void TestContainersGrid()
		{
			var containerTabPage = control.ContainerTabPage;
			var containersGrid = control.ContainersGrid;
			CombineAssertions(() =>
			{
				AssertEquals("ContainerTabPage: Caption", "Containers/Equipment", containerTabPage.CaptionResourceString.Caption);
				AssertEquals("ContainersGrid: inside ContainerTabPage", true, containerTabPage.Contains(containersGrid));
				AssertEquals("ContainersGrid: Binding", nameof(EnRouteIncident.IncidentContainers), containersGrid.BindTo);

				var modeColumnStyle = containersGrid.GetColumnStyle(nameof(NctsContainer.BC_Mode));
				AssertEquals("ContainersGrid: BC_Mode CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, modeColumnStyle.CharacterCasing);
				AssertEquals("ContainersGrid: BC_Mode column width", 100, modeColumnStyle.Width);
				AssertEquals("ContainersGrid: BC_ContainerNum column width", 175, containersGrid.GetColumnWidth(nameof(NctsContainer.BC_ContainerNum)));
				AssertEquals("ContainersGrid: Seal1 column width", 95, containersGrid.GetColumnWidth(nameof(NctsContainer.BC_Seal1)));
				AssertEquals("ContainersGrid: Seal2 column width", 95, containersGrid.GetColumnWidth(nameof(NctsContainer.BC_Seal2)));
				AssertEquals("ContainersGrid: TotalSealCount column width", 70, containersGrid.GetColumnWidth(nameof(NctsContainer.TotalSealCount)));
			});
		}

		public void TestContainersGridReadOnly()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			header.BH_ExportFlag = EventFlagList.Codes.Yes;
			header.EnRouteIncidents.AddNew();

			AssertContainersGridIsReadonly(header, false, "Should not be readOnly if it's not sent");

			header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var headerLoaded = newFactory.Load<NctsHeader>(header.PK);

			AssertContainersGridIsReadonly(headerLoaded, true, "Should be readOnly if it's sent");
		}

		void AssertContainersGridIsReadonly(NctsHeader nctsHeader, bool expectedValue, string assertionMessage)
		{
			using (var form = new Phase5ArrivalMovementForm(nctsHeader))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var incidentsTabPage = mainTabControl.FindSingle<ZTabPage>("IncidentsTabPage");
				mainTabControl.SelectTab(incidentsTabPage);
				var eventTabUserControl = incidentsTabPage.FindSingle<Phase5EventTabUserControl>("EventTabUserControl");
				var containersAndSealsUserControl = eventTabUserControl.FindSingle<Phase5ArrivalIncidentsContainersAndSealsUserControl>("ContainersAndSealsUserControl");

				AssertEquals($"[{assertionMessage}], Incident child Containers ReadOnly", expectedValue, containersAndSealsUserControl.ContainersGrid.ReadOnly);
			}
		}

		public void TestAdditionalSealsGrid()
		{
			var additionalSealsTabPage = control.AdditionalSealsTabPage;
			var additionalSealsGrid = control.AdditionalSealsGrid;
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalSealsTabPage: Caption", "Additional Seals", additionalSealsTabPage.CaptionResourceString.Caption);
				AssertEquals("AdditionalSealsGrid: inside AdditionalSealsTabPage", true, additionalSealsTabPage.Contains(additionalSealsGrid));
				AssertEquals("AdditionalSealsGrid: Binding", nameof(EnRouteIncident.IncidentContainers) + "." + nameof(NctsContainer.Seals), additionalSealsGrid.BindTo);
				AssertEquals("AdditionalSealsGrid: BK_SealNumber column width", 105, additionalSealsGrid.GetColumnWidth(nameof(CusSeal.BK_SealNumber)));
			});
		}

		public void TestItemNumbersGrid()
		{
			var itemNumbersTabPage = control.ItemNumbersTabPage;
			var itemNumbersGrid = control.ItemNumbersGrid;
			CombineAssertions(() =>
			{
				AssertEquals("ItemNumbersTabPage: Caption", "Item numbers", itemNumbersTabPage.CaptionResourceString.Caption);
				AssertEquals("ItemNumbersGrid: inside ItemNumbersTabPage", true, itemNumbersTabPage.Contains(itemNumbersGrid));
				AssertEquals("ItemNumbersGrid: Binding", nameof(EnRouteIncident.IncidentContainers) + "." + nameof(NctsContainer.ItemNumbers), itemNumbersGrid.BindTo);
				AssertEquals("ItemNumbersGrid: CY_DataNumeric column width", 105, itemNumbersGrid.GetColumnWidth(nameof(NctsContainerItem.CY_DataNumeric)));
			});
		}

		public void TestContainerTabVisibleAndSelected()
		{
			var incident = Factory.New<EnRouteIncident>();

			using (var form = new ZForm(incident))
			{
				form.Controls.Add(control);
				form.Show();

				var containerTabPage = control.ContainerTabPage;
				var sealTabControl = control.SealTabControl;
				CombineAssertions(() =>
				{
					AssertEquals("containerTabPage is visible", true, containerTabPage.TabVisible);
					AssertEquals("SealTabControl: Visible)", true, sealTabControl.Visible);
					AssertEquals("SealTabControl: Selected", containerTabPage, sealTabControl.SelectedTab);
					AssertEquals("AdditionalSealsTabControl is visible", true, control.AdditionalSealsTabControl.Visible);
					AssertEquals("AdditionalSealsTabPage is visible", true, control.AdditionalSealsTabPage.TabVisible);
				});
			}
		}

		public void TestSealTabControl()
		{
			AssertEquals("SealTabControl Dock", DockStyle.Top, control.SealTabControl.Dock);
		}

		public void TestAdditionalSealsTabControl()
		{
			AssertEquals("AdditionalSealsTabControl Dock", DockStyle.Left, control.AdditionalSealsTabControl.Dock);
		}

		public void ItemNumbersTabControl()
		{
			AssertEquals("ItemNumbersTabControl Dock", DockStyle.Fill, control.ItemNumbersTabControl.Dock);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new Phase5ArrivalIncidentsContainersAndSealsUserControl();
			var header = Factory.New<NctsHeader>();
			var incident = header.EnRouteIncidents.AddNew();
			incident.IncidentContainers.AddNew();
			control.SetDataBinding(incident, null);
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		Phase5ArrivalIncidentsContainersAndSealsUserControl control;
	}
}
